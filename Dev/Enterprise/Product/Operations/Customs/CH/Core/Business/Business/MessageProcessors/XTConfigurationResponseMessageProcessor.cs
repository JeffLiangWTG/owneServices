using System.Collections.Generic;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

[assembly: UniversalCustomsMessageCFGProcessor(ApplicationCodeList.Codes.CHCustomsEdec, typeof(Enterprise.Customs.CH.Business.XTConfigurationResponseMessageProcessor))]

namespace Enterprise.Customs.CH.Business;

public class XTConfigurationResponseMessageProcessor : BaseConfigurationMessageProcessor<ConfigurationRequest, ConfigurationMessageResponse>
{
	protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, ConfigurationRequest requestMessage, EDIMessage message)
	{
		return message.EM_LinkedObject is GlbCompany;
	}

	protected override void ProcessMessageCore(EDIMessage message, ConfigurationMessageResponse responseMessage, ILoggingInformation logger)
	{
		var parseConfigurationMessageResult = responseMessage;
		var type = parseConfigurationMessageResult.IsSuccessful ? EventReferenceConstants.Types.XHC_OK : EventReferenceConstants.Types.XHC_Error;

		var company = (GlbCompany)message.EM_LinkedObject;
		company.Logs.AddNew(Events.MiscellaneousEvent,
			new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, type),
			new KeyValuePair<string, string>(EventReferenceParameters.Codes.InterchangeNumber, message.Interchange.EI_InterchangeNum));

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
