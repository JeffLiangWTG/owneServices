using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AddLogCommentPopupForm : ZChildForm
	{
		public AddLogCommentPopupForm(AccQueryClaimBase accQueryClaim) : this(new AccQueryClaimLogAdder(accQueryClaim))
		{
		}

		public AddLogCommentPopupForm(AccQueryClaimLogAdder businessEntity)
			: base(businessEntity)
		{
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public new AccQueryClaimLogAdder BusinessEntity
		{
			get { return (AccQueryClaimLogAdder)base.BusinessEntity; }
		}

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (!BusinessEntity.HasErrors)
			{
				BusinessEntity.AddLogToParent();
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		#endregion
	}
}

