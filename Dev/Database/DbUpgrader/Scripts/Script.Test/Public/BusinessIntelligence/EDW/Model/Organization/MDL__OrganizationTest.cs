using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Organization;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Organization.Testing
{
	[TestedType(typeof(MDL__Organization))]
	internal class MDL__OrganizationTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			//Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues("Test: Name 1, ", resultTable, 1, 0, 0, "(CIN) Company Inc", "CIN", 1, "Company Inc", "English", 0, 0);
				AssertRowValues("Test: Name 2, ", resultTable, 0, 1, 1, "(TIN) Tech Company", "TIN", 2, "Tech Company", "French", 1, 1);
				AssertRowValues("Test: Name 3, ", resultTable, 1, 1, 0, "(GVM) Government", "GVM", 3, "Government", "German", 0, 1);
			});
		}
		void AssertRowValues(string testName, DataTable resultTable, int? isActive, int? isForwarder, int? isWarehouseClient, string organization, string organizationCode, int? organizationKey, string organizationName, string language, int? isBroker, int? isPackDepot)
		{
			var selectqry = string.Format("[Is Active] {0} AND [Is Forwarder] {1} AND [Is Warehouse Client] {2} AND [Organization] {3} AND [Organization Code] {4} AND [Organization Key] {5} AND [Organization Name] {6} AND [Language] {7} AND [Is Broker] {8} AND [Is Pack Depot] {9}",
				isActive == null ? "IS NULL" : "= " + isActive,
				isForwarder == null ? "IS NULL" : "= " + isForwarder,
				isWarehouseClient == null ? "IS NULL" : "= " + isWarehouseClient,
				organization == null ? "IS NULL" : "= '" + organization + "'",
				organizationCode == null ? "IS NULL" : "= '" + organizationCode + "'",
				organizationKey == null ? "IS NULL" : "= " + organizationKey,
				organizationName == null ? "IS NULL" : "= '" + organizationName + "'",
				language == null ? "IS NULL" : "= '" + language + "'",
				isBroker == null ? "IS NULL" : "= " + isBroker,
				isPackDepot == null ? "IS NULL" : "= " + isPackDepot
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals(testName, 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
						"SELECT * FROM [{0}].[Organization].[MDL__Organization]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
				@"
				INSERT INTO [{0}].[Organization].[BAS__Organization]
					([Category], [ClosestPort], [Code], [FullName], [IsActive],
					[IsAirCTO], [IsAirLine], [IsAirWholesaler],
					[IsBroker], [IsCompetitor], [IsConsignee], [IsConsignor], [IsContainerYard], [IsControllingAgent], [IsDistributionCentre],
					[IsForwarder], [IsFumigationContractor], [IsGlobalAccount], [IsLineHaulProvider], [IslOcalTransport], [IsMiscFreightServices], [IsNationalAccount],
					[IsPackDepot], [IsPersonalEffectsAccount], [IsRailHead], [IsRailProvider], [IsRoadFreightDepot], [IsSalesLead], [IsSeaCTO], [IsSeaWholesaler], [IsShippingConsortium], [IsShippingLine], [IsShippingProvider], [IsTempAccount], [IsTransportClient], [IsUnpackDepot],
					[IsUserFlag1], [IsUserFlag2], [IsUserFlag3], [IsUserFlag4], [IsUserFlag5], [IsUserFlag6], [IsUserFlag7], [IsUserFlag8], [IsUserFlag9], [IsUserFlag10], [IsUserFlag11], [IsUserFlag12], [IsUserFlag13], [IsUserFlag14], [IsUserFlag15], [IsUserFlag16], [IsUserFlag17], [IsUserFlag18], [IsUserFlag19], [IsUserFlag20], [IsUserFlag21], [IsUserFlag22], [IsUserFlag23], [IsUserFlag24],
					[IsValid], [IsWarehouseClient], [Language], [OrganizationID], [OrganizationKey], [ScreeningStatus], [SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditUser]
					)
				VALUES
					('', 'FIRST', 'CIN', 'Company Inc', 1,
						0, 0, 0,
						0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						1, 0,  'English', 'F27E513C-6F18-43C1-B69B-73034917FCB9', 1, '', null, '~BP', '~BP'),
					('', 'FIRST', 'TIN', 'Tech Company', 0,
						0, 0, 0,
						1, 0, 0, 0, 0, 0, 0,
						1, 0, 0, 0, 0, 0, 0,
						1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						1, 1,  'French', 'EF24BEBF-4C9A-4A9C-AD33-C8C115E27F8C', 2, '', null, '~BP', '~BP'),
					('', 'FIRST', 'GVM', 'Government', 1,
						0, 0, 0,
						0, 0, 0, 0, 0, 0, 0,
						1, 0, 0, 0, 0, 0, 0,
						1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						1, 0,  'German', 'BB6080D5-B1A5-4BFF-B206-A97386FD4BE2', 3, '', null, '~BP', '~BP')
				",
				ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		//void Execute()
		//{
		//	string sqlText = String.Format(CultureInfo.InvariantCulture,
		//		//@"
		//		//INSERT INTO [{0}].[Organization].[GRP__Team] 
		//		//		   ([TeamCode]
		//		//		   ,[TeamName]
		//		//		   ,[ParentTeam])
		//		//SELECT[TeamCode]
		//		//	  ,[TeamName]
		//		//	  ,[ParentTeam]
		//		//FROM [{0}].[Organization].[vw_GRP__Team]",
		//		ScriptDbName
		//	);
		//	TestConnection.ExecuteNonQuery(sqlText);
		//}
	}
}
