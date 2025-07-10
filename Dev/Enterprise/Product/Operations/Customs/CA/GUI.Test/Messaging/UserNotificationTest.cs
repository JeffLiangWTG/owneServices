using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class UserNotificationTest : TestCaseWithFactory
	{
		public void TestShowMessageInstructionForm()
		{
			var userNotif = new MessageInstructionUserNotificationForTest();
			var instruction = new MessageInstruction(Factory, false, string.Empty, string.Empty);
			AssertNoExceptionThrown("No Exception means form is not shown", () => userNotif.ThrowExceptionIfShowFormIsTrue(instruction));
			instruction = new MessageInstruction(Factory, false, string.Empty, "test");
			AssertExceptionThrown("Has Exception means form is shown", typeof(Exception), () => userNotif.ThrowExceptionIfShowFormIsTrue(instruction));
			instruction = new MessageInstruction(Factory, false, "test", string.Empty);
			AssertExceptionThrown("Has Exception means form is shown", typeof(Exception), () => userNotif.ThrowExceptionIfShowFormIsTrue(instruction));
			instruction = new MessageInstruction(Factory, true, string.Empty, string.Empty);
			AssertExceptionThrown("Has Exception means form is shown", typeof(Exception), () => userNotif.ThrowExceptionIfShowFormIsTrue(instruction));
		}

		public void TestSupervisorOverridesOnInstructionForm()
		{
			JobDeclaration decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = "ZZZ";
			decl.Validation.ValidateAll();
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_IsActive = true;

			GlbSecurity se = Factory.New<GlbSecurity>();
			se.GU_SecurityRight = "AllowMessageErrors";
			se.GU_GG = Core.Constants.Groups.AllPK;
			se.GU_SecurityItemIsAllowed = true;
			se.GU_GS = staff.PK;
			Factory.Save();

			var userNotif = new MessageInstructionUserNotificationForTest();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var ubstruction = new MessageInstruction(Factory, true, "Test Error Message", string.Empty, decl, true, true);
			Assert(ubstruction.ContainsValidationErrors);
			Assert(userNotif.ShowMessageInstructionForm(ubstruction));

			ubstruction = new MessageInstruction(Factory, true, string.Empty, string.Empty, decl, true, true);
			Assert(!ubstruction.ContainsValidationErrors);
			Assert(userNotif.ShowMessageInstructionForm(ubstruction));
		}
	}
}
