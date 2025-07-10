using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashDocument))]
	sealed class DashDocumentTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var eDoc1 = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc1.PK, SharedConstants.ParseType.Code.CommercialInvoice);
			return dashDocument;
		}

		public void TestDashAPInvoice_Calculated_Property()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helpers.CreateDashAPInvoice(dashDocument.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(dashAPInvoice.PK, loadedDashDocument.DashAPInvoice.PK);
		}

		public void TestDashAPInvoice_Calculated_Property_Returns_Null_When_ParseType_Is_Not_PIN()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helpers.CreateDashDocument(SharedConstants.ParseType.Code.CommercialInvoice);
			var dashCommercialInvoice = helpers.CreateDashCommercialInvoice(dashDocument.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertNull(loadedDashDocument.DashAPInvoice);
		}

		public void TestDashCommercialInvoice_Calculated_Property()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helpers.CreateDashDocument(SharedConstants.ParseType.Code.CommercialInvoice);
			var dashCommercialInvoice = helpers.CreateDashCommercialInvoice(dashDocument.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertEquals(dashCommercialInvoice.PK, loadedDashDocument.DashCommercialInvoice.PK);
		}

		public void TestDashCommercialInvoice_Calculated_Property_Returns_Null_When_ParseType_Is_Not_CIV()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var dashDocument = helpers.CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice);
			var dashAPInvoice = helpers.CreateDashAPInvoice(dashDocument.PK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);
			AssertNull(loadedDashDocument.DashCommercialInvoice);
		}
	}
}
