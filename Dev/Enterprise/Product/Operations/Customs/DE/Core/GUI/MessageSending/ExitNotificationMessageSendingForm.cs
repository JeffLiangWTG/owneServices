using System;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public partial class ExitNotificationMessageSendingForm : MessageSendingForm<ExitNotificationMessageSendingActionParent>
	{
		public ExitNotificationMessageSendingForm(ExitNotificationMessageSendingActionParent parent) : base(parent, Res.GetString("7D81A9B4-9CDC-414C-BC60-41104C095472", "Exit Notification"))
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			MessageSendingObjectsGrid.AfterBind += MessageSendingObjectsGrid_AfterBind;
		}

		void MessageSendingObjectsGrid_AfterBind(object sender, EventArgs e)
		{
			if (MessageSendingObjectsGrid.ListManager != null)
			{
				MessageSendingObjectsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			bottomSectionUserControl.EntryTypeChanged((ExitNotificationMessageSendingAction)MessageSendingObjectsGrid.ListManager.GetCurrent());
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			bottomSectionUserControl = new ExitNotificationBottomSectionUserControl();
			return bottomSectionUserControl;
		}
		ExitNotificationBottomSectionUserControl bottomSectionUserControl;
	}
}
