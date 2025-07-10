using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseAttachingForm : ZChildForm
	{
		public ImportLicenseAttachingForm(ImportLicenseAttachingObjectParent parent, JobDeclaration destinationDeclaration)
			: base(parent)
		{
			InitializeComponent();

			DestinationDeclaration = Argument.NotNull(destinationDeclaration, nameof(destinationDeclaration));

			LicensesGrid.DisableImportDataMenuItem = true;
			LicensesGrid.ShowMassUpdateMenuItem = false;
		}

		internal readonly JobDeclaration DestinationDeclaration;

		internal ImportLicenseAttachingObjectParent AttachingObjectParent => DataSource as ImportLicenseAttachingObjectParent;

		public static void ShowDialog(ImportLicenseAttachingObjectParent parent, JobDeclaration destinationDeclaration)
		{
			ZFormModaliser.ShowDialogAndDispose(new ImportLicenseAttachingForm(parent, destinationDeclaration));
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Buttons

		void OnAttachButton_Click(object sender, EventArgs e)
		{
			var importLicenseAttachingObject = AttachingObjectParent.ImportLicenses.Cast<ImportLicenseAttachingObject>().Where(x => x.ShouldAttach).ToList();
			if (importLicenseAttachingObject.Any())
			{
				BusinessEntity.RunPreSaveValidation();
				if (BusinessEntity.Notifications.HasErrors())
				{
					ShowErrorsDialog();
				}
				else
				{
					DestinationDeclaration.AttachImportLicense(importLicenseAttachingObject);
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(
					Res.GetString("0ca68f5a-60a8-4e41-9d55-d25da367b2f7", "Please select at least one License to attach."),
					Res.GetString("f5fc72e5-d14c-442f-ad82-3813008a19ff", "Cannot Attach"));
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
