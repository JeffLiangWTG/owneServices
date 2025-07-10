using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class ExplanationOnDelayMessageSendingForm : EMCSMessageSendingForm<ExplanationOnDelaySendingAction>
	{
		public ExplanationOnDelayMessageSendingForm(ExplanationOnDelaySendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			return new ExplanationOnDelayBottomSectionUserControl();
		}
	}
}
