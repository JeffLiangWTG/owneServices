using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IManualReleaseSupport = Enterprise.Integration.Customs.CA.IManualReleaseSupport;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ManualReleaseForm : ZChildForm
	{
		public ManualReleaseForm()
		{
		}

		public ManualReleaseForm(IManualReleaseSupport support, BusinessObjectFactory factory)
			: base(new ManualReleaseCancelBO(support.ManualReleaseNoteText, factory))
		{
			this.support = Argument.NotNull(support, "support");
			this.DeleteButton.Enabled = !BusinessEntity.ManualReleaseDate.IsEmpty;
		}

		readonly IManualReleaseSupport support;

		public new ManualReleaseCancelBO BusinessEntity
		{
			get { return base.BusinessEntity as ManualReleaseCancelBO; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("1DF94287-9BD2-403A-8B4E-425B8EA5C24E", "Manual Release"); }
		}

		protected void DeleteButton_Click(object sender, System.EventArgs e)
		{
			if (IsValidateSupportBeforSaving())
			{
				if (Globals.Message.Show(Res.GetString("52F2AD86-FAC1-4BB7-B46F-C643C4F07869", "Do you want to remove manual release data?"), Res.GetString("F416AC3A-228B-4457-A373-A7554685B687", "Confirm"),
					 MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
				{
					BusinessEntity.Delete();
					if (ValidateAndSave() == ContinueWithSave.Yes)
					{
						DialogResult = System.Windows.Forms.DialogResult.OK;
						Close();
					}
				}
			}
			else
			{
				ShowErrorAndClose();
			}
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			if (IsValidateSupportBeforSaving())
			{
				if (ValidateAndSave() == ContinueWithSave.Yes)
				{
					Close();
				}
			}
			else
			{
				ShowErrorAndClose();
			}
		}

		protected override void SaveInternal()
		{
			if (BusinessEntity.IsDeleted)
			{
				support.DeleteManualRelease();
			}
			else
			{
				support.ManualRelease(BusinessEntity);
			}
			base.SaveInternal();
		}

		bool IsValidateSupportBeforSaving()
		{
			var bo = support as BusinessObject;
			if (bo != null)
			{
				bo.RunPreSaveValidationWithFetchHints();
			}
			return bo == null || !bo.HasErrors;
		}

		void ShowErrorAndClose()
		{
			Globals.Message.ShowError(Res.GetString("0AEF8ED5-C2FC-4344-9B54-ED6623A1B3B7", "Please clear all errors before saving."));
			Close();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = ContinueWithSave.No;
			try
			{
				result = base.ValidateAndSave();
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveException(ex);
			}
			return result;
		}
	}
}
