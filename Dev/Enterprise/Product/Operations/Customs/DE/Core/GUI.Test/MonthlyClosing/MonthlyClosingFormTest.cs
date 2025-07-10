using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Module;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(MonthlyClosingForm))]
	class MonthlyClosingFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			reconDeclaration.CRD_JobReferenceNumber = "MON000021";
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				AssertEquals("Monthly Closing Job - MON000021", form.FormCaption);
			}
		}

		public void TestMessagingMenu()
		{
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				var messagingMenu = form.Menu.MenuItems.FindByText("Messages");
				CombineAssertions(() =>
				{
					AssertType<MonthlyClosingMessagingMenu>("Type", messagingMenu);
					AssertEquals("Index", 2, messagingMenu.Index);
				});
			}
		}

		public void TestLinkSimplifiedDeclarationsMenuItem_EnabledWhenIsFinalizedFalse()
		{
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Declaration is not finalized", false, reconDeclaration.IsFinalized);

					var linkSimplifiedDeclarationsMenuItem = GetLinkSimplifiedDeclarationsMenu(form);
					AssertEquals("LinkSimplifiedDeclarationsMenuItem is enabled", true, linkSimplifiedDeclarationsMenuItem.Enabled);
				});
			}
		}

		public void TestLinkSimplifiedDeclarationsMenuItem_DisabledWhenIsFinalizedTrue()
		{
			reconDeclaration.CreateFinalizationFlagNote("1");
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Declaration is finalized", true, reconDeclaration.IsFinalized);

					var linkSimplifiedDeclarationsMenuItem = GetLinkSimplifiedDeclarationsMenu(form);
					AssertEquals("LinkSimplifiedDeclarationsMenuItem is disabled", false, linkSimplifiedDeclarationsMenuItem.Enabled);
				});
			}
		}

		public void TestLinkSimplifiedDeclarationsMenuItem_Click_DialogShown()
		{
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				var linkSimplifiedDeclarationsMenuItem = GetLinkSimplifiedDeclarationsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				linkSimplifiedDeclarationsMenuItem.PerformClick();
				AssertType<SimplifiedDeclarationModuleForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestFilterBusinessObjectDefaultsSet()
		{
			var fromDate = new ZDate(2021, 04, 01);
			var toDate = new ZDate(2021, 04, 30);
			reconDeclaration.CRD_PeriodFrom = fromDate;
			reconDeclaration.CRD_PeriodTo = toDate;
			AssertFilterBusinessObjectDefaults(reconDeclaration, collection => AssertGreaterThan("LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter is called to set default filters", collection.FilterBusinessObjectDefaults.Count, 0));
		}

		public void TestMessagesTabPage()
		{
			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				var messagesTabPage = form.FindSingle<ZTabPage>("MessagesTabPage");
				var messagesTabUserControl = messagesTabPage.FindSingle<EU.GUI.MessagesTabUserControl>("MessagesTabUserControl");
				AssertEquals("DataSourceType", typeof(EDIMessageCollection), messagesTabUserControl.DataSourceType);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			Factory.Save();
			var form = new MonthlyClosingForm(declaration);
			form.ControllerID = ControllerIDs.Customs.DE.MonthlyClosing;
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			reconDeclaration = Factory.New<CusReconDeclaration>();
		}
		CusReconDeclaration reconDeclaration;

		void AssertFilterBusinessObjectDefaults(CusReconDeclaration reconDeclaration, Action<IFilterBusinessObjectDefaultsProvider> assertions)
		{
			ActiveBusinessObjectCollection<CusReconEntry> moduleGridCollection = null;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
			{
				if (o is SimplifiedDeclarationModuleForm moduleForm)
				{
					moduleForm.Closing += (s, e) =>
					{
						moduleGridCollection = (ActiveBusinessObjectCollection<CusReconEntry>)moduleForm.FindSingle<ZFilterStripControl>().GridCollection;
					};
				}
			});

			using (var form = new MonthlyClosingForm(reconDeclaration))
			{
				var linkSimplifiedDeclarationsMenuItem = GetLinkSimplifiedDeclarationsMenu(form);
				linkSimplifiedDeclarationsMenuItem.PerformClick();
				assertions.Invoke(moduleGridCollection);
			}
		}

		MenuItem GetLinkSimplifiedDeclarationsMenu(MonthlyClosingForm form)
		{
			var actionsMenu = form.Menu.MenuItems.FindByText("Actions");
			return actionsMenu.MenuItems.FindByText("Link Simplified Declarations");
		}
	}
}
