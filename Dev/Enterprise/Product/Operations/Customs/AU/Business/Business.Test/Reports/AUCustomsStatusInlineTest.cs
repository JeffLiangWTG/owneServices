using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCustomsStatusInlineTest : ReportFunctionalTestCase
	{
		public void TestAllResults()
		{
			var inputOutputList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("ACZ","ACS Seized - Cargo is seized by Customs"),
				new KeyValuePair<string, string>("AQZ","Quarantine Seized - Cargo is seized by Quarantine"),
				new KeyValuePair<string, string>("CLR","CLEAR - Cargo is free of any impediments"),
				new KeyValuePair<string, string>("CLH","CLEARHRM - Cargo is clear, but is identified as High Risk Movement"),
				new KeyValuePair<string, string>("CCL","CONDCLEAR - Cargo can be released into home consumption subject to conditions"),
				new KeyValuePair<string, string>("HLD","HELD - Cargo is held under Customs controlled"),
				new KeyValuePair<string, string>("TRS","TRANSHIP - Cargo is for transhipment. A transhipment Number will be generated and transmitted with Status"),
				new KeyValuePair<string, string>("TRH","TRANSHPHRM - Transhipment Cargo is Clear but is identified as High Risk Movement"),
				new KeyValuePair<string, string>("TRT","TRANSIT - Cargo is transit cargo. This value will only be viewable from dbo.an interactive function"),
				new KeyValuePair<string, string>("WTD","WITHDRAWN - Cargo Report had been withdrawn"),
				new KeyValuePair<string, string>("XXX","XXX")
			};

			foreach (var inputOutput in inputOutputList)
			{
				var sqlText = string.Format("SELECT * FROM {0}('{1}')", ObjectName, inputOutput.Key);
				using (var command = TestConnection.Command(sqlText))
				using (var reader = command.ExecuteReader())
				{
					Assert("Should return " + inputOutput.Value + " for input " + inputOutput.Key, reader.Read() && reader[0].Equals(inputOutput.Value));
				}
			}
		}

		protected override ZString ObjectName => "AUCustomsStatusInline";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn> { new ReportSchemaColumn(typeof(string), "CustomsStatusDescription") };

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals("Should reutrn 'ACS Seized - Cargo is seized by Customs' for 'ACZ'", "ACS Seized - Cargo is seized by Customs", resultsOrderedByExpectedColumnNames.Rows[0][0]);
		}

		protected override void PrepareTestData() { }

		protected override bool ShouldTestColumns => false;

		protected override List<string> ParametersValuesList => new List<string> { "'ACZ'" };
	}
}
