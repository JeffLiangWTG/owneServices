using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class ReportOfReceiptMessageSendingForm : EMCSMessageSendingForm<ReportOfReceiptSendingAction>
	{
		public ReportOfReceiptMessageSendingForm(ReportOfReceiptSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			return new ReportOfReceiptBottomSectionUserControl();
		}
	}
}
