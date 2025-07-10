using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationMiscMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckNewDate()
		{
			var entry = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			parent.ShouldSend = true;
			parent.NewDate = CargoWise.Types.ZDate.Empty;
			parent.Validation.ValidateNewDate();
			AssertHasMessageErrorContaining(parent.NewDateInfo, MandatoryValidation.YouHaveNotEntered);

			parent.NewDate = CargoWise.Types.ZDate.Today;
			parent.Validation.ValidateNewDate();
			AssertNoMessageErrorContaining(parent.NewDateInfo, MandatoryValidation.YouHaveNotEntered);

			var entry2 = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var entryNum = entry2.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "EXP";
			entryNum.CE_ExpiryDate = CargoWise.Types.ZDate.Today.AddDays(-1);

			var parent2 = new JobDeclarationMiscMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			parent2.ShouldSend = true;
			parent2.NewDate = CargoWise.Types.ZDate.Empty;
			parent2.Validation.ValidateNewDate();
			AssertHasMessageErrorContaining(parent2.NewDateInfo, MandatoryValidation.YouHaveNotEntered);

			parent2.NewDate = CargoWise.Types.ZDate.Today.AddDays(-2);
			parent2.Validation.ValidateNewDate();
			AssertHasErrorContaining(parent2.NewDateInfo, "The 'New Date' must be after the 'Current Date'.");

			parent2.NewDate = CargoWise.Types.ZDate.Today;
			parent2.Validation.ValidateNewDate();
			AssertNoMessageErrors(parent2.NewDateInfo);
			AssertNoErrors(parent2.NewDateInfo);

			var parent3 = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DKJ, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			parent3.ShouldSend = true;
			parent3.NewDate = CargoWise.Types.ZDate.Empty;
			parent3.Validation.ValidateNewDate();
			AssertNoMessageErrors(parent3.NewDateInfo);

			parent3.NewDate = CargoWise.Types.ZDate.Today;
			parent3.Validation.ValidateNewDate();
			AssertNoMessageErrors(parent3.NewDateInfo);
		}

		public void TestShouldSendFor5TM()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100m;

			var entry2 = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 200m;

			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original, x => x.CL_CustomsValue > 50m);
			var messageSendingObject2 = new JobDeclarationMiscMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original, x => x.CL_CustomsValue > 50m);
			messageSendingObject.ShouldSend = true;
			messageSendingObject2.ShouldSend = false;
			AssertHasErrorContaining(messageSendingObject.ShouldSendInfo, "There are no lines which are gold or its product. Please indicate so for at least one entry line.");
			AssertNoErrorContaining(messageSendingObject2.ShouldSendInfo, "There are no lines which are gold or its product. Please indicate so for at least one entry line.");
			messageSendingObject.MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
			messageSendingObject2.ValidateShouldSend();
			AssertNoErrorContaining(messageSendingObject.ShouldSendInfo, "There are no lines which are gold or its product. Please indicate so for at least one entry line.");
			AssertNoErrorContaining(messageSendingObject2.ShouldSendInfo, "There are no lines which are gold or its product. Please indicate so for at least one entry line.");
		}
	}
}
