using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM405;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM405Processor : AISMessageProcessor<AISInboundEDIMessage, IIM405Provider>
	{
		public IM405Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B1F48E76-E80B-4088-88BC-AB2943F7CBFF", "IM405: Amendment Request Rejection");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM405Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im405)))
				{
					return typeof(UCC6.V1.IM405MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im405)))
				{
					return typeof(IM405MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
