using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoiceClusterRef))]
	sealed class DashAPInvoiceClusterRefTest : EnterpriseBusinessObjectTestCase
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
			var dashAPInvoiceRef = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceClusterRef = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster.PK, dashAPInvoiceRef.PK);
			return dashAPInvoiceClusterRef;
		}

		public void TestDashAPInvoiceCluster_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);

			var dashAPInvoiceClusterRef1 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceClusterRef2 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster2.PK, dashAPInvoiceRef1.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoiceClusterRef1 = newFactory.Load<DashAPInvoiceClusterRef>(dashAPInvoiceClusterRef1.PK);

			AssertNotNull(loadedDashAPInvoiceClusterRef1.DashAPInvoiceCluster);
			AssertEquals(dashAPInvoiceCluster1.PK, loadedDashAPInvoiceClusterRef1.DashAPInvoiceCluster.PK);
		}

		public void TestDashAPInvoiceRef_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster1 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceCluster2 = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);

			var dashAPInvoiceClusterRef1 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceClusterRef2 = helper.CreateDashAPInvoiceClusterRef(dashAPInvoiceCluster2.PK, dashAPInvoiceRef1.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoiceClusterRef1 = newFactory.Load<DashAPInvoiceClusterRef>(dashAPInvoiceClusterRef1.PK);

			AssertNotNull(loadedDashAPInvoiceClusterRef1.DashAPInvoiceRef);
			AssertEquals(dashAPInvoiceRef1.PK, loadedDashAPInvoiceClusterRef1.DashAPInvoiceRef.PK);
		}
	}
}
