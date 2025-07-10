using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class ZSecurityCheckpointTest : TransactionedTestCase
	{
		public void TestSecurityConstructorChecksMaxLengthOfCode()
		{
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			string code = "";
			for (int i = 0; i < GlbSecuritySchema.GU_SecurityRight.MaxLength; i++)
			{
				code += "a";
			}

			AssertEquals(GlbSecuritySchema.GU_SecurityRight.MaxLength, code.Length);

			SecurityCheckpoint testCheckPoint = new SecurityCheckpoint(code, (NoResString)"Zubin", null, testSecurity.ZSecurityInstance);
			AssertNotNull("Exact length works", testCheckPoint);

			bool exceptionThrown = false;
			try
			{
				SecurityCheckpoint testCheckPoint41Chars = new SecurityCheckpoint(code + "Z", (NoResString)"Zubin", null, testSecurity.ZSecurityInstance);
			}
			catch (ArgumentException e)
			{
				exceptionThrown = true;
				AssertEquals("The Code of a security checkpoint must have a maximum length of " + GlbSecuritySchema.GU_SecurityRight.MaxLength + " characters. [" + code + "Z] is invalid.", e.Message);
			}

			Assert(exceptionThrown);
		}

		public void TestSecurityCheckpointInstantiatedTwice()
		{
			try
			{
				SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				testSecurity.CachingEnabled = false;

				SecurityCheckpoint testCheckPoint = new SecurityCheckpoint("ZUB", (NoResString)"Zubin", null, testSecurity.ZSecurityInstance);
				AssertEquals("No errors shown", 0, UnitTestUserNotification.Instance.ShownErrorKeys.Length);

				testCheckPoint = new SecurityCheckpoint("ZUB", (NoResString)"Zubin", null, testSecurity.ZSecurityInstance);
				AssertEquals("Errors shown", 1, UnitTestUserNotification.Instance.ShownErrorKeys.Length);
				AssertEquals("Errors is about duplicate instanation", "SecurityCheckpointInstantiatedTwice_ZUB", UnitTestUserNotification.Instance.ShownErrorKeys[0]);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestSetDisplayText()
		{
			SecurityCheckpoint securityCheckpoint = new SecurityCheckpoint("Code", (NoResString)"DisplayText", null, null, false);
			AssertEquals("DisplayText", securityCheckpoint.DisplayText);

			securityCheckpoint.SetDisplayText((NoResString)"ModuleDisplayText");
			AssertEquals("ModuleDisplayText", securityCheckpoint.DisplayText);
		}

		public void TestCheckPoint()
		{
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			testSecurity.CachingEnabled = false;
			SecurityCheckpoint generalLedgerReports = new SecurityCheckpoint("TestGLRepReports", (NoResString)"Reports", null, testSecurity.ZSecurityInstance);

			AssertEquals("Code", "TestGLRepReports", generalLedgerReports.Code);
			AssertEquals("DisplayText", "Reports", generalLedgerReports.DisplayText);
			AssertNull("Parent", generalLedgerReports.Parent);
			AssertEquals("ChildCheckPoints", 0, generalLedgerReports.ChildCheckPoints.Count());

			SecurityCheckpoint balanceSheet = new SecurityCheckpoint("TestGLRepBalanceSheet", (NoResString)"Balance Sheet", generalLedgerReports, testSecurity.ZSecurityInstance);

			AssertEquals("Parent", generalLedgerReports, balanceSheet.Parent);
			AssertEquals("ChildCheckPoints.Count", 1, generalLedgerReports.ChildCheckPoints.Count());
			AssertEquals("ChildCheckPoints.First()", balanceSheet, generalLedgerReports.ChildCheckPoints.First());
		}

		public void TestVisible()
		{
			SecurityForTest security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityCheckpoint checkpoint = new SecurityCheckpoint("Code", (NoResString)"DisplayText", null, security.ZSecurityInstance);
			security.CachingEnabled = false;

			checkpoint.Country = null;
			AssertEquals("Visible - Null Country", true, checkpoint.Visible);

			checkpoint.Country = "";
			AssertEquals("Visible - Empty Country", true, checkpoint.Visible);

			checkpoint.Country = "SMELLY FISH";
			AssertEquals("Visible - None matching Country", false, checkpoint.Visible);

			checkpoint.Country = EnvProxy.Instance.CurrentCompany.Country.Code;
			AssertEquals("Visible - Matching Country", true, checkpoint.Visible);

			checkpoint.Hide();
			AssertEquals("Visible - Hidden", false, checkpoint.Visible);
		}

		public void TestShowErrorAndErrorMessageForNotAllowed()
		{
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityCheckpoint fruit = new SecurityCheckpoint("Fruits", (NoResString)"Fruit", null, testSecurity.ZSecurityInstance);
			SecurityCheckpoint apple = new SecurityCheckpoint("Apples", (NoResString)"Apple", fruit, testSecurity.ZSecurityInstance);
			SecurityCheckpoint appleCore = new SecurityCheckpoint("AppleCores", (NoResString)"AppleCore", apple, testSecurity.ZSecurityInstance);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			appleCore.ShowError();

			Assert("Error message shown should contain AppleCore security right", UnitTestUserNotification.Instance.LastMessage.Contains("Fruit -> Apple -> AppleCore"));
			AssertEquals("ErrorMessageForNotAllowed", UnitTestUserNotification.Instance.LastMessage.Text, appleCore.ErrorMessageForNotAllowed);
		}

		public void TestDisplayTextPath()
		{
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityCheckpoint fruit = new SecurityCheckpoint("Fruits", (NoResString)"Fruit", null, testSecurity.ZSecurityInstance);
			SecurityCheckpoint apple = new SecurityCheckpoint("Apples", (NoResString)"Apple", fruit, testSecurity.ZSecurityInstance);
			SecurityCheckpoint appleJuiceInvisible = new SecurityCheckpoint("AppleJuiceInvisible", (NoResString)string.Empty, apple, testSecurity.ZSecurityInstance);
			SecurityCheckpoint appleCore = new SecurityCheckpoint("AppleCores", (NoResString)"AppleCore", appleJuiceInvisible, testSecurity.ZSecurityInstance);

			AssertEquals("Display path", "Fruit", fruit.DisplayTextPathToSecurityRight);
			AssertEquals("Display path", "Fruit -> Apple", apple.DisplayTextPathToSecurityRight);
			AssertEquals("Display path - Apple Juice has no display text and should not be shown", "Fruit -> Apple -> AppleCore", appleCore.DisplayTextPathToSecurityRight);

			apple.SetDisplayText((NoResString)"Granny Smith");
			AssertEquals("Display path", "Fruit -> Granny Smith", apple.DisplayTextPathToSecurityRight);
		}

		public void TestMultilingualDisplayText()
		{
			var resouceStringKey = "LEMONed I Scream";
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityCheckpoint fruit = new SecurityCheckpoint("Fruits", (NoResString)"Fruit", null, testSecurity.ZSecurityInstance);
			SecurityCheckpoint lemon = new SecurityCheckpoint("Lemon", ResString.GetMultilingualString(resouceStringKey, "Lemon"), fruit, testSecurity);

			AssertEquals("Lemon", lemon.DisplayText);
			AssertEquals("Fruit -> Lemon", lemon.DisplayTextPathToSecurityRight);

			using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put(resouceStringKey, new ResourceStringData(resouceStringKey, "柠檬"));
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("柠檬", lemon.DisplayText);
					AssertEquals("Fruit -> 柠檬", lemon.DisplayTextPathToSecurityRight);
				}
			}
		}

		public void TestCheckPointName()
		{
			SecurityForTest testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityCheckpoint parrot = new SecurityCheckpoint("Parrot", (NoResString)"Parrot", null, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotEdit = new SecurityCheckpoint("ParrotEdit", (NoResString)"Edit", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotNew = new SecurityCheckpoint("ParrotNew", (NoResString)"New", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotDelete = new SecurityCheckpoint("ParrotDelete", (NoResString)"Delete", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotModify = new SecurityCheckpoint("ParrotModify", (NoResString)"Modify", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotView = new SecurityCheckpoint("ParrotView", (NoResString)"View", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotPrint = new SecurityCheckpoint("ParrotPrint", (NoResString)"Print", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotReports = new SecurityCheckpoint("ParrotReports", (NoResString)"Reports", parrot, testSecurity.ZSecurityInstance);
			SecurityCheckpoint parrotLorikeet = new SecurityCheckpoint("ParrotLorikeet", (NoResString)"Lorikeet", parrot, testSecurity.ZSecurityInstance);

			AssertEquals("SecurityCheckpoint should have name", "Parrot", parrot.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> Edit", parrotEdit.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> New", parrotNew.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> Delete", parrotDelete.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> Modify", parrotModify.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> View", parrotView.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> Print", parrotPrint.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Parrot -> Reports", parrotReports.HumanReadableName);
			AssertEquals("SecurityCheckpoint should have name", "Lorikeet", parrotLorikeet.HumanReadableName);
		}

		public void TestMaxSecurityCheckpointCodeLength()
		{
			SecurityForTest testEnglishSecurityA = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			SecurityForTest testEnglishSecurityB = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var testEnglishSecurityACustomsExportManifestTabPageCode = testEnglishSecurityA.FindCheckPoint("ShipmentForm").ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(14).Code;
			var testEnglishSecurityBCustomsExportManifestTabPageCode = testEnglishSecurityB.FindCheckPoint("ShipmentForm").ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(14).Code;
			Assert(testEnglishSecurityACustomsExportManifestTabPageCode == testEnglishSecurityBCustomsExportManifestTabPageCode);

			const string Bearbeiten = "Bearbeiten";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(key =>
				{ return key.StartsWith("Edit") ? null : new ResourceStringData(key, Bearbeiten); }));
				SecurityForTest testGermanSecurityA = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				var testGermanSecurityACustomsExportManifestTabPageCode = testGermanSecurityA.FindCheckPoint("ShipmentForm").ChildCheckPoints.ElementAt(1).ChildCheckPoints.ElementAt(14).Code;

				Assert(string.Format("A Code should not be localized as it is used as the key for persistence, MultilingualDescription is used for display", testGermanSecurityACustomsExportManifestTabPageCode), !testGermanSecurityACustomsExportManifestTabPageCode.Contains(Bearbeiten));
				Assert(testEnglishSecurityACustomsExportManifestTabPageCode == testGermanSecurityACustomsExportManifestTabPageCode);
			}
		}

		public void TestAllSecurityCheckpointsAreNotTranslated()
		{
			SecurityForTest testEnglishSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var testEnglishCheckpointArray = testEnglishSecurity.AllLoadedCheckPoints.ToArray();

			const string Bearbeiten = "Bearbeiten";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(key =>
				{ return key.StartsWith("Edit") ? null : new ResourceStringData(key, Bearbeiten); }));
				SecurityForTest testGermanSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				var testGermanCheckpointArray = testGermanSecurity.AllLoadedCheckPoints.ToArray();

				AssertContainsExactElementsInAnyOrder("A Code should not be localized as it is used as the key for persistence, MultilingualDescription is used for display",
					testEnglishCheckpointArray.Select(c => c.Code), testGermanCheckpointArray.Select(c => c.Code));
			}
		}
	}
}
