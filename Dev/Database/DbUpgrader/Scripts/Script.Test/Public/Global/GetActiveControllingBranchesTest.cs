using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Global;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Global.Testing
{
	[TestedType(typeof(GetActiveControllingBranches))]
	internal sealed class GetActiveControllingBranchesTest : DbCreateScriptTest
	{
		public void TestFindControllingBranchWithFallBackToAnyCompany()
		{
			AssertEquals("Method handles NULLs", Guid.Empty, Run(null).FirstOrDefault());

			var orgPK = helper.InsertOrgHeader("AAO", "AAO Org");

			var companyPK = CreateCompany("AAA", "AU");
			var branchPK = CreateBranch(companyPK, "BAA");
			var orgDataPK = CreateOrgCompanyData(orgPK, companyPK, branchPK, false);
			AssertEquals("No branches has been set", Guid.Empty, Run(orgPK).FirstOrDefault());

			var company1PK = CreateCompany("AA1", "AU");
			var branch1PK = CreateBranch(company1PK, "BA1");
			var orgData1PK = CreateOrgCompanyData(orgPK, company1PK, branch1PK, true);
			var company2PK = CreateCompany("AA2", "AU");
			var branch2PK = CreateBranch(company2PK, "BA2");
			var orgData2PK = CreateOrgCompanyData(orgPK, company2PK, branch2PK, true);
			var company3PK = CreateCompany("AA3", "AU");
			var branch3PK = CreateBranch(company3PK, "BA3");
			var orgData3PK = CreateOrgCompanyData(orgPK, company3PK, branch3PK, true);
			AssertNotEquals("Falling back to a random company", Guid.Empty, Run(orgPK).FirstOrDefault());

			Update("OrgHeader", new { OH_RL_NKClosestPort = "AUSYD" }, new { OH_PK = orgPK });
			Update("GlbBranch", new { GB_RL_NKHomePort = "AUSYD", GB_SystemLastEditUser = "E", GB_SystemLastEditTimeUtc = DateTime.Now }, new { GB_PK = branch2PK });
			AssertEquals("Falling back to the company that matches because of a home port", branch2PK, Run(orgPK).FirstOrDefault());

			Update("OrgHeader", new { OH_RL_NKClosestPort = "NZAKL" }, new { OH_PK = orgPK });
			helper.Insert("GlbBranchExtraPorts", new { GY_PK = Guid.NewGuid(), GY_GB = branch2PK, GY_RL_NKAdditionalBranchRelatedPort = "NZAKL" });
			AssertEquals("Falling back to the company that matches because of a related port", branch2PK, Run(orgPK).FirstOrDefault());

			Update("OrgHeader", new { OH_RL_NKClosestPort = "AUSYD" }, new { OH_PK = orgPK });
			Update("GlbBranch", new { GB_RL_NKHomePort = "AUMEL", GB_SystemLastEditUser = "E", GB_SystemLastEditTimeUtc = DateTime.Now }, new { GB_PK = branch2PK });
			AssertEquals("Falling back to the company from the same country", branch2PK, Run(orgPK).FirstOrDefault());
		}

		#region Implementation

		Guid CreateCompany(string code, string countryCode, string currencyCode = "AUD")
		{
			var companyPK = Guid.NewGuid();

			helper.Insert("GlbCompany", new { GC_PK = companyPK, GC_IsActive = true, GC_Code = code, GC_Name = "Company", GC_RN_NKCountryCode = countryCode, GC_RX_NKLocalCurrency = currencyCode });

			return companyPK;
		}

		Guid CreateBranch(Guid companyPK, string code)
		{
			var branchPK = Guid.NewGuid();

			helper.Insert("GlbBranch", new { GB_PK = branchPK, GB_Code = code, GB_IsActive = true, GB_GC = companyPK });

			return branchPK;
		}

		Guid CreateOrgCompanyData(Guid orgPK, Guid companyPK, Guid branchPK, bool setControllingBranch)
		{
			var orgDataPK = Guid.NewGuid();

			if (setControllingBranch)
			{
				helper.Insert("OrgCompanyData", new { OB_PK = orgDataPK, OB_OH = orgPK, OB_GC = companyPK, OB_GB_ControllingBranch = branchPK });
			}
			else
			{
				helper.Insert("OrgCompanyData", new { OB_PK = orgDataPK, OB_OH = orgPK, OB_GC = companyPK });
			}

			return orgDataPK;
		}

		public void Update(string tableName, object valuesObject, object whereValues)
		{
			string sql =
				string.Format("UPDATE [{0}] SET {1} WHERE {2}",
				tableName,
				string.Join(",", valuesObject.GetType().GetProperties().Select(p => string.Format("[{0}] = {1}", p.Name, "@" + p.Name))),
				string.Join(",", whereValues.GetType().GetProperties().Select(p => string.Format("[{0}] = {1}", p.Name, "@" + p.Name))));

			using (var command = TestConnection.Command(sql))
			{
				command.CommandType = CommandType.Text;

				if (valuesObject != null)
				{
					foreach (var property in valuesObject.GetType().GetProperties())
					{
						object val = property.GetValue(valuesObject, null);
						val = val ?? DBNull.Value;
						var parameter = new SqlParameter(property.Name, val);
						command.AddParameter(parameter, parameter.Precision, parameter.Scale, val);
					}
				}
				if (whereValues != null)
				{
					foreach (var property in whereValues.GetType().GetProperties())
					{
						object val = property.GetValue(whereValues, null);
						val = val ?? DBNull.Value;
						var parameter = new SqlParameter(property.Name, val);
						command.AddParameter(parameter, parameter.Precision, parameter.Scale, val);
					}
				}

				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TestDbHelper(TestConnection);
		}

		TestDbHelper helper;

		IEnumerable<Guid> Run(Guid? organizationPK)
		{
			var result = new List<Guid>();

			using (var reader = helper.RunSP("GetActiveControllingBranches", new { organizationPK = organizationPK }))
			{
				while (reader.Read())
				{
					result.Add(reader.GetGuid(0));
				}
			}

			return result.ToArray();
		}
		#endregion

	}
}

