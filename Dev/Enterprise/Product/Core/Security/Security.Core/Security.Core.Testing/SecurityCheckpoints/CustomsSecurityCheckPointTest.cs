using System;
using CargoWise.Data;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class CustomsSecurityCheckPointTest : TransactionedTestCase
	{
		public void TestUSItemsAreVisibleToPRCompany()
		{
			var sqlText = @"SELECT TOP 1 GB_PK, GC_PK FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK WHERE GC_RN_NKCountryCode = 'PR'";
			var command = Db.Connection.Command(sqlText);
			var data = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
			var branchPK = Guid.Empty;
			var companyPK = Guid.Empty;
			if (data.Read())
			{
				branchPK = data.GetGuid(0);
				companyPK = data.GetGuid(1);
			}

			data.Close();

			if (branchPK != Guid.Empty && companyPK != Guid.Empty)
			{
				var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, branchPK, EnvProxy.Instance.CurrentDepartment.PK, companyPK);
				var checkpoint = new CustomsSecurityCheckPoint("Code", (NoResString)"DisplayText", null, security.ZSecurityInstance, Enterprise.Core.Constants.CountryCodes.UnitedStates);
				security.CachingEnabled = false;
				AssertEquals("Visible - US items are visible to PR company", true, checkpoint.Visible);
			}
			else
			{
				Assert(true);
			}
		}
	}
}
