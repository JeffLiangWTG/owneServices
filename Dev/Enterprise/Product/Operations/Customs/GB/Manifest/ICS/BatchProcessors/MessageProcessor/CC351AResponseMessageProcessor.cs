using CargoWise.Customs.GB.MessageDefinitions.ICS.CC351A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.ICS
{
	public class CC351AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc351AType>
	{
		public CC351AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC351A;

		protected override ZString MessageInterpretation(Cc351AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC351A);
			if (messageObject?.Cusint632?.Count > 0)
			{
				result += $"Customs Intervention{(messageObject.Cusint632.Count > 1 ? "s" : "")}<br>";
				var tableCreator = new HtmlTableCreator(columnTitles: ["Item Number concerned", "Customs intervention code", "Customs intervention text"]);
				foreach (var cusint in messageObject.Cusint632)
				{
					tableCreator.WriteRow(cusint.IteNumConCusint668, cusint.CusIntCodCusint665, cusint.CusIntTexCusint666);
				}
				result += tableCreator.ToHtml();
			}
			return result;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, Cc351AType messageObject)
		{
			manifestHeader.RegistrationStatus = RegistrationStatusList.Codes.DoNotLoad;
			if (string.IsNullOrEmpty(manifestHeader.RegistrationNumber))
			{
				manifestHeader.RegistrationNumber = messageObject.Heahea?.DocNumHea5;
			}
		}
	}
}
