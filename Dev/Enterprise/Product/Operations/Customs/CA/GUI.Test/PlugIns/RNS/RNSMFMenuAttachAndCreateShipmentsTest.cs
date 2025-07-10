using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSMFMenuAttachAndCreateShipmentsTest : BaseLoadListForwardManifestSupportTest
	{
		public void TestForwardedManifestsCountIndiction()
		{
			var rnsMFMenu = new RNSMFMenuForTesting(new RNSPlugInSupportLoadListWrapper(loadList));
			var menuItem = rnsMFMenu.MenuItems.FindByText("Attach/Create Shipments from Forwarded Manifests");

			menuItem.PerformClick();

			AssertEquals("Matching Forwarded Manifests count prompt",
				string.Format(@"Matching Forwarded Manifests founded:
    1 new shipment(s) will be created.
    1 new House Bill Manifest(s) will be linked to existing shipments.
Do you want to continue?"),
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestForwardedManifestsShowResult()
		{
			var rnsMFMenu = new RNSMFMenuForTesting(new RNSPlugInSupportLoadListWrapper(loadList));
			var menuItem = rnsMFMenu.MenuItems.FindByText("Attach/Create Shipments from Forwarded Manifests");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menuItem.PerformClick();

			ZStringBuilder expectedTextBuilder = new ZStringBuilder();

			expectedTextBuilder.Append(string.Format("{0} was created and linked to Forwarded Manifest (CCN: 10207000067891).", loadList.Shipments[loadList.Shipments.Count - 1].HumanReadableName));
			expectedTextBuilder.Append(string.Format("{0} was linked to Forwarded Manifest (CCN: 10207000067892).", shipment2.HumanReadableName, message2.CargoControlNumber));
			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067894), \r\n    but it is not a shipment that is attached to this Load List.", shipment4.HumanReadableName));
			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067895), \r\n    but the shipment house CCN is 10207000067896", shipment5.HumanReadableName));

			AssertEquals("Attach/Create Shipments from Forwarded Manifests result", expectedTextBuilder.ToStringWithNewLineBetweenAppends(), UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
