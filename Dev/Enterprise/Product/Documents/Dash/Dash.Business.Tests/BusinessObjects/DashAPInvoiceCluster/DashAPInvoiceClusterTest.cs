using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoiceCluster))]
	sealed class DashAPInvoiceClusterTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);

			var eDoc = helper.CreateStorageFile();
			var dashDocument = helper.CreateDashDocument(eDoc.PK, "PIN");
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			return dashAPInvoiceCluster;
		}

		public void TestDashAPInvoice_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashDocument2 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice1 = helper.CreateDashAPInvoice(dashDocument1.PK);
			var dashAPInvoice2 = helper.CreateDashAPInvoice(dashDocument2.PK);
			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoceCluster = newFactory.Load<DashAPInvoiceCluster>(dashAPInvoiceCluster1.PK);

			AssertNotNull(loadedDashAPInvoceCluster.DashAPInvoice);
			AssertEquals(dashAPInvoice1.PK, loadedDashAPInvoceCluster.DashAPInvoice.PK);
		}

		public void TestDashAPInvoiceChargeLines_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);

			var dashAPInvoice1 = helper.CreateDashAPInvoice(dashDocument1.PK);

			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);

			var dashAPInvoiceCluster1ChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster1.PK);
			var dashAPInvoiceCluster1ChargeLine2 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster1.PK);
			var dashAPInvoiceCluster2ChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoceCluster = newFactory.Load<DashAPInvoiceCluster>(dashAPInvoiceCluster1.PK);

			AssertNotNull(loadedDashAPInvoceCluster.DashAPInvoiceChargeLines);
			AssertEquals(2, loadedDashAPInvoceCluster.DashAPInvoiceChargeLines.Count);

			var loadedAPInvoiceCluster1ChargeLine1 = loadedDashAPInvoceCluster.DashAPInvoiceChargeLines.FirstOrDefault(x => x.PK == dashAPInvoiceCluster1ChargeLine1.PK);
			var loadedAPInvoiceCluster1ChargeLine2 = loadedDashAPInvoceCluster.DashAPInvoiceChargeLines.FirstOrDefault(x => x.PK == dashAPInvoiceCluster1ChargeLine2.PK);
			AssertNotNull(loadedAPInvoiceCluster1ChargeLine1);
			AssertNotNull(loadedAPInvoiceCluster1ChargeLine2);
		}

		public void TestDashAPInvoiceClusterRefs_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);

			var dashAPInvoice1 = helper.CreateDashAPInvoice(dashDocument1.PK);

			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice1.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice1.PK);
			var dashAPInvoiceRef3 = helper.CreateDashAPInvoiceRef(dashAPInvoice1.PK);

			var dashAPInvoiceClusterRef1 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceClusterRef2 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster1.PK, dashAPInvoiceRef2.PK);
			var dashAPInvoiceClusterRef3 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster2.PK, dashAPInvoiceRef3.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoceCluster = newFactory.Load<DashAPInvoiceCluster>(dashAPInvoiceCluster1.PK);

			AssertNotNull(loadedDashAPInvoceCluster.DashAPInvoiceClusterRefs);
			AssertEquals(2, loadedDashAPInvoceCluster.DashAPInvoiceClusterRefs.Count);

			var loadedAPInvoiceCluster1Ref1 = loadedDashAPInvoceCluster.DashAPInvoiceClusterRefs.FirstOrDefault(x => x.PK == dashAPInvoiceClusterRef1.PK);
			var loadedAPInvoiceCluster1Ref2 = loadedDashAPInvoceCluster.DashAPInvoiceClusterRefs.FirstOrDefault(x => x.PK == dashAPInvoiceClusterRef2.PK);
			AssertNotNull(loadedAPInvoiceCluster1Ref1);
			AssertNotNull(loadedAPInvoiceCluster1Ref2);
		}
	}
}
