using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHMasterUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsGroupBoxCaption()
		{
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				var overrideCheckBox = userControl.FindSingle<ZCheckBox>("OverrideFreightDefaultsCheckBox");
				AssertEquals("OverrideFreightDefaultsCheckBox Caption", "Override Default Values from Consol", overrideCheckBox.CaptionResourceString.Caption);

				var detailsGroupBox = userControl.FindSingle<ZGroupBox>("DetailsGroupBox");
				AssertEquals("DetailsGroupBox Caption", "eManifest Details", detailsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestOverrideFreightDefaultsCheckBox()
		{
			var consol = Factory.New<ForwardingConsol>();
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			master.BP_OverrideFreightDefaults = true;

			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var overrideCheckBox = userControl.FindSingle<ZCheckBox>("OverrideFreightDefaultsCheckBox");
				AssertNotNull(overrideCheckBox);
				Assert(overrideCheckBox.Visible);

				var detailsGroupBox = userControl.FindSingle<ZGroupBox>("DetailsGroupBox");
				AssertNotNull(detailsGroupBox);
				Assert(detailsGroupBox.Visible);

				master.BP_OverrideFreightDefaults = false;
				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Removing the override will reset your eManifest data.\r\nYou will lose changes that you have made to the eManifest data.\r\n\r\nProceed?", lastMessage);
			}

			master = Factory.New<CusCAeMHMaster>();

			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var overrideCheckBox = userControl.FindSingle<ZCheckBox>("OverrideFreightDefaultsCheckBox");
				AssertNotNull(overrideCheckBox);
				Assert("Override checkbox invisible for standalone eManifest", !overrideCheckBox.Visible);
			}
		}
	}
}
