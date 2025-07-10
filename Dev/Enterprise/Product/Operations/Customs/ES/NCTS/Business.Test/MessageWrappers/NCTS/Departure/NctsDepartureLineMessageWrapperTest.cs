using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureLineMessageWrapperTest : WrapperHelperTest<NctsDepartureLineMessageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if good item is null", () => new NctsDepartureLineMessageWrapper(null));
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Expected filled GoodsItemNumber", 1, wrapper.GoodsItemNumber);
		}

		public void TestGoodsCustomsProcedureCategory1()
		{
			goodsItem.BY_HarmonisedTariff = "AH";
			AssertEquals("Expected filled GoodsCustomsProcedureCategory1", "AH", wrapper.GoodsCustomsProcedureCategory1);
		}

		public void TestGoodsDescription()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, parent: parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Commodity Code Description");
			Factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.BY_Description = "Desc";
				AssertEquals("Expected filled GoodsDescription when BY_Description is filled", "Desc", wrapper.GoodsDescription);

				goodsItem.BY_Description = ZString.Empty;
				AssertEquals("Expected empty GoodsDescription when BY_Description and Commodity code's description are empty", ZString.Empty, wrapper.GoodsDescription);

				goodsItem.BY_HarmonisedTariff = "0304798000";
				AssertEquals("Expected filled GoodsDescription when BY_Description is empty and Commodity code's description is filled", "Commodity Code Description", wrapper.GoodsDescription);

				goodsItem.BY_Description = "Goods Description";
				AssertEquals("Expected filled GoodsDescription (with BY_Description) when BY_Description is empty and Commodity code's description is filled", "Goods Description", wrapper.GoodsDescription);
			});
		}

		public void TestGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItem.BY_GrossWeight = 1.1;
				AssertEquals("Expected filled GrossWeightInKG with weight more than 1 rounded to the upper integer unit", new ZDecimal(2), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeight = 0.01;
				AssertEquals("Expected filled GrossWeightInKG with weight less than 1", new ZDecimal(1), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeight = 100.01;
				AssertEquals("Expected filled GrossWeightInKG round to the upper integer unit", new ZDecimal(101), wrapper.GrossWeightInKG);

				goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.ShortTons;
				goodsItem.BY_GrossWeight = 9;
				AssertEquals("Expected filled GrossWeightInKG with conversion from ShortTons to KG", new ZDecimal(8165), wrapper.GrossWeightInKG);
			});
		}

		public void TestExternalPackages()
		{
			foreach (string tag in ContainerTagsWithEmpty)
			{
				AddContainerForTest(nctsHeader, tag);
			}
			var externalPackages = wrapper.ExternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled ExternalPackages", ContainerTags.Length, externalPackages.NumberOfPackages);
				AssertSame("Cached ExternalPackages", wrapper.ExternalPackages, externalPackages);
			});

			void AddContainerForTest(NctsHeader nctsHeaderToTest, ZString containerTag)
			{
				var nctsHeaderContainer = nctsHeaderToTest.DepartureHeaderContainers.AddNew();
				nctsHeaderContainer.BC_ContainerNum = containerTag;
				var nonPersistentContainerPivot = goodsItem.ContainersPivots.AddNew();
				nonPersistentContainerPivot.Container = nctsHeaderContainer;
				nonPersistentContainerPivot.ContainerSelected = true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			wrapper = new NctsDepartureLineMessageWrapper(goodsItem);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsDepartureLineMessageWrapper wrapper;

		protected override NctsDepartureLineMessageWrapper GetProvider() => wrapper;
	}
}
