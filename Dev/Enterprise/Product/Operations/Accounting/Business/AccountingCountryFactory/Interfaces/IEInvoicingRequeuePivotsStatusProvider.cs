using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingRequeuePivotsStatusProvider
	{
		IEnumerable<ZString> GetEligibleForRequeuingStatus();

		IEnumerable<ZString> GetSecurityConstraintStatus();
	}
}
