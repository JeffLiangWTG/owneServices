using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS906;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS906Processor : PNTSBaseProcessor<Iets906>
	{
		public IETS906Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS906 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets906 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromCorrelationId(factory, responseMessage.MessageHeader?.CorrelationId, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided CorrelationId#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override ZString GetMessageSubType() => "906";
	}
}
