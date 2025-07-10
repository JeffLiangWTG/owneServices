using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(B3ImportStatusCalculator))]
	sealed class B3ImportStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		#region TestCalculatedJobStatus

		public override void TestCalculatedJobStatus()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var header = jobDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(header));

			const string syntaxError = @"UNH+1+CUSRES:S:99B:UN+10207'BGM+:::+020+11'DTM+137:201007071025:203'GIS+14'ERP+2:35:29'FTX+AAO+++SEGMENTMOALINE8ELE POS1,2:ELEM TOO LONG'UNT+11+1'";
			AddEDIMessage(header, "10", syntaxError);
			AssertEquals("SyntaxError", EDIReleaseImportEntryStatusList.Codes.SyntaxError, calculator.CalculatedJobStatus(header));

			const string error = "UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:216'ERC+943152'ERP+:I99'RFF+ABO:216'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";
			AddEDIMessage(header, "20", error);
			AssertEquals("Error", B3EntryStatusList.Codes.Error, calculator.CalculatedJobStatus(header));

			const string confirmed = "UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:216'ERC+942861'ERP+:I99'RFF+ABO:216'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";

			AddEDIMessage(header, "25", confirmed);
			AssertEquals("Confirmed", B3EntryStatusList.Codes.Confirmed, calculator.CalculatedJobStatus(header));

			const string clear = "UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:216'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
			AddEDIMessage(header, "30", clear);
			AssertEquals("Clear", B3EntryStatusList.Codes.Accepted, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "40", syntaxError);
			AddEDIMessage(header, "50", error);
			AssertEquals("Clear", B3EntryStatusList.Codes.Accepted, calculator.CalculatedJobStatus(header));

			var header2 = jobDeclaration.CustomsEntryHeaders.AddNew();
			header2.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;

			AddEDIMessage(header2, "10", error);
			AssertEquals("Should not override a previous 'Clear' Status", B3EntryStatusList.Codes.Accepted, calculator.CalculatedJobStatus(header2));

			var header3 = jobDeclaration.CustomsEntryHeaders.AddNew();
			header3.CH_Status = B3EntryStatusList.Codes.Accepted;
			header3.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			const string error1 = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:216'ERC+942991'ERP+:I99'RFF+ABO:216'ERC+942995'DOC+961'CST++1+0+1'UNT+12+1";
			AddEDIMessage(header3, "25", error1);
			AssertEquals("Error1", B3EntryStatusList.Codes.Error, calculator.CalculatedJobStatus(header3));
		}

		static void AddEDIMessage(CusEntryHeader header, string messageNum, string messageText)
		{
			var result = header.Messages.AddNew();
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			result.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			result.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_MessageNum = messageNum;
			result.EM_MessageText = messageText;
		}

		#endregion

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.B3CUSDEC, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new B3ImportStatusCalculator();
		}
	}
}
