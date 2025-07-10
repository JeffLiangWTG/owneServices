using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Uruguay
{
	class UruguayEInvoicingRequeuePivotsStatusProvider : IEInvoicingRequeuePivotsStatusProvider
	{
		public IEnumerable<ZString> GetEligibleForRequeuingStatus() => new List<ZString> { Constants.EInvoicingPivotState.Failed, Constants.EInvoicingPivotState.BatchedWithError, Constants.EInvoicingPivotState.Sent, Constants.EInvoicingPivotState.Delivered };
		public IEnumerable<ZString> GetSecurityConstraintStatus() => new List<ZString> { Constants.EInvoicingPivotState.Sent, Constants.EInvoicingPivotState.Delivered };
	}
}
