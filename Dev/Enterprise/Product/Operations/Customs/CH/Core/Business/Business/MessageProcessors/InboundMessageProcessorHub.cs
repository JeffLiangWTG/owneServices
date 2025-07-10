using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public class InboundMessageProcessorHub : BranchCustomsMessageProcessor
{
	public InboundMessageProcessorHub(IEnumerable<ZString> applicationCodes, IEnumerable<ZString> messageTypes) : base(applicationCodes, messageTypes)
	{
	}

	public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
	{
		return MessageProcessors.Cast<BaseMessageProcessor>().FirstOrDefault(x => x.CanProcess(message));
	}

	protected override bool MessageShouldBeProcessedInASeparateFactory => true;

	protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
	{
		var result = base.GetMessageProcessorsCore();
		result.Add(new EdecRuleErrorMessageProcessor(Logger));
		result.Add(new EdecAcceptanceMessageProcessor(Logger));
		result.Add(new EdecXmlSchemaErrorMessageProcessor(Logger));
		result.Add(new DocumentMessageProcessor(Logger));
		result.Add(new EdecStatusMessageProcessor(Logger));
		result.Add(new EdecCustomsRejectionMessageProcessor(Logger));
		result.Add(new EbdDocumentImportResponseMessageProcessor(Logger));
		result.Add(new EvvDocumentResponseMessageProcessor(Logger));
		result.Add(new EvvRejectionMessageProcessor(Logger));
		result.Add(new BordereauListResponseMessageProcessor(Logger));
		result.Add(new BordereauResponseMessageProcessor(Logger));
		result.Add(new EComResponseMessageProcessor(Logger));
		result.Add(new EComRequestMessageProcessor(Logger));
		result.Add(new XtErrorResponseMessageProcessor(Logger));
		result.Add(new BordereauErrorResponseMessageProcessor(Logger));

		if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Switzerland && GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled)
		{
			result.Add(new TokenRefreshMessageProcessor(Logger));
			result.Add(new PassarMessageListAcceptanceMessageProcessor(Logger));
			result.Add(new PassarMessageListRejectionMessageProcessor(Logger));
			result.Add(new PassarGetMessageAcknowledgeMessageProcessor(Logger));
			result.Add(new PassarGetMessageRejectionMessageProcessor(Logger));
			result.Add(new CharteraOutputGetMessageRejectionMessageProcessor(Logger));
			result.Add(new CharteraOutputMessageListAcceptanceMessageProcessor(Logger));
			result.Add(new CharteraOutputMessageListRejectionMessageProcessor(Logger));
			result.Add(new CharteraOutputDocumentDeliveryResultMessageProcessor(Logger));
			result.Add(new CharteraOutputDocumentSearchResultMessageProcessor(Logger));
			result.Add(new CharteraOutputDocumentRejectionMessageProcessor(Logger));
			result.Add(new CharteraOutputErrorMessageProcessor(Logger));
			result.Add(new CharteraOutputAcknowledgeMessageProcessor(Logger));
			result.Add(new PassarExportMessageProcessor(Logger));
			result.Add(new NC084ResponseMessageProcessor(Logger));
			result.Add(new NE004ResponseMessageProcessor(Logger));
			result.Add(new NE009ResponseMessageProcessor(Logger));
			result.Add(new NE021ResponseMessageProcessor(Logger));
			result.Add(new NE028ResponseMessageProcessor(Logger));
			result.Add(new NE029ResponseMessageProcessor(Logger));
			result.Add(new NE060ResponseMessageProcessor(Logger));
			result.Add(new NE083ResponseMessageProcessor(Logger));
			result.Add(new NE096ResponseMessageProcessor(Logger));
			result.Add(new NE131ResponseMessageProcessor(Logger));

			result.AddRange(ObjectFactory.Get<ICHNctsMessageProcessorsProvider>().GetMessageProcessors(Logger));
		}

		return result;
	}
}
