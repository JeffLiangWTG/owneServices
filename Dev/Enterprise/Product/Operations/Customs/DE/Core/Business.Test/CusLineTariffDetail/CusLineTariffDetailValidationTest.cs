using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusLineTariffDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBZ_Type()
		{
			var de = Core.Constants.CountryCodes.Germany;

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			Factory.Save();

			helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			Factory.Save();

			var collection = invLine.CusLineTariffDetails;
			AssertEquals("precondition", 1, invLine.CusLineTariffDetails.Count);
			var tariffDetail2 = collection.AddNew();
			var tariffDetail3 = collection.AddNew();

			tariffDetail.Validation.ValidateBZ_Type();
			tariffDetail2.Validation.ValidateBZ_Type();
			tariffDetail3.Validation.ValidateBZ_Type();

			AssertNoMessageError(tariffDetail.BZ_TypeInfo, "A maximum of 3 'Additional Tariffs' are permitted.");
			AssertNoMessageError(tariffDetail2.BZ_TypeInfo, "A maximum of 3 'Additional Tariffs' are permitted.");
			AssertNoMessageError(tariffDetail3.BZ_TypeInfo, "A maximum of 3 'Additional Tariffs' are permitted.");

			var tariffDetail4 = collection.AddNew();
			tariffDetail4.Validation.ValidateBZ_Type();
			AssertHasMessageError(tariffDetail4.BZ_TypeInfo, "A maximum of 3 'Additional Tariffs' are permitted.");
		}

		public void TestCheckBZ_UQ1() => CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(tariffDetail.BZ_UQ1Info, ["XX", "YY"], ((CodeDescriptionPairList)tariffDetail.Lookups.QuantityUnitList).GetAllCodesZString());
		});

		public void TestCheckBZ_Qty1()
		{
			var info = tariffDetail.BZ_Qty1Info;
			string uomMsgError(string uom) => string.Format("For 'UOM' {0}, the 'Quantity' must be an integer (a number with no decimal value).", uom);
			tariffDetail.BZ_UQ1 = "ZZZ";

			tariffDetail.BZ_Qty1 = ZDecimal.Zero;
			AssertHasMessageError(info, "The 'Quantity' must be between 0,001 and 999.999.999,999");

			tariffDetail.BZ_Qty1 = 0.001;
			AssertNoMessageError(info, "The 'Quantity' must be between 0,001 and 999.999.999,999");

			tariffDetail.BZ_Qty1 = 999999999.999;
			AssertNoMessageError(info, "The 'Quantity' must be between 0,001 and 999.999.999,999");

			tariffDetail.BZ_Qty1 = 1000000000;
			AssertHasMessageError(info, "The 'Quantity' must be between 0,001 and 999.999.999,999");

			tariffDetail.BZ_Qty1 = 5.5;
			AssertNoMessageErrors(info);

			tariffDetail.BZ_UQ1 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
			tariffDetail.BZ_Qty1 = 5.5;
			AssertHasMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems));

			tariffDetail.BZ_Qty1 = 5;
			AssertNoMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems));

			tariffDetail.BZ_UQ1 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells;
			tariffDetail.BZ_Qty1 = 5.5;
			AssertHasMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells));

			tariffDetail.BZ_Qty1 = 5;
			AssertNoMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells));

			tariffDetail.BZ_UQ1 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
			tariffDetail.BZ_Qty1 = 5.5;
			AssertHasMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs));

			tariffDetail.BZ_Qty1 = 5;
			AssertNoMessageError(info, uomMsgError(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs));
		}

		public void TestCheckBZ_Tariff()
		{
			var de = Core.Constants.CountryCodes.Germany;

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			Factory.Save();

			var tariffTypeEXC = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			var tariffTypeIMP = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			helper.CreateTariff(de, tariffTypeEXC.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff1 = helper.CreateTariff(de, tariffTypeEXC.PK, "1111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffRelationship(tariff1.PK, tariffTypeIMP.PK, "08091998");
			Factory.Save();

			invLine.JI_Tariff = "08091998";

			var tariffDetailCollection = invLine.CusLineTariffDetails;
			var tariffDetail1 = tariffDetail;
			var tariffDetail2 = tariffDetailCollection.AddNew();
			var tariffDetail3 = Factory.New<CusLineTariffDetail>();

			CombineAssertions(() =>
			{
				tariffDetail1.BZ_Tariff = ZString.Empty;
				AssertHasMessageError(tariffDetail1.BZ_TariffInfo, "You have not entered a Code.");

				tariffDetail1.BZ_Tariff = "12345";
				AssertHasMessageError(tariffDetail1.BZ_TariffInfo, "The entered Excise Code must have 4 digits.");

				tariffDetail1.BZ_Tariff = "1111";
				AssertNoMessageError(tariffDetail1.BZ_TariffInfo, "You have not entered a Code.");
				AssertNoMessageError(tariffDetail1.BZ_TariffInfo, "The entered Excise Code must have 4 digits.");
				AssertNoMessageError(tariffDetail1.BZ_TariffInfo, "Each additional tariff 'Code' must be unique.");

				tariffDetail2.BZ_Tariff = "4321";
				AssertNoMessageError(tariffDetail2.BZ_TariffInfo, "Each additional tariff 'Code' must be unique.");

				tariffDetail2.BZ_Tariff = "1111";
				tariffDetail1.Validation.ValidateBZ_Tariff();
				AssertHasMessageError(tariffDetail1.BZ_TariffInfo, "Each additional tariff 'Code' must be unique.");
				AssertHasMessageError(tariffDetail2.BZ_TariffInfo, "Each additional tariff 'Code' must be unique.");

				tariffDetail3.BZ_Tariff = "1111";
				AssertNoMessageError("tariffDetail3 has no invoiceLine", tariffDetail3.BZ_TariffInfo, "Each additional tariff 'Code' must be unique.");
			});
		}

		public void TestCheckBZ_Tariff_1042_CanBeEnteredTwice()
		{
			var de = Core.Constants.CountryCodes.Germany;
			const string tariff1042 = UniversalReferenceConstants.CusLineTariffCodes._1042;
			const string notificationExpectedIfEnteredMoreThanTwice = "Code 1042 may be repeated a maximum of two times.";

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			Factory.Save();

			var tariffTypeEXC = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			var tariffTypeIMP = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			helper.CreateTariff(de, tariffTypeEXC.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff1 = helper.CreateTariff(de, tariffTypeEXC.PK, "1042", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffRelationship(tariff1.PK, tariffTypeIMP.PK, "08091998");
			Factory.Save();

			invLine.JI_Tariff = "08091998";

			var tariffDetailCollection = invLine.CusLineTariffDetails;
			var tariffDetail1 = tariffDetail;
			var tariffDetail2 = tariffDetailCollection.AddNew();
			var tariffDetail3 = tariffDetailCollection.AddNew();
			var tariffDetail4 = Factory.New<CusLineTariffDetail>();

			CombineAssertions(() =>
			{
				tariffDetail1.BZ_Tariff = tariff1042;
				AssertNoMessageError("One 1042", tariffDetail1.BZ_TariffInfo, notificationExpectedIfEnteredMoreThanTwice);

				tariffDetail2.BZ_Tariff = tariff1042;
				AssertNoMessageError("Two 1042s", tariffDetail2.BZ_TariffInfo, notificationExpectedIfEnteredMoreThanTwice);

				tariffDetail3.BZ_Tariff = tariff1042;
				AssertHasMessageError("Three 1042s", tariffDetail3.BZ_TariffInfo, notificationExpectedIfEnteredMoreThanTwice);

				tariffDetail4.BZ_Tariff = tariff1042;
				AssertNoMessageError("tariffDetail4 has no invoiceLine", tariffDetail4.BZ_TariffInfo, notificationExpectedIfEnteredMoreThanTwice);
			});
		}

		public void TestCheckBZ_Value()
		{
			const string errorMessage = "Retail Price is mandatory for Tobacco Tariffs. Please enter a value greater than zero.";
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			Factory.Save();
			var tobaccoTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(CusLineTariffDetailHelper.AttributeName, CusLineTariffDetailHelper.ExciseTypes._10, tobaccoTariff);
			helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "08091999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var info = tariffDetail.BZ_ValueInfo;

			CombineAssertions(() =>
			{
				tariffDetail.BZ_Tariff = "08091999";
				Assert("Precondition, tariff 08091999", !tariffDetail.IsTobaccoRelatedTariff);
				AssertNoMessageError("BZ_Value, non tobacco tariff", info, errorMessage);

				tariffDetail.BZ_Tariff = "08091998";
				tariffDetail.Validation.ValidateBZ_Value();
				Assert("Precondition, tariff 08091998", tariffDetail.IsTobaccoRelatedTariff);
				AssertHasMessageError("BZ_Value, tobacco tariff, no value", info, errorMessage);

				tariffDetail.BZ_Value = -1.556m;
				AssertHasMessageError("BZ_Value, tobacco tariff, negative value", info, errorMessage);

				tariffDetail.BZ_Value = 1.556m;
				AssertNoMessageError("BZ_Value, tobacco tariff, valid value", info, errorMessage);

				tariffDetail.BZ_Tariff = "123";
				tariffDetail.BZ_Value = 0;
				Assert("Precondition, invalid tariff", !tariffDetail.IsTobaccoRelatedTariff);
				AssertNoMessageError("BZ_Value, invalid tariff, no value", info, errorMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);

			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			tariffDetail = invLine.CusLineTariffDetails.AddNew();
		}

		JobComInvoiceLine invLine;
		CusLineTariffDetail tariffDetail;
		UniversalReferenceTestDataHelper helper;
	}
}
