using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(RegistryOverrideCollector))]
	class RegistryOverrideCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{ }

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("Registry Overridden", "0", transaction1.Reference1);

			var sqlQuery = @"
			INSERT INTO [dbo].[StmData]
				   ([SD_PK]
				   ,[SD_Name]
				   ,[SD_Owner]
				   ,[SD_DepartmentGuid]
				   ,[SD_Type]
				   ,[SD_IsLogged]
				   ,[SD_BinaryValue])
			 VALUES
				   (NEWID()
				   ,'DeniedPartyScreeningWebService'
				   ,NEWID()
				   ,NEWID()
				   ,'BIN'
				   ,0
				   ,CONVERT(VARBINARY(MAX), 'test'))";

			TestConnection.ExecuteNonQuery(sqlQuery);

			AssertEquals("Number of Transactions", 1, transactions.Count());
			transaction1 = transactions.First();
			AssertEquals("Registry Overridden", "0", transaction1.Reference1);
		}
	}
}

