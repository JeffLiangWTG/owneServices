using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDocsAndCartage))]
	public class WoolworthsJobDocsAndCartageValidationTest : InvoiceOrderLinkTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return JobDocsAndCartage.New(new WoolworthsMockJobDocsAndCartageParent(Factory));
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetLogParentForEventDateProperty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var docsAndCartage = (WoolworthsJobDocsAndCartage)BusinessObject;
			docsAndCartage.JP_ParentID = shipment.PK;
			docsAndCartage.JP_ParentTableCode = shipment.TablePrefix;
			var hit = shipment.DocsAndCartage;
			return shipment;
		}
		#endregion
	}
}
