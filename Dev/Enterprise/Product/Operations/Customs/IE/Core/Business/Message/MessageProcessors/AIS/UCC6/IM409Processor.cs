using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM409;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM409Processor : AISMessageProcessor<AISInboundEDIMessage, IIM409Provider>
	{
		public IM409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("32DFEC1C-D6CF-4781-B4A9-C63F7C90A2A7", "IM409: Invalidation Request Decision");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM409Provider provider)
		{
			return provider.InvalidationDecision ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;
		}

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM409Provider provider)
		{
			return provider.InvalidationDecision ? AISEntryStatusList.Codes.Cancelled : null;
		}

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im409)))
				{
					return typeof(UCC6.V1.IM409MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im409)))
				{
					return typeof(IM409MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
