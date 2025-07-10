using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class CommodityProviderTest : DataProviderTestCase<CommodityProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When 'item' is null",
				() => new CommodityProvider(item: null));

			AssertExceptionThrown<ArgumentNullException>(
				"When 'item.Header' is null",
				() => new CommodityProvider(item: Factory.New<NctsDepartureCargoDesc>()));
		}

		public void TestDescriptionOfGoods()
		{
			AssertEquals("desc", Provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			AssertEquals("14", Provider.CusCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("123456", Provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			var movementHeader = header.MovementHeader;

			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			_ = movementHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDeparture, "GB123456");
			AssertEquals("pre-req", "GB", movementHeader.DepartureCustomsOfficeCodeCountry);

			item.BY_HarmonisedTariff = "";
			AssertNull(Provider.CombinedNomenclatureCode);
			item.BY_HarmonisedTariff = "123456";
			AssertNull(Provider.CombinedNomenclatureCode);
			item.BY_HarmonisedTariff = "1234567890";
			AssertNull(Provider.CombinedNomenclatureCode);

			header.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			_ = movementHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDeparture, "XI123456");
			AssertEquals("pre-req", "XI", movementHeader.DepartureCustomsOfficeCodeCountry);
			item.BY_HarmonisedTariff = "";
			AssertNull(Provider.CombinedNomenclatureCode);
			item.BY_HarmonisedTariff = "123456";
			AssertNull(Provider.CombinedNomenclatureCode);
			item.BY_HarmonisedTariff = "1234567890";
			AssertEquals("78", Provider.CombinedNomenclatureCode);
		}

		public void TestDangerousGoods()
		{
			var dangerousGoods = Provider.DangerousGoods.Single();
			AssertEquals("SequenceNumber", 1, dangerousGoods.SequenceNumber);
		}

		public void TestGrossMass_InKilograms()
		{
			item.BY_GrossWeight = 1500m;
			CombineAssertions(() =>
			{
				item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				AssertEquals("BY_GrossWeightUnit is Kilograms", 1500m, new CommodityProvider(item).GrossMass);
				item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("BY_GrossWeightUnit is Grams", 1.5m, new CommodityProvider(item).GrossMass);
			});
		}

		public void TestGrossMass_Normalized()
		{
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			CombineAssertions(() =>
			{
				item.BY_GrossWeight = 10.0000m;
				AssertEquals("BY_GrossWeight = 10.0000", "10", new CommodityProvider(item).GrossMass.ToString());
				item.BY_GrossWeight = 10.2000m;
				AssertEquals("BY_GrossWeight = 10.2000", "10.2", new CommodityProvider(item).GrossMass.ToString());
				item.BY_GrossWeight = 10.2750m;
				AssertEquals("BY_GrossWeight = 10.2750", "10.275", new CommodityProvider(item).GrossMass.ToString());
			});
		}

		public void TestGrossMass_InTransitionPeriod()
		{
			item.BY_GrossWeight = 12345678.123456789m;
			item.BY_GrossWeightUnit = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 6 digits", 12345678.123457m, GetProvider().GrossMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 3 digits", 12345678.123m, GetProvider().GrossMass);
			});
		}

		public void TestNetMass_InKilograms()
		{
			item.BY_NetWeight = 1500m;
			CombineAssertions(() =>
			{
				item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				AssertEquals("BY_NetWeightUnit is Kilograms", 1500m, new CommodityProvider(item).NetMass);
				item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("BY_NetWeightUnit is Grams", 1.5m, new CommodityProvider(item).NetMass);
			});
		}

		public void TestNetMass_Normalized()
		{
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			CombineAssertions(() =>
			{
				item.BY_NetWeight = 10.0000m;
				AssertEquals("BY_NetWeight = 10.0000", "10", new CommodityProvider(item).NetMass.ToString());
				item.BY_NetWeight = 10.2000m;
				AssertEquals("BY_NetWeight = 10.2000", "10.2", new CommodityProvider(item).NetMass.ToString());
				item.BY_NetWeight = 10.2750m;
				AssertEquals("BY_NetWeight = 10.2750", "10.275", new CommodityProvider(item).NetMass.ToString());
			});
		}

		public void TestNetMass_InTransitionPeriod()
		{
			item.BY_NetWeight = 12345678.123456789m;
			item.BY_NetWeightUnit = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("NetMass is rounded to 6 digits", 12345678.123457m, GetProvider().NetMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("NetMass is rounded to 3 digits", 12345678.123m, GetProvider().NetMass);
			});
		}

		public void TestSupplementaryQty_Normalized()
		{
			item.BY_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			CombineAssertions(() =>
			{
				item.BY_CustomsSecondQuantity = 10.0000m;
				AssertEquals("BY_CustomsSecondQuantity = 10.0000", "10", new CommodityProvider(item).SupplementaryQty.ToString());
				item.BY_CustomsSecondQuantity = 10.2000m;
				AssertEquals("BY_CustomsSecondQuantity = 10.2000", "10.2", new CommodityProvider(item).SupplementaryQty.ToString());
				item.BY_CustomsSecondQuantity = 10.2750m;
				AssertEquals("BY_CustomsSecondQuantity = 10.2750", "10.275", new CommodityProvider(item).SupplementaryQty.ToString());
			});
		}

		protected override CommodityProvider GetProvider()
		{
			return new CommodityProvider(item);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			item = header.Bills.AddNew().GoodsItems.AddNew();
			item.BY_Description = "desc";
			item.BY_CusC4Number = "14";
			item.BY_HarmonisedTariff = "1234567890";
			item.BY_GrossWeight = 10;
			item.BY_NetWeight = 20;
			var dangerousGood = Factory.New<UNDGDataItem>();
			dangerousGood.DI_ParentID = item.PK;
			dangerousGood.DI_ParentTableCode = item.TablePrefix;
		}

		NctsHeader header;
		EU.NCTS.Business.NctsDepartureCargoDesc item;
	}
}
