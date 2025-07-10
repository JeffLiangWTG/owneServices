using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE818MessageProcessor))]
	abstract class IE818MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE818MessageProcessor, IIE818>
	{
		public void TestEndToEndProcessing_WhenIsConsignee()
		{
			CreateSetupData();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("JE_MessageStatus should have been set RCV", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, declaration.JE_MessageStatus);
					AssertEquals("JE_EntryStatus should have been set COM.", EntryStatusList.Codes.COM, declaration.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					MessageProcessorNotificationTestHelper.AssertEmail("EMCS Report of Receipt", new[] { "Your EMCS Declaration for Job E00000810 received a report of receipt. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE818;

		protected override IE818MessageProcessor Processor => new IE818MessageProcessor(logger, typeof(TMessageType));

		protected override void CreateSetupData()
		{
			base.CreateSetupData();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_CustomsQuantity = 150;
			line1.ZG_DeclaredValue = 150;

			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.JI_CustomsQuantity = 200;
			line2.ZG_DeclaredValue = 200;
		}

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			var reportOfReceipt1 = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 1);
			AssertEquals("1st line, Customs Quantity", 140m, reportOfReceipt1.JI_CustomsQuantity);
			AssertEquals("1st line, Refused Quantity", 20m, reportOfReceipt1.Outturn.C5_RejectedQuantity);
			AssertEquals("2nd line, Observed Quantity", -10m, reportOfReceipt1.Outturn.ObservedDifference);

			var reasons1 = reportOfReceipt1.Outturn.ReportOfReceiptReasons;
			AssertEquals("1st line, 1st reason, Reason Code", "3", reasons1[0].CY_Code);
			AssertEquals("1st line, 1st reason, Reason Description", "Goods were damaged during transport", reasons1[0].CY_Data);
			AssertEquals("1st line, 2nd reason, Reason Code", "2", reasons1[1].CY_Code);
			AssertEquals("1st line, 2nd reason, Reason Description", "Quantity is less than what was reported", reasons1[1].CY_Data);

			var reportOfReceipt2 = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 2);
			AssertEquals("2nd line, Customs Quantity", 230m, reportOfReceipt2.JI_CustomsQuantity);
			AssertEquals("2nd line, Refused Quantity", 20m, reportOfReceipt2.Outturn.C5_RejectedQuantity);
			AssertEquals("2nd line, Observed Quantity", 30m, reportOfReceipt2.Outturn.ObservedDifference);

			var reasons2 = reportOfReceipt2.Outturn.ReportOfReceiptReasons;
			AssertEquals("2nd line, 1st reason, Reason Code", "3", reasons2[0].CY_Code);
			AssertEquals("2nd line, 1st reason, Reason Description", "Goods were damaged during transport", reasons2[0].CY_Data);

			AssertEquals("JE_EntryStatus should have been set COM.", EntryStatusList.Codes.COM, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Report of Receipt", new[] { "Your EMCS Declaration for Job E00000810 received a report of receipt. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
