using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TrackAndTraceNotificationsControl))]
	sealed class TrackAndTraceNotificationsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TrackAndTraceNotificationsRule();
		}

		public void TestUpdateLayout()
		{
			using (TrackAndTraceNotificationsControl control = new TrackAndTraceNotificationsControl())
			{
				TrackAndTraceNotificationsRule rule = new TrackAndTraceNotificationsRule();
				control.SetDataBinding(rule, null);

				Assert(control.Controls.Find("notifySenderOnSuccessOrAcknowledgementCheckBox", true)[0].Visible);
				Assert(control.Controls.Find("notifySenderOnErrorCheckBox", true)[0].Visible);
				Assert(control.Controls.Find("notifySenderOnDiscrepancyCheckBox", true)[0].Visible);
				Assert(control.Controls.Find("notifyGroupOnDiscrepancyCheckBox", true)[0].Visible);

				ZGroupBox errorAndDiscrepancyGroupBox = (control.Controls.Find("errorAndDiscrepancyGroupBox", true)[0]) as ZGroupBox;
				AssertEquals("Error And Discrepancy Notifications", errorAndDiscrepancyGroupBox.CaptionResourceString.Caption);

				ZGroupBox groupForErrorsAndDiscrepanciesGroupBox = (control.Controls.Find("groupForErrorsAndDiscrepanciesGroupBox", true)[0]) as ZGroupBox;
				AssertEquals("Group For Errors And Discrepancies", groupForErrorsAndDiscrepanciesGroupBox.CaptionResourceString.Caption);
			}

			using (TrackAndTraceNotificationsControl control = new TrackAndTraceNotificationsControl())
			{
				TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider = new TrackAndTraceNotificationsRuleVisibilityProvider();

				visibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled = false;
				visibilityProvider.NotifySenderOnErrorEnabled = false;
				visibilityProvider.NotifySenderOnDiscrepancyEnabled = false;
				visibilityProvider.NotifyGroupOnDiscrepancyEnabled = false;

				TrackAndTraceNotificationsRule rule = new TrackAndTraceNotificationsRule(visibilityProvider);

				control.SetDataBinding(rule, null);

				Assert(!control.Controls.Find("notifySenderOnSuccessOrAcknowledgementCheckBox", true)[0].Visible);
				Assert(!control.Controls.Find("notifySenderOnErrorCheckBox", true)[0].Visible);
				Assert(!control.Controls.Find("notifySenderOnDiscrepancyCheckBox", true)[0].Visible);
				Assert(!control.Controls.Find("notifyGroupOnDiscrepancyCheckBox", true)[0].Visible);

				ZGroupBox errorAndDiscrepancyGroupBox = (control.Controls.Find("errorAndDiscrepancyGroupBox", true)[0]) as ZGroupBox;
				AssertEquals("Error Notifications", errorAndDiscrepancyGroupBox.CaptionResourceString.Caption);

				ZGroupBox groupForErrorsAndDiscrepanciesGroupBox = (control.Controls.Find("groupForErrorsAndDiscrepanciesGroupBox", true)[0]) as ZGroupBox;
				AssertEquals("Group For Errors", groupForErrorsAndDiscrepanciesGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
