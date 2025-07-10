using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class Ucc6JobDeclarationMessageSendingObjectLookups_MessageTypeListTest : TestCaseWithFactory
{
	public void TestBuilderConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new Ucc6SendingObjectMessageTypeListBuilder(entryHeader: null));
	}

	public void TestMessageTypeList_NewDeclarationCode()
	{
		CombineAssertions(() =>
		{
			entryHeader.CH_EntryStatus = "";
			AssertContainsNewDeclarationMessageType(expectedContainsCode: true);

			entryHeader.CH_EntryStatus = "AMG";
			AssertContainsNewDeclarationMessageType(expectedContainsCode: false);
		});
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryDoesNotHaveMrn()
	{
		entryHeader.MovementReferenceNumberSetter("");
		AssertContainsCancellationMessageType(expectedContainsCode: false);
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsRegistered()
	{
		entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
		entryHeader.CH_EntryStatus = "REG";
		AssertContainsCancellationMessageType(expectedContainsCode: true);
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsCleared()
	{
		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
			entryHeader.CH_EntryStatus = "ICC";
			AssertContainsCancellationMessageType(expectedContainsCode: true);

			entryHeader.CH_EntryStatus = "ECC";
			AssertContainsCancellationMessageType(expectedContainsCode: true);
		});
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsUnderControl()
	{
		entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
		entryHeader.CH_EntryStatus = "UCL";
		AssertContainsCancellationMessageType(expectedContainsCode: true);
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsDeposited()
	{
		entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
		entryHeader.CH_EntryStatus = "DEP";
		AssertContainsCancellationMessageType(expectedContainsCode: true);
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsCanceling()
	{
		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
			entryHeader.CH_EntryStatus = "CNG";
			entryHeader.CH_Status = "";
			AssertContainsCancellationMessageType(expectedContainsCode: false);

			entryHeader.CH_Status = "FFT";
			AssertContainsCancellationMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "ERO";
			AssertContainsCancellationMessageType(expectedContainsCode: true);
		});
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsAmending()
	{
		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
			entryHeader.CH_EntryStatus = "AMG";
			entryHeader.CH_Status = "XYZ";
			AssertContainsCancellationMessageType(expectedContainsCode: false);

			entryHeader.CH_Status = "";
			AssertContainsCancellationMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "FFT";
			AssertContainsCancellationMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "ERO";
			AssertContainsCancellationMessageType(expectedContainsCode: true);
		});
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnAndEntryStatusIsAmended()
	{
		entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
		entryHeader.CH_EntryStatus = "AMD";
		AssertContainsCancellationMessageType(expectedContainsCode: true);
	}

	public void TestMessageTypeList_CancellationCode_WhenEntryHasMrnButEntryStatusDoesNotAllow()
	{
		entryHeader.MovementReferenceNumberSetter("23ITQSS080005404T7");
		entryHeader.CH_EntryStatus = "";
		AssertContainsCancellationMessageType(expectedContainsCode: false);
	}

	public void TestMessageTypeList_AmendmentCode()
	{
		CombineAssertions(() =>
		{
			entryHeader.CH_EntryStatus = "AMG";
			entryHeader.CH_Status = "XYZ";
			AssertContainsAmendmentMessageType(expectedContainsCode: false);

			entryHeader.CH_Status = "";
			AssertContainsAmendmentMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "FFT";
			AssertContainsAmendmentMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "ERO";
			AssertContainsAmendmentMessageType(expectedContainsCode: true);

			entryHeader.CH_Status = "ACO";
			AssertContainsAmendmentMessageType(expectedContainsCode: false);
		});
	}

	public void TestMessageTypeListIsCached()
	{
		var sendingObject = GetNewSendingObject();
		var messageTypeList = sendingObject.Lookups.MessageTypeList;
		AssertSame("Result is cached", messageTypeList, sendingObject.Lookups.MessageTypeList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;

	void AssertContainsCancellationMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "CAN");

	void AssertContainsNewDeclarationMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "NEW");

	void AssertContainsAmendmentMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "AMD");

	void AssertContainsCode(bool expectedContainsCode, string codeBeingTested)
	{
		var sendingObject = GetNewSendingObject();
		var assertionMessage = $"When CH_EntryStatus = '{entryHeader.CH_EntryStatus}', CH_Status = '{entryHeader.CH_Status}', contains '{codeBeingTested}'?";
		AssertEquals(assertionMessage, expectedContainsCode, sendingObject.Lookups.MessageTypeList.ContainsCode(codeBeingTested));
	}

	Ucc6JobDeclarationMessageSendingObject GetNewSendingObject()
	{
		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
		return new Ucc6MessageSendingObjectForTest(entryHeader, sendingObjectParent);
	}
}
