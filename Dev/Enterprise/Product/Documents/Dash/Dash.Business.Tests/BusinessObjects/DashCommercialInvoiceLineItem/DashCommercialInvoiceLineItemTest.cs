using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashCommercialInvoiceLineItem))]
	sealed class DashCommercialInvoiceLineItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);

			var eDoc = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc.PK, "CIV");
			var dashCommercialInvoice = helpers.CreateDashCommercialInvoice(dashDocument.PK);
			var dashCommercialInvoiceLineItem = helpers.CreateDashCommercialInvoiceLineItem(dashCommercialInvoice.PK);
			return dashCommercialInvoiceLineItem;
		}
	}
}
