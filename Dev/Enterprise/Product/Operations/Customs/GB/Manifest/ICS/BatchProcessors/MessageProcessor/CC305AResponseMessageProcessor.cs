using System.Linq;
using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC305A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.ICS
{
	public class CC305AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc305AType>
	{
		public CC305AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC305A;

		protected override ZString MessageInterpretation(Cc305AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC305A);
			if (messageObject?.Heahea?.AmeRejMotTexHea605 is string reason)
			{
				result += $"<p>Rejection Reason: {WebUtility.HtmlEncode(reason)}</p>";
			}
			result += GenerateFunctionalErrorsInterpretation(messageObject?.Funerrer1?.Select(error => (error.ErrTypEr11, error.ErrPoiEr12, error.ErrReaEr13, error.OriAttValEr14)).ToList());
			return result;
		}
	}
}
