using Enterprise.Customs.DE.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public partial class ExitSummaryMessageSendingForm : MessageSendingForm<ExitSummaryMessageSendingActionParent>
	{
		public ExitSummaryMessageSendingForm(ExitSummaryMessageSendingActionParent parent) : base(parent, Res.GetString("3EEE8751-393F-41C3-9887-BF9C17EEE3C8", "Exit Summary"))
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
