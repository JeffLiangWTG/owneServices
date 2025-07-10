using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "MXP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Mexican Customs Incoming Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "MXC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Mexico, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
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
						ServiceTaskApplicationCodeList.Descriptions.MXP,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.MXCustoms,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorService serviceTask)
		{
			var message = factory.Load<MXMessage>(testData.MessagePK);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;

			var bill = header.Bills.AddNew();

			var bill2 = header.Bills.AddNew();

			CreateMessageSend(bill);
			CreateMessageSend(bill2);

			var firstBody = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFirstResponse));
			var finalBody = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFinalResponse));

			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.MXF);
			var requestMessage = CreateMessage(firstBody, requestInterchange.PK, bill.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXF);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypes.Codes.MXF);
			var responseMessage = CreateMessage(finalBody, responseInterchange.PK, bill.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.ProcessedOK, MessageTypes.Codes.MXF);

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = responseMessage.PK
			};
		}

		MXMessage CreateMessage(ZString file, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString type)
		{
			var message = Factory.New<MXMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_ApplicationReference = "MAN0000001";
			message.EM_MessageNum = "0070";
			message.EM_MessageType = type;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = file;
			message.EM_EI = interchangePK;
			message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			message.EM_LinkUniqueID = billPK;

			Factory.Save();
			return message;
		}

		void CreateMessageSend(AsycudaBill bill)
		{
			var messageSend = bill.Factory.New<MXMessage>();
			messageSend.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			messageSend.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			messageSend.EM_LinkUniqueID = bill.PK;
			messageSend.EM_MessageNum = "0070";
			messageSend.EM_MessageText = "Message Text";
			messageSend.EM_MessageType = MessageTypes.Codes.MXF;
			messageSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageSend.EM_Status = EDIMessageStatusList.Codes.Received;

			Factory.Save();
		}

		MXInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status, ZString type)
		{
			var interchange = Factory.New<MXInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = type;
			interchange.EI_From = MXMessageConstants.MXCustomsForSeaMode;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;

			Factory.Save();
			return interchange;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
