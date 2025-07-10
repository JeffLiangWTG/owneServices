using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public partial class ExitNotificationBottomSectionUserControl : ZUserControl
	{
		public ExitNotificationBottomSectionUserControl()
		{
			InitializeComponent();
		}

		public void SetControlsVisibility(ExitNotificationMessageSendingAction action)
		{
		}

		void EntryTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var action = (ExitNotificationMessageSendingAction)sender;
			EntryTypeChanged(action);
		}

		public void EntryTypeChanged(ExitNotificationMessageSendingAction action)
		{
			SetControlsVisibility(action);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
