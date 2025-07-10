using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashAPInvoiceChargeLineCollection))]
	public class DashAPInvoiceChargeLineCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashAPInvoiceChargeLineCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashAPInvoiceClusterPk = CreateDashAPInvoiceCluster();
			var loadedDashAPInvoiceCluster = Factory.Load<DashAPInvoiceCluster>(createdDashAPInvoiceClusterPk);
			var dashAPInvoiceChargeLines = loadedDashAPInvoiceCluster.DashAPInvoiceChargeLines;

			AssertEquals(2, dashAPInvoiceChargeLines.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashAPInvoiceCluster>().DashAPInvoiceChargeLines;
		}

		ZGuid CreateDashAPInvoiceCluster()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();

			var dashAPInvoice = factory.NewWithValidTestData<DashAPInvoice>();
			var dashAPInvoiceCluster = factory.NewWithValidTestData<DashAPInvoiceCluster>();
			var dashAPInvoiceChargeLine1 = factory.NewWithValidTestData<DashAPInvoiceChargeLine>();
			var dashAPInvoiceChargeLine2 = factory.NewWithValidTestData<DashAPInvoiceChargeLine>();

			dashAPInvoiceChargeLine1.DPL_DPC_ClusterID = dashAPInvoiceCluster.PK;
			dashAPInvoiceChargeLine2.DPL_DPC_ClusterID = dashAPInvoiceCluster.PK;

			dashAPInvoiceCluster.DPC_DPI_HeaderID = dashAPInvoice.PK;

			dashAPInvoice.DPI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashAPInvoiceCluster.PK;
		}
	}
}
