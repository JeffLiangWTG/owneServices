using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(PrimaryOrgSelector))]
	public class PrimaryOrgSelectorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrimaryOrgSelector(Factory, TestObjectCreator.AALSHI);
		}

		protected override void SetUp()
		{
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			apInvoice.AH_OutstandingAmount = 100M;
			arInvoice.AH_OutstandingAmount = 200M;

			primaryOrg = (PrimaryOrgSelector)GetNewBusinessObject();
			primaryOrg.Transactions.Add(apInvoice);
			primaryOrg.Transactions.Add(arInvoice);
		}

		public void TestOrganisationPK()
		{
			AssertEquals(TestObjectCreator.AALSHI.PK, primaryOrg.OrganisationPK);
		}

		public void TestOrganisationCode()
		{
			AssertEquals(TestObjectCreator.AALSHI.OH_Code, primaryOrg.OrganisationCode);
		}

		public void TestOrganisationName()
		{
			AssertEquals(TestObjectCreator.AALSHI.OH_FullName, primaryOrg.OrganisationName);
		}

		public void TestTotalLocalOutstandingAmount()
		{
			AssertEquals(300M, primaryOrg.TotalLocalOutstandingAmount);
		}

		public void TestTransactionsCount()
		{
			AssertEquals(2, primaryOrg.TransactionsCount);
		}

		PrimaryOrgSelector primaryOrg;
		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
