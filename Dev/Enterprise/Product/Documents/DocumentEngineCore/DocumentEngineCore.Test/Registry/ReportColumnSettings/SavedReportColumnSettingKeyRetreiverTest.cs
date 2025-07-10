using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	class SavedReportColumnSettingKeyRetreiverTest : TestCaseWithFactory
	{
		public void TestSavedColumnSettingKeysForCurrentCompany()
		{
			ZGuid reportID1 = Guid.NewGuid();
			ZGuid reportID2 = Guid.NewGuid();
			ZGuid link1 = Guid.NewGuid();
			ZGuid link2 = Guid.NewGuid();

			ReportColumnSettingRegistryItem registryItem = new ReportColumnSettingRegistryItem();
			string description1 = "I was just a skinny lad";
			string description2 = "Never new no good from bad";
			string[] report1TestData = new string[]
			{
				"but I knew life before I left my nursery",
				"left alone with big fat fanny, she was such a naughty nanny",
				"hey big woman you made a bad boy out of me."
			};
			string report2TestData = "Test data courtesy of Queen";

			registryItem.SetValue(description1, reportID1, link2, report1TestData[0]);
			registryItem.SetValue(description1, reportID1, link1, report1TestData[0]);
			registryItem.SetValue(description2, reportID1, link1, report1TestData[0]);
			registryItem.SetValue(description1, reportID2, link1, report2TestData);

			ReportColumnSettingsRetriever retreiver = new ReportColumnSettingsRetriever();
			List<ReportColumnSettingKeys> savedSettings = retreiver.GetSavedColumnSettingKeysForCurrentCompany(reportID1);
			AssertEquals("There should be 3 saved settings for report 1", 3, savedSettings.Count);
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[0].Description, savedSettings[0].ReportID, savedSettings[0].LinkID)));
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[1].Description, savedSettings[1].ReportID, savedSettings[1].LinkID)));
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[2].Description, savedSettings[2].ReportID, savedSettings[2].LinkID)));

			savedSettings = retreiver.GetSavedColumnSettingKeysForCurrentCompany(reportID2);
			AssertEquals("There should be 1 saved setting for report 2", 1, savedSettings.Count);
			AssertEquals("Retreived data shoudl be form report 1 test data", report2TestData, registryItem.GetValueWithoutFallback(savedSettings[0].Description, savedSettings[0].ReportID, savedSettings[0].LinkID));
		}

		public void TestSavedColumnSettingKeysForAllCompanies()
		{
			ZGuid reportID1 = Guid.NewGuid();
			ZGuid reportID2 = Guid.NewGuid();
			ZGuid link1 = Guid.NewGuid();
			ZGuid link2 = Guid.NewGuid();

			ReportColumnSettingRegistryItem registryItem = new ReportColumnSettingRegistryItem();
			string description1 = "I was just a skinny lad";
			string description2 = "Never new no good from bad";
			string[] report1TestData = new string[]
			{
				"but I knew life before I left my nursery",
				"left alone with big fat fanny, she was such a naughty nanny",
				"hey big woman you made a bad boy out of me."
			};
			string report2TestData = "Test data courtesy of Queen";

			registryItem.SetValue(description1, reportID1, link2, report1TestData[0]);
			registryItem.SetValue(description1, reportID1, link1, report1TestData[1]);
			registryItem.SetValue(description2, reportID1, link1, report1TestData[2]);
			registryItem.SetValue(description1, reportID2, link1, report2TestData);

			ZQuery settingFilter = new ZQuery(StmDataSchema.SD_Owner, reportID1);
			settingFilter.AddToFilter(StmDataSchema.SD_DepartmentGuid, link1);
			settingFilter.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.EndsWith, description2);
			StmData data = Factory.LoadTop1<StmData>(settingFilter);
			AssertNotNull("StmData", data);
			data.SD_Name = data.SD_Name.Replace("$" + Env.CurrentCompany.Code + "$", "$SOMECOMPANY$");
			Factory.Save();

			ReportColumnSettingsRetriever retreiver = new ReportColumnSettingsRetriever();
			List<ReportColumnSettingKeys> savedSettings = retreiver.GetSavedColumnSettingKeysForCurrentCompany(reportID1);
			AssertEquals("There should be 2 saved settings for report 1", 2, savedSettings.Count);
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[0].Description, savedSettings[0].ReportID, savedSettings[0].LinkID)));
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[1].Description, savedSettings[1].ReportID, savedSettings[1].LinkID)));

			savedSettings = retreiver.GetSavedColumnSettingKeysForAllCompanies(reportID1);
			AssertEquals("There should be 3 saved settings for report 1", 3, savedSettings.Count);
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[0].Description, savedSettings[0].ReportID, savedSettings[0].LinkID)));
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[1].Description, savedSettings[1].ReportID, savedSettings[1].LinkID)));
			AssertEquals("Retreived data shoudl be form report 1 test data", true, string.Concat(report1TestData).Contains(registryItem.GetValueWithoutFallback(savedSettings[2].Description, savedSettings[2].ReportID, savedSettings[2].LinkID)));
		}
	}
}
