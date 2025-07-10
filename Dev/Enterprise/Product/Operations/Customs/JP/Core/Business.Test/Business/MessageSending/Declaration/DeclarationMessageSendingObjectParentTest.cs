using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(DeclarationMessageSendingObjectParent))]
	public class DeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new DeclarationMessageSendingObjectParent(declaration);
		}

		public void TestSetDefaultValues()
		{
			using (var dir = new TempDirectory())
			using (JPRegistry.Instance.DefaultFolderForExportingMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dir.DirectoryName))
			{
				var defaultFolderForExportingMessages = JPRegistry.Instance.DefaultFolderForExportingMessages.Value;
				AssertEquals(dir.DirectoryName, defaultFolderForExportingMessages);

				var declaration = Factory.New<JobDeclaration>();
				var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
				AssertEquals(defaultFolderForExportingMessages, messageSendingObjectParent.ExportPath);
			}
		}

		public void TestExportPath()
		{
			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());
			var data = DataBoundResourceStrings.GetDataForProperty(messageSendingObjectParent.ExportPathInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Export To", data.Caption);
				AssertEquals("FullDescription", $"The folder the message will be exported to. This can be set in Registry -> {JPRegistry.Instance.DefaultFolderForExportingMessages.GetLocationInEnglish()}", data.FullDescription);
			});
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var messageSendingObjectParent1 = new DeclarationMessageSendingObjectParent(declaration1);
			AssertEquals(0, messageSendingObjectParent1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, messageSendingObjectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration2.CustomsEntryInstructions.AddNew();
			var header1 = declaration2.CustomsEntryHeaders.AddNew();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header1.CH_CEI_Instruction = entryInstruction.PK;
			header2.CH_CEI_Instruction = entryInstruction.PK;
			var messageSendingObjectParent2 = new DeclarationMessageSendingObjectParent(declaration2);
			AssertEquals(2, messageSendingObjectParent2.SendingObjectsCollection.Count);
		}

		public void TestBizObjValidationMessageErrors()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateJE_TransportMode();

			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;
			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();

			messageSendingObject.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.BizObjValidationMessageErrors);

			messageSendingObject.ShouldSend = true;
			AssertContains("Test message error on JE_TransportModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
		}

		public void TestBizObjValidationMessageErrorsAndAdditionalWarnings_ECRMessage()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.CreateMessageErrorForTest = true;
			declaration.CreateWarningForTest = true;
			declaration.Validation.ValidateJE_TransportMode();

			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);

			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();

			messageSendingObject.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.BizObjValidationMessageErrors);

			messageSendingObject.ShouldSend = true;
			AssertContains("Test message error on JE_ReceiptModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
			AssertNotContains("Test message error on JE_TransportModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
			AssertContains("Test warning on JE_ReceiptModeInfo.", messageSendingObjectParent.AdditionalWarnings);
			AssertNotContains("Test warning on JE_TransportModeInfo.", messageSendingObjectParent.AdditionalWarnings);

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA;
			messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();

			messageSendingObject.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.BizObjValidationMessageErrors);

			messageSendingObject.ShouldSend = true;
			AssertNotContains("Test message error on JE_ReceiptModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
			AssertContains("Test message error on JE_TransportModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
			AssertNotContains("Test warning on JE_ReceiptModeInfo.", messageSendingObjectParent.AdditionalWarnings);
			AssertContains("Test warning on JE_TransportModeInfo.", messageSendingObjectParent.AdditionalWarnings);
		}

		public void TestSendingECRMessageWithActionIsOne()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR, Action = ActionList.Codes.One };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);

			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Action", ActionList.Codes.One, messageSendingObject.Action);
				Assert("ShouldSend", messageSendingObject.ShouldSend);
			});
		}

		public void TestGetEntryHeadersToBeSent()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			header1.CH_CEI_Instruction = instruction1.PK;
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			header2.CH_CEI_Instruction = instruction2.PK;

			var messageSendingContext = new MessageSendingContext()
			{
				ProcedureCode = JPProcedureCodeList.Codes.ECR,
				Action = ActionList.Codes.One,
				EntryHeadersToBeSent = [header2]
			};

			declaration.SetCurrentMessageSendingContext(messageSendingContext);

			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, messageSendingObject.Count);
				AssertSame("Should be header2", header2, messageSendingObject.Single().Header);
			});
		}

		[TestDate(2024, 8, 23)]
		public void TestAddtionalWarnings()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.CreateWarningForTest = true;
			declaration.Validation.ValidateJE_TransportMode();

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_JE = declaration.PK;
			instruction1.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			header1.CH_CEI_Instruction = instruction1.PK;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CreateWarningForTest = true;
			instruction2.CEI_JE = declaration.PK;
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			header2.CH_CEI_Instruction = instruction2.PK;

			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().ElementAt(0);
			var messageSendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().ElementAt(1);
			messageSendingObject1.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.AdditionalWarnings);

			messageSendingObject1.ShouldSend = true;
			var additionalWarnings = messageSendingObjectParent.AdditionalWarnings;
			AssertContains("Test warning on JE_TransportModeInfo.", additionalWarnings);
			AssertNotContains("Test warning on CEI_DateForDutyInfo.", additionalWarnings);

			messageSendingObject2.ShouldSend = true;
			additionalWarnings = messageSendingObjectParent.AdditionalWarnings;
			AssertContains("Test warning on JE_TransportModeInfo.", additionalWarnings);
			AssertContains("Test warning on CEI_DateForDutyInfo.", additionalWarnings);

			messageSendingObject1.ProcedureCode = JPProcedureCodeList.Codes.EDA;
			AssertNotContains("Scheduled Declaration Date is different from the current date.", messageSendingObjectParent.AdditionalWarnings);

			messageSendingObject1.ProcedureCode = JPProcedureCodeList.Codes.EDC;
			AssertContains("Scheduled Declaration Date is different from the current date.", messageSendingObjectParent.AdditionalWarnings);

			instruction1.CEI_DateForDuty = ZDateTime.Today;
			messageSendingObject1.ProcedureCode = JPProcedureCodeList.Codes.EDE;
			AssertNotContains("Scheduled Declaration Date is different from the current date.", messageSendingObjectParent.AdditionalWarnings);
		}

		public void TestAllowSendWithError()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateAll();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;
			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as MessageSendingObject;

			messageSendingObject.ShouldSend = true;
			Assert(!messageSendingObjectParent.AllowSendWithErrorInfo.ReadOnly);
			Assert(!messageSendingObjectParent.AllowSendWithError);

			messageSendingObjectParent.AllowSendWithError = true;
			messageSendingObject.ShouldSend = false;
			Assert(!messageSendingObjectParent.AllowSendWithError);
			Assert(messageSendingObjectParent.AllowSendWithErrorInfo.ReadOnly);
		}

		public void TestRunPreSaveValidationInConstructor()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			declaration.CreateMessageErrorForTest = true;
			declaration.CreateWarningForTest = true;
			var targetInfo = declaration.JE_TransportModeInfo;
			CombineAssertions(() =>
			{
				AssertNoMessageErrors(targetInfo);
				AssertNoWarnings(targetInfo);
			});

			new DeclarationMessageSendingObjectParent(declaration);
			CombineAssertions(() =>
			{
				AssertHasMessageError(targetInfo, "Test message error on JE_TransportModeInfo.");
				AssertHasWarning(targetInfo, "Test warning on JE_TransportModeInfo.");
			});
		}

		public void TestRefreshValidation()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;
			var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			declaration.CreateMessageErrorForTest = true;
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as MessageSendingObject;
			declaration.MarkAsNeedingValidation();
			messageSendingObject.ShouldSend = true;
			AssertNotContains("Test message error on JE_TransportModeInfo.", messageSendingObjectParent.BizObjValidationMessageErrors);
		}
	}
}
