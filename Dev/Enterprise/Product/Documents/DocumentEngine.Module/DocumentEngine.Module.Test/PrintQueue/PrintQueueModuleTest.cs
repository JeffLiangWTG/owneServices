using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintQueueModule))]
	sealed class PrintQueueModuleTest : ZModuleBasherTest
	{
		public void TestIOperationalActionSupportable()
		{
			using (var module = new PrintQueueModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));

				var supportable = module as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertType<StmPrintQueueActionSupporter>(supportable.OperationalActionSupporter);
			}
		}

		protected override BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module)
		{
			var printer = (StmPrintQueue)base.GetBusinessObjectForHyperlinking(module);
			printer.SQ_DisplayName = "Reginald's Printer";
			return printer;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PrintQueue;
		}

		public void TestModuleID()
		{
			using (PrintQueueModule module = new PrintQueueModule())
			{
				AssertEquals("ModuleID", ModuleIDs.PrintQueue, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (PrintQueueModule module = new PrintQueueModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.PrintQueues, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (PrintQueueModuleForTest module = new PrintQueueModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is PrintQueueFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (PrintQueueModuleForTest module = new PrintQueueModuleForTest())
			{
				IBusinessObjectCollection printQueuesCollection = module.NewGridCollection;
				Assert("Invalid type", printQueuesCollection is StmPrintQueueCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (PrintQueueModuleForTest module = new PrintQueueModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is PrintQueueFilterBusinessObject);
			}
		}

		#endregion

		public void TestDeleteShouldBeShownForBothTypesOfQueues()
		{
			using (PrintQueueModuleForTest module = new PrintQueueModuleForTest())
			{
				StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
				Factory.Save();

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

				module.ShowDeleteFormForTesting(printQueue);
				ZController controller = ((IFilterGridModuleInternalsForTesting)module).LastController;
				AssertNotNull("Controller shouldn't be null", controller);
				AssertNotNull("Last Shown form should not be null - a delete form should be shown for an active queue", controller.LastShownForm);
				controller.LastShownForm.Dispose();

				printQueue.SQ_QueueDeleted = ZDateTime.Now;
				module.ShowDeleteFormForTesting(printQueue);
				controller = ((IFilterGridModuleInternalsForTesting)module).LastController;
				AssertNotNull("Last shown form is not null - a delete form should be shown for an inactive queue", controller.LastShownForm);
				controller.LastShownForm.Dispose();
			}
		}

		public void TestReplacePrintQueue()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			using (var module = new PrintQueueModuleForTest())
			{
				var formActionsMenu = module.FormActionMenu;
				AssertNotNull("Must have Replace Print Queue action", module.ActionsMenuItem.MenuItems.FindByText("Replace with another print queue"));

				using (var form = new ZForm())
				{
					const string expectedSecurityError =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Printing -> Print Queues -> Edit";

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var filterBO = (PrintQueueFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearchForTest();

					module.DisplayGrid.SelectAllElements();
					AssertEquals("GridCollection Should have 1 print queue selected", 1, module.DisplayGrid.SelectedElements.Length);

					Env.Security.PrintQueuesModify.IsAllowed = false;
					module.ReplacePrintQueueForTesting();
					AssertEquals(expectedSecurityError, UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.PrintQueuesModify.IsAllowed = true;
					module.ReplacePrintQueueForTesting();
					AssertType("The PrintQueueReplaceForm was not displayed when MenuItem clicked", typeof(PrintQueueReplaceForm), ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_QueueDeleted = ZDateTime.Now;

			return printQueue;
		}
	}
}
