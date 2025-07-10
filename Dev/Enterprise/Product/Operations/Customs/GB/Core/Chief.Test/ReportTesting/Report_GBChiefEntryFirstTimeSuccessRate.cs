using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.GB.Chief.ReportTesting
{
	class Report_GBChiefEntryFirstTimeSuccessRate : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var responseType = new ReportSchemaColumn(typeof(string), "FirstResponseType");
				var counter = new ReportSchemaColumn(typeof(int), "counter");
				return new List<ReportSchemaColumn>() { responseType, counter };
			}
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.StoredProc; }
		}

		protected override ZString ObjectName
		{
			get { return "Report_GBChiefEntryFirstTimeSuccessRate"; }
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				var parameters = new List<string>();
				parameters.Add("'" + ZDateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "'");
				parameters.Add("'" + ZDateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "'");
				parameters.Add("'" + GlbBranch.CurrentBranch.PK.ToString() + "'");
				return parameters;
			}
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(2, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			var row2 = FormatRowsValues(results.Rows[1], results);
			AssertContains("[FirstResponseType]='OK'; [counter]='2'", row1);
			AssertContains("[FirstResponseType]='Rejected'; [counter]='1'", row2);
		}

		protected override void PrepareTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryOneSuccessFirstTime = declaration.CustomsEntryHeaders.AddNew();
			var entryTwoSuccessFirstTimeButThenFailToAmend = declaration.CustomsEntryHeaders.AddNew();
			var entryThreeFailThenSucceed = declaration.CustomsEntryHeaders.AddNew();
			var entrySentOutsideDateRange = declaration.CustomsEntryHeaders.AddNew();

			entryOneSuccessFirstTime.CH_BGMReference = "ONE";
			entryTwoSuccessFirstTimeButThenFailToAmend.CH_BGMReference = "TWO";
			entryThreeFailThenSucceed.CH_BGMReference = "THREE";
			entrySentOutsideDateRange.CH_BGMReference = "FOUR";

			var messageOneSuccess = MakeMessageAndEntryCode(entryOneSuccessFirstTime, "29");
			var messageTwoSuccess = MakeMessageAndEntryCode(entryTwoSuccessFirstTimeButThenFailToAmend, "29");
			var messageTwoFail = MakeMessageAndEntryCode(entryTwoSuccessFirstTimeButThenFailToAmend, "27");
			var messageThreeFail = MakeMessageAndEntryCode(entryThreeFailThenSucceed, "27");
			var messageThreeSuccess = MakeMessageAndEntryCode(entryThreeFailThenSucceed, "29");
			var messageTooOld = MakeMessageAndEntryCode(entrySentOutsideDateRange, "29");
			messageTooOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
		}

		protected virtual EDIMessage MakeMessageAndEntryCode(CusEntryHeader entry, string businessResponseCode)
		{
			entry.CH_MessageType = "GBG";
			var message = entry.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_ApplicationCode = "gbe";
			message.EM_ApplicationReference = "cusres";
			message.EM_MessageSubType = businessResponseCode;
			message.EM_Status = "RCV"; // processed OK
			return message;
		}
	}

	class Report_GBChiefEntryFirstTimeSuccessRateTestForChief : Report_GBChiefEntryFirstTimeSuccessRate
	{
		protected override EDIMessage MakeMessageAndEntryCode(CusEntryHeader entry, string businessResponseCode)
		{
			entry.CH_MessageType = "CHF";
			var message = entry.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_ApplicationCode = "gbe";
			message.EM_ApplicationReference = "cusres";
			message.EM_MessageSubType = businessResponseCode;
			message.EM_Status = "RCV"; // processed OK
			return message;
		}
	}
}
