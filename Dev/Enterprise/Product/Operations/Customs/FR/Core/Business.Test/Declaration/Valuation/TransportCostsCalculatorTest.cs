using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class TransportCostsCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculationForEXW()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "EXW", "3", "AIR", "2");

				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "EXW", "3", "AIR", "3");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "EXW", "3", "AIR", "4");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.6m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.6m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.6m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.6m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "EXW", "3", "AIR", "5");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "EXW", "3", "SEA", "");
				AssertEquals(1.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForFCA()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FCA", "3", "AIR", "2");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FCA", "3", "AIR", "3");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FCA", "3", "AIR", "4");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.6m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.6m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.6m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.6m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FCA", "3", "AIR", "5");
				AssertEquals(4.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(7.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FCA", "3", "SEA", "");
				AssertEquals(1.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForCPT()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "1", "AIR", "4");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.4m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.6m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.5m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.6m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "1", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "2", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "2", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CPT", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForCIP()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "1", "AIR", "3");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "1", "AIR", "4");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.4m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.4m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.5m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.6m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "1", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(8.8m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "2", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "2", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIP", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForDPU()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "1", "AIR", "3");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(33.9m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(36.9m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "1", "AIR", "4");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.4m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.6m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.5m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.6m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "1", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "2", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "2", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DPU", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForDAP()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "1", "AIR", "3");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "1", "AIR", "4");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.4m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.4m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.5m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.5m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "1", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(8.8m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "2", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "2", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DAP", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForDDP()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "1", "AIR", "3");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "1", "AIR", "4");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(6.4m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(8.4m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(3.5m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(4.5m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "1", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(8.8m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "2", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(9.9m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(12.9m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "2", "AIR", "5");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(6.8m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(8.8m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(27.9m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(30.9m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "DDP", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(15.9m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(18.9m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForFAS()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FAS", "3", "SEA", "");
				AssertEquals(1.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForFOB()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FOB", "3", "SEA", "");
				AssertEquals(1.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FOB", "3", "AIR", "2");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FOB", "3", "AIR", "3");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(10.2m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(13.2m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "FOB", "3", "AIR", "5");
				AssertEquals(1.2m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(7.0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(9.0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForCFR()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CFR", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CFR", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(2.2m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		public void TestCalculationForCIF()
		{
			CombineAssertions(() =>
			{
				var calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIF", "1", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);

				calculator = new TransportCostCalculator(declaration.CustomsEntryHeaders[0], "CIF", "2", "SEA", "");
				AssertEquals(0m, calculator.CumulTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienTiers.Insurance.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulCEHorsFRInclus.Insurance.Amount);
				AssertEquals(22.2m, calculator.CumulCEHorsFRExclus.Costs.Amount);
				AssertEquals(25.2m, calculator.CumulCEHorsFRExclus.Insurance.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Costs.Amount);
				AssertEquals(0m, calculator.CumulAerienFR.Insurance.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Costs.Amount);
				AssertEquals(0m, calculator.CumulFRInclus.Insurance.Amount);
				AssertEquals(34.2m, calculator.CumulFRExclus.Costs.Amount);
				AssertEquals(37.2m, calculator.CumulFRExclus.Insurance.Amount);
			});
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();

			#region OFT

			var new_ThirdCountry_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_OFT_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_OFT_Included_Charge.J7_IsDutiable = false;
			new_EU_OFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_OFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_OFT_Included_Charge.J7_IsDutiable = false;
			new_Domestic_OFT_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_OFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region ONS

			var new_ThirdCountry_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ONS_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ONS_Included_Charge.J7_IsDutiable = false;
			new_EU_ONS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ONS_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ONS_Included_Charge.J7_IsDutiable = false;
			new_Domestic_ONS_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ONS_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region AFT

			var new_ThirdCountry_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_AFT_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_AFT_Included_Charge.J7_IsDutiable = false;
			new_EU_AFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_AFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_AFT_Included_Charge.J7_IsDutiable = false;
			new_Domestic_AFT_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_AFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region ANS#

			var new_ThirdCountry_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ANS_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ANS_Included_Charge.J7_IsDutiable = false;
			new_EU_ANS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ANS_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ANS_Included_Charge.J7_IsDutiable = false;
			new_Domestic_ANS_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ANS_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CEI

			var new_ThirdCountry_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEI_Included_Charge.J7_IsDutiable = false;
			new_EU_CEI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CEI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CNI

			var new_ThirdCountry_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNI_Included_Charge.J7_IsDutiable = false;
			new_EU_CNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CNI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CEE

			var new_ThirdCountry_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEE_Included_Charge.J7_IsDutiable = false;
			new_EU_CEE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CEE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CNE

			var new_ThirdCountry_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNE_Included_Charge.J7_IsDutiable = false;
			new_EU_CNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CNE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FRI

			var new_ThirdCountry_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRI_Included_Charge.J7_IsDutiable = false;
			new_EU_FRI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FRI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FNI

			var new_ThirdCountry_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNI_Included_Charge.J7_IsDutiable = false;
			new_EU_FNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FNI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FRE

			var new_ThirdCountry_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRE_Included_Charge.J7_IsDutiable = false;
			new_EU_FRE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FRE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FNE

			var new_ThirdCountry_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNE_Included_Charge.J7_IsDutiable = false;
			new_EU_FNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FNE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			foreach (var charge in invoiceHeader.Charges)
			{
				charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			}

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_CL = cusEntryLine.PK;

			Factory.Save();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
