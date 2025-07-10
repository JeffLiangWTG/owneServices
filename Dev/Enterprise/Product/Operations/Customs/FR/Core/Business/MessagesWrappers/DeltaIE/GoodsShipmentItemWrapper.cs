#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageContracts;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;
using Enterprise.ZArchitecture.Schema;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;
using JobComInvoiceLine = Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentItemWrapper : IGoodsShipmentItem
	{
		GoodsShipmentItemWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		readonly CusEntryLine entryLine;

		public static GoodsShipmentItemWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new GoodsShipmentItemWrapper(entryLine);

		public ICollection<IAdditionalFiscalReference> AdditionalFiscalReference => additionalFiscalReference ?? (additionalFiscalReference = GetAdditionalFiscalReference());
		ICollection<IAdditionalFiscalReference> additionalFiscalReference;

		ICollection<IAdditionalFiscalReference> GetAdditionalFiscalReference()
		{
			var result = new Collection<IAdditionalFiscalReference>();

			entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
				.SelectMany(invoiceLine => invoiceLine.FiscalReferences).Cast<CusFiscalReference>()
				.DistinctBy(f => f.CFR_Code + f.CFR_Reference)
				.ToList()
				.ForEach(fiscalReference => result.Add(AdditionalFiscalReferenceWrapper.New(fiscalReference)));

			return result;
		}

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;

		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var declaration = entryLine.Declaration;

			var lineAdditionalInfos = entryLine.AdditionalInfos.Where(a => a.IsAnAdditionalInformation);
			var headerAI2Infos = Enumerable.Empty<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();

			if (declaration.ZG_VATDeferType == VATProcedureList.Codes._2)
			{
				headerAI2Infos = entryLine.Header.AdditionalInfos.
					Where(ai =>
					ai.IsAnAdditionalInformation &&
					(ai.CSI_Code == RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption || ai.CSI_Code == RefCusCodeList.AdditionalInformationCodes.AI2WithoutVisaExemption));
			}

			var result = lineAdditionalInfos.Concat(headerAI2Infos)
				.DistinctBy(a => a.CSI_Code)
				.Select(ai => (IAdditionalInformation)AdditionalInformationWrapper.New(ai, declaration.JE_CustomsOffice))
				.ToCollection();

			return result;
		}

		public ICollection<IAdditionalReference> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
		ICollection<IAdditionalReference> additionalReference;

		ICollection<IAdditionalReference> GetAdditionalReference()
		{
			var result = new Collection<IAdditionalReference>();
			entryLine.AdditionalInfos.Where(a => a.IsAnAdditionalReference)
				.DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(additionalInfo => result.Add(AdditionalReferenceWrapper.New(additionalInfo, entryLine.Declaration?.JE_CustomsOffice ?? string.Empty)));

			return result;
		}

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActor());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();

			entryLine?.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().Where(cusReference => cusReference.CFR_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix && cusReference.CFR_Type == "SCA").ToList().ForEach(cusReference => result.Add(AdditionalSupplyChainActorWrapper.New(cusReference)));

			return result.Any() ? result : null;
		}

		public ICollection<IAuthorisation> Authorisation => authorisation ?? (authorisation = GetAuthorisation());
		ICollection<IAuthorisation> authorisation;

		ICollection<IAuthorisation> GetAuthorisation()
		{
			var result = new Collection<IAuthorisation>();
			if (entryLine.RandomLine is JobComInvoiceLine line)
			{
				line.CusAuthorizationUsages.Cast<CusAuthorizationUsage>()
				.DistinctBy(x => x.AGC_Code + x.EffectiveReferenceNumber + x.AGC_OH_Owner)
				.ToList()
				.ForEach(authorizationUsage => result.Add(AuthorisationWrapper.New(authorizationUsage)));
			}

			return result;
		}

		public IBuyer Buyer => buyer ?? (buyer = BuyerWrapper.New(entryLine.RandomLine?.BuyerDocAddress.Address));
		IBuyer buyer;

		public ICommodity Commodity => commodity ?? (commodity = CommodityWrapper.New(entryLine));
		ICommodity commodity;

		public ICountryOfDispatch CountryOfDispatch => countryOfDispatch ?? (countryOfDispatch = entryLine.RandomLine != null ? CountryOfDispatchWrapper.New(entryLine.RandomLine.ZG_CountryOfDispatch) : null);
		ICountryOfDispatch countryOfDispatch;

		public ICustomsValuation CustomsValuation => customsValuation ?? (customsValuation = CustomsValuationWrapper.New(entryLine));
		ICustomsValuation customsValuation;

		public string DateOfAcceptance => dateOfAcceptance ?? (dateOfAcceptance = entryLine.Header?.EntryInstruction.CEI_DateForDuty.ToString("yyyy-MM-ddTHH:mm:ss"));
		string dateOfAcceptance;

		public string DeclarationGoodsItemNumber => declarationGoodsItemNumber ?? (declarationGoodsItemNumber = entryLine.CL_LineNumber.ToString());
		string declarationGoodsItemNumber;

		public IDestination Destination => destination ?? (destination = entryLine.RandomLine != null ? GoodsShipmentItemDestinationWrapper.New(entryLine.RandomLine) : null);
		IDestination destination;

		public IExporter Exporter => exporter ?? (exporter = ExporterWrapper.New(entryLine.RandomLine?.ExporterAddress));
		IExporter exporter;

		public IConsignee Consignee => consignee ?? (consignee = ConsigneeWrapper.New(entryLine.RandomLine?.ConsigneeAddress));
		IConsignee consignee;

		public string NatureOfTransaction => natureOfTransaction ?? (natureOfTransaction = GetNatureOfTransaction());
		string natureOfTransaction;

		string GetNatureOfTransaction() => entryLine.RandomLine?.ZG_TransNature;

		public IOrigin Origin => origin ?? (origin = OriginWrapper.New(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault()));
		IOrigin origin;

		public ICollection<IPackaging> Packaging => packaging ?? (packaging = GetPackaging());
		ICollection<IPackaging> packaging;

		ICollection<IPackaging> GetPackaging()
		{
			var result = new Collection<IPackaging>();

			var linkPackagesGrouping = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesForInvoiceLinesForBindingOnly).Cast<InvoiceLineCusLinkPackage>().Where(x => x.IsLinked).GroupBy(x => x.PackagePk);
			foreach (var linkPackages in linkPackagesGrouping)
			{
				var sum = linkPackages.Sum(x => x.PackQty);
				result.Add(PackagingWrapper.New(linkPackages.FirstOrDefault(), sum));
			}

			return result;
		}

		public ICollection<IGoodsShipmentItemPreviousDocument> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocument());
		ICollection<IGoodsShipmentItemPreviousDocument> previousDocument;

		ICollection<IGoodsShipmentItemPreviousDocument> GetPreviousDocument()
		{
			var result = new Collection<IGoodsShipmentItemPreviousDocument>();
			entryLine.PreviousDocuments.Cast<PreviousDocument>()
				.DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(previousDocument => result.Add(GoodsShipmentItemPreviousDocumentWrapper.New(previousDocument)));

			return result;
		}

		public IProcedure Procedure => procedure ?? (procedure = entryLine.RandomLine != null ? DeltaIEProcedureWrapper.New(entryLine.RandomLine) : null);
		IProcedure procedure;

		public string ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = entryLine.RandomLine?.InvoiceHeader?.JZ_UCR ?? string.Empty);
		string referenceNumberUCR;

		public ISeller Seller => seller ?? (seller = SellerWrapper.New(entryLine.RandomLine?.SellerDocAddress?.Address));
		ISeller seller;

		public string SequenceNumber => sequenceNumber ?? (sequenceNumber = entryLine.CL_LineNumber.ToString());
		string sequenceNumber;

		public double StatisticalValue => statisticalValue == 0d ? statisticalValue = (double)entryLine.CL_StatisticalValue : 0d;
		double statisticalValue;

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
		ICollection<ISupportingDocument> supportingDocument;

		ICollection<ISupportingDocument> GetSupportingDocument()
		{
			var result = new Collection<ISupportingDocument>();

			entryLine.SupportingDocuments.Cast<SupportingDocument>()
				.DistinctBy(x => x.CSI_Code + x.CSI_DateOfExpiry + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(supportingDocument => result.Add(SupportingDocumentWrapper.New(supportingDocument, entryLine.Declaration?.JE_CustomsOffice ?? string.Empty)));

			return result;
		}

		public ICollection<ITransportDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportingDocument());
		ICollection<ITransportDocument> transportDocument;

		ICollection<ITransportDocument> GetTransportingDocument()
		{
			var result = new Collection<ITransportDocument>();

			entryLine.AdditionalInfos.Where(a => a.IsATransportDocument)
				.DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(additionalInfo => result.Add(TransportDocumentWrapper.New(additionalInfo)));

			return result;
		}

		public IValuationAdjustment ValuationAdjustment => valuationAdjustment ?? (valuationAdjustment = entryLine.RandomLine != null ? ValuationAdjustmentWrapper.New(entryLine.RandomLine) : null);
		IValuationAdjustment valuationAdjustment;

		public string AdditionalDeclarationType => additionalDeclarationType ?? (additionalDeclarationType = entryLine.Header.EntryInstruction?.CEI_SubStyle ?? string.Empty);

		public string DescriptionOfGoods => descriptionOfGoods ?? (descriptionOfGoods = entryLine.CL_Description);
		string descriptionOfGoods;

		public string PrimaryPreference => primaryPreference ?? (primaryPreference = entryLine.RandomLine?.JI_PrimaryPreference);
		string primaryPreference;

		string additionalDeclarationType;
	}
}
