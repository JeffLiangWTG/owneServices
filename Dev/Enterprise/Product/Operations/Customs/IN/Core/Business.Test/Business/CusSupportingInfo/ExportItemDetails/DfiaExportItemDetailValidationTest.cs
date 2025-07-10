using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaExportItemDetailValidation))]
sealed class DfiaExportItemDetailValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_ReferenceNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_ReferenceNumberInfo);
	}

	public void TestCSI_DateOfIssue()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_DateOfIssueInfo);
	}

	public void TestCSI_ReferenceNumber2()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_ReferenceNumber2Info);
	}

	public void TestCSI_Quantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_QuantityInfo);
	}

	DfiaExportItemDetail SupportingInfo => supportingInfo ??= Factory.New<DfiaExportItemDetail>();
	DfiaExportItemDetail supportingInfo;
}
