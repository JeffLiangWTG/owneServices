using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using static Enterprise.Customs.IE.Business.Constants;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM099;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM099;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM099Processor : AISMessageProcessor<AISInboundEDIMessage, IIM099Provider>
	{
		public IM099Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => AISInterchangeTypeList.Descriptions.IM099;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM099Provider provider)
		{
			if (provider != null)
			{
				if (provider.Remarks.EqualsIgnoringCase(RemarksTypeList.RefundApplicationRequired))
				{
					return AISEntryStatusList.Codes.RefundApplicationRequested;
				}
				else if (provider.Remarks.EqualsIgnoringCase(RemarksTypeList.InsufficientFund))
				{
					return AISEntryStatusList.Codes.InsufficientFund;
				}
			}
			return null;
		}

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im099)))
				{
					return typeof(UCC6.V1.IM099MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im099)))
				{
					return typeof(IM099MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
