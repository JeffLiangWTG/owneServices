using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsMessageCFGProcessor(ApplicationCodeList.Codes.ILCustoms, typeof(Enterprise.Customs.IL.Business.MessageProcessors.ILConfigurationResponseMessageProcessor))]

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public class ILConfigurationResponseMessageProcessor : BaseConfigurationMessageProcessor<ConfigurationRequest, ConfigurationMessageResponse>
	{
		protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, ConfigurationRequest requestMessage, EDIMessage message)
		{
			return message.EM_LinkedObject is GlbCompany;
		}

		protected override void ProcessMessageCore(EDIMessage message, ConfigurationMessageResponse responseMessage, ILoggingInformation logger)
		{
			var parseConfigurationMessageResult = responseMessage;

			var wrapper = new GlbCompanyWrapper((GlbCompany)message.EM_LinkedObject);
			wrapper.GlbExternalPassword.GP_PasswordStatus = parseConfigurationMessageResult.IsSuccessful ? Constants.GlbILExternalPasswordLookups.PasswordReg : PasswordStatusList.Codes.Invalid;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(EDIMessage message, object linkedObject, ILoggingInformation logger)
		{
			if (linkedObject is GlbCompany company)
			{
				return new LinkedBusinessObjectMetaData(GlbCompany.Schema.TableName, company.PK, message.EM_GB, company.GC_Code);
			}

			return null;
		}
	}
}
