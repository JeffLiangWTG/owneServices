using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_EuNctsSealReport_Phase5))]
	class Report_EuNctsSealReport_Phase5Test : CustomsReportDbCreateScriptTest
	{
		public void TestJobNumber()
		{
			AssertFunctionReturnExpectedValue("JobNumber", "001");

			UpdateCusInBondHeaderColumn("BH_JobReference", "002", SqlDbType.VarChar, inBondHeaderPK);

			AssertFunctionReturnExpectedValue("JobNumber", "002");
		}

		public void TestDepartureStatus()
		{
			AssertFunctionReturnExpectedValue("DepartureStatus", "AWO");

			UpdateCusInBondMoveHeaderColumn("BM_CustomsStatus", "AWP", SqlDbType.VarChar, inBondMoveHeaderPK);

			AssertFunctionReturnExpectedValue("DepartureStatus", "AWP");
		}

		public void TestDeclarationType()
		{
			AssertFunctionReturnExpectedValue("DeclarationType", "");

			UpdateCusInBondMoveHeaderColumn("BM_InBondEntryType", "AAA", SqlDbType.VarChar, inBondMoveHeaderPK);

			AssertFunctionReturnExpectedValue("DeclarationType", "AAA");
		}

		public void TestDispatchCountry()
		{
			AssertFunctionReturnExpectedValue("DispatchCountry", "");

			UpdateCusInBondMoveHeaderColumn("BM_RN_NKCountryOfDispatch", "DE", SqlDbType.VarChar, inBondMoveHeaderPK);

			AssertFunctionReturnExpectedValue("DispatchCountry", "DE");
		}

		public void TestDestinationCountry()
		{
			AssertFunctionReturnExpectedValue("DestinationCountry", "");

			UpdateCusInBondMoveHeaderColumn("BM_RL_NKDestinationPort", "DE", SqlDbType.VarChar, inBondMoveHeaderPK);

			AssertFunctionReturnExpectedValue("DestinationCountry", "DE");
		}

		public void TestPlaceOfLoading()
		{
			AssertFunctionReturnExpectedValue("PlaceOfLoading", "");

			UpdateCusInBondMoveHeaderColumn("BM_PortOfPresentationCode", "DE", SqlDbType.VarChar, inBondMoveHeaderPK);

			AssertFunctionReturnExpectedValue("PlaceOfLoading", "DE");
		}

		public void TestShipmentNumber()
		{
			AssertFunctionReturnExpectedValue("ShipmentNumber", "");

			var shipmentPK = TestDataCreator.CreateShipment("NUM0001");

			UpdateCusInBondHeaderColumn("BH_ParentId", shipmentPK, SqlDbType.UniqueIdentifier, inBondHeaderPK);
			UpdateCusInBondHeaderColumn("BH_ParentTableCode", "JS", SqlDbType.VarChar, inBondHeaderPK);

			AssertFunctionReturnExpectedValue("ShipmentNumber", "NUM0001");
		}

		public void TestSealNumber()
		{
			AssertFunctionReturnExpectedValue("SealNumber", DBNull.Value);

			var testHelper = new TestDbHelper(Db.Connection);

			var container = Guid.NewGuid();
			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = container,
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "111",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 1",
				BC_Seal2 = "SEAL 2",
				BC_DataModel = "DENCT",
			});

			AssertContainsExactElementsInAnyOrder(new[] { "SEAL 1", "SEAL 2" }, GetFilteredRows("SealNumber").Select(s => s.ToString()));

			for (var i = 1; i <= 3; i++)
			{
				testHelper.Insert(CusSealSchema.Constants.TableName, new
				{
					BK_PK = Guid.NewGuid(),
					BK_SealNumber = $"SEAL {i + 2}",
					BK_ParentID = container,
					BK_ParentTableCode = "BC",
					BK_SequenceNumber = i,
				});
			}

			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = Guid.NewGuid(),
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "111",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 9",
				BC_DataModel = "DENCT",
			});

			AssertContainsExactElementsInAnyOrder(new[] { "SEAL 1", "SEAL 2", "SEAL 3", "SEAL 4", "SEAL 5", "SEAL 9" }, GetFilteredRows("SealNumber").Select(s => s.ToString()));
		}

		public void TestSealNumberPrecedence()
		{
			AssertFunctionReturnExpectedValue("SealNumberPrecedence", 0);
			var testHelper = new TestDbHelper(Db.Connection);

			var container = Guid.NewGuid();
			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = container,
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "111",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 1",
				BC_Seal2 = "SEAL 2",
				BC_DataModel = "DENCT",
			});

			for (var i = 1; i <= 3; i++)
			{
				testHelper.Insert(CusSealSchema.Constants.TableName, new
				{
					BK_PK = Guid.NewGuid(),
					BK_SealNumber = $"SEAL {i + 2}",
					BK_ParentID = container,
					BK_ParentTableCode = "BC",
					BK_SequenceNumber = i,
				});
			}

			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = Guid.NewGuid(),
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "111",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 9",
				BC_DataModel = "DENCT",
			});

			var resultList = new Dictionary<string,string>();
			var stringQueryBuilder = new StringBuilder();

			using (var command = Db.Connection.Command(string.Empty))
			{
				stringQueryBuilder.Append($"SELECT SealNumber, SealNumberPrecedence FROM Report_EuNctsSealReport_Phase5 (");

				foreach (var sqlParameter in GetSqlParameters())
				{
					AddParameter(Array.Empty<(string ParamName, object ParamValue)>(), command, sqlParameter);
					stringQueryBuilder.Append($" {sqlParameter.ParameterName},");
				}

				command.CommandText = $"{stringQueryBuilder.ToString().TrimEnd(',')})";
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add(reader[0].ToString(), reader[1].ToString());
					}
				}
			}
			AssertEquals("1", resultList["SEAL 1"]);
			AssertEquals("2", resultList["SEAL 2"]);
			AssertEquals("3", resultList["SEAL 3"]);
			AssertEquals("4", resultList["SEAL 4"]);
			AssertEquals("5", resultList["SEAL 5"]);
			AssertEquals("1", resultList["SEAL 9"]);
		}

		public void TestDepartureDate()
		{
			AssertFunctionReturnExpectedValue("DepartureDate", DBNull.Value);

			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2022, 12, 31), SqlDbType.DateTime, cenMRN);

			AssertFunctionReturnExpectedValue("DepartureDate", new DateTime(2022, 12, 31));
		}

		public void TestMovementReferenceNumber_WhenParentIsBH()
		{
			AssertFunctionReturnExpectedValue("MovementReferenceNumber", "MRN001");

			UpdateEntryNumberColumn("CE_EntryNum", "MRN002", SqlDbType.VarChar, cenMRN);

			AssertFunctionReturnExpectedValue("MovementReferenceNumber", "MRN002");
		}

		public void TestMovementReferenceNumber_WhenParentIsBM()
		{
			using var command = Db.Connection.Command("DELETE FROM dbo.CusEntryNum WHERE CE_PK = @cusEntryPK");
			command.AddParameter("@cusEntryPK", SqlDbType.UniqueIdentifier, cenMRN);
			command.ExecuteScalar();
			TestDataCreator.CreateCusEntryNum(inBondMoveHeaderPK, "CusInBondMoveHeader", "MRN002", "MRN", "CUS", "");

			AssertFunctionReturnExpectedValue("MovementReferenceNumber", "MRN002");
		}

		public void TestDepartureOffice_WhenParentIsBH()
		{
			AssertFunctionReturnExpectedValue("DepartureOffice", "");

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "DE020202", inBondHeaderPK, "BH");

			AssertFunctionReturnExpectedValue("DepartureOffice", "DE020202");
		}

		public void TestDepartureOffice_WhenParentIsBM()
		{
			AssertFunctionReturnExpectedValue("DepartureOffice", "");

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "DE020202", inBondMoveHeaderPK, "BM");

			AssertFunctionReturnExpectedValue("DepartureOffice", "DE020202");
		}

		public void TestDestinationOffice_WhenParentIsBH()
		{
			AssertFunctionReturnExpectedValue("DestinationOffice", "");

			TestDataCreator.CreateCusCodeData("EUO", "DES", "DE020202", inBondHeaderPK, "BH");

			AssertFunctionReturnExpectedValue("DestinationOffice", "DE020202");
		}

		public void TestDestinationOffice_WhenParentIsBM()
		{
			AssertFunctionReturnExpectedValue("DestinationOffice", "");

			TestDataCreator.CreateCusCodeData("EUO", "DES", "DE020202", inBondMoveHeaderPK, "BM");

			AssertFunctionReturnExpectedValue("DestinationOffice", "DE020202");
		}

		public void TestContainerEquipment()
		{
			AssertFunctionReturnExpectedValue("ContainerEquipment", DBNull.Value);

			var testHelper = new TestDbHelper(Db.Connection);
			var container1 = Guid.NewGuid();
			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = container1,
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "111",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 1",
				BC_DataModel = "DENCT",
			});

			AssertFunctionReturnExpectedValue("ContainerEquipment", "111");

			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = Guid.NewGuid(),
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "222",
				BC_ParentTableCode = "BH",
				BC_Seal1 = "SEAL 1",
				BC_Seal2 = "SEAL 2",
				BC_DataModel = "DENCT",
			});

			testHelper.Insert(CusInBondContainerSchema.Constants.TableName, new
			{
				BC_PK = Guid.NewGuid(),
				BC_ParentID = inBondHeaderPK,
				BC_ContainerNum = "333",
				BC_ParentTableCode = "BH",
				BC_DataModel = "DENCT",
			}); // not included as has no seals

			AssertContainsExactElementsInAnyOrder(new[] { "111", "222", "222" }, GetFilteredRows("ContainerEquipment").Select(s => s.ToString()));
		}

		public void TestPrincipalOAPK()
		{
			AssertFunctionReturnExpectedValue("PrincipalOAPK", addressPK);
			var addressPK2 = TestDataCreator.CreateAddress(organisationPK, "AD2", "ADDRESS 2");
			UpdateTableColumn("JobDocAddress", "E2_OA_Address", addressPK2, SqlDbType.UniqueIdentifier, "E2_PK", docAddressPK);

			AssertFunctionReturnExpectedValue("PrincipalOAPK", addressPK2);
		}

		public void TestFilterByDepartureDateRange()
		{
			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2020, 01, 01), SqlDbType.DateTime, cenMRN);

			var testData2 = GetTestData("MRN002", "002", "AD2", companyPK: companyPK, branchPK: branchPK);
			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2020, 01, 10), SqlDbType.DateTime, testData2.cenMRN);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.DateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.DateFrom, new DateTime(2020, 01, 01)), (Report_EuNctsSealReport_Phase5Parameters.DateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.DateFrom, new DateTime(2020, 01, 05)), (Report_EuNctsSealReport_Phase5Parameters.DateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.DateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new[] { "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.DateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);
		}

		public void TestFilterByPrincipalOrgHeaderPK()
		{
			var testData2 = GetTestData("MRN002", "002", "AD2", companyPK: companyPK, branchPK: branchPK);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "001", "002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.PrincipalOrgHeaderPK, organisationPK));
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_EuNctsSealReport_Phase5Parameters.PrincipalOrgHeaderPK, testData2.organisationPK));
			AssertContainsExactElementsInAnyOrder(new[] { "002" }, filteredRows);
		}

		public void TestFilterByDepartureOfficeCode()
		{
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "DE111111", inBondHeaderPK, "BH");

			var testData2 = GetTestData("MRN002", "002", "AD2", companyPK: companyPK, branchPK: branchPK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "DE222222", testData2.inBondHeaderPK, "BH");

			var testData3 = GetTestData("MRN003", "003", "AD3", companyPK: companyPK, branchPK: branchPK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "DE222222", testData3.inBondHeaderPK, "BH");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "001", "002", "003" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_EuNctsSealReport_Phase5Parameters.DepartureOfficeCode, "DE111111"));
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_EuNctsSealReport_Phase5Parameters.DepartureOfficeCode, "DE222222"));
			AssertContainsExactElementsInAnyOrder(new[] { "002", "003" }, filteredRows);
		}

		public void TestFilterByDestinationOfficeCode()
		{
			TestDataCreator.CreateCusCodeData("EUO", "DES", "DE111111", inBondHeaderPK, "BH");

			var testData2 = GetTestData("MRN002", "002", "AD2", companyPK: companyPK, branchPK: branchPK);
			TestDataCreator.CreateCusCodeData("EUO", "DES", "DE222222", testData2.inBondHeaderPK, "BH");

			var testData3 = GetTestData("MRN003", "003", "AD3", companyPK: companyPK, branchPK: branchPK);
			TestDataCreator.CreateCusCodeData("EUO", "DES", "DE222222", testData3.inBondHeaderPK, "BH");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "001", "002", "003" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_EuNctsSealReport_Phase5Parameters.DestinationOfficeCode, "DE111111"));
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_EuNctsSealReport_Phase5Parameters.DestinationOfficeCode, "DE222222"));
			AssertContainsExactElementsInAnyOrder(new[] { "002", "003" }, filteredRows);
		}

		public void TestFilterByCurrentCompany()
		{
			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "001" }, filteredRows);

			var newHeaderInNewCompany = GetTestData("MRN0002","002", "AD1", "DDE");
			var newHeaderInExistingCompany = GetTestData("MRN003", "003", "AD2", branchPK: branchPK, companyPK: companyPK);

			filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new [] { "001", "003" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			(companyPK, branchPK, inBondHeaderPK, cenMRN, inBondMoveHeaderPK, organisationPK, addressPK, docAddressPK) = GetTestData("MRN001", "001", "ADD");
		}

		(Guid companyPK, Guid branchPK, Guid inBondHeaderPK, Guid cenMRN, Guid inBondMoveHeaderPK, Guid organisationPK, Guid addressPK, Guid docAddressPK)
			GetTestData(string entryNum, string jobNumber, string addressCode, string companyCode = "DDD", Guid? companyPK = null, Guid? branchPK = null)
		{
			var company = companyPK ?? TestDataCreator.CreateCompany(companyCode: companyCode, countryCode: "DE", currencyCode: "EUR");
			var branch = branchPK ?? TestDataCreator.CreateBranch(company, companyCode, homePort: "DE");
			var inBondHeaderPK = TestDataCreator.CreateCusInbondHeader(jobNumber, branch, "NC5");
			var cenMRN = TestDataCreator.CreateCusEntryNum(inBondHeaderPK, "CusInBondHeader", entryNum, "MRN", "CUS", "");
			var inBondMoveHeaderPK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeaderPK, "D", "AWO");
			var organisationPK = TestDataCreator.CreateOrganisation(addressCode, addressCode);
			var addressPK = TestDataCreator.CreateAddress(organisationPK, addressCode, addressCode);
			var docAddressPK = TestDataCreator.CreateDocAddress(addressPK, addressCode, inBondHeaderPK, "BH", "PRC");
			UpdateCusInBondHeaderColumn("BH_HeaderType", "D", SqlDbType.VarChar, inBondHeaderPK);

			return (company, branch, inBondHeaderPK, cenMRN, inBondMoveHeaderPK, organisationPK, addressPK, docAddressPK);
		}

		protected override void AddParameter((string ParamName, object ParamValue)[] filters, DbCommand command,
			(SqlDbType ParameterType, string ParameterName) sqlParameter)
		{
			if (sqlParameter.ParameterName == Report_EuNctsSealReport_Phase5Parameters.CurrentCompany)
			{
				command.AddParameter(sqlParameter.ParameterName, sqlParameter.ParameterType, companyPK);
			}
			else
			{
				base.AddParameter(filters, command, sqlParameter);
			}
		}

		Guid companyPK;
		Guid branchPK;
		Guid inBondHeaderPK;
		Guid inBondMoveHeaderPK;
		Guid cenMRN;
		Guid addressPK;
		Guid organisationPK;
		Guid docAddressPK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsSealReport_Phase5Parameters.CurrentCompany);
			yield return (SqlDbType.SmallDateTime, Report_EuNctsSealReport_Phase5Parameters.DateFrom);
			yield return (SqlDbType.SmallDateTime, Report_EuNctsSealReport_Phase5Parameters.DateTo);
			yield return (SqlDbType.VarChar, Report_EuNctsSealReport_Phase5Parameters.DepartureOfficeCode);
			yield return (SqlDbType.VarChar, Report_EuNctsSealReport_Phase5Parameters.DestinationOfficeCode);
			yield return (SqlDbType.UniqueIdentifier, Report_EuNctsSealReport_Phase5Parameters.PrincipalOrgHeaderPK);
		}

		class Report_EuNctsSealReport_Phase5Parameters
		{
			public const string CurrentCompany = "@CurrentCompany";
			public const string DateFrom = "@DateFrom";
			public const string DateTo = "@DateTo";
			public const string PrincipalOrgHeaderPK = "@PrincipalOrgHeaderPK";
			public const string DepartureOfficeCode = "@DepartureOfficeCode";
			public const string DestinationOfficeCode = "@DestinationOfficeCode";
		}

		void UpdateCusInBondMoveHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondMoveHeader", columnName, columnValue, columnType, "BM_PK", primaryKeyValue);
		}

		void UpdateCusInBondHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondHeader", columnName, columnValue, columnType, "BH_PK", primaryKeyValue);
		}
	}
}
