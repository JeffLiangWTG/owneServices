using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class LegalActInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_DateOfIssue()
		{
			var legalAct = Factory.New<LegalActInfo>();
			legalAct.CSI_DateOfIssue = new ZDateTime(1899, 12, 31);
			AssertHasError("earlier than '1900-01-01", legalAct.CSI_DateOfIssueInfo, TypeValidation.ErrorForSmallDateTimePast(legalAct.CSI_DateOfIssue));
			legalAct.CSI_DateOfIssue = new ZDateTime(2079, 06, 07);
			AssertHasError("later than '2079-06-06'", legalAct.CSI_DateOfIssueInfo, TypeValidation.ErrorForSmallDateTimeFuture(legalAct.CSI_DateOfIssue));
			legalAct.CSI_DateOfIssue = ZDateTime.Today;
			AssertNoErrors("valid SmallDateTime", legalAct.CSI_DateOfIssueInfo);
			legalAct.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoErrors("empty SmallDateTime", legalAct.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			var legalAct = invoiceLine.LegalActInfos.AddNew();

			legalAct.CSI_ReferenceNumber = "XXX";
			AssertHasMessageError(legalAct.CSI_ReferenceNumberInfo, "Act Number must be numeric.");

			legalAct.CSI_ReferenceNumber = "123";
			AssertNoMessageError(legalAct.CSI_ReferenceNumberInfo, "Act Number must be numeric.");
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			AssertCheckFieldIsMandatory(nameof(LegalActInfo.CSI_ReferenceNumber));
		}

		public void TestCheckCSI_Code()
		{
			AssertCheckFieldIsMandatory(nameof(LegalActInfo.CSI_Code));
		}

		public void TestCheckCSI_IssuerType()
		{
			AssertCheckFieldIsMandatory(nameof(LegalActInfo.CSI_IssuerType));
		}

		public void TestCheckCSI_YearOfIssue()
		{
			AssertCheckFieldIsMandatory(nameof(LegalActInfo.CSI_YearOfIssue));
		}

		void AssertCheckFieldIsMandatory(string propertyName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			var legalAct = invoiceLine.LegalActInfos.AddNew();
			var propertyInfo = legalAct.FindPropertyInfo(propertyName);

			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.ExDutyTariff;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);

			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.ExIPITariff;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Suspension;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.IPITaxBenefit;
			ValidationTestHelper.AssertWarningIfNotEntered(propertyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.IPITaxBenefit;
			ValidationTestHelper.AssertWarningIfNotEntered(propertyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Exemption;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.IPITaxBenefit;
			ValidationTestHelper.AssertWarningIfNotEntered(propertyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			legalAct.CSI_SubType = AdditionalTaxTypeList.Codes.IPITaxBenefit;
			ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
		}
	}
}
