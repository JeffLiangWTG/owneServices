using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

class TobaccoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCodeList()
	{
		RefCusCodeTestHelper.CreateTobaccoLists(Factory);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Tobacco.CSI_CodeInfo, "9", TobaccoMainGroupCodes.CutTobacco);
	}

	public void TestSubtype()
	{
		RefCusCodeTestHelper.CreateTobaccoLists(Factory);
		Tobacco.CSI_Code = TobaccoMainGroupCodes.Cigars;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Tobacco.CSI_SubTypeInfo, "99", "01");
	}

	public void TestCSI_CodeR331abcd()
	{
		CombineAssertions(() =>
		{
			var invoiceLine = Tobacco.Parent;
			invoiceLine.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
			AssertHasMessageError("if tariffNumber = 2402.1000 and statisticalCode = 999 and CSI_Code = 2, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco;
			AssertHasMessageError("if tariffNumber = 2402.1000 and statisticalCode = 999 and CSI_Code = 3, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars;
			AssertNoMessageError("if tariffNumber = 2402.1000 and statisticalCode = 999 and CSI_Code = 1, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment;
			AssertNoMessageError("if tariffNumber = 2402.1000 and statisticalCode = 999 and CSI_Code = 4, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber = 2402.1000 and statisticalCode != 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ProductsContainingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber != 2402.1000 and statisticalCode = 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331a);

			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars;
			AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and CSI_Code = 1, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
			AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment;
			AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and CSI_Code = 4, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.CutTobacco;
			AssertHasMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and CSI_Code = 3, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertHasMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and CSI_Code = 3, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarCherootsCigarillosOthers, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertHasMessageError("if tariffNumber = 2402.9000 and statisticalCode = 999 and CSI_Code = 3, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.CigarCherootsCigarillosOthers, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber = 2402.9000 and statisticalCode != 999 and CSI_Code = 3, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ProductsContainingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber not in (2402.2010, 2402.2020, 2402.9000) and statisticalCode = 999 and CSI_Code = 3, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331b);

			invoiceLine.JI_Tariff = createTariff(TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.CutTobacco;
			AssertNoMessageError("if tariffNumber = 2403.1100 and statisticalCode = 999 and CSI_Code = 3, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment;
			AssertNoMessageError("if tariffNumber = 2403.1100 and statisticalCode = 999 and CSI_Code = 4, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars;
			AssertHasMessageError("if tariffNumber = 2403.1100 and statisticalCode = 999 and CSI_Code = 1, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
			AssertHasMessageError("if tariffNumber = 2403.1100 and statisticalCode = 999 and CSI_Code = 2, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertHasMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and CSI_Code = 2, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertHasMessageError("if tariffNumber = 2402.9000 and statisticalCode = 999 and CSI_Code = 2, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber = 2402.9000 and statisticalCode != 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ProductsContainingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber not in (2403.1100, 2403.1900, 2403.9910) and statisticalCode = 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331c);

			invoiceLine.JI_Tariff = createTariff(TariffNumbers.OtherManufacturedTobaccoOtherOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars;
			AssertNoMessageError("if tariffNumber = 2403.9990 and statisticalCode = 999 and CSI_Code = 1, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco;
			AssertNoMessageError("if tariffNumber = 2403.9990 and statisticalCode = 999 and CSI_Code = 3, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
			Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment;
			AssertNoMessageError("if tariffNumber = 2403.9990 and statisticalCode = 999 and CSI_Code = 4, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
			Tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
			AssertHasMessageError("if tariffNumber = 2403.9990 and statisticalCode = 999 and CSI_Code = 2, message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.OtherManufacturedTobaccoOtherOther, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber = 2403.9990 and statisticalCode != 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
			invoiceLine.JI_Tariff = createTariff(TariffNumbers.ProductsContainingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoMessageError("if tariffNumber != 2403.9990 and statisticalCode = 999 and CSI_Code = 2, no message error", Tobacco.CSI_CodeInfo, ValidationMessages.Plausi.MessageR331d);
		});

		string createTariff(string tariffNumber, string statisticalCode) => tariffNumber + "000" + statisticalCode;
	}

	public void TestCSI_CodeR257()
	{
		var invoiceLine = Tobacco.Parent;
		invoiceLine.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		string[] tariffNumbers = { TariffNumbers.CigarCherootsCigarillosContainingTobacco,
				TariffNumbers.CigarettesContainingTobaccoMoreThan,
				TariffNumbers.CigarettesContainingTobaccoNotMoreThan,
				TariffNumbers.CigarCherootsCigarillosOthers,
				TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading,
				TariffNumbers.SmokingTobaccoOther,
				TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff,
				TariffNumbers.OtherManufacturedTobaccoOtherOther };

		CombineAssertions(() =>
		{
			foreach (string tariff in tariffNumbers)
			{
				assertTariffNoTobaccos(tariff);
			}

			invoiceLine.JI_Tariff = createTariff(TariffNumbers.GraphiteInPowderOrFlakes, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("if tariffNumber not in required list and statisticalCode = 911, message error", Tobacco, ValidationMessages.Plausi.MessageR257);
		});

		void assertTariffNoTobaccos(string tariffNumber)
		{
			invoiceLine.JI_Tariff = createTariff(tariffNumber, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF);
			Tobacco.Validation.ValidateCSI_Code();
			AssertHasRowMessageError($"if tariffNumber = {tariffNumber} and statisticalCode = 911, message error", Tobacco, ValidationMessages.Plausi.MessageR257);
			invoiceLine.JI_Tariff = createTariff(tariffNumber, TariffStatisticalCodes.StatisticalCodeOther);
			Tobacco.Validation.ValidateCSI_Code();
			AssertNoRowMessageError($"if tariffNumber = {tariffNumber} and statisticalCode != 911, message error", Tobacco, ValidationMessages.Plausi.MessageR257);
		}

		string createTariff(string tariffNumber, string statisticalCode)
		{
			return tariffNumber + "000" + statisticalCode;
		}
	}

	public void TestDescription_Mandatory() => CombineAssertions(() =>
	{
		Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigarettes;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Tobacco.CSI_DescriptionInfo);

		Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes;
		ValidationTestHelper.AssertFieldIsNotMandatory(Tobacco.CSI_DescriptionInfo);
	});

	public void TestItemNumber() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Tobacco.CSI_ItemNumberInfo);

	public void TestValue_Mandatory() => CombineAssertions(() =>
	{
		Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigarettes;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Tobacco.CSI_ValueInfo);

		Tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes;
		ValidationTestHelper.AssertFieldIsNotMandatory(Tobacco.CSI_ValueInfo);
	});

	public void TestAdditionalDescription()
	{
		RefCusCodeTestHelper.CreateTobaccoLists(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(Tobacco.CSI_AdditionalDescriptionInfo, "99", "5");
	}

	public void TestCSI_ReferenceNumberMandatory()
	{
		Tobacco.Parent.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Tobacco.CSI_ReferenceNumberInfo);

		Tobacco.Parent.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		ValidationTestHelper.AssertFieldIsNotMandatory(Tobacco.CSI_ReferenceNumberInfo);
	}

	public void TestCSI_UnitOfQuantityMandatory()
	{
		Tobacco.Parent.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Tobacco.CSI_UnitOfQuantityInfo);

		Tobacco.Parent.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		ValidationTestHelper.AssertFieldIsNotMandatory(Tobacco.CSI_UnitOfQuantityInfo);
	}

	Tobacco Tobacco => tobacco ?? (tobacco = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Tobaccos.AddNew());
	Tobacco tobacco;
}
