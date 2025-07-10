using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureControlForm : ZTemplateForm
	{
		public FeatureControlForm(FeatureControlHeader featureControlHeader)
			: base(featureControlHeader)
		{
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			if (this.BusinessEntity is FeatureControlHeader header)
			{
				SetControlAccess(header);
			}
		}

		protected override bool ShowAuditTab => true;
		protected override bool SupportsEDocs => true;
		protected override bool AllowNew => false;

		FeatureControlHeader ControlHeader => (FeatureControlHeader)BusinessEntity;

		void NewRuleButton_Click(object sender, System.EventArgs e)
		{
			if (ShouldEditRule())
			{
				var newRule = new BusinessObjectFactory().New<FeatureControlRule>();
				newRule.FCR_FCM_FeatureControl = ControlHeader.PK;
				ZFormModaliser.ShowDialogAndDispose(new FeatureControlRuleForm(newRule));
			}
		}

		void DeleteRuleButton_Click(object sender, System.EventArgs e)
		{
			if (RulesGrid.ListManager.Position > -1)
			{
				var rule = (FeatureControlRule)RulesGrid.ListManager.GetCurrent();
				if (!rule.CanDelete)
				{
					Globals.Message.ShowError(rule.ReasonForNotAbleToDelete);
					return;
				}

				if (Globals.Message.Show(Res.GetString("6451f728-a12e-4eb4-b62d-3015d469b6ee", "Are you sure you wish to delete this rule? This operation cannot be undone."), Res.GetString("3ad94c47-1be1-46b6-88f8-b5fb44034b14", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					rule.Delete();
				}
			}
		}

		void EditRuleButton_Click(object sender, System.EventArgs e) => EditFeatureControlRule();

		void RulesGrid_DoubleClick(object sender, System.EventArgs e) => EditFeatureControlRule();

		void EditFeatureControlRule()
		{
			if (RulesGrid.ListManager.Position > -1 && ShouldEditRule())
			{
				var rulePK = ((FeatureControlRule)RulesGrid.ListManager.GetCurrent()).PK;
				var rule = new BusinessObjectFactory().Load<FeatureControlRule>(rulePK);
				if (ControlHeader.ReadOnly)
				{
					rule.SetReadOnlyIncludingChildren(true);
				}
				var form = new FeatureControlRuleForm(rule);
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		bool ShouldEditRule()
		{
			var shouldEdit = true;
			if (ControlHeader.HasChanges)
			{
				var result = Globals.Message.Show(Res.GetString("b1cb4881-e1a4-47c6-bce5-e631c612b48e", "You must save this form first. Would you like to save now?"), Res.GetString("893dc991-8a7c-4af1-82c4-e891b451f657", "Cannot Edit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				shouldEdit = (result == DialogResult.Yes && FireSaveButton() == ContinueWithSave.Yes);
			}

			return shouldEdit;
		}

		void SetControlAccess(FeatureControlHeader header)
		{
			this.RulesGrid.ReadOnly = header.BaseReadOnly;
			this.NewRuleButton.ReadOnly = header.BaseReadOnly;
			this.EditRuleButton.ReadOnly = header.BaseReadOnly;
			this.DeleteRuleButton.ReadOnly = header.BaseReadOnly;
		}
	}
}
