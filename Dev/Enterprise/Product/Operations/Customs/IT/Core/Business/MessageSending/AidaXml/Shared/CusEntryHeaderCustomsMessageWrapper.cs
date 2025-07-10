using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

class CusEntryHeaderCustomsMessageWrapper : ICusEntryHeaderCustomsMessageWrapper
{
	public CusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		InitializeLazy();
	}

	#region ICusEntryHeaderCustomsMessageWrapper

	ZString ICusEntryHeaderCustomsMessageWrapper.InvoiceCurrencyCode => entryHeader.InvoiceAmountCurrency;

	decimal ICusEntryHeaderCustomsMessageWrapper.ExchangeRate => lazyExchangeRate.Value;
	Lazy<decimal> lazyExchangeRate;

	decimal? ICusEntryHeaderCustomsMessageWrapper.InvoiceTotalAmount => lazyInvoiceTotalAmount.Value;
	Lazy<decimal?> lazyInvoiceTotalAmount;

	int ICusEntryHeaderCustomsMessageWrapper.NatureOfTransaction => lazyNatureOfTransaction.Value;
	Lazy<int> lazyNatureOfTransaction;

	ITermsOfDelivery ICusEntryHeaderCustomsMessageWrapper.TermOfDelivery => lazyTermOfDelivery.Value;
	Lazy<ITermsOfDelivery> lazyTermOfDelivery;

	int ICusEntryHeaderCustomsMessageWrapper.NumberOfPackages => lazyNumberOfPackages.Value;
	Lazy<int> lazyNumberOfPackages;

	IReadOnlyCollection<IAdditionalInformation> ICusEntryHeaderCustomsMessageWrapper.AdditionalInformation => lazyAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	decimal ICusEntryHeaderCustomsMessageWrapper.GrossMass => lazyGrossMass.Value;
	Lazy<decimal> lazyGrossMass;

	ZString ICusEntryHeaderCustomsMessageWrapper.TransportChargesMethodOfPayment => lazyTransportChargesMethodOfPayment.Value;
	Lazy<ZString> lazyTransportChargesMethodOfPayment;

	IReadOnlyCollection<ITransportEquipment> ICusEntryHeaderCustomsMessageWrapper.TransportEquipment => lazyTransportEquipment.Value;
	Lazy<IReadOnlyCollection<ITransportEquipment>> lazyTransportEquipment;

	int? ICusEntryHeaderCustomsMessageWrapper.ExportNatureOfTransaction => lazyExportNatureOfTransaction.Value;
	Lazy<int?> lazyExportNatureOfTransaction;

	ZString ICusEntryHeaderCustomsMessageWrapper.CountryOfDestination => lazyCountryOfDestination.Value;
	Lazy<ZString> lazyCountryOfDestination;

	ZString ICusEntryHeaderCustomsMessageWrapper.CountryOfExport => lazyCountryOfExport.Value;
	Lazy<ZString> lazyCountryOfExport;

	IEoriTrader ICusEntryHeaderCustomsMessageWrapper.Consignor => lazyConsignor.Value;
	Lazy<IEoriTrader> lazyConsignor;

	IEoriTrader ICusEntryHeaderCustomsMessageWrapper.Consignee => lazyConsignee.Value;
	Lazy<IEoriTrader> lazyConsignee;

	ZString ICusEntryHeaderCustomsMessageWrapper.DeferredPayment => lazyDeferredPayment.Value;
	Lazy<ZString> lazyDeferredPayment;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyExchangeRate = new Lazy<decimal>(() => RandomHeader.JZ_InvoiceCurrExRate);
		lazyInvoiceTotalAmount = new Lazy<decimal?>(() => entryHeader.InvoiceAmount.NullIfZero());
		lazyNatureOfTransaction = new Lazy<int>(() => SharedWrapperDataProvider.GetNatureOfTransaction(entryHeader));
		lazyTermOfDelivery = new Lazy<ITermsOfDelivery>(() => TermsOfDeliveryWrapper.NewOrNull(RandomHeader));
		lazyNumberOfPackages = new Lazy<int>(GetNumberOfPackages);
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformation);
		lazyGrossMass = new Lazy<decimal>(GetGrossMass);
		lazyTransportChargesMethodOfPayment = new Lazy<ZString>(GetTransportChargesMethodOfPayment);
		lazyTransportEquipment = new Lazy<IReadOnlyCollection<ITransportEquipment>>(GetTransportEquipment);
		lazyExportNatureOfTransaction = new Lazy<int?>(GetExportNatureOfTransaction);
		lazyCountryOfDestination = new Lazy<ZString>(GetCountryOfDestination);
		lazyCountryOfExport = new Lazy<ZString>(GetCountryOfExport);
		lazyConsignor = new Lazy<IEoriTrader>(GetConsignor);
		lazyConsignee = new Lazy<IEoriTrader>(GetConsignee);
		lazyDeferredPayment = new Lazy<ZString>(GetDeferredPayment);
	}

	int GetNumberOfPackages()
	{
		return InvoiceLines
			.SelectMany(x => x.PackagesPivot)
			.Cast<InvoiceLinePackagePivot>()
			.Where(x => x.Package != null)
			.Sum(x => x.CHC_NumberOfPacks);
	}

	IEnumerable<JobComInvoiceLine> InvoiceLines => entryHeader.MergedLines
		.Cast<CusEntryLine>()
		.SelectMany(x => x.InvoiceLines)
		.Cast<JobComInvoiceLine>();

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
	{
		return new Collection<IAdditionalInformation>() { new NoneOfAboveAdditionalInformationWrapper() };
	}

	JobComInvoiceHeader RandomHeader => entryHeader.RandomHeader;

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment()
	{
		var (containerList, equipmentList) = entryHeader.GetContainerOrEquipmentToEntryLineMapping();

		var result = new List<ITransportEquipment>();
		result.AddRange(GetContainerEquipmentList(containerList));
		result.AddRange(GetTransportEquipmentList(equipmentList));
		return result.AsReadOnly();
	}

	IEnumerable<ITransportEquipment> GetTransportEquipmentList(IDictionary<Customs.Business.CusEquipment, IReadOnlyCollection<Customs.Business.CusEntryLine>> equipmentList)
	{
		return equipmentList
			.OrderBy(keyValuePair => keyValuePair.Key.CEQ_IdentificationNumber)
			.Select(keyValuePair => new
			{
				EntryLineNumbers = keyValuePair
					.Value
					.Select(entryLine => (int)entryLine.CL_LineNumber)
					.ToImmutableHashSet(),
				Seals = ((EU.Business.Declaration.CusEquipment)keyValuePair.Key).Seals
					.Where(cusSeal => !cusSeal.BK_SealNumber.IsEmpty)
					.Select(cusSeal => (string)cusSeal.BK_SealNumber)
					.Distinct()
					.ToImmutableList(),
			})
			.Select(x => new TransportEquipmentWrapper(x.EntryLineNumbers, x.Seals))
			.ToList();
	}

	IEnumerable<ITransportEquipment> GetContainerEquipmentList(IDictionary<Customs.Business.BaseCusContainer, IReadOnlyCollection<Customs.Business.CusEntryLine>> containerList)
	{
		return containerList
			.OrderBy(keyValuePair => keyValuePair.Key.CO_ContainerNumber)
			.Select(x => GetTransportEquipmentWrapper(x))
			.WhereNotNull()
			.ToList();
	}

	static TransportEquipmentWrapper GetTransportEquipmentWrapper(KeyValuePair<Customs.Business.BaseCusContainer, IReadOnlyCollection<Customs.Business.CusEntryLine>> keyValuePair)
	{
		if (keyValuePair.Key is EU.Business.Declaration.CusContainer container)
		{
			var entryLines = keyValuePair.Value;

			var entryLineNumbers = entryLines
				.Select(entryLine => (int)entryLine.CL_LineNumber)
				.ToImmutableHashSet();

			var seals = new[] { container.CO_Seal, container.CO_SecondSeal }
				.Concat(container.AdditionalSeals.Select(x => x.BK_SealNumber))
				.Where(s => !s.IsEmpty)
				.Select(s => s.ToString())
				.Distinct()
				.ToImmutableList();

			return new TransportEquipmentWrapper(container.CO_ContainerNumber, entryLineNumbers, seals);
		}

		return null;
	}

	ZString GetTransportChargesMethodOfPayment()
	{
		var resolver = SharedWrapperDataProvider.GetNewTransportChargesMoPHeaderOrLineValueResolver();
		return resolver.GetValueForHeader(entryHeader);
	}

	ZString GetCountryOfDestination()
	{
		var resolver = SharedWrapperDataProvider.GetNewCountryOfDestinationHeaderOrLineValueResolver();
		return resolver.GetValueForHeader(entryHeader);
	}

	ZString GetCountryOfExport()
	{
		var resolver = SharedWrapperDataProvider.GetNewCountryOfExportHeaderOrLineValueResolver();
		return resolver.GetValueForHeader(entryHeader);
	}

	IEoriTrader GetConsignor() => SharedWrapperDataProvider.GetNewConsignorHeaderOrLineValueResolver().GetValueForHeader(entryHeader);

	IEoriTrader GetConsignee() => SharedWrapperDataProvider.GetNewConsigneeHeaderOrLineValueResolver().GetValueForHeader(entryHeader);

	int? GetExportNatureOfTransaction()
	{
		var resolver = SharedWrapperDataProvider.GetNewNatureOfTransactionHeaderOrLineValueResolver();
		var fieldForHeader = resolver.GetValueForHeader(entryHeader);
		return int.TryParse(fieldForHeader, out var result) ? result : null;
	}

	protected ZString GetDeferredPayment()
	{
		var jobDeclaration = entryHeader.Declaration;
		var paymentMethod = jobDeclaration.JE_PaymentMethod;
		var defermentAccountNumber = jobDeclaration.JE_DefermentAccountNumber;

		if (paymentMethodsDoesNotNeedCIN.Contains(paymentMethod))
		{
			return defermentAccountNumber.RemoveLastCharSafe();
		}
		return defermentAccountNumber;
	}

	decimal GetGrossMass()
	{
		return InvoiceLines.Sum(x => x.GrossWeightInKG);
	}

	#endregion

	readonly CusEntryHeader entryHeader;

	static readonly ImmutableArray<string> paymentMethodsDoesNotNeedCIN = new string[]
	{
		DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14,
		DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority,
		DefermentMethodList.Codes.ConsigneesAccountStandingAuthority,
		DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration,
	}.ToImmutableArray();
}
