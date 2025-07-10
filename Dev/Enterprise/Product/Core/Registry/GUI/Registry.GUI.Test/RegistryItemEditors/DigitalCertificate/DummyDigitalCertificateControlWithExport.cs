using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DummyDigitalCertificateControlWithExport : DigitalCertificateControlWithExport
	{
		public DummyDigitalCertificateControlWithExport(FileUpLoaderX509CertificateRegistryEditorInfo editorInfo)
			: base(editorInfo)
		{
		}

		protected override DialogResult GetLoginDialogResultWithoutDispose(DeveloperLoginForm loginForm)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = LoginDialogResult;
			return base.GetLoginDialogResultWithoutDispose(loginForm);
		}

		protected override bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
			return base.IsValidPassword(loginForm);
		}

		protected override DialogResult GetSaveDialogResultWithoutDispose(ZSaveFileDialog fileDialog)
		{
			ZFormModaliser.FileNameToSelectInShowCommonDialog = SaveDialogFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = SaveDialogResult;
			return base.GetSaveDialogResultWithoutDispose(fileDialog);
		}

		protected override void SaveToDisk(Stream fileStream)
		{
			if (ExceptionToThrowOnSaveToDisk != null)
			{
				throw ExceptionToThrowOnSaveToDisk;
			}

			base.SaveToDisk(fileStream);
		}

		public DialogResult LoginDialogResult;
		public string LoginPassword;
		public string SaveDialogFileName;
		public DialogResult SaveDialogResult;
		public Exception ExceptionToThrowOnSaveToDisk;
	}
}
