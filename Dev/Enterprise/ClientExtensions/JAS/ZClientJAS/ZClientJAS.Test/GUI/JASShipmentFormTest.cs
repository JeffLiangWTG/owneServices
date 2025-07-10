using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	internal class JASShipmentFormTest : TestCaseWithFactory
	{
		public void TestOnJXCMenuItemClick()
		{
			MenuItem jXCMenuItem = ShipmentForm.ActionsMenuItem.MenuItems.FindByText("Export JXC Pre-Shipment Message");
			AssertNotNull("JXC Export menu item should exist", jXCMenuItem);
			jXCMenuItem.PerformClick();
			AssertEquals("You must save before you can export data to JXC file", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			Shipment.Factory.Save();
			jXCMenuItem.PerformClick();
			AssertEquals("Could not export JXC message from this Shipment", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.Consols.AddNew();
			Shipment.Consols[0].FillWithValidTestData();
			Shipment.Factory.Save();
			jXCMenuItem.PerformClick();
			AssertEquals("Shipment is already attached to a consol (not a pre-shipment). Please create message from the consol instead", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			Shipment.Consols.RemoveAndDeleteAll();
			Shipment.Factory.Save();
			jXCMenuItem.PerformClick();
			AssertEquals(typeof(PreShipmentExporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		#region Implementation
		protected override void TearDown()
		{
			ShipmentForm.Dispose();
			base.TearDown();
		}

		JASShipmentFormForTest ShipmentForm
		{
			get
			{
				if (fShipmentForm == null)
				{
					ChildEditableService.SetState(Shipment.Factory, ChildEditableServiceStates.Shipment);
					fShipmentForm = new JASShipmentFormForTest(Shipment);
				}

				return fShipmentForm;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		JASShipmentFormForTest fShipmentForm;
		JASForwardingShipment fShipment;
		#region class JASShipmentFormForTest
		class JASShipmentFormForTest : JASShipmentForm
		{
			public JASShipmentFormForTest(JASForwardingShipment shipment) : base(shipment)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get
				{
					return base.ActionsMenuItem;
				}
			}
		}
		#endregion
		#endregion
	}
}
