using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadCustomsExitAndReleaseMessageProcessorTest<TMessageProcessor> : SadIncomingCustomsMessageProcessorTest<TMessageProcessor>
	where TMessageProcessor : IncomingCustomsMessageProcessor<ISadCustomsLinkedObjectAdapter>
{
	protected abstract ZString ValidResponseMRN { get; }
	protected abstract ZString EntryType { get; }

	public abstract void TestIncomingCustomsMessageProcessing();

	public void TestIncomingCustomsMessageProcessing_EntryHeaderNotFound()
	{
		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var entryHeader, var receivedEdiMessage, _) = SetupDeclarationAndGetDataForTest("284067", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		entryHeader.MovementReferenceNumberSetter("20ITQ3J08000387XXX");

		processor.ProcessMessage(receivedEdiMessage);
		AssertLoggerContainsLogText($"EntryHeader with MRN {ValidResponseMRN} couldn't be found.");
		AssertNull($"CusEntryNum with EntryType '{EntryType}' doesn't exists", GetCusEntryNumber(entryHeader));
	}

	public void TestIncomingCustomsMessageProcessing_CusEntryNumAlreadyExists()
	{
		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var entryHeader, var receivedEdiMessage, _) = SetupDeclarationAndGetDataForTest("284067", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		entryHeader.MovementReferenceNumberSetter(ValidResponseMRN);

		var newCusEntryNumber = entryHeader.EntryNumbersProvider.InsertOrUpdateEntryNum(EntryType, string.Empty, new ZDateTime(2020, 03, 01));
		newCusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

		processor.ProcessMessage(receivedEdiMessage);
		AssertLoggerContainsLogText("CusEntryNum already exists for Entry");

		AssertEquals($"There must be only 1 CusEntryNumber of EntryType '{EntryType}'", 1, Factory.Load<CusEntryNumber>(GetCusEntryNumQuery(entryHeader)).Length);
		var cusEntryNumber = GetCusEntryNumber(entryHeader);
		AssertEntryNumber(cusEntryNumber, newCusEntryNumber.CE_EntryType, newCusEntryNumber.CE_EntryNum, newCusEntryNumber.CE_Category, newCusEntryNumber.CE_EntryLineReference, newCusEntryNumber.CE_IssueDate);
	}

	protected CusEntryNumber GetCusEntryNumber(CusEntryHeader entryHeader)
	{
		return Factory.LoadTop1<CusEntryNumber>(GetCusEntryNumQuery(entryHeader));
	}

	ZQuery GetCusEntryNumQuery(CusEntryHeader entryHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, EntryType);
		query.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
		return query;
	}
}
