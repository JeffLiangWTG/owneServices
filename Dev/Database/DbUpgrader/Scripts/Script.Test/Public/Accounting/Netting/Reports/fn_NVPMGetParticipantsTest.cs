using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NVPMGetParticipants))]
	class fn_NVPMGetParticipantsTest : DbCreateScriptTest
	{
		public void Testfn_NVPMGetParticipants()
		{
			var sydneyCompanyPK = TestDataCreator.CreateCompany("SYD", "AU", "AUD");
			var melbourneCompanyPK = TestDataCreator.CreateCompany("MEL", "AU", "AUD");
			var brisbaneCompanyPK = TestDataCreator.CreateCompany("BNE", "AU", "AUD");

			var sqlQuery = "SELECT * FROM fn_NVPMGetParticipants(@CompanyPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar
				, string.Format(CultureInfo.InvariantCulture
					, "{0},{1},{2}"
					, sydneyCompanyPK.ToString(), melbourneCompanyPK.ToString(), brisbaneCompanyPK.ToString()));

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("3 records should be returned", 3, result.Rows.Count);
		}
	}
}

