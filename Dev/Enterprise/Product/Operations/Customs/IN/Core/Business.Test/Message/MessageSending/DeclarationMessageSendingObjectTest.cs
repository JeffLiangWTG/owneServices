using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DeclarationMessageSendingObject))]
sealed class DeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageTypeAtributes()
	{
		var messageSendingObject = CreateObjectForTest();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(messageSendingObject.MessageTypeInfo);
		AssertEquals("MessageType Caption", "Message Type", resourceStringData.Caption);
		AssertEquals("MessageType is editable", expected: false, messageSendingObject.MessageTypeInfo.ReadOnly);
	}

	public void TestLocalReferenceNumberDate()
	{
		var entryHeader = CreateEntryHeaderWithInstruction();
		var messageSendingObject = new DeclarationMessageSendingObject(entryHeader);

		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(messageSendingObject.LocalReferenceNumberDateInfo);
		AssertEquals("ReferenceNumberDate Caption", "LRN Date", resourceStringData.Caption);

		entryHeader.CH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
		AssertEquals("ReferenceNumberDate is set from CusEntryInstruction", ZDateTime.BrettsBirthday.ToLocalBranchTime(), messageSendingObject.LocalReferenceNumberDate);
	}

	public void TestShippingBill()
	{
		var entryHeader = CreateEntryHeaderWithInstruction();
		var messageSendingObject = new DeclarationMessageSendingObject(entryHeader);

		CaptionTestHelper.AssertCaptions(messageSendingObject.ShippingBillNumberInfo, "SB No.", "SB No.", "SB No.");

		AssertEquals("ShippingBillNumber is set from CusEntryInstruction", "123", messageSendingObject.ShippingBillNumber);
	}

	public void TestShippingBillDate()
	{
		var entryHeader = CreateEntryHeaderWithInstruction();
		var messageSendingObject = new DeclarationMessageSendingObject(entryHeader);

		CaptionTestHelper.AssertCaptions(messageSendingObject.ShippingBillDateInfo, "SB No.Date.", "SB Dt.", "SB Dt.");

		AssertEquals("ShippingBillDate is set from CusEntryInstruction", ZDateTime.BrettsBirthday.Date, messageSendingObject.ShippingBillDate);
	}

	public void TestDescription()
	{
		var entryHeader = CreateEntryHeaderWithInstruction();
		var messageSendingObject = new DeclarationMessageSendingObject(entryHeader);

		CaptionTestHelper.AssertCaptions(messageSendingObject.DescriptionInfo, "Description", "Desc.", "Desc.");
		AssertEquals("Description is set from CusEntryInstruction", "Test Description", messageSendingObject.Description);
	}

	public void TestCustomsHouse()
	{
		var entryHeader = CreateEntryHeaderWithInstruction();
		var messageSendingObject = new DeclarationMessageSendingObject(entryHeader);

		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(messageSendingObject.CustomsHouseInfo);
		AssertEquals("CustomsHouse Caption", "Customs House", resourceStringData.Caption);

		entryHeader.Declaration.JE_CustomsOffice = "123";
		AssertEquals("CustomsHouse is set from Declaration", "123", messageSendingObject.CustomsHouse);
	}

	public void TestLookups()
	{
		var messageSendingObject = CreateObjectForTest();
		AssertType<DeclarationMessageSendingObjectLookups>(messageSendingObject.Lookups);
	}

	CusEntryHeader CreateEntryHeaderWithInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		instruction.ShippingBillNumber = "123";
		instruction.ShippingBillDate = ZDateTime.BrettsBirthday.Date;
		instruction.CEI_Description = "Test Description";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		return entryHeader;
	}

	public void TestValidation()
	{
		AssertType<DeclarationMessageSendingObjectValidation>(CreateObjectForTest().Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => CreateObjectForTest();

	DeclarationMessageSendingObject CreateObjectForTest() => new(Factory.New<CusEntryHeader>());
}
