using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(Report_CommonDeclarations))]
	class Report_CommonDeclarationsTest : CustomsReportDbCreateScriptTest
	{
		public void TestFilterByCreateDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 11, 01), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 10, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 11, 30, hour: 23, minute: 59, second: 23), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportCommonDeclarationParameters.CreateDateFrom, new DateTime(2020, 11, 01)), (ReportCommonDeclarationParameters.CreateDateTo, new DateTime(2020, 11, 30)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestFilterByDepartureDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 12, 01), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 11, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 12, 31, hour: 23, minute: 59, second: 23), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportCommonDeclarationParameters.DepartureDateFrom, new DateTime(2020, 12, 01)), (ReportCommonDeclarationParameters.DepartureDateTo, new DateTime(2020, 12, 31)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestFilterByArrivalDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 12, 01), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 11, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 12, 31, hour: 23, minute: 59, second: 23), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportCommonDeclarationParameters.ArrivalDateFrom, new DateTime(2020, 12, 01)), (ReportCommonDeclarationParameters.ArrivalDateTo, new DateTime(2020, 12, 31)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestClusterKeyRetrieved()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "COM", 1);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "EXP", 3);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00004", "COM", 4);
			var filteredRows = GetFilteredRows(selectedColumn: "JE_ClusterKey", (ReportCommonDeclarationParameters.MessageType, "COM"));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new int[] { 1, 4 }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "AU", currencyCode: "AUD");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "SYDNEY");
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, ReportCommonDeclarationParameters.CurrentCompany);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.MessageType);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.TransportMode);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.Origin);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.Destination);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.Loading);
			yield return (SqlDbType.VarChar, ReportCommonDeclarationParameters.Arrival);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.CreateDateTo);
			yield return (SqlDbType.UniqueIdentifier, ReportCommonDeclarationParameters.ImporterPk);
			yield return (SqlDbType.UniqueIdentifier, ReportCommonDeclarationParameters.SupplierPk);
			yield return (SqlDbType.UniqueIdentifier, ReportCommonDeclarationParameters.BranchPk);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.DepartureDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.DepartureDateTo);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.ArrivalDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportCommonDeclarationParameters.ArrivalDateTo);
		}

		Guid companyPK;
		Guid branchPK;

		class ReportCommonDeclarationParameters
		{
			public const string CurrentCompany = "@CurrentCompany";
			public const string MessageType = "@MessageType";
			public const string TransportMode = "@TransportMode";
			public const string Origin = "@Origin";
			public const string Destination = "@Destination";
			public const string Loading = "@Loading";
			public const string Arrival = "@Arrival";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string ImporterPk = "@ImporterPk";
			public const string SupplierPk = "@SupplierPk";
			public const string BranchPk = "@BranchPk";
			public const string DepartureDateFrom = "@DepartureDateFrom";
			public const string DepartureDateTo = "@DepartureDateTo";
			public const string ArrivalDateFrom = "@ArrivalDateFrom";
			public const string ArrivalDateTo = "@ArrivalDateTo";
		}
	}
}
