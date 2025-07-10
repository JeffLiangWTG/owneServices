using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusLineTariffDetailValidation))]
sealed class CusLineTariffDetailValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBZ_Tariff_AdditionalFees()
	{
		new RefCusRateTestHelper(Factory).CreateFeeRateCodes();

		CombineAssertions(() =>
		{
			LineDetail.BZ_Type = RateTypes.AdditionalFees;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(LineDetail.BZ_TariffInfo, RefCusRateTestHelper.InvalidFeeRateCode, RefCusRateTestHelper.ValidFeeRateCode);
		});
	}

	public void TestCheckBZ_Tariff_AdditionalTaxes()
	{
		const string validTariffCode = "280-000";
		const string invalidTariffCode = "290-000";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(validTariffCode);

		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = invoiceLine.AdditionalTaxes[0];

		CombineAssertions(() => ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(additionalTax.BZ_TariffInfo, invalidTariffCode, validTariffCode));
	}

	public void TestCheckBZ_Tariff_R137_WhenDeclarationTypeChanges()
	{
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff280200;

		CombineAssertions(() =>
		{
			LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			LineDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);

			LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			LineDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);
		});
	}

	public void TestCheckBZ_Tariff_R137_WhenPermitTypeChanges()
	{
		LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff280200;
		var permit = LineDetail.InvoiceLine.Permits.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.Commitment;

		CombineAssertions(() =>
		{
			permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.Other;
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);

			permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.AAT;
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);
		});
	}

	public void TestCheckBZ_Tariff_R137_WhenPermitCodeChanges()
	{
		LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff280200;
		var permit = LineDetail.InvoiceLine.Permits.AddNew();
		permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.AAT;

		CombineAssertions(() =>
		{
			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.SingleEPermit;
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);

			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.Commitment;
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR137);
		});
	}

	public void TestCheckBZ_Tariff_R138_WhenDeclarationTypeChanges()
	{
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff700002;

		CombineAssertions(() =>
		{
			LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			LineDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);

			LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			LineDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);
		});
	}

	public void TestCheckBZ_Tariff_R138_WhenPermitTypeChanges()
	{
		LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff700002;
		var permit = LineDetail.InvoiceLine.Permits.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.Commitment;

		CombineAssertions(() =>
		{
			permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.Other;
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);

			permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.FOCBS_COV;
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);
		});
	}

	public void TestCheckBZ_Tariff_R138_WhenPermitCodeChanges()
	{
		LineDetail.InvoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		LineDetail.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff700002;
		var permit = LineDetail.InvoiceLine.Permits.AddNew();
		permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.FOCBS_COV;

		CombineAssertions(() =>
		{
			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.SingleEPermit;
			AssertHasMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);

			permit.CSI_Code = UniversalReferenceConstants.PermitCodes.Commitment;
			AssertNoMessageError(LineDetail.BZ_TariffInfo, ValidationMessages.Plausi.MessageR138);
		});
	}

	public void TestCheckBZ_Tariff_R330()
	{
		var invoiceHeader = LineDetail.InvoiceLine.InvoiceHeader;

		void TestR330(bool messageExpected, string taxType, string specialMentions)
		{
			invoiceHeader.SpecialMentions = specialMentions;
			LineDetail.BZ_TaxType = taxType;
			LineDetail.BZ_Tariff = taxType + "-001";
			if (messageExpected)
			{
				AssertHasRowMessageError(GetAssertionMessage(), LineDetail, ValidationMessages.Plausi.MessageR330);
				LineDetail.BZ_Tariff = taxType + "-000";
				AssertNoRowMessageError(GetAssertionMessage(), LineDetail, ValidationMessages.Plausi.MessageR330);
				LineDetail.BZ_Tariff = ZString.Empty;
				AssertNoRowMessageError(GetAssertionMessage(), LineDetail, ValidationMessages.Plausi.MessageR330);
			}
			else
			{
				AssertNoRowMessageError(GetAssertionMessage(), LineDetail, ValidationMessages.Plausi.MessageR330);
			}
			string GetAssertionMessage() => $"TaxType={LineDetail.BZ_TaxType} Tariff={LineDetail.BZ_Tariff} SpecialMentions={invoiceHeader.SpecialMentions}";
		}

		CombineAssertions(() =>
		{
			TestR330(true, AdditionalTaxesTypes.CitesFlora, "");
			TestR330(true, AdditionalTaxesTypes.CitesFauna, "");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES01xxx");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES02xxx");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES03xxx");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES04xxx");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES05xxx");
			TestR330(true, AdditionalTaxesTypes.CitesFauna, "xxxCITES06xxx");
			TestR330(false, AdditionalTaxesTypes.CitesFauna, "xxxCITES07xxx");
			TestR330(false, "123", "");
		});
	}

	public void TestCheckBZ_Tariff_R326() => TestCheckBZ_Tariff_R326_R327(AdditionalTaxesTypes.CitesFauna, ValidationMessages.Plausi.MessageR326);

	public void TestCheckBZ_Tariff_R327() => TestCheckBZ_Tariff_R326_R327(AdditionalTaxesTypes.VeterinaryInspection, ValidationMessages.Plausi.MessageR327);

	void TestCheckBZ_Tariff_R326_R327(string tariff, string validationMessage)
	{
		const string ok = "000";
		const string error = "001";

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff("12345678000912");
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, $"{tariff}-{error}");
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, $"{tariff}-{ok}");
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, $"{AdditionalTaxesTypes.CitesFlora}-{error}");
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, $"{AdditionalTaxesTypes.CitesFlora}-{ok}");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Austria;

		var tax1 = invoiceLine.AdditionalTaxes.AddNew();
		var tax2 = invoiceLine.AdditionalTaxes.AddNew();

		CombineAssertions(() =>
		{
			tax1.BZ_TaxType = tariff;
			tax2.BZ_TaxType = AdditionalTaxesTypes.CitesFlora;
			tax1.BZ_Tariff = $"{tariff}-{error}";
			tax2.BZ_Tariff = $"{AdditionalTaxesTypes.CitesFlora}-{error}";
			tax2.Validation.ValidateBZ_Tariff();
			AssertHasMessageErrorContaining($"t1={tax1.BZ_Tariff}, t2={tax2.BZ_Tariff}", tax2.BZ_TariffInfo, validationMessage);

			tax1.BZ_Tariff = $"{tariff}-{ok}";
			tax2.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining($"t1={tax1.BZ_Tariff}, t2={tax2.BZ_Tariff}", tax2.BZ_TariffInfo, validationMessage);

			tax1.BZ_Tariff = $"{tariff}-{error}";
			tax2.BZ_Tariff = $"{AdditionalTaxesTypes.CitesFlora}-{ok}";
			tax2.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining($"t1={tax1.BZ_Tariff}, t2={tax2.BZ_Tariff}", tax2.BZ_TariffInfo, validationMessage);

			tax1.BZ_Tariff = $"{tariff}-{ok}";
			tax2.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining($"t1={tax1.BZ_Tariff}, t2={tax2.BZ_Tariff}", tax2.BZ_TariffInfo, validationMessage);
		});
	}

	public void TestCheckBZ_Qty1()
	{
		CombineAssertions(() =>
		{
			LineDetail.BZ_Type = RateTypes.AdditionalFees;
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(LineDetail.BZ_Qty1Info);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(LineDetail.BZ_Qty1Info);
		});
	}

	public void TestCheckBZ_Qty1_R149a()
	{
		CombineAssertions(() =>
		{
			AssertNoMessageError("Precondition", LineDetail.BZ_Qty1Info, ValidationMessages.Plausi.MessageR149a);

			LineDetail.BZ_UQ1 = "abc";
			AssertHasMessageError("UOM is not empty, Quantity = 0", LineDetail.BZ_Qty1Info, ValidationMessages.Plausi.MessageR149a);

			LineDetail.BZ_Qty1 = 1;
			AssertNoMessageError("UOM is not empty, Quantity != 0", LineDetail.BZ_Qty1Info, ValidationMessages.Plausi.MessageR149a);

			LineDetail.BZ_UQ1 = string.Empty;
			LineDetail.BZ_Qty1 = 0;
			AssertNoMessageError("UOM is empty, Quantity = 0", LineDetail.BZ_Qty1Info, ValidationMessages.Plausi.MessageR149a);
		});
	}

	public void TestCheckBZ_Qty1_R146() => TestCheckBZ_Qty1_R146R147R148((i, v) => i.JI_CustomsQuantity = v, ValidationMessages.Plausi.MessageR146, "11");

	public void TestCheckBZ_Qty1_R147() => TestCheckBZ_Qty1_R146R147R148((i, v) => i.JI_CustomsSecondQuantity = v, ValidationMessages.Plausi.MessageR147, "24");

	public void TestCheckBZ_Qty1_R148() => TestCheckBZ_Qty1_R146R147R148((i, v) => i.JI_CustomsThirdQuantity = v, ValidationMessages.Plausi.MessageR148, "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23");

	void TestCheckBZ_Qty1_R146R147R148(Action<JobComInvoiceLine, decimal> quantitySetter, string messageErrorText, params string[] assessmentCods)
	{
		const string additionalCode = "11111111000111";
		const string included = UniversalReferenceConstants.AdditionalTaxesTypes.VeterinaryInspection;
		const string excluded = UniversalReferenceConstants.AdditionalTaxesTypes.Beer;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff(additionalCode);
		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{included}-002", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));
		additionalTaxTariffHelper.CreateAttributeForTariff(tariff2, UniversalReferenceConstants.TariffAttributes.AssessmentCode, "2");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;

		foreach (var assessmentCod in assessmentCods)
		{
			var subCode = assessmentCod.PadLeft(3, '0');
			var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{included}-{subCode}", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));
			var tariff3 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{excluded}-{subCode}", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));

			additionalTaxTariffHelper.CreateAttributeForTariff(tariff1, UniversalReferenceConstants.TariffAttributes.AssessmentCode, assessmentCod);
			additionalTaxTariffHelper.CreateAttributeForTariff(tariff3, UniversalReferenceConstants.TariffAttributes.AssessmentCode, assessmentCod);

			invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

			CusLineTariffDetail prepareTestCase(string taxType, string tariff, decimal invoiceQuantity, decimal taxQty)
			{
				quantitySetter(invoiceLine, invoiceQuantity);
				var additionalTax = invoiceLine.AdditionalTaxes.Where(s => s.BZ_TaxType == taxType).FirstOrDefault();
				additionalTax.BZ_Tariff = tariff;
				additionalTax.BZ_Qty1 = taxQty;
				additionalTax.Validation.ValidateBZ_Qty1();
				return additionalTax;
			}

			CombineAssertions($"Assessment Code {assessmentCod}", () =>
			{
				var additionalTax = prepareTestCase(included, $"{included}-{subCode}", 5, 0);
				AssertHasMessageError("Quantity not equal", additionalTax.BZ_Qty1Info, messageErrorText);

				additionalTax = prepareTestCase(included, $"{included}-{subCode}", 5, 5);
				AssertNoMessageError("Quantity is equal", additionalTax.BZ_Qty1Info, messageErrorText);

				additionalTax = prepareTestCase(included, $"{included}-002", 5, 0);
				AssertNoMessageError("AssessmentCode = 2", additionalTax.BZ_Qty1Info, messageErrorText);

				additionalTax = prepareTestCase(excluded, $"{excluded}-{subCode}", 5, 0);
				AssertNoMessageError("Tax Tariff in ExcludedTaxTypes", additionalTax.BZ_Qty1Info, messageErrorText);
			});
		}
	}

	public void TestCheckBZ_Qty1_R262()
	{
		const string additionalCode = "11111111000111";
		const string included = UniversalReferenceConstants.AdditionalTaxesTypes.VeterinaryInspection;
		const string excluded = UniversalReferenceConstants.AdditionalTaxesTypes.Beer;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff(additionalCode);
		var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{excluded}-001", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));
		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{included}-001", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = additionalCode;

		CusLineTariffDetail prepareTestCase(string taxType, string tariff, decimal invoiceQuantity, decimal taxQty)
		{
			invoiceLine.JI_CustomsThirdQuantity = invoiceQuantity;
			var additionalTax = invoiceLine.AdditionalTaxes.Where(s => s.BZ_TaxType == taxType).FirstOrDefault();
			additionalTax.BZ_Tariff = tariff;
			additionalTax.BZ_Qty1 = taxQty;
			additionalTax.Validation.ValidateBZ_Qty1();
			return additionalTax;
		}

		CombineAssertions(() =>
		{
			var additionalTax = prepareTestCase(excluded, $"{excluded}-001", 100, 100);
			AssertHasMessageError("Same quantity", additionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR262);

			additionalTax = prepareTestCase(excluded, $"{excluded}-001", 100, 1);
			AssertNoMessageError("Quantity divided by 100", additionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR262);

			additionalTax = prepareTestCase(included, $"{included}-001", 100, 100);
			AssertNoMessageError("Excluded Tax type", additionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR262);
		});
	}

	public void TestCheckBZ_Qty1_R334ab()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CustomsSecondQuantity = 123m;
		invoiceLine.JI_CustomsThirdQuantity = 123m;

		CombineAssertions(() =>
		{
			foreach (var tariffNumber in new string[] { TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffNumbers.CigarCherootsCigarillosOthers })
			{
				AssertAdditionalTaxQuantityByTariffNumber(tariffNumber, "000999", true, ValidationMessages.Plausi.MessageR334a);
			}

			foreach (var tariffNumber in new string[] { TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading, TariffNumbers.SmokingTobaccoOther, TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffNumbers.OtherManufacturedTobaccoOtherOther })
			{
				AssertAdditionalTaxQuantityByTariffNumber(tariffNumber, "000999", true, ValidationMessages.Plausi.MessageR334b);
			}
			AssertAdditionalTaxQuantityByTariffNumber(TariffNumbers.CigarCherootsCigarillosContainingTobacco, "000911", false, ValidationMessages.Plausi.MessageR334a);
			AssertAdditionalTaxQuantityByTariffNumber(TariffNumbers.ProductsContainingTobaccoOther, "000999", false, ValidationMessages.Plausi.MessageR334a);
			AssertAdditionalTaxQuantityByTariffNumber(TariffNumbers.ProductsContainingTobaccoOther, "000999", false, ValidationMessages.Plausi.MessageR334b);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertAdditionalTaxQuantityByTariffNumber(TariffNumbers.CigarCherootsCigarillosContainingTobacco, "000999", false, ValidationMessages.Plausi.MessageR334a);
		});

		void AssertAdditionalTaxQuantityByTariffNumber(string tariffNumber, string customsFavourAndStatisticalCode, bool expectedError, string errorMessage)
		{
			invoiceLine.JI_Tariff = $"{tariffNumber}{customsFavourAndStatisticalCode}";
			var quantityAdditionalTax = invoiceLine.AdditionalTaxes.AddNew();
			quantityAdditionalTax.BZ_Tariff = "450-000";
			quantityAdditionalTax.BZ_Type = RateTypes.AdditionalTaxes;
			quantityAdditionalTax.BZ_Qty1 = 321m;
			quantityAdditionalTax.Validation.ValidateBZ_Qty1();
			if (expectedError)
			{
				AssertHasMessageError("Different quantity", quantityAdditionalTax.BZ_Qty1Info, errorMessage);
			}
			else
			{
				AssertNoMessageError("Different quantity", quantityAdditionalTax.BZ_Qty1Info, errorMessage);
			}
			quantityAdditionalTax.BZ_Qty1 = 123m;
			quantityAdditionalTax.Validation.ValidateBZ_Qty1();
			AssertNoMessageError("Same quantity", quantityAdditionalTax.BZ_Qty1Info, errorMessage);
		}
	}

	public void TestCheckBZ_Qty1_R337a()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "24022010000999";

		var taxTypes = new ZString[] { "450-002", "450-202", "470-001", "470-201" };

		AssertCusEntryLineDetailBZ_Qty1_MessageErrors(invoiceLine, taxTypes);
	}

	public void TestCheckBZ_Qty1_R337c()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "24031900000999";

		var tobacco = invoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = TobaccoMainGroupCodes.CutTobacco;
		tobacco.CSI_SubType = TobaccoSubGroupCodes._02;

		var taxTypes = new ZString[] { "450-001", "450-201", "470-002", "470-202" };

		AssertCusEntryLineDetailBZ_Qty1_MessageErrors(invoiceLine, taxTypes);
	}

	public void TestCheckBZ_Qty1_R337d()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "24022020000999";

		var tobacco = invoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;

		var taxTypes = new ZString[] { "450-002", "450-202", "470-001", "470-201" };

		AssertCusEntryLineDetailBZ_Qty1_MessageErrors(invoiceLine, taxTypes);
	}

	void AssertCusEntryLineDetailBZ_Qty1_MessageErrors(JobComInvoiceLine invoiceLine, params ZString[] taxTypes) => CombineAssertions(() =>
	{
		var messageError = ValidationMessages.Plausi.MessageR337;

		var additionalTax1 = PopulateAdditionalTax(invoiceLine, taxTypes[0], 1);
		var additionalTax2 = PopulateAdditionalTax(invoiceLine, taxTypes[1], 1);
		var additionalTax3 = PopulateAdditionalTax(invoiceLine, taxTypes[2], 1);
		var additionalTax4 = PopulateAdditionalTax(invoiceLine, taxTypes[3], 2);

		var additionalTax5 = PopulateAdditionalTax(invoiceLine, "480-XXX", 2);
		var additionalTax6 = PopulateAdditionalTax(invoiceLine, "280-001", 1);

		AssertHasMessageError($"{additionalTax1.BZ_Tariff} has messageError", additionalTax1.BZ_Qty1Info, messageError);
		AssertHasMessageError($"{additionalTax2.BZ_Tariff} has messageError", additionalTax2.BZ_Qty1Info, messageError);
		AssertHasMessageError($"{additionalTax3.BZ_Tariff} has messageError", additionalTax3.BZ_Qty1Info, messageError);
		AssertHasMessageError($"{additionalTax4.BZ_Tariff} has messageError", additionalTax4.BZ_Qty1Info, messageError);

		AssertNoMessageError("470-XXX no messageError", additionalTax5.BZ_Qty1Info, messageError);
		AssertNoMessageError("280-001 no messageError", additionalTax6.BZ_Qty1Info, messageError);

		additionalTax4.BZ_Qty1 = 1;

		foreach (var additionalTax in invoiceLine.AdditionalTaxes)
		{
			additionalTax.Validation.ValidateBZ_Qty1();
		}

		AssertNoMessageError($"{additionalTax1.BZ_Tariff} no messageError", additionalTax1.BZ_Qty1Info, messageError);
		AssertNoMessageError($"{additionalTax2.BZ_Tariff} no messageError", additionalTax2.BZ_Qty1Info, messageError);
		AssertNoMessageError($"{additionalTax3.BZ_Tariff} no messageError", additionalTax3.BZ_Qty1Info, messageError);
		AssertNoMessageError($"{additionalTax4.BZ_Tariff} no messageError", additionalTax4.BZ_Qty1Info, messageError);

		CusLineTariffDetail PopulateAdditionalTax(JobComInvoiceLine invoiceLine, ZString tariff, ZDecimal quantity)
		{
			var additionalTax = invoiceLine.AdditionalTaxes.AddNew();
			additionalTax.BZ_Tariff = tariff;
			additionalTax.BZ_Qty1 = quantity;
			return additionalTax;
		}
	});

	public void TestCheckBZ_Qty1_R339()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CustomsQuantity = 123m;

		CombineAssertions(() =>
		{
			foreach (var tariffNumber in new string[] { TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffNumbers.CigarCherootsCigarillosOthers,
					TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading, TariffNumbers.SmokingTobaccoOther, TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffNumbers.OtherManufacturedTobaccoOtherOther })
			{
				invoiceLine.JI_Tariff = tariffNumber + "000911";
				var quantityAdditionalTax = invoiceLine.AdditionalTaxes.AddNew();
				quantityAdditionalTax.BZ_Type = RateTypes.AdditionalTaxes;

				foreach (var taxKey in new string[] { AdditionalTaxesTariffs.TariffKey_011, AdditionalTaxesTariffs.TariffKey_012, AdditionalTaxesTariffs.TariffKey_013, AdditionalTaxesTariffs.TariffKey_014,
						AdditionalTaxesTariffs.TariffKey_015, AdditionalTaxesTariffs.TariffKey_016, AdditionalTaxesTariffs.TariffKey_017 })
				{
					quantityAdditionalTax.BZ_Tariff = "450-" + taxKey;
					quantityAdditionalTax.BZ_Qty1 = 321m;
					quantityAdditionalTax.Validation.ValidateBZ_Qty1();
					AssertHasMessageError($"Tariff={invoiceLine.JI_Tariff}, AdditionalTaxTariff={quantityAdditionalTax.BZ_Tariff}, Different quantity", quantityAdditionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR339);
					quantityAdditionalTax.BZ_Qty1 = 123m;
					quantityAdditionalTax.Validation.ValidateBZ_Qty1();
					AssertNoMessageError($"Tariff={invoiceLine.JI_Tariff}, AdditionalTaxTariff={quantityAdditionalTax.BZ_Tariff}, Same quantity", quantityAdditionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR339);
				}
				quantityAdditionalTax.BZ_Tariff = "450-115";
				quantityAdditionalTax.BZ_Qty1 = 321m;
				quantityAdditionalTax.Validation.ValidateBZ_Qty1();
				AssertNoMessageError($"Tariff={invoiceLine.JI_Tariff}, AdditionalTaxTariff={quantityAdditionalTax.BZ_Tariff}", quantityAdditionalTax.BZ_Qty1Info, ValidationMessages.Plausi.MessageR339);
			}

			invoiceLine.JI_Tariff = "24022222000911";
			var quantityAdditionalTax2 = invoiceLine.AdditionalTaxes.AddNew();
			quantityAdditionalTax2.BZ_Type = RateTypes.AdditionalTaxes;
			quantityAdditionalTax2.BZ_Tariff = "450-011";
			quantityAdditionalTax2.BZ_Qty1 = 321m;
			quantityAdditionalTax2.Validation.ValidateBZ_Qty1();
			AssertNoMessageError($"Tariff={invoiceLine.JI_Tariff}, AdditionalTaxTariff={quantityAdditionalTax2.BZ_Tariff}", quantityAdditionalTax2.BZ_Qty1Info, ValidationMessages.Plausi.MessageR339);
		});
	}

	public void TestBZ_ManualRate()
	{
		var lineDetail = Factory.New<CusLineTariffDetail>();
		lineDetail.BZ_Type = RateTypes.AdditionalFees;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(lineDetail.BZ_ManualRateInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lineDetail.BZ_ManualRateInfo);
		});
	}

	public void TestCheckBZ_ManualRate_R220a()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("290-001");
		var optionalTariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("292-000");
		var tariffWithFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("290-002", rateFormula: "123");

		InvoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		InvoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = InvoiceLine.AdditionalTaxes[0];

		CombineAssertions(() =>
		{
			additionalTax.BZ_Tariff = tariffWithoutFormula.ZZ1_TariffCode;
			additionalTax.BZ_ManualRate = 0;
			AssertHasMessageError("Tariff without formula", additionalTax.BZ_ManualRateInfo, ValidationMessages.Plausi.MessageR220a);

			additionalTax.BZ_Tariff = tariffWithoutFormula.ZZ1_TariffCode;
			additionalTax.BZ_ManualRate = 1;
			AssertNoMessageError("Tariff without formula", additionalTax.BZ_ManualRateInfo, ValidationMessages.Plausi.MessageR220a);

			additionalTax.BZ_Tariff = tariffWithFormula.ZZ1_TariffCode;
			additionalTax.BZ_ManualRate = 0;
			AssertNoMessageError("Tariff with formula", additionalTax.BZ_ManualRateInfo, ValidationMessages.Plausi.MessageR220a);

			additionalTax.BZ_Tariff = optionalTariffWithoutFormula.ZZ1_TariffCode;
			additionalTax.BZ_ManualRate = 0;
			AssertNoMessageError("Optional tariff without formula", additionalTax.BZ_ManualRateInfo, ValidationMessages.Plausi.MessageR220a);
		});
	}

	public void TestCheckBZ_AlcoholPercentage_R149b()
	{
		const string spiritsTaxType = AdditionalTaxesTypes.Spirits;
		const string notSpiritsTaxType = AdditionalTaxesTypes.CitesFlora;

		var spiritsAdditionalTax = InvoiceLine.AdditionalTaxes.AddNew();
		spiritsAdditionalTax.BZ_TaxType = spiritsTaxType;
		var notSpiritsAdditionalTax = InvoiceLine.AdditionalTaxes.AddNew();
		notSpiritsAdditionalTax.BZ_TaxType = notSpiritsTaxType;

		CombineAssertions(() =>
		{
			spiritsAdditionalTax.BZ_AlcoholPercentage = 0;
			spiritsAdditionalTax.BZ_Tariff = "280-001";
			spiritsAdditionalTax.Validation.ValidateBZ_AlcoholPercentage();
			AssertHasMessageError("Tax type is spirits, AdditionalTaxType applied", spiritsAdditionalTax.BZ_AlcoholPercentageInfo, ValidationMessages.Plausi.MessageR149b);

			spiritsAdditionalTax.BZ_Tariff = "280-000";
			spiritsAdditionalTax.Validation.ValidateBZ_AlcoholPercentage();
			AssertNoMessageError("Tax type is spirits, AdditionalTaxType not applied", spiritsAdditionalTax.BZ_AlcoholPercentageInfo, ValidationMessages.Plausi.MessageR149b);

			spiritsAdditionalTax.BZ_AlcoholPercentage = 1;
			AssertNoMessageError("Tax type is spirits", spiritsAdditionalTax.BZ_AlcoholPercentageInfo, ValidationMessages.Plausi.MessageR149b);

			AssertEquals("Precondition", 0m, notSpiritsAdditionalTax.BZ_AlcoholPercentage);
			AssertNoMessageError("Tax type is not spirits", notSpiritsAdditionalTax.BZ_AlcoholPercentageInfo, ValidationMessages.Plausi.MessageR149b);
		});
	}

	CusLineTariffDetail LineDetail => lineDetail ??= InvoiceLine.AdditionalTaxes.AddNew();
	CusLineTariffDetail lineDetail;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateInvoiceLine();
	JobComInvoiceLine invoiceLine;

	JobComInvoiceLine CreateInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		return invoiceLine = invoice.InvoiceLines.AddNew();
	}
}
