using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC057C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC057CMessageInterpreter))]
	public class CC057CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC057CMessageInterpreter, CC057CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE057;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Rejection from Office of Destination (IE057) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Rejection Code</td><td>22 - Rejection Code 22</td></tr><tr><td>Rejection Reason</td><td>Invalid CC057C</td></tr><tr><td>Error Pointer</td><td>Test Pointer</td></tr><tr><td>Error Code</td><td>93 - Error Code 93</td></tr><tr><td>Error Reason</td><td>Test</td></tr><tr><td>Original Attribute Value</td><td>Test Attribute Value</td></tr><tr><td>Error Pointer</td><td>Test Pointer 2</td></tr><tr><td>Error Code</td><td>14 - Error Code 14</td></tr><tr><td>Error Reason</td><td>Test 2</td></tr><tr><td>Original Attribute Value</td><td>Test Attribute Value 2</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC057CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC057CProvider GetProvider(TextReader reader) => new CC057CProvider(new MailBoxItemProvider<Cc057CType>(reader).Message);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);

			helper.CreateNewOrGetExistingCusCodeType("CL227", "Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL227",
				code: "22",
				description: "Rejection Code 22",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL180",
				code: "14",
				description: "Error Code 14",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL180",
				code: "93",
				description: "Error Code 93",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			base.SetUp();
		}
	}
}
