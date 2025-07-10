using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashAPInvoiceChargeLineRefCollection))]
	public class DashAPInvoiceChargeLineRefCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashAPInvoiceChargeLineRefCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashAPInvoiceChargeLinePk = CreateDashAPInvoiceChargeLine();
			var loadedDashAPInvoiceChargeLine = Factory.Load<DashAPInvoiceChargeLine>(createdDashAPInvoiceChargeLinePk);
			var dashAPInvoiceChargeLineRefs = loadedDashAPInvoiceChargeLine.DashAPInvoiceChargeLineRefs;

			AssertEquals(2, dashAPInvoiceChargeLineRefs.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashAPInvoiceChargeLine>().DashAPInvoiceChargeLineRefs;
		}

		ZGuid CreateDashAPInvoiceChargeLine()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();

			var dashAPInvoice = factory.NewWithValidTestData<DashAPInvoice>();
			var dashAPInvoiceCluster = factory.NewWithValidTestData<DashAPInvoiceCluster>();
			var dashAPInvoiceChargeLine1 = factory.NewWithValidTestData<DashAPInvoiceChargeLine>();
			var dashAPInvoiceChargeLine2 = factory.NewWithValidTestData<DashAPInvoiceChargeLine>();

			var dashApInvoiceChargeLine1Ref1 = factory.NewWithValidTestData<DashAPInvoiceChargeLineRef>();
			var dashApInvoiceChargeLine1Ref2 = factory.NewWithValidTestData<DashAPInvoiceChargeLineRef>();
			var dashApInvoiceChargeLine2Ref1 = factory.NewWithValidTestData<DashAPInvoiceChargeLineRef>();

			var dashAPInvoiceRef1 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef2 = factory.NewWithValidTestData<DashAPInvoiceRef>();
			var dashAPInvoiceRef3 = factory.NewWithValidTestData<DashAPInvoiceRef>();

			dashAPInvoiceRef1.DPR_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceRef2.DPR_DPI_HeaderID = dashAPInvoice.PK;
			dashAPInvoiceRef3.DPR_DPI_HeaderID = dashAPInvoice.PK;

			dashApInvoiceChargeLine1Ref1.DLR_DPL_ChargeLineID = dashAPInvoiceChargeLine1.PK;
			dashApInvoiceChargeLine1Ref2.DLR_DPL_ChargeLineID = dashAPInvoiceChargeLine1.PK;
			dashApInvoiceChargeLine2Ref1.DLR_DPL_ChargeLineID = dashAPInvoiceChargeLine2.PK;

			dashAPInvoiceChargeLine1.DPL_DPC_ClusterID = dashAPInvoiceCluster.PK;
			dashAPInvoiceChargeLine2.DPL_DPC_ClusterID = dashAPInvoiceCluster.PK;

			dashAPInvoiceCluster.DPC_DPI_HeaderID = dashAPInvoice.PK;

			dashAPInvoice.DPI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashAPInvoiceChargeLine1.PK;
		}
	}
}
