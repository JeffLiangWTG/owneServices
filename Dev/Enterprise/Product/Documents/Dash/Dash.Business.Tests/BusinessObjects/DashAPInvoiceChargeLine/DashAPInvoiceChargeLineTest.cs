using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoiceChargeLine))]
	sealed class DashAPInvoiceChargeLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);

			var eDoc = helper.CreateStorageFile();
			var dashDocument = helper.CreateDashDocument(eDoc.PK, SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceChargeLine = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster.PK);
			return dashAPInvoiceChargeLine;
		}

		public void TestDashAPInvoiceCluster_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);

			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);

			var dashAPInvoiceChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster1.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoceChargeLine = newFactory.Load<DashAPInvoiceChargeLine>(dashAPInvoiceChargeLine1.PK);

			AssertNotNull(dashAPInvoiceChargeLine1.DashAPInvoiceCluster);
			AssertEquals(dashAPInvoiceCluster1.PK, dashAPInvoiceChargeLine1.DashAPInvoiceCluster.PK);
		}

		public void TestDashAPInvoiceChargeLineRefs_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);

			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);

			var dashAPInvoiceChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster1.PK);
			var dashAPInvoiceChargeLine2 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster1.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);

			var dashAPInvoiceChargeLineRef1 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceChargeLineRef2 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine1.PK, dashAPInvoiceRef2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoceChargeLine = newFactory.Load<DashAPInvoiceChargeLine>(dashAPInvoiceChargeLine1.PK);

			AssertNotNull(loadedDashAPInvoceChargeLine.DashAPInvoiceChargeLineRefs);
			var loadedDashAPInvoiceChargeLineRef1 = loadedDashAPInvoceChargeLine.DashAPInvoiceChargeLineRefs.FirstOrDefault(x => x.PK == dashAPInvoiceChargeLineRef1.PK);
			var loadedDashAPInvoiceChargeLineRef2 = loadedDashAPInvoceChargeLine.DashAPInvoiceChargeLineRefs.FirstOrDefault(x => x.PK == dashAPInvoiceChargeLineRef2.PK);

			AssertNotNull(loadedDashAPInvoiceChargeLineRef1);
			AssertNotNull(loadedDashAPInvoiceChargeLineRef2);
		}
	}
}
