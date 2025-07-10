using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX515VProcessor : AESMessageProcessor<AESInboundEDIMessage, EX515VProvider>
	{
		public EX515VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override Type MessageInterpreterType => typeof(EX515VMessageInterpreter);
		protected override string MessageFriendlyNameCore => Res.GetString("7E995679-9D2A-4663-9752-D5EBBEE625A3", "EX515V: Export Declaration Acknowledgment");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX515VProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX515VProvider provider) => AESEntryStatusList.Codes.Prelodged;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, EX515VProvider provider)
		{
			if (!string.IsNullOrEmpty(provider.MovementReferenceNumber))
			{
				var movementReferenceNumber = CusEntryNumber.LoadOrCreate((BusinessObject)messageAttachee, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
				movementReferenceNumber.CE_EntryNum = provider.MovementReferenceNumber;
			}
		}
	}
}
