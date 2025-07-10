using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DeclarationMessageSendingObjectValidation))]
public class DeclarationMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMessageType()
	{
		var messageSendingObject = new DeclarationMessageSendingObject(Header);
		messageSendingObject.ShouldSend = false;
		var expectedMessage = "SB Number and Date is required.";
		AssertNoError("When ShouldSend Unticked", messageSendingObject.MessageTypeInfo, expectedMessage);

		messageSendingObject.ShouldSend = true;
		AssertNoError("When Message type Empty", messageSendingObject.MessageTypeInfo, expectedMessage);

		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertNoError("When Message type is Fresh and SB Number and Date Empty", messageSendingObject.MessageTypeInfo, expectedMessage);

		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.GoodsRegistration;
		AssertHasError("When Message type is GoodsRegistration with Empty SB Number and Date", messageSendingObject.MessageTypeInfo, expectedMessage);

		Instruction.ShippingBillNumber = "123456";
		Instruction.ShippingBillDate = new CargoWise.Types.ZDateTime(2023, 10, 1);
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.GoodsRegistration;
		AssertNoError("When Message type is GoodsRegistration with SB Number and Date", messageSendingObject.MessageTypeInfo, expectedMessage);
	}

	CusEntryInstruction Instruction => instruction ??= GetInstruction();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryHeader Header => header ??= Factory.New<CusEntryHeader>();
	CusEntryHeader header;

	CusEntryInstruction GetInstruction()
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		header.CH_CEI_Instruction = entryInstruction.PK;
		return entryInstruction;
	}
}

