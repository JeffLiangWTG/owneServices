using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseResponseMessageForm : BaseImportFileForm
	{
		public ImportLicenseResponseMessageForm()
		{
		}

		public ImportLicenseResponseMessageForm(IImportLicenseResponseObjectParent importLicenseResponseObjectParent)
		: base(importLicenseResponseObjectParent)
		{
			ImportLicenseResponseObjectParent.AddLog = SetProgressAndLog;
			ImportLicensesGrid.SetAvailability(ImportLicenseResponseObjectParent.ResponseHasDiagnosis, ImportLicenseResponseObject.Schema.Diagnosis);
			ImportLicensesGrid.SetAvailability(ImportLicenseResponseObjectParent.ResponseHasReferenceNumber, ImportLicenseResponseObject.Schema.ReferenceNumber);
			ImportLicensesGrid.SetAvailability(ImportLicenseResponseObjectParent.ResponseHasStatus, ImportLicenseResponseObject.Schema.Status);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		IImportLicenseResponseObjectParent ImportLicenseResponseObjectParent => (IImportLicenseResponseObjectParent)DataSource;

		public override string FormVerb => string.Empty;

		public override string FormCaption => ImportLicenseResponseObjectParent.HumanReadableName;

		protected override bool ShowSaveProgressOnImporting => false;

		#region Events On Click

		protected override void OnClickImportButton()
		{
			if (ImportLicenseResponseObjectParent.CreateDataFromXml())
			{
				HideProgressMediator();
				Globals.Message.Show(Res.GetString("a3639373-0c98-4766-aaf6-72e8761e1fa3", "The message(s) has been loaded."));
				Close();
			}
			else
			{
				HideProgressMediator();
				Globals.Message.Show(Res.GetString("3353a9f6-08d2-49c4-a2d4-7e25fc6102c4", "No message has been created. Please check the Log Details."));
			}
		}

		protected override void OnClickBrowseButton()
		{
			using (var fs = OpenFileDialog.OpenFile())
			{
				LogDetailsListBox.Items.Clear();
				ImportLicenseResponseObjectParent.LoadAndValidateXML(FileNameTextBox.Text, fs);
			}
		}

		#endregion
	}
}
