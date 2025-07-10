using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsDepartureCargoDesc))]
	public abstract class NctsDepartureCargoDescAbstractTest<TMaster> : NctsCommonCargoDescAbstractTest<TMaster>
		where TMaster : NctsHeader
	{
		protected override void AssertAdditionalTypes(BusinessObject goodsItem)
		{
			AssertType("EU NctsDepartureCargoDesc", goodsItem.GetType(), new BusinessObjectFactory().Load<NctsDepartureCargoDesc>(goodsItem.PK));
		}
	}

	[TestedType(typeof(NctsDepartureCargoDesc))]
	sealed class NctsDepartureCargoDescTest : NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public void TestDelete()
		{
			var fee = goodsItem.Fees.AddNew();
			AssertEquals(1, goodsItem.Fees.Count);
			goodsItem.Delete();
			AssertEquals(0, goodsItem.Fees.Count);
		}

		public void TestValidation_Phase4()
		{
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsDepartureCargoDescPhase4Validation>(goodsItem.Validation);
		}

		public void TestValidation_Phase5()
		{
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureCargoDescPhase5Validation>(header.Bills.AddNew().GoodsItems.AddNew().Validation);
		}

		public void TestLookups_Phase4()
		{
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsDepartureCargoDescPhase4Lookups>(goodsItem.Lookups);
		}

		public void TestLookups_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			AssertType<NctsDepartureCargoDescPhase5Lookups>(goodsItem.Lookups);
		}

		public void TestCusCodeDataTypes()
		{
			var supporter = goodsItem as Integration.Customs.ICusCodeDataTypeSupporter;
			AssertContainsExactElementsInAnyOrder(new Type[] { typeof(SupplementaryCode) }, supporter.GetCusCodeDataTypes().Values);
		}

		public void TestConditionC0505CargoDesc()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, goodsItem.Consignee, !c0505Applied);

				goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
				header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, goodsItem.Consignee, c0505Applied);
			});
		}

		public void TestISupplementaryCodeSupporterProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Latvia, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "ADDCD");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "TST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			var supporter = goodsItem as ISupplementaryCodeSupporter;

			var additionalSupplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = "1";
			var loader = new BaseSupplementaryCode.Loader(Factory);
			var supplementaryCode1 = loader.LoadOrCreate<SupplementaryCode, NctsCommonCargoDesc>(goodsItem, 1);

			CombineAssertions(() =>
			{
				AssertEquals("SupplementaryCodesFieldType", nameof(FieldType.TextDropEdit), supporter.SupplementaryCodesFieldType);
				AssertArrayEqualsByElements("SupplementaryCodes", new ZGuid[] { additionalSupplementaryCode.PK }, supporter.SupplementaryCodes.Select(x => x.PK).ToArray());
				AssertEquals("Tariff", tariff.PK, supporter.Tariff.PK);
				AssertType<SpecificRateSelectionCriteria>("RateSelectionCriteria", supporter.RateSelectionCriteria);
				AssertEquals("GetCountryCodeForSupplementaryCodeProvider", Core.Constants.CountryCodes.Latvia, supporter.GetCountryCodeForCodeProvider());
				AssertEquals("CachedListOfAdditionalCodeDescriptions", "TST", supporter.CachedListOfAdditionalCodeDescriptions.CodesAsString);
				AssertEquals("SupplementaryCodeCaption", null, supporter.SupplementaryCodeCaption);
			});
		}

		public void TestAllApplicableRatesSelectionCriteria()
		{
			var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode.CY_Code = "1";

			AssertEquals(0, goodsItem.AllApplicableRatesSelectionCriteria.AdditionalCodes.Count);
		}

		public void TestBY_CommercialReferenceNumber_MaxLength()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

			var headerDepartureP4 = Factory.New<NctsHeader>();
			headerDepartureP4.SetMovementType(NctsMovementType.Codes.Departure);
			headerDepartureP4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItemDepartureP4 = headerDepartureP4.MovementHeader.GoodsItems.AddNew();

			var goodsItemWithHeaderNotYetSet = Factory.New<NctsDepartureCargoDesc>();

			CombineAssertions(() =>
			{
				AssertEquals("Max length is 35 in Phase 5 and Departure", 35, goodsItem.BY_CommercialReferenceNumberInfo.MaxLength);
				AssertEquals("Max length is 70 in Phase 4 and Departure", AutoCusInBondCargoDesc.Schema.BY_CommercialReferenceNumberMaxLength, goodsItemDepartureP4.BY_CommercialReferenceNumberInfo.MaxLength);
				AssertEquals("Max length is 70 when header is not yet set", AutoCusInBondCargoDesc.Schema.BY_CommercialReferenceNumberMaxLength, goodsItemWithHeaderNotYetSet.BY_CommercialReferenceNumberInfo.MaxLength);
			});
		}

		public void TestVatAmount_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.VatAmountInfo, "Vat Amount", "", "");
		}

		public void TestVatAmount_ReadOnly()
		{
			AssertEquals(true, goodsItem.VatAmountInfo.ReadOnly);
		}

		public void TestExciseAmount_Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(goodsItem.ExciseAmountInfo, "Excise Amount", "Excise Amount", "Excise Amount", "Total amount of Excise Taxes calculated in Euros");
		}

		public void TestExciseAmount_ReadOnly()
		{
			AssertEquals(true, goodsItem.ExciseAmountInfo.ReadOnly);
		}

		public void TestCVDAdditionalCodesWithOutTariff()
		{
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), goodsItem.CVDAdditionalCodes);
		}

		public void TestJobDocAddressCanOverrideFlag()
		{
			var goodsItem = Factory.New<NctsDepartureCargoDesc>();
			AssertEquals(false, goodsItem.ConsignorJobDocAddressRequirement.CanOverride);
			AssertEquals(false, goodsItem.ConsigneeJobDocAddressRequirement.CanOverride);
		}

		public void TestJobDocAddressRequirementDefaultContactType_Consignee()
		{
			AssertEquals(ContactType.NoContactType, goodsItem.ConsigneeJobDocAddressRequirement.DefaultContactType);
		}

		public void TestConsignor_IsPersistentWhenIsPluggedIn()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var consignor = goodsItem.Consignor;
			AssertEquals("Consignor is persisted", true, consignor.IsPersistent);
		}

		public void TestConsignee_IsPersistentWhenIsPluggedIn()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var consignee = goodsItem.Consignee;
			AssertEquals("Consignee is persisted", true, consignee.IsPersistent);
		}

		public void TestHighestVATRate()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_ZZF_NKTaxType = "RID";
			AssertEquals(0.05m, goodsItem.HighestVATRate);

			goodsItem.BY_ZZF_NKTaxType = "MIN";
			AssertEquals(0.2m, goodsItem.HighestVATRate);

			goodsItem.BY_ZZF_NKTaxType = "";
			AssertEquals(0.2m, goodsItem.HighestVATRate);
		}

		public void TestHighestVATRateWithIncompleteHarmonisedTariff()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			goodsItem.BY_HarmonisedTariff = "0304";
			goodsItem.BY_ZZF_NKTaxType = "RID";
			AssertEquals(0m, goodsItem.HighestVATRate);

			goodsItem.BY_ZZF_NKTaxType = "MIN";
			AssertEquals(0m, goodsItem.HighestVATRate);

			goodsItem.BY_ZZF_NKTaxType = "";
			AssertEquals(0m, goodsItem.HighestVATRate);
		}

		public void TestVATRate()
		{
			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);
			goodsItem.BY_HarmonisedTariff = "9111200000";

			CombineAssertions(() =>
			{
				goodsItem.BY_ZZF_NKTaxType = "ORD";
				AssertEquals(0.21m, goodsItem.VATRate);

				goodsItem.BY_ZZF_NKTaxType = "BRR";
				AssertEquals(0.06m, goodsItem.VATRate);

				goodsItem.BY_ZZF_NKTaxType = "BRP";
				AssertEquals(0.12m, goodsItem.VATRate);
			});
		}

		public void TestDefaultTaxType_SingleTaxOrFeeCode()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "MIL", "");

			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("BY_ZZF_NKTaxType is set to default tax type", "ORD", goodsItem.BY_ZZF_NKTaxType);
		}

		public void TestDefaultTaxType_MultipleTaxOrFeeCodes()
		{
			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);

			goodsItem.BY_HarmonisedTariff = "9111200000";

			AssertEquals("BY_ZZF_NKTaxType is max value of applicable tax", "ORD", goodsItem.BY_ZZF_NKTaxType);
		}

		public void TestDefaultTaxType()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "MIL", "");

			goodsItem.BY_HarmonisedTariff = "2222222222";
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.SetDefaultTaxType();

			AssertEquals("BY_ZZF_NKTaxType is set to default tax type", "ORD", goodsItem.BY_ZZF_NKTaxType);

			goodsItem.BY_HarmonisedTariff = "X";
			AssertEquals("BY_ZZF_NKTaxType is cleard", ZString.Empty, goodsItem.BY_ZZF_NKTaxType);
		}

		public void TestHighestVATRateWithoutApplicability()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			helper.CreateTaxOrFee("VATS", 0.01m, Core.Constants.CountryCodes.Latvia, new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
			helper.CreateTaxOrFee("VATD", 0.02m, Core.Constants.CountryCodes.Latvia, new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "VATS", "", new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
			Factory.Save();

			goodsItem.BY_RN_NKCountryOfOrigin = "LV";
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;
			goodsItem.BY_ZZF_NKTaxType = "VATS";

			AssertEquals("Use VAT Applicability where it's available", 0.01m, goodsItem.HighestVATRate);

			goodsItem.BY_HarmonisedTariff = "9999999999";
			goodsItem.BY_ZZF_NKTaxType = "VATD";

			AssertEquals("Use RefCusTaxFee VAT rate directly where applicability not available", 0.02m, goodsItem.HighestVATRate);
		}

		public void TestPreviousDocuments()
		{
			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS;
			pd.CSI_Code = "PD1";
			pd.CSI_ReferenceNumber = "PD123";
			AssertEquals(1, goodsItem.PreviousDocuments.Count);
			pd = goodsItem.PreviousDocuments[0];
			AssertEquals("PD123", pd.CSI_ReferenceNumber);
		}

		public void TestIUNDGDataItemProviderMembers()
		{
			var provider = (IUNDGDataItemProvider)goodsItem;

			CombineAssertions(() =>
			{
				AssertSame("UNDGs", goodsItem.UNDGs, provider.UNDGs);
				AssertSame("Factory", goodsItem.Factory, provider.Factory);
				AssertEquals("NeedFetchHintForLoad", true, provider.NeedFetchHintForLoad);
			});
		}

		public void TestUNDGs()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsUNDGDataItemCollection>("UNDGs Type", goodsItem.UNDGs);

				goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
				AssertEquals("0004a", goodsItem.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
				AssertEquals("IMO", goodsItem.UNDangerousGoodsStandard);
				AssertEquals("0004a", goodsItem.UNDangerousGoodsCode);
			});
		}

		public void TestTestUNDGsAsString_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.UNDGsAsStringInfo, "DG Substance", "DG Subs.", "DG");
		}

		public void TestUNDGsAsString()
		{
			CombineAssertions(() =>
			{
				goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				var uNDG2 = goodsItem.UNDGs.AddNew();
				uNDG2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
				AssertEquals("0004a,0004b", goodsItem.UNDGsAsString);
				uNDG2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "c", "IMO").First().PK;
				AssertEquals("0004a,0004c", goodsItem.UNDGsAsString);
			});
		}

		public void TestBY_IsMainPack_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(goodsItem.BY_IsMainPackInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Is Main Pack?", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Main Pack?", resourceStringData.MediumCaption);
			});
		}

		public void TestUNDangerousGoodsCode_Caption()
		{
			var propertyInfo = goodsItem.GetType().GetProperty(nameof(goodsItem.UNDangerousGoodsCode));
			AssertEquals("Dangerous Goods Substance", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestBY_CusC4Number_Caption()
		{
			AssertEquals("CUS-Code", DataBoundResourceStrings.GetDataForProperty(goodsItem.BY_CusC4NumberInfo).Caption);
		}

		public void TestTraders()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CO2", goodsItem.Consignor, "1");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE2", goodsItem.Consignee, "2");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", goodsItem.SecurityConsignor, "3");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "RAD", goodsItem.SecurityConsignee, "4");

			NCTSTestHelper.AssertJobDocAddress(goodsItem.Consignor, DocAddressType.ConsignorDocumentaryAddress, "1");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.Consignee, DocAddressType.ConsigneeAddress, "2");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.SecurityConsignor, DocAddressType.NotifyParty2, "3");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.SecurityConsignee, DocAddressType.NotifyParty3, "4");

			Factory.Save();
			header = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
			var movementHeader = header.MovementHeader;
			AssertEquals(1, movementHeader.GoodsItems.Count);
			goodsItem = movementHeader.GoodsItems[0];

			NCTSTestHelper.AssertJobDocAddress(goodsItem.Consignor, DocAddressType.ConsignorDocumentaryAddress, "1");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.Consignee, DocAddressType.ConsigneeAddress, "2");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.SecurityConsignor, DocAddressType.NotifyParty2, "3");
			NCTSTestHelper.AssertJobDocAddress(goodsItem.SecurityConsignee, DocAddressType.NotifyParty3, "4");
		}

		public void TestDutiableAmountWithThirdUnit_Phase4()
		{
			NCTSTestHelper.SetUpTariffSecondUnit(Factory);
			goodsItem.BY_HarmonisedTariff = "9111100000";
			goodsItem.BY_CustomsThirdUnitQty = "NAR";
			goodsItem.BY_MonetaryValue = 200m;
			goodsItem.BY_CustomsThirdQuantity = 15.00m;

			AssertEquals("Calculate by NAR of the third unit and apply by price", 7.50m, goodsItem.DutyAmount);
		}

		public void TestDutiableAmountWithAllUnits_Phase4()
		{
			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);
			goodsItem.BY_HarmonisedTariff = "9111200000";
			goodsItem.BY_CustomsFourthUnitQty = "LTR";
			goodsItem.BY_CustomsFourthQuantity = 15.00m;
			goodsItem.BY_CustomsThirdUnitQty = "NAR";
			goodsItem.BY_CustomsThirdQuantity = 15.00m;
			goodsItem.BY_CustomsUnitQty = "KGM";
			goodsItem.BY_CustomsQuantity = 15.00m;
			goodsItem.BY_CustomsSecondUnitQty = "DTN";
			goodsItem.BY_CustomsSecondQuantity = 15.00m;

			AssertEquals("Calculate DutyAmount with the three Customs fields", 22.50m, goodsItem.DutyAmount);
		}

		public void TestFeeRounding()
		{
			NCTSTestHelper.SetUpTariffAndAntidumpingAndCountervailing(Factory);
			var departureGoodsItem = Factory.New<NctsDepartureCargoDescForTest>();
			var movementHeader = header.MovementHeader;
			departureGoodsItem.BY_ParentTableCode = movementHeader.TablePrefix;
			departureGoodsItem.BY_ParentID = movementHeader.PK;
			departureGoodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			departureGoodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			departureGoodsItem.BY_MonetaryValue = 333.34m;
			departureGoodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
			departureGoodsItem.ExciseAmountForTesting = 1.12345m;
			CombineAssertions(() =>
			{
				AssertEquals("DutyAmount", 40.00m, departureGoodsItem.DutyAmount);
				AssertEquals("ExciseAmount", 1.12m, departureGoodsItem.ExciseAmount);
				AssertEquals("VatAmount calculation (333.34+40+1.12+4.3+4.3)*0.2=", 76.61m, departureGoodsItem.VatAmount);
				var vatFee = departureGoodsItem.Fees.SingleOrDefault(x => x.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.VAT);
				AssertNotNull($"Fees should have {NctsCommonCargoDesc.ChargeType.VAT} record", vatFee);
				if (vatFee != null)
				{
					AssertEquals($"{NctsCommonCargoDesc.ChargeType.VAT} record BFE_ChargeAmount", 76.61m, vatFee.BFE_ChargeAmount);
					AssertEquals($"{NctsCommonCargoDesc.ChargeType.VAT} record BFE_Rate", 20.00m, vatFee.BFE_Rate);
				}
			});
		}

		public void TestExciseAmount()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var departureGoodsItem = Factory.New<NctsDepartureCargoDescForTest>();
			departureGoodsItem.BY_ParentTableCode = header.MovementHeader.TablePrefix;
			departureGoodsItem.BY_ParentID = header.MovementHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Initial ExciseAmount", 0m, departureGoodsItem.ExciseAmount);

				departureGoodsItem.ExciseAmountForTesting = 200m;
				AssertEquals("New ExciseAmount", 200m, departureGoodsItem.ExciseAmount);
				AssertEquals($"Fees should have {NctsDepartureCargoDesc.ChargeType.Excise} record", 200m, departureGoodsItem.Fees.Single(x => x.BFE_ChargeType == NctsDepartureCargoDesc.ChargeType.Excise).BFE_ChargeAmount);
			});
		}

		public void TestIsSecurityDeclaration_SecurityConsignor()
		{
			AssertEquals(false, header.IsSecurityDeclaration);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", goodsItem.SecurityConsignor);
			header.BH_FTZMove = true;
			header.MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals(true, header.IsSecurityDeclaration);
		}

		public void TestIsSecurityDeclaration_SecurityConsignee()
		{
			AssertEquals(false, header.IsSecurityDeclaration);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "RAD", goodsItem.SecurityConsignee);
			header.BH_FTZMove = true;
			header.MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals(true, header.IsSecurityDeclaration);
		}

		public void TestConditionB1822()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL505, "CusCodeTypeCL505");
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL505, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2040, 12, 31));
			Factory.Save();

			var errorMessage = "Consignment Item Consignee's post code is mandatory if the country is not from CL505.";
			var consignee = goodsItem.Consignee;
			var info = consignee.OrganisationPKInfo;
			var validation = consignee.Validation;

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1822Active), value: true))
			using (SetTransitionPeriod(true))
			{
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", consignee, countryCode: "AT", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertNoMessageError(info, errorMessage);

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", consignee, countryCode: "ZA", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertHasMessageError(info, errorMessage);
			}

			var newFactory = new BusinessObjectFactory();
			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			consignee = goodsItem.Consignee;

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1822Active), value: false))
			using (SetTransitionPeriod(true))
			{
				info = goodsItem.Consignee.OrganisationPKInfo;
				validation = consignee.Validation;

				NCTSTestHelper.CreateJobDocAddressForTest(newFactory, "PC1", consignee, countryCode: "AT", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertNoMessageError(info, errorMessage);

				NCTSTestHelper.CreateJobDocAddressForTest(newFactory, "PC1", consignee, countryCode: "ZA", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertNoMessageError(info, errorMessage);
			}

			newFactory = new BusinessObjectFactory();
			nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			consignee = goodsItem.Consignee;

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1822Active), value: true))
			using (SetTransitionPeriod(false))
			{
				info = goodsItem.Consignee.OrganisationPKInfo;
				validation = consignee.Validation;

				NCTSTestHelper.CreateJobDocAddressForTest(newFactory, "PC1", consignee, countryCode: "AT", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertNoMessageError(info, errorMessage);

				NCTSTestHelper.CreateJobDocAddressForTest(newFactory, "PC1", consignee, countryCode: "ZA", suffix: "", postCode: "");
				validation.ValidateOrganisationPK();
				AssertNoMessageError(info, errorMessage);
			}
		}

		public void TestConsigneeConditionB2400_1()
		{
			const string errorMessage = "[B2400-1] Consignee field must be empty";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var consignee = goodsItem.Consignee;
			var org = Factory.New<OrgHeader>();
			var info = consignee.OrganisationPKInfo;

			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(goodsItem);
			deciderTestContext.EnableRule(x => x.IsRuleB2400_1Active);

			using (SetTransitionPeriod(false))
			{
				CombineAssertions("Not in Transition Period", () =>
				{
					consignee.OrganisationPK = org.PK;
					AssertHasMessageError("When Consignee is filled", info, errorMessage);

					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("When Consignee is not filled", info, errorMessage);
				});
			}

			using (SetTransitionPeriod(true))
			{
				CombineAssertions("In Transition Period", () =>
				{
					consignee.OrganisationPK = org.PK;
					AssertNoMessageError("When Consignee is filled", info, errorMessage);

					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("When Consignee is not filled", info, errorMessage);
				});
			}
		}

		public void TestConditionC002_HeaderLevel()
		{
			CombineAssertions("In Phase 4", () =>
			{
				goodsItem.BY_Description = "xxx";
				goodsItem.BY_RN_NKCountryOfDestination = "AU";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				goodsItem.BY_RN_NKCountryOfDestination = "AX";
				AssertHasMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CEE", header.Consignee, "1", traderTin: "");
				goodsItem.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");
			});

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			CombineAssertions("In Phase 5", () =>
			{
				goodsItem.BY_Description = "xxx";
				goodsItem.BY_RN_NKCountryOfDestination = "AU";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				goodsItem.BY_RN_NKCountryOfDestination = "AX";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CEE", header.Consignee, "1", traderTin: "");
				goodsItem.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");
			});
		}

		public void TestConditionC002_LineLevel()
		{
			CombineAssertions("In Phase 4", () =>
			{
				goodsItem.BY_Description = "xxx";
				goodsItem.BY_RN_NKCountryOfDestination = "AU";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				goodsItem.BY_RN_NKCountryOfDestination = "AX";
				AssertHasMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CEE", goodsItem.Consignee, "1", traderTin: "");
				goodsItem.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");
			});

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			CombineAssertions("In Phase 5", () =>
			{
				goodsItem.BY_Description = "xxx";
				goodsItem.BY_RN_NKCountryOfDestination = "AU";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				goodsItem.BY_RN_NKCountryOfDestination = "AX";
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");

				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CEE", goodsItem.Consignee, "1", traderTin: "");
				goodsItem.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(goodsItem.Consignee.OrganisationPKInfo, "C002");
			});
		}

		public void TestConditionR020()
		{
			goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			header.CustomsOffices.Load();
			var desOffice = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			desOffice.CY_Data = "CH123456";
			goodsItem.Validation.ValidateBY_Type();
			AssertHasMessageErrorContaining(goodsItem.BY_TypeInfo, "R020");
			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_Code = "T2F";
			pd.CSI_ReferenceNumber = "MyDoc";
			goodsItem.Validation.ValidateBY_Type();
			AssertNoMessageErrorContaining(goodsItem.BY_TypeInfo, "R020");
		}

		public void TestBY_RN_NKCountryOfDispatch_Phase4ReadOnly()
		{
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDispatchInfo.ReadOnly);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
			AssertEquals(true, goodsItem.BY_RN_NKCountryOfDispatchInfo.ReadOnly);

			goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Taiwan;
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDispatchInfo.ReadOnly);
		}

		public void TestBY_RN_NKCountryOfDispatch_Phase5ReadOnly()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDispatchInfo.ReadOnly);
		}

		public void TestBY_RN_NKCountryOfDestination_Phase4ReadOnly()
		{
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDestinationInfo.ReadOnly);

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
			AssertEquals(true, goodsItem.BY_RN_NKCountryOfDestinationInfo.ReadOnly);

			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.SierraLeone;
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDestinationInfo.ReadOnly);
		}

		public void TestBY_RN_NKCountryOfDestination_Phase5ReadOnly()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
			AssertEquals(false, goodsItem.BY_RN_NKCountryOfDestinationInfo.ReadOnly);
		}

		public void TestSetTariffDefaultsGoodsDescriptionWhenEmpty()
		{
			NCTSTestHelper.SetUpTariff(Factory);
			NCTSTestHelper.AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description(goodsItem);
		}

		public void TestSetTariffDefaultsTruncatedGoodsDescriptionWhenExceedsMaxLength()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));

			goodsItem.BY_HarmonisedTariff = "9999999999";
			var descriptionMaxLength = goodsItem.BY_DescriptionInfo.MaxLength;
			CombineAssertions(() =>
			{
				AssertEquals("Truncated when Exceeds", descriptionMaxLength, goodsItem.BY_Description.Length);
				AssertEquals("Correct value in Description", new string('0', descriptionMaxLength), goodsItem.BY_Description);
			});
		}

		public void TestSetTariffDoesNotDefaultGoodsDescriptionWhenAlreadyFilledWithUnofficialDescription()
		{
			NCTSTestHelper.SetUpTariff(Factory);
			goodsItem.BY_Description = "I'M NOT EMPTY SO I STAY HERE!";
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			AssertEquals("When setting BY_HarmonisedTariff and BY_Description is already filled with an unofficial description, BY_Description", "I'M NOT EMPTY SO I STAY HERE!", goodsItem.BY_Description);
		}

		public void TestITariffDescriptionSynchronizerSupporterMembers()
		{
			NCTSTestHelper.SetUpTariff(Factory);
			NCTSTestHelper.AssertCargoDescITariffDescriptionSynchronizerSupporterMembers(goodsItem);
		}

		[TestDate(2022, 11, 11)]
		public void TestValuationDateAffectsUsedTariffData()
		{
			var date1 = new ZDateTime(2022, 11, 7);
			var date2 = new ZDateTime(2022, 11, 9);
			var date3 = new ZDateTime(2022, 11, 10);
			var date4 = new ZDateTime(2022, 11, 12);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, date1, date2, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, date3, date4, "VFD * 0.05", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, date1, date2);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, date3, date4);
			helper.CreateTaxOrFee("RID", 0.01m, Core.Constants.CountryCodes.Latvia, date1, date2);
			helper.CreateTaxOrFee("RID", 0.02m, Core.Constants.CountryCodes.Latvia, date3, date4);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "RID", "", date1, date4);
			Factory.Save();

			var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode.CY_Code = "AC01";
			goodsItem.BY_RN_NKCountryOfOrigin = "EU";
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;
			goodsItem.BY_ZZF_NKTaxType = "RID";

			CombineAssertions("Should use data for 'today' if ValuationDate is not set", () =>
			{
				AssertEquals("VFD * 0.05", goodsItem.HighestDuty.ZZ2_RateFormula);
				AssertEquals(0.02m, goodsItem.HighestVATRate);
			});

			header.MovementHeader.BM_ValuationDate = new ZDateTime(2022, 11, 8);
			CombineAssertions("Should use data for the set ValuationDate", () =>
			{
				AssertEquals("VFD * 0.08", goodsItem.HighestDuty.ZZ2_RateFormula);
				AssertEquals(0.01m, goodsItem.HighestVATRate);
			});
		}

		public void TestBY_Type_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_TypeInfo, NctsHeader.Phase4CaptionKey, "[1] Declaration Type", "Decl. Type", "Type");
		}

		public void TestBY_Type_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_TypeInfo, NctsHeader.Phase5CaptionKey, "Declaration Type", "Dec. Type", "Type");
		}

		public void TestBY_TypeOfAutoSetBM_InBondEntryType()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = header.MovementHeader;
			movementHeader.BM_InBondEntryType = "T";
			goodsItem.BY_Type = "T1";
			AssertEquals("Declaration Type in MovementHeader is not empty when set BY_Type to T1", "T", movementHeader.BM_InBondEntryType);
		}

		public void TestBY_RN_NKCountryOfDispatch_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_RN_NKCountryOfDispatchInfo, NctsHeader.Phase4CaptionKey, "[15A] Dispatch Country/Region", "Disp.", "Disp. Ctry./Rgn.", "Country/Region of Dispatch");
		}

		public void TestBY_RN_NKCountryOfDispatch_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_RN_NKCountryOfDispatchInfo, NctsHeader.Phase5CaptionKey, "Country/Region of Dispatch", "Disp. Ctry./Rgn.", "Dispatch Ctry./Rgn.");
		}

		public void TestBY_RN_NKCountryOfDestination_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_RN_NKCountryOfDestinationInfo, NctsHeader.Phase4CaptionKey, "[17A] Destination Country/Region", "Dest.", "Dest. Ctry./Rgn.", "Country/Region of Destination");
		}

		public void TestBY_RN_NKCountryOfDestination_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(goodsItem.BY_RN_NKCountryOfDestinationInfo, NctsHeader.Phase5CaptionKey, "Country/Region of Destination", "Destin. Ctry./Rgn.", "Destination Ctry./Rgn.");
		}

		public void TestBY_CommercialReferenceNumber_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_CommercialReferenceNumberInfo, NctsHeader.Phase4CaptionKey, "Commercial Reference", "Comm. Ref.", "Ref.");
		}

		public void TestBY_CommercialReferenceNumber_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_CommercialReferenceNumberInfo, NctsHeader.Phase5CaptionKey, "Commercial Reference", "Commercial Ref.", "Reference");
		}

		public void TestBY_CommercialReferenceNumber_Phase5DepartureCaption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(goodsItem.BY_CommercialReferenceNumberInfo, NctsHeader.Phase5DepartureCaptionKey, "Reference Number / UCR", string.Empty, "Ref. No. / UCR", "Indicate the Reference Number / Unique Consignment Reference (UCR)");
		}

		public void TestBY_TransportChargesMethodOfPayment_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(goodsItem.BY_TransportChargesMethodOfPaymentInfo, NctsHeader.Phase4CaptionKey, "Transport Charges / Method of Payment", "Trans. Chg. MoP", "MoP", "Method of Payment of Transport Charges");
		}

		public void TestBY_TransportChargesMethodOfPayment_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(goodsItem.BY_TransportChargesMethodOfPaymentInfo, NctsHeader.Phase5CaptionKey, "Transport Charges MoP", "Transport Chg. MoP", "Trans. Chg. MoP", "Method of Payment of Transport Charges");
		}

		public void TestBY_TransportChargesMethodOfPayment_ReadOnly_WhenC0337Active()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0337Active)))
			{
				CombineAssertions(() =>
				{
					bill.B0_TransportPaymentMethod = ZString.Empty;
					AssertEquals($"Bill B0_TransportPaymentMethod {bill.B0_TransportPaymentMethod}, read-only", false, goodsItem.BY_TransportChargesMethodOfPaymentInfo.ReadOnly);

					bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
					AssertEquals($"Bill B0_TransportPaymentMethod {bill.B0_TransportPaymentMethod}, read-only", true, goodsItem.BY_TransportChargesMethodOfPaymentInfo.ReadOnly);
				});
			}
		}

		public void TestBY_TransportChargesMethodOfPayment_ReadOnly_WhenC0337NotActive()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0337Active)))
			{
				AssertEquals(false, goodsItem.BY_TransportChargesMethodOfPaymentInfo.ReadOnly);
			}
		}

		public void TestCusSupplyChainActorReferences()
		{
			CombineAssertions(() =>
			{
				var cusSupplyChainActorReferences = goodsItem.CusSupplyChainActorReferences;
				AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>("Type", cusSupplyChainActorReferences);
				AssertEquals("IsRegisteredEditableChildObject", true, goodsItem.IsRegisteredEditableChildObject(cusSupplyChainActorReferences));
				AssertSame("Cached", cusSupplyChainActorReferences, goodsItem.CusSupplyChainActorReferences);
				AssertEquals("IsLoaded", true, cusSupplyChainActorReferences.IsLoaded);
			});
		}

		public void TestCustomsFirstQuantityInKilograms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BY_CustomsQuantity is 0 before setting CustomsFirstQuantityInKilograms", 0m, goodsItem.BY_CustomsQuantity);
				AssertEquals("BY_CustomsUnitQty is empty before setting CustomsFirstQuantityInKilograms", ZString.Empty, goodsItem.BY_CustomsUnitQty);

				goodsItem.CustomsFirstQuantityInKilograms = new ZDecimal(2000);
				AssertEquals("CustomsFirstQuantityInKilograms is set", 2000.00m, goodsItem.CustomsFirstQuantityInKilograms);
				AssertEquals("BY_CustomsQuantity is set when setting CustomsFirstQuantityInKilograms", 2000.00m, goodsItem.BY_CustomsQuantity);
				AssertEquals("BY_CustomsUnitQty is set to KGM when setting CustomsFirstQuantityInKilograms", "KGM", goodsItem.BY_CustomsUnitQty);

				goodsItem.CustomsFirstQuantityInKilograms = new ZDecimal(0);
				AssertEquals("CustomsFirstQuantityInKilograms is set to 0", 0m, goodsItem.CustomsFirstQuantityInKilograms);
				AssertEquals("BY_CustomsQuantity is set to 0 when setting CustomsFirstQuantityInKilograms to 0", 0m, goodsItem.BY_CustomsQuantity);
				AssertEquals("BY_CustomsUnitQty is set to empty when setting CustomsFirstQuantityInKilograms to 0", ZString.Empty, goodsItem.BY_CustomsUnitQty);
			});
		}

		public void TestCustomsFirstUnitQtyKilograms()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_CustomsUnitQty = ZString.Empty;
				AssertEquals("CustomsFirstUnitQtyKilograms is KGM when BY_CustomsUnitQty is empty", "KGM", goodsItem.CustomsFirstUnitQtyKilograms);
				goodsItem.BY_CustomsUnitQty = "AA";
				AssertEquals("CustomsFirstUnitQtyKilograms is AA when BY_CustomsUnitQty is set to AA", "AA", goodsItem.CustomsFirstUnitQtyKilograms);
				AssertEquals("Field should be readonly", true, goodsItem.CustomsFirstUnitQtyKilogramsInfo.ReadOnly);
			});
		}

		public void TestCustomsFirstQuantityInKilograms_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(NctsDepartureCargoDesc), nameof(NctsDepartureCargoDesc.CustomsFirstQuantityInKilograms), false, x => x.DecimalPlaces == 6);
		}

		public void TestBY_CustomsUnitQty()
		{
			AssertEquals("BY_CustomsUnitQty is readonly", true, goodsItem.BY_CustomsUnitQtyInfo.ReadOnly);
		}

		public void TestConvertCustomsFirstQtyFromNetWeight()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_NetWeight = 10m;
				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Milligrams;
				AssertEquals("10 miligrams converted to kilograms", 0.00001m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.LongTons;
				AssertEquals("10 longtons converted to kilograms", 10160.4691m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Ounces;
				AssertEquals("10 ounces converted to kilograms", 0.283495m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeight = 10.123456m;
				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				AssertEquals("10.123456m kilograms converted to kilograms (no conversion needed)", 10.123456m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeight = 1000m;
				AssertEquals("1000 kilograms converted to kilograms (no conversion needed)", 1000m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("1000 grams converted to kilograms", 1m, goodsItem.CustomsFirstQuantityInKilograms);

				goodsItem.BY_NetWeightUnit = "X";
				AssertEquals("When unit is not correct no change is done", 1m, goodsItem.CustomsFirstQuantityInKilograms);
			});
		}

		public void TestCustomsThirdQuantity()
		{
			goodsItem.BY_CustomsThirdQuantity = new ZDecimal(2000);
			goodsItem.BY_CustomsThirdUnitQty = "DTNG";
			AssertEquals(2000.000m, goodsItem.BY_CustomsThirdQuantity);
		}

		public void TestCustomsQuantityValuesInDatabase()
		{
			goodsItem.BY_CustomsQuantity = 1m;
			goodsItem.BY_CustomsUnitQty = "KGM";
			Factory.Save();
			AssertCustomsQuantityDbValues(goodsItem, 1m, "KGM");

			goodsItem.BY_CustomsQuantity = 0m;
			goodsItem.BY_CustomsUnitQty = "";
			Factory.Save();
			AssertCustomsQuantityDbValues(goodsItem, DBNull.Value, DBNull.Value);
		}

		public void TestRecalculateDutyAmount_Phase4()
		{
			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);
			goodsItem.BY_HarmonisedTariff = "9111200000";
			goodsItem.BY_CustomsFourthUnitQty = "LTR";
			goodsItem.BY_CustomsFourthQuantity = 15.00m;
			goodsItem.BY_CustomsThirdUnitQty = "NAR";
			goodsItem.BY_CustomsThirdQuantity = 15.00m;
			goodsItem.BY_CustomsUnitQty = "KGM";
			goodsItem.BY_CustomsQuantity = 15.00m;
			goodsItem.BY_CustomsSecondUnitQty = "DTN";
			goodsItem.BY_CustomsSecondQuantity = 15.00m;

			CombineAssertions(() =>
			{
				AssertEquals("[Precondition] DutyAmount value", 22.50m, goodsItem.DutyAmount);

				goodsItem.BY_CustomsQuantity = 10.00m;
				AssertEquals("DutyAmount is recalculated when Customs value change", 20.00m, goodsItem.DutyAmount);

				goodsItem.BY_CustomsSecondQuantity = 10.00m;
				AssertEquals("DutyAmount is recalculated when Customs Second value change", 17.5m, goodsItem.DutyAmount);

				goodsItem.BY_CustomsThirdQuantity = 10.00m;
				AssertEquals("DutyAmount is recalculated when Customs Third value change", 15.00m, goodsItem.DutyAmount);
			});
		}

		public void TestRecalculateVatAmount_Phase4()
		{
			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);
			goodsItem.BY_HarmonisedTariff = "9111200000";
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.BY_MonetaryValue = 50m;
			goodsItem.BY_CustomsFourthUnitQty = "LTR";
			goodsItem.BY_CustomsFourthQuantity = 10.00m;
			goodsItem.BY_CustomsThirdUnitQty = "NAR";
			goodsItem.BY_CustomsThirdQuantity = 10.00m;
			goodsItem.BY_CustomsUnitQty = "KGM";
			goodsItem.BY_CustomsQuantity = 10.00m;
			goodsItem.BY_CustomsSecondUnitQty = "DTN";
			goodsItem.BY_CustomsSecondQuantity = 10.00m;

			CombineAssertions(() =>
			{
				AssertEquals("[Precondition] Vat value", 13.65m, goodsItem.VatAmount);

				goodsItem.BY_CustomsQuantity = 2.00m;
				AssertEquals("VatAmount is recalculated when Customs value change", 12.81m, goodsItem.VatAmount);

				goodsItem.BY_CustomsSecondQuantity = 2.00m;
				AssertEquals("VatAmount is recalculated when Customs Second value change", 11.97m, goodsItem.VatAmount);

				goodsItem.BY_CustomsThirdQuantity = 4.00m;
				AssertEquals("VatAmount is recalculated when Customs Third value change", 11.34m, goodsItem.VatAmount);

				goodsItem.BY_MonetaryValue = 100.00m;

				goodsItem.BY_ZZF_NKTaxType = "BRR";
				AssertEquals("VatAmount is recalculated when Tax Type changes (BRR - no VAT Applicability)", 6.24m, goodsItem.VatAmount);

				goodsItem.BY_ZZF_NKTaxType = "BRP";
				AssertEquals("VatAmount is recalculated when Tax Type changes (BRP - with VAT Applicability)", 12.48m, goodsItem.VatAmount);

				goodsItem.BY_ZZF_NKTaxType = "ORD";
				AssertEquals("VatAmount is recalculated when Tax Type changes (ORD - with VAT Applicability)", 21.84m, goodsItem.VatAmount);
			});
		}

		public void TestRecalculateVatAmount_Phase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

			NCTSTestHelper.SetUpTariffAllUnits(Factory, CountryCode);
			goodsItem.BY_HarmonisedTariff = "9111200000";
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.BY_MonetaryValue = 50m;
			goodsItem.BY_CustomsFourthUnitQty = "LTR";
			goodsItem.BY_CustomsFourthQuantity = 10.00m;
			goodsItem.BY_CustomsThirdUnitQty = "NAR";
			goodsItem.BY_CustomsThirdQuantity = 10.00m;
			goodsItem.BY_CustomsUnitQty = "KGM";
			goodsItem.BY_CustomsQuantity = 10.00m;
			goodsItem.BY_CustomsSecondUnitQty = "DTN";
			goodsItem.BY_CustomsSecondQuantity = 10.00m;

			CombineAssertions(() =>
			{
				AssertEquals("[Precondition] Vat value", 14.70m, goodsItem.VatAmount);

				goodsItem.BY_CustomsQuantity = 2.00m;
				AssertEquals("VatAmount is recalculated when Customs value change", 13.86m, goodsItem.VatAmount);

				goodsItem.BY_CustomsSecondQuantity = 2.00m;
				AssertEquals("VatAmount is recalculated when Customs Second value change", 13.02m, goodsItem.VatAmount);

				goodsItem.BY_CustomsThirdQuantity = 4.00m;
				AssertEquals("VatAmount is recalculated when Customs Third value change", 12.39m, goodsItem.VatAmount);

				goodsItem.BY_CustomsFourthQuantity = 4.00m;
				AssertEquals("VatAmount is recalculated when Customs Fourth value change", 11.76m, goodsItem.VatAmount);

				goodsItem.BY_MonetaryValue = 100.00m;

				goodsItem.BY_ZZF_NKTaxType = "BRR";
				AssertEquals("VatAmount is recalculated when Tax Type changes (BRR - no VAT Applicability)", 6.36m, goodsItem.VatAmount);

				goodsItem.BY_ZZF_NKTaxType = "BRP";
				AssertEquals("VatAmount is recalculated when Tax Type changes (BRP - with VAT Applicability)", 12.72m, goodsItem.VatAmount);

				goodsItem.BY_ZZF_NKTaxType = "ORD";
				AssertEquals("VatAmount is recalculated when Tax Type changes (ORD - with VAT Applicability)", 22.26m, goodsItem.VatAmount);
			});
		}

		public void TestRecalculateVatAmountAntidumpingAndCountervailing()
		{
			NCTSTestHelper.SetUpTariffAndAntidumpingAndCountervailing(Factory);
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.BY_MonetaryValue = 1_000m;
			goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

			var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC01";

			CombineAssertions(() =>
			{
				AssertEquals("[Precondition] Vat value (1000+120+4.3+4.3)", 225.72m, goodsItem.VatAmount);

				supplementaryCode1.CY_Code = "AC02";
				AssertEquals("VatAmount is recalculated when Antidumping value change(1000+120+24.3+4.3)", 229.72m, goodsItem.VatAmount);

				supplementaryCode1.CY_Code = "AC03";
				AssertEquals("VatAmount is recalculated when Countervailing value change(1000+120+4.3+25.3)", 229.92m, goodsItem.VatAmount);
			});
		}

		public void TestRecalculateVatAmountExciseAmount()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var departureGoodsItem = Factory.New<NctsDepartureCargoDescForTest>();
			departureGoodsItem.BY_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			departureGoodsItem.BY_ParentID = header.MovementHeader.PK;
			departureGoodsItem.BY_HarmonisedTariff = "0304798000";
			departureGoodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			departureGoodsItem.BY_MonetaryValue = 1_000m;

			CombineAssertions(() =>
			{
				AssertEquals(0m, departureGoodsItem.ExciseAmount);
				AssertEquals(120m, departureGoodsItem.DutyAmount);
				AssertEquals(224m, departureGoodsItem.VatAmount);

				departureGoodsItem.ExciseAmountForTesting = 120m;
				AssertEquals(120m, departureGoodsItem.ExciseAmount);
				AssertEquals("VatAmount is recalculated when ExciseAmount changes", 248m, departureGoodsItem.VatAmount);
			});
		}

		public void TestDefaultThirdQuantityUOMFromTariff_ExistingThirdUOM()
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

		public void TestDefaultThirdQuantityUOMFromTariff_ExistingThirdQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithOneUnit = helper.CreateTariff(CountryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.CustomsUOM3Type, "SSS");
			Factory.Save();

			goodsItem.BY_CustomsThirdQuantity = 2.4m;
			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("Customs Third Unit Qty (BY_CustomsThirdUnitQty) is set when it already has quantity set", "SSS", goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestDefaultThirdQuantityUOMFromTariff_InvalidTariff()
		{
			goodsItem.BY_HarmonisedTariff = "4444444444";
			AssertEquals("Customs Third Unit (BY_CustomsThirdUnitQty) is not set for invalid tariff", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestDefaultThirdQuantityUOMFromTariff_MultipleUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			var tariffWithMultipleUnits = helper.CreateTariff(CountryCode, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "NAR");
			helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.CustomsUOM3Type, "XXX");
			Factory.Save();

			goodsItem.BY_HarmonisedTariff = "1111111111";

			AssertCollectionContains("Customs Third Unit (BY_CustomsThirdUnitQty) has the collection if has multiple UOMS", goodsItem.BY_CustomsThirdUnitQty, new ZString[] { "NAR", "XXX" });
		}

		public void TestDefaultThirdQuantityUOMFromTariff_NoUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCode, TariffTypes.Import);
			Factory.Save();

			helper.CreateTariff(CountryCode, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			goodsItem.BY_CustomsThirdUnitQty = "AAA";
			goodsItem.BY_HarmonisedTariff = "3333333333";

			AssertEquals("Customs Third Unit (BY_CustomsThirdUnitQty) is set to empty if the tariff has no UOMS", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestDefaultThirdQuantityUOMFromTariff_RateUOM()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "MIL", "");

			goodsItem.BY_CustomsThirdQuantity = 2.4m;
			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("Customs Third Unit Qty (BY_CustomsThirdUnitQty) is the UOM of the rate Formula when there is no CU3", "MIL", goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestDefaultThirdQuantityUOMFromTariff_RateUOMIsConvertable()
		{
			NCTSTestHelper.SetUpTariffCustomRateFormula(Factory, CountryCode, "DTN", "NAR", true);

			goodsItem.BY_CustomsThirdQuantity = 2.4m;
			goodsItem.BY_HarmonisedTariff = "2222222222";

			AssertEquals("Customs Third Unit Qty (BY_CustomsThirdUnitQty) is NAR cause DTN is a conversion of KGM", "NAR", goodsItem.BY_CustomsThirdUnitQty);
		}

		public void TestGetFetchStrategy()
		{
			AssertType<NctsDepartureCargoDescFetchStrategy>(goodsItem.FetchStrategy);
		}

		public void TestConsigneeRequirementValidation()
		{
			var consignee = goodsItem.Consignee;
			consignee.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(consignee);
		}

		public void TestConsigneeRequirementValidation_WorkPhoneTR0079()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var departureGoodsItem = bill.GoodsItems.AddNew();

			var consignee = departureGoodsItem.Consignee;
			consignee.E2_AddressOverride = true;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(consignee,nctsHeader);
		}

		public void TestSelectedContainer_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();

			AssertEquals("SelectedContainers count should be equal to the number of selected container at package level - 1.", 1, goodsItem.SelectedContainers.Count());
			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = false;
			AssertEquals("SelectedContainers count should be equal to the number of selected container at package level - 0.", 0, goodsItem.SelectedContainers.Count());
		}

		public void TestSelectedContainer_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var movementHeader = nctsHeader.MovementHeader;
			var goodsItem = movementHeader.GoodsItems.AddNew();
			Factory.Save();

			AssertEquals("SelectedContainers count should be equal to the number of selected container at goodsItem level - 0.", 0, goodsItem.SelectedContainers.Count());
			var containerPivot = goodsItem.ContainersPivots[0];
			containerPivot.ContainerSelected = true;
			AssertEquals("SelectedContainers count should be equal to the number of selected container at goodsItem level - 1.", 1, goodsItem.SelectedContainers.Count());
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenB1820_1Active()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var message = $"[{ValidationRuleCodeConstants.B1820_1}] {MandatoryValidation.YouHaveNotEntered}";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var orgHeader = Factory.New<OrgHeader>();

			CombineAssertions(() =>
			{
				using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_1Active)))
				{
					using (SetTransitionPeriod(true))
					{
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("BM_TypeOfSecurity is EXI, no 30600 additional info", goodsItem.Consignee.OrganisationPKInfo, message);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on header level", goodsItem.Consignee.OrganisationPKInfo, message);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						var billAdditionalInfo = Add30600BillAdditionalInfo(bill.AdditionalDocuments);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on bill level", goodsItem.Consignee.OrganisationPKInfo, message);
						bill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						var goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on goods item level", goodsItem.Consignee.OrganisationPKInfo, message);
						goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("BM_TypeOfSecurity is BTH, no 30600 additional info", goodsItem.Consignee.OrganisationPKInfo, message);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on header level", goodsItem.Consignee.OrganisationPKInfo, message);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						billAdditionalInfo = Add30600BillAdditionalInfo(bill.AdditionalDocuments);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on bill level", goodsItem.Consignee.OrganisationPKInfo, message);
						bill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on goods item level", goodsItem.Consignee.OrganisationPKInfo, message);
						goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("BM_TypeOfSecurity isn't EXI or BTH", goodsItem.Consignee.OrganisationPKInfo, message);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("BM_RL_NKDestinationPort is C0009 code", goodsItem.Consignee.OrganisationPKInfo, message);

						movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
						goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("BY_RN_NKCountryOfDestination is C0009 code", goodsItem.Consignee.OrganisationPKInfo, message);

						nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("header.Consignee isn't empty", goodsItem.Consignee.OrganisationPKInfo, message);

						nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
						goodsItem.Consignee.OrganisationPK = orgHeader.PK;
						AssertNoMessageErrorContaining("goodsItem.Consignee isn't empty", goodsItem.Consignee.OrganisationPKInfo, message);

						goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
						AssertHasMessageErrorContaining("Precondition", goodsItem.Consignee.OrganisationPKInfo, message);
						goodsItem.Consignee.E2_AddressOverride = true;
						AssertHasMessageErrorContaining("Override is true but is empty", goodsItem.Consignee.OrganisationPKInfo, message);

						goodsItem.Consignee.E2_Address1 = "My Address";
						goodsItem.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Override is true and is not empty", goodsItem.Consignee.OrganisationPKInfo, message);
					}
				}

				using (SetTransitionPeriod(false))
				{
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Not in Transition Period", goodsItem.Consignee.OrganisationPKInfo, message);
				}
			});

			NctsAdditionalInfo Add30600AdditionalInfo(INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalInfoCollection)
			{
				var additionalInfo = additionalInfoCollection.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

				return additionalInfo;
			}

			NctsBillAdditionalDocument Add30600BillAdditionalInfo(INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> additionalInfoCollection)
			{
				var additionalInfo = additionalInfoCollection.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

				return additionalInfo;
			}
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenB1820_1NotActive()
		{
			var message = $"[{ValidationRuleCodeConstants.B1820_1}] {MandatoryValidation.YouHaveNotEntered}";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			using (SetTransitionPeriod(true))
			{
				ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(Factory, goodsItem.Consignee.OrganisationPKInfo, message, nameof(ValidationRuleConfiguration.IsRuleB1820_1Active), () => goodsItem.Consignee.Validation.ValidateOrganisationPK());
			}
		}

		public void TestRuleB1820_2WithCountrySetCL009AndConsignee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();
			var message = "[B1820-2] Consignee field must be empty";
			var orgHeader = Factory.New<OrgHeader>();
			var (nctsHeader, movementHeader, goodsItem) = RuleB1820_2SetUp();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			using (SetTransitionPeriod(true))
			{
				CombineAssertions("When RuleB1820_2 is active", () =>
				{
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Destination field is in not SET CL009, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("Destination field is in SET CL009, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Destination field is in SET CL009, header Consignee is not filled", goodsItem.Consignee.OrganisationPKInfo, message);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("goodsItem.Consignee is empty", goodsItem.Consignee.OrganisationPKInfo, message);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			{
				CombineAssertions("When RuleB1820_2 is inactive", () =>
				{
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Destination field is in SET CL009, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);
				});
			}
		}

		public void TestRuleB1820_2WithCountryNotSetCL009AndConsigneeAndSecuritySetNONOrENT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();
			var message = "[B1820-2] Consignee field must be empty";
			var orgHeader = Factory.New<OrgHeader>();
			var (nctsHeader, movementHeader, goodsItem) = RuleB1820_2SetUp();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			using (SetTransitionPeriod(true))
			{
				CombineAssertions("When RuleB1820_2 is active", () =>
				{
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("Destination field is not in SET CL009, BM_TypeOfSecurity is NON, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("Destination field is not in SET CL009, BM_TypeOfSecurity is ENT, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("goodsItem.Consignee is empty", goodsItem.Consignee.OrganisationPKInfo, message);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			{
				CombineAssertions("When RuleB1820_2 is inactive", () =>
				{
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Destination field is not in SET CL009, BM_TypeOfSecurity is NON, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("Destination field is not in SET CL009, BM_TypeOfSecurity is ENT, header Consignee is filled", goodsItem.Consignee.OrganisationPKInfo, message);
				});
			}
		}

		public void TestRuleB1820_2WithCountryNotSetCL009AndSecurityNotSetNONOrENTAndDocType30600()
		{
			var message = "[B1820-2] Consignee field must be empty";
			var orgHeader = Factory.New<OrgHeader>();
			var (nctsHeader, movementHeader, goodsItem) = RuleB1820_2SetUp();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			using (SetTransitionPeriod(true))
			{
				CombineAssertions("When RuleB1820_2 is active", () =>
				{
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is ENT, NON and no 30600 additional info on header, goodsItem level and header Consignee is empty", goodsItem.Consignee.OrganisationPKInfo, message);

					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and no 30600 additional info on header, goodsItem level", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and has 30600 additional info on header levels", goodsItem.Consignee.OrganisationPKInfo, message);
					nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					var goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and has 30600 additional info on goodsItem level", goodsItem.Consignee.OrganisationPKInfo, message);
					goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("goodsItem.Consignee is empty", goodsItem.Consignee.OrganisationPKInfo, message);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1820_2Active)))
			{
				CombineAssertions("When RuleB1820_2 is inactive", () =>
				{
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					goodsItem.Consignee.OrganisationPK = orgHeader.PK;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and no 30600 additional info on header, goodsItem level", goodsItem.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and has 30600 additional info on header levels", goodsItem.Consignee.OrganisationPKInfo, message);
					nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					var goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
					goodsItem.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is not ENT, NON and has 30600 additional info on goodsItem level", goodsItem.Consignee.OrganisationPKInfo, message);
					goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);
				});
			}

			NctsAdditionalInfo Add30600AdditionalInfo(INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalInfoCollection)
			{
				var additionalInfo = additionalInfoCollection.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

				return additionalInfo;
			}
		}

		public void TestValidateConsigneeDoesNotThrowExceptionAndSilentlyReportErrorWhenHeaderIsNull()
		{
			const string expectedErrorKey = "EU.NctsDepartureCargoDesc.ValidateConsignee: could not retrieve the 'Header' for this goods item (goodsItem.Header is null).";
			const string expectedReportedMessage =
				"See below debug information:\r\n" +
				"BY_ParentTableCode = ''\r\n" +
				"BY_ParentID = '00000000-0000-0000-0000-000000000000'\r\n" +
				"Is MoveHeaderOrBillParent null? Yes\r\n" +
				"Is MoveHeader null? Yes\r\n" +
				"Is Bill null? Yes";

			var goodsItem = Factory.New<NctsDepartureCargoDesc>();
			AssertNull("PRE-CONDITION: GoodsItem.Header", goodsItem.Header);

			goodsItem.Consignee.Validation.ValidateOrganisationPK();
			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertNoNotifications("No notifications on OrganisationPKInfo and -implicitly- no exceptions thrown", goodsItem.Consignee.OrganisationPKInfo);
				AssertEquals("ErrorReporter.LastKeyReported", expectedErrorKey, ErrorReporter.LastKeyReported);
				AssertEquals("ErrorReporter.LastMessageReported", expectedReportedMessage, ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		(NctsHeader, NctsDepartureMovementHeader, NctsDepartureCargoDesc) RuleB1820_2SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			return (nctsHeader, movementHeader, goodsItem);
		}

		public void TestGetTariffNomenclatureSelectionModes()
		{
			AssertArrayEqualsByElements($"Selection Modes", new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff }, goodsItem.GetTariffNomenclatureSelectionModes().ToArray());
		}

		public void TestBY_OP_Part_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_OP_PartInfo, "Product Code", "Prod. Code", "Product");
		}

		public void TestBY_BondedWhsQuantity_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_BondedWhsQuantityInfo, "Warehouse Quantity", "Warehouse Qty.", "Whs. Qty.");
		}

		public void TestBY_BondedWhsUnitQty_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_BondedWhsUnitQtyInfo, "Warehouse Unit of Quantity", string.Empty, string.Empty);
		}

		public void TestBY_WarehouseEntryNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_WarehouseEntryNumberInfo, "Previous Entry Number", "Prev. Entry No.", "Prev. Entry");
		}

		public void TestBY_WarehouseEntryLineNo_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_WarehouseEntryLineNoInfo, "Previous Entry Line", "Prev. Entry Line", "Prev. Line");
		}

		public void TestBY_BondedWHSOrderNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_BondedWHSOrderNumberInfo, "Warehouse Order Number", "Whs. Order No.", "Whs. Order");
		}

		public void TestBY_BondedWHSOrderLineNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_BondedWHSOrderLineNumberInfo, "Warehouse Order Line", "Whs. Order Line", "Whs. Line");
		}

		public void TestValidateConsigneeAddressPostcode_RuleE1102_1()
		{
			const string expectedWarningMessage = "Consignee Address Postcode is longer than 9 characters, it will be truncated in the message";

			var (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10) = SetupOrganizationsForRuleE1102_1();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			var consigneeAddressInfo = goodsItem.Consignee.E2_OA_AddressInfo;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active)))
			{
				using (SetTransitionPeriod(true))
				{
					goodsItem.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertHasWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);

					goodsItem.Consignee.OrganisationPK = organizationPostcodeLengthLessThan10.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode less than 10", consigneeAddressInfo, expectedWarningMessage);

					goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on is Empty", consigneeAddressInfo, expectedWarningMessage);
				}

				using (SetTransitionPeriod(false))
				{
					goodsItem.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is OFF and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);
				}
			}
			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active), value: false))
			using (SetTransitionPeriod(true))
			{
				goodsItem.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
				AssertNoWarningContaining("When RuleE1102_1 disabled and Transit period is on and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);
			}
		}

		void AssertCustomsQuantityDbValues(NctsDepartureCargoDesc bizObj, object expectedQtyInDb, object expectedUnitOfQtyInDb)
		{
			CombineAssertions("CusInBondCargoDesc CustomsQuantity values in the database.", () =>
			{
				var actualQtyInDb = TestConnection.ExecuteScalar($"SELECT BY_CustomsQuantity FROM dbo.CusInBondCargoDesc WHERE BY_PK = '{bizObj.PK}'");
				AssertEquals($"BY_CustomsQuantity (when set to {bizObj.BY_CustomsQuantity} in the Business Object)", expectedQtyInDb, actualQtyInDb);

				var actualUnitQtyInDb = TestConnection.ExecuteScalar($"SELECT BY_CustomsUnitQty FROM dbo.CusInBondCargoDesc WHERE BY_PK = '{bizObj.PK}'");
				AssertEquals($"BY_CustomsUnitQty (when set to {bizObj.BY_CustomsUnitQty} in the Business Object)", expectedUnitOfQtyInDb, actualUnitQtyInDb);
			});
		}

		public void TestCountNCTSPreviousDocuments() => CombineAssertions(() =>
		{
			AssertEquals("No documents", 0, goodsItem.NCTSPreviousDocumentsCount);

			goodsItem.PreviousDocuments.AddNew().CSI_Code = "N1";
			AssertEquals("1 NCTS documents", 1, goodsItem.NCTSPreviousDocumentsCount);

			goodsItem.PreviousDocuments.AddNew().CSI_Code = "X1";
			AssertEquals("1 NCTS documents and 1 other document", 1, goodsItem.NCTSPreviousDocumentsCount);

			goodsItem.PreviousDocuments.AddNew().CSI_Code = "N2";
			AssertEquals("2 NCTS documents", 2, goodsItem.NCTSPreviousDocumentsCount);
		});

		public void TestAllNCTSPreviousDocumentsCount()
		{
			header.PreviousDocuments.AddNew().CSI_Code = "N1";
			header.PreviousDocuments.AddNew().CSI_Code = "N2";
			header.PreviousDocuments.AddNew().CSI_Code = "X1";
			var bill = header.Bills.AddNew();
			bill.PreviousDocuments.AddNew().CSI_Code = "N3";
			bill.PreviousDocuments.AddNew().CSI_Code = "N4";
			bill.PreviousDocuments.AddNew().CSI_Code = "N5";
			bill.PreviousDocuments.AddNew().CSI_Code = "X2";
			var goodsItem = bill.GoodsItems.AddNew();
			goodsItem.PreviousDocuments.AddNew().CSI_Code = "N6";
			goodsItem.PreviousDocuments.AddNew().CSI_Code = "N7";
			goodsItem.PreviousDocuments.AddNew().CSI_Code = "X3";
			AssertEquals(7, goodsItem.AllNCTSPreviousDocumentsCount);
		}

		public void TestAllNCTSPreviousDocumentsCount_ForHeader()
		{
			goodsItem.PreviousDocuments.AddNew().CSI_Code = "N6";
			AssertEquals(1, goodsItem.AllNCTSPreviousDocumentsCount);
		}

		public void TestHas30600AdditionalInformation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodItems = bills.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When additionalDocument is null.", expected: false, goodItems.AdditionalInfos.Has30600AdditionalInformation());

				var additionalDocument = goodItems.AdditionalInfos.AddNew();
				additionalDocument.CSI_Code = "30601";
				additionalDocument.CSI_SubType = "INF";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type != 30600.", expected: false, goodItems.AdditionalInfos.Has30600AdditionalInformation());

				additionalDocument.CSI_Code = "30600";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type = 30600.", expected: true, goodItems.AdditionalInfos.Has30600AdditionalInformation());
			});
		}

		public void TestConsignee_ShouldHaveIgnoreValidationStatusErrorSet()
		{
			Assert(goodsItem.Consignee.IgnoreValidationStatusError);
		}

		public void TestBY_MonetaryValue_Readonly()
		{
			Assert(goodsItem.BY_MonetaryValueInfo.ReadOnly);
		}

		public void TestBY_MonetaryValue_ShouldBeCalculated_WhenLinePriceChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_StartDate = ZDateTime.Today;
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate.RE_SellRate = 1.5;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			Factory.Save();

			var testCases = new[]
			{
				new { Message = "Should be calculated for LinePrice = 15", LinePrice = 15, ExpectedMonetaryValue = 10m, ValuationDate = ZDateTime.Empty },
				new { Message = "Should be calculated for LinePrice = 30", LinePrice = 30, ExpectedMonetaryValue = 20m, ValuationDate = ZDateTime.Empty },
				new { Message = "Should be 0 for LinePrice = 0", LinePrice = 0, ExpectedMonetaryValue = 0m, ValuationDate = ZDateTime.Empty },
				new { Message = "Should be 0 for LinePrice < 0", LinePrice = -1, ExpectedMonetaryValue = 0m, ValuationDate = ZDateTime.Empty },
				new { Message = "Should not be calculated for ValuationDate not empty", LinePrice = 15, ExpectedMonetaryValue = 0m, ValuationDate = ZDateTime.Today },
			};

			foreach (var testCase in testCases)
			{
				nctsHeader.MovementHeader.BM_ValuationDate = testCase.ValuationDate;
				goodsItem.BY_LinePrice = testCase.LinePrice;

				AssertEquals("Precondition", Core.Constants.CurrencyCodes.EuropeanUnion, goodsItem.Bill.Header.Company.CustomsCurrency.Code);
				AssertEquals(testCase.Message, testCase.ExpectedMonetaryValue, goodsItem.BY_MonetaryValue);
			}
		}

		public void TestBY_MonetaryValue_ShouldBeCalculated_WhenLinePriceCurrencyChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			goodsItem.BY_LinePrice = 15;
			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate1 = Factory.New<RefExchangeRate>();
			exchangeRate1.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			exchangeRate1.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate1.RE_StartDate = ZDateTime.Today;
			exchangeRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate1.RE_SellRate = 1.5;
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			var exchangeRate2 = Factory.New<RefExchangeRate>();
			exchangeRate2.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
			exchangeRate2.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate2.RE_StartDate = ZDateTime.Today;
			exchangeRate2.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate2.RE_SellRate = 0.5;
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			Factory.Save();

			var testCases = new[]
			{
				new { Message = "Should be calculated for LinePriceCurrency = GBP", LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom, ExpectedMonetaryValue = 10m, ValuationDay = ZDateTime.Empty },
				new { Message = "Should be calculated for LinePriceCurrency = AUD", LinePriceCurrency = Core.Constants.CurrencyCodes.Australia, ExpectedMonetaryValue = 30m, ValuationDay = ZDateTime.Empty },
				new { Message = "Should be 0 for invalid LinePriceCurrency ", LinePriceCurrency = "RRR", ExpectedMonetaryValue = 0m, ValuationDay = ZDateTime.Empty },
				new { Message = "Should not be calculated for ValuationDate not empty", LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom, ExpectedMonetaryValue = 0m, ValuationDay = ZDateTime.Today },
			};

			foreach (var testCase in testCases)
			{
				nctsHeader.MovementHeader.BM_ValuationDate = testCase.ValuationDay;
				goodsItem.BY_RX_NKLinePriceCurrency = testCase.LinePriceCurrency;

				AssertEquals("Precondition", Core.Constants.CurrencyCodes.EuropeanUnion, goodsItem.Bill.Header.Company.CustomsCurrency.Code);
				AssertEquals(testCase.ExpectedMonetaryValue, goodsItem.BY_MonetaryValue);
			}
		}

		public void TestBY_LinePrice()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_LinePriceInfo, "Invoice or Commercial Price of Line Item", "Line Price", "Price");
		}

		public void TestBY_RX_NKLinePriceCurrency()
		{
			NCTSTestHelper.AssertCaptions(goodsItem.BY_RX_NKLinePriceCurrencyInfo, "Line Price Currency", string.Empty, "Currency");
		}

		public void TestCloneExcludesBY_DeclarationGoodsItemNumber_Phase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItem.BY_DeclarationGoodsItemNumber = 5;
			CombineAssertions(() =>
			{
				AssertEquals("Test BY_DeclarationGoodsItemNumber of the original", 5, goodsItem.BY_DeclarationGoodsItemNumber);
				var clonedGoodsItem = (NctsDepartureCargoDesc)goodsItem.Clone();
				AssertEquals("Test BY_DeclarationGoodsItemNumber is not copied when cloning", ZInt.Zero, clonedGoodsItem.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestCloneExcludesBY_DeclarationGoodsItemNumber_Phase4()
		{
			goodsItem.BY_DeclarationGoodsItemNumber = 5;
			CombineAssertions(() =>
			{
				AssertEquals("Test BY_DeclarationGoodsItemNumber of the original", 5, goodsItem.BY_DeclarationGoodsItemNumber);
				var clonedGoodsItem = (NctsDepartureCargoDesc)goodsItem.Clone();
				AssertEquals("Test BY_DeclarationGoodsItemNumber is not copied when cloning", ZInt.Zero, clonedGoodsItem.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestValuationDate()
		{
			header.BH_SystemCreateTimeUtc = ZDateTime.Empty;
			AssertEquals(ZDateTime.Today, goodsItem.ValuationDate);

			header.BH_SystemCreateTimeUtc = new ZDateTime(2024, 7, 24, 0, 0, 0, 1);
			AssertEquals(header.BH_SystemCreateTimeUtc, goodsItem.ValuationDate);

			var mrnNumber = header.MovementReferenceEntryNumber;
			mrnNumber.CE_SystemCreateTimeUtc = new ZDateTime(2024, 7, 24, 0, 0, 0, 2);
			AssertEquals(mrnNumber.CE_SystemCreateTimeUtc, goodsItem.ValuationDate);

			mrnNumber.CE_IssueDate = new ZDateTime(2024, 7, 24, 0, 0, 0, 3);
			AssertEquals(mrnNumber.CE_IssueDate, goodsItem.ValuationDate);

			header.MovementHeader.BM_ValuationDate = new ZDateTime(2024, 7, 24, 0, 0, 0, 4);
			AssertEquals(header.MovementHeader.BM_ValuationDate, goodsItem.ValuationDate);
		}

		public void TestBY_OP_Part_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_OP_PartInfo);
		}

		public void TestBY_BondedWhsQuantity_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_BondedWhsQuantityInfo);
		}

		public void TestBY_BondedWhsUnitQty_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_BondedWhsUnitQtyInfo);
		}

		public void TestBY_BY_WarehouseEntryNumber_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_WarehouseEntryNumberInfo);
		}

		public void TestBY_WarehouseEntryLineNo_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_WarehouseEntryLineNoInfo);
		}

		public void TestBY_BondedWHSOrderNumber_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_BondedWHSOrderNumberInfo);
		}

		public void TestBY_BondedWHSOrderLineNumber_ReadOnly()
		{
			TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(goodsItem.BY_BondedWHSOrderLineNumberInfo);
		}

		public void TestGetNctsPackageCollection()
		{
			AssertType<NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>>(goodsItem.Packages);
		}

		void TestPropertyReadOnlyForReadOnlyWarehouseStatusCodes(ZPropertyInfo propertyInfo)
		{
			foreach (var statusCode in ReadOnlyWarehouseStatusCodes)
			{
				CombineAssertions(statusCode, () =>
				{
					goodsItem.MoveHeader.BM_WarehouseTransactionStatus = string.Empty;
					AssertEquals(false, propertyInfo.ReadOnly);

					goodsItem.MoveHeader.BM_WarehouseTransactionStatus = statusCode;
					AssertEquals(true, propertyInfo.ReadOnly);
				});
			}
		}

		public void TestConsignee_ReadOnly()
		{
			using (SetTransitionPeriod(false))
			{
				AssertEquals("When is not in transition period, Consignee is not read only", false, goodsItem.Consignee.ReadOnly);
				goodsItem.Consignee.Delete();
			}

			using (SetTransitionPeriod(true))
			{
				AssertEquals("When is in transition period, Consignee is not read only", false, goodsItem.Consignee.ReadOnly);
			}
		}

		public void TestConsignee_ReadOnly_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

			using (SetTransitionPeriod(false))
			{
				AssertEquals("When is not in transition period, Consignee is read only", true, goodsItem.Consignee.ReadOnly);
				goodsItem.Consignee.Delete();
			}

			using (SetTransitionPeriod(true))
			{
				AssertEquals("When is in transition period, Consignee is not read only", false, goodsItem.Consignee.ReadOnly);
			}
		}

		public void TestBY_DescriptionReadOnly()
		{
			header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			CombineAssertions(() =>
			{
				AssertEquals("BY_Description is ReadOnly if Header.idDepartureReadonly is true", true, goodsItem.BY_DescriptionInfo.ReadOnly);
				header.MovementHeader.BM_CustomsStatus = "ABC";
				AssertEquals("BY_Description is not ReadOnly if Header.idDepartureReadonly is false", false, goodsItem.BY_DescriptionInfo.ReadOnly);
			});
		}

		public void TestUNDGs_ReadOnly()
		{
			header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			CombineAssertions(() =>
			{
				AssertEquals("UNDGs is ReadOnly if Header.idDepartureReadonly is true", true, goodsItem.UNDGs.ReadOnly);
				header.MovementHeader.BM_CustomsStatus = "ABC";
				AssertEquals("UNDGs is not ReadOnly if Header.idDepartureReadonly is false", false, goodsItem.UNDGs.ReadOnly);
			});
		}

		public void TestAdditionalSupplementaryCodes_ReadOnly()
		{
			header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalSupplementaryCodes is ReadOnly if Header.idDepartureReadonly is true", true, goodsItem.AdditionalSupplementaryCodes.ReadOnly);
				header.MovementHeader.BM_CustomsStatus = "ABC";
				AssertEquals("AdditionalSupplementaryCodes is not ReadOnly if Header.idDepartureReadonly is false", false, goodsItem.AdditionalSupplementaryCodes.ReadOnly);
			});
		}

		public void TestSuspendUpdateMonetaryValue()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			goodsItem.BY_LinePrice = 15;
			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate1 = Factory.New<RefExchangeRate>();
			exchangeRate1.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			exchangeRate1.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate1.RE_StartDate = ZDateTime.Today;
			exchangeRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate1.RE_SellRate = 1.3;
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			var exchangeRate2 = Factory.New<RefExchangeRate>();
			exchangeRate2.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Australia;
			exchangeRate2.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate2.RE_StartDate = ZDateTime.Today;
			exchangeRate2.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate2.RE_SellRate = 0.5;
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			Factory.Save();

			using (goodsItem.SuspendUpdateMonetaryValue())
			{
				goodsItem.BY_LinePrice = 64.0m;
				goodsItem.BY_RX_NKLinePriceCurrency = "GBP";
			}

			AssertEquals("Monetary value not updated - calculation suspended", 15.0m, goodsItem.BY_MonetaryValue);

			goodsItem.BY_LinePrice = 65.0m;
			AssertEquals("Monetary value updated when Line Price is set", 50.0m, goodsItem.BY_MonetaryValue);

			goodsItem.BY_RX_NKLinePriceCurrency = "AUD";
			AssertEquals("Monetary value updated when Line Price Currency is set", 130.0m, goodsItem.BY_MonetaryValue);
		}

		IList<string> ReadOnlyWarehouseStatusCodes => new[] {
			WarehouseTransactionStatusList.Codes.OutwardCreated,
			WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
			WarehouseTransactionStatusList.Codes.OutwardHolding,
			WarehouseTransactionStatusList.Codes.OutwardUpdated,
			WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
		};

		public void TestResetBY_DeclarationGoodsItemNumberOnDeleteAndAfterSaving_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var bill1 = nctsHeader.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var bill2 = nctsHeader.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = bill2.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;
			Factory.Save();

			CombineAssertions(() =>
			{
				bill2.Delete();
				AssertNotNull(nctsHeader);
				Factory.Save();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is 0 after deleting bill2", 0, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is 0 after deleting bill2", 0, goodItem12.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestResetBY_DeclarationGoodsItemNumberOnSaving_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var bill1 = nctsHeader.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var bill2 = nctsHeader.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = bill2.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			CombineAssertions(() =>
			{
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 before saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 before saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 before saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 before saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				Factory.Save();
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				var goodItem23 = bill2.GoodsItems.AddNew();
				Factory.Save();
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is 0 after saving, new item", 0, goodItem23.BY_DeclarationGoodsItemNumber);

				goodItem11.BY_DeclarationGoodsItemNumber = 1;
				goodItem12.BY_DeclarationGoodsItemNumber = 2;
				goodItem21.BY_DeclarationGoodsItemNumber = 3;
				goodItem22.BY_DeclarationGoodsItemNumber = 4;
				goodItem23.BY_DeclarationGoodsItemNumber = 5;
				Factory.Save();
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after second saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after second saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after second saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after second saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem23.BY_DeclarationGoodsItemNumber is not 0 after second saving", 5, goodItem23.BY_DeclarationGoodsItemNumber);

				goodItem21.BY_DeclarationGoodsItemNumber = 0;
				Factory.Save();
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is 0 after saving, the item changed", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is 0 after saving", 0, goodItem23.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestResetBY_DeclarationGoodsItemNumberOnSaving_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var goodItem11 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var goodItem21 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			CombineAssertions(() =>
			{
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 before saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 before saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 before saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 before saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				Factory.Save();
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				var goodItem23 = nctsHeader.MovementHeader.GoodsItems.AddNew();
				Factory.Save();
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When adding a new item with BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is 0 after saving, new item", 0, goodItem23.BY_DeclarationGoodsItemNumber);

				goodItem23.BY_DeclarationGoodsItemNumber = 5;
				Factory.Save();
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after second saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after second saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem21.BY_DeclarationGoodsItemNumber is not 0 after second saving", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after second saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When all items have BY_DeclarationGoodsItemNumber not 0,goodItem23.BY_DeclarationGoodsItemNumber is not 0 after second saving", 5, goodItem23.BY_DeclarationGoodsItemNumber);

				goodItem21.BY_DeclarationGoodsItemNumber = 0;
				Factory.Save();
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem11.BY_DeclarationGoodsItemNumber is not 0 after saving", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem12.BY_DeclarationGoodsItemNumber is not 0 after saving", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem21.BY_DeclarationGoodsItemNumber is 0 after saving, the item changed", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem22.BY_DeclarationGoodsItemNumber is not 0 after saving", 4, goodItem22.BY_DeclarationGoodsItemNumber);
				AssertEquals("When changing an item and setting BY_DeclarationGoodsItemNumber 0,goodItem23.BY_DeclarationGoodsItemNumber is not 0 after saving", 5, goodItem23.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestResetBY_DeclarationGoodsItemNumberOnDelete_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var bill1 = nctsHeader.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var bill2 = nctsHeader.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = bill2.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			CombineAssertions(() =>
			{
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				goodItem11.Delete();
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				Factory.Save();
				goodItem12.Delete();
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is 0 after deleting goodItem12, when in database", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is 0 after deleting goodItem12, when in database", 0, goodItem22.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestResetBY_DeclarationGoodsItemNumberOnDelete_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var goodItem11 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var goodItem21 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			CombineAssertions(() =>
			{
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a goodsItem", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				goodItem11.Delete();
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem11, when not in database", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				Factory.Save();
				goodItem12.Delete();
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem12, when in database", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting goodItem12, when in database", 4, goodItem22.BY_DeclarationGoodsItemNumber);
			});
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.MovementHeader.GoodsItems.AddNew();
			Factory.Save();
		}

		NctsHeader header;
		NctsDepartureCargoDesc goodsItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			return goodsItem;
		}

		IDisposable SetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		(OrgHeader organizationPostcodeLengthMoreThan9, OrgHeader organizationPostcodeLengthLessThan10) SetupOrganizationsForRuleE1102_1()
		{
			var organizationPostcodeLengthMoreThan9 = Factory.New<OrgHeader>();
			organizationPostcodeLengthMoreThan9.MainAddress.Postcode = "1234567890";

			var organizationPostcodeLengthLessThan10 = Factory.New<OrgHeader>();
			organizationPostcodeLengthLessThan10.MainAddress.Postcode = "123456789";
			return (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10);
		}
	}
}
