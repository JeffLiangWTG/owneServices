using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobWorkValidation))]
sealed class JobWorkValidatioinTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_DateOfIssue()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_DateOfIssueInfo);
	}

	public void TestCheckCSI_CustomsOffice()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_CustomsOfficeInfo);
	}

	public void TestCheckCSI_ReferenceNumber2()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_ReferenceNumber2Info);
	}

	public void TestCheckCSI_ItemNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_ItemNumberInfo);
	}

	public void TestCheckCSI_Quantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_QuantityInfo);
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobWork.CSI_UnitOfQuantityInfo);
	}

	JobWork JobWork => fJobWork ??= Factory.New<JobWork>();
	JobWork fJobWork;
}
