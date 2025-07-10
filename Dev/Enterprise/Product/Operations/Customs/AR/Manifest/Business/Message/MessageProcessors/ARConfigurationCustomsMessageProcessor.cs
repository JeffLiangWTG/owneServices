using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using BaseEDIInterchange = Enterprise.Messaging.Business.EDIInterchange;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

[assembly: UniversalCustomsMessageCFGProcessor(ApplicationCodeList.Codes.ARCustoms, typeof(Enterprise.Customs.AR.Manifest.Business.ARConfigurationCustomsMessageProcessor))]

namespace Enterprise.Customs.AR.Manifest.Business
{
	public sealed class ARConfigurationCustomsMessageProcessor : BaseConfigurationMessageProcessor<CredentialChangesRequest, UniversalEventWrapper>
	{
		protected override bool IsValidMessageCore(BaseEDIInterchange outgoingInterchange, CredentialChangesRequest requestMessage, BaseEDIMessage message)
		{
			if (outgoingInterchange != null)
			{
				var company = outgoingInterchange.Branch?.Company;
				return company != null;
			}
			return false;
		}

		protected override void ProcessMessageCore(BaseEDIMessage message, UniversalEventWrapper responseMessage, ILoggingInformation logger)
		{
			var company = message.Branch?.Company;

			if (company != null)
			{
				var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
				credential.GP_PasswordStatus = responseMessage.IsAcknowledgement ? (CargoWise.Types.ZString)GlbARExternalPassword.PasswordRegisteredCode : (CargoWise.Types.ZString)PasswordStatusList.Codes.Invalid;
			}

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(BaseEDIMessage message, object linkedObject, ILoggingInformation logger) => null;
	}
}
