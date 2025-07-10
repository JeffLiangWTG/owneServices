using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CustomsUnitOfMeasureList))]
	sealed class CustomsUnitOfMeasureListTest : TestCaseWithFactory
	{
		public void TestConvertGrossUnitsToJPCustomsGrossUnits()
		{
			CombineAssertions(() =>
			{
				AssertEquals("", CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits("", Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeDTN, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Decitons, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeGRM, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Grams, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeHGM, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Hectograms, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeKGM, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Kilograms, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeKTN, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Kilotonnes, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeLBR, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Pounds, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeLBT, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.PoundsTroy, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeCTM, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.MetricCarat, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeMGM, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Milligrams, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeAPZ, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.OuncesTroy, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeONZ, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Ounces, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeTNE, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.Tonnes, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeLTN, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.LongTons, Factory));
				AssertEquals(CustomsUnitOfMeasureList.CodeSTN, CustomsUnitOfMeasureList.ConvertGrossUnitsToJPCustomsGrossUnits(Core.Constants.Weight.ShortTons, Factory));
			});
		}

		public void TestConvertJPCustomsGrossUnitsToGrossUnits()
		{
			CombineAssertions(() =>
			{
				AssertEquals("", CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits("", Factory));
				AssertEquals(Core.Constants.Weight.Decitons, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeDTN, Factory));
				AssertEquals(Core.Constants.Weight.Grams, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeGRM, Factory));
				AssertEquals(Core.Constants.Weight.Hectograms, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeHGM, Factory));
				AssertEquals(Core.Constants.Weight.Kilograms, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeKGM, Factory));
				AssertEquals(Core.Constants.Weight.Kilotonnes, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeKTN, Factory));
				AssertEquals(Core.Constants.Weight.Pounds, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeLBR, Factory));
				AssertEquals(Core.Constants.Weight.PoundsTroy, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeLBT, Factory));
				AssertEquals(Core.Constants.Weight.MetricCarat, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeCTM, Factory));
				AssertEquals(Core.Constants.Weight.Milligrams, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeMGM, Factory));
				AssertEquals(Core.Constants.Weight.OuncesTroy, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeAPZ, Factory));
				AssertEquals(Core.Constants.Weight.Ounces, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeONZ, Factory));
				AssertEquals(Core.Constants.Weight.Tonnes, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeTNE, Factory));
				AssertEquals(Core.Constants.Weight.LongTons, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeLTN, Factory));
				AssertEquals(Core.Constants.Weight.ShortTons, CustomsUnitOfMeasureList.ConvertJPCustomsGrossUnitsToGrossUnits(CustomsUnitOfMeasureList.CodeSTN, Factory));
			});
		}

		public void TestGetVolumeUnitList()
		{
			AssertEquals("BF, CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", CustomsUnitOfMeasureList.GetVolumeUnitList(Factory).CodesAsString);
		}	
	}
}
