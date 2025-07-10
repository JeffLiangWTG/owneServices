using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public abstract class AESHeaderProvider : IAESHeader
	{
		protected AESHeaderProvider(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
			EntryInstruction = EntryHeader?.EntryInstruction;
		}
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		protected readonly CusEntryInstruction EntryInstruction;

		public string LocalReferenceNumber => EntryHeader.LocalReferenceNumber;

		public bool IsContainerized => Declaration.IsContainerised;

		public bool ContainerIndicatorSpecified => !Declaration.JE_ContainerMode.IsEmpty;

		public string MRN => EntryHeader.MovementReferenceNumber;

		public string ExportCustomsOffice => Declaration.JE_CustomsOffice;

		public string SupplementaryDeclarationCustomsOffice => Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice);

		public IAESParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => !EntryInstruction.Constellation2ndDigitIs1() ? PartyProvider.NewOrNull(Declaration.Declarant) : null);
		CachedValue<IAESParty> declarantCached;

		public IAESParty Representative => CachedValueHelper.GetValue(ref representativeCached, () => EntryInstruction.Constellation3rdDigitIs1() ? PartyProvider.NewOrNull(Declaration.Representative, GlbStaff.CurrentUser) : null);
		CachedValue<IAESParty> representativeCached;

		public string InlandTransportMeansMode => !EntryInstruction.Style4thDigitIs9() ? Declaration.TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportModeInland).ToString() : null;

		public bool ActiveBorderTransportMeansSpecified => !BorderTransportMeansMode.IsNullOrEmpty() && !BorderTransportMeansType.IsNullOrEmpty();

		public string BorderTransportMeansMode => Declaration.TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportMode);

		public string BorderTransportMeansType => Declaration.ZG_BorderTransportMeans;

		public string BorderTransportMeansIdentity => CachedValueHelper.GetValue(ref borderTransportMeansIdentityCached, () =>
		{
			string result = null;
			if (ActiveBorderTransportMeansSpecified)
			{
				var transportMode = Declaration.JE_TransportMode;
				if (transportMode == Core.Constants.TransportModes.Sea && Declaration.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10)
				{
					result = Declaration.Vessel?.RV_LloydsNumber ?? null;
				}
				else if (transportMode == Core.Constants.TransportModes.Air)
				{
					result = Declaration.JE_VoyageFlightNo;
				}
				else
				{
					result = Declaration.JE_VesselName;
				}
			}
			return result;
		});
		CachedValue<string> borderTransportMeansIdentityCached;

		public string BorderTransportMeansNationality => Declaration.JE_RN_NKTransportNationality;

		public string TransactionType => CachedValueHelper.GetValue(ref transactionTypeCached, () => EntryHeader.GetAllSameValue(x => x.JZ_ValuationCode));
		CachedValue<string> transactionTypeCached;

		public decimal InvoiceAmount => CachedValueHelper.GetValue(ref invoiceAmount, () =>
		{
			var result = 0m;
			var invoiceHeadersNotFreeOfCharge = InvoiceHeadersNotFreeOfCharge;
			var hasMultipleCurrencies = HasMultipleCurrencies(invoiceHeadersNotFreeOfCharge);

			foreach (var header in invoiceHeadersNotFreeOfCharge)
			{
				if (hasMultipleCurrencies)
				{
					result += header.JZ_InvoiceAmountInLocalCurrency;
				}
				else
				{
					result += header.JZ_InvoiceAmount;
				}
			}
			return CargoWise.Customs.Shared.MessageContracts.DecimalExtensions.RoundAndNormalize(result, 2);
		});
		CachedValue<decimal> invoiceAmount;

		public string Currency => GetCurrency();

		string GetCurrency()
		{
			string currency;
			var invoiceHeadersNotFreeOfCharge = InvoiceHeadersNotFreeOfCharge;
			if (invoiceHeadersNotFreeOfCharge.Length > 0)
			{
				var header = invoiceHeadersNotFreeOfCharge.First();
				currency = HasMultipleCurrencies(invoiceHeadersNotFreeOfCharge)
					? header.LocalCurrencyCode
					: header.JZ_RX_NKInvoice_Currency;
			}
			else
			{
				currency = EntryHeader.TotalPrice.Currency?.Code;
			}

			return currency;
		}

		JobComInvoiceHeader[] InvoiceHeadersNotFreeOfCharge =>
			DistinctInvoiceHeaders.Where(i => !i.JZ_FreeOfCharge).ToArray();

		bool HasMultipleCurrencies(JobComInvoiceHeader[] invoiceHeadersNotFreeOfCharge) =>
			IEnumerableExtensions.DistinctBy(invoiceHeadersNotFreeOfCharge, i => i.JZ_RX_NKInvoice_Currency).IsCountMoreThan(1);

		public string CommercialReferenceNumber => CachedValueHelper.GetValue(ref commercialReferenceNumber, () => EntryHeader.GetAllSameValue(x => x.JZ_UCR));
		CachedValue<string> commercialReferenceNumber;

		public IDeliveryTerms DeliveryTerms => CachedValueHelper.GetValue(ref deliveryTermsCached, GetDeliveryTerms);
		CachedValue<IDeliveryTerms> deliveryTermsCached;

		IDeliveryTerms GetDeliveryTerms()
		{
			IDeliveryTerms result = null;
			var invoice = EntryHeader.RandomHeader;
			if (invoice != null)
			{
				var incoTerm = invoice.JZ_IncoTerm;
				var incoTermPlace = invoice.JZ_IncoTermPlace;
				if (!incoTerm.IsEmpty && EntryHeader.InvoiceHeaders.AllSame(x => x.JZ_IncoTerm == incoTerm && x.JZ_IncoTermPlace == incoTermPlace))
				{
					result = GetDeliveryTermsFromInvoice((JobComInvoiceHeader)invoice);
				}
			}
			return result;
		}

		protected virtual IDeliveryTerms GetDeliveryTermsFromInvoice(JobComInvoiceHeader invoice)
		{
			return new ExportHeaderDeliveryTermsProvider(invoice);
		}

		public decimal TotalGrossMass => EntryHeader.GrossWeight.InKilogramsSafe.Round(3).Normalize();

		public IDateAndTime SubmissionDateAndTimeUtc => submissionDateAndTimeUtc ?? (submissionDateAndTimeUtc = new UniversalDateAndTimeProvider());
		IDateAndTime submissionDateAndTimeUtc;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.CreateCollection(Declaration));
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = Declaration.IsContainerised ? Declaration.CusContainers
			.Where(x => !x.CO_ContainerNumber.IsEmpty).Select(x => TransportEquipmentProvider.NewOrNull(EntryInstruction, x)).ToArray() : Array.Empty<ITransportEquipment>());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public bool InvoiceAmountAndCurrencySpecified => CachedValueHelper.GetValue(ref invoiceAmountAndCurrencySpecified, () =>
		{
			return InvoiceAmount != 0 || DistinctInvoiceHeaders.Any(x => !x.JZ_RX_NKInvoice_Currency.IsEmpty);
		});
		CachedValue<bool> invoiceAmountAndCurrencySpecified;

		protected IReadOnlyCollection<JobComInvoiceHeader> DistinctInvoiceHeaders => distinctInvoiceHeaders
			?? (distinctInvoiceHeaders = EntryInstruction != null ? EntryInstruction.InvoiceLines.Select(l => new { l.InvoiceHeader.PK, l.InvoiceHeader }).GroupBy(h => h).Select(x => x.Key.InvoiceHeader).Cast<JobComInvoiceHeader>().ToArray() : Array.Empty<JobComInvoiceHeader>());
		IReadOnlyCollection<JobComInvoiceHeader> distinctInvoiceHeaders;
	}
}
