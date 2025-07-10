using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ES.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrievingService))]
	class MessageRetrievingServiceTest : ServiceTaskTestCase<MessageRetrievingService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ESR", hostedServiceAttribute.Code);
				AssertEquals("Description", "ES Customs Message Retrieving", hostedServiceAttribute.Description);
				AssertEquals("Category", "ESC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Spain, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
			});
		}

		public void TestInitialiseSchedule()
		{
			var serviceTask = new MessageRetrievingService();
			InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

			CombineAssertions(() =>
			{
				AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
				Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
				AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
				AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
			});
		}

		[TestDate(2019, 10, 30, 09, 36, 0)]
		public void TestRunTaskForEDIFACTMessage_Ehub()
		{
			var edifactString = "UNB+UNOA:1+AEATADUE:ZZ+BUZON:ZZ+200102:1100+02110053624233++&EE'UNH+1+CUSRES:1:921:UN:ECS001'";
			var processedEdifactString = "UNH+1+CUSRES:1:921:UN:ECS001'";

			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00183715";
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			Factory.Save();

			var interchange1 = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForEhub(edifactString), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), true);
			Factory.Save();

			var task = new MessageRetrievingService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			interchange1.Reload();

			CombineAssertions(() =>
			{
				var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("NumberOfMessages", 1, messagesCreated.Length);

				var message = messagesCreated[0];
				AssertMessage(message, "EDIFACT", processedEdifactString, interchange1, entryHeader.CH_BGMReference, interchange1.EI_InterchangeNum, true);
			});
		}

		[TestDate(2019, 10, 30, 09, 36, 0)]
		public void TestRunTaskForEDIFACTMessage_DirectxT()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00183715";
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			Factory.Save();

			var correctEDIFACTAnswer = "UNH+1+CUSRES:1:921:UN:ECS001'";

			var interchange1 = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWithData.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), false);
			Factory.Save();

			var task = new MessageRetrievingService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			interchange1.Reload();

			CombineAssertions(() =>
			{
				var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("NumberOfMessages", 1, messagesCreated.Length);

				var message = messagesCreated[0];
				AssertMessage(message, "EDIFACT", correctEDIFACTAnswer, interchange1, ZString.Empty, interchange1.EI_InterchangeNum, false);
			});
		}

		[TestDate(2019, 10, 30, 09, 36, 0)]
		public void TestRunTaskForXMLMessage_Ehub()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00183715";
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			Factory.Save();

			var interchange1 = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, BodyTextSoap("ResponseWithCorrectResponse.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), true);
			Factory.Save();

			var task = new MessageRetrievingService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			interchange1.Reload();

			CombineAssertions(() =>
			{
				var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("NumberOfMessages", 1, messagesCreated.Length);

				var message = messagesCreated[0];
				AssertMessage(message, "XML", BodyTextSoap("ResponseWithCorrectResponse.xml"), interchange1, entryHeader.CH_BGMReference, interchange1.EI_InterchangeNum, true);
			});
		}

		[TestDate(2019, 10, 30, 09, 36, 0)]
		public void TestRunTaskForXMLMessage_DirectxT()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00183715";
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			Factory.Save();

			var interchange1 = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithCorrectResponse.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), false);
			Factory.Save();

			var task = new MessageRetrievingService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			interchange1.Reload();

			CombineAssertions(() =>
			{
				var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("NumberOfMessages", 1, messagesCreated.Length);

				var message = messagesCreated[0];
				AssertMessage(message, "XML", ExpectedBodyTextSoap, interchange1, ZString.Empty, interchange1.EI_InterchangeNum, false);
			});
		}

		public void TestRunTaskWithConcurrentEhubAndXtInterchanges()
		{
			var edifactAnswer = "UNH+1+CUSRES:1:921:UN:ECS001'";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B20221207";
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			Factory.Save();

			var ehubInterchange = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForEhub(edifactAnswer), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), isEHub: true);
			var xtInterchange = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWithData.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), isEHub: false);
			Factory.Save();

			var task = new MessageRetrievingService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			CombineAssertions("Message Count", () =>
			{
				AssertEquals("Total", 2, Factory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("eHub", 1, ehubInterchange.ContainedMessages.Count);
				AssertEquals("xT", 1, xtInterchange.ContainedMessages.Count);
			});

			CombineAssertions("Interchange Statuses and Unwrapped Messages", () =>
			{
				AssertInterchangeAndContainedMessage("eHub", ehubInterchange, edifactAnswer, expectedAppReference: entryHeader.CH_BGMReference, expectedIsTest: true);
				AssertInterchangeAndContainedMessage("xT", xtInterchange, edifactAnswer, expectedAppReference: ZString.Empty, expectedIsTest: false);
			});
		}

		void AssertInterchangeAndContainedMessage(string msgExchangeSystem, ESEDIInterchange interchange, ZString expectedBody, ZString expectedAppReference, ZBool expectedIsTest)
		{
			interchange.Reload();
			AssertEquals(msgExchangeSystem + " Interchange => EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertMessage(interchange.ContainedMessages[0], msgExchangeSystem, expectedBody, interchange, expectedAppReference, interchange.EI_InterchangeNum, expectedIsTest);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						MessageRetrievingService.FriendlyName,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.ESCustomsMessage),
				};
			}
		}

		void AssertMessage(EDIMessage message, ZString messageType, ZString bodyText, EDIInterchange interchange, ZString bGMReference, ZString messageNum, ZBool isTest)
		{
			AssertEquals(messageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(messageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(messageType + " message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals(messageType + " message.EM_MessageType", interchange.EI_InterchangeType, message.EM_MessageType);
			AssertEquals(messageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(messageType + " message.EM_EI", interchange.PK, message.EM_EI);
			AssertEquals(messageType + " message.EM_GB", interchange.EI_GB, message.EM_GB);
			AssertEquals(messageType + " message.EM_ApplicationReference", bGMReference, message.EM_ApplicationReference);
			AssertEquals(messageType + " message.EM_IsActive", true, message.EM_IsActive);
			AssertEquals(messageType + " message.EM_IsTestMessage", isTest, message.EM_IsTestMessage);
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageText = textReader.ReadToEnd();
				AssertEquals(messageType + " message.EM_MessageText", bodyText, messageText);
			}
		}

		ESEDIInterchange CreateInterchange(ZString from, ZString bodyText, ZString entryRefNum, ZString interchangeType, ZGuid sessionGuid, bool isEHub)
		{
			var interchange = Factory.New<ESEDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = from;
			interchange.EI_To = "TEST";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_BodyText = bodyText;
			interchange.EI_SessionGUID = sessionGuid;
			if (isEHub)
			{
				interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
				interchange.EI_HeaderText = ZString.Format(@"
<Headers>
	<BrokerCode>AZ</BrokerCode>
	<CertificateName>CertName</CertificateName>
	<CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
	<EntryReferenceNumber>{0}</EntryReferenceNumber>
	<TestMessage>Y</TestMessage>
	<SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>
", entryRefNum);
			}
			else
			{
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			}
			return interchange;
		}

		ZString CreateBodyTextEdiFactForEhub(ZString answer)
		{
			return ZString.Format(@"
<Response>
	<DeclarationStatus>0</DeclarationStatus>
	<Respuesta>{0}</Respuesta>
</Response>
", answer);
		}

		ZString CreateBodyTextEdiFactForDirectxT(ZString fileName)
		{
			using (var stream = typeof(EdifactResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.EdifactMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		ZString BodyTextSoap(ZString fileName)
		{
			using (var stream = typeof(SoapResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.SoapMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		string ExpectedBodyTextSoap => @"<?xml version=""1.0""?><env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header /><env:Body Id=""Body""><DVDH2V1Sal xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDH2V1Sal.xsd"" xmlns:cau=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/cau/ws/CAUTiposDeDatos.xsd"" xmlns:dvdt=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd""><Mensaje><SegmentosDeServicio><dvdt:Id>20220923121419948549</dvdt:Id><dvdt:Remitente>ES.AEAT</dvdt:Remitente><dvdt:IdCorr>LSV230920221663927974</dvdt:IdCorr><dvdt:FechaPreparacion>20220923</dvdt:FechaPreparacion><dvdt:HoraPreparacion>121254</dvdt:HoraPreparacion><dvdt:IndicadorTest>S</dvdt:IndicadorTest></SegmentosDeServicio></Mensaje><Respuesta><CodigoRespuesta>A</CodigoRespuesta><CodigoOperacion>2</CodigoOperacion><ED_2_5_NRL>TEST_PWS</ED_2_5_NRL><ED_3_16_NumIdentifDepositante>ESA78587268</ED_3_16_NumIdentifDepositante><ED_3_18_NumIdentifDeclarante>ESA78587268</ED_3_18_NumIdentifDeclarante><TipoDeDeclaracion>DVD</TipoDeDeclaracion><MRN>22ES009999D04136R3</MRN><Circuito>V</Circuito><FechaPresentacion>20220923</FechaPresentacion><HoraPresentacion>121254</HoraPresentacion><FechaAdmision>20220923</FechaAdmision><HoraAdmision>121254</HoraAdmision><CSV_DeclaracionElectronica>PNS6NA3WMAUC4J8W</CSV_DeclaracionElectronica></Respuesta></DVDH2V1Sal></env:Body></env:Envelope>";
	}
}
