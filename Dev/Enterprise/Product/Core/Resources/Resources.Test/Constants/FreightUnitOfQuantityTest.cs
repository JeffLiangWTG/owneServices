using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class FreightUnitOfQuantityTest : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConvertInvalidVolumeSource()
		{
			AssertEquals("FOOBLE to M3", 0.001M, Constants.Length.Convert(1M, "FO", "M3"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConvertInvalidVolumeDestination()
		{
			AssertEquals("M3 to FOOBLE", 0.001M, Constants.Length.Convert(1M, "M3", "FO"));
		}

		public void TestConvertSafe()
		{
			AssertEquals("CM to M3", 0M, Constants.Volume.ConvertSafe(1M, "CM", "M3"));
			AssertEquals("CY to CF", 27M, Constants.Volume.ConvertSafe(1M, "CY", "CF"));
			AssertEquals("1 Cubic metre in cubic inches, without rounding", 61023.7440947323M, Constants.Volume.ConvertSafe(1M, Constants.Volume.CubicMetres, Constants.Volume.CubicInches, false));

			AssertEquals("CM to M3", 0M, Constants.Weight.ConvertSafe(1M, "CM", "M3"));
			AssertEquals("KG to G", 1000M, Constants.Weight.ConvertSafe(1M, "KG", "G"));
			AssertEquals("Ounces to Kilograms, no rounding", 0.0283495231m, Constants.Weight.ConvertSafe(1m, Constants.Weight.Ounces, Constants.Weight.Kilograms, false));

			AssertEquals("CM to M3", 0M, Constants.Length.ConvertSafe(1M, "CM", "M3"));
			AssertEquals("M to CM", 100M, Constants.Length.ConvertSafe(1M, "M", "CM"));
			AssertEquals("Millimetre to Metre, without rounding.", 0.0012345678m, Constants.Length.ConvertSafe(1.2345678m, Constants.Length.Millimetres, Constants.Length.Metres, applyDefaultRounding: false));
		}

		public void TestConvertLength()
		{
			AssertEquals("Millimetre to Metre", 0.001M, Constants.Length.Convert(1M, Constants.Length.Millimetres, Constants.Length.Metres));
			AssertEquals("Metre to Millimetre", 1000M, Constants.Length.Convert(1M, Constants.Length.Metres, Constants.Length.Millimetres));

			AssertEquals("Inches to Centimetre", 2.54M, Constants.Length.Convert(1M, Constants.Length.Inches, Constants.Length.Centimetres));

			AssertEquals("Millimetre to Metre, with rounding.", 0.001235m, Constants.Length.Convert(1.2345678m, Constants.Length.Millimetres, Constants.Length.Metres));
			AssertEquals("Millimetre to Metre, without rounding.", 0.0012345678m, Constants.Length.Convert(1.2345678m, Constants.Length.Millimetres, Constants.Length.Metres, applyDefaultRounding: false));

			AssertEquals("Converting without a unit", 10M, Constants.Length.Convert(10M, "", ""));
			AssertEquals("Converting without a unit", 10M, Constants.Length.Convert(10M, "", Constants.Length.Centimetres));
			AssertEquals("Converting without a unit", 10M, Constants.Length.Convert(10M, Constants.Length.Inches, ""));
		}

		public void TestGetFeetFromFeetDecimal()
		{
			AssertEquals("10.5 ft -> 10 ft", 10, (int)Constants.Length.GetFeetFromFeetDecimal(10.5m));
			AssertEquals("10 ft -> 10 ft", 10, (int)Constants.Length.GetFeetFromFeetDecimal(10m));
			AssertEquals("0 ft -> 0 ft", 0, (int)Constants.Length.GetFeetFromFeetDecimal(0m));
		}

		public void TestGetInchesFromFeetDecimal()
		{
			AssertEquals("10.5 ft -> 6 inches", 6m, Constants.Length.GetInchesFromFeetDecimal(10.5m));
			AssertEquals("10 ft -> 0 inches", 0m, Constants.Length.GetInchesFromFeetDecimal(10m));
			AssertEquals("0 ft -> 0 inches", 0m, Constants.Length.GetInchesFromFeetDecimal(0m));
		}

		public void TestIsImperialVolume()
		{
			StringBuilder assertionMessage = new StringBuilder("Volume Codes Expected Length.");
			assertionMessage.AppendLine("If you have added a new Volume Code, ");
			assertionMessage.AppendLine("check whether it needs to be added to ");
			assertionMessage.AppendLine("the IsImperial() method in Constants.Volume");
			assertionMessage.AppendLine("Then correct the expected number below and ");
			assertionMessage.AppendLine("add an Assertion to the test below");
			AssertEquals(assertionMessage.ToString(), 11, Constants.Volume.Codes.Length);

			AssertIsImperialVolume(null, false);
			AssertIsImperialVolume(string.Empty, false);
			AssertIsImperialVolume(Constants.Volume.CubicDecimetres, false);
			AssertIsImperialVolume(Constants.Volume.CubicFeet, true);
			AssertIsImperialVolume(Constants.Volume.CubicInches, true);
			AssertIsImperialVolume(Constants.Volume.CubicMetres, false);
			AssertIsImperialVolume(Constants.Volume.CubicYards, true);
			AssertIsImperialVolume(Constants.Volume.Litre, false);
			AssertIsImperialVolume(Constants.Volume.MegaLitre, false);
			AssertIsImperialVolume(Constants.Volume.TeaChest, false);
			AssertIsImperialVolume(Constants.Volume.CubicCentimeters, false);
			AssertIsImperialVolume(Constants.Volume.USGallons, false);
			AssertIsImperialVolume(Constants.Volume.ImperialGallons, false);
		}

		void AssertIsImperialVolume(string volumeUnit, bool expected)
		{
			AssertEquals("Volume Unit [" + volumeUnit + "] is Imperial",
				expected, Constants.Volume.IsImperial(volumeUnit));
		}

		public void TestConvertVolume()
		{
			AssertEquals("Cubic yards to cubic feet", 27M, Constants.Volume.Convert(1M, Constants.Volume.CubicYards, Constants.Volume.CubicFeet));
			AssertEquals("1 Cubic metre in cubic inches", 61023.744095M, Constants.Volume.Convert(1M, Constants.Volume.CubicMetres, Constants.Volume.CubicInches));
			AssertEquals("1 Cubic metre in cubic inches, no rounding", 61023.7440947323M, Constants.Volume.Convert(1M, Constants.Volume.CubicMetres, Constants.Volume.CubicInches, false));
			AssertEquals("1 Cubic metre in cubic decimetres", 1000M, Constants.Volume.Convert(1M, Constants.Volume.CubicMetres, Constants.Volume.CubicDecimetres));
			AssertEquals("converting without a unit", 10M, Constants.Volume.Convert(10M, Constants.Volume.CubicMetres, ""));
			AssertEquals("1 Cubic metre in cubic centimeters", 1000000M, Constants.Volume.Convert(1M, Constants.Volume.CubicMetres, Constants.Volume.CubicCentimeters));
		}

		public void TestConvertWeight()
		{
			AssertEquals("Pounds to Grams", 453.59237M, Constants.Weight.Convert(1M, Constants.Weight.Pounds, Constants.Weight.Grams));
			AssertEquals("Metric Carat to Kilograms", 0.0002m, Constants.Weight.Convert(1m, Constants.Weight.MetricCarat, Constants.Weight.Kilograms));
			AssertEquals("Milligrams to Kilograms", 0.000001m, Constants.Weight.Convert(1m, Constants.Weight.Milligrams, Constants.Weight.Kilograms));
			AssertEquals("Hectograms to Kilograms", 0.1m, Constants.Weight.Convert(1m, Constants.Weight.Hectograms, Constants.Weight.Kilograms));
			AssertEquals("Decitons to Kilograms", 100m, Constants.Weight.Convert(1m, Constants.Weight.Decitons, Constants.Weight.Kilograms));
			AssertEquals("Kilotonnes to Kilograms", 1000000m, Constants.Weight.Convert(1m, Constants.Weight.Kilotonnes, Constants.Weight.Kilograms));
			AssertEquals("Ounces to Kilograms", 0.02835m, Constants.Weight.Convert(1m, Constants.Weight.Ounces, Constants.Weight.Kilograms));
			AssertEquals("Ounces to Kilograms, no rounding", 0.0283495231m, Constants.Weight.Convert(1m, Constants.Weight.Ounces, Constants.Weight.Kilograms, false));
			AssertEquals("Should not error", 1m, Constants.Weight.Convert(1m, "kg", Constants.Weight.Kilograms));
		}

		public void TestIsImperialWeight()
		{
			StringBuilder assertionMessage = new StringBuilder("Weight Codes Expected Length.");
			assertionMessage.AppendLine("If you have added a new Weight Code, ");
			assertionMessage.AppendLine("check whether it needs to be added to ");
			assertionMessage.AppendLine("the IsImperial() method in Constants.Weight");
			assertionMessage.AppendLine("Then correct the expected number below and ");
			assertionMessage.AppendLine("add an Assertion to the test below");
			AssertEquals(assertionMessage.ToString(), 14, Constants.Weight.Codes.Length);

			CombineAssertions(() =>
			{
				AssertIsImperialWeight(null, false);
				AssertIsImperialWeight(string.Empty, false);
				AssertIsImperialWeight(Constants.Weight.Decitons, false);
				AssertIsImperialWeight(Constants.Weight.Grams, false);
				AssertIsImperialWeight(Constants.Weight.Hectograms, false);
				AssertIsImperialWeight(Constants.Weight.Kilograms, false);
				AssertIsImperialWeight(Constants.Weight.Kilotonnes, false);
				AssertIsImperialWeight(Constants.Weight.LongTons, true);
				AssertIsImperialWeight(Constants.Weight.MetricCarat, false);
				AssertIsImperialWeight(Constants.Weight.Milligrams, false);
				AssertIsImperialWeight(Constants.Weight.Ounces, true);
				AssertIsImperialWeight(Constants.Weight.OuncesTroy, true);
				AssertIsImperialWeight(Constants.Weight.Pounds, true);
				AssertIsImperialWeight(Constants.Weight.PoundsTroy, true);
				AssertIsImperialWeight(Constants.Weight.ShortTons, true);
				AssertIsImperialWeight(Constants.Weight.Tonnes, false);
			});
		}

		void AssertIsImperialWeight(string weightUnit, bool expected)
		{
			AssertEquals("Volume Unit [" + weightUnit + "] is Imperial",
				expected, Constants.Weight.IsImperial(weightUnit));
		}

		public void TestConvertMultipleTypes()
		{
			AssertEquals("KG to KG", 1M, Constants.Weight.Convert(1M, Constants.Weight.Kilograms, Constants.Weight.Kilograms));
			AssertEquals("Pounds to Grams", 453.59237M, Constants.Weight.Convert(1M, Constants.Weight.Pounds, Constants.Weight.Grams));
			AssertEquals("Cubic yards to cubic feet", 27M, Constants.Volume.Convert(1M, Constants.Volume.CubicYards, Constants.Volume.CubicFeet));
			AssertEquals("Square Feet to Square Millimetres", 92903.04M, Constants.Area.Convert(1M, Constants.Area.SquareFoot, Constants.Area.SquareMillimetre));
		}

		public void TestConvertArea()
		{
			AssertEquals("Square Feet to Square Millimetres", 92903.04M, Constants.Area.Convert(1M, Constants.Area.SquareFoot, Constants.Area.SquareMillimetre));
			AssertEquals("Square Mile to Square Feet", 27878400M, Constants.Area.Convert(1M, Constants.Area.SquareMile, Constants.Area.SquareFoot));
			AssertEquals("Square Metre to Square Metres", 1000000M, Constants.Area.Convert(1M, Constants.Area.SquareKilometer, Constants.Area.SquareMetre));
			AssertEquals("Square Mile to Square Kilometers", 0.386102M, Constants.Area.Convert(1M, Constants.Area.SquareKilometer, Constants.Area.SquareMile));
		}

		public void TestConvertTemperature()
		{
			AssertEquals("Centigrade to Fahrenheit", 33.8m, Constants.Temperature.Convert(1, Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit));
			AssertEquals("Fahrenheit to Centigrade", -17.2222m, decimal.Round(Constants.Temperature.Convert(1, Constants.Temperature.Fahrenheit, Constants.Temperature.Centigrade), 4));
			AssertEquals("Centigrade to Kelvin", 274.15m, Constants.Temperature.Convert(1, Constants.Temperature.Centigrade, Constants.Temperature.Kelvin));
			AssertEquals("Fahrenheit to Kelvin", 255.9278m, decimal.Round(Constants.Temperature.Convert(1, Constants.Temperature.Fahrenheit, Constants.Temperature.Kelvin), 4));
			AssertExceptionThrown("Centigrade to JooblyBoobly", typeof(ArgumentException), () => Constants.Temperature.Convert(1, Constants.Temperature.Centigrade, "J"));
		}

		public void TestGetAreaUnitFromLengthUnit()
		{
			AssertEquals("area unit for length", Constants.Area.SquareMetre, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Metres));
			AssertEquals("area unit for length", Constants.Area.SquareCentimetre, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Centimetres));
			AssertEquals("area unit for length", Constants.Area.SquareMillimetre, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Millimetres));
			AssertEquals("area unit for length", Constants.Area.SquareInch, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Inches));
			AssertEquals("area unit for length", Constants.Area.SquareFoot, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Feet));
			AssertEquals("area unit for length", Constants.Area.SquareYard, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Yards));
			AssertEquals("area unit for length", Constants.Area.SquareKilometer, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Kilometres));
			AssertEquals("area unit for length", Constants.Area.SquareMile, Constants.Area.GetAreaUnitForLengthUnit(Constants.Length.Miles));
		}

		public void TestEmailAddresses()
		{
			AssertEquals("Email addresses should be correct", "EnterpriseSupportRequest@edi.net.au", Constants.EmailAddresses.SupportRequestEmailAddress);
			AssertEquals("Email addresses should be correct", "Eml2Fax@eagletrac4.eagletrac.com", Constants.EmailAddresses.EDI_FAX_GATEWAY);
		}

		public void TestUsaAndTerritoriesList()
		{
			List<string> list = new List<string>();
			list.AddRange(Constants.CountryCodes.UsaAndTerritoriesList);
			AssertEquals(6, list.Count);
			Assert(list.Contains(Constants.CountryCodes.UnitedStates));
			Assert(list.Contains(Constants.CountryCodes.PuertoRico));
		}

		public void TestIsUsaAndTerritory()
		{
			Assert(!Constants.CountryCodes.IsUsaOrTerritory(""));
			Assert(!Constants.CountryCodes.IsUsaOrTerritory("hello"));
			Assert(!Constants.CountryCodes.IsUsaOrTerritory(Constants.CountryCodes.Israel));
			Assert(!Constants.CountryCodes.IsUsaOrTerritory(Constants.CountryCodes.Jamaica));
			Assert(Constants.CountryCodes.IsUsaOrTerritory(Constants.CountryCodes.Guam));
			Assert(Constants.CountryCodes.IsUsaOrTerritory(Constants.CountryCodes.NorthernMarianaIslands));
			Assert(Constants.CountryCodes.IsUsaOrTerritory(Constants.CountryCodes.UnitedStates));
			foreach (string country in Constants.CountryCodes.UsaAndTerritoriesList)
			{
				Assert(Constants.CountryCodes.IsUsaOrTerritory(country));
			}
		}

		public void TestIsFilerIDEnabledUsaOrTerritory()
		{
			Assert(!Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(""));
			Assert(!Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory("hello"));
			Assert(!Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.Israel));
			Assert(!Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.Jamaica));
			Assert(!Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.Guam));
			Assert(Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.PuertoRico));
			Assert(Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.UnitedStates));
			Assert(Constants.CountryCodes.IsFilerIDEnabledUsaOrTerritory(Constants.CountryCodes.VirginIslands));
		}

		public void TestStaffDefaultCertificateIDAndTrainingTypes()
		{
			AssertEquals("APP", Constants.StaffDefaultCertificateIDAndTrainingTypes.APP);
			AssertEquals("BRK", Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK);
			AssertEquals("CAR", Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR);
			AssertEquals("DBH", Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH);
			AssertEquals("DGN", Constants.StaffDefaultCertificateIDAndTrainingTypes.DGN);
			AssertEquals("DTA", Constants.StaffDefaultCertificateIDAndTrainingTypes.DTA);
			AssertEquals("FIN", Constants.StaffDefaultCertificateIDAndTrainingTypes.FIN);
			AssertEquals("FKL", Constants.StaffDefaultCertificateIDAndTrainingTypes.FKL);
			AssertEquals("IAT", Constants.StaffDefaultCertificateIDAndTrainingTypes.IAT);
			AssertEquals("MSC", Constants.StaffDefaultCertificateIDAndTrainingTypes.MSC);
			AssertEquals("NID", Constants.StaffDefaultCertificateIDAndTrainingTypes.NID);
			AssertEquals("PAS", Constants.StaffDefaultCertificateIDAndTrainingTypes.PAS);
			AssertEquals("PID", Constants.StaffDefaultCertificateIDAndTrainingTypes.PID);
			AssertEquals("TFN", Constants.StaffDefaultCertificateIDAndTrainingTypes.TFN);
			AssertEquals("TRK", Constants.StaffDefaultCertificateIDAndTrainingTypes.TRK);
			AssertEquals("WKP", Constants.StaffDefaultCertificateIDAndTrainingTypes.WKP);

			AssertEquals("BCT", Constants.StaffDefaultCertificateIDAndTrainingTypes.BCT);
			AssertEquals("CON", Constants.StaffDefaultCertificateIDAndTrainingTypes.CON);
			AssertEquals("CDN", Constants.StaffDefaultCertificateIDAndTrainingTypes.CDN);
			AssertEquals("CDL", Constants.StaffDefaultCertificateIDAndTrainingTypes.CDL);
			AssertEquals("REP", Constants.StaffDefaultCertificateIDAndTrainingTypes.REP);
			AssertEquals("RTD", Constants.StaffDefaultCertificateIDAndTrainingTypes.RTD);
			AssertEquals("CAR", Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR);
			AssertEquals("EDL", Constants.StaffDefaultCertificateIDAndTrainingTypes.EDL);
			AssertEquals("HZM", Constants.StaffDefaultCertificateIDAndTrainingTypes.HZM);
			AssertEquals("LVC", Constants.StaffDefaultCertificateIDAndTrainingTypes.LVC);
			AssertEquals("MID", Constants.StaffDefaultCertificateIDAndTrainingTypes.MID);
			AssertEquals("NAI", Constants.StaffDefaultCertificateIDAndTrainingTypes.NAI);
			AssertEquals("NEX", Constants.StaffDefaultCertificateIDAndTrainingTypes.NEX);
			AssertEquals("OTD", Constants.StaffDefaultCertificateIDAndTrainingTypes.OTD);
			AssertEquals("PAS", Constants.StaffDefaultCertificateIDAndTrainingTypes.PAS);
			AssertEquals("PR1", Constants.StaffDefaultCertificateIDAndTrainingTypes.PR1);
			AssertEquals("PR2", Constants.StaffDefaultCertificateIDAndTrainingTypes.PR2);
			AssertEquals("SEN", Constants.StaffDefaultCertificateIDAndTrainingTypes.SEN);
			AssertEquals("AR1", Constants.StaffDefaultCertificateIDAndTrainingTypes.AR1);
			AssertEquals("AR2", Constants.StaffDefaultCertificateIDAndTrainingTypes.AR2);
			AssertEquals("MMD", Constants.StaffDefaultCertificateIDAndTrainingTypes.MMD);
			AssertEquals("USP", Constants.StaffDefaultCertificateIDAndTrainingTypes.USP);
			AssertEquals("VIM", Constants.StaffDefaultCertificateIDAndTrainingTypes.VIM);
			AssertEquals("VNI", Constants.StaffDefaultCertificateIDAndTrainingTypes.VNI);

			AssertEquals("ACE", Constants.StaffDefaultCertificateIDAndTrainingTypes.ACE);
			AssertEquals("APC", Constants.StaffDefaultCertificateIDAndTrainingTypes.APC);
		}

		public void TestIsCountryInEuropeanUnionAviationSecurityScheme()
		{
			Assert(!Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(""));
			Assert(!Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme("blah"));
			Assert(!Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Jamaica));
			Assert(!Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.UnitedArabEmirates));
			Assert("TR is not in EU", !Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Turkey));

			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Belgium));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Spain));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Italy));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Romania));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Iceland));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Liechtenstein));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Norway));
			Assert(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Constants.CountryCodes.Switzerland));
		}

		public void TestGetCountriesAndTerritoriesBelongingToCustomsJurisdiction()
		{
			var list = Constants.CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(Constants.CountryCodes.France);
			AssertEquals(8, list.Count());
			Assert(list.Contains(Constants.CountryCodes.France));
			Assert(list.Contains(Constants.CountryCodes.FrenchGuyana));
			Assert(list.Contains(Constants.CountryCodes.Guadeloupe));
			Assert(list.Contains(Constants.CountryCodes.Martinique));
			Assert(list.Contains(Constants.CountryCodes.Mayotte));
			Assert(list.Contains(Constants.CountryCodes.Reunion));
			Assert(list.Contains(Constants.CountryCodes.SaintBarthelemy));
			Assert(list.Contains(Constants.CountryCodes.SaintMartin));

			list = Constants.CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(Constants.CountryCodes.UnitedStates);
			AssertEquals(2, list.Count());
			Assert(list.Contains(Constants.CountryCodes.UnitedStates));
			Assert(list.Contains(Constants.CountryCodes.PuertoRico));

			list = Constants.CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(Constants.CountryCodes.Switzerland);
			AssertEquals(2, list.Count());
			Assert(list.Contains(Constants.CountryCodes.Switzerland));
			Assert(list.Contains(Constants.CountryCodes.Liechtenstein));

			list = Constants.CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(Constants.CountryCodes.Belgium);
			AssertEquals(1, list.Count());
			Assert(list.Contains(Constants.CountryCodes.Belgium));
		}
		public void TestFranceAndTerritoriesList()
		{
			var list = new List<string>();
			list.AddRange(Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories);
			AssertEquals(13, list.Count);
			Assert(list.Contains(Constants.CountryCodes.France));
			Assert(list.Contains(Constants.CountryCodes.FrenchGuyana));
			Assert(list.Contains(Constants.CountryCodes.FrenchPolynesia));
			Assert(list.Contains(Constants.CountryCodes.Guadeloupe));
			Assert(list.Contains(Constants.CountryCodes.Martinique));
			Assert(list.Contains(Constants.CountryCodes.Mayotte));
			Assert(list.Contains(Constants.CountryCodes.NewCaledonia));
			Assert(list.Contains(Constants.CountryCodes.StPierreEtMiquelon));
			Assert(list.Contains(Constants.CountryCodes.WallisAndFutunaIslands));
			Assert(list.Contains(Constants.CountryCodes.SaintBarthelemy));
			Assert(list.Contains(Constants.CountryCodes.SaintMartin));
		}

		public void TestIsFranceAndTerritory()
		{
			Assert(!Constants.CountryCodes.IsFranceOrTerritory(""));
			Assert(!Constants.CountryCodes.IsFranceOrTerritory("unknown"));
			Assert(!Constants.CountryCodes.IsFranceOrTerritory(Constants.CountryCodes.Belgium));

			Assert(Constants.CountryCodes.IsFranceOrTerritory(Constants.CountryCodes.Reunion));
			Assert(Constants.CountryCodes.IsFranceOrTerritory(Constants.CountryCodes.FrenchPolynesia));
			Assert(Constants.CountryCodes.IsFranceOrTerritory(Constants.CountryCodes.France));
			Assert(Constants.CountryCodes.IsFranceOrTerritory(Constants.CountryCodes.France + "A5N"));
			foreach (string country in Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories)
			{
				Assert(Constants.CountryCodes.IsFranceOrTerritory(country));
			}
		}

		public void TestIsFranceOrTerritoryNotReunion()
		{
			Assert(!Constants.CountryCodes.IsFranceOrTerritoryNotReunion(""));
			Assert(!Constants.CountryCodes.IsFranceOrTerritoryNotReunion("unknown"));
			Assert(!Constants.CountryCodes.IsFranceOrTerritoryNotReunion(Constants.CountryCodes.Belgium));

			Assert(!Constants.CountryCodes.IsFranceOrTerritoryNotReunion(Constants.CountryCodes.Reunion));
			Assert(Constants.CountryCodes.IsFranceOrTerritoryNotReunion(Constants.CountryCodes.FrenchPolynesia));
			Assert(Constants.CountryCodes.IsFranceOrTerritoryNotReunion(Constants.CountryCodes.France));
			Assert(Constants.CountryCodes.IsFranceOrTerritoryNotReunion(Constants.CountryCodes.France + "A5N"));
			foreach (string country in Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.Where(n => n != Constants.CountryCodes.Reunion))
			{
				Assert(Constants.CountryCodes.IsFranceOrTerritoryNotReunion(country));
			}
		}

		public void TestIsUnderFrenchCustomsJurisdiction()
		{
			Assert(!Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(""));
			Assert(!Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction("unknown"));
			Assert(!Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(Constants.CountryCodes.Belgium));

			Assert(!Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(Constants.CountryCodes.FrenchPolynesia));
			Assert(Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(Constants.CountryCodes.France));
			Assert(Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(Constants.CountryCodes.France + "PAR"));
			foreach (string country in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				Assert(Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(country));
			}
		}

		public void TestFranceAndOverseasDepartmentsUnderItsCustomsJurisdictionList()
		{
			var list = new List<string>();
			list.AddRange(Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction);
			AssertEquals(8, list.Count);
			Assert(list.Contains(Constants.CountryCodes.France));
			Assert(list.Contains(Constants.CountryCodes.FrenchGuyana));
			Assert(list.Contains(Constants.CountryCodes.Guadeloupe));
			Assert(list.Contains(Constants.CountryCodes.Martinique));
			Assert(list.Contains(Constants.CountryCodes.Mayotte));
			Assert(list.Contains(Constants.CountryCodes.Reunion));
			Assert(list.Contains(Constants.CountryCodes.SaintBarthelemy));
			Assert(list.Contains(Constants.CountryCodes.SaintMartin));
		}
	}
}
