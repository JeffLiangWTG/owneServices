using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashAPInvoiceRef))]
	sealed class DashAPInvoiceRefTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);

			var eDoc = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc.PK, SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helpers.CreateDashAPInvoice(dashDocument.PK);
			var dashAPInvoiceRef = helpers.CreateDashAPInvoiceRef(dashAPInvoice.PK);
			return dashAPInvoiceRef;
		}

		public void TestDashAPInvoice_Calculated_Property()
		{
			var helper = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument1 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashDocument2 = helper.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice1 = helper.CreateDashAPInvoice(dashDocument1.PK);
			var dashAPInvoice2 = helper.CreateDashAPInvoice(dashDocument2.PK);

			var dashAPInvoiceRef1 = helper.CreateDashAPInvoiceRef(dashAPInvoice1.PK);
			var dashAPInvoiceRef2 = helper.CreateDashAPInvoiceRef(dashAPInvoice2.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashAPInvoiceRef1 = newFactory.Load<DashAPInvoiceRef>(dashAPInvoiceRef1.PK);

			AssertNotNull(loadedDashAPInvoiceRef1.DashAPInvoice);
			AssertEquals(dashAPInvoice1.PK, loadedDashAPInvoiceRef1.DashAPInvoice.PK);
		}
	}
}
