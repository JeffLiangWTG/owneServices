using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	class NctsDepartureCargoDescTest : NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public void TestCusSupplyChainActors()
		{
			AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(goodsItem.CusSupplyChainActorReferences);
		}

		public void TestGetNewLookups()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			AssertType<NctsDepartureCargoDescPhase5Lookups>(goodsItem.Lookups);
		}

		public void TestGetNewLookups_NCTS4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			AssertType<NctsDepartureCargoDescPhase4Lookups>(goodsItem.Lookups);
		}

		public void TestAutoFillExciseBox()
		{
			SetUpTariffAndTax();

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

				customsOfficeForDeparture.CY_Data = "ES0008";
				goodsItem.BY_HarmonisedTariff = "4444444444";
				AssertEquals("When there are several excise taxes", ZString.Empty, goodsItem.ExciseCode);

				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals("When only one excise", "0A0", goodsItem.ExciseCode);

				goodsItem.BY_HarmonisedTariff = "3333333333";
				AssertEquals("When there is no excise tax", ZString.Empty, goodsItem.ExciseCode);
			});
		}

		public void TestExciseCodePersistance()
		{
			var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_NCTS_ExciseCode");

			var goodsItem = Factory.NewWithValidTestData<NctsDepartureCargoDesc>();
			goodsItem.ExciseCode = "AAA";

			CombineAssertions(() =>
			{
				AssertNotNull("ExciseCode is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
			});
		}

		public void TestPVPValuePersistance()
		{
			var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_NCTS_PVPValue");

			var goodsItem = Factory.NewWithValidTestData<NctsDepartureCargoDesc>();
			goodsItem.PVPValue = 10;

			CombineAssertions(() =>
			{
				AssertNotNull("PVPValue is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
			});
		}

		public void TestIsCustomOfficeCanaryIsland()
		{
			nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
			customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

			CombineAssertions(() =>
			{
				customsOfficeForDeparture.CY_Data = ZString.Empty;
				AssertEquals("Customs Office for departure empty", false, goodsItem.IsCustomOfficeCanaryIsland);

				customsOfficeForDeparture.CY_Data = "ES0035";
				AssertEquals("Customs Office for departure with code of Canary Island", true, goodsItem.IsCustomOfficeCanaryIsland);

				customsOfficeForDeparture.CY_Data = "ES0008";
				AssertEquals("Customs Office for departure without code of Canary Island", false, goodsItem.IsCustomOfficeCanaryIsland);
			});
		}

		public void TestIsPVPApplicable()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
			helper.CreateRate(tariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			var tariffExciseNoPVP = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A1", rateType.PK);
			helper.CreateRate(tariffExciseNoPVP, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*ZZZ");
			helper.CreateTariffRelationship(tariffExciseNoPVP.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

				customsOfficeForDeparture.CY_Data = "ES0035";
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.ExciseCode = "0A1";
				AssertEquals("IsPVPApplicable equals to false when the formula does not contain PVP", false, goodsItem.IsPVPApplicable);

				goodsItem.ExciseCode = "0A0";
				AssertEquals("IsPVPApplicable equals to true when the formula does contain PVP", true, goodsItem.IsPVPApplicable);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("IsPVPApplicable equals to false when no Phase5 declaration", false, goodsItem.IsPVPApplicable);
			});
		}

		public void TestCurrentExciseRate()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var canTariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var esTariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
			helper.CreateRate(canTariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateRate(esTariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.3*PVP");
			helper.CreateTariffRelationship(canTariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(esTariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.ExciseCode = ZString.Empty;
				AssertNull("Is null when no ExciseCode", goodsItem.CurrentExciseRate);

				goodsItem.ExciseCode = "ZZ";
				AssertNull("Is null when invalid ExciseCode", goodsItem.CurrentExciseRate);

				goodsItem.ExciseCode = "0A0";
				AssertNotNull("Is not null when valid Tariff and ExciseCode", goodsItem.CurrentExciseRate);
				AssertEquals("ES ExciseCode", "0.3*PVP", goodsItem.CurrentExciseRate.ZZ2_RateFormula);

				nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				customsOfficeForDeparture.CY_Data = "ES0035";
				AssertEquals("Canary Island ExciseCode", "0.2*PVP", goodsItem.CurrentExciseRate.ZZ2_RateFormula);

				goodsItem.BY_HarmonisedTariff = ZString.Empty;
				AssertNull("Is null when no Tariff", goodsItem.CurrentExciseRate);

				goodsItem.BY_HarmonisedTariff = "123";
				AssertNull("Is null when invalid Tariff", goodsItem.CurrentExciseRate);
			});
		}

		public void TestSpecialExciseCode()
		{
			CombineAssertions(() =>
			{
				goodsItem.ExciseCode = "0E0";
				AssertEquals("The 5E0 code was expected and we have received 0E0", "5E0", goodsItem.SpecialExciseCode);

				goodsItem.ExciseCode = "5E0";
				AssertEquals("The 0E0 code was expected and we have received 5E0", "0E0", goodsItem.SpecialExciseCode);

				goodsItem.ExciseCode = "AE0";
				AssertEquals("The result must be empty when it does not start with 0 or 5.", ZString.Empty, goodsItem.SpecialExciseCode);
			});
		}

		public void TestCurrentSpecialExciseRate()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			var tariffExcise0 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0E0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc0");
			var rateCodePVP0 = helper.LoadOrCreateNewCusRateCode(Factory, "0E0", rateType.PK);
			helper.CreateRate(tariffExcise0, rateCodePVP0.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(tariffExcise0.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			var tariffExcise5 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "5E0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc5");
			var rateCodePVP5 = helper.LoadOrCreateNewCusRateCode(Factory, "5E0", rateType.PK);
			helper.CreateRate(tariffExcise5, rateCodePVP5.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(tariffExcise5.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			var tariffExciseO = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "AE0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "descO");
			var rateCodePVPO = helper.LoadOrCreateNewCusRateCode(Factory, "AE0", rateType.PK);
			helper.CreateRate(tariffExciseO, rateCodePVPO.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(tariffExciseO.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.ExciseCode = "0E0";
				AssertEquals("The 5E0 code was expected and we have received 0E0", "5E0", goodsItem.CurrentSpecialExciseRate.RateCode);

				goodsItem.ExciseCode = "5E0";
				AssertEquals("The 0E0 code was expected and we have received 5E0", "0E0", goodsItem.CurrentSpecialExciseRate.RateCode);

				goodsItem.ExciseCode = "AE0";
				AssertEquals("The result must be empty when it does not start with 0 or 5.", null, goodsItem.CurrentSpecialExciseRate);
			});
		}

		public void TestGetExciseAmount()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			CreateNewFormula(helper, esexcTariffType.PK, "0A7", "0.2*PVP", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A0", "0.5*[KGM]", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A6", "0.75*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0E0", "0.5*PVP", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "5E0", "0.6*PVP", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "AE0", "0.4*PVP", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A1", "0.5*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "5A1", "0.6*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "AA1", "0.4*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.ExciseCode = ZString.Empty;
				AssertEquals("Is 0 when no ExciseCode", ZDecimal.Zero, goodsItem.ExciseAmount);

				goodsItem.ExciseCode = "APC";
				AssertEquals("Is 0 when ExciseCode is not valid", ZDecimal.Zero, goodsItem.ExciseAmount);

				goodsItem.ExciseCode = "0A7";
				goodsItem.PVPValue = 30;
				AssertEquals("Value of Excise Amount when is PVP formula", 6m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 6m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.ExciseCode = "0A0";
				goodsItem.CustomsFirstQuantityInKilograms = 30;
				AssertEquals("Value of Excise Amount when is First Quantity", 15m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 15m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.CustomsFirstQuantityInKilograms = ZDecimal.Zero;
				goodsItem.BY_CustomsSecondQuantity = 50;
				goodsItem.BY_CustomsSecondUnitQty = "KGM";
				AssertEquals("Value of Excise Amount when is Second Quantity", 25m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 25m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.BY_CustomsSecondQuantity = ZDecimal.Zero;
				goodsItem.BY_CustomsSecondUnitQty = ZString.Empty;
				goodsItem.BY_CustomsThirdQuantity = 60;
				goodsItem.BY_CustomsThirdUnitQty = "KGM";
				AssertEquals("Value of Excise Amount when is Third Quantity", 30m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 30m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.BY_CustomsThirdQuantity = ZDecimal.Zero;
				goodsItem.BY_CustomsThirdUnitQty = ZString.Empty;
				goodsItem.BY_CustomsFourthQuantity = 80;
				goodsItem.BY_CustomsFourthUnitQty = "KGM";
				AssertEquals("Value of Excise Amount when is Fourth Quantity", 40m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 40m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.ExciseCode = "0A6";
				goodsItem.BY_MonetaryValue = 30;
				AssertEquals("Value of Excise Amount when is VFD", 22.5m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 22.5m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.ExciseCode = "0E0";
				goodsItem.PVPValue = 50;
				AssertEquals("Value of Excise Amount when is PVP formula and we have a special excise to add because excise starts with 0", 55m, goodsItem.ExciseAmount);
				AssertContainsExactElementsInAnyOrder($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} records", new List<ZDecimal> { 25m, 30m }, goodsItem.Fees.Where(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).Select(x => x.BFE_ChargeAmount));

				goodsItem.ExciseCode = "AE0";
				goodsItem.PVPValue = 40;
				AssertEquals("Value of Excise Amount when is PVP formula and we have no special excise to add because excise doesn't start with 0 or 5", 16m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 16m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);

				goodsItem.ExciseCode = "5A1";
				goodsItem.BY_MonetaryValue = 40;
				AssertEquals("Value of Excise Amount when is VFD and we have a special excise to add because excise starts with 5", 44m, goodsItem.ExciseAmount);
				AssertContainsExactElementsInAnyOrder($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} records", new List<ZDecimal> { 24m, 20m }, goodsItem.Fees.Where(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).Select(x => x.BFE_ChargeAmount));

				goodsItem.ExciseCode = "AA1";
				goodsItem.BY_MonetaryValue = 50;
				AssertEquals("Value of Excise Amount when is VFD formula and we have no special excise to add because excise doesn't start with 0 or 5", 20m, goodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 20m, goodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);
			});
		}

		public void TestPackages()
		{
			AssertType<NctsPackageCollection<NctsDepartureCargoDesc>>(goodsItem.Packages);
		}

		public void TestSupportingDocuments()
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(goodsItem.SupportingDocuments);
		}

		public void TestPreviousDocuments()
		{
			AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>(goodsItem.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(goodsItem.AdditionalInfos);
		}

		public void TestExciseCode()
		{
			goodsItem.ExciseCode = "AAA";
			AssertEquals("Goods Item Excise Code", "AAA", goodsItem.ExciseCode);
		}

		public void TestPVPValue()
		{
			ZDecimal pvp = 100.02;
			goodsItem.PVPValue = pvp;
			AssertEquals("Goods Item PVP", pvp, goodsItem.PVPValue);
		}

		public void TestPVPCurrency()
		{
			AssertEquals("Currency for PVP is EUR", Core.Constants.CurrencyCodes.EuropeanUnion, goodsItem.PVPCurrency);
		}

		public void TestCommodityCode()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "868494";
				AssertEquals("If Tariff doesn't start with 84 or 87, IsVehicles is not set to true", false, goodsItem.IsVehicles);
				goodsItem.BY_HarmonisedTariff = "878494";
				AssertEquals("If Tariff starts with 84 or 87, IsVehicles is set to true", true, goodsItem.IsVehicles);
				goodsItem.BY_HarmonisedTariff = "848270";
				AssertEquals("If Tariff starts with 84 or 87, IsVehicles is set to true", true, goodsItem.IsVehicles);
			});
		}

		public void TestPackagesClearedWhenIsVehicles()
		{
			goodsItem.IsVehicles = false;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			goodsItem.IsVehicles = true;
			AssertEquals(0, goodsItem.Packages.Count);
		}

		public void TestVehiclesClearedWhenIsNotVehicles()
		{
			goodsItem.IsVehicles = true;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			goodsItem.IsVehicles = false;
			AssertEquals(0, goodsItem.Packages.Count);
		}

		public void TestCloneInternalPackages()
		{
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_UnitType = "1A";
			package1.B5_UnitCount = 2;
			package1.B5_MarksAndNumbers = "package1";
			var package2 = goodsItem.Packages.AddNew();
			package2.B5_UnitType = "1B";
			package2.B5_UnitCount = 3;
			package2.B5_MarksAndNumbers = "package2";
			Factory.Save();
			var cloned = (NctsDepartureCargoDesc)goodsItem.TemplateCopy(ZGuid.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("Cloned IsVehicles false", goodsItem.IsVehicles, cloned.IsVehicles);
				AssertEquals("Cloned packages count", 2, cloned.Packages.Count);
				AssertEquals("Cloned package 1 B5_UnitType", package1.B5_UnitType, cloned.Packages[0].B5_UnitType);
				AssertEquals("Cloned package 1 B5_UnitCount", package1.B5_UnitCount, cloned.Packages[0].B5_UnitCount);
				AssertEquals("Cloned package 1 B5_MarksAndNumbers", package1.B5_MarksAndNumbers, cloned.Packages[0].B5_MarksAndNumbers);
				AssertEquals("Cloned package 2 B5_UnitType", package2.B5_UnitType, cloned.Packages[1].B5_UnitType);
				AssertEquals("Cloned package 2 B5_UnitCount", package2.B5_UnitCount, cloned.Packages[1].B5_UnitCount);
				AssertEquals("Cloned package 2 B5_MarksAndNumbers", package2.B5_MarksAndNumbers, cloned.Packages[1].B5_MarksAndNumbers);

				goodsItem.IsVehicles = true;
				package1 = goodsItem.Packages.AddNew();
				package1.B5_PackageID = "VIN1";
				package1.B5_Brand = "Brand1";
				package1.B5_Model = "Model1";
				package2 = goodsItem.Packages.AddNew();
				package2.B5_PackageID = "VIN2";
				package2.B5_Brand = "Brand2";
				package2.B5_Model = "Model2";
				cloned = (NctsDepartureCargoDesc)goodsItem.TemplateCopy(ZGuid.Empty);
				AssertEquals("Cloned IsVehicles true", goodsItem.IsVehicles, cloned.IsVehicles);
				AssertEquals("Cloned vehicles count", 2, cloned.Packages.Count);
				AssertEquals("Cloned vehicle 1 B5_PackageID", package1.B5_PackageID, cloned.Packages[0].B5_PackageID);
				AssertEquals("Cloned vehicle 1 B5_Brand", package1.B5_Brand, cloned.Packages[0].B5_Brand);
				AssertEquals("Cloned vehicle 1 B5_Model", package1.B5_Model, cloned.Packages[0].B5_Model);
				AssertEquals("Cloned vehicle 2 B5_PackageID", package2.B5_PackageID, cloned.Packages[1].B5_PackageID);
				AssertEquals("Cloned vehicle 2 B5_Brand", package2.B5_Brand, cloned.Packages[1].B5_Brand);
				AssertEquals("Cloned vehicle 2 B5_Model", package2.B5_Model, cloned.Packages[1].B5_Model);
			});
		}

		public void TestValidation_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			AssertType<NctsDepartureCargoDescPhase4Validation>(goodsItem.Validation);
		}

		public void TestValidation_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			AssertType<NctsDepartureCargoDescPhase5Validation>(goodsItem.Validation);
		}

		public void TestDefaultBY_ThirdQuantityUOMFromTariff_ExistingThirdUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			Factory.Save();

			goodsItem.BY_CustomsThirdUnitQty = "AAA";
			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("Customs Third Unit Qty (BY_CustomsThirdUnitQty) change when it already has one set", "SSS", goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestDefaultFourthQuantityUOMFromTariff_ExistingThirdUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM4Type, "CCC");
			Factory.Save();

			goodsItem.BY_CustomsFourthUnitQty = "AAA";
			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("Customs Fourth Unit Qty change when it already has one set", "CCC", goodsItem.BY_CustomsFourthUnitQty);
		}

		public void TestBY_LineNo()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_LineNo = 2;
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_LineNo = 3;
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_LineNo = 3);
			});
		}

		public void TestBY_DeclarationGoodsItemNumber()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_DeclarationGoodsItemNumber = 2;
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_DeclarationGoodsItemNumber = 3;
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_DeclarationGoodsItemNumber = 3);
			});
		}

		public void TestBY_Type()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_Type = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_Type = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_Type = "AH");
			});
		}

		public void TestBY_RN_NKCountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_RN_NKCountryOfOrigin = "AH";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_RN_NKCountryOfOrigin = "A";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_RN_NKCountryOfOrigin = "A");
			});
		}

		public void TestBY_RN_NKCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_RN_NKCountryOfDestination = "AH";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_RN_NKCountryOfDestination = "A";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
			});
		}

		public void TestBY_CommercialReferenceNumber()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_CommercialReferenceNumber = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_CommercialReferenceNumber = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
			});
		}

		public void TestBY_Description()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_Description = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_Description = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_Description = "AH");
			});
		}

		public void TestBY_CusC4Number()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_CusC4Number = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_CusC4Number = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_CusC4Number = "AH");
			});
		}

		public void TestBY_FormattedHarmonisedTariff()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_FormattedHarmonisedTariff = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_FormattedHarmonisedTariff = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_FormattedHarmonisedTariff = "AH");
			});
		}

		public void TestBY_GrossWeight()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_GrossWeight = 2;
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_GrossWeight = 3;
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
			});
		}

		public void TestBY_NetWeight()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_NetWeight = 2;
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_NetWeight = 3;
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_NetWeight = 3);
			});
		}

		public void TestBY_CustomsSecondQuantity()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_CustomsSecondQuantity = 2;
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_CustomsSecondQuantity = 3;
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_CustomsSecondQuantity = 3);
			});
		}

		public void TestBY_CustomsSecondUnitQty()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_CustomsSecondUnitQty = "AH3";
				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_CustomsSecondUnitQty = "AH";
				AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);

				goodsItem = Factory.New<NctsBill>().GoodsItems.AddNew();
				AssertNoExceptionThrown("No exception thown when Header is null", () => goodsItem.BY_CustomsSecondUnitQty = "AH");
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				goodsItem.Consignee.E2_OA_Address = orgAddress.PK;

				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress2.OA_OH = orgHeader2.PK;
				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.Consignee.E2_OA_Address = orgAddress2.PK;
				AssertEquals("When CustomsStatus is PRE and Carrier changes", "1", nctsHeader.BH_ReleaseStatus);
			});
		}

		public void TestParentIsNotBillForReleaseStatus()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				AssertNotEquals("Prereq: Parent is not Bill", CusInBondBillSchema.Constants.Prefix, goodsItem.BY_ParentTableCode);

				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;

				goodsItem.BY_LineNo = 2;
				goodsItem.BY_DeclarationGoodsItemNumber = 2;
				goodsItem.BY_Type = "AH3";
				goodsItem.BY_RN_NKCountryOfOrigin = "AH";
				goodsItem.BY_RN_NKCountryOfDestination = "AH";
				goodsItem.BY_CommercialReferenceNumber = "AH3";
				goodsItem.BY_Description = "AH3";
				goodsItem.BY_CusC4Number = "AH3";
				goodsItem.BY_FormattedHarmonisedTariff = "AH3";
				goodsItem.BY_GrossWeight = 2;
				goodsItem.BY_NetWeight = 2;
				goodsItem.BY_CustomsSecondQuantity = 2;
				goodsItem.BY_CustomsSecondUnitQty = "AH3";
				goodsItem.Consignee.E2_OA_Address = orgAddress.PK;

				AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress2.OA_OH = orgHeader2.PK;
				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				goodsItem.BY_LineNo = 3;
				goodsItem.BY_DeclarationGoodsItemNumber = 3;
				goodsItem.BY_Type = "AH";
				goodsItem.BY_RN_NKCountryOfOrigin = "A";
				goodsItem.BY_RN_NKCountryOfDestination = "A";
				goodsItem.BY_CommercialReferenceNumber = "AH";
				goodsItem.BY_Description = "AH";
				goodsItem.BY_CusC4Number = "AH";
				goodsItem.BY_FormattedHarmonisedTariff = "AH";
				goodsItem.BY_GrossWeight = 3;
				goodsItem.BY_NetWeight = 3;
				goodsItem.BY_CustomsSecondQuantity = 3;
				goodsItem.BY_CustomsSecondUnitQty = "AH";
				goodsItem.Consignee.E2_OA_Address = orgAddress2.PK;

				AssertEquals("ReleaseStatus is empty when parent is not Bill", ZString.Empty, nctsHeader.BH_ReleaseStatus);
			});
		}

		public void TestDefaultTaxType()
		{
			SetUpTariffAndTax();

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

				customsOfficeForDeparture.CY_Data = "ES0035";
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals("BY_ZZF_NKTaxType is set to IG1 if Custom is ES0035", "IG1", goodsItem.BY_ZZF_NKTaxType);

				goodsItem.BY_ZZF_NKTaxType = "APC";
				goodsItem.BY_HarmonisedTariff = ZString.Empty;
				AssertEquals("BY_ZZF_NKTaxType is cleared if tariff code it's empty", ZString.Empty, goodsItem.BY_ZZF_NKTaxType);

				customsOfficeForDeparture.CY_Data = "ES009999";
				goodsItem.BY_HarmonisedTariff = "3333333333";
				AssertEquals("BY_ZZF_NKTaxType is set to default if Custom is not ES0035 and ES0038", "IV1", goodsItem.BY_ZZF_NKTaxType);

				customsOfficeForDeparture.CY_Data = "ES0038";
				goodsItem.BY_HarmonisedTariff = "2222222222";
				AssertEquals("BY_ZZF_NKTaxType is set to IG1 if Custom is ES0038", "IG1", goodsItem.BY_ZZF_NKTaxType);
			});
		}

		public void TestDefaultTaxType_MultipleTaxOrFeeCodes()
		{
			SetUpTariffAllUnits();

			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("BY_ZZF_NKTaxType is max value of aplicable tax except IG types", "IV1", goodsItem.BY_ZZF_NKTaxType);
		}

		public void TestChangeExciseCode()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			CreateNewFormula(helper, esexcTariffType.PK, "0A7", "0.2*PVP", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A0", "0.5*[MIL]", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A1", "0.5*[HG]", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.BY_CustomsSecondUnitQty = "HG";
				AssertEquals("PreReq: First Unit filled", "KGM", goodsItem.CustomsFirstUnitQtyKilograms);

				goodsItem.ExciseCode = ZString.Empty;
				AssertEquals("BY_CustomsThirdUnitQty: When no ExciseCode, nothing change", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When no ExciseCode, nothing change", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.ExciseCode = "0A7";
				AssertEquals("BY_CustomsThirdUnitQty: When PVP, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When PVP, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.ExciseCode = "0A1";
				AssertEquals("BY_CustomsThirdUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsSecondUnitQty = "AAA";
				goodsItem.ExciseCode = "0A0";
				AssertEquals("BY_CustomsThirdUnitQty: When there is unit in First and Second Qty, unit is set in Third", "MIL", goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When Third is empty, no unit set in Fourth", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsThirdUnitQty = "AAA";
				goodsItem.ExciseCode = "0A1";
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First, Second Qty or Third, unit is set on Fourth", "HG", goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsThirdUnitQty = "NAR";
				goodsItem.BY_CustomsFourthUnitQty = ZString.Empty;
				goodsItem.ExciseCode = "0A0";
				AssertEquals("BY_CustomsFourthUnitQty: If a related unit is set in First, Second Qty, Third or Fourth, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);
			});
		}

		public void TestGetConvertibleUnitsOfMeasure()
		{
			CombineAssertions(() =>
			{
				var convertibleUnitsOfMeasure = goodsItem.ConvertibleUnitsOfMeasureSets;
				AssertEquals("Convertible Units Of Measure List Count", 4, convertibleUnitsOfMeasure.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "DTN", "GRM", "KG", "KGM", "TNE" }, convertibleUnitsOfMeasure.ElementAt(0).ToArray());
				AssertContainsExactElementsInAnyOrder(new[] { "LTR", "HLT", "KLT" }, convertibleUnitsOfMeasure.ElementAt(1).ToArray());
				AssertContainsExactElementsInAnyOrder(new[] { "LPA", "ASVX" }, convertibleUnitsOfMeasure.ElementAt(2).ToArray());
				AssertContainsExactElementsInAnyOrder(new[] { "NAR", "MIL" }, convertibleUnitsOfMeasure.ElementAt(3).ToArray());
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

		void SetUpTariffAllUnits()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(CountryCode, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			helper.CreateTaxOrFee("IG3", 0.21m, CountryCode);
			helper.CreateTaxOrFee("IV1", 0.16m, CountryCode);
			helper.CreateTaxOrFee("IV2", 0.12m, CountryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, CountryCode, "IG3");
			helper.CreateNewOrGetExistingVATApplicability(tariff, CountryCode, "IV1");
			helper.CreateNewOrGetExistingVATApplicability(tariff, CountryCode, "IV2");

			Factory.Save();
		}

		void CreateNewFormula(Universal.Testing.UniversalReferenceTestDataHelper helper, ZGuid tariffTypePK, string rateCode, string formula, ZGuid rateTypePK, ZGuid impTariffTypePK, ZString tariffCode)
		{
			var tariffExcise = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, tariffTypePK, rateCode, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rate = helper.LoadOrCreateNewCusRateCode(Factory, rateCode, rateTypePK);
			helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: formula);

			helper.CreateTariffRelationship(tariffExcise.PK, impTariffTypePK, tariffCode);
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

		protected override ZString CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
