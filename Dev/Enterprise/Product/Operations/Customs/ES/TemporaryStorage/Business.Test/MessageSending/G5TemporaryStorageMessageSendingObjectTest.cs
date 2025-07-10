using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

[TestedType(typeof(G5TemporaryStorageMessageSendingObject))]
public class G5TemporaryStorageMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestReadOnlyProperties()
	{
		testItem.ShouldSend = true;
		CombineAssertions(() =>
		{
			AssertEquals("LRN is readonly", true, testItem.LRNInfo.ReadOnly);
			AssertEquals("MRN is readonly", true, testItem.MRNInfo.ReadOnly);
			AssertEquals("MessageSubType is readonly", true, testItem.MessageSubTypeInfo.ReadOnly);
			AssertEquals("EntryStatus is readonly", true, testItem.EntryStatusInfo.ReadOnly);
			AssertEquals("MessageStatus is readonly", true, testItem.MessageStatusInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.CustomsStatus = ZString.Empty;
			AssertEquals("MessageType is readonly when G5X and Customs Status is not CLR", true, testItem.MessageTypeInfo.ReadOnly);

			header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
			AssertEquals("MessageType is not readonly when G5X and Customs Status is CLR", false, testItem.MessageTypeInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.CustomsStatus = ZString.Empty;
			AssertEquals("MessageType is readonly when G5P and Customs Status is empty", true, testItem.MessageTypeInfo.ReadOnly);
		});
	}

	public void TestPropertiesSetFromHeader()
	{
		header.CustomsStatus = "CAN";
		header.AMA_MessageStatus = "ACC";
		header.LRN = "refNum";
		header.MRN = "AH3MRN";

		CombineAssertions(() =>
		{
			testItem = new G5TemporaryStorageMessageSendingObject(header);
			AssertEquals("CustomsStatus", "CAN", testItem.EntryStatus);
			AssertEquals("MessageStatus", "ACC", testItem.MessageStatus);
			AssertEquals("LRN", "refNum", testItem.LRN);
			AssertEquals("MRN", "AH3MRN", testItem.MRN);
		});
	}

	public void TestGetDefaultMessageType()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		header.CustomsStatus = ZString.Empty;
		testItem = new G5TemporaryStorageMessageSendingObject(header);
		AssertEquals("MessageType is defaulted to G5X when G5X and Customs Status is not CLR", DeclarationMessageTypeList.Codes.G5v1Expedition, testItem.MessageType);

		header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
		testItem = new G5TemporaryStorageMessageSendingObject(header);
		AssertEquals("MessageType is defaulted to empty when G5X and Customs Status is CLR", ZString.Empty, testItem.MessageType);

		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		header.CustomsStatus = ZString.Empty;
		testItem = new G5TemporaryStorageMessageSendingObject(header);
		AssertEquals("MessageType is defaulted to G5P when G5P and Customs Status is empty", DeclarationMessageTypeList.Codes.G5v1Reception, testItem.MessageType);
	}

	public void TestDefaultMessageSubType()
	{
		AssertEquals("MessageSubType", "ORG", testItem.MessageSubType);
	}

	public void TestMessageTypeList()
	{
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.CustomsStatus = ZString.Empty;
			Factory.Save();
			testItem = new G5TemporaryStorageMessageSendingObject(header);
			var typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List is CodeDescriptionPairList when G5X and Customs Status is not CLR", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("Message Type List is not empty when G5X and Customs Status is not CLR", 1, typeList.Count);
			AssertContainsExactElementsInAnyOrder("Message Type List when G5X and Customs Status is not CLR contains only G5X",
				new ZString[] { DeclarationMessageTypeList.Codes.G5v1Expedition }, typeList.GetAllCodesZString());

			header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
			Factory.Save();
			testItem = new G5TemporaryStorageMessageSendingObject(header);
			typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List is CodeDescriptionPairList when G5X and Customs Status is CLR", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("MessageType is not empty when G5X and Customs Status is CLR", 2, typeList.Count);
			AssertContainsExactElementsInAnyOrder("Message Type List when G5X and Customs Status is CLR contains G5D and G5N",
				new ZString[] { DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation }, typeList.GetAllCodesZString());

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.CustomsStatus = ZString.Empty;
			Factory.Save();
			testItem = new G5TemporaryStorageMessageSendingObject(header);
			typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List is CodeDescriptionPairList when G5P and Customs Status is empty", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("MessageType is not empty when G5P and Customs Status is empty", 1, typeList.Count);
			AssertContainsExactElementsInAnyOrder("Message Type List when G5P and Customs Status is empty contains only G5P",
				new ZString[] { DeclarationMessageTypeList.Codes.G5v1Reception }, typeList.GetAllCodesZString());
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		return new G5TemporaryStorageMessageSendingObject(header);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		Factory.Save();

		testItem = new G5TemporaryStorageMessageSendingObject(header);
	}
	G5TemporaryStorageMessageSendingObject testItem;
	TemporaryStorageHeader header;
}
