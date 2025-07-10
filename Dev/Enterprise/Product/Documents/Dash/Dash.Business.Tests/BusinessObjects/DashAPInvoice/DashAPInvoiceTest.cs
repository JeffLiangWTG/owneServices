using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoice))]
	sealed class DashAPInvoiceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var eDoc = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc.PK, SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helpers.CreateDashAPInvoice(dashDocument.PK);
			return dashAPInvoice;
		}

		public void TestDashDocument_Calculated_Property()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helpers.CreateDashAPInvoice(dashDocument.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoce = newFactory.Load<DashAPInvoice>(dashAPInvoice.PK);

			AssertNotNull(loadedDashAPInvoce.DashDocument);
			AssertEquals(dashDocument.PK, loadedDashAPInvoce.DashDocument.PK);
		}

		public void TestDashAPInvoiceClusters_Calculated_Property()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashDocument2 = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);

			var dashAPInvoice1 = helpers.CreateDashAPInvoice(dashDocument1.PK);
			var dashAPInvoice2 = helpers.CreateDashAPInvoice(dashDocument2.PK);

			var dashAPInvoiceCluster1 = helpers.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);
			var dashAPInvoiceCluster2 = helpers.CreateDashAPInvoiceCluster(dashAPInvoice1.PK);
			var dashAPInvoiceCluster3 = helpers.CreateDashAPInvoiceCluster(dashAPInvoice2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoce = newFactory.Load<DashAPInvoice>(dashAPInvoice1.PK);

			AssertNotNull(loadedDashAPInvoce.DashAPInvoiceClusters);
			AssertEquals(2, loadedDashAPInvoce.DashAPInvoiceClusters.Count);
			var loadedAPInvoiceCluster1 = loadedDashAPInvoce.DashAPInvoiceClusters.FirstOrDefault(x => x.PK == dashAPInvoiceCluster1.PK);
			var loadedAPInvoiceCluster2 = loadedDashAPInvoce.DashAPInvoiceClusters.FirstOrDefault(x => x.PK == dashAPInvoiceCluster2.PK);
			AssertNotNull(loadedAPInvoiceCluster1);
			AssertNotNull(loadedAPInvoiceCluster2);
		}

		public void TestDashAPInvoiceRefs_Calculated_Property()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashDocument2 = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);

			var dashAPInvoice1 = helpers.CreateDashAPInvoice(dashDocument1.PK);
			var dashAPInvoice2 = helpers.CreateDashAPInvoice(dashDocument2.PK);

			var dashAPInvoiceRef1 = helpers.CreateDashAPInvoiceRef(dashAPInvoice1.PK);
			var dashAPInvoiceRef2 = helpers.CreateDashAPInvoiceRef(dashAPInvoice1.PK);
			var dashAPInvoiceRef3 = helpers.CreateDashAPInvoiceRef(dashAPInvoice2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoce = newFactory.Load<DashAPInvoice>(dashAPInvoice1.PK);

			AssertNotNull(loadedDashAPInvoce.DashAPInvoiceRefs);
			AssertEquals(2, loadedDashAPInvoce.DashAPInvoiceRefs.Count);

			var loadedAPInvoiceRef1 = loadedDashAPInvoce.DashAPInvoiceRefs.FirstOrDefault(x => x.PK == dashAPInvoiceRef1.PK);
			var loadedAPInvoiceRef2 = loadedDashAPInvoce.DashAPInvoiceRefs.FirstOrDefault(x => x.PK == dashAPInvoiceRef2.PK);

			AssertNotNull(loadedAPInvoiceRef1);
			AssertNotNull(loadedAPInvoiceRef2);
		}
	}
}
