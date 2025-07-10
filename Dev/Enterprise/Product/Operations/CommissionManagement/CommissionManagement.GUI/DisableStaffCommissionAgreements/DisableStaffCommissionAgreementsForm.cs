using System;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class DisableStaffCommissionAgreementsForm : ZChildForm
	{
		#region Constructors

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DisableStaffCommissionAgreementsForm()
		{
		}

		public DisableStaffCommissionAgreementsForm(DisableStaffCommissionAgreementsAction action, DisableStaffCommissionAgreementsFormStrings strings)
			: base(action)
		{
			MessageLabel.CaptionResourceString = strings.Message;
			yesButton.CaptionResourceString = strings.Yes;
			yesAndApproveButton.CaptionResourceString = strings.YesAndApprove;
			noButton.CaptionResourceString = strings.No;
		}

		#endregion

		#region BusinessEntity

		new DisableStaffCommissionAgreementsAction BusinessEntity
		{
			get { return (DisableStaffCommissionAgreementsAction)base.BusinessEntity; }
		}

		#endregion

		#region Initalization

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Buttons

		#region YesButton

		public ZButton YesButton
		{
			get { return yesButton; }
		}

		void YesButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				return;
			}

			try
			{
				BusinessEntity.ExecuteButDontApprove();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				Close();
			}
		}

		#endregion

		#region YesAndApproveButton

		public ZButton YesAndApproveButton
		{
			get { return yesAndApproveButton; }
		}

		void YesAndApproveButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CommissionAgreementApproval.IsAllowed)
			{
				Env.Security.CommissionAgreementApproval.ShowError();
				return;
			}

			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				return;
			}

			var controller = new CommissionAgreementApproveProgressController();
			try
			{
				controller.Show(BusinessEntity.ExecuteAndApprove, this);
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				Close();
			}
		}

		#endregion

		#region NoButton

		public ZButton NoButton
		{
			get { return noButton; }
		}

		void NoButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#endregion
	}
}
