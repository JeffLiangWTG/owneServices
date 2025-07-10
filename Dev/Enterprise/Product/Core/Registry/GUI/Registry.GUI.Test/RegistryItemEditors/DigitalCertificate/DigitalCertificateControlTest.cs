using System;
using System.IO;
using Enterprise.CryptoUtilities;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	class DigitalCertificateControlTest : NUnit.Framework.TestCase
	{
		public void TestSetFileData()
		{
			using (DigitalCertificateControl control = GetNewControl(null))
			{
				control.SetFileData(new byte[] { 1, 2 });
				AssertEquals("State", DataLoadState.DataExists, control.State);
				AssertEquals("FileDataAsBinary", new byte[] { 1, 2 }, control.FileDataAsBinary);

				control.SetFileData(Array.Empty<byte>());
				AssertEquals("State", DataLoadState.NoData, control.State);
				AssertEquals("FileDataAsBinary", Array.Empty<byte>(), control.FileDataAsBinary);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "WTG2008:Do not specify filesystem path separators in path string literals.", Justification = "WHY? this should work...")]
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowCertificateDetails()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			string certificatePath = Path.Combine(BaseSourcePath, @"Enterprise\Tools\StandAlone\CryptoUtilities\CryptoUtilities.Test\TestFiles\Conf.cer");
			byte[] certificateData = File.ReadAllBytes(certificatePath);
			string expectedMessage;

			using (Certificate certificate = new Certificate(certificateData))
			{
				DateTime validFromDate = certificate.ValidFromDate;
				DateTime validToDate = certificate.ValidToDate;

				expectedMessage = string.Format(
					"Issued To: Eagle Datamation SEDI\n" +
					"Issued By: Acme Trust CA\n\n" +
					"Valid From: {0}\n" +
					"Valid To: {1}\n\n" +
					"Email Address: cmr@acsedi.edi.net.au\n" +
					"Serial Number: 3e 8b 8f c2\n\n" +
					"Errors: The certificate has expired as of {1}.\n" +
					"Warnings: None",
					validFromDate, validToDate);
			}

			AssertEquals("Precondition: LastMessage.Text should be null.", null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (DigitalCertificateControl control = GetNewControl(new FileUpLoaderX509CertificateRegistryEditorInfo()))
			{
				control.Show();
				control.SetFileData(certificateData);
				control.ViewButton.PerformClick();
				AssertEquals("LastMessage.Text", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCertificateDetailsIfDataIsEmpty()
		{
			using (DigitalCertificateControl control = GetNewControl(new FileUpLoaderX509CertificateRegistryEditorInfo()))
			{
				AssertEquals("Precondition: FileDataAsBinary should be null.", null, control.FileDataAsBinary);

				control.Show();
				control.ViewButton.PerformClick();
				AssertEquals("LastMessage.Text", "There is currently no Certificate selected.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCertificateDetailsCatchesException()
		{
			using (DigitalCertificateControl control = GetNewControl(new DummyFileUpLoaderX509CertificateRegistryEditorInfo()))
			{
				control.Show();
				control.SetFileData(new byte[] { 1, 2 });
				control.ViewButton.PerformClick();
				AssertEquals("LastMessage.Text", "ERROR: GetCertificate() threw an exception!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected virtual DigitalCertificateControl GetNewControl(FileUpLoaderX509CertificateRegistryEditorInfo editorInfo)
		{
			return new DigitalCertificateControl(editorInfo);
		}

		#endregion
	}
}
