using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SystemToSystemCertificateControl))]
	public class SystemToSystemCertificateControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SystemToSystemTrustInfo();
		}

		public void TestRegistryValueDisplayCorrectly_DefaultValue()
		{
			using (var form = new ZForm())
			using (var control = new SystemToSystemCertificateControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlsReadOnly(control);

					AssertEmptyValue(control);
				});
			}
		}

		public void TestRegistryValueDisplayCorrectly_OverrideValue()
		{
			using (var form = new ZForm())
			using (var control = new SystemToSystemCertificateControlForTest())
			{
				var trustInfo = new SystemToSystemTrustInfo
				{
					TenantId = "35DEBE15-9104-48A6-95C5-EF91E1093233",
					ClientId = "3132BE50-1883-49C6-8B15-EAE67B748DC4",
					Certificate = Encoding.UTF8.GetBytes(Certificate)
				};
				control.SetDataBinding(trustInfo, "");
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlsReadOnly(control);

					var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
					AssertRegistryValue(control, certificate);
				});
			}
		}

		public void TestEnableResetButton_UserDoesNotHaveAccess()
		{
			TestEnableResetButton(false);
		}

		public void TestEnableResetButton_UserHasAccess()
		{
			TestEnableResetButton(true);
		}

		void TestEnableResetButton(bool userHasAccess)
		{
			var setLocatorMock = new Mock<IRegistryItemSetLocator>();
			setLocatorMock.Setup(x => x.GetAllRegistryItems())
				.Returns(new[] { SystemDataRegistry.Instance.SystemToSystemCertificate }); // Mock only 1 registry item for 'system to system certificate'

			var checkpoint = Env.Security.GetRegistryCheckPoint(SystemDataRegistry.Instance.SystemToSystemCertificate.Name, SystemDataRegistry.Instance.SystemToSystemCertificate.Caption);
			checkpoint.IsAllowed = userHasAccess;

			using (ObjectFactory.Substitute(setLocatorMock.Object))
			using (var registryForm = new RegistryFormTesting())
			{
				var registriesTreeView = registryForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				registryForm.Show();

				// Initiating 'system to system certificate' registry item node
				var testNode = new TreeNode();
				var regItem = SystemDataRegistry.Instance.SystemToSystemCertificate;
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;

				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode; // Simulating selecting 'system to system certificate' registry item

				var pluginControl = registryForm.GetPluginControl() as SystemToSystemCertificateControl;
				AssertNotNull(nameof(pluginControl), pluginControl);

				var resetButton = pluginControl.Controls.Find("resetButton", true).Single();
				AssertNotNull(nameof(resetButton), resetButton);
				AssertEquals(userHasAccess, resetButton.Enabled);
			}
		}

		public void TestResetRegistry()
		{
			var trustInfo = new SystemToSystemTrustInfo
			{
				TenantId = "35DEBE15-9104-48A6-95C5-EF91E1093233",
				ClientId = "3132BE50-1883-49C6-8B15-EAE67B748DC4",
				Certificate = Encoding.UTF8.GetBytes(Certificate)
			};
			using (var form = new ZForm())
			using (var control = new SystemToSystemCertificateControlForTest())
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, trustInfo))
			{
				control.SetDataBinding(trustInfo, "");
				form.Controls.Add(control);
				form.Show();

				var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				control.ResetButton.PerformClick();

				CombineAssertions(() =>
				{
					var registry = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
					AssertEquals("35DEBE15-9104-48A6-95C5-EF91E1093233", registry.TenantId);
					AssertEquals("3132BE50-1883-49C6-8B15-EAE67B748DC4", registry.ClientId);
					AssertEquals(Encoding.UTF8.GetBytes(Certificate), registry.CertificateBytes);

					AssertRegistryValue(control, certificate);
				});

				UnitTestUserNotification.Instance.AddOKAnswer();
				control.ResetButton.PerformClick();

				CombineAssertions(() =>
				{
					var registry = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
					AssertEquals(string.Empty, registry.TenantId);
					AssertEquals(string.Empty, registry.ClientId);
					AssertEquals(0, registry.CertificateBytes.Length);

					AssertEmptyValue(control);
				});
			}
		}

		void AssertControlsReadOnly(SystemToSystemCertificateControlForTest control)
		{
			Assert(control.TenantIDTextBox.ReadOnly);
			Assert(control.ClientIDTextBox.ReadOnly);
			Assert(control.ThumbprintTextBox.ReadOnly);
			Assert(control.ValidFromDateEdit.ReadOnly);
			Assert(control.ValidToDateEdit.ReadOnly);
		}

		void AssertEmptyValue(SystemToSystemCertificateControlForTest control)
		{
			AssertEquals(string.Empty, control.TenantIDTextBox.Text);
			AssertEquals(string.Empty, control.ClientIDTextBox.Text);
			AssertEquals(string.Empty, control.ThumbprintTextBox.Text);
			AssertEquals(string.Empty, control.ValidFromDateEdit.Text);
			AssertEquals(string.Empty, control.ValidToDateEdit.Text);
		}

		void AssertRegistryValue(SystemToSystemCertificateControlForTest control, X509Certificate2 certificate)
		{
			AssertEquals("tenant", "35DEBE15-9104-48A6-95C5-EF91E1093233", control.TenantIDTextBox.Text);
			AssertEquals("client", "3132BE50-1883-49C6-8B15-EAE67B748DC4", control.ClientIDTextBox.Text);
			AssertEquals("thumbprint", "0153A7C4B112DE00D8E50756C5249D5789C64B03", control.ThumbprintTextBox.Text);
			AssertEquals("valid from", certificate.NotBefore.ToString(DateTimeFormatStrings.LongTimeFormat.ToString()).ToUpper(), control.ValidFromDateEdit.Text);
			AssertEquals("valid to", certificate.NotAfter.ToString(DateTimeFormatStrings.LongTimeFormat.ToString()).ToUpper(), control.ValidToDateEdit.Text);
		}

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";
	}

	class SystemToSystemCertificateControlForTest : SystemToSystemCertificateControl
	{
		public SystemToSystemCertificateControlForTest() : base()
		{
		}

		public TextBox TenantIDTextBox => zTextBox1;
		public TextBox ClientIDTextBox => zTextBox2;
		public TextBox ThumbprintTextBox => zTextBox3;
		public ZDateEdit ValidFromDateEdit => validFromDateTimeEdit;
		public ZDateEdit ValidToDateEdit => validToDateTimeEdit;
		public ZButton ResetButton => resetButton;
	}

	class RegistryFormTesting : RegistryForm
	{
		public RegistryFormTesting()
			: base(ObjectFactory.Get<IRegistryProvider>())
		{
		}

		public RegistryItemsTreeView GetRegistriesTreeView()
		{
			return RegistriesTreeView;
		}

		public Control GetPluginControl()
		{
			return PluginControl;
		}
	}
}
