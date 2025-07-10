using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class FinalPriceReportByDateExtensionHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWithMessageErrorIfNotEnteredInCustomsOffice()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			parent.CustomsOffice = ZString.Empty;
			AssertHasErrorContaining(parent.CustomsOfficeInfo, MandatoryValidation.MustBeEntered);

			SetCusCodeData();
			parent.CustomsOffice = "X";
			AssertHasMessageErrorContaining(parent.CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			parent.CustomsOffice = "033";
			AssertNoErrorContaining(parent.CustomsOfficeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageError(parent.CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestWithMessageErrorIfNotEnteredInGlbBranch()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			parent.GB_Branch = ZGuid.Empty;
			AssertHasErrorContaining(parent.GB_BranchInfo, MandatoryValidation.MustBeEntered);

			AssertNoMessageErrorContaining(parent.GB_BranchInfo, "Please enter a valid branch.");

			parent.GB_Branch = new ZGuid("7F23B397-03C4-42AE-A3E8-EA47559E52A8");
			AssertHasMessageErrorContaining(parent.GB_BranchInfo, "Please enter a valid branch.");
			AssertNoErrorContaining(parent.GB_BranchInfo, MandatoryValidation.MustBeEntered);

			var declaration = Factory.New<JobDeclaration>();
			parent.GB_Branch = declaration.Branch.PK;
			AssertNoMessageErrorContaining(parent.GB_BranchInfo, "Please enter a valid branch.");
			AssertNoErrorContaining(parent.GB_BranchInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckHasExtensionLines()
		{
			var parent = new FinalPriceReportByDateExtensionHeader(Factory);
			parent.Validation.ValidateHasExtensionLines();
			AssertHasErrorContaining(parent.HasExtensionLinesInfo, "You must enter at least one line.");

			parent.FinalPriceReportByDateExtensionLines.AddNew();
			parent.Validation.ValidateHasExtensionLines();
			AssertNoErrors(parent.HasExtensionLinesInfo);
		}

		void SetCusCodeData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "033", "양산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();
		}
	}
}
