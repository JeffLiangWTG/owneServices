using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashAPInvoiceRefCollection))]
	public class DashAPInvoiceRefCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashAPInvoiceRefCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashAPInvoicePk = CreateDashAPInvoice();
			var loadedDashAPInvoice = Factory.Load<DashAPInvoice>(createdDashAPInvoicePk);
			var dashAPInvoiceRefs = loadedDashAPInvoice.DashAPInvoiceRefs;

			AssertEquals(2, dashAPInvoiceRefs.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashAPInvoice>().DashAPInvoiceRefs;
		}

		ZGuid CreateDashAPInvoice()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();
			var dashAPInvoice1 = factory.NewWithValidTestData<DashAPInvoice>();
			var dashAPInvoice2 = factory.NewWithValidTestData<DashAPInvoice>();

			var dashAPInvoiceRef1 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef2 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef3 = factory.NewWithValidTestData<DashAPInvoiceRef>();

			dashAPInvoiceRef1.DPR_DPI_HeaderID = dashAPInvoice1.PK;
			dashAPInvoiceRef2.DPR_DPI_HeaderID = dashAPInvoice1.PK;
			dashAPInvoiceRef3.DPR_DPI_HeaderID = dashAPInvoice2.PK;

			dashAPInvoice1.DPI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashAPInvoice1.PK;
		}
	}
}
