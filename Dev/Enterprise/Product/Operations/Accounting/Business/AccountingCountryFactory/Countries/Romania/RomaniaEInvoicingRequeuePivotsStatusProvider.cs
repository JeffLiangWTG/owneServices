using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class RomaniaEInvoicingRequeuePivotsStatusProvider : IEInvoicingRequeuePivotsStatusProvider
	{
		public IEnumerable<ZString> GetEligibleForRequeuingStatus() => new List<ZString> { EInvoicingPivotState.Failed, EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Sent, EInvoicingPivotState.Delivered };

		public IEnumerable<ZString> GetSecurityConstraintStatus() => new List<ZString> { EInvoicingPivotState.Sent, EInvoicingPivotState.Delivered };
	}
}
