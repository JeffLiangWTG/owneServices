using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class EComInboundMessageCreator : BaseInboundMessageCreator
{
	public EComInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override IEnumerable<ResponseParsingResult> GetResponseParsingResults(EDIInterchange interchange)
	{
		using (var reader = interchange.GetEI_BodyTextReader())
		{
			return (interchange.EI_From == MessagingConstants.CustomsDestinationCodes.CustomsEComEmail
				? MIMETypeXTParser.ParseTextMail(reader)
				: MIMETypeXTParser.ParseTextHttp(reader)).ToArray();
		}
	}

	protected override ZString GetMessageSubTypeFromResponse(string xmlResponse) => MessageSchemaDecider.GetSpecificMessageAnalyzer<IEdecComplaintMessageAnalyzer>(xmlResponse)?.GetMessageSubType();

	public override bool RemoveSoapEnvelope => true;
}
