using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoiceBulkBatchValidationTest : InvoiceBatchHeaderValidationTest
	{
		public new void TestAH_OH()
		{
			InvoiceBulkBatch testBatchHeader = Factory.NewWithValidTestData<InvoiceBulkBatch>();

			testBatchHeader.AH_OH = ZGuid.Empty;
			testBatchHeader.Validation.ValidateAH_OH();
			AssertEquals("Allways has not errors.", false, testBatchHeader.AH_OHInfo.HasErrors());

			testBatchHeader.AH_OH = TestObjectCreator.AALSHI.PK;
			testBatchHeader.Validation.ValidateAH_OH();
			AssertEquals("Allways has not errors.", false, testBatchHeader.AH_OHInfo.HasErrors());
		}
	}
}
