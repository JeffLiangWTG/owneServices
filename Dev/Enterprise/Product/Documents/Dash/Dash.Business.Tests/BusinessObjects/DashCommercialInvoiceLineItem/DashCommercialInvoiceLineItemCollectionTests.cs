using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests.BusinessObjects
{
	[TestedType(typeof(DashCommercialInvoiceLineItemCollection))]
	public class DashCommercialInvoiceLineItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(DashCommercialInvoiceLineItemCollection), GetCollectionToTest().GetType());
		}

		public void TestCollectionItemsAreLoaded()
		{
			var createdDashCommercialInvoicePk = CreateDashCommercialInvoice();
			var loadedCommercialInvoice = Factory.Load<DashCommercialInvoice>(createdDashCommercialInvoicePk);
			var dashCommercialInviceLineItems = loadedCommercialInvoice.CommercialInvoiceLineItems;

			AssertEquals(2, dashCommercialInviceLineItems.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DashCommercialInvoice>().CommercialInvoiceLineItems;
		}

		ZGuid CreateDashCommercialInvoice()
		{
			var factory = new BusinessObjectFactory();
			var dashDocument = factory.NewWithValidTestData<DashDocument>();
			var dashCommercialInvoice = factory.NewWithValidTestData<DashCommercialInvoice>();
			var dashCommercialInvoiceLine1 = factory.NewWithValidTestData<DashCommercialInvoiceLineItem>();
			var dashCommercialInvoiceLine2 = factory.NewWithValidTestData<DashCommercialInvoiceLineItem>();
			dashCommercialInvoiceLine1.DLI_DCI_HeaderID = dashCommercialInvoice.PK;
			dashCommercialInvoiceLine2.DLI_DCI_HeaderID = dashCommercialInvoice.PK;
			dashCommercialInvoice.DCI_DDD_DashDocID = dashDocument.PK;

			factory.Save();

			return dashCommercialInvoice.PK;
		}
	}
}
