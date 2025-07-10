using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ES.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSendingService))]
	class MessageSendingServiceTest : ServiceTaskTestCase<MessageSendingService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ESS", hostedServiceAttribute.Code);
				AssertEquals("Description", "ES Customs Message Sending", hostedServiceAttribute.Description);
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
			var serviceTask = new MessageSendingService();
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

		public void TestRunTask()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
			var correctMessageXML = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ImportQuery, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, XMLMessageText);
			correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

			var entryHeaderCorrectEdifact = declaration.CustomsEntryHeaders.AddNew();
			var correctMessageEdifact = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, EdifactMessageText);
			correctMessageEdifact.EM_LinkedObject = entryHeaderCorrectEdifact;

			var incorrectApplicationCodeMessage = CreateOutboundMessage("AAA", DeclarationMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, EdifactMessageText);
			var incorrectDirectionMessage = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, EdifactMessageText);
			var incorrectStatusMessage = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ArrivalAtExit, EDIMessage.Direction.Transmit, EDIMessage.Status.Withdrawn, EdifactMessageText);

			var nullLinkedObjectMessage = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ImportQuery, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, XMLMessageText);

			var entryHeaderEmptyApplicationReference = declaration.CustomsEntryHeaders.AddNew();
			var emptyApplicationReferenceMessage = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ImportQuery, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, XMLMessageText);
			emptyApplicationReferenceMessage.EM_LinkedObject = entryHeaderEmptyApplicationReference;
			emptyApplicationReferenceMessage.EM_ApplicationReference = ZString.Empty;

			var entryHeaderemptyText = declaration.CustomsEntryHeaders.AddNew();
			var emptyTextMessage = CreateOutboundMessage(ApplicationCodeList.Codes.ESCustomsMessage, DeclarationMessageTypeList.Codes.ImportQuery, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, ZString.Empty);
			emptyTextMessage.EM_LinkedObject = entryHeaderemptyText;

			Factory.Save();

			CombineAssertions("PRE-CONDITION", () =>
			{
				AssertNull("correctMessageXML Message Interchange", correctMessageXML.Interchange);
				AssertNull("correctMessageEdifact Message Interchange", correctMessageEdifact.Interchange);
				AssertNull("incorrectApplicationCodeMessage Message Interchange", incorrectApplicationCodeMessage.Interchange);
				AssertNull("incorrectDirectionMessage Message Interchange", incorrectDirectionMessage.Interchange);
				AssertNull("incorrectStatusMessage Message Interchange", incorrectStatusMessage.Interchange);
				AssertNull("nullLinkedObjectMessage Message Interchange", nullLinkedObjectMessage.Interchange);
				AssertNull("emptyApplicationReferenceMessage Message Interchange", emptyApplicationReferenceMessage.Interchange);
				AssertNull("emptyTextMessage Message Interchange", emptyTextMessage.Interchange);
			});

			var logs = InitialiseAndRunTaskSchedule(new MessageSendingService());

			correctMessageXML.Reload();
			correctMessageEdifact.Reload();
			incorrectApplicationCodeMessage.Reload();
			incorrectDirectionMessage.Reload();
			incorrectStatusMessage.Reload();
			nullLinkedObjectMessage.Reload();
			emptyApplicationReferenceMessage.Reload();
			emptyTextMessage.Reload();

			CombineAssertions("Status", () =>
			{
				AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
				AssertEquals("correctMessageEdifact Status", EDIMessage.Status.Sent, correctMessageEdifact.EM_Status);
				AssertEquals("incorrectApplicationCodeMessage Status", EDIMessage.Status.Queued, incorrectApplicationCodeMessage.EM_Status);
				AssertEquals("incorrectDirectionMessage Status", EDIMessage.Status.Queued, incorrectDirectionMessage.EM_Status);
				AssertEquals("incorrectStatusMessage Status", EDIMessage.Status.Withdrawn, incorrectStatusMessage.EM_Status);
				AssertEquals("nullLinkedObjectMessage Status", EDIMessage.Status.Failed, nullLinkedObjectMessage.EM_Status);
				AssertEquals("emptyApplicationReferenceMessage Status", EDIMessage.Status.Failed, emptyApplicationReferenceMessage.EM_Status);
				AssertEquals("emptyTextMessage Status", EDIMessage.Status.Failed, emptyTextMessage.EM_Status);

				AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
				AssertNotNull("correctMessageEdifact Interchange", correctMessageEdifact.Interchange);
				AssertNull("incorrectApplicationCodeMessage Interchange", incorrectApplicationCodeMessage.Interchange);
				AssertNull("incorrectDirectionMessage Interchange", incorrectDirectionMessage.Interchange);
				AssertNull("incorrectStatusMessage Interchange", incorrectStatusMessage.Interchange);
				AssertNull("nullLinkedObjectMessage Interchange", nullLinkedObjectMessage.Interchange);
				AssertNull("emptyApplicationReferenceMessage Interchange", emptyApplicationReferenceMessage.Interchange);
				AssertNull("emptyTextMessage Interchange", emptyTextMessage.Interchange);

				AssertNotEquals("correctMessageXML.Interchange and correctMessageEdifact.Interchange must be different (no collation)", correctMessageXML.EM_EI, correctMessageEdifact.EM_EI);
				AssertEquals("correctMessageXML Interchange has EI_GP same as the messages's EM_GP", correctMessageXML.EM_GP, correctMessageXML.Interchange.EI_GP);
				AssertEquals("correctMessageEdifact Interchange has EI_GP same as the messages's EM_GP", correctMessageEdifact.EM_GP, correctMessageEdifact.Interchange.EI_GP);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						MessageSendingService.FriendlyName,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ESCustomsMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		ESEDIMessage CreateOutboundMessage(ZString applicationCode, ZString messageType, ZString direction, ZString status, ZString messageText)
		{
			var message = Factory.New<ESEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageText = messageText;
			message.EM_IsTestMessage = true;
			message.EM_ApplicationReference = certificate.CertificateName;
			message.EM_GP = certificate.CertificatePK;
			return message;
		}

		ZString XMLMessageText => @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}:ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Ent.xsd"">
  <SegmentosDeServicio Id = ""ES2001091613234560"" fecha=""20200109"" hora=""161323"" Test=""S"" />
  <NumeroDeReferencia>21ES00999912345678</NumeroDeReferencia>
  <DatosEnATC>S</DatosEnATC>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>";

		ZString EdifactMessageText => @"UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200109:1513+1++&EE++++1'UNH+1+CUSDEC:1:921:UN:ECSR02'BGM+EAL+1234123444'CST++++++11ES00280110000101'LOC+42+ES::141:000801'LOC+43+0811::148+BCN010::148'DTM+128:20190815:102'NAD+1+1210244B::148+MIDIRECCION.CORREO.EN.CASTILLAYLEON:@MIXMAIL.COM+GUTIERREZ S.A.'UNT+8+1'UNZ+1+1'";

		protected override void SetUpCore()
		{
			base.SetUpCore();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
		}

		CertificateProviderTestClass certificate;
		GlbStaff staff;
	}
}
