using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class ExportDeltaGResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessorBehaviourAgainstLiquidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsSecondUnitQty = "DTNG";
			invoiceLine1.JI_CustomsSecondQuantity = 400;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT";
			invoiceLine1.JI_CustomsSecondQuantity = 100;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsSecondUnitQty = "DTNG";
			invoiceLine2.JI_CustomsSecondQuantity = 100;
			invoiceLine2.JI_CustomsThirdUnitQty = "HLT";
			invoiceLine2.JI_CustomsSecondQuantity = 100;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "9000-B00177613";

			var entryLine1 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			var entryLineFee1 = entryLine1.Fees.AddNew();
			entryLineFee1.FillWithValidTestData();
			var entryLineFee2 = entryLine1.ConfirmedFees.AddNew();
			var entryLine2 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var entryHeaderCharge = entryHeader.Charges.AddNew();
			entryHeaderCharge.C1_ChargeType = "V905";
			entryHeaderCharge.C1_ChargeAmount = 100;
			entryHeaderCharge.C1_MethodOfPayment = "";

			Factory.Save();

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithLiquidation.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("There should be 1 entry header charge", 1, entryHeader.Charges.Count);
				AssertEquals("There should be 2 entry header confirmed charges", 2, entryHeader.ConfirmedCharges.Count);
				AssertConfirmedEntryHeaderFee("Charges1", entryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>().First(), UniversalReferenceConstants.RefCusRateCodes.P635, 500m, "1");
				AssertConfirmedEntryHeaderFee("Charges2", entryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>().Last(), "V905", 1000m, "2");

				AssertEquals("There should be 1 fees against the first entry line.", 1, entryLine1.Fees.Count);
				AssertEquals("There should be 5 ConfirmedFees against the first entry line.", 5, entryLine1.ConfirmedFees.Count);

				AssertConfirmedEntryLineFee("entryLine1_Fee1", entryLine1.ConfirmedFees[0], "A445", Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 26m, 20m, 5m, "6", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee2", entryLine1.ConfirmedFees[1], "G065", "TNE1", 50m, 6.1m, 305m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee3: Because the content of <typtax> is 0, and because it is not based on goods value, the method of calculation should reflect the supplementary units of all invoice lines bound to the entry line. In case of calculation based on supplementary units, base value should be the sum of supplementary quantities of all invoice lines of the entry line.",
					entryLine1.ConfirmedFees[2], "A325", "DTNG", 500m, 1.2m, 600m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee4: Because the content of <typtax> is 1, the method of calculation should reflect the third units of all invoice lines bound to the entry line. In case of calculation based on third units, base value should be the sum of third quantities of all invoice lines of the entry line.",
					entryLine1.ConfirmedFees[3], "V906", "HLT", 200m, 3.1m, 620m, "1", "");
				AssertConfirmedEntryLineFee("entryLine1_Fee5", entryLine1.ConfirmedFees[4], UniversalReferenceConstants.RefCusRateCodes.U425, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 0m, 0m, 212m, "2", "");

				AssertEquals("There should be 1 fee against the second entry line.", 1, entryLine2.ConfirmedFees.Count);
				AssertConfirmedEntryLineFee("entryLine2", entryLine2.ConfirmedFees[0], UniversalReferenceConstants.RefCusRateCodes.U165, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 16323m, 2.7m, 441m, "1", "");
				AssertEquals(5962m, entryLine1.CL_ConfirmedStatisticalValue);
				AssertEquals(5962m, entryLine1.CL_ConfirmedCustomsValue);
				AssertEquals(0m, entryLine1.CL_ConfirmedValueForVAT);
				AssertEquals(0m, entryLine2.CL_ConfirmedStatisticalValue);
				AssertEquals(0m, entryLine2.CL_ConfirmedCustomsValue);
				AssertEquals(0m, entryLine2.CL_ConfirmedValueForVAT);

				AssertEquals(2183m, entryHeader.CH_TotalPaid);

				var exportMessageText2 = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithLiquidation2.xml");
				var message2 = Factory.New<DeltaCExportFREDIMessage>();
				message2.EM_ApplicationCode = "FRC";
				message2.EM_MessageType = MessageTypeList.Codes.EXC;
				message2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
				message2.EM_MessageText = exportMessageText2;
				message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message2.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();
				processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message2);
				Factory.Save();

				AssertEquals("There should be 1 entry header confirmed charges and the existed ones deleted", 1, entryHeader.ConfirmedCharges.Count);
				AssertConfirmedEntryHeaderFee("Charges1", entryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>().First(), "P636", 500m, "1");
				AssertEquals("There should be 1 ConfirmedFees against the first entry line and the existed ones deleted.", 1, entryLine1.ConfirmedFees.Count);
				AssertConfirmedEntryLineFee("entryLine1_Fee1", entryLine1.ConfirmedFees[0], "A465", Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 26m, 20m, 5m, "6", "");
			});
		}

		public void TestProcessorBehaviourAgainstLiquidation_ShouldKeepEntryLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "9000-B00177613";

			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 4;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingMessage);

			Factory.Save();

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithFiveLiquidations.xml");
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(10m, entryHeader.AllEntryLines.FindByLineNumber(1).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(20m, entryHeader.AllEntryLines.FindByLineNumber(2).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(30m, entryHeader.AllEntryLines.FindByLineNumber(3).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(40m, entryHeader.AllEntryLines.FindByLineNumber(4).ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals(50m, entryHeader.AllEntryLines.FindByLineNumber(5).ConfirmedFees[0].CF_ChargeAmount);
		}

		public void TestProcessMessageDeltaC_Export_Valid()
		{
			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT");
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DaltaCExportValidResponseMessage.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("No CIN message should be sent before the entry reaches BAE.", 0, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
			AssertEquals(entry.PK, message.EM_LinkedObject.PK);

			var entryNum = CusEntryNumber.Load(entry, CusEntryNumberTypes.France.Export, Core.Constants.CountryCodes.France);
			AssertNotNull("Export CusEntryNum", entryNum);
			AssertEquals("CE_EntryNum", "1907396130", entryNum.CE_EntryNum);
			AssertEquals("CE_IssueDate", new ZDateTime(2019, 9, 16, 10, 23, 0), entryNum.CE_IssueDate);
			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES050, entry.CH_EntryStatus);
			AssertEquals("JE_EntryStatusDescription", EntryStatusDescriptionCodeList.Descriptions.ES050, entry.Declaration.JE_EntryStatusDescription);
			AssertEquals("CH_Status", MessageStatusCodeList.Codes.OK, entry.CH_Status);
			AssertEquals("19FRD617C073961303", entry.MovementReferenceNumber);
			AssertEquals(new ZDateTime(2019, 9, 16, 10, 23, 0), entry.MovementReferenceNumberIssueDate);
			AssertEquals("No CIN Message should have been sent on MRN setting because the entry has not reached BAE status.", 0, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
		}

		public void TestProcessMessageDeltaC_ECS_Valid()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportECSResponseMessage.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			AssertEquals(entry.PK, message.EM_LinkedObject.PK);

			var entryNum = CusEntryNumber.Load(entry, CusEntryNumberTypes.France.Export, Core.Constants.CountryCodes.France);
			AssertNotNull("Export CusEntryNum", entryNum);
			AssertEquals("CE_EntryNum", "1907396130", entryNum.CE_EntryNum);
		}

		public void TestProcessMessageDeltaC_Export_Erreur()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErrorMessage.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("CH_EntryStatus", EntryStatusDescriptionCodeList.Codes.ES040, entry.CH_EntryStatus);
			AssertEquals("JE_EntryStatusDescription", "ERROR", entry.Declaration.JE_EntryStatusDescription);
			AssertEquals("CH_Status", MessageStatusCodeList.Codes.Error, entry.CH_Status);
		}

		public void TestDoNotSendCINMessageBeforeEEC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportESOResponseMessage.xml");
				var message = Factory.New<DeltaCExportFREDIMessage>();
				message.EM_ApplicationCode = "FRC";
				message.EM_MessageType = MessageTypeList.Codes.EXC;
				message.EM_MessageText = exportMessageText;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				AssertEquals("No CIN message should be sent when the entry has not received EEC status.", 0, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));

				exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");
				message = Factory.New<DeltaCExportFREDIMessage>();
				message.EM_ApplicationCode = "FRC";
				message.EM_MessageType = MessageTypeList.Codes.EXC;
				message.EM_MessageText = exportMessageText;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				Factory.Save();
				AssertEquals("A CIN Messagde should have been sent because the entry received an EEC status.", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
				AssertEquals(entry.PK, message.EM_LinkedObject.PK);
			}
		}

		public void TestNoWarningLoggedWhenCINIsSentSuccessfully()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");
				var message = Factory.New<DeltaCExportFREDIMessage>();
				message.EM_ApplicationCode = "FRC";
				message.EM_MessageType = MessageTypeList.Codes.EXC;
				message.EM_MessageText = exportMessageText;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				var log = new LoggingInformation();
				var processor = new ExportDeltaCResponseMessageProcessor(log);
				processor.ProcessMessage(message);
				Factory.Save();
				AssertEquals("A CIN Message should have been sent because the entry received an EEC status.", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
				AssertEquals("Log should have no warning.", 0, log.Logs.Count());
			}
		}

		public void TestDoNotSendCINMessageIfNotAir()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				Factory.Save();

				var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				AssertEquals("No CIN message should be sent when the declaration is not Air.", 0, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				Factory.Save();

				processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				Factory.Save();
				AssertEquals("A CIN Message should have been sent because the declaration is Air.", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
				AssertEquals(entry.PK, message.EM_LinkedObject.PK);
			}
		}

		public void TestSendCINMessage_745()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				Factory.Save();

				var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				var createdCin755Message = entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EntryActionCodeList.Codes.CIN745);
				AssertNotNull("A CIN 755 Message should have been sent cause JE_MasterBill is not empty.", createdCin755Message);
				Assert("Saving of factory should be delayed.", !createdCin755Message.IsInDatabase);
				Factory.Save();
				Assert("Factory should now be saved.", createdCin755Message.IsInDatabase);
				AssertEquals(entry.PK, message.EM_LinkedObject.PK);
			}
		}

		public void TestSendCINMessage_755()
		{
			JobDeclaration declaration;
			CusEntryHeader cusEntryHeader;
			DeltaCExportFREDIMessage message;
			GenerateDataForCin755Message(out declaration, out cusEntryHeader, out message, "FR123456800" , "FR123456799");

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				Factory.Save();

				var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				var createdCin755Message = cusEntryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EntryActionCodeList.Codes.CIN755);
				AssertNotNull("A CIN 755 Message should have been sent cause JE_MasterBill is not empty.", createdCin755Message);
				Assert("Saving of factory should be delayed.", !createdCin755Message.IsInDatabase);
				Factory.Save();
				Assert("Factory should now be saved.", createdCin755Message.IsInDatabase);
				AssertEquals(cusEntryHeader.PK, message.EM_LinkedObject.PK);
			}
		}

		public void TestSendCINMessage_755_ShouldLogError()
		{
			JobDeclaration declaration;
			CusEntryHeader cusEntryHeader;
			DeltaCExportFREDIMessage message;
			GenerateDataForCin755Message(out declaration, out cusEntryHeader, out message, "", "");

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				Factory.Save();

				var log = new LoggingInformation();
				var processor = new ExportDeltaCResponseMessageProcessor(log);
				processor.ProcessMessage(message);
				Factory.Save();
				AssertEquals("A CIN 755 Message shouldn't have been sent cause JE_MasterBill is not empty.", 0, cusEntryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageSubType == EntryActionCodeList.Codes.CIN755));
				AssertNotNull("Should have an error", log);
				AssertEqualsIgnoreLineBreaks("Error Magasin is missing", $"CINMessagethrowsanerrorduringtheprocessoftheEntry9000-B00177613andhasnotbeensent.Errors:MAGASINisempty.", log.Logs.LastOrDefault().Message.Replace(" ", ""));
			}
		}

		void GenerateDataForCin755Message(out JobDeclaration declaration, out CusEntryHeader cusEntryHeader, out DeltaCExportFREDIMessage message, string codeCTO, string codeDepot)
		{
			var cTO = Factory.New<OrgHeader>();
			cTO.FillWithValidTestData();
			var addressCTO = cTO.Addresses.AddNew();
			addressCTO.FillWithValidTestData();

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = codeCTO;
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			var depot = Factory.New<OrgHeader>();
			depot.FillWithValidTestData();
			var addressDepot = depot.Addresses.AddNew();
			addressDepot.FillWithValidTestData();

			var customCodedepot = depot.CustomsCodes.AddNew();
			customCodedepot.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodedepot.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodedepot.OK_CustomsRegNo = codeDepot;
			customCodedepot.OK_OA_PremisesAddress = addressDepot.PK;

			declaration = Factory.New<JobDeclaration>();
			declaration.DepotDocAddress.E2_OA_Address = addressDepot.PK;
			declaration.JE_MasterBill = "88883013825";
			declaration.JE_HouseBill = "AFCDG2019F128796";

			declaration.CustomsOffices.RemoveAndDeleteAll();
			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = "EXT";
			customsOfficeExit.CY_Data = "OfficeExit";

			OrgHeader testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";

			OrgHeader testCarrier = Factory.NewWithValidTestData<OrgHeader>();
			testCarrier.OH_FullName = "London Test Supplier DHL Ltd.";
			testCarrier.MainAddress.OA_Address1 = "Heathrow Airport";
			testCarrier.MainAddress.OA_Address2 = "Building 1B";

			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_ShippingLine = testCarrier.PK;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "PK";

			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "9000-B00177613";
			CusEntryNumber cusEntryNumber = CusEntryNumber.New(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber.CE_EntryNum = "123";
			cusEntryNumber.CE_IssueDate = ZDateTime.Now;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 5;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 15;

			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT");
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "X1Z";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			cusEntryHeader.Messages.Add(outgoingMessage);

			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");

			message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
		}

		public void TestDoNotSendCINMessageWhenCINSenderIDNotSetInRegistry()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportValidResponseMessage.xml");
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXD;
			message.EM_MessageText = exportMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var processor = new ExportDeltaDResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("CINSenderID registry is defaulted no CIN message should be send automatically.", 0, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));

			using (FRCustomsDataRegistry.Instance.CINSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT"))
			{
				processor = new ExportDeltaDResponseMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(message);
				Factory.Save();
				AssertEquals("CINSenderID registry is overriden a CIN message should have been sent automatically.", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == MessageTypeList.Codes.CIN));
				AssertEquals("Message EM_LinkedObject should be equals to the entry PK.", entry.PK, message.EM_LinkedObject.PK);
			}
		}

		[TestDate(2020, 12, 8)]
		public void TestUpdateStatus_DeltaCExport()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");
			CombineAssertions(() =>
			{
				AssertEquals("EEC", entry.CH_ExitedStatus);
				var mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals(Events.CustomsEntryStatus.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ECS=EEC", mostRecentLog.SL_Reference);
				AssertEquals("09-Dec-20 10:59:00", mostRecentLog.EventTimeOffset.ToZDateTime().ToString());
			});
		}

		[TestDate(2021, 1, 31)]
		public void TestUpdateStatus_DeltaDExport()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportECSResponseMessage.xml");
			CombineAssertions(() =>
			{
				AssertEquals("EEC", entry.CH_ExitedStatus);
				AssertEquals("100", entry.CH_EntryStatus);
				var mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals(Events.CustomsEntryStatus.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ECS=EEC", mostRecentLog.SL_Reference);
				AssertEquals("01-Feb-21 09:43:00", mostRecentLog.EventTimeOffset.ToZDateTime().ToString());
			});
		}

		[TestDate(2019, 2, 12)]
		public void TestUpdateStatus_DeltaCExportSORResponseMessage()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportSORResponseMessage.xml");
			CombineAssertions(() =>
			{
				AssertEquals("SOR", entry.CH_ExitedStatus);
				AssertEquals("100", entry.CH_EntryStatus);
				var mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals(Events.CustomsEntryStatus.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ECS=SOR", mostRecentLog.SL_Reference);
				AssertEquals("13-Feb-19 04:04:00", mostRecentLog.EventTimeOffset.ToZDateTime().ToString());
			});
		}

		public void TestUpdateStatus_DeltaCExportBAEResponseMessage()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportBAEResponseMessage.xml");
			AssertEquals(ZString.Empty, entry.CH_ExitedStatus);
		}

		[TestDate(2020, 12, 8)]
		public void TestUpdateStatus_DeltaCExportESOResponseMessage()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportESOResponseMessage.xml");
			CombineAssertions(() =>
			{
				AssertEquals("ESO", entry.CH_ExitedStatus);
				var mostRecentLog = entry.Logs.MostRecentLog;
				AssertEquals(Events.CustomsEntryStatus.Code, mostRecentLog.SL_SE_NKEvent);
				AssertEquals("ECS=ESO", mostRecentLog.SL_Reference);
				AssertEquals("09-Dec-20 10:59:00", mostRecentLog.EventTimeOffset.ToZDateTime().ToString());
			});
		}

		public void TestUpdateStatus_DeltaCExportErreurResponseMessage()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErreurResponseMessage.xml");
			AssertEquals(ZString.Empty, entry.CH_ExitedStatus);
		}

		public void TestUpdateStatus_DeltaCExportValidResponseMessage()
		{
			var entry = CreateHeaderForTestUpdateStatus("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DaltaCExportValidResponseMessage.xml");
			AssertEquals(ZString.Empty, entry.CH_ExitedStatus);
		}

		CusEntryHeader CreateHeaderForTestUpdateStatus(string xmlFilePath)
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString(xmlFilePath);
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ExportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			return entry;
		}

		public void TestConvenientProvider()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");

			var message = Factory.NewWithValidTestData<FREDIMessage>();
			message.EM_MessageType = "EXC";
			message.EM_MessageSubType = "EXC";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			Factory.Save();
			message.EM_MessageText = exportMessageText;
			var processor2 = new ExportDeltaDResponseMessageProcessor(new LoggingInformation());
			processor2.ProcessMessage(message);

			var factory = new BusinessObjectFactory();

			AssertType<DeltaCExportFREDIMessage>(factory.Load<EDIMessage>(message.PK));
			var mess = (DeltaCExportFREDIMessage)factory.Load<EDIMessage>(message.PK);
			mess.EM_MessageText = exportMessageText;

			AssertEquals("EXC", message.EM_MessageType);
			AssertType<DeltaCExportResponseMessageDataObject>("Export DELTA C", mess.MessageDataObject);

			var dataProvider = (DeltaCExportResponseMessageDataObject)mess.MessageDataObject;
			AssertEquals("EEC", dataProvider.EtatECS);
		}

		void AssertConfirmedEntryHeaderFee(ZString message, CusEntryHeaderCharges confirmedFee, ZString typeCode, ZDecimal chargeAmount, ZString methodOfPayment)
		{
			AssertEquals(message + ":Charge type should reflect the content of <codtax>", typeCode, confirmedFee.C1_ChargeType);
			AssertEquals(message + ":Charge amount should reflect the content of <mtttax>", chargeAmount, confirmedFee.C1_ChargeAmount);
			AssertEquals(message + ":Method of Payment should reflect the content of <statutLiquidation>", methodOfPayment, confirmedFee.C1_MethodOfPayment);
		}

		void AssertConfirmedEntryLineFee(ZString message, CusEntryLineFee confirmedFee, ZString typeCode, ZString methodOfCalculation, ZDecimal baseValue, ZDecimal rate, ZDecimal chargeAmount, ZString methodOfPayment, ZString rateOverride)
		{
			AssertEquals(message + ":Charge type should be the EU code", FeeTypeCodeConverter.GetEUFeeTypeCode(typeCode), confirmedFee.CF_ChargeType);
			AssertEquals(message + ":Method Of Calculation should be as the taxation is based on the value", methodOfCalculation, confirmedFee.CF_MethodOfCalculation);
			AssertEquals(message + ":Base value should reflect the content of <asstax> when the taxation is based on value", baseValue, confirmedFee.CF_BaseValue);
			AssertEquals(message + ":Rate should reflect the content of <quotax>", rate, confirmedFee.CF_Rate);
			AssertEquals(message + ":Charge amount should reflect the content of <mtttax>", chargeAmount, confirmedFee.CF_ChargeAmount);
			AssertEquals(message + ":Method of Payment should reflect the content of <statutLiquidation>", methodOfPayment, confirmedFee.CF_MethodOfPayment);
			AssertEquals(message + ":NationalFeeTypeCode should reflect the content of <codtax>", typeCode, confirmedFee.NationalFeeTypeCode);
			AssertEquals(message + ":Action should be empty", rateOverride, confirmedFee.G4_RateOverride);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
