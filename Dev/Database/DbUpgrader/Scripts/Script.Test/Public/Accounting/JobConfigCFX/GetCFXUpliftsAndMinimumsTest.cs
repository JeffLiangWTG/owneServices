using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.JobConfigCFX;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.JobConfigCFX
{
	[TestedType(typeof(GetCFXUpliftsAndMinimums))]
	class GetCFXUpliftsAndMinimumsTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var companyPk = Guid.Parse(TestConnection.ExecuteScalar("SELECT TOP 1 GC_PK FROM dbo.GlbCompany").ToString());
			var branchPk = Guid.Parse(TestConnection.ExecuteScalar($"SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{companyPk}'").ToString());
			var orgHeaderPk = Guid.Parse(TestConnection.ExecuteScalar($"SELECT TOP 1 OH_PK FROM dbo.OrgHeader INNER JOIN dbo.OrgCompanyData ON OB_OH = OH_PK WHERE OB_GC = '{companyPk}'").ToString());

			Action<string, Guid, decimal, decimal> createCFXConfig =
				(parentTableCode, parentId, percent, min) =>
				{
					var sql =
						$@"INSERT INTO dbo.AccCFXUpliftConfigurationView (JCF_PK ,JCF_ConfigType ,JCF_GC ,JCF_Ledger ,JCF_ParentTableCode ,JCF_ParentId ,JCF_JobType ,JCF_ServiceDirection ,JCF_TransportMode ,JCF_RX_NKCurrency ,JCF_CFXPercentage ,JCF_CFXMinimum) 
						VALUES(NEWID(), 'CFX', '{companyPk}', 'AR', '{parentTableCode}', {(string.IsNullOrEmpty(parentTableCode) ? "NULL" : $"'{parentId.ToString()}'")}, 'ALL', 'EXP', 'SEA', '', {percent}, {min})";

					TestConnection.ExecuteNonQuery(sql);
				};

			Action<decimal, decimal, string> assertAsExpected =
				(percent, min, level) => TestConnection.ExecuteReader($"SELECT * FROM dbo.GetCFXUpliftsAndMinimums('{orgHeaderPk}', '{companyPk}', '{branchPk}', 'SEA', 'EXP')", r =>
											{
												AssertEquals(percent, (decimal)r[0]);
												AssertEquals(min, (decimal)r[1]);
												AssertEquals(level, (string)r[2]);
											});

			createCFXConfig(string.Empty, companyPk, 5, 10);
			assertAsExpected(5, 10, "Cmp");

			createCFXConfig("GB", branchPk, 11, 7);
			assertAsExpected(11, 7, "Brn");

			createCFXConfig("OH", orgHeaderPk, 3, 2);
			assertAsExpected(3, 2, "Org");
		}
	}
}

