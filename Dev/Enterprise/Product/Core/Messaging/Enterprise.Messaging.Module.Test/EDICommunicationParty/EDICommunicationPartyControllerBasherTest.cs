using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Test.EDICommunicationParty
{
	[TestedType(typeof(EDICommunicationPartyController))]
	sealed class EDICommunicationPartyControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Messaging.EDICommunicationParty;
		}

		public void TestSupportUserSecurityCheckpoints()
		{
			var party = Factory.NewWithValidTestData<MasterFiles.Business.EDICommunicationParty>();
			Factory.Save();
			Assert("Precondition", Env.CurrentUser.IsSupportUser);

			ShowFormAndAssert(() => Controller.ShowViewForm(party), true);
			ShowFormAndAssert(() => Controller.ShowNewForm(), true);
			ShowFormAndAssert(() => Controller.ShowEditForm(party), true);
			ShowFormAndAssert(() => Controller.ShowDeleteForm(party), true);
		}

		public void TestNonSupportUserSecurityCheckpoints()
		{
			var party = Factory.NewWithValidTestData<MasterFiles.Business.EDICommunicationParty>();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Assert("Precondition", !Env.CurrentUser.IsSupportUser);

				ShowFormAndAssert(() => Controller.ShowViewForm(party), false);
				AssertExceptionThrown<SecurityAccessDeniedException>(() => Controller.ShowNewForm());
				ShowFormAndAssert(() => Controller.ShowEditForm(party), false);
				ShowFormAndAssert(() => Controller.ShowDeleteForm(party), false);

				Env.Security.EDICommunicationPartyView.IsAllowed = true;
				AssertExceptionThrown<SecurityAccessDeniedException>(() => Controller.ShowNewForm());
				ShowFormAndAssert(() => Controller.ShowViewForm(party), true);
				using (var form = Controller.ShowEditForm(party))
				{
					//If no edit access, the form should be readonly
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
				ShowFormAndAssert(() => Controller.ShowDeleteForm(party), false);

				Env.Security.EDICommunicationPartyEdit.IsAllowed = true;
				ShowFormAndAssert(() => Controller.ShowEditForm(party), true);

				Env.Security.EDICommunicationPartyNew.IsAllowed = true;
				ShowFormAndAssert(Controller.ShowNewForm, true);
				ShowFormAndAssert(() => Controller.ShowEditForm(party), true);
				ShowFormAndAssert(() => Controller.ShowDeleteForm(party), false);

				Env.Security.EDICommunicationPartyDelete.IsAllowed = true;
				ShowFormAndAssert(() => Controller.ShowDeleteForm(party), true);
			}
		}

		void ShowFormAndAssert(Func<IZForm> showForm, bool isAllowed)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (showForm.Invoke())
			{
				if (isAllowed)
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGetFormIsTheSameForAllUsers()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			Env.Security.EDICommunicationPartyView.IsAllowed = true;

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.Instance.CurrentBranchPK, Env.Instance.CurrentDepartmentPK))
			{
				var nonSupportUserParty = Factory.NewWithValidTestData<MasterFiles.Business.EDICommunicationParty>();
				Factory.Save();

				using (var form = Controller.ShowViewForm(nonSupportUserParty))
				{
					AssertEquals(typeof(EDICommunicationPartyForm), form.GetType());
				}
			}

			var supportUserParty = Factory.NewWithValidTestData<MasterFiles.Business.EDICommunicationParty>();
			Factory.Save();
			using (var form = Controller.ShowViewForm(supportUserParty))
			{
				AssertEquals(typeof(EDICommunicationPartyForm), form.GetType());
			}
		}
	}
}
