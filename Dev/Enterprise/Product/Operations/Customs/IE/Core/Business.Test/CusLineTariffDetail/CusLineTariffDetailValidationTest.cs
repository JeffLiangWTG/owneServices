using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class CusLineTariffDetailValidationTest : EU.Business.Testing.CusLineTariffDetailValidationTest
	{
		public void TestCheckBZ_Type()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(cusLineTariffDetail.BZ_TypeInfo, "XX", "X2");
		}

		public void TestCheckBZ_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Cigarettes containing tobaco (a)");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "X2", description: "Tobacco Products Tax");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "402.32 * [MIL] + 0.0873 + [RSP]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			helper.CreateRateUOM(rate.PK, "MIL");
			helper.CreateRateUOM(rate.PK, "RSP");
			helper.CreateTariffRelationship(tariff.PK, tariffType.PK, "2402209000");

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MIL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "RSP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CombineAssertions(() =>
			{
				cusLineTariffDetail.BZ_Type = "X2";
				cusLineTariffDetail.BZ_Tariff = "XXXX";
				AssertHasMessageError("Unknown Tariff", cusLineTariffDetail.BZ_TariffInfo, "The code you have selected is not in the list.");

				cusLineTariffDetail.BZ_Tariff = "X203";
				AssertNoMessageError("Known Tariff", cusLineTariffDetail.BZ_TariffInfo, "The code you have selected is not in the list.");

				AssertNoMessageError("Type and Tariff is filled", cusLineTariffDetail.BZ_TariffInfo, "You have not entered an Excise Reference Number when Type is entered.");

				cusLineTariffDetail.BZ_Tariff = string.Empty;
				AssertHasMessageError("Type is filled, Tariff is empty", cusLineTariffDetail.BZ_TariffInfo, "You have not entered an Excise Reference Number when Type is entered.");
			});
		}

		public void TestCheckBZ_Qty1()
		{
			CombineAssertions(() =>
			{
				cusLineTariffDetail.BZ_UQ1 = "ASV%";
				cusLineTariffDetail.BZ_Qty1 = 500.0;
				AssertHasMessageError("Quantity > 100 entered with unit ASV%", cusLineTariffDetail.BZ_Qty1Info, "The maximum value is 100.");

				cusLineTariffDetail.BZ_UQ1 = "ASV";
				AssertHasMessageError("Quantity > 100 entered with unit ASV%", cusLineTariffDetail.BZ_Qty1Info, "The maximum value is 100.");

				cusLineTariffDetail.BZ_Qty1 = 12.5;
				AssertNoMessageError("Quantity < 100 entered with unit ASV%", cusLineTariffDetail.BZ_Qty1Info, "The maximum value is 100.");

				cusLineTariffDetail.BZ_UQ1 = "KGM";
				cusLineTariffDetail.BZ_Qty1 = 500.0;
				AssertNoMessageError("Quantity > 100 with unit <> ASV%", cusLineTariffDetail.BZ_Qty1Info, "The maximum value is 100.");

				cusLineTariffDetail.BZ_Qty1 = ZDecimal.Zero;
				AssertHasMessageError("Quantity is not filled", cusLineTariffDetail.BZ_Qty1Info, "Quantity should be greater than 0.");

				cusLineTariffDetail.BZ_Qty1 = 500.0;
				AssertNoMessageError("Quantity is filled", cusLineTariffDetail.BZ_Qty1Info, "Quantity should be greater than 0.");

				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				cusLineTariffDetail.BZ_Qty1 = ZDecimal.Zero;
				AssertNoMessageError("Unit is empty, Quantity is empty", cusLineTariffDetail.BZ_Qty1Info, "Quantity should be greater than 0.");

				ValidationTestHelper.AssertErrorIfValueIsNegative(cusLineTariffDetail.BZ_Qty1Info);
			});
		}

		public void TestCheckBZ_UQ1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "EXC", "Excise");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X205", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containing cloves");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "479.37 * [MIL]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X207", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Other manufactured tobacco");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0", dataGrouping: Core.Constants.CountryCodes.Ireland);
			Factory.Save();

			cusLineTariffDetail.BZ_Type = "X2";
			cusLineTariffDetail.BZ_Qty1 = 500.0;
			CombineAssertions("MIL", () =>
			{
				var message = "Quantity Unit should be MIL.";
				cusLineTariffDetail.BZ_Tariff = "X205";
				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				AssertHasMessageError(cusLineTariffDetail.BZ_UQ1Info, message);

				cusLineTariffDetail.BZ_UQ1 = "MIL";
				AssertNoMessageError(cusLineTariffDetail.BZ_UQ1Info, message);
			});

			CombineAssertions("empty", () =>
			{
				var message = "Quantity Unit should be empty.";
				cusLineTariffDetail.BZ_Tariff = "X207";
				cusLineTariffDetail.BZ_UQ1 = "MIL";
				AssertHasMessageError(cusLineTariffDetail.BZ_UQ1Info, message);

				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				AssertNoMessageError(cusLineTariffDetail.BZ_UQ1Info, message);
			});
		}

		public void TestCheckBZ_Qty2()
		{
			var targetInfo = cusLineTariffDetail.BZ_Qty2Info;
			ValidationTestHelper.AssertErrorIfValueIsNegative(targetInfo);

			cusLineTariffDetail.BZ_UQ2 = "RSP";
			cusLineTariffDetail.BZ_Qty2 = ZDecimal.Zero;
			var message = "Quantity 2 should be greater than 0.";
			AssertHasMessageError(targetInfo, message);
			cusLineTariffDetail.BZ_Qty2 = 10m;
			AssertNoMessageError(targetInfo, message);
		}

		public void TestCheckBZ_UQ2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containin tobaco");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "EXC", "Excise");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "428.48 * [MIL] + 0.0885 * [RSP]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X205", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containing cloves");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "479.37 * [MIL]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			Factory.Save();

			var targetInfo = cusLineTariffDetail.BZ_UQ2Info;
			cusLineTariffDetail.BZ_Type = "X2";
			CombineAssertions("Second Unit of Rate Formula is RSP", () =>
			{
				cusLineTariffDetail.BZ_Tariff = "X203";
				var message = "Quantity Unit 2 should be RSP.";
				cusLineTariffDetail.BZ_UQ2 = ZString.Empty;
				AssertHasMessageError(targetInfo, message);

				cusLineTariffDetail.BZ_UQ2 = "RSP";
				AssertNoMessageError(targetInfo, message);
			});

			CombineAssertions("Second Unit of Rate Formula is empty", () =>
			{
				cusLineTariffDetail.BZ_Tariff = "X205";
				var message = "Quantity Unit 2 should be empty.";
				cusLineTariffDetail.BZ_UQ2 = "RSP";
				AssertHasMessageError(targetInfo, message);

				cusLineTariffDetail.BZ_UQ2 = ZString.Empty;
				AssertNoMessageError(targetInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
		}

		CusLineTariffDetail cusLineTariffDetail;
	}
}
