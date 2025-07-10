using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CTCMessageRetrieverServiceTask))]
	public class CTCMessageRetrieverServiceTaskTest : ServiceTaskTestCase<CTCMessageRetrieverServiceTask>
	{
		public void TestPhase5ExampleWithXml()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00001";
			var outgoingMessage = nctsHeader.Messages.AddNew();
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsNCTS);
			outgoingMessage.EM_MessageNum = "9";

			var interchangeBody = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("TestResponseInterchangeFromEhub.xml", "Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.Phase5.TestFiles.");
			interchangeBody = interchangeBody.Replace("{{COMMONACCESSREFERENCE}}", GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outgoingMessage));

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbCustomsNCTS;
			incomingInterchange.EI_InterchangeNum = "00001";
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_BodyText = interchangeBody;

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CTCMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { incomingInterchange.PK });
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;

			var message = new BusinessObjectFactory().LoadTop1<EDIMessage>(query);

			AssertStartsWith("Message should be CC019C", "<q1:CC019C ", message.EM_MessageText);
			//TODO Add these once message processors are implemented
			//Note: message text in TestResponseInterchangeFromEhub.xml will also need to be updated* to include the correct data
			// * After changing message text, also check NctsInboundInterchangeProcessorTest.TestSpawnOneRealExampleWithXml
			//AssertEquals("Message.EM_LinkTable", CusInBondHeaderSchema.Constants.TableName, message.EM_LinkTable);
			//AssertEquals("Message.EM_LinkUniqueID", nctsHeader.PK, message.EM_LinkUniqueID);
			//AssertEquals("00001/16763", message.EM_MessageNum);
			//AssertEquals(nctsHeader.PK, message.EM_LinkUniqueID);
			//AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs NCTS Phase 5 messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsNCTS,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"UK Customs NCTS Phase 5 interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsNCTS),
				};
			}
		}
	}
}
