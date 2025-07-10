using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC022C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC022CMessageInterpreter))]
	class CC022CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC022CMessageInterpreter, CC022CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE022;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC022CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC022CProvider GetProvider(TextReader reader) => new CC022CProvider(new MailBoxItemProvider<Cc022CType>(reader).Message);

		public static ZString GetExpectedInterpretationText()
		{
			return @"A Notification to Amend Declaration (IE022) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amend Notification Date &amp; Time</td><td>31-Jan-23 10:22</td></tr></table><br /><br />Functional Error:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>EP01</td></tr><tr><td>Error Code</td><td>26 - Description 26</td></tr><tr><td>Error Reason</td><td>ER0001</td></tr><tr><td>Original Value</td><td>Original value 1</td></tr></table><br /><br />Functional Error:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>2</td></tr><tr><td>Error Pointer</td><td>EP02</td></tr><tr><td>Error Code</td><td>12 - Another Description 12</td></tr><tr><td>Error Reason</td><td>ER0022</td></tr><tr><td>Original Value</td><td>Previous value 2</td></tr></table>";
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180, "CL180");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180,
				code: "26",
				description: "Description 26",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180,
				code: "12",
				description: "Another Description 12",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			Factory.Save();

			base.SetUp();
		}
	}
}
