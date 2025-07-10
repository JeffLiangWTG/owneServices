using System.Linq;
using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC324A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.ICS
{
	public class CC324AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc324AType>
	{
		public CC324AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC324A;

		protected override ZString MessageInterpretation(Cc324AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC324A);
			if (messageObject?.Heahea?.RejReaHea127 is string reason)
			{
				result += $"<p>Rejection Reason: {WebUtility.HtmlEncode(reason)}</p>";
			}
			result += GenerateFunctionalErrorsInterpretation(messageObject?.Funerrer1?.Select(error => (error.ErrTypEr11, error.ErrPoiEr12, error.ErrReaEr13, error.OriAttValEr14)).ToList());
			return result;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, Cc324AType messageObject)
		{
			manifestHeader.RegistrationStatus = RegistrationStatusList.Codes.DiversionRequestDenied;
		}
	}
}
