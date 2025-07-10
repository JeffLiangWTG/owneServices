using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWControlValidation))]
sealed class SWControlValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCheckCSI_ReferenceNumber2()
	{
		RefDataSetupTestHelper.SetupControlResultCode(Factory);
		var controlResultCodeList = SWControl.Lookups.ControlResultCodeList;
		controlResultCodeList.Load();
		AssertGreaterThan("Count", controlResultCodeList.Count, 0);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(
			SWControl.CSI_ReferenceNumber2Info,
			["X", "XX"],
			controlResultCodeList.OfType<ICodeDescription>().Select(x => new ZString(x.Code)).ToArray()
		);
	}

	public void TestCheckCSI_ControlLocation()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWControl.CSI_ControlLocationInfo);
	}

	public void TestCheckCSI_DateOfIssue()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWControl.CSI_DateOfIssueInfo);
	}

	public void TestCheckCSI_DateOfExpiry()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWControl.CSI_DateOfExpiryInfo);

		const string endDateBeforeStartDateError = "The Control End Date cannot be earlier than the Control Start Date.";

		SWControl.CSI_DateOfIssue = ZDateTime.Empty;
		SWControl.CSI_DateOfExpiry = ZDateTime.Empty;
		AssertNoMessageError(SWControl.CSI_DateOfExpiryInfo, endDateBeforeStartDateError);

		SWControl.CSI_DateOfIssue = ZDateTime.Today;
		AssertNoMessageError(SWControl.CSI_DateOfExpiryInfo, endDateBeforeStartDateError);

		SWControl.CSI_DateOfIssue = ZDateTime.Empty;
		SWControl.CSI_DateOfExpiry = ZDateTime.Today;
		AssertNoMessageError(SWControl.CSI_DateOfExpiryInfo, endDateBeforeStartDateError);

		SWControl.CSI_DateOfIssue = ZDateTime.Today;
		AssertNoMessageError(SWControl.CSI_DateOfExpiryInfo, endDateBeforeStartDateError);

		SWControl.CSI_DateOfExpiry = ZDateTime.Today.AddDays(-1);
		AssertHasMessageError(SWControl.CSI_DateOfExpiryInfo, endDateBeforeStartDateError);
	}

	SWControl SWControl => fSWControl ??= Factory.New<SWControl>();
	SWControl fSWControl;
}
