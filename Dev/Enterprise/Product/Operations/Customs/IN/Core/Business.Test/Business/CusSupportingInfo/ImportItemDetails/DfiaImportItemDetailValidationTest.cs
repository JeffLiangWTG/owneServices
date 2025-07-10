using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaImportItemDetailValidation))]
sealed class DfiaImportItemDetailValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_ReferenceNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_ReferenceNumberInfo);
	}

	public void TestCSI_Quantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_QuantityInfo);
	}

	public void TestCSI_IssuerType()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_IssuerTypeInfo);
	}

	DfiaImportItemDetail SupportingInfo => supportingInfo ??= Factory.New<DfiaImportItemDetail>();
	DfiaImportItemDetail supportingInfo;
}
