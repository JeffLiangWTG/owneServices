using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NVPMGetParticipantsAsHeader))]
	class fn_NVPMGetParticipantsAsHeaderTest : DbCreateScriptTest
	{
		public void Testfn_NVPMGetParticipantsAsHeader()
		{
			var sydneyCompanyPK = TestDataCreator.CreateCompany("SYD", "AU", "AUD");
			var melbourneCompanyPK = TestDataCreator.CreateCompany("MEL", "AU", "AUD");
			var brisbaneCompanyPK = TestDataCreator.CreateCompany("BNE", "AU", "AUD");

			var sqlQuery = "SELECT * FROM fn_NVPMGetParticipantsAsHeader(@CompanyPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar
				, string.Format(CultureInfo.InvariantCulture
					, "{0},{1},{2}"
					, sydneyCompanyPK.ToString(), melbourneCompanyPK.ToString(), brisbaneCompanyPK.ToString()));

			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("The results are transposed, since only one row will be returned with all 3 companies", 1, result.Rows.Count);

			var companyCodes = new string[] { "BNE", "MEL", "SYD" };

			for (int column = 1; column <= 25; column++)
			{
				if (column <= 3)
				{
					AssertEquals(string.Format("Result sorted alphabetically - {0}", companyCodes[column - 1])
						, companyCodes[column - 1], result.Rows[0]["Participant" + column]);
				}
				else
				{
					AssertEquals("No record returned for participant" + column, DBNull.Value, result.Rows[0]["Participant" + column]);
				}
			}
		}
	}
}

