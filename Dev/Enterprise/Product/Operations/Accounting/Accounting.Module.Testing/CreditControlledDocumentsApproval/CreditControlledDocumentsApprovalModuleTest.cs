using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalModule))]
	public class CreditControlledDocumentsApprovalModuleTest : ZModuleBasherTest
	{
		public CreditControlledDocumentsApprovalModuleTest()
			: base()
		{
		}

		public void TestGetActionMenus()
		{
			using (var module = (CreditControlledDocumentsApprovalModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var actionMenuItems = module.GetNewActionMenuItems_Test();
				AssertNotNull("There should be a 'approve' menu item", actionMenuItems.FindByText("&Approve"));
				AssertNotNull("There should be a 'reject' menu item", actionMenuItems.FindByText("&Reject"));
				AssertNotNull("There should be a 'cancel' menu item", actionMenuItems.FindByText("&Cancel"));
			}
		}

		public void TestShowErrorMessage()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";
			var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var doc = Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
			doc.Initialize(businessObject, menutItem.PK, new int[] { 1 });
			Factory.Save();
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = false;

			using (var module = (CreditControlledDocumentsApprovalModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					var actionMenuItems = module.GetNewActionMenuItems_Test();
					var mi = actionMenuItems.FindByText("&Approve");
					mi.PerformClick();

					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 
Manage -> Receivables -> Receivables Transactions -> On Credit Hold Controller", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShowCancelErrorMessage()
		{
			using (IMockResourceStringCache mockData = Res.UseMockData())
			{
				mockData.Put("8aafca9b-d78f-40e0-aac6-4376b190d760", new ResourceStringData("8aafca9b-d78f-40e0-aac6-4376b190d760", @"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied."));

				var menutItem = Factory.New<StmMenuItem>();
				menutItem.SU_MenuPath = "menu/path/newbizo";
				menutItem.SU_MenuName = "name";
				var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

				var doc = Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
				doc.Initialize(businessObject, menutItem.PK, new int[] { 1 });
				var request = Factory.LoadTop1<GenApprovalRequest>(new ZQuery());
				request.XP_SystemCreateUser = "AAA";
				Factory.Save();

				Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = false;

				using (var module = (CreditControlledDocumentsApprovalModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						module.PerformSearch_ForTest();
						module.DisplayGrid.SelectAllElements();

						var actionMenuItems = module.GetNewActionMenuItems_Test();
						var mi = actionMenuItems.FindByText("&Cancel");
						mi.PerformClick();

						AssertEquals(@"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestCreateUserCancelCreditControlledDocumentsApproval()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";
			var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var doc = Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
			doc.Initialize(businessObject, menutItem.PK, new int[] { 1 });
			Factory.Save();

			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = false;

			using (var module = (CreditControlledDocumentsApprovalModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					var actionMenuItems = module.GetNewActionMenuItems_Test();
					var mi = actionMenuItems.FindByText("&Cancel");
					mi.PerformClick();

					var type = typeof(CreditControlledDocumentsApprovalForm);
					AssertNotNull(string.Format("Expected a form, Type: {0}", type), ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals(type, ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CreditControlledDocumentsApproval;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}
	}
}
