using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.DE;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.DE.Testing
{
	[TestedType(typeof(Report_DEOpenSimplifiedDeclarations))]
	class Report_DEOpenSimplifiedDeclarationsTest : CustomsReportDbCreateScriptTest
	{
		public void TestJobNumber()
		{
			AssertFunctionReturnExpectedValue("JobNumber", "ANY001");

			UpdateJobDeclarationColumn("JE_DeclarationReference", "ANY002", SqlDbType.VarChar, jobDeclaration);

			AssertFunctionReturnExpectedValue("JobNumber", "ANY002");
		}

		public void TestOwnerReference()
		{
			AssertFunctionReturnExpectedValue("OwnerReference", "");

			UpdateJobDeclarationColumn("JE_OwnerRef", "ANY002", SqlDbType.VarChar, jobDeclaration);

			AssertFunctionReturnExpectedValue("OwnerReference", "ANY002");
		}

		public void TestEntryDate()
		{
			AssertFunctionReturnExpectedValue("EntryDate", entryDate);

			UpdateCusReconEntryColumn("CRE_EntryDate", new DateTime(2021, 2, 10), SqlDbType.SmallDateTime, reconEntry);

			AssertFunctionReturnExpectedValue("EntryDate", new DateTime(2021, 2, 10));
		}

		public void TestBranchCode()
		{
			AssertFunctionReturnExpectedValue("BranchCode", "TB1");

			var branchPK2 = TestDataCreator.CreateBranch(companyPK, "TB2", "DEBER");

			UpdateCusReconEntryColumn("CRE_GB_Branch", branchPK2, SqlDbType.UniqueIdentifier, reconEntry);

			AssertFunctionReturnExpectedValue("BranchCode", "TB2");
		}

		public void TestEntryType()
		{
			AssertFunctionReturnExpectedValue("EntryType", "VZL");

			UpdateCusReconEntryColumn("CRE_EntryType", "VZA", SqlDbType.VarChar, reconEntry);

			AssertFunctionReturnExpectedValue("EntryType", "VZA");
		}

		public void TestRegistrationNumber()
		{
			AssertFunctionReturnExpectedValue("RegistrationNumber", "ENT001");

			UpdateCusReconEntryColumn("CRE_OriginalEntryNumber", "ENT002", SqlDbType.VarChar, reconEntry);

			AssertFunctionReturnExpectedValue("RegistrationNumber", "ENT002");
		}

		public void TestLineNumber()
		{
			AssertFunctionReturnExpectedValue("LineNumber", (short)1);

			UpdateCusReconEntryLineColumn("CRL_OriginalEntryLineNumber", 2, SqlDbType.Int, reconLine);

			AssertFunctionReturnExpectedValue("LineNumber", (short)2);
		}

		public void TestStatus()
		{
			AssertFunctionReturnExpectedValue("Status", "RX5");

			UpdateCusReconEntryLineColumn("CRL_CustomsStatus", "TX5", SqlDbType.VarChar, reconLine);

			AssertFunctionReturnExpectedValue("Status", "TX5");
		}

		public void TestFilterByCurrentCompany()
		{
			var inNewCompany = CreateTestData(entryDate, "ANY002", "TB2");
			CreateTestData(entryDate, "ANY003", "TB3", companyPK: companyPK);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_DEOpenSimplifiedDeclarationsParameters.CompanyPk, inNewCompany.companyPK));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_DEOpenSimplifiedDeclarationsParameters.CompanyPk, companyPK));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001", "ANY003" }, filteredRows);
		}

		public void TestFilterByEntryDate()
		{
			CreateTestData(new DateTime(2020, 02, 10), "ANY002", companyPK: companyPK, branchPK: branchPK);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001", "ANY002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom, new DateTime(2020, 02, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001", "ANY002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom, new DateTime(2020, 02, 01)), (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateTo, new DateTime(2020, 02, 05)));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom, new DateTime(2020, 02, 05)), (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateTo, new DateTime(2020, 02, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom, new DateTime(2020, 02, 05)));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_DEOpenSimplifiedDeclarationsParameters.EntryDateTo, new DateTime(2020, 02, 04)));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001" }, filteredRows);
		}

		public void TestFilterByStatusCode()
		{
			CreateTestData(entryDate, "ANY002", entryType: "VZA", companyPK: companyPK, branchPK: branchPK);
			CreateTestData(entryDate, "ANY003", entryType: "VZA", companyPK: companyPK, branchPK: branchPK);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001", "ANY002", "ANY003" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_DEOpenSimplifiedDeclarationsParameters.EntryType, "VZL"));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber",
				(Report_DEOpenSimplifiedDeclarationsParameters.EntryType, "VZA"));
			AssertContainsExactElementsInAnyOrder(new[] { "ANY002", "ANY003" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryDate = new DateTime(2020, 2, 2);
			lastClusterKey = 1;
			var org = TestDataCreator.CreateOrganisation("DDE", "DDE");
			address = TestDataCreator.CreateAddress(org, "ANY", "ANY", "DE");
			(companyPK, branchPK, jobDeclaration, reconEntry, reconLine) = CreateTestData(entryDate);
		}

		(Guid companyPK, Guid branchPK, Guid declarationPK, Guid reconEntry, Guid reconLine)
			CreateTestData(DateTime entryDate, string jobNumber = "ANY001",  string companyCode = "TB1", string entryType = "VZL", Guid? companyPK = null, Guid? branchPK = null)
		{
			var clusterKey = lastClusterKey++;
			var company = companyPK ?? TestDataCreator.CreateCompany(companyCode, "DE", "DDE");
			var branch = branchPK ?? TestDataCreator.CreateBranch(company, companyCode, "DEBER");

			var jobDeclaration = TestDataCreator.CreateJobDeclaration(branch, company, jobNumber, "IMP", clusterKey, dataModel: "DE");

			var entryHeader = TestDataCreator.CreateCusEntryHeader(jobDeclaration, clusterKey, "IMP", dataModel: "DE");
			var reconEntry = CreateCusReconEntry(entryHeader, branch, address, entryDate, entryType, "ENT001");
			var reconLine = CreateCusReconEntryLine(reconEntry);

			return (company, branch, jobDeclaration, reconEntry, reconLine);
		}

		static Guid CreateCusReconEntry(
			Guid originalEntryNumberPk,
			Guid branchPk,
			Guid address,
			DateTime entryDate,
			string entryType = "",
			string originalEntryNumber = "",
			Guid? crePk = null
		)
		{
			crePk = crePk ?? Guid.NewGuid();
			const string sql = @"
INSERT INTO [dbo].[CusReconEntry] (
		[CRE_PK]
	   ,[CRE_CRD]
	   ,[CRE_OriginalEntryNumber]
	   ,[CRE_EntryDate]
	   ,[CRE_EntryType]
	   ,[CRE_GB_Branch]
	   ,[CRE_OA_DeclarantAddress]
	   ,[CRE_OA_ImporterAddress]
	   ,[CRE_OA_RepresentativeAddress]
	   ,[CRE_OA_BuyingAgentAddress]
	   ,[CRE_CH_OriginalEntry]
	   ,[CRE_SystemCreateTimeUtc]
	   ,[CRE_SystemCreateUser]
	   ,[CRE_SystemLastEditTimeUtc]
	   ,[CRE_SystemLastEditUser]
	) VALUES (
		@CRE_PK			--	 <CRE_PK, uniqueidentifier,>
		,@CRE_CRD		--  ,<CRE_CRD, uniqueidentifier,>
		,@originalNumber --  ,<CRE_OriginalEntryNumber, varchar(35),>
		,@entryDate		--	,<CRE_EntryDate, date,>
		,@entryType		--	,<CRE_EntryType, varchar(3),>
		,@branchPk		--  ,<CRE_GB_Branch, uniqueidentifier,>
		,@address		--	,<CRE_OA_DeclarantAddress, uniqueidentifier,>
		,NULL			--	,<CRE_OA_ImporterAddress, uniqueidentifier,>
		,NULL			--	,<CRE_OA_RepresentativeAddress, uniqueidentifier,>
		,NULL			--	,<CRE_OA_BuyingAgentAddress, uniqueidentifier,>
		,@cre_ch			--	,<CRE_CH_OriginalEntry, uniqueidentifier,>
		,GetUtcDate()	--	,<CRE_SystemCreateTimeUtc, smalldatetime,>
		,'~BP'			--	,<CRE_SystemCreateUser, varchar(3),>
		,GetUtcDate()   --	,<CRE_SystemLastEditTimeUtc, smalldatetime,>
		,'~BP'			--  ,<CRE_SystemLastEditUser, varchar(3),>
	)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cre_pk", SqlDbType.UniqueIdentifier, crePk.Value);
				command.AddParameter("@cre_crd", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@address", SqlDbType.UniqueIdentifier, address);
				command.AddParameter("@originalNumber", SqlDbType.VarChar, originalEntryNumber);

				command.AddParameter("@entryDate", SqlDbType.DateTime, entryDate);
				command.AddParameter("@entryType", SqlDbType.VarChar, entryType);
				command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPk);
				command.AddParameter("@cre_ch", SqlDbType.UniqueIdentifier, originalEntryNumberPk);

				command.ExecuteNonQuery();
			}

			return crePk.Value;
		}

		static Guid CreateCusReconEntryLine(
			Guid reconEntry,
			Guid? crlPk = null
		)
		{
			crlPk = crlPk ?? Guid.NewGuid();
			const string sql = @"
INSERT INTO [dbo].[CusReconEntryLine] (
			[CRL_PK]
		   ,[CRL_CRE]
		   ,[CRL_LineNumber]
		   ,[CRL_CustomsStatus]
		   ,[CRL_Description]
		   ,[CRL_OriginalEntryLineNumber]
		   ,[CRL_SystemCreateTimeUtc]
		   ,[CRL_SystemCreateUser]
		   ,[CRL_SystemLastEditTimeUtc]
		   ,[CRL_SystemLastEditUser]
	) VALUES (
		@crlPK			--	  <CRL_PK, uniqueidentifier,>
		,@CRL_CRE		--   ,<CRL_CRE, uniqueidentifier,>
		,1				--   ,<CRL_LineNumber, smallint,>
		,'RX5'			--   ,<CRL_CustomsStatus, varchar(3),>
		,''				--   ,<CRL_Description, varchar(512),>
		,1				--   ,<CRL_OriginalEntryLineNumber, smallint,>
		,GetUtcDate()	--   ,<CRL_SystemCreateTimeUtc, smalldatetime,>
		,'~BP'			--   ,<CRL_SystemCreateUser, varchar(3),>
		,GetUtcDate()   --   ,<CRL_SystemLastEditTimeUtc, smalldatetime,>
		,'~BP'			--   ,<CRL_SystemLastEditUser, varchar(3),>
	)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@crlPK", SqlDbType.UniqueIdentifier, crlPk.Value);
				command.AddParameter("@crl_cre", SqlDbType.UniqueIdentifier, reconEntry);

				command.ExecuteNonQuery();
			}

			return crlPk.Value;
		}

		Guid companyPK, branchPK;
		DateTime entryDate;
		Guid jobDeclaration;
		Guid reconEntry;
		Guid reconLine;
		Guid address;
		int lastClusterKey;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_DEOpenSimplifiedDeclarationsParameters.CompanyPk);
			yield return (SqlDbType.DateTime, Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom);
			yield return (SqlDbType.DateTime, Report_DEOpenSimplifiedDeclarationsParameters.EntryDateTo);
			yield return (SqlDbType.Char, Report_DEOpenSimplifiedDeclarationsParameters.EntryType);
		}

		protected override void AddParameter((string ParamName, object ParamValue)[] filters, DbCommand command,
			(SqlDbType ParameterType, string ParameterName) sqlParameter)
		{
			var value = filters.SingleOrDefault(x => x.ParamName == sqlParameter.ParameterName).ParamValue ??
				sqlParameter.ParameterName switch
				{
					Report_DEOpenSimplifiedDeclarationsParameters.CompanyPk => companyPK,
					Report_DEOpenSimplifiedDeclarationsParameters.EntryDateFrom => new DateTime(1753, 1, 1, 0, 0, 1),
					Report_DEOpenSimplifiedDeclarationsParameters.EntryDateTo => new DateTime(9999, 12, 31, 23, 59, 59),
					_ => DBNull.Value,
				};
			command.AddParameter(sqlParameter.ParameterName, sqlParameter.ParameterType, value);
		}

		class Report_DEOpenSimplifiedDeclarationsParameters
		{
			public const string CompanyPk = "@CompanyPk";
			public const string EntryDateFrom = "@EntryDateFrom";
			public const string EntryDateTo = "@EntryDateTo";
			public const string EntryType = "@EntryType";
		}

		void UpdateJobDeclarationColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobDeclaration", columnName, columnValue, columnType, "JE_PK", primaryKeyValue);
		}

		void UpdateCusReconEntryColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusReconEntry", columnName, columnValue, columnType, "CRE_PK", primaryKeyValue);
		}

		void UpdateCusReconEntryLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusReconEntryLine", columnName, columnValue, columnType, "CRL_PK", primaryKeyValue);
		}
	}
}
