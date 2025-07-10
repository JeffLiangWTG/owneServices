using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CashFlowDetailedListingTest : ScriptTest
	{
		[TestDate(2017, 01, 01)]
		public void TestCashFlowDescriptionWithMaxLength()
		{
			TestDescriptionMaxLength(false);
		}

		[TestDate(2017, 01, 01)]
		public void TestActivityDescriptionWithMaxLength()
		{
			TestDescriptionMaxLength(true);
		}

		void TestDescriptionMaxLength(bool isTestingActivityDescription)
		{
			var maxLengthToTest = isTestingActivityDescription ? CashFlowActivityConfiguration.Schema.ActivityDescriptionMaxLength : CashFlowActivityConfiguration.Schema.DescriptionMaxLength;
			var descriptionWithMaxLength = new string('a', maxLengthToTest);
			var shortDescription = "Test Cash Flow Activity";
			var cashFlowDescription = isTestingActivityDescription ? shortDescription : descriptionWithMaxLength;
			var activityDescription = isTestingActivityDescription ? descriptionWithMaxLength : shortDescription;

			var cashFlowConfiguration = CreateCashFlowConfiguration(cashFlowDescription, activityDescription);
			AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cashFlowConfiguration);
			CreateDirectReceipt(cashFlowConfiguration[0].Code);

			DataTable result = null;
			AssertNoExceptionThrown(() => result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_CashFlowDetailedListing(201701, '{GlbCompany.CurrentCompany.PK}','','')"));
			AssertEquals(2, result.Rows.Count);
			var columnName = isTestingActivityDescription ? "ActivityDescription" : "CashFlowDescription";
			AssertEquals(descriptionWithMaxLength, result.Rows[0][columnName]);
			AssertEquals(descriptionWithMaxLength, result.Rows[1][columnName]);
		}

		CashFlowActivityConfigurationCollection CreateCashFlowConfiguration(string cashFlowDescription, string activityDescription)
		{
			var configurationCollection = new CashFlowActivityConfigurationCollection();
			var configuration = configurationCollection.AddNew();
			configuration.Code = "ABC";
			configuration.EnglishDescription = cashFlowDescription;
			configuration.ActivityType = "C";
			configuration.EnglishActivityDescription = activityDescription;
			return configurationCollection;
		}

		void CreateDirectReceipt(ZString cashFlowActivityCode)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var glAccount = TestObjectCreator.CreateGLHeader();
			glAccount.AG_CashFlowType = cashFlowActivityCode;
			var directReceipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Today.AddDays(5), 10m, 0m, 20m, 0m);
			directReceipt.Lines[0].AL_AG = glAccount.PK;
			directReceipt.Lines[1].AL_AG = glAccount.PK;
			Factory.Save();
		}
	}
}
