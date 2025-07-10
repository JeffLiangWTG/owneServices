using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing;

class EmailInboundProcessorTest : TestCaseWithFactory
{
	public void TestCreateMesssage_Import()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("azm@mail.com"))
		{
			var correctMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009999 3 001164 1");
			var correctMailVexcan = AddMailItem("azm@mail.com", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009998 3 000053 7 - VEXCAN");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009998 3 000053 8");
			var incorrectStatusMail = AddMailItem("azm@mail.com", MailStatus.Unprocessed, "Despacho DUA IMP: 20 ES 009998 3 000053 9");
			var incorrectSubjectMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho AAA: 20 ES 009998 3 000053 6");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("20ES00999930011641", DeclarationMessageTypeList.Codes.ImportClearanceEmail),
															("20ES00999830000537", DeclarationMessageTypeList.Codes.ImportClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMailVexcan.Reload();
				AssertEquals("correctMailVexcan MI_Status", "PRS", correctMailVexcan.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_Ncts()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("azm@mail.com"))
		{
			var correctMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088719");
			var correctMail2 = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088720");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00359150119711");
			var incorrectStatusMail = AddMailItem("azm@mail.com", MailStatus.Unprocessed, "Despacho del tránsito con MRN: 21ES00359150119712");
			var incorrectSubjectMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho AAA: 20 ES 009998 3 000053 6");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES00084152088719", DeclarationMessageTypeList.Codes.NctsClearanceEmail),
															("21ES00084152088720", DeclarationMessageTypeList.Codes.NctsClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());
				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("CorrectMail2 MI_Status", "PRS", correctMail.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_T2L()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("test@mail.com"))
		{
			var correctMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");
			var incorrectStatusMail = AddMailItem("test@mail.com", MailStatus.Unprocessed, "Despacho T2L: 21ES009999L0002748");
			var incorrectSubjectMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 L 000037 4");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_T2C()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("test@mail.com"))
		{
			var correctMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999M0000707");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999M0002748");
			var incorrectStatusMail = AddMailItem("test@mail.com", MailStatus.Unprocessed, "Despacho T2L: 21ES009999M0002748");
			var incorrectSubjectMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 M 000037 4");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_T2CPOUS()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("test@mail.com"))
		{
			var correctMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho JEC: 21ES009999M0000707");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho JEC: 21ES009999M0002748");
			var incorrectStatusMail = AddMailItem("test@mail.com", MailStatus.Unprocessed, "Despacho JEC: 21ES009999M0002748");
			var incorrectSubjectMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 M 000037 4");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_Export()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("test@mail.com"))
		{
			var correctMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var correctMail2 = AddMailItem("<test@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00389110181283 VEXCAN");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var incorrectStatusMail = AddMailItem("test@mail.com", MailStatus.Unprocessed, "Levante AES EXP: 21ES00280120889150");
			var incorrectSubjectMail = AddMailItem("<test@mail.com>", MailStatus.Queued, "Levante AAA: 21 ES 00280 12 0889150");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES00280120889150", DeclarationMessageTypeList.Codes.ExportClearanceEmail),
															("21ES00389110181283", DeclarationMessageTypeList.Codes.ExportClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("CorrectMail2 MI_Status", "PRS", correctMail.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_G5()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("azm@mail.com"))
		{
			var correctMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 25ESG5G000000749Y0");
			var correctMail2 = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 21ES00084152088720");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 21ES00359150119711");
			var incorrectStatusMail = AddMailItem("azm@mail.com", MailStatus.Unprocessed, "Despacho de G5G de Recepción con MRN 21ES00359150119712");
			var incorrectSubjectMail = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho AAA 20 ES 009998 3 000053 6");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("25ESG5G000000749Y0", DeclarationMessageTypeList.Codes.G5ClearanceEmail),
															("21ES00084152088720", DeclarationMessageTypeList.Codes.G5ClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());
				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("CorrectMail2 MI_Status", "PRS", correctMail.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);
			});
		}
	}

	public void TestCreateMesssage_All()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("azm@mail.com"))
		{
			var correctMailImport = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009999 3 001164 1");
			var correctMailNcts = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088719");
			var correctMailT2L = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");
			var correctMailT2C = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999M0000707");
			var correctMailT2CPOUS = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho JEC: 21ES009999M0000808");
			var correctMailExport = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var correctMailG5 = AddMailItem("<azm@mail.com>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 25ESG5G000000749Y0");

			Factory.Save();

			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			Factory.Save();

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 7, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("20ES00999930011641", DeclarationMessageTypeList.Codes.ImportClearanceEmail),
															("21ES00084152088719", DeclarationMessageTypeList.Codes.NctsClearanceEmail),
															("21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail),
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail),
															("21ES009999M0000808", DeclarationMessageTypeList.Codes.T2cClearanceEmail),
															("21ES00280120889150", DeclarationMessageTypeList.Codes.ExportClearanceEmail),
															("25ESG5G000000749Y0", DeclarationMessageTypeList.Codes.G5ClearanceEmail),
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMailImport.Reload();
				AssertEquals("CorrectMailImport MI_Status", "PRS", correctMailImport.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMailNcts.Reload();
				AssertEquals("correctMailNcts MI_Status", "PRS", correctMailNcts.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				correctMailT2L.Reload();
				AssertEquals("correctMailT2L MI_Status", "PRS", correctMailT2L.MI_Status);
				var message3 = messages[2];
				AssertMessage("CorrectMail 3", message3);

				correctMailT2C.Reload();
				AssertEquals("correctMailT2C MI_Status", "PRS", correctMailT2C.MI_Status);
				var message4 = messages[3];
				AssertMessage("CorrectMail 4", message4);

				correctMailT2CPOUS.Reload();
				AssertEquals("correctMailT2CPOUS MI_Status", "PRS", correctMailT2CPOUS.MI_Status);
				var message5 = messages[4];
				AssertMessage("CorrectMail 5", message5);

				correctMailExport.Reload();
				AssertEquals("correctMailExport MI_Status", "PRS", correctMailExport.MI_Status);
				var message6 = messages[5];
				AssertMessage("CorrectMail 6", message6);

				correctMailG5.Reload();
				AssertEquals("correctMailExport MI_Status", "PRS", correctMailG5.MI_Status);
				var message7 = messages[5];
				AssertMessage("CorrectMail 7", message7);
			});
		}
	}

	public void TestProcessorDecodesQuotedPrintableEmail()
	{
		var mailHeader = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ClearanceEmailTestFilePath, "QuotedPrintableEmailHeader.txt");
		var mailBody = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ClearanceEmailTestFilePath, "QuotedPrintableEmailBody.txt");

		AddMailItem("<test@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00000000000001", mailHeader, mailBody);
		Factory.Save();

		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("test@mail.com"))
		{
			var logger = new Integration.DummyLogger();
			var processor = new EmailInboundProcessor(logger);
			processor.ExecuteBatch();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, "21ES00000000000001");
			var ediMessages = Factory.Load<EDIMessage>(query);
			AssertEquals("messages created", 1, ediMessages.Length);

			var createdEdiMessage = ediMessages[0];
			var expectedMsgText =
