using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS
{
	public class NctsTransmissionMessageGenerator : EU.NCTS.Business.MessageGeneration.NctsTransmissionMessageGenerator
	{
		public NctsTransmissionMessageGenerator(NctsMessageFunctionSet messageFunction) : base(messageFunction)
		{
		}

		protected override ZString GetApplicationCode()
		{
			return ApplicationCodeList.Codes.GbCommonTransitConvention;
		}

		protected override string GetMessageStatusAfterSentCore(string messageCode) => NctsMessageStatusList.Codes.MessageQueued;
	}
}
