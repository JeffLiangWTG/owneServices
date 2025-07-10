using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.IDEA;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.IDEA
{
	public class IDEAPersistentDataTest : TestCaseWithFactory
	{
		public void TestDefaultValues()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var persistentData = IDEAPersistentData.Load(complianceReport);
			AssertEquals("AccountBalancePerPeriod dictionary should be empty", 0, persistentData.AccountBalancePerPeriod.Count);
			AssertEquals("AccountLastPostDate dictionary should be empty", 0, persistentData.AccountLastPostDate.Count);
			AssertEquals("AllOrganisations hash-set should be empty", 0, persistentData.AllOrganisations.Count);
			AssertEquals("AllAccounts hast-set should be empty", 0, persistentData.AllAccounts.Count);
			var minimumMaximum = persistentData.CalculateMinMaxPeriod();
			AssertEquals("Period minimum", 999999, minimumMaximum.periodMinimum);
			AssertEquals("Period maximum", 0, minimumMaximum.periodMaximum);
		}

		public void TestSerialize()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var persistentData = IDEAPersistentData.Load(complianceReport);
			for (var i = 0; i < 12; i++)
			{
				persistentData.AccountBalancePerPeriod.Add(("10.10.0000", i + 24000), (i * 100, i * 200));
				persistentData.AccountLastPostDate.Add($"10.{i}.0000", new CargoWise.Types.ZDate(2022, i + 1, 25));
				persistentData.AllOrganisations.Add($"OHCODE{i + 100}");
				persistentData.AllAccounts.Add($"10.{i}.0000");
			}
			persistentData.Store();
			Factory.Save();

			var loadedData = IDEAPersistentData.Load(complianceReport);

			AssertContainsExactElementsInAnyOrder("accountBalancePerPeriod dictionary", loadedData.AccountBalancePerPeriod, persistentData.AccountBalancePerPeriod);
			AssertContainsExactElementsInAnyOrder("accountLastPostDate dictionary", loadedData.AccountLastPostDate, persistentData.AccountLastPostDate);
			AssertContainsExactElementsInAnyOrder("allOrganisations hash-set", loadedData.AllOrganisations, persistentData.AllOrganisations);
			AssertContainsExactElementsInAnyOrder("allAccounts hash-set", loadedData.AllAccounts, persistentData.AllAccounts);
			var minimumMaximum = persistentData.CalculateMinMaxPeriod();
			AssertEquals("Period minimum", 24000, minimumMaximum.periodMinimum);
			AssertEquals("Period maximum", 24011, minimumMaximum.periodMaximum);
		}

		public void TestDelete()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var persistentData = IDEAPersistentData.Load(complianceReport);
			persistentData.Store();
			Factory.Save();
			AssertEquals("StmNote record exists after saving", true, IDEAPersistentData.GetIDEANoteIfItExists(complianceReport) != null);
			persistentData.Delete();
			Factory.Save();
			AssertEquals("StmNote record exists after deleting", false, IDEAPersistentData.GetIDEANoteIfItExists(complianceReport) != null);
		}
	}
}
