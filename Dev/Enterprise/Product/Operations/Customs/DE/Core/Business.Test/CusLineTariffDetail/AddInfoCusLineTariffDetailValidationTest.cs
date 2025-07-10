using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AddInfoCusLineTariffDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AlcoholicStrength()
		{
			var de = Core.Constants.CountryCodes.Germany;

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			Factory.Save();

			var tariffType = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			Factory.Save();

			var tariff = helper.CreateTariff(de, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff, "CU1", CusLineTariffDetailHelper.KnownUoMs.HLT, de);

			helper.CreateTariffAttribute("ExciseType", "40", tariff);
			Factory.Save();

			var info = tariffDetail.BZ_PercentAlcoholInfo;
			var msgError = "The 'Degree Percentage' must be between 0,01% and 100%";

			tariffDetail.BZ_Tariff = "08091998";
			tariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
			tariffDetail.BZ_UQ1 = "HLTZ"; //invalid UQ, thus PercentAlcohol Isn't Mandatory
			Assert("precondition", tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);

			tariffDetail.BZ_PercentAlcohol = ZDecimal.Zero;
			tariffDetail.AddInfoValidation.ValidateZG_AlcoholicStrength();
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 0.01m;
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 100.00m;
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 100.01m;
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_UQ1 = CusLineTariffDetailHelper.KnownUoMs.HLT; //valid UQ, thus PercentAlcohol Is Mandatory
			Assert("precondition", !tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);

			tariffDetail.BZ_PercentAlcohol = ZDecimal.Zero;
			AssertHasMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 0.01m;
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 100.00m;
			AssertNoMessageError(info, msgError);

			tariffDetail.BZ_PercentAlcohol = 100.01m;
			AssertHasMessageError(info, msgError);
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
