using System;
using System.Linq;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.EHI
{
	public class EHIMessageProcessingTests : TestCaseWithFactory
	{
		public void TestProcessUSDISReply()
		{
			var logger = new NotificationBuffer();
			var companySettingManager = GetCompanySettings();

			var messageTrackingId = Guid.NewGuid();
			using (var messageStream = ResourceManager.GetFileResource("EndToEndMessageProcessing.EHI.TestFiles.USDISReply.xml"))
			{
				var eHubMessage = new eHubMessage(
					messageTrackingId,
					"USDIS",
					"HYEDAUIKB",
					MessageSchemaType.Xml,
					"USD",
					"http://cbp.dhs.gov/DIS#MessageEnvelope",
					messageStream);

				var serviceTask = new InboundServiceTaskMock(companySettingManager, new[] { eHubMessage }) { Notifier = logger };

				int interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("PRECONDITION: EDIInterchange table should be empty", 0, interchangeCount);
				AssertEquals("PRECONDITION: EDIMessage table should be empty", 0, messageCount);

				var serviceTaskJob = serviceTask.GetJobs().First();
				serviceTaskJob.Execute(CancellationToken.None);

				interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals(1, interchangeCount);
				AssertEquals("Only Interchange created", 0, messageCount);

				var createdInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

				string expectedMessage = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHI.TestFiles.USDISReply.xml");

				AssertEquals(EDIInterchangeStatusList.Codes.Queued, createdInterchange.EI_Status);
				AssertEquals(ApplicationCodeList.Codes.USCustomsDIS, createdInterchange.EI_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Receive, createdInterchange.EI_ReceiveTransmit);
				AssertEquals(ZBool.True, createdInterchange.EI_IsActive);
				AssertEquals("REP", createdInterchange.EI_InterchangeType);
				AssertEquals("USDIS", createdInterchange.EI_From);
				AssertEquals("HYEDAUIKB", createdInterchange.EI_To);
				AssertEquals(messageTrackingId.ToString(), createdInterchange.EI_SessionGUID.ToString());
				AssertEquals(expectedMessage, createdInterchange.EI_BodyText);
				AssertEquals("", createdInterchange.EI_HeaderText);
				AssertEquals("", createdInterchange.EI_FooterText);
			}
		}

		public void TestProcessUSCAMA()
		{
			var logger = new NotificationBuffer();
			var companySettingManager = GetCompanySettings();

			var messageTrackingId = Guid.NewGuid();
			using (var messageStream = ResourceManager.GetFileResource("EndToEndMessageProcessing.EHI.TestFiles.USCAMA.txt"))
			{
				var eHubMessage = new eHubMessage(
					messageTrackingId,
					"USC",
					"HYEDAUIKB",
					MessageSchemaType.FlatFile,
					"AMA",
					"USCustoms AMA",
					messageStream);

				var serviceTask = new InboundServiceTaskMock(companySettingManager, new[] { eHubMessage }) { Notifier = logger };

				int interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("PRECONDITION: EDIInterchange table should be empty", 0, interchangeCount);
				AssertEquals("PRECONDITION: EDIMessage table should be empty", 0, messageCount);

				var serviceTaskJob = serviceTask.GetJobs().First();
				serviceTaskJob.Execute(CancellationToken.None);

				interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals(1, interchangeCount);
				AssertEquals("Only Interchange created", 0, messageCount);

				var createdInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

				var expectedHeader = @"QK WTGTAAF
.WASUSCR 10034259";
				var expectedMessage = @"FSC
LAXZI
439-21031010-BKG210310HB1
ARR/ZI100/10MAR
FSC/10
WBL/SYD/T1/L1/THINGS
ARR/ZI100/10MAR";

				AssertEquals(EDIInterchangeStatusList.Codes.Queued, createdInterchange.EI_Status);
				AssertEquals(ApplicationCodeList.Codes.USAMA, createdInterchange.EI_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Receive, createdInterchange.EI_ReceiveTransmit);
				AssertEquals(ZBool.True, createdInterchange.EI_IsActive);
				AssertEquals("FSC", createdInterchange.EI_InterchangeType);
				AssertEquals("USC", createdInterchange.EI_From);
				AssertEquals("HYEDAUIKB", createdInterchange.EI_To);
				AssertEquals(messageTrackingId.ToString(), createdInterchange.EI_SessionGUID.ToString());
				AssertEquals(expectedMessage, createdInterchange.EI_BodyText);
				AssertEquals(expectedHeader, createdInterchange.EI_HeaderText);
				AssertEquals(string.Empty, createdInterchange.EI_FooterText);
			}
		}

		public void TestProcessZACustomMessage()
		{
			var logger = new NotificationBuffer();
			var companySettingManager = GetCompanySettings();

			var messageTrackingId = Guid.NewGuid();
			using (var messageStream = ResourceManager.GetFileResource("EndToEndMessageProcessing.EHI.TestFiles.ZACustoms.txt"))
			{
				var eHubMessage = new eHubMessage(
					messageTrackingId,
					"ZACustoms",
					"HYEDAUIKB",
					MessageSchemaType.FlatFile,
					EDIInterchangeTypeList.Codes.ZACustoms,
					EDIInterchangeTypeList.Descriptions.ZACustoms,
					messageStream);

				var serviceTask = new InboundServiceTaskMock(companySettingManager, new[] { eHubMessage }) { Notifier = logger };

				int interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("PRECONDITION: EDIInterchange table should be empty", 0, interchangeCount);
				AssertEquals("PRECONDITION: EDIMessage table should be empty", 0, messageCount);

				var serviceTaskJob = serviceTask.GetJobs().First();
				serviceTaskJob.Execute(CancellationToken.None);

				interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals(1, interchangeCount);
				AssertEquals("Only Interchange created", 0, messageCount);

				var createdInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

				string expectedMessage = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHI.TestFiles.ZACustoms.txt");

				AssertEquals(EDIInterchangeStatusList.Codes.Queued, createdInterchange.EI_Status);
				AssertEquals(ApplicationCodeList.Codes.ZACustoms, createdInterchange.EI_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Receive, createdInterchange.EI_ReceiveTransmit);
				AssertEquals(ZBool.True, createdInterchange.EI_IsActive);
				AssertEquals(EDIInterchangeTypeList.Codes.ZACustoms, createdInterchange.EI_InterchangeType);
				AssertEquals("ZACustoms", createdInterchange.EI_From);
				AssertEquals("HYEDAUIKB", createdInterchange.EI_To);
				AssertEquals(messageTrackingId.ToString(), createdInterchange.EI_SessionGUID.ToString());
				AssertEquals(expectedMessage, createdInterchange.EI_BodyText);
				AssertEquals("", createdInterchange.EI_HeaderText);
				AssertEquals("", createdInterchange.EI_FooterText);
			}
		}

		public void TestProcessHKCustomsMessage()
		{
			var logger = new NotificationBuffer();
			var companySettingManager = GetCompanySettings();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.HongKong);

			var messageTrackingId = Guid.NewGuid();
			using (var messageStream = ResourceManager.GetFileResource("EndToEndMessageProcessing.EHI.TestFiles.HKCustoms.txt"))
			{
				var eHubMessage = new eHubMessage(
					messageTrackingId,
					"HKCustoms",
					"HYEDAUIKB",
					MessageSchemaType.FlatFile,
					EDIInterchangeTypeList.Codes.HKCustoms,
					EDIInterchangeTypeList.Descriptions.HKCustoms,
					messageStream);

				var serviceTask = new InboundServiceTaskMock(companySettingManager, new[] { eHubMessage }) { Notifier = logger };

				int interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("PRECONDITION: EDIInterchange table should be empty", 0, interchangeCount);
				AssertEquals("PRECONDITION: EDIMessage table should be empty", 0, messageCount);

				var serviceTaskJob = serviceTask.GetJobs().First();
				serviceTaskJob.Execute(CancellationToken.None);

				interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals(1, interchangeCount);
				AssertEquals(1, messageCount);

				var createdInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

				string expectedMessage = ResourceManager.GetFileResourceString("EndToEndMessageProcessing.EHI.TestFiles.HKCustomsBody.txt").TrimEnd(System.Environment.NewLine.ToCharArray());

				AssertEquals(EDIInterchangeStatusList.Codes.Received, createdInterchange.EI_Status);
				AssertEquals(ApplicationCodeList.Codes.HKTraxon, createdInterchange.EI_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Receive, createdInterchange.EI_ReceiveTransmit);
				AssertEquals(ZBool.True, createdInterchange.EI_IsActive);
				AssertEquals(EDIInterchangeTypeList.Codes.HKCustoms, createdInterchange.EI_InterchangeType);
				AssertEquals("RHKAPT01HKGSTCR", createdInterchange.EI_From);
				AssertEquals("RHKAGT021330984/HKG85", createdInterchange.EI_To);
				AssertEquals(messageTrackingId.ToString(), createdInterchange.EI_SessionGUID.ToString());
				AssertEquals(expectedMessage, createdInterchange.EI_BodyText);
				AssertEquals("UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:PIMA+RHKAGT021330984/HKG85:PIMA+210720:0939+5162+0'", createdInterchange.EI_HeaderText);
				AssertEquals("UNZ+1+5162'", createdInterchange.EI_FooterText);

				var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
				AssertEquals(EDIMessageStatusList.Codes.Queued, createdMessage.EM_Status);
				AssertEquals(ApplicationCodeList.Codes.HKTraxon, createdMessage.EM_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Receive, createdMessage.EM_ReceiveTransmit);
				AssertEquals(ZBool.True, createdMessage.EM_IsActive);
				AssertEquals(EDIMessageTypeList.Codes.Traxon, createdMessage.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.Traxon, createdMessage.EM_MessageSubType);
				AssertEquals(expectedMessage, createdMessage.EM_MessageText);
			}
		}

		ICompanySettingsManager GetCompanySettings()
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var companySettingManager = new Mock<ICompanySettingsManager>();
			var companySettings = new Mock<ICompanySettings>();
			companySettingManager.Setup(m => m.Companies).Returns(new[] { currentCompany });
			companySettingManager.Setup(m => m.GetSetting(currentCompany)).Returns(companySettings.Object);
			companySettings.Setup(m => m.PasswordExists).Returns(true);

			return companySettingManager.Object;
		}
	}
}
