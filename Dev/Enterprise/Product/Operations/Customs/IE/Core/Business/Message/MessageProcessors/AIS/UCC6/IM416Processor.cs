using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM416Processor : AISMessageProcessor<AISInboundEDIMessage, IIM416Provider>
	{
		public IM416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A141F031-8DC6-46F0-BC05-8E2A734CA4AC", "IM416: Customs Declaration Rejection");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM416Provider provider)
		{
			return !provider.HasFunctionalErrors ? null : LogicalStatusList.Codes.Invalid;
		}

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM416Provider provider) => provider.EntryStatus;

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im416)))
				{
					return typeof(UCC6.V1.IM416MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im416)))
				{
					return typeof(IM416MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
