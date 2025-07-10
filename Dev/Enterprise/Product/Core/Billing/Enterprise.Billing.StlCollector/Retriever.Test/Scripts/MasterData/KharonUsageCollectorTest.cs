using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(KharonUsageCollector))]
	class KharonUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = @"
		IF EXISTS(SELECT 1 FROM dbo.RefComplianceList WHERE RCL_ListCode = 'KN-SANOWN')
		BEGIN
			UPDATE dbo.RefComplianceList SET RCL_IsExcluded = 1, RCL_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE RCL_ListCode = 'KN-SANOWN';
		END
		ELSE
		BEGIN
		INSERT INTO dbo.RefComplianceList(RCL_Pk, RCL_IsActive, RCL_IsSystem, RCL_ListCode, RCL_ListName, RCL_ListPublisher, RCL_ListType, RCL_PublisherJurisdiction, RCL_IntegrationDate, RCL_SystemLastEditTimeUtc, RCL_SystemLastEditUser, RCL_IsExcluded, RCL_LastUpdatedDate) VALUES
		(NEWID(), 1, 1, 'KN-SANOWN', 'Kharon Sanctions Ownership List','Kharon', 'Sanctions Ownership', 'Global', '2020-07-22', '2024-03-29 05:29:00', 'E', 1, '2021-01-01');
		END
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("Exclusion value", "Excluded", transaction1.Reference1);
		}
	}
}

