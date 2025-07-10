#if DEBUG

using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class ExchangeRate
	{
		public bool IsDataVersionsAutoLogged_ForTestOnly => ((IDataVersionLoggingSupported)this).IsDataVersionsAutoLogged;

		public IUniqueIndexFailureHandler UniqueIndexFailureHandler_ForTestOnly => UniqueIndexFailureHandlers.Single();

		internal void InvokeChanged_ForTestsOnly()
		{
			changed?.Invoke(this, new EventArgs());
		}

		void IExchangeRate.SetBuyRate_ForTestOnly(decimal rate)
		{
			JF_BaseRate = rate;
		}

		public ZAccExchangeRate ZAccExchangeRate_ForTestOnly => ZAccExchangeRate;
	}
}

#endif
