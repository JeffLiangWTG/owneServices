using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public interface ICustomsValueCalculationDataProvider
	{
		ZDecimal LinePriceAmount { get; }
		IEnumerable<JobComInvCharge> AllCharges { get; }
	}

	public interface ICustomsValueCalculationDataProviderForInvoiceHeader : ICustomsValueCalculationDataProvider
	{
		CurrencyConverter CurrencyConverter { get; }
		ICurrency InvoiceCurrency { get; }
	}
}
