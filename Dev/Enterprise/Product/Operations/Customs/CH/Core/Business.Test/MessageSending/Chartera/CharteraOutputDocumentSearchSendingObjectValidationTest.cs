using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchSendingObjectValidation))]
class CharteraOutputDocumentSearchSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckCreationTimeFrom() => CombineAssertions(() =>
	{
		const string message1 = "Creation Time From should be earlier than Creation Time To.";
		const string message2 = "The time range cannot exceed 24 hours.";

		ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.CreationTimeFromInfo);

		sendingObject.CreationTimeTo = new ZDateTime(2023, 1, 10, 12, 0, 0);

		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 10, 12, 0, 0);
		AssertHasError("From >= To", sendingObject.CreationTimeFromInfo, message1);
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 10, 11, 59, 59);
		AssertNoError("From < To", sendingObject.CreationTimeFromInfo, message1);

		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 9, 11, 59, 59);
		AssertHasError("To - From > 24h", sendingObject.CreationTimeFromInfo, message2);
		sendingObject.CreationTimeFrom = new ZDateTime(2023, 1, 9, 12, 0, 0);
		AssertNoError("To - From <= 24h", sendingObject.CreationTimeFromInfo, message2);
	});

	public void TestCheckCreationTimeTo()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.CreationTimeToInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.CreationTimeFromInfo.HumanReadableName));
	}

	[TestDate(2023, 7, 1, 12, 0, 0)]
	public void TestIsHistoricalQuery() => CombineAssertions(() =>
	{
		const string message1 = "Please tick Historical Query to request documents created earlier than 1 month ago.";
		const string message2 = "Creation Time To should be earlier than 1 month ago if Historical Query is ticked.";

		sendingObject.CreationTimeFrom = new ZDateTime(2023, 6, 1, 12, 0, 0);
		sendingObject.IsHistoricalQuery = ZBool.False;
		AssertHasError(sendingObject.IsHistoricalQueryInfo, message1);

		sendingObject.IsHistoricalQuery = ZBool.True;
		AssertNoError(sendingObject.IsHistoricalQueryInfo, message1);

		sendingObject.CreationTimeFrom = new ZDateTime(2023, 6, 1, 12, 0, 1);
		sendingObject.IsHistoricalQuery = ZBool.False;
		AssertNoError(sendingObject.IsHistoricalQueryInfo, message1);

		sendingObject.CreationTimeTo = new ZDateTime(2023, 6, 1, 12, 0, 1);
		sendingObject.IsHistoricalQuery = ZBool.True;
		AssertHasError(sendingObject.IsHistoricalQueryInfo, message2);

		sendingObject.IsHistoricalQuery = ZBool.False;
		AssertNoError(sendingObject.IsHistoricalQueryInfo, message2);

		sendingObject.CreationTimeTo = new ZDateTime(2023, 6, 1, 12, 0, 0);
		sendingObject.IsHistoricalQuery = ZBool.True;
		AssertNoError(sendingObject.IsHistoricalQueryInfo, message2);
	});

	protected override void SetUp()
	{
		base.SetUp();
		sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
	}

	CharteraOutputDocumentSearchSendingObject sendingObject;
}
