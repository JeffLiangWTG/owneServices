using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business
{
	public class ExchangeRatesSettings : AutoExchangeRatesSettings
	{
		public ExchangeRatesSettings(ExRateSourceType[] rateSources)
		{
			exchangeRatesSource_List = new ExchangeRatesSourceList(rateSources);
		}

		readonly ExchangeRatesSourceList exchangeRatesSource_List;

		[List("ExchangeRatesSource_List")]
		public override ZString ExchangeRatesSource
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ExchangeRatesSource; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ExchangeRatesSource = value; }
		}

		public ExchangeRatesSourceList ExchangeRatesSource_List
		{
			get { return exchangeRatesSource_List; }
		}
	}
}