@"Su declaración de exportación con número 21ES00000000000001 ha sido despachada con el siguiente código seguro de verificación (C.S.V.) del Justificante de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): AAAV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021 Fecha de Levante: 20-06-2021 Resultado al Despacho: A2
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN";
			AssertEquals("EM_MessageText (decoded email body)", expectedMsgText, createdEdiMessage.EM_MessageText);
		}
	}

	public void TestGetMessageTypeAndMrnFromSubject()
	{
		var logger = new Integration.DummyLogger();
		var processor = new EmailInboundProcessorForTest(logger);

		CombineAssertions(() =>
		{
			var (messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho DUA IMP: 20 ES 009999 3 001164 1");
			AssertEquals("ImportClearanceEmail message type", DeclarationMessageTypeList.Codes.ImportClearanceEmail, messageType);
			AssertEquals("ImportClearanceEmail MRN", "20ES00999930011641", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho DUA IMP: 20 ES 009998 3 000053 7 - VEXCAN");
			AssertEquals("ImportClearanceEmail VEXCAN message type", DeclarationMessageTypeList.Codes.ImportClearanceEmail, messageType);
			AssertEquals("ImportClearanceEmail VEXCAN MRN", "20ES00999830000537", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho del tránsito con MRN: 21ES00084152088719");
			AssertEquals("NctsClearanceEmail message type", DeclarationMessageTypeList.Codes.NctsClearanceEmail, messageType);
			AssertEquals("NctsClearanceEmail MRN", "21ES00084152088719", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho T2L: 21ES009999L0002748");
			AssertEquals("T2L L message type", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail, messageType);
			AssertEquals("T2L L MRN", "21ES009999L0002748", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho T2L: 21ES009999M0000707");
			AssertEquals("T2L M message type", DeclarationMessageTypeList.Codes.T2cClearanceEmail, messageType);
			AssertEquals("T2L M MRN", "21ES009999M0000707", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Despacho JEC: 21ES009999M0000808");
			AssertEquals("JEC message type", DeclarationMessageTypeList.Codes.T2cClearanceEmail, messageType);
			AssertEquals("JEC MRN", "21ES009999M0000808", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Levante AES EXP: 21ES00280120889150");
			AssertEquals("ExportClearanceEmail message type", DeclarationMessageTypeList.Codes.ExportClearanceEmail, messageType);
			AssertEquals("ExportClearanceEmail MRN", "21ES00280120889150", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Levante AES EXP: 21ES00389110181283 VEXCAN");
			AssertEquals("ExportClearanceEmail VEXCAN message type", DeclarationMessageTypeList.Codes.ExportClearanceEmail, messageType);
			AssertEquals("ExportClearanceEmail VEXCAN MRN", "21ES00389110181283", mrnCode);

			(messageType, mrnCode) = processor.GetMessageTypeAndMrnFromSubjectForTest("Other");
			AssertEquals("Empty message type", ZString.Empty, messageType);
			AssertEquals("Empty MRN", ZString.Empty, mrnCode);
		});
	}

	void AssertMessage(ZString mailType, EDIMessage message)
	{
		AssertEquals(mailType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
		AssertEquals(mailType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals(mailType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals(mailType + " message.EM_IsActive", true, message.EM_IsActive);
		AssertEquals(mailType + " message.EM_MessageText", "Body", message.EM_MessageText);
	}

	MailItem AddMailItem(ZString from, ZString status, ZString subject, string header = "Header", string body = "Body")
	{
		var mailItem = Factory.New<MailItem>();
		mailItem.MI_From = from;
		mailItem.MI_Subject = subject;
		mailItem.MI_Status = status;
		mailItem.MI_Direction = MailDirection.Receive;
		mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
		mailItem.MI_ReceivedDateTime = ZDateTime.Now;
		mailItem.MI_Header = header;
		mailItem.MI_Body = body;
		mailItem.MI_Application = MailFilterCodes.ESImportMailTask;
		return mailItem;
	}

	sealed class EmailInboundProcessorForTest : EmailInboundProcessor
	{
		public EmailInboundProcessorForTest(Integration.ILogger serviceLogger) : base(serviceLogger)
		{
		}

		public (ZString messageType, ZString mrnCode) GetMessageTypeAndMrnFromSubjectForTest(ZString subject) => GetMessageTypeAndMrnFromSubject(subject);
	}
}
