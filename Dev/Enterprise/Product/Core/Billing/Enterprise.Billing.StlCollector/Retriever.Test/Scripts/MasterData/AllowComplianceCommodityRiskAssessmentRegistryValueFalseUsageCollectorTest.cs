using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(AllowComplianceCommodityRiskAssessmentRegistryUsageCollector))]
	class AllowComplianceCommodityRiskAssessmentRegistryValueFalseUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
			INSERT INTO [dbo].[StmData]
				   ([SD_PK]
				   ,[SD_Name]
				   ,[SD_Owner]
				   ,[SD_DepartmentGuid]
				   ,[SD_Type]
				   ,[SD_IsLogged]
				   ,[SD_BinaryValue]
				   ,[SD_SystemLastEditTimeUtc])
			 VALUES
				   (NEWID()
				   ,'AllowComplianceCommodityRiskAssessment'
				   ,NEWID()
				   ,NEWID()
				   ,'BIN'
				   ,0
				   ,CAST(N'False' AS VARBINARY)
                   ,'2025-02-23 10:11:00')";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2025, 2);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("FeatureCode", "ROV", transaction1.PriceItemCode);
			AssertEquals("False", "False", transaction1.Reference1);
			AssertEquals("Transaction Time", new DateTime(2025, 2,23,10,11,0), transaction1.ServiceOccuredUTC);
		}
	}
}

