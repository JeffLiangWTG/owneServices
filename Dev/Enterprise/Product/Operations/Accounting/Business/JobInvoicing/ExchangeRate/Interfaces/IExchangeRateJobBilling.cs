using System;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IExchangeRateJobBilling : IExchangeRate
	{
		ZDecimal SellRate { get; }

		ZGuid OrgPk { get; }

		ExchangeRateOrgTypeEnum OrgType { get; }

		ZDecimal CFXPercent { get; }

		ZDecimal CFXMinimum { get; }

		event EventHandler Changed;

		void EnsureWillNotBeAutoDeleted();

		void SetBaseRate(decimal value);

		void RefreshCFXMinimum();

		ZGuid ExchangeRatePk { get; }

		bool IsUserDefinedOrTransformed { get; }

		bool IsDeleted { get; }
	}
}
