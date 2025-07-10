using System;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	abstract class OrganizationEventProcessorTestBase : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestProcessEvent_Negative()
		{
			var (outboundMessage, responseMessage, code) = SetupDeliverAndProcessResponse(BuildNegativeResponse);
			var validity = code.OrgCusCodeValidity;

			CombineAssertions(() =>
			{
				AssertEquals("validity.OCV_SnapShotOfWhatIsVerified", NegativeResponseJsonPayload, validity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals("responseMessage.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("outboundMessage.EM_Status", EDIMessageStatusList.Codes.Acknowledged, outboundMessage.EM_Status);
				AssertEquals("validity.OCV_Verified", expected: false, validity.OCV_Verified);

				AssertNegativeEventResponseValidity(validity);
			});
		}

		protected virtual void AssertNegativeEventResponseValidity(OrgCusCodeValidity validity)
		{
		}

		public void TestProcessEvent_Positive()
		{
			var (outboundMessage, responseMessage, code) = SetupDeliverAndProcessResponse(pk => BuildResponse(PositiveResponseJsonPayload, pk));
			var validity = code.OrgCusCodeValidity;

			CombineAssertions(() =>
			{
				AssertEquals("validity.OCV_LastVerifiedTimeUTC", new ZDateTime(2025, 3, 19, 18, 20, 21), validity.OCV_LastVerifiedTimeUTC);
				AssertEquals("validity.OCV_VerificationAuthority", "HMC", validity.OCV_VerificationAuthority);
				AssertEquals("validity.OCV_Verified", expected: true, validity.OCV_Verified);
				AssertEquals("validity.OCV_SnapShotOfWhatIsVerified", PositiveResponseJsonPayload, validity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals("responseMessage.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("outboundMessage.EM_Status", EDIMessageStatusList.Codes.Acknowledged, outboundMessage.EM_Status);
			});
		}

		(CDSDISQueryMessage outboundMessage, IEDIMessage responseMessage, OrgCusCode code) SetupDeliverAndProcessResponse(Func<ZGuid, string> buildResponse)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "UKDEMOLHR";
			var code = GetCodeForTest(org);
			var outboundMessage = Factory.New<CDSDISQueryMessage>();
			outboundMessage.EM_ApplicationReference = code.PK.ToString();
			outboundMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.ContainedMessages.Add(outboundMessage);
			outboundInterchange.EI_From = Env.CurrentCompany.GetLicenceCode();
			outboundInterchange.EI_To = CDS.Constants.EDIInterchange.GBCustoms;

			var responseMessage = GetQueuedUniversalEventMessage(buildResponse(outboundInterchange.PK));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(responseMessage);
			Factory.SaveForTesting();

			return (outboundMessage, responseMessage, code);
		}

		protected virtual string BuildNegativeResponse(ZGuid interchangePK) => BuildResponse(NegativeResponseJsonPayload, interchangePK);

		protected string BuildResponse(string jsonPayload, ZGuid interchangePK)
		{
			var encodedPayload = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload));

			return $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>UKDEMOLHR</Key>
					<Type>Organization</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSource>
				<Key>{DataProviderString}</Key>
				<Type>DataProvider</Type> 
			</DataSource>
		</DataContext>
		<EventTime>2023-11-10T10:41:20</EventTime>
		<EventType>SVR</EventType>
		<ContextCollection>
			<Context>
				<Type>ResponseType</Type>
				<Value>{DataProviderString}</Value> 
			</Context>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{interchangePK}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>{encodedPayload}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
		}

		protected abstract string DataProviderString { get; }
		protected abstract OrgCusCode GetCodeForTest(OrgHeader org);
		protected abstract string NegativeResponseJsonPayload { get; }
		protected abstract string PositiveResponseJsonPayload { get; }
	}
}
