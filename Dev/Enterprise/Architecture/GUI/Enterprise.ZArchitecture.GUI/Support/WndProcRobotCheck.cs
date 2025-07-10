using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	public static class WndProcRobotCheck
	{
		public static void CheckForRPAOnNonRobotUser(uint msgId)
		{
			if (msgId == WM_GETCONTROLNAME)
			{
				if (Env.CurrentUser == null || Env.CurrentUser.IsRobot)
				{
					return;
				}

				if (lastReportTime == null || lastReportTime.Elapsed > timeSpanToWaitBeforeNextCheck.Value)
				{
					lastReportTime = Stopwatch.StartNew();
					if (!DataRegistry.Instance.DisableRobotDetection)
					{
						ReportRobotBreach();
					}
				}
			}
		}

		static void ReportRobotBreach()
		{
			var messageBox = new ZMessageBox(
				Res.GetString("0C6369AC-10F2-4BEA-9461-AB43CEBD666D", @"Your user ({0}) is not flagged as a robot, yet this session appears to have process automation software active.
To enable robot usage, please go to Maintain/System/User Accounts and enable this account as a robot.
If you are, in fact, not a robot please raise an incident with clear details of any automation software you have installed such as text to speech tools or other visual aids.", Env.CurrentUser.LoginName),
				"",
				System.Windows.Forms.MessageBoxButtons.OK,
				System.Windows.Forms.MessageBoxIcon.Error);

			ZFormModaliser.ShowDialogAndDispose(messageBox);
		}

		[ThreadStatic]
		static Stopwatch lastReportTime;

		[ThreadSafe]
		static readonly Overridable<TimeSpan> timeSpanToWaitBeforeNextCheck = new Overridable<TimeSpan>(TimeSpan.FromMinutes(37));

		public static uint WM_GETCONTROLNAME { get; } = RegisterWindowMessage("WM_GETCONTROLNAME");

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint RegisterWindowMessage(string lpString);

		[Conditional("DEBUG")]
		public static void SetTimeSpanForTest(TimeSpan value)
		{
			lastReportTime = null;
			timeSpanToWaitBeforeNextCheck.Value = value;
		}
	}
}
