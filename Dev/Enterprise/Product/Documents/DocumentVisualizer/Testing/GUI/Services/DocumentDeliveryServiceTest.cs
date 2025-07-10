using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DocumentDeliveryServiceTest : TestCaseWithFactory
	{
		public void TestSelectCommunicationModeViaMessageSettingsForm()
		{
			var broker = Factory.New<OrgHeader>();

			var mode1 = broker.EDICommunicationsModes.AddNew();
			mode1.EK_Module = "SHP";
			mode1.EK_CommsDirection = "TRX";
			mode1.EK_FileFormat = "FXL";
			mode1.EK_CommunicationsTransport = "HUB";
			mode1.EK_Destination = "ZZZ";

			var mode2 = broker.EDICommunicationsModes.AddNew();
			mode2.EK_Module = "SHP";
			mode2.EK_CommsDirection = "TRX";
			mode2.EK_FileFormat = "FXL";
			mode2.EK_MessagePurpose = "EVT";
			mode2.EK_CommunicationsTransport = "HUB";
			mode2.EK_Destination = "ZZZ";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_OH_ExportBroker = broker.PK;

			var deliveryService = new DocumentDeliveryService();

			var notifications = new DummyNotifications();

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var settingsForm = f as MessageSettingsForm;

				AssertNotNull("Settings Form is Showing", settingsForm);

				settingsForm.Shown += (s, e) =>
				{
					var settings = settingsForm.DataSource as MessageSettingsExport;
					settings.RecipientType = "BRO";
					settings.PurposeCode = "APP";

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var sendButton = settingsForm.Controls["sendButton"] as ZButton;
					sendButton.PerformClick();
				};
			});

			var result = deliveryService.GetCommunicationSettings((IBusiness)shipment, notifications);

			AssertNotNull("Received result", result);

			AssertContainsExactElementsInAnyOrder("",
				new[] { mode1 },
				result.CommunicationsModes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ZFormModaliser.ShowDialogsInTest = true;
		}

		protected override void TearDown()
		{
			base.TearDown();

			ZFormModaliser.ShowDialogsInTest = false;
		}
	}
}