using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineExportPermitCollection))]
	sealed class InvoiceLineExportPermitCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<InvoiceLineExportPermit>
	{
		public void TestAddNewPermitNumbersAsAString()
		{
			var coll = GetCollectionToTest() as InvoiceLineExportPermitCollection;
			coll.Add("X1,X2;X3");
			AssertEquals("3 nums", 3, coll.Count);
			Assert(coll.ContainsNumber("X1"));
			Assert(coll.ContainsNumber("X2"));
			Assert(coll.ContainsNumber("X3"));
		}

		public void TestContainsNumber()
		{
			var coll = GetCollectionToTest() as InvoiceLineExportPermitCollection;
			var newCode = coll.AddNew();
			newCode.CY_Data = "X1";
			Assert(coll.ContainsNumber("X1"));
		}

		public void TestAddNewWithOneParameter()
		{
			InvoiceLineExportPermitCollection collection = InvoiceLine.Permits;
			AssertEquals("Count", 0, collection.Count);
			InvoiceLineExportPermit permit = collection.AddNew("PERM12");
			AssertEquals("CY_Data", "PERM12", permit.CY_Data);
		}

		protected override Customs.Business.CusCodeDataCollection<InvoiceLineExportPermit> GetCusCodeDataCollection()
		{
			return new InvoiceLineExportPermitCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			InvoiceLineExportPermit result = Factory.New<InvoiceLineExportPermit>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
