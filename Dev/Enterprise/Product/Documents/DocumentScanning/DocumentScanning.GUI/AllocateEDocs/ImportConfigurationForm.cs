using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportConfigurationForm : ZChildForm
	{
		public ImportConfigurationForm(FileImporter importer)
			: base(importer)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetAutoAllocateEnabled(this, EventArgs.Empty);
		}

		#region Form Configuration

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
			SetLabelText();
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				Importer.SaveRegistrySettings();
				Importer.HasChanges = false;
			}

			return result;
		}

		#endregion

		#region Importer

		protected FileImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = (FileImporter)BusinessEntity;
				}
				return fImporter;
			}
		}
		FileImporter fImporter;

		#endregion

		#region Actions

		void DirectoryBrowseButton_Click(object sender, System.EventArgs e)
		{
			ImportConfigDialog.RequireMappablePath = true;
			ImportConfigDialog.SelectedPath = Importer.DefaultImportDirectory;

			if (this.ImportConfigDialog.ShowDialog() == DialogResult.OK)
			{
				Importer.DefaultImportDirectory = ImportConfigDialog.MappedSelectedPath;
			}
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			Importer.SaveRegistrySettings();
		}

		void SetAutoAllocateEnabled(object sender, System.EventArgs e)
		{
			AutoAllocateCheckBox.Enabled = AutoRadioButton.Checked | AutoSingleRadioButton.Checked;
		}

		void UseCoverSheetCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SetLabelText();
		}

		void SetLabelText()
		{
			if (this.UseCoverSheetCheckBox.Checked)
			{
				this.AutoSingleRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|c32c72fa-aef1-4fb9-a0bd-1840fbf7c4d1", "Process as specified by Barcodes and create a new eDoc for each page");
				this.AutoRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|bef13241-67f8-4b6f-99d0-49ee6d67db36", "Process as specified by Barcodes");
			}
			else
			{
				this.AutoSingleRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|0A21BA80-28F9-498E-AD48-CB372D041DE3", "Process as specified by User or Barcodes, and create a new eDoc for each page");
				this.AutoRadioButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportConfigurationForm|226B79CE-C012-450F-8DA4-DE9A2AE79AD8", "Process as specified by User or Barcodes");
			}
			this.AutoSingleRadioButton.UpdateCaption();
			this.AutoRadioButton.UpdateCaption();
		}

		#endregion
	}
}
