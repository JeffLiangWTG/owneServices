using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SITTCertificationNumberCollection))]
	sealed class SITTCertificationNumberCollectionTest : CusCodeDataCollectionTest<SITTCertificationNumber>
	{
		public void TestAddNewRegNumsAsAString()
		{
			var coll = GetCusCodeDataCollection() as SITTCertificationNumberCollection;
			coll.Add("X1,X2;X3");
			AssertEquals("3 nums", 3, coll.Count);
			Assert(coll.ContainsNumber("X1"));
			Assert(coll.ContainsNumber("X2"));
			Assert(coll.ContainsNumber("X3"));
		}

		public void TestAddNewWithOneParameter()
		{
			var declaration = Factory.New<JobDeclaration>();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			SITTCertificationNumberCollection collection = invoiceLine.SITTCertificationNumbers;
			AssertEquals("Count", 0, collection.Count);
			SITTCertificationNumber number = collection.AddNew("123534");
			AssertEquals("CY_Data", "123534", number.CY_Data);
			AssertEquals("CY_Code", CusCodeDataTypeList.Codes.SITTNumber, number.CY_Code);
		}

		public void TestContainsNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var collection = invoiceLine.SITTCertificationNumbers;
			collection.AddNew("111");
			collection.AddNew("222");
			Assert("Contains 111", collection.ContainsNumber("111"));
			Assert("Contains 222", collection.ContainsNumber("222"));
			Assert("Does not contain 333", !collection.ContainsNumber("333"));
		}

		protected override CusCodeDataCollection<SITTCertificationNumber> GetCusCodeDataCollection()
		{
			return new SITTCertificationNumberCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SITTCertificationNumber>();
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
