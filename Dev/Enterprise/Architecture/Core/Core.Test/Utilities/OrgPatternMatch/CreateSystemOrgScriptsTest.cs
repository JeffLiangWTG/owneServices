using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public abstract class CreateSystemOrgScriptsTest : TransactionedTestCase
	{
		protected abstract CreateSystemOrgScripts Scripts { get; }

		public void TestOrganisationCreateUnmatched()
		{
			PrepareTestData();
			AssertInsertionResults();
		}

		public void PrepareTestData()
		{
			DoAdditionalPreparations();
		}

		protected virtual void DoAdditionalPreparations()
		{ }

		public void AssertInsertionResults()
		{
			AssertRecordInDB(
				"SELECT COUNT(*) FROM dbo.OrgHeader WHERE OH_Code = @code",
				Scripts.OrgCode + " Org exists in system",
				AddScriptsOrgCodeParameter);
			AssertRecordInDB(
				"SELECT COUNT(*) FROM dbo.OrgAddress JOIN dbo.OrgHeader ON OA_OH = OH_PK WHERE OH_Code = @code",
				Scripts.OrgCode + " Org Address exists in system",
				AddScriptsOrgCodeParameter);
			AssertRecordInDB(
				"SELECT COUNT(*) FROM dbo.OrgPatternMatch JOIN dbo.OrgHeader ON OS_OH = OH_PK WHERE OH_Code = @code",
				Scripts.OrgCode + " Org PatternMatch exists in system",
				AddScriptsOrgCodeParameter);
			AssertRecordInDB(
				"SELECT COUNT(*) FROM dbo.OrgPatternMatch JOIN dbo.OrgAddress ON OS_OA = OA_PK JOIN dbo.OrgHeader ON OA_OH = OH_PK WHERE OH_Code = @code",
				Scripts.OrgCode + " Org PatternMatch exists in system (Correct Addresses assigned)",
				AddScriptsOrgCodeParameter);
			DoAdditionalAssertions();
		}

		void AddScriptsOrgCodeParameter(DbCommand cmd)
		{
			cmd.AddParameterBasedOnDbColumn("@code", Scripts.OrgCode, OrgHeaderSchema.OH_Code);
		}

		protected virtual void DoAdditionalAssertions()
		{ }

		protected void AssertRecordInDB(string sQL, string message, Action<DbCommand> paramsAction = null)
		{
			int count = (int)Db.Connection.ExecuteScalar(sQL, paramsAction);
			Assert(message, count == 1);
		}
	}
}
