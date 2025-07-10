using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMConsignmentItemProviderTest : Customs.Business.Testing.DataProviderTestCase<DESREMConsignmentItemProvider>
	{
		public void TestConstructor_NullArgument()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DESREMConsignmentItemProvider(null));
		}

		public void TestSequenceNumber()
		{
			cargoDesc.BY_LineNo = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestDescriptionOfGoods_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_Description = "DESCRIPTION";
			AssertEquals("DESCRIPTION", Provider.DescriptionOfGoods);
		}

		public void TestDescriptionOfGoods_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_Description = "DESCRIPTION";
			cargoDesc.UnloadedGoodsItem.BY_Description = "DESCRIPTION_UNLOADED";
			AssertEquals("DESCRIPTION_UNLOADED", Provider.DescriptionOfGoods);
		}

		public void TestDescriptionOfGoods_Other()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			cargoDesc.BY_Description = "DESCRIPTION";
			AssertNull(Provider.DescriptionOfGoods);
		}

		public void TestCusCode_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_CusC4Number = "12345";
			AssertEquals("12345", Provider.CusCode);
		}

		public void TestCusCode_SameBY_CusC4Number_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_CusC4Number = "12345";
			cargoDesc.UnloadedGoodsItem.BY_CusC4Number = "12345";
			AssertNull(Provider.CusCode);
		}

		public void TestCusCode_DifferentBY_CusC4Number_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_CusC4Number = "12345";
			cargoDesc.UnloadedGoodsItem.BY_CusC4Number = "54321";
			AssertEquals("54321", Provider.CusCode);
		}

		public void TestCusCode_Other()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			cargoDesc.BY_CusC4Number = "12345";
			AssertNull(Provider.CusCode);
		}

		public void TestHarmonizedSystemSubheadingCode_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			AssertEquals("123456", Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestHarmonizedSystemSubheadingCode_SameBY_HarmonisedTariff_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			cargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "12345678";
			AssertNull(Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestHarmonizedSystemSubheadingCode_DifferentBY_HarmonisedTariff_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			cargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "12345621";
			AssertEquals("123456", Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestHarmonizedSystemSubheadingCode_Other()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			AssertNull(Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestCombinedNomenclatureCode_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			AssertEquals("78", Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_Digit7And8Empty_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_HarmonisedTariff = "123456";
			AssertNull(Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_SameBY_HarmonisedTariff_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_HarmonisedTariff = "123456";
			cargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "123456";
			AssertNull(Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_DifferentBY_HarmonisedTariff_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			cargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "87654321";
			AssertEquals("21", Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_DifferentBY_HarmonisedTariff_Digit7And8Empty_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			cargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "876543";
			AssertNull(Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_Other()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			cargoDesc.BY_HarmonisedTariff = "12345678";
			AssertNull(Provider.CombinedNomenclatureCode);
		}

		public void TestGrossMass_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_GrossWeight = 1.2;
			AssertEquals(1.2m, Provider.GrossMass);
		}

		public void TestGrossMass_SameBY_GrossWeight_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_GrossWeight = 1.2;
			cargoDesc.UnloadedGoodsItem.BY_GrossWeight = 1.2;
			AssertNull(Provider.GrossMass);
		}

		public void TestGrossMass_DifferentBY_GrossWeight_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_GrossWeight = 1.2;
			cargoDesc.UnloadedGoodsItem.BY_GrossWeight = 1.3;
			AssertEquals(1.3m, Provider.GrossMass);
		}

		public void TestGrossMassConversionAndRounding()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_GrossWeight = 1123.567m;
			cargoDesc.BY_GrossWeightUnit = "G";
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMassNormalize()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_GrossWeight = 1.200m;
			cargoDesc.BY_GrossWeightUnit = "KG";
			AssertEquals("1.2", Provider.GrossMass.ToString());
		}

		public void TestNetMass_NEW()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_NetWeight = 1.2;
			AssertEquals(1.2m, Provider.NetMass);
		}

		public void TestNetMass_NeverZero()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_NetWeight = 0m;
			AssertNull(Provider.NetMass);
		}

		public void TestNetMass_SameBY_NetWeight_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_NetWeight = 1.2;
			cargoDesc.UnloadedGoodsItem.BY_NetWeight = 1.2;
			AssertNull(Provider.NetMass);
		}

		public void TestNetMass_DifferentBY_NetWeight_DIF()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			cargoDesc.BY_NetWeight = 1.2;
			cargoDesc.UnloadedGoodsItem.BY_NetWeight = 1.3;
			AssertEquals(1.3m, Provider.NetMass);
		}

		public void TestNetMassConversionAndRounding()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_NetWeight = 1123.4567m;
			cargoDesc.BY_NetWeightUnit = "G";
			AssertEquals(1.123457m, Provider.NetMass);
		}

		public void TestNetMassRoundingTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				cargoDesc.BY_NetWeight = 1.123567;
				cargoDesc.BY_NetWeightUnit = "KG";
				AssertEquals(1.124m, Provider.NetMass);
			}
		}

		public void TestNetMassNormalize()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			cargoDesc.BY_NetWeight = 1.200m;
			cargoDesc.BY_NetWeightUnit = "KG";
			AssertEquals("1.2", Provider.NetMass.ToString());
		}

		public void TestPackaging_NotMapped()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;

			var package = cargoDesc.Packages.AddNew();
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var package2 = cargoDesc.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			var package3 = cargoDesc.Packages.AddNew();
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("Not populated because BY_UnloadedState != 'DIF' or 'NEW'", Array.Empty<IDESREMPackage>(), Provider.Packaging);
		}

		public void TestPackaging()
		{
			cargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

			var package = cargoDesc.Packages.AddNew();
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var package2 = cargoDesc.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			var package3 = cargoDesc.Packages.AddNew();
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("Just populate if B5_TypeOfDifference = 'NEW' or 'MIS' AND BY_UnloadedState = 'DIF' or 'NEW'", 2, Provider.Packaging.Count);
		}

		protected override DESREMConsignmentItemProvider GetProvider() => new DESREMConsignmentItemProvider(cargoDesc);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			cargoDesc = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
		}
		NctsArrivalCargoDesc cargoDesc;

		new IDESREMConsignmentItem Provider => base.Provider;
	}
}
