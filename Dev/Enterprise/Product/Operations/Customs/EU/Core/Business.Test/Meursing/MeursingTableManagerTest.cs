using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Meursing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class MeursingTableManagerTest : TestCaseWithFactory
	{
		public void TestMeursingCalculator()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			TestScenario(6, 7, 2, 19, "7166", manager, lines);
			TestScenario(3, 65, 11, 5, "7903", manager, lines);
			TestScenario(4, 55, 2, 63, "", manager, lines);
			TestScenario(50, 4, 13, 7, "7515", manager, lines);
			TestScenario(88, 6, 20, 4, "7979", manager, lines);
			TestScenario(51, 29, 0.01m, 0.02m, "7016", manager, lines);
			TestScenario(24, 71, 10, 17, "7409", manager, lines);
			TestScenario(0.01m, 4, 90, 000, "7780", manager, lines);
			TestScenario(0.01m, 4, 90, 050, "7780", manager, lines);
			TestScenario(0.01m, 4, 90, 100, "7780", manager, lines);
		}

		void TestScenario(decimal starch, decimal sucrose, decimal milkFat, decimal milkProtein, string suffixExpected, MeursingTableManager manager, List<JobComInvoiceLine> lines)
		{
			manager.MeursingTable.StarchGlucose = starch;
			manager.MeursingTable.Sucrose = sucrose;
			manager.MeursingTable.MilkFat = milkFat;
			manager.MeursingTable.MilkProtein = milkProtein;
			manager.Execute();
			AssertEquals(suffixExpected, lines[0].JI_SupplementaryCode1);
			AssertEquals(lines[0].JI_SupplementaryCode1, lines[1].JI_SupplementaryCode1);
		}

		// All the below tests were written by Leon.  
		// Nuff respect to him.
		//		- DJC. 

		public void TestMeursingCalculatorColumn1()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 0.01m;
			decimal sugar = 0.01m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7000", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7020", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7040", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7060", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7080", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7800", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7100", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7120", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7140", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7160", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7180", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7820", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7840", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7200", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7200", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7260", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7260", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7260", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7860", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7860", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7300", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7360", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7360", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7360", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7900", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7900", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7400", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7460", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7460", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7460", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7940", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7940", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7500", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7560", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7560", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7560", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7960", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7960", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7600", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7600", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7600", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7600", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7980", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7980", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7700", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7700", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7700", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7700", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7720", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7740", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7760", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "7780", manager, lines);
		}

		public void TestMeursingCalculatorColumn2()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 0.01m;
			decimal sugar = 5m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7001", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7021", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7041", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7061", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7081", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7801", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7101", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7121", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7141", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7161", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7181", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7821", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7841", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7201", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7201", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7261", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7261", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7261", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7861", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7861", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7301", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7361", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7361", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7361", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7901", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7901", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7401", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7461", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7461", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7461", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7941", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7941", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7501", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7561", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7561", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7561", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7961", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7961", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7601", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7601", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7601", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7601", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7981", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7981", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7701", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7701", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7701", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7701", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7721", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7741", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7761", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "7781", manager, lines);
		}

		public void TestMeursingCalculatorColumn3()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 0.01m;
			decimal sugar = 30m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7002", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7022", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7042", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7062", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7082", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7802", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7102", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7122", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7142", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7162", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7182", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7822", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7842", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7202", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7202", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7262", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7262", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7262", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7862", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7862", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7302", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7362", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7362", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7362", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7902", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7902", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7402", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7462", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7462", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7462", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7942", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7942", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7502", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7562", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7562", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7562", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7962", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7962", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7602", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7602", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7602", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7602", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7982", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7982", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7702", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7702", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7702", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7702", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7722", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7742", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7762", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn4()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 0.01m;
			decimal sugar = 50m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7003", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7023", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7043", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7063", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7083", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7103", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7123", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7143", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7163", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7183", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7843", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7203", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7203", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7263", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7263", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7263", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7863", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7863", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7303", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7363", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7363", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7363", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7903", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7903", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7403", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7463", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7463", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7463", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7943", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7943", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7503", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7563", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7563", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7563", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7963", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7963", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7603", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7603", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7603", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7603", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7983", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7983", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7703", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7703", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7703", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7703", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7723", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn5()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 0.01m;
			decimal sugar = 70m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7004", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7024", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7044", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7064", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7084", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7104", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7124", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7144", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7164", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7844", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7204", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7204", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7264", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7264", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7264", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7864", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7864", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7304", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7364", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7364", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7364", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7904", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7904", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7404", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7464", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7464", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7464", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7944", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7944", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7504", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7564", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7564", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7564", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7964", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7964", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7604", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7604", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7604", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7604", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7984", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7984", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn6()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 5m;
			decimal sugar = 0.01m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7005", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7025", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7045", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7065", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7085", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7805", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7105", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7125", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7145", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7165", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7185", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7825", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7845", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7205", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7205", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7265", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7265", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7265", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7865", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7865", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7305", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7365", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7365", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7365", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7905", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7905", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7405", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7465", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7465", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7465", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7945", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7945", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7505", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7565", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7565", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7565", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7965", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7965", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7605", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7605", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7605", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7605", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7985", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7985", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7705", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7705", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7705", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7705", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7725", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7745", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7765", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "7785", manager, lines);
		}

		public void TestMeursingCalculatorColumn7()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 5m;
			decimal sugar = 5m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7006", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7026", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7046", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7066", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7086", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7806", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7106", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7126", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7146", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7166", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7186", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7826", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7846", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7206", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7206", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7266", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7266", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7266", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7866", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7866", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7306", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7366", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7366", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7366", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7906", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7906", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7406", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7466", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7466", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7466", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7946", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7946", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7506", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7566", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7566", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7566", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7966", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7966", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7606", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7606", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7606", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7606", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7986", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7986", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7706", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7706", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7706", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7706", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7726", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7746", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7766", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "7786", manager, lines);
		}

		public void TestMeursingCalculatorColumn8()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 5m;
			decimal sugar = 30m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7007", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7027", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7047", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7067", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7087", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7807", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7107", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7127", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7147", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7167", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7187", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7827", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7847", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7207", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7207", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7267", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7267", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7267", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7867", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7867", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7307", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7367", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7367", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7367", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7907", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7907", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7407", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7467", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7467", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7467", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7947", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7947", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7507", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7567", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7567", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7567", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7967", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7967", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7607", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7607", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7607", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7607", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7987", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7987", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7707", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7707", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7707", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7707", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7727", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7747", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn9()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 5m;
			decimal sugar = 50m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7008", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7028", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7048", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7068", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7088", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7108", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7128", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7148", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7168", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7188", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7848", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7208", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7208", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7268", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7268", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7268", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7868", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7868", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7308", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7368", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7368", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7368", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7908", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7908", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7408", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7468", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7468", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7468", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7948", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7948", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7508", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7568", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7568", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7568", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7968", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7968", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7608", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7608", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7608", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7608", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7988", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7988", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7708", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7708", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7708", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7708", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7728", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn10()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 5m;
			decimal sugar = 70m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7009", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7029", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7049", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7069", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7109", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7129", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7149", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7169", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7849", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7209", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7209", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7269", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7269", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7269", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7869", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7869", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7309", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7369", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7369", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7369", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7909", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7909", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7409", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7949", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7949", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7509", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7969", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7969", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7609", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7609", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7609", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7609", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn11()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 25m;
			decimal sugar = 0.01m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7010", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7030", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7050", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7070", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7090", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7810", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7110", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7130", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7150", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7170", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7190", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7830", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7850", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7210", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7210", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7270", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7270", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7270", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7870", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7870", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7310", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7370", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7370", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7370", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7910", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7910", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7410", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7470", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7470", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7470", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7950", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7950", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7510", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7570", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7570", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7570", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7970", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7970", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7610", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7610", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7610", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7610", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7990", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7990", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7710", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7710", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7710", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7710", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7730", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7750", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7770", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn12()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 25m;
			decimal sugar = 5m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7011", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7031", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7051", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7071", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7091", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "7811", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7111", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7131", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7151", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7171", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7191", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "7831", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7851", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7211", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7211", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7271", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7271", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7271", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7871", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7871", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7311", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7371", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7371", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7371", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7911", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7911", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7411", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7471", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7471", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7471", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7951", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7951", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7511", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7571", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7571", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7571", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7971", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7971", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7611", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7611", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7611", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7611", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7991", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7991", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7711", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7711", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7711", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7711", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7731", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "7751", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "7771", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn13()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 25m;
			decimal sugar = 30m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7012", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7032", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7052", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7072", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7092", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7112", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7132", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7152", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7172", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7192", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7852", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7212", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7212", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7272", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7272", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7272", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7872", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7872", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7312", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7372", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7372", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7372", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7912", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7912", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7412", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7472", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7472", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7472", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7952", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7952", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7512", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7572", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7572", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7572", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7972", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7972", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7612", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7612", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7612", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7612", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7992", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7992", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7712", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7712", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7712", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7712", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7732", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn14()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 25m;
			decimal sugar = 50m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7013", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7033", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7053", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7073", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7113", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7133", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7153", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7173", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7853", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7213", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7213", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7273", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7273", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7273", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7873", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7873", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7313", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7373", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7373", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7373", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7913", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7913", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7413", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7953", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7953", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7513", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7973", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7973", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7613", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7613", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7613", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7613", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn15()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 50m;
			decimal sugar = 0.01m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7015", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7035", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7055", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7075", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7095", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7115", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7135", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7155", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7175", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7195", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7855", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7215", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7215", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7275", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7275", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7275", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7875", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7875", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7315", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7375", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7375", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7375", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7915", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7915", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7415", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7475", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7475", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7475", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7955", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7955", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7515", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7575", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7575", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7575", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7975", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7975", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7615", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7615", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7615", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7615", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7995", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7995", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7715", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7715", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7715", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7715", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7735", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn16()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 50m;
			decimal sugar = 5m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7016", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7036", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7056", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7076", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "7096", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7116", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7136", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7156", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7176", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "7196", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7856", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7216", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7216", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7276", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7276", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7276", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7876", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7876", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7316", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7376", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7376", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7376", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7916", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7916", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7416", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "7476", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "7476", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "7476", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7956", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7956", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7516", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "7576", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "7576", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "7576", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7976", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7976", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7616", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7616", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7616", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7616", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "7996", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "7996", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "7716", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "7716", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "7716", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "7716", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "7736", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn17()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 50m;
			decimal sugar = 30m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7017", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7037", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7057", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7077", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7117", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7137", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7157", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7177", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7857", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7217", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7217", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7877", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7877", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7317", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7917", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7917", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7417", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7957", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7957", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7517", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7977", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7977", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn18()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 75m;
			decimal sugar = 0.01m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7758", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7768", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7778", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7788", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7798", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7808", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7818", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7828", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7858", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7220", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7220", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "7838", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "7838", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "7838", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7878", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7878", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7320", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "7378", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "7378", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "7378", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7918", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7918", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7420", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7958", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7958", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7520", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7978", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7978", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "7620", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "7620", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "7620", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "7620", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}

		public void TestMeursingCalculatorColumn19()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			var manager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			decimal starch = 75m;
			decimal sugar = 5m;

			TestScenario(starch, sugar, 0.01m, 0.01m, "7759", manager, lines);
			TestScenario(starch, sugar, 0.01m, 2.5m, "7769", manager, lines);
			TestScenario(starch, sugar, 0.01m, 6m, "7779", manager, lines);
			TestScenario(starch, sugar, 0.01m, 18m, "7789", manager, lines);
			TestScenario(starch, sugar, 0.01m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 0.01m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 1.5m, 0.01m, "7799", manager, lines);
			TestScenario(starch, sugar, 1.5m, 2.5m, "7809", manager, lines);
			TestScenario(starch, sugar, 1.5m, 6m, "7819", manager, lines);
			TestScenario(starch, sugar, 1.5m, 18m, "7829", manager, lines);
			TestScenario(starch, sugar, 1.5m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 1.5m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 3m, 0.01m, "7859", manager, lines);
			TestScenario(starch, sugar, 3m, 2.5m, "7221", manager, lines);
			TestScenario(starch, sugar, 3m, 6m, "7221", manager, lines);
			TestScenario(starch, sugar, 3m, 12m, "", manager, lines);
			TestScenario(starch, sugar, 3m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 3m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 6m, 0.01m, "7879", manager, lines);
			TestScenario(starch, sugar, 6m, 2.5m, "7879", manager, lines);
			TestScenario(starch, sugar, 6m, 4m, "7321", manager, lines);
			TestScenario(starch, sugar, 6m, 15m, "", manager, lines);
			TestScenario(starch, sugar, 6m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 6m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 9m, 0.01m, "7919", manager, lines);
			TestScenario(starch, sugar, 9m, 2.5m, "7919", manager, lines);
			TestScenario(starch, sugar, 9m, 6m, "7421", manager, lines);
			TestScenario(starch, sugar, 9m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 9m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 12m, 0.01m, "7959", manager, lines);
			TestScenario(starch, sugar, 12m, 2.5m, "7959", manager, lines);
			TestScenario(starch, sugar, 12m, 6m, "7521", manager, lines);
			TestScenario(starch, sugar, 12m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 12m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 18m, 0.01m, "7979", manager, lines);
			TestScenario(starch, sugar, 18m, 2.5m, "7979", manager, lines);
			TestScenario(starch, sugar, 18m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 18m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 26m, 0.01m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 2.5m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 6m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 18m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 30m, "", manager, lines);
			TestScenario(starch, sugar, 26m, 60m, "", manager, lines);

			TestScenario(starch, sugar, 40m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 55m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 70m, 0m, "", manager, lines);
			TestScenario(starch, sugar, 85m, 0m, "", manager, lines);
		}
	}
}
