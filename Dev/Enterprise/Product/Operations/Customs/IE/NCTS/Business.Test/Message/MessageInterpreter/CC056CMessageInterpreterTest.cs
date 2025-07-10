using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC056C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC056CMessageInterpreter))]
	class CC056CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC056CMessageInterpreter, CC056CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE056;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Rejection From Office of Departure (IE056) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC056C0123456</td></tr><tr><td>Rejection Date and Time</td><td>31-Jan-23 10:22</td></tr><tr><td>Rejection Code</td><td>7 - Rejection Code 7</td></tr><tr><td>Rejection Reason</td><td>Guarantee not valid for this customs territory</td></tr><tr><td>Error Pointer</td><td>Error Pointer 1</td></tr><tr><td>Error Code</td><td>12 - Error Code Item12</td></tr><tr><td>Error Reason</td><td>REASON1</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC056CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC056CProvider GetProvider(TextReader reader) => new CC056CProvider(new MailBoxItemProvider<Cc056CType>(reader).Message);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL226, "CL226");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL226,
				code: "7",
				description: "Rejection Code 7",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "Error Code Item12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			base.SetUp();
		}
	}
}
