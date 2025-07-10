using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashAPInvoiceClusterRefCollection))]
	public class DashAPInvoiceClusterRefCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashAPInvoiceClusterRefCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashAPInvoiceClusterPk = CreateDashAPInvoiceCluster();
			var loadedDashAPInvoiceCluster = Factory.Load<DashAPInvoiceCluster>(createdDashAPInvoiceClusterPk);
			var dashAPInvoiceClusterRefs = loadedDashAPInvoiceCluster.DashAPInvoiceClusterRefs;

			AssertEquals(2, dashAPInvoiceClusterRefs.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashAPInvoiceCluster>().DashAPInvoiceClusterRefs;
		}

		ZGuid CreateDashAPInvoiceCluster()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();
			var dashAPInvoice = factory.NewWithValidTestData<DashAPInvoice>();

			var dashAPInvoiceCluster1 = factory.NewWithValidTestData<DashAPInvoiceCluster>();
			var dashAPInvoiceCluster2 = factory.NewWithValidTestData<DashAPInvoiceCluster>();

			var dashAPInvoiceRef1 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef2 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef3 = factory.NewWithValidTestData<DashAPInvoiceRef>();

			var dashAPInvoiceClusterRef1 = factory.NewWithValidTestData<DashAPInvoiceClusterRef>();
			var dashAPInvoiceClusterRef2 = factory.NewWithValidTestData<DashAPInvoiceClusterRef>();
			var dashAPInvoiceClusterRef3 = factory.NewWithValidTestData<DashAPInvoiceClusterRef>();

			dashAPInvoiceRef1.DPR_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceRef2.DPR_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceRef3.DPR_DPI_HeaderID = dashAPInvoice.PK;

			dashAPInvoiceClusterRef1.DRC_DPC_ClusterID = dashAPInvoiceCluster1.PK;
			dashAPInvoiceClusterRef1.DRC_DPR_RefID = dashAPInvoiceRef1.PK;
			dashAPInvoiceClusterRef2.DRC_DPC_ClusterID = dashAPInvoiceCluster1.PK;
			dashAPInvoiceClusterRef2.DRC_DPR_RefID = dashAPInvoiceRef2.PK;
			dashAPInvoiceClusterRef3.DRC_DPC_ClusterID = dashAPInvoiceCluster2.PK;
			dashAPInvoiceClusterRef3.DRC_DPR_RefID = dashAPInvoiceRef3.PK;

			dashAPInvoiceCluster1.DPC_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceCluster2.DPC_DPI_HeaderID = dashAPInvoice.PK;

			dashAPInvoice.DPI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashAPInvoiceCluster1.PK;
		}
	}
}
