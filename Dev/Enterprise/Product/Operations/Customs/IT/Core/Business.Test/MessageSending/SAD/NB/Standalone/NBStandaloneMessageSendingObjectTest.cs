using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(NBStandaloneMessageSendingObject))]
sealed class NBStandaloneMessageSendingObjectTest : SADMessageSendingObjectTest<NBStandaloneMessageSendingObject>
{
	public void TestMessageSendingObjectOverrides()
	{
		Declaration.JE_CustomsProfile = "1234";
		Declaration.JE_GS_NKCusAgent = "BBB";
		Declaration.JE_CustomsOffice = "IT000000";

		AddPreviousDocuments();

		Declaration.ResetApportionedPreviousDocuments();

		EntryHeader.CH_EntryStatus = "NBR";
		var entryLineApproved = EntryHeader.MergedLines.Cast<CusEntryLine>().First();
		entryLineApproved.ZG_NBStatus = "NBA";

		var entryLineRejected = EntryHeader.MergedLines.AddNew();
		entryLineRejected.ZG_NBStatus = "NBR";

		var entryLineSent = EntryHeader.MergedLines.AddNew();
		entryLineSent.ZG_NBStatus = "NBS";

		var messageSendingObject = GetNewNBStandaloneMessageSendingObjectForTest() as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var nbCustomsMessages = messageSendingObject.GetCustomsMessageObjects();
		AssertEquals("Number of Nb Customs Messages", 2, nbCustomsMessages.Count());
		foreach (var nbCustomsMessage in nbCustomsMessages)
		{
			AssertType<NBMessage>("Message object to serialize", nbCustomsMessage);
		}

		Declaration.PreviousDocuments.RemoveAndDeleteAll();
		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("Number of Nb Customs Messages", 0, nbCustomsMessages.Count());
	}

	public void TestNbMessageSendingObjects()
	{
		AddPreviousDocuments();

		Declaration.ResetApportionedPreviousDocuments();
		var messageSendingObject = GetNewNBStandaloneMessageSendingObjectForTest();

		AssertNotNull(nameof(NBStandaloneMessageSendingObject.NbMessageSendingObjects), messageSendingObject.NbMessageSendingObjects);
		AssertEquals("Number if Nb Message Sending Object", 1, messageSendingObject.NbMessageSendingObjects.Count());

		var nbMessageSendingObjects = messageSendingObject.NbMessageSendingObjects;
		AssertSame(nameof(NBStandaloneMessageSendingObject.NbMessageSendingObjects), nbMessageSendingObjects, messageSendingObject.NbMessageSendingObjects);
	}

	public override void TestCombinedCustomsMessageSubType()
	{
		AddPreviousDocuments();

		Declaration.ResetApportionedPreviousDocuments();
		var messageSendingObject = GetNewNBStandaloneMessageSendingObjectForTest();

		AssertEquals("CombinedCustomsMessageSubType", "NB", messageSendingObject.CombinedCustomsMessageSubType);
	}

	public void TestFountainProvider()
	{
		var messageSendingObject = GetNewNBStandaloneMessageSendingObjectForTest();

		var sadValuesProvider = messageSendingObject as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var fountainProvider = sadValuesProvider.FountainProvider;
		AssertType<JobDeclarationFountainProvider>("FountainProvider", fountainProvider);
		AssertSame("FountainProivder cached", fountainProvider, sadValuesProvider.FountainProvider);
	}

	NBStandaloneMessageSendingObject GetNewNBStandaloneMessageSendingObjectForTest() => new NBStandaloneMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);

	protected override ZString ExpectedMessageSubType => SADConstants.MessageSubTypes.NB;

	void AddPreviousDocuments()
	{
		var paDocument1 = Declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = Declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = Declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";
	}

	protected override NBStandaloneMessageSendingObject GetNewSADMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		=> new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);

	protected override NBStandaloneMessageSendingObject GetNewSADMessageSendingObjectForDeterminingMessageChangedStatus(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		=> new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent, isForDeterminingMessageChangedStatus: true);

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.NB;
}
