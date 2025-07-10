using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoiceChargeLineRef))]
	sealed class DashAPInvoiceChargeLineRefTest : EnterpriseBusinessObjectTestCase
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
			var dashAPInvoiceRef = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceChargeLineRef = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine.PK, dashAPInvoiceRef.PK);
			return dashAPInvoiceChargeLineRef;
		}

		public void TestDashAPInvoiceChargeLine_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster.PK);
			var dashAPInvoiceChargeLine2 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);

			var dashAPInvoiceChargeLineRef1 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceChargeLineRef2 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine2.PK, dashAPInvoiceRef2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoiceChargeLineRef = newFactory.Load<DashAPInvoiceChargeLineRef>(dashAPInvoiceChargeLineRef1.PK);

			AssertNotNull(dashAPInvoiceChargeLineRef1.DashAPInvoiceChargeLine);
			AssertEquals(dashAPInvoiceChargeLine1.PK, dashAPInvoiceChargeLineRef1.DashAPInvoiceChargeLine.PK);

			AssertNotNull(dashAPInvoiceChargeLineRef1.DashAPInvoiceRef);
			AssertEquals(dashAPInvoiceRef1.PK, dashAPInvoiceChargeLineRef1.DashAPInvoiceRef.PK);
		}

		public void TestDashAPInvoiceRef_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helper.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceCluster = helper.CreateDashAPInvoiceCluster(dashAPInvoice.PK);
			var dashAPInvoiceChargeLine1 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster.PK);
			var dashAPInvoiceChargeLine2 = helper.CreateDashAPInvoiceChargeLine(dashAPInvoiceCluster.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice.PK);

			var dashAPInvoiceChargeLineRef1 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine1.PK, dashAPInvoiceRef1.PK);
			var dashAPInvoiceChargeLineRef2 = helper.CreateDashAPInvoiceChargeLineRef(dashAPInvoiceChargeLine2.PK, dashAPInvoiceRef2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoiceChargeLineRef = newFactory.Load<DashAPInvoiceChargeLineRef>(dashAPInvoiceChargeLineRef1.PK);

			AssertNotNull(dashAPInvoiceChargeLineRef1.DashAPInvoiceRef);
			AssertEquals(dashAPInvoiceRef1.PK, dashAPInvoiceChargeLineRef1.DashAPInvoiceRef.PK);
		}
	}
}
