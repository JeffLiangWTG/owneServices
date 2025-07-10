using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC928CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC928CProvider>
	{
		public CC928CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("396C4A7E-CC50-4CD4-ADBA-E0C3138DE147", "CC928C: POSITIVE ACKNOWLEDGE");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC928CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC928CProvider provider)
		{
			var entryStatus = "";
			if (messageAttachee is NctsDepartureMovementHeader movementHeader)
			{
				entryStatus = movementHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D ? NCTS5DepartureCustomsStatusList.Codes.PreLodged : NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			}
			return entryStatus;
		}

		protected override Type MessageInterpreterType => typeof(CC928CMessageInterpreter);
	}
}
