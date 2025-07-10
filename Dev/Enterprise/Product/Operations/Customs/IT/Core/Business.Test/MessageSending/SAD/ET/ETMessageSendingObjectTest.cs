using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class ETMessageSendingObjectAbstactTest<T> : SADMessageSendingObjectTest<T>
	where T : ETMessageSendingObject
{
	public void TestHeader()
	{
		var messageSendingObjectAsInterface = messageSendingObject as IETMessageSendingObject;
		CombineAssertions(() =>
		{
			AssertNotNull(messageSendingObjectAsInterface.MessageHeader);
			AssertEquals(GetHeaderWrapperType(), messageSendingObjectAsInterface.MessageHeader.GetType());
		});
	}

	public void TestLines()
	{
		var messageSendingObjectAsInterface = messageSendingObject as IETMessageSendingObject;

		AssertNotNull("Lines should not be null", messageSendingObjectAsInterface.MessageLines);
		AssertEquals("Lines type", GetLineWrapperType(), messageSendingObjectAsInterface.MessageLines.First().GetType());
	}

	public override void TestCombinedCustomsMessageSubType()
	{
		AssertEquals("CombinedCustomsMessageSubType", GetMessageSubType(), messageSendingObject.CombinedCustomsMessageSubType);

		var paDocument1 = Declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = ZDateTime.Now;
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = Declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = Declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";

		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("CombinedCustomsMessageSubType", $"{GetMessageSubType()} + NB", messageSendingObject.CombinedCustomsMessageSubType);
	}

	public void TestCombinedCustomsMessageSubTypeNB()
	{
		Declaration.JE_MessageType = GetDeclarationMessageType();
		Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		var expectedMessageSubType = GetMessageSubType();

		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("Pre: CombinedCustomsMessageSubType", expectedMessageSubType, messageSendingObject.CombinedCustomsMessageSubType);

		var previousDocument1 = Declaration.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		previousDocument1.CSI_ReferenceNumber = "1234";

		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("When procedure A3, CombinedCustomsMessageSubType", expectedMessageSubType, messageSendingObject.CombinedCustomsMessageSubType);

		var previousDocument2 = Declaration.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "T1";
		previousDocument2.CSI_ReferenceNumber = "134";

		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("When procedures A3 and T1, CombinedCustomsMessageSubType", $"{expectedMessageSubType} + NB", messageSendingObject.CombinedCustomsMessageSubType);

		Declaration.PreviousDocuments.RemoveAndDeleteAll();
		previousDocument1 = Declaration.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "T2";
		previousDocument1.CSI_ReferenceNumber = "1234";
		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("When procedure T2, CombinedCustomsMessageSubType", expectedMessageSubType, messageSendingObject.CombinedCustomsMessageSubType);
	}

	public void TestFountainProvider()
	{
		var sadValuesProvider = messageSendingObject as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var fountainProvider = sadValuesProvider.FountainProvider;
		AssertType<JobDeclarationFountainProvider>("FountainProvider", fountainProvider);
		AssertSame("FountainProivder cached", fountainProvider, sadValuesProvider.FountainProvider);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);

	protected virtual Type GetHeaderWrapperType() => typeof(ETHeaderWrapper);

	protected virtual Type GetLineWrapperType() => typeof(ETLineWrapper);

	protected virtual ZString GetDeclarationMessageType() => "EXP";

	protected override void SetUp()
	{
		base.SetUp();
		Declaration.JE_MessageType = GetDeclarationMessageType();
		messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
	}

	T messageSendingObject;

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.ET;

	protected override ZString ExpectedMessageSubType => GetMessageSubType();
}

[TestedType(typeof(ETMessageSendingObject))]
sealed class ETMessageSendingObjectTestBaseOnlyTest : ETMessageSendingObjectAbstactTest<ETMessageSendingObject>
{
	public void TestMessageSendingObjectOverrides()
	{
		Declaration.JE_CustomsProfile = "1234";
		Declaration.JE_GS_NKCusAgent = "CCC";
		Declaration.JE_CustomsOffice = "IT000000";

		var messageSendingObject = new ETMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent) as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var customsMessages = messageSendingObject.GetCustomsMessageObjects();
		AssertEquals("Message objects to serialize count", 1, customsMessages.Count());
		AssertType<ETMessage>("Message object to serialize", customsMessages.Single());
	}

	protected override ETMessageSendingObject GetNewSADMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
	{
		return new ETMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
	}

	protected override ETMessageSendingObject GetNewSADMessageSendingObjectForDeterminingMessageChangedStatus(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
	{
		return new ETMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent, true);
	}

	class ETMessageSendingObjectForTest : ETMessageSendingObject
	{
		public ETMessageSendingObjectForTest(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(entryHeader, jobDeclarationMessageSendingObjectParent)
		{
		}
	}
}
