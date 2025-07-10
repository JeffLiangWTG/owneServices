using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComplaintMessageSendingObjectLookupTest : TestCaseWithFactory
{
	public void TestCorrectionReasonList()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);

		var list = lookups.CorrectionReasonList;

		CombineAssertions(() =>
		{
			AssertEquals("Valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidCorrectionReasonTypeCode));
			AssertEquals("Invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		sendingObject = new EComplaintMessageSendingObject(entryHeader);
		lookups = sendingObject.Lookups;
	}
	CusEntryHeader entryHeader;
	EComplaintMessageSendingObject sendingObject;
	EComplaintMessageSendingObjectLookups lookups;
}
