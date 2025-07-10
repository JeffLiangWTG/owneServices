using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(StaffCredentialsUserControl))]
	sealed class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestSignatureControls()
		{
			using var form = (ZForm)GetFormToBash();
			form.Show();

			CombineAssertions(() =>
			{
				form.AssertContainsControl<ZDropEdit>("CertificateAuthorityDropEdit", x => x.WithBindTo("PasswordCollection.GP_CertificateAuthority"));
				form.AssertContainsControl<ZTextBox>("CertificateIdTextBox", x => x.WithBindTo("PasswordCollection.GP_UserID"));
				form.AssertContainsControl<ZTextBox>("PinCodeTextBox", x => x.WithBindTo("PasswordCollection.CurrentDecryptedPassword"));
			});
		}

		public void TestAddAndClearSignatureButtons()
		{
			using var form = (ZForm)GetFormToBash();
			form.Show();

			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;
			var addSignatureButton = form.FindSingle<ZButton>("AddSignatureButton");
			var clearSignatureButton = form.FindSingle<ZButton>("ClearSignatureButton");

			CombineAssertions("Initial Scenario", () =>
			{
				AssertEquals("HasChanges", false, staffWrapper.HasChanges);
				AssertEquals("SignaturePasswordCollection Count", 0, staffWrapper.PasswordCollection.Count);
				AssertEquals("AddSignatureButton Enabled", true, addSignatureButton.Enabled);
				AssertEquals("ClearSignatureButton Enabled", false, clearSignatureButton.Enabled);
			});

			addSignatureButton.PerformClick();
			CombineAssertions("When AddSignatureButton is clicked", () =>
			{
				AssertEquals("HasChanges", true, staffWrapper.HasChanges);
				AssertEquals("SignaturePasswordCollection Count", 1, staffWrapper.PasswordCollection.Count);
				AssertEquals("AddSignatureButton Enabled", false, addSignatureButton.Enabled);
				AssertEquals("ClearSignatureButton Enabled", true, clearSignatureButton.Enabled);
			});

			staffWrapper.HasChanges = false;
			clearSignatureButton.PerformClick();
			CombineAssertions("When ClearSignatureButton is clicked", () =>
			{
				AssertEquals("HasChanges", true, staffWrapper.HasChanges);
				AssertEquals("SignaturePasswordCollection Count", 0, staffWrapper.PasswordCollection.Count);
				AssertEquals("AddSignatureButton Enabled", true, addSignatureButton.Enabled);
				AssertEquals("ClearSignatureButton Enabled", false, clearSignatureButton.Enabled);
			});
		}
	}
}
