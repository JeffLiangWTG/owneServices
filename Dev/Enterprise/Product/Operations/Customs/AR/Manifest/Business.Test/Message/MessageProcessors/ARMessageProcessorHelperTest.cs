using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	sealed class ARMessageProcessorHelperTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookForAsycudaManifestHeader()
		{
			var header = CreateHeader();
			var message = CreateMessage(SeaAcceptedResponse, MessageTypes.Codes.ARB);

			var processor = new ARBranchMessageProcessor { Logger = new LoggingInformation() };
			processor.ExecuteBatch();

			message.Reload();

			AssertMessage(message, header.PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInterpretationSeaMode()
		{
			ARBranchMessageProcessor processor = new ARBranchMessageProcessor { Logger = new LoggingInformation() };

			CreateHeader();
			var acceptedMessage = CreateMessage(SeaAcceptedResponse, MessageTypes.Codes.ARB);

			processor.ExecuteBatch();
			acceptedMessage.Reload();

			var expectedAcceptedResult = @"Request Accepted by AFIP
ID: 2021087894512445";
			AssertEquals(expectedAcceptedResult, acceptedMessage.EM_MessageInterpretation);

			var rejectedMessage = CreateMessage(SeaRejectedResponse, MessageTypes.Codes.ARB);

			processor.ExecuteBatch();
			acceptedMessage.Reload();
			rejectedMessage.Reload();

			var expectedResultRejected = @"Request Rejected by AFIP
ID: 2021087894512445
Description:

Reason: 3021
El código puerto es obligatorio
Verifica la lista de posibles puertos

Reason: 10282
Tipo de documento de identidad inválido.
Documento inválido.";
			AssertEquals(expectedResultRejected, rejectedMessage.EM_MessageInterpretation);
		}

		void AssertMessage(EDIMessage message, ZGuid headerPK)
		{
			CombineAssertions(() =>
			{
				AssertEquals(headerPK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, message.EM_LinkTable);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "2021087894512445";
			header.Bills.AddNew();

			Factory.Save();
			return header;
		}

		ARMessage CreateMessage(ZString bodyText, ZString messageType)
		{
			var message = Factory.New<ARMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_MessageText = bodyText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();
			return message;
		}

		string SeaAcceptedResponse => seaAcceptedResponse ?? (seaAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse)));
		string seaAcceptedResponse;

		string SeaRejectedResponse => seaRejectedResponse ?? (seaRejectedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaRejectedResponse)));
		string seaRejectedResponse;
	}
}
