using System;
using System.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideTransactionBranchAndDepartmentForm : OverrideInvoiceDetailsForm, IButtonPostTextOverride, IButtonApplyTextOverride, IButtonCloseTextOverride
	{
		public OverrideTransactionBranchAndDepartmentForm(TransactionWithOverriddenBranchAndDepartmentAdaptor bo)
			: base(bo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			DisplayMode = ODisplayMode.Browse;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get
			{
				return Res.GetString("a09aa10f-8cfa-4079-aa36-5964a2639967", "A&pply && Close");
			}
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get
			{
				return Res.GetString("42fad633-a8b8-4c23-9b1d-5adabf22d9b9", "A&pply");
			}
		}

		string IButtonCloseTextOverride.CloseButtonText
		{
			get
			{
				if (BusinessEntity.HasChanges)
				{
					return Res.GetString("ba8dd716-d4a7-4a57-807b-ce76f31e0d3e", "C&ancel");
				}
				else
				{
					return ZFormPostingButtonsStrategy.DefaultCloseButtonText;
				}
			}
		}

		protected override void SaveInternal()
		{
			((TransactionWithOverriddenBranchAndDepartmentAdaptor)BusinessEntity).ApplyOrCancelChanges(true);
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			base.ZForm_Closing(sender, e);
			if (BusinessEntity.HasChanges && !e.Cancel)
			{
				((TransactionWithOverriddenBranchAndDepartmentAdaptor)BusinessEntity).ApplyOrCancelChanges(false);
			}
		}
	}
}


