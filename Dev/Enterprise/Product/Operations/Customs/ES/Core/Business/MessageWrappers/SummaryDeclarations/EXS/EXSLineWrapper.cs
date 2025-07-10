using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using SupplyChainActorRoleList = Enterprise.Customs.Business.SupplyChainActorRoleList;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSLineWrapper : IEXSLine
	{
		public EXSLineWrapper(CusEntryLine cusEntryLine, bool shouldDeclareMopValueinLine = true, bool shouldDeclareImporterInLine = true, bool shouldDeclareSupplierInLine = true, bool showINFAddInfoInEntryLines = true, bool showActorInLine = true)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
			this.shouldDeclareMopValueinLine = shouldDeclareMopValueinLine;
			randomLine = entryLine.RandomLine;
			this.shouldDeclareImporterInLine = shouldDeclareImporterInLine;
			this.shouldDeclareSupplierInLine = shouldDeclareSupplierInLine;
			this.showINFAddInfoInEntryLines = showINFAddInfoInEntryLines;
			this.showActorInLine = showActorInLine;
		}

		protected readonly JobComInvoiceLine randomLine;
		protected readonly CusEntryLine entryLine;
		readonly ZBool shouldDeclareMopValueinLine;
		readonly ZBool shouldDeclareImporterInLine;
		readonly ZBool shouldDeclareSupplierInLine;
		readonly ZBool showINFAddInfoInEntryLines;
		readonly ZBool showActorInLine;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public ZString GoodsDescription => entryLine.RandomLine.JI_Description.Substring(0, 512);

		public ZDecimal GrossWeight
		{
			get
			{
				var grossWeight = entryLine.EffectiveGrossWeight.InKilograms;
				return grossWeight.Round(6);
			}
		}

		public ZString MethodOfPayment
		{
			get
			{
				var mop = ZString.Empty;
				if (shouldDeclareMopValueinLine)
				{
					mop = entryLine.RandomLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment;
				}
				return mop;
			}
		}

		public ZString UNDangerousCode => randomLine.UNDGs.FirstItemForBinding[0].SubstanceCode;

		public ZString ReferenceNumber => randomLine.ZG_CommercialReference;

		public IReadOnlyCollection<IDocumentsCommon> Certificates
		{
			get
			{
				if (certificates == null)
				{
					var certificatesList = new List<EXSDocumentWrapper>();

					certificatesList.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>().Select(doc => new EXSDocumentWrapper(doc)));
					certificatesList.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>().Select(doc => new EXSDocumentWrapper(doc)));
					certificates = certificatesList.AsReadOnly();
				}
				return certificates;
			}
		}
		IReadOnlyCollection<EXSDocumentWrapper> certificates;

		public IEXSDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					var previousDocuments = randomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
					if (previousDocuments != null)
					{
						var result = previousDocuments.Cast<PreviousDocument>().FirstOrDefault();
						if (result != null)
						{
							previousDocument = new EXSDocumentWrapper(result);
						}
					}
				}
				return previousDocument;
			}
		}
		IEXSDocument previousDocument;

		public IPartyProvider Consignor
		{
			get
			{
				var invoiceHeader = randomLine?.InvoiceHeader;
				var consignor = invoiceHeader?.SupplierAddress
								?? invoiceHeader?.Supplier?.Addresses?.MainAddress
								?? randomLine?.Declaration?.SupplierDocumentaryAddress?.Address;
				return !shouldDeclareSupplierInLine || consignor == null ? null : PartyWrapper.New(consignor);
			}
		}

		public ZString CommodityCode => entryLine.Tariff.SubstringSafe(0, 8);

		public IPartyProvider Consignee
		{
			get
			{
				var invoiceHeader = randomLine?.InvoiceHeader;
				var consignee = (invoiceHeader?.BuyerAddress
								?? invoiceHeader?.Buyer?.Addresses?.MainAddress)
								?? (randomLine?.Declaration?.ImporterDocumentaryAddress?.Address);
				return !shouldDeclareImporterInLine || consignee == null ? null : PartyWrapper.New(consignee);
			}
		}

		public IReadOnlyCollection<ZString> Containers => containers ?? (containers = entryLine.Containers.ToList().AsReadOnly());
		IReadOnlyCollection<ZString> containers;

		public IReadOnlyCollection<IEXSPackage> Packages => packages ?? (packages = EXSPackageWrapper.GetPackagesList(entryLine));
		IReadOnlyCollection<IEXSPackage> packages;

		public ZString CusCode => randomLine.ZG_CusNumber;

		public IReadOnlyCollection<IEXSAdditionalActor> AdditionalActors
		{
			get
			{
				if (additionalActors == null)
				{
					var additionalActorsList = new List<EXSAdditionalActorWrapper>();
					if (showActorInLine)
					{
						var actorCodesList = new List<string>() { SupplyChainActorRoleList.Codes.CS, SupplyChainActorRoleList.Codes.FW, SupplyChainActorRoleList.Codes.MF, SupplyChainActorRoleList.Codes.WH };
						foreach (Customs.Business.CusReference actor in randomLine.CusSupplyChainActorReferences)
						{
							var actorCode = actor.CFR_Code;
							if (actorCodesList.Contains(actorCode) && !additionalActorsList.Any(a => a.Role == actorCode))
							{
								additionalActorsList.Add(new EXSAdditionalActorWrapper(actor));
							}
						}
						if (randomLine.EntryInstruction != null)
						{
							foreach (Customs.Business.CusReference actor in randomLine.EntryInstruction.CusSupplyChainActorReferences)
							{
								var actorCode = actor.CFR_Code;
								if (actorCodesList.Contains(actorCode) && !additionalActorsList.Any(a => a.Role == actorCode))
								{
									additionalActorsList.Add(new EXSAdditionalActorWrapper(actor));
								}
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
					if (showINFAddInfoInEntryLines)
					{
						entryLine.InvoiceLines.ForEach(line => additionalInfoList.AddRange(((JobComInvoiceLine)line).AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>().
														Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).
														Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))));

						entryLine.Header.InvoiceHeaders.ForEach(invoice => additionalInfoList.AddRange(invoice.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>().
														Where(doc => doc.CSI_SubType.Equals(AdditionalDocList.Codes.AdditionalInformation)).
														Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))));
					}
					additionalInfo = additionalInfoList.AsReadOnly();
				}
				return additionalInfo;
			}
		}
		IReadOnlyCollection<DocumentCommonWrapper> additionalInfo;
	}
}
