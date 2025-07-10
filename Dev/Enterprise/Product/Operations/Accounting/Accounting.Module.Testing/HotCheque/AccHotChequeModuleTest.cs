using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccHotChequeModuleTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPrint()
		{
			using (AccHotChequeModule testModule = new AccHotChequeModule())
			{
				Env.Security.PrintHotCheque.IsAllowed = false;
				testModule.HandlePrint_ForTestOnly(null, new EventArgs());
				AssertEquals("LastMessage", Env.Security.PrintHotCheque.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.PrintHotCheque.IsAllowed = true;
				testModule.HandlePrint_ForTestOnly(null, new EventArgs());
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestMenuItems()
		{
			using (AccHotChequeModule testModule = new AccHotChequeModule())
			{
				testModule.GetNewActionMenuItems_ForTestOnly();
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (AccHotChequeModule module = new AccHotChequeModule())
			{
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().Caption, "Cancel");
			}
		}
	}
}
