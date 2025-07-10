using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using IDocument = CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument;

namespace Enterprise.Customs.IE.Business.AIS
{
	class GoodsShipmentProvider : IGoodsShipment
	{
		readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;
		readonly CusEntryHeader entryHeader;
		readonly JobComInvoiceHeader randomInvoiceHeader;

		public GoodsShipmentProvider(int sequenceNumber, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
			entryHeader = entryHeaderWrapper.EntryHeader;
			randomInvoiceHeader = entryHeaderWrapper.RandomInvoiceHeader;
			SequenceNumber = sequenceNumber.ToString();
		}

		public string SequenceNumber { get; }

		public string NatureOfTransaction => randomInvoiceHeader.JZ_ValuationCode;

		public decimal TotalAmountInvoiced => declaration.Invoices.Sum(x => x.JZ_InvoiceAmount);

		public string InvoiceCurrency => randomInvoiceHeader.JZ_RX_NKInvoice_Currency;

		public DateTime DateOfAcceptance => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(instruction.CEI_DateForDuty.IsValid ? instruction.CEI_DateForDuty : ZDateTime.Empty, true);

		public decimal ExchangeRate => randomInvoiceHeader.JZ_InvoiceCurrExRate;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors =
			instruction.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
				.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Buyer => null;

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Seller => null;

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () =>
		!entryHeader.HasInvoiceExporterAddress && declaration.SupplierDocumentaryAddress?.Address is OrgAddress address ? new PartyProvider(address) : null);
		CachedValue<CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty> exporterCached;

		public IDeliveryTerms DeliveryTerms => CachedValueHelper.GetValue(ref deliveryTerms, () => DeliveryTermsProvider.New(randomInvoiceHeader));
		CachedValue<IDeliveryTerms> deliveryTerms;

		public string CountryOfDispatch => declaration.JE_GoodsOrigin;

		public string MemberStateTerritory => null;

		public IDestination Destination => CachedValueHelper.GetValue(ref destinationCached, () =>
		{
			return DestinationProvider.New(ZString.Empty, declaration.ZG_RegionOfDestination);
		});
		CachedValue<IDestination> destinationCached;

		public IIdType Warehouse => CachedValueHelper.GetValue(ref warehouseCached, () =>
		{
			var type = instruction.ToWarehouseType;
			var id = instruction.ToWarehouseCode;

			if (type.IsEmpty || id.IsEmpty)
			{
				return null;
			}

			return IdTypeProvider.New(type, id);
		});
		CachedValue<IIdType> warehouseCached;

		public IReadOnlyCollection<IMPreviousDocument> PreviousDocuments => previousDocumentsCached ?? (previousDocumentsCached = entryHeader.PreviousDocuments.Select(x => new MPreviousDocumentProvider(x)).ToArray<IMPreviousDocument>());
		IReadOnlyCollection<IMPreviousDocument> previousDocumentsCached;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocumentsCached ?? (supportingDocumentsCached = entryHeader.SupportingDocuments.Select(x => new SupportingDocumentProvider(x)).ToArray<ISupportingDocument>());
		IReadOnlyCollection<ISupportingDocument> supportingDocumentsCached;

		public IReadOnlyCollection<ICcQualifierDocument> AdditionalReferences => additionalReferencesCached ?? (additionalReferencesCached = entryHeader.GetAdditionalReferences<CcQualifierDocumentProvider>().ToArray<ICcQualifierDocument>());
		IReadOnlyCollection<ICcQualifierDocument> additionalReferencesCached;

		public IReadOnlyCollection<ICcQualifierAdditionalInformation> AdditionalInformations => additionalInformationsCached ?? (additionalInformationsCached = entryHeader.GetAdditionalInformations<CcQualifierAdditionalInformationProvider>().ToArray<ICcQualifierAdditionalInformation>());
		IReadOnlyCollection<ICcQualifierAdditionalInformation> additionalInformationsCached;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocumentsCached ?? (transportDocumentsCached = entryHeader.GetTransportDocuments<DocumentProvider>().ToArray<IDocument>());
		IReadOnlyCollection<IDocument> transportDocumentsCached;

		public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsAndDeductions => additionsAndDeductions ?? (additionsAndDeductions = getAdditionsAndDeductions());
		IReadOnlyCollection<IAdditionsAndDeductions> additionsAndDeductions;

		IReadOnlyCollection<IAdditionsAndDeductions> getAdditionsAndDeductions()
		{
			var apportionedCharges = new Dictionary<string, decimal>();

			foreach (var invoiceLine in entryHeader.InvoiceLines)
			{
				foreach (var apportionedCharge in invoiceLine.ApportionedCharges)
				{
					var dictionaryCharge = apportionedCharges.GetOrAdd(apportionedCharge.J7_ChargeType);
					apportionedCharges[apportionedCharge.J7_ChargeType] = dictionaryCharge + apportionedCharge.MoneyInLocalCurrency.Amount;
				}

				foreach (var charge in invoiceLine.Charges)
				{
					var dictionaryCharge = apportionedCharges.GetOrAdd(charge.J7_ChargeType);
					apportionedCharges[charge.J7_ChargeType] = dictionaryCharge + charge.MoneyInInvoiceCurrency.Amount;
				}
			}

			return apportionedCharges.Select(y => new AdditionsAndDeductionsProvider(y.Key, y.Value)).ToArray();
		}

		public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences => additionalFiscalReferences ?? (additionalFiscalReferences = instruction.FiscalReferences.Select((x, index) => new AdditionalFiscalReferenceProvider(index + 1, x.CFR_Code, MessageProviderHelper.GetVatIdentificationNumber(instruction.Factory, x.OwnerOrgPK))).ToArray());
		IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

		public IMoney PostalCharges => null;

		public IMConsignment04 Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new MConsignment04Provider(entryHeaderWrapper));
		CachedValue<IMConsignment04> consignmentCached;

		public IReadOnlyCollection<IGoodsShipmentItem> GoodsShipmentItems => goodsShipmentItem ?? (goodsShipmentItem =
			entryHeader.MergedLines
				.Select(x => new GoodsShipmentItemProvider(x, entryHeaderWrapper)).ToArray());

		IReadOnlyCollection<IGoodsShipmentItem> goodsShipmentItem;

		public IReadOnlyCollection<IAuthorisation> Authorisations => Array.Empty<IAuthorisation>();

		public ICommodity Commodity => null;

		public IReadOnlyCollection<string> ContainerIds => Array.Empty<string>();

		public ICustomsValuation CustomsValuation => null;

		public string DeclarationGoodsItemNumber => null;

		public IOrigin Origin => null;

		public IReadOnlyCollection<IPackaging> Packages => Array.Empty<IPackaging>();

		public IProcedure Procedure => null;

		public string ReferenceNumberUCR => null;

		public IMoney TransportAndInsuranceCostsToTheDestination => null;

		public string ValuationAdjustment => null;
	}
}
