using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class FinalPriceReportByDateExtensionLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWithMessageErrorIfNotEnteredInImportDeclarationNumber()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			var parentLine = parent.FinalPriceReportByDateExtensionLines.AddNew();
			parentLine.ImportDeclarationNumber = ZString.Empty;
			AssertHasErrorContaining(parentLine.ImportDeclarationNumberInfo, MandatoryValidation.MustBeEntered);

			parentLine.ImportDeclarationNumber = "Test";
			AssertNoErrors(parentLine.ImportDeclarationNumberInfo);
		}

		public void TestWithMessageErrorIfNotEnteredInExtensionDate()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			var parentLine = parent.FinalPriceReportByDateExtensionLines.AddNew();
			parentLine.ExtensionDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(parentLine.ExtensionDateInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine.ExtensionDate = new ZDateTime(2021, 07, 07);
			AssertNoMessageErrorContaining(parentLine.ExtensionDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInApplicationReason()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			var parentLine = parent.FinalPriceReportByDateExtensionLines.AddNew();
			parentLine.ApplicationReason = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine.ApplicationReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine.ApplicationReason = "연장신청사유";
			AssertNoMessageErrorContaining(parentLine.ApplicationReasonInfo, MandatoryValidation.YouHaveNotEntered);
			Assert("Korean characters should be allowed", !parent.HasErrors);
		}
	}
}
