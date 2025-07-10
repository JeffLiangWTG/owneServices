using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class AlterPaymentApprovalForm : PaymentApprovalForm
	{
		public AlterPaymentApprovalForm()
			: base()
		{
		}

		public AlterPaymentApprovalForm(PaymentApprovalBase paymentApprovalBizO)
			: base(paymentApprovalBizO)
		{
			CloseButton.ReadOnly = false;
			Payment.AlterPaymentApprovalFormEvent(true);
			PaymentDetailButton.ReadOnly = true;
		}

		protected override void AdjustLocationOfProcessEPaymentButton()
		{
			var locationXForProcessEPaymentButton = CloseButton.Bounds.X - ProcessEPaymentButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
			ProcessEPaymentButton.Location = ControlDpiScalingHelper.NewScaledPoint(locationXForProcessEPaymentButton, PaymentDetailButton.Location.Y, true);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			HideSaveAsDraftButton();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			Payment.AlterPaymentApprovalFormEvent(false);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			return ContinueWithSave.No;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			base.ValidateAndSave();
			return ContinueWithSave.Yes;
		}

		protected override void SetupPostingButtons()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);

			PaymentDetailButton.Hide();
			PostWithoutMatchingButton.Hide();
			CloseButton.Text = Res.GetString("AlterPaymentApprovalForm|CD6365FC-864D-4402-AFF9-7CC308BD1463", "OK");
			OrganizationGuidFindBox.ReadOnly = true;
		}

		public override void ShowOtherUsersCurrentlyAccessingThisEntity()
		{
			// This form is only ever called from the MatchingForm.
			// This functionaltiy is used by the PaymentApproval Form and is not required by this form.
		}
	}
}
