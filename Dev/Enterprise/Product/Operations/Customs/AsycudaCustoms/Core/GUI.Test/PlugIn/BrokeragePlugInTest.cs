using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class BrokeragePlugInTestCase : TransactionedTestCase
	{
		public void TestClearHasChanges()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_OverrideFreightDefaults = false;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			shipment.JS_HouseBill = "HBL1";
			shipment.JS_INCO = "FOB";

			factory.Save(); //to clear HasChanges for all children

			using (new BrokeragePlugIn(shipment))
			{
				AssertEquals(false, declaration.HasChanges);
			}
		}
	}

	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(CustomsBrokerageUserControl), plugin.UserControl.GetType());
			}
		}

		public override void TestClearHasChanges()
		{
			Assert("Moved to BrokeragePlugInTestCase.TestClearHasChanges", true);
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
	}
}
