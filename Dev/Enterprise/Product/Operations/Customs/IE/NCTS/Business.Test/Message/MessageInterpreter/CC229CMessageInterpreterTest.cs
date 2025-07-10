using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC229C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC229CMessageInterpreter))]
	class CC229CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC229CMessageInterpreter, CC229CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE229;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var grn = "123456";
			var endDate = new DateTime(2025, 06, 15);
			var identificationNumber = "TESTREF24680";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var messageText = InterchangeProcessorTestHelper.GetStandardCC229CText(grn, endDate, identificationNumber);
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"An Individual Guarantee voucher revocation Notification (IE229) message has been received for GRN 123456.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Guarantor&#39;s Name</td><td>Bond</td></tr><tr><td>Guarantor&#39;s Address</td><td>7 Main Street, London, MI5, GB</td></tr><tr><td>GRN</td><td>123456</td></tr><tr><td>Invalidity Date</td><td>15-Jun-25</td></tr><tr><td>Customs Office of Guarantee</td><td>RNCC229C</td></tr></table>";

		protected override CC229CProvider GetProvider(TextReader reader) => new CC229CProvider(new MailBoxItemProvider<Cc229CType>(reader).Message);

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "RNCC229C",
				description: "Customs Office 6",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			base.SetUp();
		}
	}
}
