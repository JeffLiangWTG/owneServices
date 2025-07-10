using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS928Processor : PNTSBaseProcessor<Iets928>
	{
		public IETS928Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.ACK;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS928 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets928 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromCorrelationId(factory, responseMessage.CorrelationId, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided CorrelationId#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override ZString GetMessageSubType() => "928";
	}
}
