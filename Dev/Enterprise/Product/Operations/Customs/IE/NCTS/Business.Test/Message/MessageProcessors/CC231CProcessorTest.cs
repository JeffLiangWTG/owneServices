using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC231C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC231CProcessor))]
	class CC231CProcessorTest : NCTSLinkedGuaranteeMessageProcessorAbstractTest<CC231CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC231CProvider>
	{
		public void TestMustHaveLinkedObject()
		{
			var incomingMessage = CreateNewIncomingMessage();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				AssertNull("EM_LinkedObject", incomingMessage.EM_LinkedObject);
			}
		}

		protected override void AssertProcessResultCore(CusGuaranteeHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"
			A Comprehensive Guarantee Cancellation Notification (IE231) message has been received for GRN 12GRNCC055C012345A678901.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Holder of Transit Procedure&#39;s Identification Number</td><td>IN231</td></tr>
			<tr><td>GRN</td><td>12GRNCC055C012345A678901</td></tr><tr><td>Invalidity Date</td><td>10-Jul-24</td></tr>
			<tr><td>Invalidity Reason Code</td><td>003</td></tr>
			<tr><td>Invalidity Reason Text</td><td>Invalidity Reason Text</td></tr>
			<tr><td>Customs Office of Guarantee</td><td>RNCC231C</td></tr>
			</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response",
				new[] { "A Comprehensive Guarantee Cancellation Notification (IE231) message has been received for GRN 12GRNCC055C012345A678901." },
				new string[] { "staff1@where.com" });
		}

		public void TestEndToEndProcessing_NoCusGuarantee()
		{
			orgHeaderCodeType = "EOR";
			orgHeaderCustomsRegNo = "007";
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData_NoCusPermit();

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_LinkUniqueID", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", ZString.Empty, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("Message should have been set PRS.", "PRS", incomingMessage.EM_Status);
					AssertMessageInterpretation(incomingMessage, ZString.Empty);
					MessageProcessorNotificationTestHelper.AssertNoEmailsSent();
				});
			}
		}

		public void TestEndToEndProcessing_NoCusGuaranteeOrOrgHeader()
		{
			var incomingMessage = CreateNewIncomingMessage();

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_LinkUniqueID", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", ZString.Empty, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("Message should have been set PRS.", "PRS", incomingMessage.EM_Status);
					AssertMessageInterpretation(incomingMessage, ZString.Empty);
					MessageProcessorNotificationTestHelper.AssertNoEmailsSent();
				});
			}
		}

		public void TestLinkToIELatestEndDate() => AssertLinkToIEOrGBLatestEndDate(Core.Constants.CountryCodes.Ireland);
		public void TestLinkToGBLatestEndDate() => AssertLinkToIEOrGBLatestEndDate(Core.Constants.CountryCodes.UnitedKingdom);

		void AssertLinkToIEOrGBLatestEndDate(ZString countryCode)
		{
			_ = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.Latvia, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));
			_ = CreateCusGuaranteeHeader(countryCode, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));
			var guarantee = CreateCusGuaranteeHeader(countryCode, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(3));
			_ = CreateCusGuaranteeHeader(countryCode, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(2));
			using (Factory.AddDisposableService())
			{
				var processor = Processor;
				var incomingMessage = CreateNewIncomingMessage();
				processor.PreProcessMessage(incomingMessage);
				processor.PreProcessMessage(incomingMessage);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				AssertEquals("Latest end date", guarantee, incomingMessage.EM_LinkedObject);
			}
		}

		(OrgHeader declaration, OrgHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData_NoCusPermit()
		{
			var orgHeader = CreateOrgHeader();

			var incomingMessage = CreateNewIncomingMessage();
			return (orgHeader, orgHeader, null, incomingMessage);
		}

		OrgHeader CreateOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = orgHeaderCodeType;
			cusCode.OK_CustomsRegNo = orgHeaderCustomsRegNo;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_OH = orgHeader.PK;
			return orgHeader;
		}

		string orgHeaderCodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		string orgHeaderCustomsRegNo = "007";

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE231;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC231CText("12GRNCC055C012345A678901", new DateTime(2024, 07, 10, 00, 00, 00));

		protected override ZString MessageFriendlyName => "CC231C: Comprehensive guarantee cancellation notification";

		protected override CC231CProcessor Processor => new CC231CProcessor(logger, typeof(Cc231CType));
	}
}
