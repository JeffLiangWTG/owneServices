using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_GetMatchStatusReasonWithDescriptionTest : ScriptTest
	{
		public void TestReport_GetMatchStatusReasonWithDescription()
		{
			var dt = RunScript(GlbCompany.CurrentCompany.PK);
			AssertEquals("default value one line", 1, dt.Rows.Count);
			AssertEquals(1, dt.Select("Code = 'ADV' and Description = 'Receipt/Payment in advance'").Length);

			var newCollection = new SystemDefinableCodeDescriptionBoolCollection();
			newCollection.Add("TS1", (NoResString)"test description1", true);
			AccountingConfigurationRegistry.Instance.MatchStatusReason.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCollection);
			newCollection.Add("TS2", (NoResString)"test description2", false);
			AccountingConfigurationRegistry.Instance.MatchStatusReason.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newCollection);
			Factory.Save();

			dt = RunScript(ZGuid.Empty);
			AssertEquals("default value and added value, system level override", 2, dt.Rows.Count);
			AssertEquals(1, dt.Select("Code = 'ADV' and Description = 'Receipt/Payment in advance'").Length);
			AssertEquals(1, dt.Select("Code = 'TS1' and Description = 'test description1'").Length);

			dt = RunScript(GlbCompany.CurrentCompany.PK);
			AssertEquals("default value and added values, company level override", 3, dt.Rows.Count);
			AssertEquals(1, dt.Select("Code = 'ADV' and Description = 'Receipt/Payment in advance'").Length);
			AssertEquals(1, dt.Select("Code = 'TS1' and Description = 'test description1'").Length);
			AssertEquals(1, dt.Select("Code = 'TS2' and Description = 'test description2'").Length);
		}

		#region Implementation

		DataTable RunScript(ZGuid companyPK)
		{
			var sql = $"SELECT Code, Description FROM [dbo].[Report_GetMatchStatusReasonWithDescription]('{companyPK}')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}
