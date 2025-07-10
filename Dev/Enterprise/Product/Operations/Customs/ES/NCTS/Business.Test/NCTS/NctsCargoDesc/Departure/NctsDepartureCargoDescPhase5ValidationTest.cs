using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_CommercialReferenceNumber()
		{
			IDisposable SetTransitionPeriod(bool isTransition) => Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, isTransition);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			var validation = goodsItem.Validation;
			const string RuleC0502ExpectedMessageError = "[C0502] You have not entered a Reference Number / UCR. A Reference Number / UCR at Goods Items level is required if there is no Reference Number / UCR at Header or House Consignments level and there is no Transport Document at Header or House Consignments level.";

			using (SetTransitionPeriod(false))
			{
				CombineAssertions("Preconditions: the field should be mandatory by default.", () =>
				{
					AssertEquals("BY_CommercialReferenceNumber", ZString.Empty, goodsItem.BY_CommercialReferenceNumber);
					AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, nctsHeader.MovementHeader.BM_UniqueConsignmentReference);
					AssertEquals("B0_ReferenceID", ZString.Empty, goodsItem.Bill.B0_ReferenceID);
					AssertContainsExactElementsInAnyOrder("Additional Documents for Bill", Array.Empty<string>(), goodsItem.Bill.AdditionalDocuments.Select(x => x.CSI_SubType));
					AssertContainsExactElementsInAnyOrder("Additional Documents for Declaration", Array.Empty<string>(), nctsHeader.AdditionalDocuments.Select(x => x.CSI_SubType));

					goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
					AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
				});

				goodsItem.BY_CommercialReferenceNumber = "A";
				AssertNoMessageErrors("Entering a commercial reference number should clear the message error, obviously.", goodsItem.BY_CommercialReferenceNumberInfo);

				goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
				AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
				AssertNoMessageErrors("No message error when Phase TNN", goodsItem.BY_CommercialReferenceNumberInfo);
			}

			using (SetTransitionPeriod(true))
			{
				goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
				AssertNoMessageErrors("during the transition period, no need to check", goodsItem.BY_CommercialReferenceNumberInfo);
			}
		}

		public void TestCheckExciseCode()
		{
			SetUpTariffAndTax();
			const string RuleTR0085ExpectedMessageError = "[TR0085] An excise code must be selected to calculate the liability amount.";

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

				customsOfficeForDeparture.CY_Data = "ES0008";

				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertNoMessageError("When BY_HarmonisedTariff has only one excise and ExciseCode is not empty (automatic)", goodsItem.ExciseCodeInfo, RuleTR0085ExpectedMessageError);
				AssertEquals("The code of Excise is automatic", "0A0", goodsItem.ExciseCode);

				goodsItem.BY_HarmonisedTariff = "4444444444";
				goodsItem.ExciseCode = ZString.Empty;
				AssertHasMessageError("When BY_HarmonisedTariff has several excise taxes and ExciseCode is empty", goodsItem.ExciseCodeInfo, RuleTR0085ExpectedMessageError);

				goodsItem.ExciseCode = "0A1";
				AssertNoMessageError("When BY_HarmonisedTariff has several excise taxes and ExciseCode not is empty", goodsItem.ExciseCodeInfo, RuleTR0085ExpectedMessageError);

				goodsItem.ExciseCode = "111";
				AssertHasMessageError("When BY_HarmonisedTariff has several excise taxes and ExciseCode not is empty but but not listed", goodsItem.ExciseCodeInfo, RuleTR0085ExpectedMessageError);

				goodsItem.BY_HarmonisedTariff = "3333333333";
				goodsItem.ExciseCode = ZString.Empty;
				AssertNoMessageError("When BY_HarmonisedTariff is not subject to excise tax", goodsItem.ExciseCodeInfo, RuleTR0085ExpectedMessageError);
			});
		}

		void SetUpTariffAndTax()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(CountryCode, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			var esexcTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "ESEXC");
			Factory.Save();

			var tariff = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(CountryCode, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff3 = helper.CreateTariff(CountryCode, tariffType.PK, "4444444444", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff4 = helper.CreateTariff(CountryCode, esexcTariffType.PK, "0A0", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "desc1");
			var tariff5 = helper.CreateTariff(CountryCode, esexcTariffType.PK, "0A1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "desc2");
			var tariff6 = helper.CreateTariff(CountryCode, esexcTariffType.PK, "0A2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "desc3");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateTaxOrFee("IV1", 0.21m, CountryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, CountryCode, "IV1");
			helper.CreateNewOrGetExistingVATApplicability(tariff2, CountryCode, "IV1");

			helper.CreateTariffRelationship(tariff4.PK, tariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff5.PK, tariffType.PK, tariff3.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff6.PK, tariffType.PK, tariff3.ZZ1_TariffCode);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}
		NctsDepartureCargoDesc goodsItem;
		NctsHeader nctsHeader;

		protected ZString CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
