using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	[TestedType(typeof(USIServiceTask))]
	public class USIServiceTaskTest : ServiceTaskTestCase<USIServiceTask>
	{
		public void TestGrEngineDisables()
		{
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var validMessage = GetMessageRowWithValidMessageContent();
			Factory.Save();

			var serviceTask = new USIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var newFactory = new BusinessObjectFactory();
			validMessage = newFactory.Load<EDIMessage>(validMessage.PK);
			AssertEquals("The service task is disabled so do nothing.", EDIMessage.Status.Queued, validMessage.EM_Status);
		}

		public void TestRightMessageGetProcessed()
		{
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "XXX";

			var invalidMessage2 = GetMessageRowWithValidMessageContent();
			invalidMessage2.EM_MessageType = "XXX";

			var invalidMessage3 = GetMessageRowWithValidMessageContent();
			invalidMessage3.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var invalidMessage4 = GetMessageRowWithValidMessageContent();
			invalidMessage4.EM_Status = "XXX";

			var invalidMessage5 = GetMessageRowWithValidMessageContent();
			invalidMessage5.EM_MessageText = "Doesn'tHaveRealXMLInIt.";

			var validMessage = GetMessageRowWithValidMessageContent();

			Factory.Save();

			var serviceTask = new USIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage1.EM_Status);

			invalidMessage2 = newFactory.Load<EDIMessage>(invalidMessage2.PK);
			AssertEquals("InvalidMessage2.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage2.EM_Status);

			invalidMessage3 = newFactory.Load<EDIMessage>(invalidMessage3.PK);
			AssertEquals("InvalidMessage3.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage3.EM_Status);

			invalidMessage4 = newFactory.Load<EDIMessage>(invalidMessage4.PK);
			AssertEquals("InvalidMessage4.EM_Status should not be changed", "XXX", invalidMessage4.EM_Status);

			invalidMessage5 = newFactory.Load<EDIMessage>(invalidMessage5.PK);
			AssertEquals("InvalidMessage5.EM_Status should be changed to 'Failed'", EDIMessage.Status.Rejected, invalidMessage5.EM_Status);

			validMessage = newFactory.Load<EDIMessage>(validMessage.PK);
			AssertEquals("ValidMessage.EM_Status should be set to 'Processed'", EDIMessage.Status.ProcessedOK, validMessage.EM_Status);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Schedule",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalSchedule),
				};
			}
		}

		EDIMessage GetMessageRowWithValidMessageContent()
		{
			return GetMessageRowWithValidMessageContent(Factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalSchedule,
				XmlEDIMessage.Status.Queued);
		}

		[ThreadSafe]
		static int messageId = 0;

		public static EDIMessage GetMessageRowWithValidMessageContent(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, string status)
		{
			var message = factory.New<EDIMessage>();

			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_Status = status;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = ValidXMLScheduleContent;
			message.EM_MessageNum = Interlocked.Increment(ref messageId).ToString("D20");
			return message;
		}

		#region ValidXMLEventContentForMasterBill123_45678901

		const string ValidXMLScheduleContent =
@"<ns0:UniversalSchedule xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <ns0:Schedule>
        <ns0:DataProvider>1ST</ns0:DataProvider>
        <ns0:IsCancellation>false</ns0:IsCancellation>
        <ns0:Carrier>
          <ns0:AddressType>Carrier</ns0:AddressType>
          <ns0:OrganizationCode>ZIM</ns0:OrganizationCode>
          <ns0:CompanyName>ZIM LINE</ns0:CompanyName>
        </ns0:Carrier>
        <ns0:Transport>
          <ns0:Sea>
            <ns0:Vessel>
              <ns0:VesselName>APL MT POOL</ns0:VesselName>
              <ns0:LloydsNumber>9999991</ns0:LloydsNumber>
            </ns0:Vessel>
            <ns0:VoyageNumber>001TestVN</ns0:VoyageNumber>
          </ns0:Sea>
        </ns0:Transport>
        <ns0:DischargeCollection>
        </ns0:DischargeCollection>
        <ns0:LoadingCollection>
          <ns0:Loading>
            <ns0:Port>
              <ns0:Code>AUFRE</ns0:Code>
            </ns0:Port>
            <ns0:ActualDeparture>2014-01-01T22:22:22</ns0:ActualDeparture>
            <ns0:EstimatedDeparture>2014-06-20T22:00:00</ns0:EstimatedDeparture>
            <ns0:FCLCutOff>2014-06-30T22:00:00</ns0:FCLCutOff>
            <ns0:FCLReceivalCommences>2012-07-06T06:00:00</ns0:FCLReceivalCommences>
            <ns0:TerminalCode>CONFR</ns0:TerminalCode>
          </ns0:Loading>
        </ns0:LoadingCollection>
      </ns0:Schedule>
    </ns0:UniversalSchedule>";

		#endregion
	}
}
