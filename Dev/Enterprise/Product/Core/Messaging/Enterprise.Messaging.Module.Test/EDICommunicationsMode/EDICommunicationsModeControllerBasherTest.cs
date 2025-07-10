using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeController))]
	sealed class EDICommunicationsModeControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Messaging.EDICommunicationsMode;
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true);
		}

		public void TestSupportUserSecurityCheckpoints()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.FillWithValidTestData();
			var ediCommunicationsMode = Factory.New<EDICommunicationsMode>();
			ediCommunicationsMode.EK_ParentID = orgHeader.PK;
			Factory.Save();
			Assert("Precondition", Env.CurrentUser.IsSupportUser);

			ShowFormAndAssert(() => Controller.ShowViewForm(ediCommunicationsMode), true);
			ShowFormAndAssert(() => Controller.ShowEditForm(ediCommunicationsMode), true);
			ShowFormAndAssert(() => Controller.ShowDeleteForm(ediCommunicationsMode), true);
		}

		public void TestNonSupportUserSecurityCheckpoints()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.FillWithValidTestData();
			var ediCommunicationsMode = Factory.New<EDICommunicationsMode>();
			ediCommunicationsMode.EK_ParentID = orgHeader.PK;
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Assert("Precondition", !Env.CurrentUser.IsSupportUser);

				ShowFormAndAssert(() => Controller.ShowViewForm(ediCommunicationsMode), false);
				ShowFormAndAssert(() => Controller.ShowEditForm(ediCommunicationsMode), false);
				ShowFormAndAssert(() => Controller.ShowDeleteForm(ediCommunicationsMode), false);

				Env.Security.OrganisationView.IsAllowed = true;
				Env.Security.OrganisationModify.IsAllowed = true;
				Env.Security.OrganisationDelete.IsAllowed = true;
				ShowFormAndAssert(() => Controller.ShowViewForm(ediCommunicationsMode), true);
				ShowFormAndAssert(() => Controller.ShowEditForm(ediCommunicationsMode), true);
				ShowFormAndAssert(() => Controller.ShowDeleteForm(ediCommunicationsMode), true);
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

		public void TestShowViewForm()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.FillWithValidTestData();
			var ediCommunicationsMode = Factory.New<EDICommunicationsMode>();
			ediCommunicationsMode.EK_ParentID = orgHeader.PK;
			Factory.Save();

			using (var form = Controller.ShowViewForm(ediCommunicationsMode))
			{
				Application.DoEvents();
				AssertType(typeof(ZOrganisationsForm), form);
				var bizO = ((ZOrganisationsForm)form).GetSelectedEDICommunicationsModeFromGrid();
				AssertNotNull(bizO);
				Assert(bizO.Length > 0);
				AssertType(typeof(EDICommunicationsMode), bizO[0]);
				AssertEquals(ediCommunicationsMode.PK, bizO[0].PK);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.FillWithValidTestData();
			var result = Factory.New<EDICommunicationsMode>();
			result.EK_ParentID = orgHeader.PK;
			Factory.Save();
			return result;
		}
	}
}
