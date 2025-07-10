using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TextMacroProcessorTest : TestCaseWithFactory
	{
		public void TestPassReportMenuItem_InValidStmMenuItem()
		{
			var invalidMenuItem = new object();
			var processor = new TextMacroProcessor();
			var exception = AssertExceptionThrown<ArgumentException>(
				() => processor.Replace(
					"",
					new BusinessObject[] { Factory.New<DummyBusinessObject>() },
					false,
					invalidMenuItem
				)
			);
			AssertEquals("stmMenuItem should be null or 'Enterprise.MasterFiles.Integration.IStmMenuItem' or 'Enterprise.MasterFiles.Business.StmMenuItem'", exception.Message);
		}

		public void TestPassReportMenuItem_NullMenuItem()
		{
			var menuItem = null as object;
			var processor = new TextMacroProcessor();
			processor.Replace(
				"",
				new BusinessObject[] { Factory.New<DummyBusinessObject>() },
				false,
				menuItem
			);
			AssertEquals(null, processor.LastReportMenuItem_ForTest);
		}

		public void TestPassReportIStmMenuItem()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			Factory.Save();

			var mockMenuItem = new Mock<IStmMenuItem>(MockBehavior.Strict);
			mockMenuItem.Setup(m => m.PK).Returns(menuItem.PK);
			mockMenuItem.Setup(m => m.SU_DeliveryRestrictionType).Returns(nameof(DeliveryRestrictionType.UDF));
			mockMenuItem.Setup(m => m.SU_DeliveryRestrictionMacro).Returns("<CreditOnHold()>");
			mockMenuItem.Setup(m => m.SU_DeliveryRestrictionDescription).Returns("Test Menu");

			var processor = new TextMacroProcessor();
			processor.Replace(
				"",
				new BusinessObject[] { Factory.New<DummyBusinessObject>() },
				false,
				mockMenuItem.Object
			);
			AssertEquals(menuItem.PK, processor.LastReportMenuItem_ForTest.PK);
		}

		public void TestPassReportStmMenuItem()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var processor = new TextMacroProcessor();
			Factory.Save();
			processor.Replace(
				"",
				new BusinessObject[] { Factory.New<DummyBusinessObject>() },
				false,
				menuItem
			);
			AssertEquals(menuItem, processor.LastReportMenuItem_ForTest);
		}

		public void TestProcessorShouldEscapeSpecialCharacters()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "<A>";
			var processor = new TextMacroProcessor();

			var result1 = processor.Replace("<Z0_Code>", new BusinessObject[] { dummy });

			AssertEquals(@"<A>", result1);

			var result2 = processor.Replace(@"""<Z0_Code>""==""\<A\>""", new BusinessObject[] { dummy }, false, null, false, true);

			AssertEquals(@"""<A>""==""<A>""", result2);

			dummy.Z0_Code = @"A\<";

			var result3 = processor.Replace(@"""<Z0_Code>""==""A\\\<""", new BusinessObject[] { dummy }, false, null, false, true);

			AssertEquals(@"""A\<""==""A\<""", result3);

			dummy.Z0_NVarChar = @"AB<""='|\&"">";

			var result4 = processor.Replace(@"""<Z0_NVarChar>""==""AB\<\""='|\\&\""\>""", new BusinessObject[] { dummy }, false, null, false, true);

			AssertEquals(@"""AB<""='|\&"">""==""AB<""='|\&"">""", result4);
		}

		public void TestCustomisedColumnMacro()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var processor = new TextMacroProcessor();

			AssertEquals(@"Should return empty because this macro only works on reports, not documents.", string.Empty, processor.Replace("<CustomisedColumn(Whatever)>", new BusinessObject[] { dummy }));
		}

		public void TestHtmlMacro()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = "Hello World";

			var processor = new TextMacroProcessor();

			AssertEquals("Hello World", processor.Replace("<Html(\"<Z0_VarCharMax>\", true)>", new BusinessObject[] { dummy }));
		}

		public void TestEmptyReplace()
		{
			AssertEquals("", new TextMacroProcessor().Replace("", new BusinessObject[] { Factory.New<DummyBusinessObject>() }));
		}

		public void TestNomatchReplace()
		{
			AssertEquals("This is a test.", new TextMacroProcessor().Replace("This is a test.", new BusinessObject[] { Factory.New<DummyBusinessObject>() }));
		}

		public void TestReplaceWithMatch()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarCharMax = "wow";
			AssertEquals("oh wow", new TextMacroProcessor().Replace("oh <Z0_VarCharMax>", new BusinessObject[] { bizo }));
		}

		public void TestReplaceWithMatch_Multiple()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarCharMax = "wow";
			AssertEquals("oh wow wow", new TextMacroProcessor().Replace("oh <Z0_VarCharMax> <Z0_VarCharMax>", new BusinessObject[] { bizo }));
		}

		public void TestReplaceFailWithMultipleBusinessObjectsShouldNotInformUser()
		{
			var isUserInteractive = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = true, () => Globals.IsUserInteractive = isUserInteractive))
			{
				var dummy = Factory.New<IDummyWithWorkflow>();
				var task = dummy.AddNewTask();

				var bizos = new BusinessObject[] { (BusinessObject)dummy, (BusinessObject)Factory.New<IDummyWithWorkflow>() };
				var processor = new TextMacroProcessor();
				var userNotification = new Mock<IUserNotification>();
				processor.UserNotification = userNotification.Object;

				var result = processor.Replace(@"<WorkflowItems.Find(""<SubString(""A"", 0,0)>""==""B"")>", bizos);

				AssertEquals("Macro did not evaluate to anything return empty", "", result);
				userNotification.Verify(u => u.Show(It.IsAny<string>()), Times.Never, "Should not notify user");
			}
		}

		public void TestReplaceMacroNowHasNewValueEveryCall()
		{
			var bizo = new BusinessObject[] { Factory.New<DummyBusinessObject>() };
			var textMacroProcessor = new TextMacroProcessor();
			var date1 = DateTime.Parse(textMacroProcessor.Replace("<Now>", bizo), CultureInfo.InvariantCulture); // not an issue with Globalization as InvariantCulture is used
			System.Threading.Thread.Sleep(2100);
			var date2 = DateTime.Parse(textMacroProcessor.Replace("<Now>", bizo), CultureInfo.InvariantCulture); // not an issue with Globalization as InvariantCulture is used
			Assert("The time difference between the dates should be greater than 1 seconds", (date2 - date1).TotalSeconds > 1);
		}

		[ExpectNoExceptions]
		public void TestExceptionDuringReplace_UserInteractiveMode_ShouldInformUser()
		{
			var isUserInteractive = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = true, () => Globals.IsUserInteractive = isUserInteractive))
			{
				var bizo = Factory.New<DummyThatThrowsOnAProperty>();
				var processor = new TextMacroProcessor();
				var userNotification = new Mock<IUserNotification>();
				processor.UserNotification = userNotification.Object;

				userNotification.Setup(m => m.Show(@"There was a problem trying to evaluate a macro template. The macro text is as follows:

oh <Z0_VarCharMax>

The error was:

Nyat nyat nyat"));

				processor.Replace("oh <Z0_VarCharMax>", new[] { bizo });

				userNotification.VerifyAll();
			}
		}

		public void TestExceptionDuringReplace_NonUserInteractiveMode_ShouldSendNotificationEmailToPostMastersGroup()
		{
			var staff = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "PMG").Staff.AddNew();
			staff.GS_EmailAddress = "me@you.com";
			Factory.Save();

			AssertExceptionDuringReplace_NonUserInteractive_ShouldEmailSpecifiedAddress("me@you.com");
		}

		public void TestExceptionDuringReplace_NonUserInteractiveMode_ShouldSendNotificationEmailToActiveStaffEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "you@me.com";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertExceptionDuringReplace_NonUserInteractive_ShouldEmailSpecifiedAddress("you@me.com");
			}
		}

		public void TestExceptionDuringReplace_CanShowDialogs_ButInTransaction_ShouldSendNotificationEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "you@me.com";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Db.Connection.RunTransactioned(() => AssertExceptionDuringReplace_NonUserInteractive_ShouldEmailSpecifiedAddress("you@me.com", true));
			}
		}

		void AssertExceptionDuringReplace_NonUserInteractive_ShouldEmailSpecifiedAddress(string emailAddress, bool isUserInteractive = false)
		{
			var bizo = Factory.New<DummyThatThrowsOnAProperty>();
			var processor = new TextMacroProcessor();

			var originalIsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = isUserInteractive;
			try
			{
				processor.Replace("oh <Z0_VarCharMax>", new[] { bizo });
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(emailAddress, email.Recipients[0].Email);
			AssertEquals("There was a problem trying to evaluate a text template", email.Subject);
			AssertStartsWith("Emails should be as detailed as possible", @"There was a problem trying to evaluate a macro template. The macro text is as follows:

oh <Z0_VarCharMax>

The error was:

Nyat nyat nyat


Service task code: N/A
Relevant Business Objects:
	- Dummy(NCODE)

Error details:", email.Body);
		}
	}
}
