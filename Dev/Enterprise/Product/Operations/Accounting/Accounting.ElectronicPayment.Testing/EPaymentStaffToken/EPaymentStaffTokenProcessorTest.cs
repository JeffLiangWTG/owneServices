using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken.Testing
{
	public class EPaymentStaffTokenProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCodes()
		{
			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessorForTesting(logger);
			AssertContainsExactElementsInAnyOrder(new string[] { EDIInterchange.ApplicationCodes.OFX }, processor.ApplicationCodes);
		}

		public void TestIsNoBranchFilter()
		{
			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessorForTesting(logger);
			Assert(processor.IsNoBranchFilter);
		}

		public class EPaymentStaffTokenProcessorForTesting : EPaymentStaffTokenProcessor
		{
			public EPaymentStaffTokenProcessorForTesting(LoggingInformation logger)
				: base(logger)
			{
			}

			public new string[] ApplicationCodes => base.ApplicationCodes;

			public new bool IsNoBranchFilter => base.IsNoBranchFilter;
		}
	}
}
