using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCForeignOperatorInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCForeignOperatorInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override IEnumerable<ZString> GetMessageTexts(ZString messageType, ZString messageSubType, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			return messageSubType == EDIMessageSubTypeList.Codes.Error ? [responseMessage] : BRMessageHelper.DeserializeJsonObjectArray(responseMessage, needsUnzip: true);
		}
	}
}
