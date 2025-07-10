using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(GetParentAccountNumber))]
	internal class GetParentAccountNumberTest : BiCreateScriptTest
	{
		public void TestCatersForAnyAccountNumberSegmentCount()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Child Account Number = null", null, GetParentAccountNumber(DBNull.Value));

				AssertParentAccount("123.00.456", "123.00.000");
				AssertParentAccount("43.123.44.0.00", "43.123.00.0.00");
				AssertParentAccount("43.123.000", "43.000.000");
				AssertParentAccount("43.0.000", null);
				AssertParentAccount("000.0.000", null);
				AssertParentAccount("00.34.12", "00.34.00");
				AssertParentAccount("222.0.132.0", "222.0.000.0");
				AssertParentAccount("00.000.132", null);
				AssertParentAccount("00.000.132.0", null);
				AssertParentAccount("00.12.0", null);
				AssertParentAccount("12.0", null);
				AssertParentAccount("12.222", "12.000");
				AssertParentAccount("0.10", null);
				AssertParentAccount("0.X", null);
				AssertParentAccount("X", null);
				AssertParentAccount("", null);
				AssertParentAccount(".", null);
				AssertParentAccount("..", null);
				AssertParentAccount(".34.", null);
				AssertParentAccount("23..", null);
				AssertParentAccount("23.320.", null);
				AssertParentAccount("23.320.XX", null);
				AssertParentAccount("23.00.XX", null);
				AssertParentAccount("23Y.00.XX", null);
				AssertParentAccount("1.234.1E10", null);
				AssertParentAccount("1.234.$34", null);
				AssertParentAccount("1.2%.34+23", null);
				AssertParentAccount("5.3..000", null);
				AssertParentAccount("5.3.001.3", "5.3.001.0");
				AssertParentAccount("5.3.001", "5.3.000");
				AssertParentAccount("5.3.00.34", "5.3.00.00");
				AssertParentAccount("5.3.00.34.500", "5.3.00.34.000");
				AssertParentAccount("5.3.2d4", null);
				AssertParentAccount("5.-3.300", null);
			});
		}

		#region Implementation

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetParentAccountNumber(object childAccNumber)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT [{0}].[{1}].[{2}](@AccountNumber)",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);

			using (DbCommand command = TestConnection.Command(sqlText))
			{
				command.AddParameter("@AccountNumber", SqlDbType.NVarChar, childAccNumber);
				object ojbResult = command.ExecuteScalar();
				return (ojbResult == DBNull.Value) ? null : ojbResult.ToString();
			}
		}

		void AssertParentAccount(string childAccount, string expectedParentAccount)
		{
			AssertEquals("[" + childAccount + "] => Parent Account?", expectedParentAccount, GetParentAccountNumber(childAccount));
		}
		#endregion
	}
}
