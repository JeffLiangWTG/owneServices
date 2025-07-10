using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsDepartureSadCustomsExitAndReleaseMessageProcessorTest<TMessageProcessor> : NctsDepartureSadIncomingCustomsMessageProcessorTest<TMessageProcessor>
	where TMessageProcessor : IncomingCustomsMessageProcessor<ISadCustomsLinkedObjectAdapter>
{
	protected abstract ZString ValidResponseMRN { get; }
	protected abstract ZString EntryType { get; }

	public abstract void TestIncomingCustomsMessageProcessing();

	public void TestIncomingCustomsMessageProcessing_EntryHeaderNotFound()
	{
		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var nctsHeader, var receivedEdiMessage, _) = SetupDepartureNctsHeaderAndGetDataForTest("284067", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "20ITQ3J08000387XXX";

		processor.ProcessMessage(receivedEdiMessage);
		AssertLoggerContainsLogText($"EntryHeader with MRN {ValidResponseMRN} couldn't be found.");
		AssertNull($"CusEntryNum with EntryType '{EntryType}' doesn't exists", GetCusEntryNumber(nctsHeader));
	}

	public void TestIncomingCustomsMessageProcessing_CusEntryNumAlreadyExists()
	{
		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var nctsHeader, var receivedEdiMessage, _) = SetupDepartureNctsHeaderAndGetDataForTest("284067", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ValidResponseMRN;

		var newCusEntryNumber = Factory.NewCusEntryNumber(nctsHeader, EntryType, ZString.Empty, ZDate.Today);
		processor.ProcessMessage(receivedEdiMessage);
		AssertLoggerContainsLogText("CusEntryNum already exists for Entry");
		AssertEquals($"There must be only 1 CusEntryNumber of EntryType '{EntryType}'", 1, Factory.Load<CusEntryNumber>(GetCusEntryNumQuery(nctsHeader)).Length);
		var cusEntryNumber = GetCusEntryNumber(nctsHeader);
		AssertEntryNumber(cusEntryNumber, newCusEntryNumber.CE_EntryType, newCusEntryNumber.CE_EntryNum, newCusEntryNumber.CE_Category, newCusEntryNumber.CE_EntryLineReference, newCusEntryNumber.CE_IssueDate);
	}

	protected CusEntryNumber GetCusEntryNumber(NctsHeader nctsHeader)
	{
		return Factory.LoadTop1<CusEntryNumber>(GetCusEntryNumQuery(nctsHeader));
	}

	ZQuery GetCusEntryNumQuery(NctsHeader nctsHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, EntryType);
		query.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
		return query;
	}
}
