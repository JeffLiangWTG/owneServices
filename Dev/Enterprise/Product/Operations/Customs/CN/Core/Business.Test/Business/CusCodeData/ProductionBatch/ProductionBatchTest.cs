using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(ProductionBatch))]
	class ProductionBatchTest : Customs.Business.Testing.CusCodeDataTest<ProductionBatch>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((ProductionBatch)BusinessObject).SupportsNotes);
		}

		public void TestDefaultValues()
		{
			var addinfo = (ProductionBatch)GetNewBusinessObject();
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo.CY_Code);
			AssertEquals(ZString.Empty, addinfo.CY_Data);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ProductionBatch>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ProductionBatch.AddNew();
		}
	}
}
