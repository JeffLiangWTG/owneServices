using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC035C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC035CMessageInterpreter))]
	class CC035CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC035CMessageInterpreter, CC035CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE035;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"Recovery Notification Message (IE035) has been received. A Competent Authority of Recovery has initiated Recovery of Transit Declaration for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Accepted Date</td><td>18-Sep-71</td></tr><tr><td>Recovery Notification Date</td><td>18-Sep-71</td></tr><tr><td>Recovery Notification Text</td><td>Test Recovery Notification</td></tr><tr><td>Amount Claimed</td><td>22.22IEC</td></tr><tr><td>Customs Office of Recovery at Departure</td><td>IE000001 - Customs Office 1</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc035Text = InterchangeProcessorTestHelper.GetStandardCC035CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc035Text, includeResponseWrap: false);

			return message;
		}

		protected override CC035CProvider GetProvider(TextReader reader) => new CC035CProvider(new MailBoxItemProvider<Cc035CType>(reader).Message);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IE000001",
				description: "Customs Office 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			base.SetUp();
		}
	}
}
