using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class AISDocumentsUploadForm : MessageSendingFormWithValidationDetails
	{
		public AISDocumentsUploadForm(UploadDocumentsSendingActionParent parent) : base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetAISDocumentsUploadAddInfoGridLayout();
			ValidationErrorsGroupBox.Visible = false;
		}

		public override string FormHeading => Res.GetString("EACC4E52-BE4F-4DEA-9F5C-AA5317479CA4", "Upload Documents");

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		void SetAISDocumentsUploadAddInfoGridLayout()
		{
			DynamicAddInfoGridsPanel.UpdateLayout(new AISDocumentsUploadAddInfoGridLayout());
			BindingSource.SetBindingMember(DynamicAddInfoGridsPanel, "SendingObjectsCollection");
		}

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = new AISUploadDocumentMessageSendingGridColumnLayout());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;
	}
}
