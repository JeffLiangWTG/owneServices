using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashAPInvoiceClusterCollection))]
	public class DashAPInvoiceClusterCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashAPInvoiceClusterCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashAPInvoicePk = CreateDashAPInvoice();
			var loadedDashAPInvoice = Factory.Load<DashAPInvoice>(createdDashAPInvoicePk);
			var dashAPInvoiceClusters = loadedDashAPInvoice.DashAPInvoiceClusters;

			AssertEquals(2, dashAPInvoiceClusters.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashAPInvoice>().DashAPInvoiceClusters;
		}

		ZGuid CreateDashAPInvoice()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();
			var dashAPInvoice = factory.NewWithValidTestData<DashAPInvoice>();
			var dashAPInvoiceCluster1 = factory.NewWithValidTestData<DashAPInvoiceCluster>();
			var dashAPInvoiceCluster2 = factory.NewWithValidTestData<DashAPInvoiceCluster>();

			dashAPInvoiceCluster1.DPC_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceCluster2.DPC_DPI_HeaderID = dashAPInvoice.PK;

			dashAPInvoice.DPI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashAPInvoice.PK;
		}
	}
}
