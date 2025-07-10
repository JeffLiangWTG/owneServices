using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM404;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM404;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM404Processor : AISMessageProcessor<AISInboundEDIMessage, IIM404Provider>
	{
		public IM404Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("AC4CA696-9EB9-4E87-B9C9-8E6336DE46E8", "IM404: Amendment Request Registration");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM404Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im404)))
				{
					return typeof(UCC6.V1.IM404MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im404)))
				{
					return typeof(IM404MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IIM404Provider provider) => provider.UpdateMessageAttacheeCore(messageAttachee);
	}
}
