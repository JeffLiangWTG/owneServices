using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using SupplyChainActorRoleList = Enterprise.Customs.Business.SupplyChainActorRoleList;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSSendMessageWrapper : SummaryDeclarationsCommonSendMessageWrapper, IEXSMessageDataProvider
	{
		public EXSSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, string messageSubType = null) : base(cusEntryHeader, certificateData)
		{
			this.messageSubType = messageSubType;
			Argument.GreaterThan(cusEntryHeader.MergedLines.Count, 0, nameof(cusEntryHeader.MergedLines));
			mopValueForHeader = MopValueForHeader();
			showAddInfoInGoodsLevel = ShowAddInfoInGoodsLevel();
		}
		readonly ZString messageSubType;
		readonly ZString mopValueForHeader;
		readonly ZBool showAddInfoInGoodsLevel;

		public IEXSHeader Header => header ?? (header = new EXSHeaderWrapper(entryHeader, mopValueForHeader, messageSubType));
		EXSHeaderWrapper header;

		public IDocumentsCommon TransportDocument
		{
			get
			{
				if (transportDocument == null)
				{
					var additionalInf = entryHeader.EntryInstruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(m => m.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments))
										?? entryHeader.InvoiceHeaders.Select(header => header.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(m => m.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments))).FirstOrDefault()
										?? entryHeader.InvoiceLines.Select(line => ((JobComInvoiceLine)line).AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(m => m.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments))).FirstOrDefault()
										?? entryHeader.Declaration.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(m => m.CSI_SubType.Equals(AdditionalDocList.Codes.TransportDocuments));

					if (additionalInf != null)
					{
						transportDocument = new DocumentCommonWrapper(additionalInf.CSI_Code, additionalInf.CSI_ReferenceNumber);
					}
				}
				return transportDocument;
			}
		}
		IDocumentsCommon transportDocument;

		public IPartyProvider Consignor => CachedValueHelper.GetValue(ref consignor, () =>
		{
			var (consignor, consignorAddress) = ShouldDeclareSupplierInLine() ? (null, null) : consignorAtHeaderLevel();
			return ShouldDeclareSupplierInLine() ? null : EXSPartyProviderWrapper.New(consignor, consignorAddress);
		});
		CachedValue<IPartyProvider> consignor;

		(OrgHeader header, OrgAddress address) consignorAtHeaderLevel()
		{
			OrgHeader consignor;
			consignor = declaration.SupplierDocumentaryAddress.Organisation;
			OrgAddress consignorAddress;
			consignorAddress = declaration.SupplierDocumentaryAddress.Address;

			if (!ShouldDeclareSupplierInLine())
			{
				(consignor, consignorAddress) = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => (((JobComInvoiceHeader)y.InvoiceHeader).Supplier, ((JobComInvoiceHeader)y.InvoiceHeader).SupplierAddress)))
				.FirstOrDefault();
			}
			return consignor != null ? (consignor, consignorAddress) : (declaration.SupplierDocumentaryAddress.Organisation, declaration.SupplierDocumentaryAddress.Address);
		}

		public IPartyProvider Consignee => CachedValueHelper.GetValue(ref consignee, () =>
		{
			var (consignee, consigneeAddress) = ShouldDeclareSupplierInLine() ? (null, null) : consigneeAtHeaderLevel();
			return ShouldDeclareImporterInLine() ? null : EXSPartyProviderWrapper.New(consignee, consigneeAddress);
		});
		CachedValue<IPartyProvider> consignee;

		(OrgHeader header, OrgAddress address) consigneeAtHeaderLevel()
		{
			OrgHeader consignee;
			consignee = declaration.ImporterDocumentaryAddress.Organisation;
			OrgAddress consigneeAddress;
			consigneeAddress = declaration.ImporterDocumentaryAddress.Address;

			if (!ShouldDeclareImporterInLine())
			{
				(consignee, consigneeAddress) = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => (((JobComInvoiceHeader)y.InvoiceHeader).Buyer, ((JobComInvoiceHeader)y.InvoiceHeader).BuyerAddress)))
				.FirstOrDefault();
			}
			return consignee != null ? (consignee, consigneeAddress) : (declaration.ImporterDocumentaryAddress.Organisation, declaration.ImporterDocumentaryAddress.Address);
		}

		public IReadOnlyCollection<IEXSAdditionalActor> AdditionalActors
		{
			get
			{
				if (additionalActors == null)
				{
					var additionalActorsList = new List<EXSAdditionalActorWrapper>();
					if (!ShowActorInLine())
					{
						var actorCodesList = new List<string>() { SupplyChainActorRoleList.Codes.CS, SupplyChainActorRoleList.Codes.FW, SupplyChainActorRoleList.Codes.MF, SupplyChainActorRoleList.Codes.WH };
						foreach (Customs.Business.CusReference actor in entryHeader.EntryInstruction.CusSupplyChainActorReferences)
						{
							if (actorCodesList.Contains(actor.CFR_Code))
							{
								additionalActorsList.Add(new EXSAdditionalActorWrapper(actor));
							}
						}
					}
					additionalActors = additionalActorsList.AsReadOnly();
				}
				return additionalActors;
			}
		}
		IReadOnlyCollection<EXSAdditionalActorWrapper> additionalActors;

		public IReadOnlyCollection<IDocumentsCommon> AdditionalInfo
		{
			get
			{
				if (additionalInfo == null)
				{
					var additionalInfoList = new List<DocumentCommonWrapper>();
					if (!showAddInfoInGoodsLevel)
					{
						additionalInfoList.AddRange(entryHeader.EntryInstruction.AdditionalInfos.Where(m => ((AdditionalInfo)m).CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).
									Select(info => new DocumentCommonWrapper(((AdditionalInfo)info).CSI_Code, ((AdditionalInfo)info).CSI_ReferenceNumber)));

						additionalInfoList.AddRange(declaration.AdditionalInfos.Where(m => ((AdditionalInfo)m).CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).
									Select(info => new DocumentCommonWrapper(((AdditionalInfo)info).CSI_Code, ((AdditionalInfo)info).CSI_ReferenceNumber)));
					}
					additionalInfo = additionalInfoList.AsReadOnly();
				}
				return additionalInfo;
			}
		}
		IReadOnlyCollection<DocumentCommonWrapper> additionalInfo;

		public IReadOnlyCollection<IEXSLine> Lines
		{
			get
			{
				if (lines == null)
				{
					var linesList = new List<EXSLineWrapper>();
					var shouldDeclareMopValueinLine = mopValueForHeader.IsEmpty;
					linesList.AddRange(entryHeader.MergedLines.Cast<CusEntryLine>().Select(entryLine => new EXSLineWrapper(entryLine, shouldDeclareMopValueinLine, ShouldDeclareImporterInLine(), ShouldDeclareSupplierInLine(), showAddInfoInGoodsLevel, ShowActorInLine())));

					lines = linesList.AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<EXSLineWrapper> lines;

		public ZString CustomsOffice => declaration.JE_CustomsOffice;

		public IEXSPartyProvider LodgingPerson => CachedValueHelper.GetValue(ref lodgingPerson, () => EXSPartyProviderWrapper.New(GetLodgingPersonOrgAddress(), declaration.DeclEmailAddr));
		CachedValue<IEXSPartyProvider> lodgingPerson;

		public IEXSRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => EXSRepresentativeWrapper.New(declaration, declaration.DeclarantAddress));
		CachedValue<IEXSRepresentative> representative;

		public IEXSContactPersonWithId Carrier => CachedValueHelper.GetValue(ref carrier, () => EXSContactPersonWithIdWrapper.New(declaration.ShippingLine));
		CachedValue<IEXSContactPersonWithId> carrier;

		public IReadOnlyCollection<ZString> ItineraryCountries
		{
			get
			{
				if (itineraryCountries == null)
				{
					var itineraryCountriesList = entryHeader.EntryInstruction.IncludeRoutingSecurityData
						? entryHeader.CountriesOfRouting.Where(x => !x.IsEmpty).ToList()
						: new List<ZString>();

					itineraryCountries = itineraryCountriesList.AsReadOnly();
				}
				return itineraryCountries;
			}
		}
		IReadOnlyCollection<ZString> itineraryCountries;

		public IReadOnlyCollection<ZString> Seals
		{
			get
			{
				if (seals == null)
				{
					seals = entryHeader.SealCodes.ToList().AsReadOnly();
				}
				return seals;
			}
		}
		IReadOnlyCollection<ZString> seals;

		ZString MopValueForHeader()
		{
			var transportChargesMOPs = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceHeader)y.InvoiceHeader).ZG_TransportChargesMethodOfPayment))
				.Where(z => !z.IsEmpty).Distinct().ToList();

			return transportChargesMOPs.Count == 1 ? transportChargesMOPs.First() : ZString.Empty;
		}

		ZBool ShouldDeclareImporterInLine()
		{
			var importersInAllGoods = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceHeader)y.InvoiceHeader).JZ_OH_Buyer))
				.Distinct().ToList();
			var importersInGoodsCount = importersInAllGoods.Count;

			return !((importersInGoodsCount < 2) || (importersInGoodsCount == 2 && importersInAllGoods.Contains(ZGuid.Empty) && importersInAllGoods.Contains(declaration.JE_OH_Importer)));
		}

		ZBool ShouldDeclareSupplierInLine()
		{
			var suppliersInAllGoods = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceHeader)y.InvoiceHeader).JZ_OH_Supplier))
				.Distinct().ToList();
			var suppliersInGoodsCount = suppliersInAllGoods.Count;

			return !((suppliersInGoodsCount < 2) || (suppliersInGoodsCount == 2 && suppliersInAllGoods.Contains(ZGuid.Empty) && suppliersInAllGoods.Contains(declaration.JE_OH_Supplier)));
		}

		bool ShowActorInLine()
		{
			var exist = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceLine)y).CusSupplyChainActorReferences))
				.Cast<IEnumerable<EU.Business.Declaration.CusSupplyChainActorReference>>()
				.Where(m => !m.IsNullOrEmpty())
				.ToList();

			return exist.Any();
		}

		bool ShowAddInfoInGoodsLevel()
		{
			var exist = entryHeader.MergedLines
				.SelectMany(x => x.InvoiceLines
				.Select(y => ((JobComInvoiceLine)y).AdditionalInfos.Where(j => ((AdditionalInfo)j).CSI_SubType == AdditionalDocList.Codes.AdditionalInformation)))
				.Where(m => !m.IsNullOrEmpty())
				.ToList();

			return exist.Any();
		}

		OrgAddress GetLodgingPersonOrgAddress()
		{
			OrgAddress result = null;
			if (declaration.JE_DeclarantType == ESRepresentationTypeList.Codes._1Auto || declaration.JE_DeclarantType == ESRepresentationTypeList.Codes._2Direct)
			{
				result = declaration.ShippingLine?.MainAddress;
			}
			else if (declaration.JE_DeclarantType == ESRepresentationTypeList.Codes._3Indirect)
			{
				result = declaration.DeclarantAddress;
			}

			return result;
		}
	}
}
