using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.GUI
{
	[SuppressBindingMemberBashingTest] // for ApplyToAllCheckBox
	public partial class EditPropertiesFileForm : ZChildForm, IEditResponse
	{
		public event DocumentEventHandler CloseOfForm;

		public EditPropertiesFileForm(StorageDocsBase eDoc)
			: base(eDoc)
		{
			InitializeComponent();

			PreviewButton.Visible = eDoc.IsImageFile;
			SaveVersionsCheckBox.Visible = eDoc.SC_IsSystemGenerated;
			eDoc.SetDefaultSC_FileNameForRenaming();

			OldFilename = eDoc.SC_FileNameForRenaming;
			OldDocumentType = eDoc.SC_DocType;
			OldDescription = eDoc.SC_DescMultilingual.GetUnresolvedString();
			OldIsPublished = eDoc.SC_IsPublished;
			OldIsParsingEnabled = eDoc.IsParsingEnabledValue;
			OldCompany = eDoc.SC_GC_Company;
			OldBranch = eDoc.SC_GB_Branch;
			OldDepartment = eDoc.SC_GE_Department;
			OldDocSource = eDoc.SC_RDS_NKDocSource;
		}

		public override string FormHeading => Res.GetString("33b48d6e-8c79-4ca2-98aa-87c06cd63df0", "{0} for {1}", base.FormCaption, EDoc.SC_FileNameWithExtension);

		StorageDocsBase EDoc => (StorageDocsBase)BusinessEntity;

		void CancelButton_Click(object sender, EventArgs e)
		{
			EDoc.SetNewFileName(OldFilename);
			EDoc.SC_DocType = OldDocumentType;
			EDoc.SC_Desc = OldDescription;
			EDoc.SC_RDS_NKDocSource = OldDocSource;
			EDoc.SC_IsPublished = OldIsPublished;
			EDoc.SC_GC_Company = OldCompany;
			EDoc.SC_GB_Branch = OldBranch;
			EDoc.SC_GE_Department = OldDepartment;
			EDoc.IsParsingEnabled = OldIsParsingEnabled;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			EDoc.Validation.ValidateSC_FileNameForRenaming();
			EDoc.Validation.ValidateSC_DocType();
			EDoc.Validation.ValidateSC_Desc();

			if (EDoc is StorageFile storageFile)
			{
				string filename = StorageFile.GetFileNameOnlyFromFilename(EDoc.SC_FileNameForRenaming);
				string extension = StorageFile.GetExtensionFromFilename(EDoc.SC_FileNameForRenaming);

				if (extension == EDoc.SC_DataType || EDoc.SC_DataType.IsEmpty ||
					DialogResult.OK == Globals.Message.Show(Res.GetString("5c3745c5-2d54-4d1f-8800-917434303a81", "Renaming the file extension from '{0}' can make the file unusable. Are you sure you want to continue?", EDoc.SC_DataType), Res.GetString("37a8ab94-52d9-4379-a94b-9f307e09ed91", "Are you sure you want to rename?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation))
				{
					if (!EDoc.SC_FileNameForRenamingInfo.HasErrors())
					{
						storageFile.SetNewFileName(filename, extension);
					}
				}
			}
			else
			{
				EDoc.SC_FileName = EDoc.SC_FileNameForRenaming.Truncate(EDoc.SC_FileNameInfo.MaxLength);
			}

			if (EDoc.HasErrors)
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
			else
			{
				if (EDoc.IsParsingEnabled != OldIsParsingEnabled)
				{
					EDoc.HasChanges = true;
				}

				DialogResult = DialogResult.OK;
				Close();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (DialogResult == DialogResult.None)
			{
				DialogResult = DialogResult.Cancel;
			}

			if (CloseOfForm != null)
			{
				var args = new DocumentEventArgs(EDoc);
				CloseOfForm(this, args);
			}

			if (EDoc != null && DialogResult == DialogResult.OK && EDoc.SC_IsPublished)
			{
				var regEx = new Regex(@"\[\d+\]$");
				var fileNameWithoutSuffix = regEx.Replace(Path.GetFileNameWithoutExtension(EDoc.SC_FileNameForRenaming), "");
				EDoc.ParentMain.SupersedeOlderVersionDocs(EDoc, $"{fileNameWithoutSuffix}.{EDoc.SC_DataType}");
			}
		}

		internal ZController RefDocTypeController
		{
			get
			{
				if (fRefDocTypeController == null)
				{
					fRefDocTypeController = ZControllerFactory.Create(ControllerIDs.RefDocType);
				}
				return fRefDocTypeController;
			}
		}

		ZController fRefDocTypeController;

		ZController RefDocSourceController => fRefDocSourceController ??= ZControllerFactory.Create(ControllerIDs.RefDocSource);

		ZController fRefDocSourceController;

		public bool ApplyToAll => ApplyToAllCheckBox.Checked;

		void CreateNewDocumentTypeButton_Click(object sender, EventArgs e)
		{
			RefDocTypeController.SetFormsModalTo(this);
			RefDocTypeController.ShowNewForm();
			if (RefDocTypeController.LastShownForm != null)
			{
				ZForm lastShownForm = ((ZForm)RefDocTypeController.LastShownForm);
				RefDocType newDocType = (RefDocType)lastShownForm.BusinessEntity;
				if (newDocType != null)
				{
					string proposedCode = ((StorageDocsBase)BusinessEntity).ParentMain.OwnerAssemblyData.ReferenceType;
					if (newDocType.RT_ReferenceType_List.ContainsCode(proposedCode))
					{
						newDocType.RT_ReferenceType = proposedCode;
					}

					lastShownForm.Closed += new EventHandler(NewDocTypeForm_Closed);
					lastShownForm.BringToFront();
				}
			}
		}

		void NewDocTypeForm_Closed(object sender, EventArgs e)
		{
			ZForm form = (ZForm)sender;
			RefDocType docTypeAdded = (RefDocType)form.BusinessEntity;

			if (docTypeAdded.IsInDatabase) // actually got saved
			{
				EDoc.AddDocType(docTypeAdded);
				EDoc.SC_DocType = docTypeAdded.RT_DocType;
			}
		}

		void CreateNewDocumentSourceButton_Click(object sender, EventArgs e)
		{
			RefDocSourceController.SetFormsModalTo(this);
			RefDocSourceController.ShowNewForm();
			if (RefDocSourceController.LastShownForm != null)
			{
				ZForm lastShownForm = ((ZForm)RefDocSourceController.LastShownForm);
				RefDocSource newDocSource = (RefDocSource)lastShownForm.BusinessEntity;
				if (newDocSource != null)
				{
					lastShownForm.Closed += new EventHandler(NewDocSourceForm_Closed);
					lastShownForm.BringToFront();
				}
			}
		}

		void NewDocSourceForm_Closed(object sender, EventArgs e)
		{
			ZForm form = (ZForm)sender;
			RefDocSource docSourceAdded = (RefDocSource)form.BusinessEntity;

			if (docSourceAdded.IsInDatabase) // actually got saved
			{
				EDoc.AddDocSource(docSourceAdded);
				EDoc.SC_RDS_NKDocSource = docSourceAdded.RDS_Code;
			}
		}

		void PreviewButton_Click(object sender, EventArgs e)
		{
			if (EDoc.IsImageFile)
			{
				using (var displayForm = new GraphicalDisplayForm(true))
				{
					string filename = EDoc.SaveToTempFile();
					try
					{
						var manager = new ImageManager(filename, EDoc, displayForm);
						manager.Tag = EDoc.PK;
						ZFormModaliser.ShowDialogWithoutDispose(displayForm);
						manager.Close();
						manager.Dispose();
					}
					finally
					{
						if (File.Exists(filename))
						{
							File.Delete(filename);
						}
					}
				}
			}
		}

		readonly string OldFilename;
		readonly string OldDocumentType;
		readonly string OldDescription;
		readonly bool OldIsPublished;
		readonly bool OldIsParsingEnabled;
		readonly ZGuid OldCompany;
		readonly ZGuid OldBranch;
		readonly ZGuid OldDepartment;
		readonly string OldDocSource;
	}
}
