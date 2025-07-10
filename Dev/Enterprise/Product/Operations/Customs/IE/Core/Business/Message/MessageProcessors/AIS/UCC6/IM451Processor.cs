using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM451;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM451;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM451Processor : AISMessageProcessor<AISInboundEDIMessage, IIM451Provider>
	{
		public IM451Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B2B544A1-279B-4CB8-BCD7-ED02D7676CDB", "IM451: No Release");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM451Provider provider) => AISEntryStatusList.Codes.NotReleased;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM451Provider provider) =>
			messageAttachee is Integration.Customs.IEH7.IAsycudaBill ? AISEntryStatusList.Codes.Accepted : base.GetLogicalStatus(message, messageAttachee, provider);

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im451)))
				{
					return typeof(UCC6.V1.IM451MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im451)))
				{
					return typeof(IM451MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
