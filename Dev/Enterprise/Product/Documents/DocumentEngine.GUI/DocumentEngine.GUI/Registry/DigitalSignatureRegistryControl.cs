using System;
using System.ComponentModel;
using System.IO;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.Registry
{
	public partial class DigitalSignatureRegistryControl : RegistryZUserControl
	{
		public DigitalSignatureRegistryControl()
		{
			InitializeComponent();
			fileDialog.Title = Res.GetString("9b104bfe-8f99-4448-b825-b282351202ad", "Choose Certificate File");
			fileDialog.Filter = (NoResString)"Personal Information Exchange (*.pfx)|*.pfx";
			fileDialog.CheckPathExists = true;
		}

		public new DigitalSignatureRegistry CurrentDataItem => (DigitalSignatureRegistry)base.CurrentDataItem;

		void HandleChooseButtonClick(object sender, EventArgs e)
		{
			fileDialog.ShowDialog();
		}

		void FileDialog_FileOk(object sender, CancelEventArgs e)
		{
			var signatureName = fileDialog.UnmappedFileName;
			if (!Path.GetExtension(signatureName).Equals(".pfx", StringComparison.OrdinalIgnoreCase))
			{
				Globals.Message.ShowError(Res.GetString("12c5b9f5-6153-4a93-9ee4-9353344da153", @"The certificate '{0}' is not in a valid format.", signatureName));
			}
			else
			{
				using (var file = fileDialog.OpenFile())
				using (var memoryStream = new MemoryStream())
				{
					file.CopyTo(memoryStream);
					CurrentDataItem.DigitalSignature = memoryStream.ToArray();
				}

				certificateNameTextBox.Text = Path.GetFileName(signatureName);
				certificateNameTextBox.Focus();
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			chooseButton.ReadOnly = readOnly;
			certificatePasswordTextBox.ReadOnly = readOnly;
			signatureDetailsLocation.ReadOnly = readOnly;
			signatureDetailsName.ReadOnly = readOnly;
			signatureDetailsReason.ReadOnly = readOnly;
		}

		void signatureDetailsName_Leave(object sender, System.EventArgs e)
		{
			CurrentDataItem.RunPreSaveValidation();
		}
	}
}
