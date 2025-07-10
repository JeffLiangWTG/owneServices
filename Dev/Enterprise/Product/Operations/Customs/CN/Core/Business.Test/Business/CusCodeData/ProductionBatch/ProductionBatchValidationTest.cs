using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ProductionBatchValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCY_Data()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var addinfo = invoiceLine.ProductionBatch.AddNew();
			addinfo.Validation.ValidateCY_Data();
			AssertHasWarningContaining(addinfo.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			addinfo.CY_Data = "NO0001";
			AssertNoWarningContaining(addinfo.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
