using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForHungary))]
	public class EInvoicingBatchCreatorForHungaryTest : EInvoicingDependentBatchCreatorTest
	{
		protected override string Country => CountryCodes.Hungary;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForHungary(company);

		protected override string ExpectedDependentTransactionStatusWhenOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled => EInvoicingPivotState.Batched;
	}
}
