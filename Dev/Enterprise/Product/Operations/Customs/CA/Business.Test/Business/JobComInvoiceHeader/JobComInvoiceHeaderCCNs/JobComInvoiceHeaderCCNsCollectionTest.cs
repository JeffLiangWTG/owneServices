using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCCNsCollection))]
	sealed class JobComInvoiceHeaderCCNsCollectionTest : ActiveBusinessObjectCollectionTestCase<JobComInvoiceHeaderCCNsCollection>
	{
		public void TestAddNewDefaultType()
		{
			var invoiceCCN = ccnCollection.AddNew();
			AssertEquals(JobComInvoiceHeaderCCNs.Constants.CCN, invoiceCCN.J2_ReferenceType);
			var invoiceCCN2 = ccnCollection.AddNew("123456");
			AssertEquals("123456", invoiceCCN2.J2_ReferenceNumber);
			AssertEquals(JobComInvoiceHeaderCCNs.Constants.CCN, invoiceCCN2.J2_ReferenceType);
		}

		protected override JobComInvoiceHeaderCCNsCollection GetCollectionToTest() => ccnCollection;

		protected override void SetUp()
		{
			base.SetUp();
			var baseDec = Factory.NewWithValidTestData<JobDeclaration>();
			baseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = baseDec.Invoices.AddNew();
			ccnCollection = header.CargoControlNumbersList;
		}
		JobComInvoiceHeaderCCNsCollection ccnCollection;
	}
}
