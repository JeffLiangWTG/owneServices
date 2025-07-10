using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CMRMessagingForm))]
	sealed class CMRMessagingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestMenu()
		{
			using (CMRMessagingFormForTest form = new CMRMessagingFormForTest(Factory.New<DummyEnterpriseBusinessObject>()))
			{
				bool menuPresent = false;
				foreach (MenuItem item in form.Menu.MenuItems)
				{
					if (item.Text == "MenuItemText")
					{
						menuPresent = true;
					}
				}

				AssertEquals("MenuPresent", true, menuPresent);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			Customs.Business.Testing.MessageManageableBusinessObject bizObj = Factory.New<Customs.Business.Testing.MessageManageableBusinessObject>();
			TestHelperManger manager = new TestHelperManger(bizObj);
			var mockChecker = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
			mockChecker.Setup(m => m.DetermineRequiredMessagesAndSendThem(manager))
				.Returns(ContinueWithSave.Yes);
			using (var form = new CMRMessagingFormForTest(bizObj))
			{
				form.fManager = manager;
				form.AmendmentController = mockChecker.Object;
				if (form is IShowPreSaveDialog dialog)
				{
					ContinueWithSave result = dialog.ShowPreSaveDialogs();
					AssertEquals(ContinueWithSave.Yes, result);
				}
				mockChecker.VerifyAll();
			}
		}

		protected override Form GetFormToBashCore() => new CMRMessagingFormForTest(Factory.New<DummyEnterpriseBusinessObject>());

		sealed class CMRMessagingFormForTest : CMRMessagingForm
		{
			public CMRMessagingFormForTest(IBusiness businessEntity) : base(businessEntity)
			{
				ZLabel label = new ZLabel();
				MainTabPage.Controls.Add(label); //shuts the basher up
			}

			public SendsMessagesToCustomsGUI AmendmentController;

			public TestHelperManger fManager;

			protected override MultiMessageManager GetManager()
			{
				if (fManager == null)
				{
					fManager = new TestHelperManger(BusinessEntity.Factory.New<Customs.Business.Testing.MessageManageableBusinessObject>());
				}

				return fManager;
			}

			protected override MenuItem GetMessagingMenu() => new ZMenuItem("MenuItemText");

			protected override SendsMessagesToCustomsGUI GetNewMessagingActionsController() => AmendmentController ?? base.GetNewMessagingActionsController();
		}

		sealed class TestHelperManger : MultiMessageManager
		{
			public TestHelperManger(Customs.Business.Testing.MessageManageableBusinessObject bizObj) : base()
			{
				this.bizObj = bizObj;
			}
			readonly Customs.Business.Testing.MessageManageableBusinessObject bizObj;

			public override IMessageManageableBizObj TopLevelBizObjToManage => bizObj;

			protected override bool SendWheneverPossibleOnceMessagingActive => false;

			protected override SingleMessageManager[] GetAllMessageManagers() => System.Array.Empty<SingleMessageManager>();
		}
	}
}
