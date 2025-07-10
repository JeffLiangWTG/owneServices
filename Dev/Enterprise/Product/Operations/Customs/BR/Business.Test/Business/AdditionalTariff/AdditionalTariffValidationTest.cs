using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AdditionalTariffValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalTariff = line.AdditionalTariffs.AddNew();
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			AssertHasMessageErrorContaining(additionalTariff.TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalTariff.TariffTypeInfo, "X", ChildTariffTypeList.Codes.LETEC);

			line.JI_PrimaryPreference = RatePreferenceType.ExTariff;
			additionalTariff.TariffType = ZString.Empty;
			additionalTariff.Validation.ValidateTariffType();
			AssertHasMessageErrorContaining(additionalTariff.TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalTariff.TariffTypeInfo, "X", ChildTariffTypeList.Codes.LETEC);

			line.DutyRateIsOverridden = true;
			additionalTariff.TariffType = ZString.Empty;
			additionalTariff.Validation.ValidateTariffType();
			AssertNoMessageErrorContaining(additionalTariff.TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalTariff.TariffTypeInfo, "X", ChildTariffTypeList.Codes.LETEC);
		}

		public void TestCheckLegalActSubject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var additionalTariff1 = invoiceLine.AdditionalTariffs.AddNew();
			ValidationTestHelper.AssertErrorIfNotEntered(additionalTariff1.LegalActSubjectInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(additionalTariff1.LegalActSubjectInfo, AdditionalTaxTypeList.Codes.Antidumping, AdditionalTaxTypeList.Codes.ExDutyTariff);

			additionalTariff1.TariffType = ChildTariffTypeList.Codes.LETEC;
			AssertNoError(additionalTariff1.LegalActSubjectInfo, "You have entered a duplicate Legal Act Subject");

			var additionalTariff2 = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff2.TariffType = ChildTariffTypeList.Codes.LETEC;
			AssertHasError(additionalTariff2.LegalActSubjectInfo, "You have entered a duplicate Legal Act Subject");

			additionalTariff2.TariffType = ChildTariffTypeList.Codes.IPI;
			AssertNoError(additionalTariff2.LegalActSubjectInfo, "You have entered a duplicate Legal Act Subject");
		}

		public void TestCheckExNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var additionalTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = ZString.Empty;
			additionalTariff.Validation.ValidateExNumber();
			AssertNoMessageErrorContaining(additionalTariff.ExNumberInfo, MandatoryValidation.YouHaveNotEntered);

			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			AssertHasMessageErrorContaining(additionalTariff.ExNumberInfo, MandatoryValidation.YouHaveNotEntered);

			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.Antidumping;
			additionalTariff.Validation.ValidateExNumber();
			AssertNoMessageErrorContaining(additionalTariff.ExNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			AssertHasMessageErrorContaining(additionalTariff.ExNumberInfo, MandatoryValidation.YouHaveNotEntered);
			additionalTariff.TariffCode = "09022000_001";
			additionalTariff.Validation.ValidateExNumber();
			AssertNoMessageErrorContaining(additionalTariff.ExNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
