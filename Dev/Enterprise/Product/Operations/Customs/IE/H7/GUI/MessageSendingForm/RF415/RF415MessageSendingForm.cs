using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public partial class RF415MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		public RF415MessageSendingForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			messageSendingObjectsGroupBox.CaptionResourceString = Res.GetData("281288b9-a866-40b9-a239-003293d56ee9", "Refund Applications");
			DocumentsGroupBox.CaptionResourceString = Res.GetData("500d1843-fec5-4ae2-8c43-9031b6329ea2", "Documents");
			ValidationErrorsGroupBox.Visible = false;
		}

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ??= new RF415MessageSendingGridColumnLayout();
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;

		public override string FormHeading => Res.GetString("2004f706-62a0-4ce4-948b-44bffd9ebb66", "Refund Applications");

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		protected override bool PreviewMessageCheckboxVisible => false;

		protected override void AddUserControlToBottomSection()
		{
			AddSelectAllButton();
		}

		protected override bool ShouldShowProgressForm => false;
	}
}
