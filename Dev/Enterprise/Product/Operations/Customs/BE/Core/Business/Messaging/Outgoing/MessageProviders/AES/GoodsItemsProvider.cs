using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.Business;

public class GoodsItemsProvider : IGoodsItem
{
	readonly Declaration.JobComInvoiceLine invoiceLine;

	public GoodsItemsProvider(Declaration.JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformation ?? (additionalInformation =
		invoiceLine.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>()
			.Where(x => x.CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalInformation)
			.Select((x, index) => new AdditionalInformationProvider(x, index + 1)).ToArray<IAdditionalInformation>());
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
		invoiceLine.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>()
			.Where(x => x.CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalReference)
			.Select((x, i) => new AdditionalReferenceProvider(x, i + 1)).ToArray<IDocument>());
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors =
		invoiceLine.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()
			.Select((x, i) => new AdditionalSupplyChainActorProvider(x, i + 1)).ToArray<IAdditionalSupplyChainActor>());
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IReadOnlyCollection<IAuthorization> Authorisations => authorizations ?? (authorizations =
		invoiceLine.CusAuthorizationUsages
			.Select((x, i) => new AuthorizationProvider(x, i + 1)).ToArray<IAuthorization>());
	IReadOnlyCollection<IAuthorization> authorizations;

	public ICommodity Commodity => commodity ?? (commodity = new CommodityProvider(invoiceLine));
	ICommodity commodity;

	public IParty Consignee => consignee ?? (consignee = invoiceLine.ConsigneeAddress == null ? null : new PartyProvider(invoiceLine.ConsigneeAddress));
	IParty consignee;

	public IParty Consignor => consignor ?? (consignor = invoiceLine.ExporterAddress == null ? null : new PartyProvider(invoiceLine.ExporterAddress));
	IParty consignor;

	public string CountryOfDestination => invoiceLine.ZG_CountryOfDestination;

	public string CountryOfExport => invoiceLine.JI_RN_NKCountryOfExport;

	public string DeclarationGoodsItemNumber => invoiceLine.CusEntryLine.CL_LineNumber.ToString();

	public string NatureOfTransaction => invoiceLine.InvoiceHeader.JobDeclaration.IsTransitionPeriodAES30 ? string.Empty : invoiceLine.InvoiceHeader.JZ_ValuationCode;

	public IOrigin Origin => origin ?? (origin = new OriginProvider(invoiceLine));
	IOrigin origin;

	public IReadOnlyCollection<IPackaging> Packagings => packaging ?? (packaging =
		invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>()
			.Select((x, i) => new PackagingProvider(x, i + 1)).ToArray<IPackaging>());
	IReadOnlyCollection<IPackaging> packaging;

	public IReadOnlyCollection<IPreviousDocumentExtended> PreviousDocuments => previousDocuments ?? (previousDocuments =
		invoiceLine.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>()
		.Select((csi, index) => new PreviousDocumentExtendedProvider(csi, index + 1)).ToArray<IPreviousDocumentExtended>());
	IReadOnlyCollection<IPreviousDocumentExtended> previousDocuments;

	public IProcedure Procedure => procedure ?? (procedure = new ProcedureProvider(invoiceLine));
	IProcedure procedure;

	public string ReferenceNumberUCR => invoiceLine.InvoiceHeader.JZ_UCR;

	public decimal? StatisticalValue => invoiceLine.JI_Calc_StatisticalValue;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
		invoiceLine.SupportingDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>()
			.Select((x, index) => new SupportingDocumentsProvider(x, index + 1)).ToArray<ISupportingDocument>());
	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public ITransportCharges TransportCharges => transportCharges ?? (transportCharges = new TransportChargesProvider(invoiceLine.InvoiceHeader));
	ITransportCharges transportCharges;

	public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments = GetTransportDocuments());
	IReadOnlyCollection<ITransportDocument> transportDocuments;

	public DateTime? AcceptanceDate => null;

	public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences => additionalFiscalReferences ?? (additionalFiscalReferences =
		invoiceLine.FiscalReferences.Cast<CusFiscalReference>()
			.Select((x, index) => new AdditionalFiscalReferencesProvider(x, index + 1)).ToArray<IAdditionalFiscalReference>());
	IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

	public IParty Buyer => buyer ?? (buyer = new PartyProvider(invoiceLine.BuyerDocAddress));
	IParty buyer;

	public string CountryOfDispatch => invoiceLine.ZG_CountryOfDispatch;

	public ICustomsValuation CustomsValuation => customsValuation ?? (customsValuation = new CustomsValuationProvider(invoiceLine));
	ICustomsValuation customsValuation;

	public IDestination Destination => destination ?? (destination = new DestinationProvider(invoiceLine.ZG_CountryOfDestination, invoiceLine.ZG_RegionOfDestination));
	IDestination destination;

	public IParty Exporter => exporter ?? (exporter = invoiceLine.ExporterAddress == null ? null : new PartyProvider(invoiceLine.ExporterAddress));
	IParty exporter;

	public IAmountCurrency IntristicValue => null;

	public IAmountCurrency PostalValue => null;

	public IParty Seller => seller ?? new PartyProvider(invoiceLine.SellerDocAddress);
	readonly IParty seller;

	public string SequenceNumber => invoiceLine.CusEntryLine.CL_LineNumber.ToString();

	public IAmountCurrency TransportAndInsuranceCostsToTheDestination => null;

	public string ValuationAdjustment => valuationAdjustment ?? (valuationAdjustment = ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceLine.InvoiceHeader.RelatedIndicator, invoiceLine.JI_RelatedIndicator)
																					+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceLine.InvoiceHeader.RelatedIndicator2, invoiceLine.ZG_RelatedIndicator2)
																					+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceLine.InvoiceHeader.RelatedIndicator3, invoiceLine.ZG_RelatedIndicator3)
																					+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceLine.InvoiceHeader.RelatedIndicator4, invoiceLine.ZG_RelatedIndicator4));
	string valuationAdjustment;

	List<ITransportDocument> GetTransportDocuments()
	{
		var result = new List<ITransportDocument>();

		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, "OTH");
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, "JI");
		query.AddToFilter(CusSupportingInfoSchema.CSI_SubType, "TRA");
		var subResultList = invoiceLine.Factory.Load<CusSupportingInfo>(query).ToList();

		for (var i = 0; i < subResultList.Count; i++)
		{
			result.Add(new TransportDocumentProvider(subResultList[i], i + 1));
		}

		return result;
	}
}
