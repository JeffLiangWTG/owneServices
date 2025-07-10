using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionRunnerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAttachmentOptionsValidation()
		{
			Action.DocumentPivots.RemoveAll();
			Action.DocumentPivots.AddNew();
			Runner.DeliverDocumentsInOneEmail = true;
			AssertEquals("Single Attachment should be Default value", "SAT", runner.AttachmentOptions);
			AssertNoErrors(Runner.PrinterInfo);
		}

		public void TestPrinterValidation_WithDocuments()
		{
			Action.DocumentPivots.AddNew();
			Runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			Runner.Printer = ZGuid.Invalid;
			AssertHasErrors(Runner.PrinterInfo);
			Runner.Printer = ZGuid.NewZGuid();
			AssertHasErrors(Runner.PrinterInfo);
			Runner.Printer = ZGuid.Empty;
			AssertHasErrors(Runner.PrinterInfo);
			Runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
			Runner.Validation.ValidatePrinter();
			AssertNoErrors(Runner.PrinterInfo);
		}

		public void TestPrinterValidation_WithoutDocuments()
		{
			Runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			Runner.Printer = ZGuid.Invalid;
			AssertHasErrors(Runner.PrinterInfo);
			Runner.Printer = ZGuid.NewZGuid();
			AssertNoErrors(Runner.PrinterInfo);
			Runner.Printer = ZGuid.Empty;
			AssertNoErrors(Runner.PrinterInfo);
		}

		public void TestBulkDeliveryMethod()
		{
			Runner.BulkDeliveryMethod = "XXX";
			AssertHasErrors(Runner.BulkDeliveryMethodInfo);
			Runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			AssertNoErrors(Runner.BulkDeliveryMethodInfo);
			Runner.BulkDeliveryMethod = "";
			AssertHasErrors(Runner.BulkDeliveryMethodInfo);
		}

		public void TestCheckRecipientEmail()
		{
			Runner.OverrideRecipientEmail = true;

			Runner.Validation.ValidateRecipientEmail();
			AssertHasError(Runner.RecipientEmailInfo, "Please enter an Email.");

			Runner.RecipientEmail = "not valid address";
			AssertHasErrorContaining(Runner.RecipientEmailInfo, "Email Address is not valid");

			Runner.RecipientEmail = "test@test.test";
			AssertNoError(Runner.RecipientEmailInfo, "Please enter an Email.");
			AssertNoErrorContaining(Runner.RecipientEmailInfo, "Email Address is not valid");

			Runner.OverrideRecipientEmail = false;

			Runner.RecipientEmail = ZString.Empty;
			AssertNoError(Runner.RecipientEmailInfo, "Please enter an Email.");

			Runner.RecipientEmail = "not valid address";
			AssertNoErrorContaining(Runner.RecipientEmailInfo, "Email Address is not valid");
		}

		#region Implementation
		OperationalActionRunner Runner
		{
			get
			{
				return runner ?? (runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()));
			}
		}

		OperationalActionRunner runner;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
