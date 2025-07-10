using System;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class TrackAndTraceNotificationsControl : RegistryBusinessObjectTemplateZUserControl
	{
		public TrackAndTraceNotificationsControl()
		{
			InitializeComponent();
		}

		TrackAndTraceNotificationsRule TrackAndTraceNotificationsRule
		{
			get { return DataSource as TrackAndTraceNotificationsRule; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			UpdateLayout();
		}

		void UpdateLayout()
		{
			if (TrackAndTraceNotificationsRule != null && TrackAndTraceNotificationsRule.VisibilityProvider != null)
			{
				if (!TrackAndTraceNotificationsRule.VisibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled)
				{
					this.notifySenderOnSuccessOrAcknowledgementCheckBox.Visible = false;
				}

				if (!TrackAndTraceNotificationsRule.VisibilityProvider.NotifySenderOnErrorEnabled)
				{
					this.notifySenderOnErrorCheckBox.Visible = false;
				}

				if (!TrackAndTraceNotificationsRule.VisibilityProvider.NotifySenderOnDiscrepancyEnabled)
				{
					this.notifySenderOnDiscrepancyCheckBox.Visible = false;
				}

				if (!TrackAndTraceNotificationsRule.VisibilityProvider.NotifyGroupOnDiscrepancyEnabled)
				{
					this.notifyGroupOnDiscrepancyCheckBox.Visible = false;

					this.errorAndDiscrepancyGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c8c887c0-df4f-42d3-b2ad-605cee993a94", "Error Notifications");
					this.groupForErrorsAndDiscrepanciesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("79cb293c-c632-4bb4-91c7-c32018a1b0ff", "Group For Errors");
				}
			}
		}
	}
}
