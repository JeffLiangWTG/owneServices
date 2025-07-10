using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class CancellationMessageSendingForm : EMCSMessageSendingForm<CancellationSendingAction>
	{
		public CancellationMessageSendingForm(CancellationSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			return new CancellationBottomSectionUserControl();
		}
	}
}
