using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class AlertOrRejectMessageSendingForm : EMCSMessageSendingForm<AlertOrRejectSendingAction>
	{
		public AlertOrRejectMessageSendingForm(AlertOrRejectSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl() => new AlertOrRejectBottomSectionUserControl();
	}
}
