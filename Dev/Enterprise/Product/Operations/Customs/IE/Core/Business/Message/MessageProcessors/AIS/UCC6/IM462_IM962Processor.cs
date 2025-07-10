using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM962;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM462_IM962Processor : AISMessageProcessor<AISInboundEDIMessage, IM462_IM962Provider>
	{
		public IM462_IM962Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("98A9895B-70FF-4551-B367-4C5D4919CF28", "{0}: Amendment Request", xmlObjectType.IsAssignableFrom(typeof(Im962)) ? "IM962" : "IM462");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM462_IM962Provider provider) => AISEntryStatusList.Codes.AmendmentRequested;

		protected override Type MessageInterpreterType => typeof(IM462_IM962MessageInterpreter);
	}
}
