using System;
using System.Windows.Forms;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class BulkCancelCommissionLinesForm : ZForm, IButtonDeleteTextOverride, IButtonCancelTextOverride
	{
		#region Constructors

		[Obsolete("Use the constructor that takes a filter biz obj, this constructor is just for the designer", true)]
		public BulkCancelCommissionLinesForm()
		{
		}

		public BulkCancelCommissionLinesForm(BulkCancelCommissionLinesAction action)
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

		new BulkCancelCommissionLinesAction BusinessEntity
		{
			get { return (BulkCancelCommissionLinesAction)base.BusinessEntity; }
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
			if (BusinessEntity.CancelCommissionLineActionCollection.Count == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("2c72d67f-8254-4d5f-be55-1f55a6d05bb7", "No entity commissions have been selected."));
			}
			else if (BusinessEntity.RemoveErrorLines() == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("eee0bb44-113d-406b-9420-7bc78b3713e8", "All entity commissions can be canceled."));
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("bedeb3c0-defc-4c4b-9604-b503680caf00", "&Cancel"); }
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
				Res.GetString("9c69f5f2-1d66-4e27-b6a5-e840eb6e778f", "You are about to cancel these entity commissions. Do you want to proceed?"),
				Res.GetString("86c3172c-5ca7-4344-abde-49ac24b541fe", "Cancel Confirmation"),
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
