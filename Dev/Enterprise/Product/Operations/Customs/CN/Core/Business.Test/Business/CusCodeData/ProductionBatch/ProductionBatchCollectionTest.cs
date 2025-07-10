using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(ProductionBatchCollection))]
	class ProductionBatchCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ProductionBatch>
	{
		public void TestRelationshipFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();
			var addinfo = invoiceLine.ProductionBatch.AddNew();
			addinfo.CY_Data = "NO001";
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo.CY_Code);
			AssertEquals("NO001", addinfo.CY_Data);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(1, invoiceLine.ProductionBatch.Count);
			var addinfo1 = invoiceLine.ProductionBatch[0];
			AssertEquals(addinfo.PK, addinfo1.PK);
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo1.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo1.CY_Code);
			AssertEquals("NO001", addinfo1.CY_Data);
		}

		protected override CusCodeDataCollection<ProductionBatch> GetCusCodeDataCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new ProductionBatchCollection(invoiceLine);
		}
	}
}
