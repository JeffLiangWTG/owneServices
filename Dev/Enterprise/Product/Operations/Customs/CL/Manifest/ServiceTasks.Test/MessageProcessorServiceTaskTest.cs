using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.ServiceTasks.Testing
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
				AssertEquals("Code", "CLP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Chilean Customs Incoming Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "CHL", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Chile, hostedServiceAttribute.RequiresCompanyInCountry);
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
						ServiceTaskApplicationCodeList.Descriptions.CLP,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CLCustoms,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorService serviceTask)
		{
			var message = factory.Load<CLMessage>(testData.MessagePK);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Chile;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLUKB239892";

			var message = CreateMessage(bill.PK);

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = message.PK
			};
		}

		protected override void SetUpCore()
		{
			registryDisposable = CLCustomsDataRegistry.Instance.EnableGlobalManifestForChile.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			registryDisposable.Dispose();
		}

		IDisposable registryDisposable;

		CLMessage CreateMessage(ZGuid billPK)
		{
			var acceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaAcceptedResponse));
			var message = Factory.New<CLMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = "0000000001";
			message.EM_MessageText = acceptedResponse;
			message.EM_MessageType = MessageTypes.Codes.CHB;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			message.EM_LinkUniqueID = billPK;

			Factory.Save();
			return message;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
