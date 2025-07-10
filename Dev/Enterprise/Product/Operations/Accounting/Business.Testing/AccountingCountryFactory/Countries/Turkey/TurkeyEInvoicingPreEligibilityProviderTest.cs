using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(TurkeyEInvoicingPreEligibilityProvider))]
	public class TurkeyEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.Turkey;

		public override void TestCanEvaluateByTransaction()
		{
			AccTransactionHeader transaction = null;
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				AssertEquals("When ComplianceDocumentNumberAllocation_Receivables is set to Manual, CanEvaluateByTransaction should return true", true, GetInvoicingPreEligibilityProvider().CanEvaluateByTransaction(transaction));
			}

			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				AssertEquals("When ComplianceDocumentNumberAllocation_Receivables is set to a value other than Manual, CanEvaluateByTransaction should return false", false, GetInvoicingPreEligibilityProvider().CanEvaluateByTransaction(transaction));
			}
		}
	}
}
