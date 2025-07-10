using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(IMMessageSendingObject))]
sealed class IMMessageSendingObjectTest : SADMessageSendingObjectTest<IMMessageSendingObject>
{
	public void TestHeader()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(imMessageSendingObject.MessageHeader);
			if (imMessageSendingObject.Header.EntryInstruction.HasIntoWarehouseProcedure)
			{
				AssertEquals(typeof(IMWarehouseHeaderProcedureWrapper), imMessageSendingObject.MessageHeader.GetType());
			}
			else
			{
				AssertEquals(typeof(IMNonWarehouseHeaderProcedureWrapper), imMessageSendingObject.MessageHeader.GetType());
			}
		});
	}

	public void TestLines()
	{
		AssertNotNull("Lines should not be null", imMessageSendingObject.MessageLines);

		var expectedMessageLinesType = imMessageSendingObject.Header.EntryInstruction.HasIntoWarehouseProcedure ? typeof(IMNonWarehouseLineProcedureCodeDependentFieldWrapper) : typeof(IMNonWarehouseLineProcedureCodeDependentFieldWrapper);
		AssertType("Lines type", expectedMessageLinesType, imMessageSendingObject.MessageLines.First());
	}

	public void TestMessageSendingObjectOverrides()
	{
		Declaration.JE_CustomsProfile = "1234";
		Declaration.JE_GS_NKCusAgent = "BBB";
		Declaration.JE_CustomsOffice = "IT000000";

		var messageSendingObject = new IMMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent) as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var customsMessages = messageSendingObject.GetCustomsMessageObjects();
		AssertEquals("Message objects to serialize count", 1, customsMessages.Count());
		AssertType<IMMessage>("Message object to serialize", customsMessages.Single());
	}

	public override void TestStatusAllowSending()
	{
		base.TestStatusAllowSending();

		EntryHeader.CH_Status = "AWO";
		EntryHeader.CH_EntryStatus = "UCL";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: UCL] StatusAllowSending", false, imMessageSendingObject.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "DRQ";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: DRQ] StatusAllowSending", false, imMessageSendingObject.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "INS";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: INS] StatusAllowSending", false, imMessageSendingObject.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "SCC";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: SCC] StatusAllowSending", false, imMessageSendingObject.StatusAllowsSending);

		EntryHeader.CH_Status = "CLO";
		EntryHeader.CH_EntryStatus = "ICC";
		AssertEquals("[CHStatus: CLO, CH_EntryStatus: ICC] StatusAllowSending", false, imMessageSendingObject.StatusAllowsSending);
	}

	public void TestFountainProvider()
	{
		var sadValuesProvider = imMessageSendingObject as ISadOutgoingCustomsMessageGeneratorValuesProvider;
		var fountainProvider = sadValuesProvider.FountainProvider;
		AssertType<JobDeclarationFountainProvider>("FountainProvider", fountainProvider);
		AssertSame("FountainProivder cached", fountainProvider, sadValuesProvider.FountainProvider);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Declaration.JE_MessageType = "IMP";
		imMessageSendingObject = new IMMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
	}

	IMMessageSendingObject imMessageSendingObject;

	protected override BusinessObject GetNewBusinessObject() => GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);

	protected override IMMessageSendingObject GetNewSADMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
	{
		return new IMMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
	}

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.IM;

	protected override IMMessageSendingObject GetNewSADMessageSendingObjectForDeterminingMessageChangedStatus(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
	{
		return new IMMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent, isForDeterminingMessageChangedStatus: true);
	}

	protected override ZString ExpectedMessageSubType => GetMessageSubType();
}

public class IMMessageSendingObjectForTest : IMMessageSendingObject
{
	public IMMessageSendingObjectForTest(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(entryHeader, jobDeclarationMessageSendingObjectParent)
	{
	}
}
