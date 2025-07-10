using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	public class WndProcRobotCheckTest : TestCaseWithFactory
	{
		public void TestRobotIsReported()
		{
			// Arrange
			using (ZFormModaliser.SuspendDispose())
			{
				WndProcRobotCheck.SetTimeSpanForTest(TimeSpan.FromHours(1));
				var msg = WndProcRobotCheck.WM_GETCONTROLNAME;

				// Act
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(msg);

				// Assert
				AssertEquals("Your user (CWSupport) is not flagged as a robot, yet this session appears to have process automation software active.\r\nTo enable robot usage, please go to Maintain/System/User Accounts and enable this account as a robot.\r\nIf you are, in fact, not a robot please raise an incident with clear details of any automation software you have installed such as text to speech tools or other visual aids.",
					((ZMessageBox)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		public void TestRobotIsNotReportedBeforeReportTimeout()
		{
			// Arrange
			using (ZFormModaliser.SuspendDispose())
			{
				var timeSpan = TimeSpan.FromHours(1);
				WndProcRobotCheck.SetTimeSpanForTest(timeSpan);
				var msg = WndProcRobotCheck.WM_GETCONTROLNAME;
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(msg);
				ZFormModaliser.LastFormShownDialogForTest = null;
				var stopwatch = Stopwatch.StartNew();

				// Act
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(msg);

				// Assert
				AssertLessThan(stopwatch.Elapsed, timeSpan);
				AssertNull("Should be no window", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestRobotIsReportedAfterReportTimeout()
		{
			// Arrange
			using (ZFormModaliser.SuspendDispose())
			{
				var timeSpan = TimeSpan.FromSeconds(5);
				WndProcRobotCheck.SetTimeSpanForTest(timeSpan);
				var msg = WndProcRobotCheck.WM_GETCONTROLNAME;
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(msg);
				ZFormModaliser.LastFormShownDialogForTest = null;
				var stopwatch = Stopwatch.StartNew();
				Thread.Sleep(timeSpan);

				// Act
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(msg);

				// Assert
				AssertGreaterThan(stopwatch.Elapsed, timeSpan);
				AssertEquals("Your user (CWSupport) is not flagged as a robot, yet this session appears to have process automation software active.\r\nTo enable robot usage, please go to Maintain/System/User Accounts and enable this account as a robot.\r\nIf you are, in fact, not a robot please raise an incident with clear details of any automation software you have installed such as text to speech tools or other visual aids.",
					((ZMessageBox)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		[ExpectNoExceptions]
		public void TestRobotUserIsNotReported()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_IsRobot = true;
			Factory.Save();
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(WndProcRobotCheck.WM_GETCONTROLNAME);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenRegistrySet()
		{
			using (RawDataRegistry.Instance.DisableRobotDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WndProcRobotCheck.CheckForRPAOnNonRobotUser(WndProcRobotCheck.WM_GETCONTROLNAME);
			}
		}

		public void TestRobotIsNotReportedWhenNoUserIsLoggedIn()
		{
			// Arrange
			using (Env.SetTemporaryUserContext(null))
			{
				// Act
				// Assert
				AssertNoExceptionThrown(() => WndProcRobotCheck.CheckForRPAOnNonRobotUser(WndProcRobotCheck.WM_GETCONTROLNAME));
				AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
