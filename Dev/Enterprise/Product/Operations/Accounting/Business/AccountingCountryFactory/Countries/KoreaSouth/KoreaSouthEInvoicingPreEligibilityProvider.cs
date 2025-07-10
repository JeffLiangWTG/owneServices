using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByComplianceDate(AccTransactionHeader transaction, DateTime complianceDate)
		{
			return complianceDate != DateTime.MinValue && transaction.AH_SystemCreateTimeUtc.ToLocationTime(transaction.Branch.HomePort).Date >= (ZDate)complianceDate.Date;
		}
	}
}
