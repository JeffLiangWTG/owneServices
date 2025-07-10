using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DbHealth.Check;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Startup.Testing
{
	sealed class StartupDbHealthRestoreWarningCheckerTest : TestCaseWithFactory
	{
		void TestExecuteWithLastDbHealthCheckWarning(string type, Action assertAction)
		{
			var testCollection = new DbHealthWarningRegistryCollection();
			testCollection.Add(CreateWarning("Source1", type, "Desc1", ZBool.True, ZBool.True));
			testCollection.Add(CreateWarning("Source2", type, "Desc2", ZBool.True, ZBool.False));
			testCollection.Add(CreateWarning("Source4", DatabaseWarning.AlwaysOnWarning, "Desc4", ZBool.True, ZBool.True));

			using (SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				assertAction();
			}
		}

		public void TestExecuteWithLastDbHealthCheckWarning()
		{
			var checker = new StartupDbHealthRestoreWarningChecker();
			checker.Execute();
			AssertNullOrEmpty("Precondition", UnitTestUserNotification.Instance.LastMessage.Text);

			var warningType = DatabaseWarning.RestoreWarning;
			TestExecuteWithLastDbHealthCheckWarning(warningType, () =>
			{
				var expectedMessage = @"Desc2

Do you want to mark above warnings as 'Acknowledged' so that these warnings will not be reminded again?
";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				checker.Execute();
				var warningList = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value.OfType<DbHealthWarningRegistryElement>()
					.Where(x => x.WarningType == warningType);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, warningList.All(x => x.IsAcknowledged));

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				checker.Execute();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, warningList.All(x => x.IsAcknowledged));

				UnitTestUserNotification.Instance.ClearMessages();
				checker.Execute();
				AssertNullOrEmpty("Will not alert again", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			warningType = DatabaseWarning.FileLocationWarning;
			TestExecuteWithLastDbHealthCheckWarning(warningType, () =>
			{
				new StartupDbHealthRestoreWarningChecker().Execute();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		DbHealthWarningRegistryElement CreateWarning(ZString source, ZString type, ZString description, ZBool isAckowledgeable, ZBool isAcknowledged)
		{
			var warning = new DbHealthWarningRegistryElement(source, type, description, isAckowledgeable);
			warning.IsAcknowledged = isAcknowledged;

			AssertEquals(isAckowledgeable, warning.IsAcknowledgeable);
			AssertEquals(isAcknowledged, warning.IsAcknowledged);

			return warning;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestShouldExecute()
		{
			var portugalCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			portugalCompany.GC_RN_NKCountryCode = CountryCodes.Portugal;
			var nonPortugalCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			nonPortugalCompany.GC_RN_NKCountryCode = CountryCodes.KoreaNorth;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var initialUserContext = Env.CurrentUserContext;
			try
			{
				AssertShouldExecute(portugalCompany, staff, false, true, "Should execute when login in PT company");
				AssertShouldExecute(nonPortugalCompany, staff, false, false, "Should NOT execute when login in NON PT company");

				AssertShouldExecute(portugalCompany, staff, true, false, "Should NOT execute when HostedWithCargowise");
				AssertShouldExecute(portugalCompany, staff, false, true, "Should execute when NOT HostedWithCargowise");
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				ErrorReporter.Clear();
			}
		}

		void AssertShouldExecute(GlbCompany company, GlbStaff staff, bool isHostedWithCargowise, bool shouldExecute, string message)
		{
			EnvProxy.SetHostedLocationForTest(isHostedWithCargowise ? "SYD" : "");

			Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff.GS_LoginName, staff.StaffPlainTextPassword), company.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			AssertEquals("pre-condition", company.GC_Code, GlbCompany.CurrentCompany.GC_Code);
			AssertEquals(message, shouldExecute, new StartupDbHealthRestoreWarningChecker().ShouldExecute());
			Env.LoginController.Logout();
		}
	}
}
