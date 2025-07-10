using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM429;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM429;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM429Processor : AISMessageProcessor<AISInboundEDIMessage, IIM429Provider>
	{
		public IM429Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C80750CF-8CFE-487E-8E92-2D03B1F23853", "IM429: Release for Import");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM429Provider provider) => provider.GetEntryStatus(message);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IIM429Provider provider) => provider.UpdateMessageAttacheeCore(messageAttachee);

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im429)))
				{
					return typeof(UCC6.V1.IM429MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im429)))
				{
					return typeof(IM429MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
