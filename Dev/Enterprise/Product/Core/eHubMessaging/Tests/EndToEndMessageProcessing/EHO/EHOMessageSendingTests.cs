using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.EHO
{
	public class EHOMessageSendingTests : TestCaseWithFactory
	{
		public void TestSendDocumentWithdrawal_USCustoms()
		{
			var mockAdapter = new Mock<EHubAdapterMock>() { CallBase = true };
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			var trackingID = Guid.NewGuid();
			var messageText = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHO.TestFiles.USDISWithdrawal.xml");

			var interchange = Factory.New<EDIInterchange>();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			interchange.ContainedMessages.Add(message);

			var interchangePK = interchange.PK;
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = Env.CurrentBranch.PK;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISWithdrawal;
			interchange.EI_BodyText = messageText;
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "US DIS";
			interchange.EI_SessionGUID = trackingID;

			Factory.Save();

			mockAdapter.Setup(m => m.SendMessages()).Callback(() =>
				{
					AssertEquals(1, mockAdapter.Object.Outbox.Count);
					var eHubMessage = mockAdapter.Object.Outbox.First();
					AssertEquals("USD", eHubMessage.ApplicationCode);
					AssertEquals("", eHubMessage.EmailSubject);
					AssertEquals("", eHubMessage.Filename);
					AssertEquals("USDIS", eHubMessage.RecipientID);
					AssertEquals(EDIMessageSchemaNameList.Descriptions.USDDIS, eHubMessage.SchemaName);
					AssertEquals(MessageSchemaType.Xml, eHubMessage.SchemaType);
					AssertEquals("ABC Corp", eHubMessage.SenderID);
					AssertEquals(trackingID, eHubMessage.TrackingID);
					AssertEquals(messageText, eHubMessage.MessageStream.ReadToEnd());
				});

			var serviceTaskJob = serviceTask.GetJobs().First();

			serviceTaskJob.Execute(CancellationToken.None);

			Factory.ReloadAll<EDIInterchange>();
			var processedInterchange = Factory.Load<EDIInterchange>(interchangePK);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange.EI_Status);

			mockAdapter.VerifyAll();
		}

		public void TestSendDocumentSubmission_USCustoms()
		{
			var mockAdapter = new Mock<EHubAdapterMock>() { CallBase = true };
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			var trackingID = Guid.NewGuid();
			var messageTextTemplate = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHO.TestFiles.USDISSubmission.xml");

			var container = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonContainer>();
			var documentManager = container as IDocManagerSupport;

			string documentText = "Document1 content pdf";
			var documentBytes = Encoding.UTF8.GetBytes(documentText);
			var documentStream = (SubStreamableStream)new MemoryStream(documentBytes);
			var eDoc1 = documentManager.DocManagerInfo.AddFileOrDocument(documentStream, "Document", "DIS");

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			message.EM_LinkedObject = container;

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = "Attachment1.pdf";
			attachment.EG_EdiMsgDocType = "APP";
			attachment.EG_StorageDocsGuid = eDoc1.UniqueKey;

			string messageText = messageTextTemplate.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", "!dOcUmEnTiMaGePlAcEHoLdEr:" + attachment.PK + "!");

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			var interchangePK = interchange.PK;
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = Env.CurrentBranch.PK;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISSubmission;
			interchange.EI_BodyText = messageText;
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "US DIS";
			interchange.EI_SessionGUID = trackingID;

			documentManager.DocManagerInfo.Save();
			Factory.Save();

			mockAdapter.Setup(x => x.SendMessages()).Callback(() =>
			{
				AssertEquals(1, mockAdapter.Object.Outbox.Count);
				var eHubMessage = mockAdapter.Object.Outbox.First();
				AssertEquals("USD", eHubMessage.ApplicationCode);
				AssertEquals("", eHubMessage.EmailSubject);
				AssertEquals("", eHubMessage.Filename);
				AssertEquals("USDIS", eHubMessage.RecipientID);
				AssertEquals(EDIMessageSchemaNameList.Descriptions.USDDIS, eHubMessage.SchemaName);
				AssertEquals(MessageSchemaType.Xml, eHubMessage.SchemaType);
				AssertEquals("ABC Corp", eHubMessage.SenderID);
				AssertEquals(trackingID, eHubMessage.TrackingID);

				string expectedMessageText = messageTextTemplate.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", Convert.ToBase64String(documentBytes));
				AssertEquals(expectedMessageText, eHubMessage.MessageStream.ReadToEnd());
			});

			var serviceTaskJob = serviceTask.GetJobs().First();

			serviceTaskJob.Execute(CancellationToken.None);

			Factory.ReloadAll<EDIInterchange>();
			var processedInterchange = Factory.Load<EDIInterchange>(interchangePK);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange.EI_Status);

			mockAdapter.VerifyAll();
		}

		public void TestSendDocumentSubmission_USCustoms_DISHostProvider()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "USC";
			company.GC_RN_NKCountryCode = "US";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "USB";
			Factory.Save();

			var mockAdapter = new Mock<EHubAdapterMock>() { CallBase = true };
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object), company);

			var trackingID = Guid.NewGuid();
			var messageTextTemplate = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHO.TestFiles.USDISSubmission.xml");

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
			declaration[JobDeclarationSchema.JE_ApplicationCode.Name] = "ACE";
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_GB] = branch.PK;

			string documentText = "Document1 content pdf";
			var documentBytes = Encoding.UTF8.GetBytes(documentText);
			var documentStream = (SubStreamableStream)new MemoryStream(documentBytes);
			var documentManager = (IDocManagerSupport)declaration;

			var eDoc1 = documentManager.DocManagerInfo.AddFileOrDocument(documentStream, "Document", "DIS");

			var requiredDoc = ((IDocsAndCartageParent)shipment).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			message.EM_LinkedObject = requiredDocAddInfo;
			message.EM_GB = branch.PK;

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = "Attachment1.pdf";
			attachment.EG_EdiMsgDocType = "APP";
			attachment.EG_StorageDocsGuid = eDoc1.UniqueKey;

			string messageText = messageTextTemplate.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", "!dOcUmEnTiMaGePlAcEHoLdEr:" + attachment.PK + "!");

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			var interchangePK = interchange.PK;
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = branch.PK;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISSubmission;
			interchange.EI_BodyText = messageText;
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "US DIS";
			interchange.EI_SessionGUID = trackingID;

			documentManager.DocManagerInfo.Save();
			Factory.Save();

			mockAdapter.Setup(x => x.SendMessages()).Callback(() =>
			{
				AssertEquals(1, mockAdapter.Object.Outbox.Count);
				var eHubMessage = mockAdapter.Object.Outbox.First();
				AssertEquals("USD", eHubMessage.ApplicationCode);
				AssertEquals("", eHubMessage.EmailSubject);
				AssertEquals("", eHubMessage.Filename);
				AssertEquals("USDIS", eHubMessage.RecipientID);
				AssertEquals(EDIMessageSchemaNameList.Descriptions.USDDIS, eHubMessage.SchemaName);
				AssertEquals(MessageSchemaType.Xml, eHubMessage.SchemaType);
				AssertEquals("ABC Corp", eHubMessage.SenderID);
				AssertEquals(trackingID, eHubMessage.TrackingID);

				string expectedMessageText = messageTextTemplate.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", Convert.ToBase64String(documentBytes));
				AssertEquals(expectedMessageText, eHubMessage.MessageStream.ReadToEnd());
			});

			var serviceTaskJob = serviceTask.GetJobs().First();

			serviceTaskJob.Execute(CancellationToken.None);

			var processedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchangePK);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange.EI_Status);

			mockAdapter.VerifyAll();
		}

		public void TestSendZACustomsMessage()
		{
			var mockAdapter = new Mock<EHubAdapterMock>() { CallBase = true };
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			var trackingID = Guid.NewGuid();
			var messageText = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHO.TestFiles.ZACustoms.txt");

			var interchange = Factory.New<EDIInterchange>();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ZACustoms;
			interchange.ContainedMessages.Add(message);

			var interchangePK = interchange.PK;
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = Env.CurrentBranch.PK;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ZACustoms;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.ZACustoms;
			interchange.EI_BodyText = messageText;
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "ZACustoms";
			interchange.EI_SessionGUID = trackingID;

			Factory.Save();

			mockAdapter.Setup(x => x.SendMessages()).Callback(() =>
			{
				AssertEquals(1, mockAdapter.Object.Outbox.Count);
				var eHubMessage = mockAdapter.Object.Outbox.First();
				AssertEquals("ZAC", eHubMessage.ApplicationCode);
				AssertEquals("", eHubMessage.EmailSubject);
				AssertEquals("", eHubMessage.Filename);
				AssertEquals("ZACustoms", eHubMessage.RecipientID);
				AssertEquals(EDIMessageSchemaNameList.Descriptions.ZACustoms, eHubMessage.SchemaName);
				AssertEquals(MessageSchemaType.FlatFile, eHubMessage.SchemaType);
				AssertEquals("ABC Corp", eHubMessage.SenderID);
				AssertEquals(trackingID, eHubMessage.TrackingID);
				AssertEquals(messageText, eHubMessage.MessageStream.ReadToEnd());
			});

			var serviceTaskJob = serviceTask.GetJobs().First();

			serviceTaskJob.Execute(CancellationToken.None);

			Factory.ReloadAll<EDIInterchange>();
			var processedInterchange = Factory.Load<EDIInterchange>(interchangePK);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange.EI_Status);

			mockAdapter.VerifyAll();
		}

		public void TestSendHKCustomsMessage()
		{
			var mockAdapter = new Mock<EHubAdapterMock>() { CallBase = true };
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			var trackingID = Guid.NewGuid();
			var messageText = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHO.TestFiles.HKCustoms.txt");

			var interchange = Factory.New<EDIInterchange>();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.HKTraxon;
			interchange.ContainedMessages.Add(message);

			var interchangePK = interchange.PK;
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = Env.CurrentBranch.PK;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.HKTraxon;
			interchange.EI_BodyText = messageText;
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "HK Customs";
			interchange.EI_SessionGUID = trackingID;

			Factory.Save();

			mockAdapter.Setup(x => x.SendMessages()).Callback(() =>
			{
				AssertEquals(1, mockAdapter.Object.Outbox.Count);
				var eHubMessage = mockAdapter.Object.Outbox.First();
				AssertEquals("TRX", eHubMessage.ApplicationCode);
				AssertEquals("", eHubMessage.EmailSubject);
				AssertEquals("", eHubMessage.Filename);
				AssertEquals("HKCustoms", eHubMessage.RecipientID);
				AssertEquals(EDIMessageSchemaNameList.Descriptions.HKCustoms, eHubMessage.SchemaName);
				AssertEquals(MessageSchemaType.FlatFile, eHubMessage.SchemaType);
				AssertEquals("ABC Corp", eHubMessage.SenderID);
				AssertEquals(trackingID, eHubMessage.TrackingID);
				AssertEquals(messageText, eHubMessage.MessageStream.ReadToEnd());
			});

			var serviceTaskJob = serviceTask.GetJobs().First();

			serviceTaskJob.Execute(CancellationToken.None);

			Factory.ReloadAll<EDIInterchange>();
			var processedInterchange = Factory.Load<EDIInterchange>(interchangePK);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange.EI_Status);

			mockAdapter.VerifyAll();
		}

		public void TestFailureSendingMessage()
		{
			var configurationForTestFailureSendingMessage = GetConfigurationDataForTestFailureSendingMessage();

			GlbCompany demoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			demoCompany.GC_RN_NKCountryCode = configurationForTestFailureSendingMessage.CountryCode;
			var demoBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "DEM");
			demoBranch.GB_Code = "DEM";
			demoCompany.Branches.Add(demoBranch);
			Factory.Save();
			var mockAdapter = new EHubAdapterMock();
			var serviceTask = GetNewOutboundServiceTaskMock(new AdaptorFactoryMockWithOneAdaptor(mockAdapter), demoCompany);

			var message = GetNewEDIMessageForTestFailureSendingMessage();
			message.EM_ApplicationCode = configurationForTestFailureSendingMessage.ApplicationCode;
			message.EM_GB = demoBranch.PK;

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message);
			interchange.EI_InterchangeNum = "00000001";
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_GB = demoBranch.PK;
			interchange.EI_ApplicationCode = configurationForTestFailureSendingMessage.ApplicationCode;
			interchange.EI_InterchangeType = configurationForTestFailureSendingMessage.InterchangeType;
			interchange.EI_BodyText = "Anything";
			interchange.EI_From = "ABC Corp";
			interchange.EI_To = "US DIS";
			interchange.EI_SessionGUID = Guid.NewGuid();

			Factory.Save();

			var serviceTaskJob = serviceTask.GetJobs().First();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("EHO", canRunInAnyBranch: true))
			{
				serviceTaskJob.Execute(CancellationToken.None);
			}

			var processedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, processedInterchange.EI_Status);
			AssertProcessedInterchangeInTestFailureSendingMessage(processedInterchange);
		}

		protected virtual ConfigurationForTestFailureSendingMessage GetConfigurationDataForTestFailureSendingMessage() => new ConfigurationForTestFailureSendingMessage(Core.Constants.CountryCodes.Australia, ApplicationCodeList.Codes.USCustomsDIS, EDIInterchangeTypeList.Codes.USDISSubmission);
		protected virtual EDIMessage GetNewEDIMessageForTestFailureSendingMessage() => Factory.NewWithValidTestData<EDIMessage>();
		protected virtual void AssertProcessedInterchangeInTestFailureSendingMessage(EDIInterchange processedInterchange) { }

		OutboundServiceTaskMock GetNewOutboundServiceTaskMock(IAdaptorFactory adaptorFactory, long maxReceivedMessageSize = -1, int outboxSizeLimitInBytes = -1)
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			return GetNewOutboundServiceTaskMock(adaptorFactory, currentCompany, maxReceivedMessageSize, outboxSizeLimitInBytes);
		}

		OutboundServiceTaskMock GetNewOutboundServiceTaskMock(IAdaptorFactory adaptorFactory, GlbCompany currentCompany, long maxReceivedMessageSize = -1, int outboxSizeLimitInBytes = -1)
		{
			var logger = new NotificationBuffer();

			var companySettingManager = new Mock<ICompanySettingsManager>();
			var companySettings = new Mock<ICompanySettings>();
			companySettingManager.Setup(m => m.Companies).Returns(new[] { currentCompany });
			companySettingManager.Setup(m => m.GetSetting(currentCompany)).Returns(companySettings.Object);
			companySettings.Setup(m => m.PasswordExists).Returns(true);

			var serviceTask = new OutboundServiceTaskMock(companySettingManager.Object, adaptorFactory, maxReceivedMessageSize, outboxSizeLimitInBytes) { Notifier = logger };
			return serviceTask;
		}
	}

	public class ConfigurationForTestFailureSendingMessage
	{
		public ConfigurationForTestFailureSendingMessage(ZString countryCode, ZString messageApplicationCode, ZString interchangeType)
		{
			CountryCode = Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
			ApplicationCode = Argument.NotNullOrEmpty(messageApplicationCode, nameof(messageApplicationCode));
			InterchangeType = Argument.NotNullOrEmpty(interchangeType, nameof(interchangeType));
		}

		public ZString CountryCode { get; }
		public ZString ApplicationCode { get; }
		public ZString InterchangeType { get; }
	}
}
