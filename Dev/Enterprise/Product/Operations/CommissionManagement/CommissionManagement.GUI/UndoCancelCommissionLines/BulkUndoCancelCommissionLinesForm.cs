using System;
using System.Windows.Forms;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class BulkUndoCancelCommissionLinesForm : ZForm, IButtonDeleteTextOverride, IButtonCancelTextOverride
	{
		#region Constructors

		public BulkUndoCancelCommissionLinesForm(BulkUndoCancelCommissionLinesAction action)
			: base(action)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
			}
		}

		#endregion

		#region Initialize

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region BusinessEntity

		public new BulkUndoCancelCommissionLinesAction BusinessEntity
		{
			get { return (BulkUndoCancelCommissionLinesAction)base.BusinessEntity; }
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisplayMode = ODisplayMode.Delete;

			BusinessEntity.RunPreSaveValidation();
		}

		#region Buttons

		void RemoveErrorLinesButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.UndoCancelCommissionLineActionCollection.Count == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("2c72d67f-8254-4d5f-be55-1f55a6d05bb7", "No entity commissions have been selected."));
			}
			else if (BusinessEntity.RemoveErrorLines() == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("652aad6b-a647-447f-b687-f508164e7082", "All entity commissions can be undo canceled."));
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("b4e590fa-cf4b-44cf-9520-7559d78da00e", "&Undo Cancel"); }
		}

		string IButtonCancelTextOverride.CancelButtonText
		{
			get { return Res.GetString("3599dbb8-4588-4949-8597-617b9e011e3b", "&Close"); }
		}

		#endregion

		#region Delete

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				return ContinueWithDelete.No;
			}

			return base.ShowPreDeleteDialogs();
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return Globals.Message.Show(
				Res.GetString("8ec26f57-01cc-4338-bc3c-8dd66fe73388", "You are about to undo cancel these entity commissions. Do you want to proceed?"),
				Res.GetString("2c8011c0-50c4-4d40-b1f5-aaea074a586d", "Undo Cancel Confirmation"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.No);
		}

		protected override void DeleteCore()
		{
			BusinessEntity.Execute();
		}

		#endregion
	}
}
