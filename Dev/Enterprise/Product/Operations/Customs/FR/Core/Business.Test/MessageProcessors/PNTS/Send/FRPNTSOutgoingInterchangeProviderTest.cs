using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRPNTSOutgoingInterchangeProviderTest : FRInterchangeProviderBaseTest<FRPNTSOutgoingInterchangeProvider>
	{
		protected override ZString MessageType => MessageTypeList.Codes.STO;

		protected override ZString MessageSubType => TemporaryStorageMessageTypeList.Codes.CombinedTSD;

		protected override ZString ExpectedInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS;
	}
}
