using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.BE.Business;

public class GoodsShipmentProvider : IGoodsShipment
{
	public GoodsShipmentProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.declaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(entryHeader)}.{nameof(entryHeader.Declaration)}");
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(entryHeader.EntryInstruction)}");
		authorisation = RetrieveAuthorisationHeader(declaration.WarehouseDocAddress.OrganisationPK);
	}

	readonly CusEntryHeader entryHeader;
	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclaration declaration;
	readonly CusAuthorisationHeader authorisation;

	public string NatureOfTransaction => entryHeader.InvoiceHeaders.FirstOrDefault()?.JZ_ValuationCode;

	public string CountryOfExport => declaration.IsTransitionPeriodAES30 ? string.Empty : declaration.JE_RL_NKOrigin;

	public string CountryOfDestination => declaration.IsTransitionPeriodAES30 ? string.Empty : declaration.JE_GoodsDestination;

	public string WarehouseType => CachedValueHelper.GetValue(ref warehouseTypeCached, () =>
	{
		var type = authorisation?.CPH_Type;
		switch (type)
		{
			case CusAuthorizationHeaderTypeList.Codes.TemporaryStorage:
				return Constants.AESWarehouseCodes.WarehouseTypeV;
			case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
				return Constants.AESWarehouseCodes.WarehouseTypeR;
			case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
				return Constants.AESWarehouseCodes.WarehouseTypeS;
			case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
				return Constants.AESWarehouseCodes.WarehouseTypeU;
		}
		return type;
	});
	CachedValue<string> warehouseTypeCached;

	public string WarehouseIdentifier => authorisation?.CPH_Number;

	public IDeliveryTerms DeliveryTerms => deliveryTerms ?? (deliveryTerms = new DeliveryTermsProvider(entryHeader.RandomHeader));
	IDeliveryTerms deliveryTerms;

	public IConsignment Consignment => consignment ?? (consignment = new ConsignmentProvider(declaration, entryInstruction, entryHeader));
	IConsignment consignment;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors =
		entryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()
			.Select((x, i) => new AdditionalSupplyChainActorProvider(x, i + 1)).ToArray<IAdditionalSupplyChainActor>());
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments =
		entryInstruction.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>()
			.Select((x, index) => new PreviousDocumentsProvider(x, index + 1)).ToArray<IDocument>());

	IReadOnlyCollection<IDocument> previousDocuments;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
		entryInstruction.SupportingDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>()
			.Select((x, index) => new SupportingDocumentsProvider(x, index + 1)).ToArray<ISupportingDocument>());

	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
		entryInstruction.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>()
			.Where(x => x.CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalReference)
			.Select((x, i) => new AdditionalReferenceProvider(x, i + 1)).ToArray<IDocument>());
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations =
		entryInstruction.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>()
			.Where(x => x.CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalInformation)
			.Select((x, index) => new AdditionalInformationProvider(x, index + 1)).ToArray<IAdditionalInformation>());
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IGoodsItem> GoodsItems => goodsItems ?? (goodsItems =
		declaration.InvoiceLines.Cast<Declaration.JobComInvoiceLine>()
			.Select((x) => new GoodsItemsProvider(x)).ToArray<IGoodsItem>());
	IReadOnlyCollection<IGoodsItem> goodsItems;

	JobComInvoiceHeader invoiceHeader => declaration.Invoices.FirstOrDefault() as JobComInvoiceHeader;

	public decimal TotalAmountInvoiced => entryHeader.InvoiceHeaders.Sum(x => x.JZ_InvoiceAmount);

	public string InvoiceCurrency => invoiceHeader?.JZ_RX_NKInvoice_Currency;

	public decimal ExchangeRate => invoiceHeader?.JZ_InvoiceCurrExRate ?? 0m;

	public string CountryOfDispatch => declaration.JE_GoodsOrigin;

	public IParty Buyer => buyer ?? (buyer = (invoiceHeader != null ? new PartyProvider(invoiceHeader.BuyerAddress) : null));
	IParty buyer;

	public IParty Seller => seller ?? (seller = (invoiceHeader != null ? new PartyProvider(invoiceHeader.SellerAddress) : null));
	IParty seller;

	public IParty Exporter => exporter ?? (exporter = (invoiceHeader != null ? new PartyProvider(invoiceHeader.ExporterAddress) : null));
	IParty exporter;

	public IDestination Destination => destination ?? (destination = new DestinationProvider(declaration.JE_GoodsDestination, declaration.ZG_RegionOfDestination));
	IDestination destination;

	public DateTime AcceptanceDate => default;

	public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsAndDeductions => additionsAndDeductions ?? (additionsAndDeductions = new List<IAdditionsAndDeductions>());
	IReadOnlyCollection<IAdditionsAndDeductions> additionsAndDeductions;

	public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences => additionalFiscalReferences ?? (additionalFiscalReferences = new List<IAdditionalFiscalReference>());
	IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

	CusAuthorisationHeader RetrieveAuthorisationHeader(ZGuid organisationPK)
	{
		CusAuthorisationHeader permit = null;

		var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
		query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, organisationPK);
		var queryResult = entryHeader.Factory.Load<CusAuthorisationHeader>(query);

		if (queryResult.Any())
		{
			permit = queryResult.FirstOrDefault();
		}

		return permit;
	}
}
