using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class DepositRefundApplicationSendingForm : MessageSendingFormWithValidationDetails
	{
		public DepositRefundApplicationSendingForm(DepositRefundApplicationMessageSendingActionParent parent) : base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			messageSendingObjectsGroupBox.CaptionResourceString = Res.GetData("A00E4CBF-F894-423F-8BFD-C63838958546", "Deposit Refund Application");

			ValidationErrorsGroupBox.Visible = false;
			SplitContainer.Panel2Collapsed = true;
		}

		public override string FormHeading => Res.GetString("A4915423-3A09-4120-81A3-62EF94BEA553", "Deposit Refund Application");

		protected override void AddUserControlToBottomSection() { }

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = new DepositRefundApplicationMessageSendingGridColumnLayout());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;
	}
}
