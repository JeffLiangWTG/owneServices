using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComplaintMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckCorrectionReasonCode()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.CorrectionReasonInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(sendingObject.CorrectionReasonInfo, RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode, RefCusCodeTestHelper.ValidCorrectionReasonTypeCode);
		});
	}

	public void TestCheckSendingObjectLineCount()
	{
		const string message = "At least one line is required.";

		CombineAssertions(() =>
		{
			sendingObject.Validation.ValidateAll();
			AssertHasRowError("No lines", sendingObject, message);

			sendingObject.SendingObjectLines.AddNew();
			sendingObject.Validation.ValidateAll();
			AssertNoRowError("With lines", sendingObject, message);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		sendingObject = new EComplaintMessageSendingObject(entryHeader);
	}
	CusEntryHeader entryHeader;
	EComplaintMessageSendingObject sendingObject;
}
