using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.TrustedMessaging.Business.Testing.EdiTrustedMessagingConfigTest;

namespace Enterprise.Client.EDI.EndpointManagement.GUI.Testing
{
	[TestedType(typeof(EDITrustedMessagingConfigForm))]
	public class EDITrustedMessagingConfigFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var config = Factory.New<EdiTrustedMessagingConfig>();
			return new EDITrustedMessagingConfigForm(config);
		}

		public void TestGenerateCertificate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pwd");
			EDISecurityCheckpoints.EdiTrustedMessagingConfig.IsAllowed = true;
			var c1 = Factory.New<EdiTrustedMessagingConfigForTest>();

			using (var form = new EDITrustedMessagingConfigForm(c1))
			{
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var menu = actionsMenuItem.MenuItems.FindByText("Generate New Certificate from CA...");
				menu.PerformClick();
			}

			AssertEquals(false, c1.ETM_CertificateData.IsEmpty);
			AssertEquals(false, c1.ETM_CertificatePassword.IsEmpty);

			var cert = c1.GetCertificate();
			AssertEquals(true, cert.GetRSAPrivateKey() != null);
			AssertEquals(true, cert.GetRSAPublicKey() != null);
			AssertEquals("CN=subject1", cert.Subject);
		}

		public void TestExportCertificate()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pwd");
			EDISecurityCheckpoints.EdiTrustedMessagingConfig.IsAllowed = true;
			var c1 = Factory.New<EdiTrustedMessagingConfigForTest>();
			c1.ETM_CertificateData = EdiTrustedMessagingConfigTest.LoadLocalCertAsBytes("Server.pfx");

			using (var ms = new MemoryStream())
			{
				using (var form = new EDITrustedMessagingConfigFormForTest(c1, ms))
				{
					var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					var menu = actionsMenuItem.MenuItems.FindByText("Export Certificate (.cer)...");
					menu.PerformClick();
				}

				var cert = new X509Certificate2(ms.ToArray(), "", X509KeyStorageFlags.MachineKeySet);
				AssertNull(cert.GetRSAPrivateKey());
				AssertNotNull(cert.GetRSAPublicKey());
			}
		}

		class EDITrustedMessagingConfigFormForTest : EDITrustedMessagingConfigForm
		{
			public EDITrustedMessagingConfigFormForTest(EdiTrustedMessagingConfig config, Stream stream) : base(config)
			{
				InternalStream = stream;
			}

			protected override IFileDialog GetSaveFileDialog(string defaultFileName) => new FileDialog(InternalStream);

			readonly Stream InternalStream;

			class FileDialog : IFileDialog
			{
				public FileDialog(Stream stream)
				{
					InternalStream = stream;
				}

				public bool CheckFileExists { get; set; }

				public string UnmappedFileName => "";

				public string Filter { get; set; }
				public string FileName { get; set; }

				public System.Windows.Forms.FileDialog Dialog => null;

				public void Dispose()
				{
				}

				public Stream OpenFile() => InternalStream;

				public DialogResult ShowDialog(IWin32Window owner) => DialogResult.OK;

				readonly Stream InternalStream;
			}
		}
	}
}
