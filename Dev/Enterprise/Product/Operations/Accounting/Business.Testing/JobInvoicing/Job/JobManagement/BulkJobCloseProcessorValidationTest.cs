using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class BulkJobCloseProcessorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJobStatusFilter()
		{
			var processor = new BulkJobCloseProcessor();
			processor.JobStatusFilter = "WRK";
			AssertNoErrors(processor.JobStatusFilterInfo);

			processor.JobStatusFilter = "XYZ";
			AssertHasError(processor.JobStatusFilterInfo, "Enter a valid selection.");
		}

		public void TestJobCloseDate()
		{
			var processor = new BulkJobCloseProcessor();
			processor.JobCloseDate = ZDateTime.Today;
			AssertNoErrors(processor.JobCloseDateInfo);

			processor.JobCloseDate = ZDateTime.Empty;
			AssertHasError(processor.JobCloseDateInfo, "Please enter a value.");
		}
	}
}
