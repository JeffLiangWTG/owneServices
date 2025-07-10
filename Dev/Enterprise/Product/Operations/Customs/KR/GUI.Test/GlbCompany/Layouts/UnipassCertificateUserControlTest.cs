using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class UnipassCertificateUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestBindingMembers()
		{
			AssertEquals("UserIDTextBox Binding", "CertificateForUnipass.GP_Name", control.FindSingle<ZTextBox>("UserIDTextBox").BindTo);
			AssertEquals("MailBoxTextBox Binding", "CertificateForUnipass.GP_MailBoxID", control.FindSingle<ZTextBox>("MailBoxTextBox").BindTo);
			AssertEquals("SenderIDTextBox Binding", "CertificateForUnipass.GP_UserID", control.FindSingle<ZTextBox>("SenderIDTextBox").BindTo);
			AssertEquals("CurrentDecryptedCertificatePassphrase Binding", "CertificateForUnipass.CurrentDecryptedCertificatePassphrase", control.FindSingle<ZTextBox>("CertificatePasswordTextBox").BindTo);
			AssertEquals("StatusTextBox Binding", "CertificateForUnipass.GP_PasswordStatus", control.FindSingle<ZTextBox>("StatusTextBox").BindTo);
			AssertEquals("StatusReasonTextBox Binding", "CertificateForUnipass.GP_StatusReason", control.FindSingle<ZTextBox>("StatusReasonTextBox").BindTo);
			AssertEquals("CertificateFileTextBox Binding", "CertificateForUnipass.CertificateStatus", control.FindSingle<ZTextBox>("CertificateFileTextBox").BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UnipassCertificateUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		UnipassCertificateUserControl control;
	}
}
