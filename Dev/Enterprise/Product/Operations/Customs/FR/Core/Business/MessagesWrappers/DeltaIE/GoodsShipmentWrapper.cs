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
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentWrapper : IGoodsShipment
	{
		protected GoodsShipmentWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected readonly CusEntryHeader entryHeader;

		public static GoodsShipmentWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new GoodsShipmentWrapper(entryHeader);

		public ICollection<IAdditionalFiscalReference> AdditionalFiscalReference => additionalFiscalReference ?? (additionalFiscalReference = GetAdditionalFiscalReference());
		ICollection<IAdditionalFiscalReference> additionalFiscalReference;

		ICollection<IAdditionalFiscalReference> GetAdditionalFiscalReference()
		{
			var result = new Collection<IAdditionalFiscalReference>();

			entryHeader.EntryInstruction?.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>()
				.DistinctBy(f => f.CFR_Code + f.CFR_Reference)
				.ToList()
				.ForEach(fiscalReference => result.Add(AdditionalFiscalReferenceWrapper.New(fiscalReference)));

			return result;
		}

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;

		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var declaration = entryHeader.Declaration;

			var result = entryHeader
				.AdditionalInfos
				.Where(ai => ai.IsAnAdditionalInformation && !(declaration.ZG_VATDeferType == VATProcedureList.Codes._2 && (ai.CSI_Code == RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption || ai.CSI_Code == RefCusCodeList.AdditionalInformationCodes.AI2WithoutVisaExemption)))
				.DistinctBy(ai => ai.CSI_Code)
				.Select(ai => (IAdditionalInformation)AdditionalInformationWrapper.New(ai, declaration.JE_CustomsOffice))
				.ToCollection();

			return result;
		}

		public ICollection<IAdditionalReference> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
		ICollection<IAdditionalReference> additionalReference;

		ICollection<IAdditionalReference> GetAdditionalReference()
		{
			var result = new Collection<IAdditionalReference>();

			if (entryHeader.Declaration.JE_CustomsProfileRelatedAccount != null)
			{
				result.Add(AdditionalReferenceByCusAccountWrapper.New(entryHeader.Declaration.JE_CustomsProfileRelatedAccount, entryHeader.Declaration.JE_CustomsOffice));
			}

			entryHeader.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(y => y.IsAnAdditionalReference)
				.DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(additionalInfo => result.Add(AdditionalReferenceWrapper.New(additionalInfo, entryHeader.Declaration.JE_CustomsOffice)));

			return result;
		}

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActor());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();

			entryHeader.EntryInstruction?.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
				.DistinctBy(x => x.CFR_Code + x.CFR_Reference)
				.ToList()
				.ForEach(actor => result.Add(AdditionalSupplyChainActorWrapper.New(actor)));

			return result;
		}

		public ICollection<IAdditionsAndDeductions> AdditionsAndDeductions => additionsAndDeductions ?? (additionsAndDeductions = GetAdditionsAndDeductions());
		ICollection<IAdditionsAndDeductions> additionsAndDeductions;

		ICollection<IAdditionsAndDeductions> GetAdditionsAndDeductions()
		{
			var result = new Collection<IAdditionsAndDeductions>();

			var entryHeaderLevelCharges = entryHeader.MergedLines.SelectMany(entryLine => entryLine.CusEntryLineCalculatedFees).Cast<CusEntryLineCalculatedFee>()
				.Where(x => !x.IsLineLevel && !x.Amount.IsEmpty && !x.CustomsCode.IsEmpty);

			foreach (var group in entryHeaderLevelCharges.GroupBy(x => x.CustomsCode))
			{
				var sum = group.Aggregate(Money.Empty, (m, x) => entryHeader.CurrencyConverter.Add(m, x.Money));
				if (!sum.IsEmpty)
				{
					result.Add(AdditionsAndDeductionWrapper.New(group.First().CustomsCode, sum.Amount, sum.Currency?.Code ?? ZString.Empty));
				}
			}

			return result;
		}

		public IBuyer Buyer => buyer ?? (buyer = GetBuyer());

		IBuyer GetBuyer()
		{
			var buyerAddress = RandomInvoice?.BuyerAddress;
			return buyerAddress != null ? BuyerWrapper.New(buyerAddress) : null;
		}

		IBuyer buyer;

		public IConsignment Consignment => consignment ?? (consignment = GetConsignment());

		IConsignment GetConsignment() => ConsignmentWrapper.New(entryHeader);
		IConsignment consignment;

		public ICountryOfDispatch CountryOfDispatch => countryOfDispatch ?? (countryOfDispatch = entryHeader.Declaration != null ? CountryOfDispatchWrapper.New(entryHeader.Declaration.JE_GoodsOrigin) : null);
		ICountryOfDispatch countryOfDispatch;

		public virtual string DateOfAcceptance => dateOfAcceptance ?? (dateOfAcceptance = entryHeader.EntryInstruction?.CEI_DateForDuty.ToString("yyyy-MM-ddTHH:mm:ss") ?? ZString.Empty);
		string dateOfAcceptance;

		public IDeliveryTerms DeliveryTerms => deliveryTerms ?? (deliveryTerms = RandomInvoice != null ? DeliveryTermsWrapper.New(RandomInvoice) : null);
		IDeliveryTerms deliveryTerms;

		public IDestination Destination => destination ?? (destination = entryHeader.Declaration != null ? DestinationWrapper.New(entryHeader.Declaration) : null);
		IDestination destination;

		public IExporter Exporter => exporter ?? (exporter = GetExporter());
		IExporter exporter;

		IExporter GetExporter()
		{
			var result = RandomInvoice?.Supplier != null ? ExporterWrapper.New(RandomInvoice.SupplierAddress) : null;
			if (result == null && RandomInvoice?.JobDeclaration?.SupplierDocumentaryAddress?.Organisation != null)
			{
				result = ExporterWrapper.New(RandomInvoice.JobDeclaration.SupplierDocumentaryAddress.Address);
			}

			return result;
		}

		public IConsignee Consignee => consignee ?? (consignee = GetConsignee());
		IConsignee consignee;

		IConsignee GetConsignee() => RandomInvoice?.ConsigneeAddress != null ? ConsigneeWrapper.New(RandomInvoice.ConsigneeAddress) : null;

		public ICollection<IGoodsShipmentItem> GoodsShipmentItem => goodsShipmentItem ?? (goodsShipmentItem = GetGoodsShipmentItem());
		ICollection<IGoodsShipmentItem> goodsShipmentItem;

		ICollection<IGoodsShipmentItem> GetGoodsShipmentItem()
		{
			var result = new Collection<IGoodsShipmentItem>();

			entryHeader.MergedLines?.Cast<CusEntryLine>().ToList().ForEach(entryLine => result.Add(GoodsShipmentItemWrapper.New(entryLine)));

			return result;
		}

		public double ExchangeRate => exchangeRate != 0d ? exchangeRate : (exchangeRate = GetExchangeRate());
		double exchangeRate;

		double GetExchangeRate()
		{
			var result = 1m;

			if (!entryHeader.IsMultiInvoiceCurrency && RandomInvoice != null)
			{
				result = RandomInvoice.EffectiveExchangeRateForInvoiceCurr;
			}

			return (double)result;
		}

		public string InvoiceCurrency => invoiceCurrency ?? (invoiceCurrency = GetInvoiceCurrency());
		string invoiceCurrency;

		ZString GetInvoiceCurrency()
		{
			var result = FRConstants.FrenchCurrency.CurrencyCode;

			if (!entryHeader.IsMultiInvoiceCurrency)
			{
				if (entryHeader?.InvoiceHeaders.Length > 0)
				{
					result = entryHeader?.InvoiceHeaders?.FirstOrDefault().JZ_RX_NKInvoice_Currency ?? ZString.Empty;
				}
			}

			return result;
		}

		public double TotalAmountInvoiced => totalAmountInvoiced != 0d ? totalAmountInvoiced : totalAmountInvoiced = GetTotalAmountInvoiced();
		double totalAmountInvoiced;

		double GetTotalAmountInvoiced()
		{
			decimal result = decimal.Zero;

			bool isMultiCurrency = entryHeader.IsMultiInvoiceCurrency;

			foreach (CusEntryLine mergeLine in entryHeader.MergedLines)
			{
				if (isMultiCurrency)
				{
					result += mergeLine.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency;
				}
				else
				{
					result += mergeLine.CL_InvoiceAmount;
				}
			}

			return (double)result;
		}

		public string NatureOfTransaction => natureOfTransaction ?? (natureOfTransaction = entryHeader.EntryInstruction?.ZG_TransNature ?? ZString.Empty);
		string natureOfTransaction;

		public ICollection<IGoodsShipmentPreviousDocument> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocument());
		ICollection<IGoodsShipmentPreviousDocument> previousDocument;

		ICollection<IGoodsShipmentPreviousDocument> GetPreviousDocument()
		{
			var result = new Collection<IGoodsShipmentPreviousDocument>();

			entryHeader.PreviousDocuments.Cast<PreviousDocument>()
				.DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber)
				.ToList()
				.ForEach(previousDocument => result.Add(GoodsShipmentPreviousDocumentWrapper.New(previousDocument, entryHeader.Declaration.JE_CustomsOffice)));

			return result;
		}

		public ISeller Seller => seller ?? (seller = RandomInvoice?.Seller != null ? SellerWrapper.New(RandomInvoice.Seller.MainAddress) : null);
		ISeller seller;

		public string SequenceNumber => sequenceNumber ?? (sequenceNumber = "1");
		string sequenceNumber;

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
		ICollection<ISupportingDocument> supportingDocument;

		ICollection<ISupportingDocument> GetSupportingDocument()
		{
			var result = new Collection<ISupportingDocument>();

			entryHeader.SupportingDocuments.Cast<SupportingDocument>().DistinctBy(x => x.CSI_Code + x.CSI_ReferenceNumber).ToList().ForEach(supportingDocument => result.Add(SupportingDocumentWrapper.New(supportingDocument, entryHeader.Declaration.JE_CustomsOffice)));

			return result;
		}

		public IWarehouse Warehouse => warehouse ?? (warehouse = GetWarehouse());
		IWarehouse warehouse;

		IWarehouse GetWarehouse()
		{
			Customs.Business.CusAuthorisationHeader authorisationHeader = null;
			var instruction = entryHeader.EntryInstruction;
			var warehouseAddress = instruction?.Warehouse2;
			if (warehouseAddress?.Header != null)
			{
				authorisationHeader = warehouseAddress.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(instruction.CountryCode).FirstOrDefault();
			}

			return authorisationHeader != null ? WarehouseWrapper.New(authorisationHeader) : null;
		}

		JobComInvoiceHeader RandomInvoice => entryHeader.InvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault();

		public string AdditionalDeclarationType => additionalDeclarationType ?? (additionalDeclarationType = entryHeader.EntryInstruction?.CEI_SubStyle ?? string.Empty);

		public IBillOfDischarge BillOfDischarge => null;

		public IDetailsOfPlannedActivities DetailsOfPlannedActivities => null;

		public IFirstPlaceOfUseOrProcessing FirstPlaceOfUseOrProcessing => null;

		public ICollection<IIdentificationOfGoods> IdentificationOfGoods => null;

		public IPeriodForDischarge PeriodForDischarge => null;

		public ICollection<IProcessedProducts> ProcessedProducts => null;

		string additionalDeclarationType;
	}
}
