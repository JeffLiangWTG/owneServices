using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.AR.Manifest.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		IDisposable registryDisposable;

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ARP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Argentina Customs Incoming Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "ARC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Argentina, hostedServiceAttribute.RequiresCompanyInCountry);
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
						ServiceTaskApplicationCodeList.Descriptions.ARP,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ARCustoms),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorService serviceTask)
		{
			var message = factory.Load<ARMessage>(testData.MessagePK);
			AssertEquals(ARMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			CreateHeader();
			var message = CreateMessage();

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = message.PK
			};
		}

		protected override void SetUpCore()
		{
			registryDisposable = ARCustomsDataRegistry.Instance.EnableARManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			registryDisposable.Dispose();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "2021087894512445";
			header.Bills.AddNew();

			Factory.Save();
			return header;
		}

		ARMessage CreateMessage()
		{
			var message = Factory.New<ARMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_MessageText = seaAcceptedResponse;
			message.EM_MessageType = MessageTypes.Codes.ARB;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();
			return message;
		}

		readonly ZString seaAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));
	}
}
