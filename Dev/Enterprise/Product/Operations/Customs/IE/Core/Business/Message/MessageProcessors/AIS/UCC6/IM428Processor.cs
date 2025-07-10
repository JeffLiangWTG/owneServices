using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM428;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM428Processor : AISMessageProcessor<AISInboundEDIMessage, IIM428Provider>
	{
		public IM428Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5993BB14-35FF-4219-989E-9DA9606B252C", "IM428: Customs Declaration Acceptance");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM428Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM428Provider provider) => AISEntryStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IIM428Provider provider) => provider.UpdateMessageAttacheeCore(messageAttachee);

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im428)))
				{
					return typeof(UCC6.V1.IM428MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im428)))
				{
					return typeof(IM428MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
