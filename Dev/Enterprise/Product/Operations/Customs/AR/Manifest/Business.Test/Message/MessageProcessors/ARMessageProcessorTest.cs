using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	sealed class ARMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLinkBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "2021087894512445";
			header.Bills.AddNew();
			Factory.Save();

			var message = CreateMessage(SeaAcceptedResponse);

			processor.ExecuteBatch();
			message.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeader.Schema.TableName, message.EM_LinkTable);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessHeaderNotFound()
		{
			_ = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var message = CreateMessage(SeaAcceptedResponse);

			processor.ExecuteBatch();
			message.Reload();

			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
		}

		public void TestProcessMessageEmptyAndInvalidBody()
		{
			var message1 = CreateMessage(ZString.Empty);
			var message2 = CreateMessage("BODYTEXT");

			processor.ExecuteBatch();
			message1.Reload();
			message2.Reload();

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message2.EM_Status);
		}

		ARMessage CreateMessage(ZString bodyText)
		{
			var message = Factory.New<ARMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_MessageText = bodyText;
			message.EM_MessageType = MessageTypes.Codes.ARB;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();
			return message;
		}

		readonly ARBranchMessageProcessor processor = new ARBranchMessageProcessor { Logger = new LoggingInformation() };

		ZString SeaAcceptedResponse => ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));
	}
}
