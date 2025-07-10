using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationAdditionLegalActProviderTest : TestCaseWithFactory
	{
		public void TestNewDeclarationAdditionLegalActProvider_ExDutyTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";
			var additionalTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = "09022000_001";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			additionalTariff.LegalActType = "AD";
			additionalTariff.LegalActNumber = "151021";
			additionalTariff.LegalActIssuingBody = "ALADI";
			additionalTariff.LegalActYear = "2022";

			var provider = DeclarationAdditionLegalActProvider.New(additionalTariff);
			AssertEquals("LegalActSubject should be", "1", provider.LegalActSubject);
			AssertEquals("LegalActType should be", "AD", provider.LegalActType);
			AssertEquals("LegalActNumber should be", "151021", provider.LegalActNumber);
			AssertEquals("LegalActIssuingBody should be", "ALADI", provider.LegalActIssuingBody);
			AssertEquals("LegalActYear should be", "2022", provider.LegalActYear);
			AssertEquals("ExNumber should be", "001", provider.ExNumber);
		}

		public void TestNewDeclarationAdditionLegalActProvider_IPITaxBenefit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.IPITaxBenefitLegalActType = "ADE";
			invoiceLine.IPITaxBenefitLegalActNumber = "50268";
			invoiceLine.IPITaxBenefitLegalActIssuingBody = "CAMEX";
			invoiceLine.IPITaxBenefitLegalActYear = "2022";

			var provider = DeclarationAdditionLegalActProvider.New(invoiceLine.LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.IPITaxBenefit));
			AssertEquals("LegalActSubject should be", AdditionalTaxTypeList.Codes.IPITaxBenefit, provider.LegalActSubject);
			AssertEquals("LegalActType should be", "ADE", provider.LegalActType);
			AssertEquals("LegalActNumber should be", "50268", provider.LegalActNumber);
			AssertEquals("LegalActIssuingBody should be", "CAMEX", provider.LegalActIssuingBody);
			AssertEquals("LegalActYear should be", "2022", provider.LegalActYear);
			AssertEquals("ExNumber should be", string.Empty, provider.ExNumber);
		}

		public void TestNewDeclarationAdditionLegalActProvider_TariffAgreement()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";
			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionTariff.TariffCode = "09022000_001";
			additionTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			additionTariff.TariffType = "ASGPC";
			additionTariff.LegalActType = "AD";
			additionTariff.LegalActIssuingBody = "ALADI";
			additionTariff.LegalActNumber = "269020";
			additionTariff.LegalActYear = "2023";

			var provider = DeclarationAdditionLegalActProvider.New(additionTariff);
			AssertEquals("LegalActSubject should be", "5", provider.LegalActSubject);
			AssertEquals("LegalActType should be", "AD", provider.LegalActType);
			AssertEquals("LegalActNumber should be", "269020", provider.LegalActNumber);
			AssertEquals("LegalActIssuingBody should be", "ALADI", provider.LegalActIssuingBody);
			AssertEquals("LegalActYear should be", "2023", provider.LegalActYear);
			AssertEquals("ExNumber should be", "001", provider.ExNumber);

			additionTariff.TariffType = "MX99";
			AssertEquals("LegalActSubject should be", "3", provider.LegalActSubject);

			additionTariff.TariffType = "AR99";
			AssertEquals("LegalActSubject should be", ZString.Empty, provider.LegalActSubject);
		}

		public void TestNewDeclarationAdditionLegalActProvider_ADDTaxBenefit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";
			var additionalTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = "09022000_001";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.Antidumping;
			additionalTariff.LegalActType = "ADV";
			additionalTariff.LegalActNumber = "1996";
			additionalTariff.LegalActIssuingBody = "ALADI";
			additionalTariff.LegalActYear = "2025";

			var provider = DeclarationAdditionLegalActProvider.New(invoiceLine.LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.Antidumping));
			AssertEquals("LegalActSubject should be", AdditionalTaxTypeList.Codes.Antidumping, provider.LegalActSubject);
			AssertEquals("LegalActType should be", "ADV", provider.LegalActType);
			AssertEquals("LegalActNumber should be", "1996", provider.LegalActNumber);
			AssertEquals("LegalActIssuingBody should be", "ALADI", provider.LegalActIssuingBody);
			AssertEquals("LegalActYear should be", "2025", provider.LegalActYear);
			AssertEquals("ExNumber should be", string.Empty, provider.ExNumber);
		}
	}
}



