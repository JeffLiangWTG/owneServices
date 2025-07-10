using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class DocumentsSendingForm : MessageSendingObjectForm
	{
		public DocumentsSendingForm()
		{
			InitializeComponent();
			InitializeMessageSendingGridColumns();
		}

		public DocumentsSendingForm(DocumentsSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
			InitializeMessageSendingGridColumns();
		}

		public override string FormHeading => Res.GetString("829246DA-75D4-4BEF-97E7-3EC3FB7312D9", "Send Documents");

		void InitializeMessageSendingGridColumns()
		{
			MessageSendingObjectsGrid.ApplyGridColumnLayout(MessageSendingGridColumnLayoutProvider);
		}

		IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = new DocumentMessageSendingGridColumnLayout());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;
	}
}
