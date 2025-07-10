using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(NACCSUnitConverter))]
	sealed class NACCSUnitConverterTest : TestCaseWithFactory
	{
		public void TestIsCustomsWeightUnit()
		{
			var uqListForTest = new string[] { Weight.Kilograms, Weight.Tonnes, Weight.Pounds, Weight.Grams };

			AssertIsCustomsWeightUnit(JPProcedureCodeList.Codes.HCH01, Weight.Kilograms, Weight.Pounds);
			AssertIsCustomsWeightUnit(JPProcedureCodeList.Codes.HDF01, Weight.Kilograms);
			AssertIsCustomsWeightUnit(JPProcedureCodeList.Codes.NVC01, Weight.Kilograms, Weight.Pounds, Weight.Tonnes);
			AssertIsCustomsWeightUnit(JPProcedureCodeList.Codes.VAE, Weight.Kilograms, Weight.Pounds, Weight.Tonnes);
			AssertIsCustomsWeightUnit(JPProcedureCodeList.Codes.VAN, Weight.Kilograms, Weight.Pounds, Weight.Tonnes);
			AssertIsCustomsWeightUnit("", Weight.Kilograms, Weight.Pounds, Weight.Tonnes);

			void AssertIsCustomsWeightUnit(string procedureCode, params string[] customsWeightUnits)
			{
				foreach (var unit in uqListForTest)
				{
					if (customsWeightUnits.Contains(unit))
					{
						Assert(NACCSUnitConverter.IsCustomsWeightUnit(unit, procedureCode));
					}
					else
					{
						Assert(!NACCSUnitConverter.IsCustomsWeightUnit(unit, procedureCode));
					}
				}
			}
		}

		public void TestCalculateValueInCustomsUnit_Scale()
		{
			AssertEquals("Normal", 3.555m, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(3.555m, Weight.Kilograms));
			AssertEquals("VAE Scale", 3.556m, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(3.5556m, Weight.Kilograms, JPProcedureCodeList.Codes.VAE));
		}

		public void TestCalculateWeightInCustomsWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Kilograms", 1000m, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(1000m, Weight.Kilograms));
				AssertEquals("Tonnes", 1000m, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(1000m, Weight.Tonnes));
				AssertEquals("Pounds", 1000m, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(1000m, Weight.Pounds));
				AssertEquals("ShortTons", 907184.995886M, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(1000m, Weight.ShortTons));
				AssertEquals("ShortTons", 9071.850M, NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(10m, Weight.ShortTons, JPProcedureCodeList.Codes.VAN));
			});
		}

		public void TestRoundFiveDownSixUp()
		{
			CombineAssertions(() =>
			{
				AssertEquals("1.45m", 1.5m, NACCSUnitConverter.RoundToExpectedScaleNeeded(1.45m, 1));
				AssertEquals("1.46m", 1m, NACCSUnitConverter.RoundToExpectedScaleNeeded(1.45m, 0));
			});
		}

		public void TestIsCustomsVolumeUnit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BoardFoot is Customs Volume Unit", true, NACCSUnitConverter.IsCustomsVolumeUnit(VolumeList.Codes.BoardFoot));
				AssertEquals("CubicMetres is Customs Volume Unit", true, NACCSUnitConverter.IsCustomsVolumeUnit(Volume.CubicMetres));
				AssertEquals("CubicFeet is Customs Volume Unit", true, NACCSUnitConverter.IsCustomsVolumeUnit(Volume.CubicFeet));
				AssertEquals("Litre is not Customs Volume Unit", false, NACCSUnitConverter.IsCustomsVolumeUnit(Volume.Litre));
			});
		}

		public void TestConvertGrossWeightToCustomsWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Kilograms", 16m, NACCSUnitConverter.ConvertWeightToCustomsWeight(16m, Weight.Kilograms));
				AssertEquals("Tonnes", 16m, NACCSUnitConverter.ConvertWeightToCustomsWeight(16m, Weight.Tonnes));
				AssertEquals("Pounds", 16m, NACCSUnitConverter.ConvertWeightToCustomsWeight(16m, Weight.Pounds));
				AssertEquals("Kilotonnes", 16000000m, NACCSUnitConverter.ConvertWeightToCustomsWeight(16m, Weight.Kilotonnes));
			});
		}

		public void TestConvertJPCustomsWeightUnitToCW1WeightUnit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Kilograms", Core.Constants.Weight.Kilograms, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsWeightUnitList.Codes.Kilograms));
				AssertEquals("Tonnes", Core.Constants.Weight.Tonnes, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsWeightUnitList.Codes.Tonnes));
				AssertEquals("Gram", Core.Constants.Weight.Grams, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsWeightUnitList.Codes.Gram));
				AssertEquals("Pound", Core.Constants.Weight.Pounds, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsWeightUnitList.Codes.Pound));
				AssertEquals("Empty", string.Empty, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(""));
			});
		}

		public void TestConvertVolumeToCustomsVolume()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BoardFoot", 16m, NACCSUnitConverter.ConvertVolumeToCustomsVolume(16m, VolumeList.Codes.BoardFoot));
				AssertEquals("CubicMetres", 16m, NACCSUnitConverter.ConvertVolumeToCustomsVolume(16m, Volume.CubicMetres));
				AssertEquals("CubicFeet", 16m, NACCSUnitConverter.ConvertVolumeToCustomsVolume(16m, Volume.CubicFeet));
				AssertEquals("Litre", 0.016m, NACCSUnitConverter.ConvertVolumeToCustomsVolume(16m, Volume.Litre));
			});
		}

		public void TestConvertJPCustomsVolumeUnitToCW1VolumeUnit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CubicMeters", Core.Constants.Volume.CubicMetres, NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(CustomsVolumeUnitList.Codes.CubicMeters));
				AssertEquals("BoardFoot", VolumeList.Codes.BoardFoot, NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(CustomsVolumeUnitList.Codes.BoardFeet));
				AssertEquals("CubicFeet", Core.Constants.Volume.CubicFeet, NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(CustomsVolumeUnitList.Codes.CubicFeet));
				AssertEquals("Empty", string.Empty, NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(ZString.Empty));
			});
		}

		public void TestConvertCustomsWeightToCW1Weight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Kilograms", 16m, NACCSUnitConverter.ConvertCustomsWeightToCW1Weight(16m, CustomsWeightUnitList.Codes.Kilograms));
				AssertEquals("Tonnes", 16m, NACCSUnitConverter.ConvertCustomsWeightToCW1Weight(16m, CustomsWeightUnitList.Codes.Tonnes));
				AssertEquals("Pounds", 16m, NACCSUnitConverter.ConvertCustomsWeightToCW1Weight(16m, CustomsWeightUnitList.Codes.Gram));
				AssertEquals("Kilotonnes", 16m, NACCSUnitConverter.ConvertCustomsWeightToCW1Weight(16m, CustomsWeightUnitList.Codes.Pound));
			});
		}

		public void TestConvertCustomsVolumeToCW1Volume()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CubicMeters", 16m, NACCSUnitConverter.ConvertCustomsVolumeToCW1Volume(16m, CustomsVolumeUnitList.Codes.CubicMeters));
				AssertEquals("CubicFeet", 16m, NACCSUnitConverter.ConvertCustomsVolumeToCW1Volume(16m, CustomsVolumeUnitList.Codes.CubicFeet));
				AssertEquals("BoardFeet", 16m, NACCSUnitConverter.ConvertCustomsVolumeToCW1Volume(16m, CustomsVolumeUnitList.Codes.BoardFeet));
			});
		}
	}
}
