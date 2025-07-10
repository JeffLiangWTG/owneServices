using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IManualCancelSupport = Enterprise.Integration.Customs.CA.IManualCancelSupport;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ManualCancelForm : ZChildForm
	{
		public ManualCancelForm()
		{
		}

		public ManualCancelForm(IManualCancelSupport support, BusinessObjectFactory factory)
			: base(new ManualReleaseCancelBO(support.ManualCancelNoteText, factory))
		{
			this.support = Argument.NotNull(support, "support");
		}

		readonly IManualCancelSupport support;

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
			get { return Res.GetString("D8033F53-E245-4446-A9D3-DAF232A6F0FD", "Manual Cancel"); }
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			if (IsValidateSupportBeforSaving())
			{
				if (ValidateAndSave() == ContinueWithSave.Yes)
				{
					DialogResult = System.Windows.Forms.DialogResult.OK;
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
			support.ManualCancel(BusinessEntity);
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
			Globals.Message.ShowError(Res.GetString("A829F1D7-93D1-40E3-A609-EDA03B5A83AC", "Please clear all errors before saving."));
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
