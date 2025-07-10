using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class RefundApplicationSendingForm : MessageSendingFormWithValidationDetails
	{
		public RefundApplicationSendingForm(RefundApplicationMessageSendingActionParent parent) : base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			messageSendingObjectsGroupBox.CaptionResourceString = Res.GetData("367B3287-0B01-4F62-AEC6-C87DD098711A", "Refund Applications");
			ValidationErrorsGroupBox.Visible = false;
		}

		protected override void AddUserControlToBottomSection() { }

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		protected override bool PreviewMessageCheckboxVisible => true;

		public override string FormHeading => Res.GetString("69904C40-7B3E-4C87-90FA-DA34717A7E0A", "Refund applications");

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = new RefundApplicationSendingGridColumnLayout());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;
	}
}
