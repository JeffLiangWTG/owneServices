using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC231C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC231CMessageInterpreter))]
	sealed class CC231CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC231CMessageInterpreter, CC231CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE231;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var grn = "12GRNCC055C012345A678901";
			var endDate = new DateTime(2025, 06, 15);
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var messageText = InterchangeProcessorTestHelper.GetStandardCC231CText(grn, endDate);
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Comprehensive Guarantee Cancellation Notification (IE231) message has been received for GRN 12GRNCC055C012345A678901.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Holder of Transit Procedure&#39;s Identification Number</td><td>IN231</td></tr><tr><td>GRN</td><td>12GRNCC055C012345A678901</td></tr><tr><td>Invalidity Date</td><td>15-Jun-25</td></tr><tr><td>Invalidity Reason Code</td><td>003</td></tr><tr><td>Invalidity Reason Text</td><td>Invalidity Reason Text</td></tr><tr><td>Customs Office of Guarantee</td><td>RNCC231C</td></tr></table>";

		protected override CC231CProvider GetProvider(TextReader reader) => new CC231CProvider(new MailBoxItemProvider<Cc231CType>(reader).Message);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "RNCC231C",
				description: "Customs Office 8",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			base.SetUp();
		}
	}
}
