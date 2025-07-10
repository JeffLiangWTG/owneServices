using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC531CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC531Provider>
	{
		public CC531CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("8691C556-E284-496F-96CF-8841D9D24232", "CC531C: EXPIRY OF TIMER FOR SUPPLEMENTARY DECLARATION NOTIFICATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC531Provider provider) => LogicalStatusList.Codes.Sent;

		protected override Type MessageInterpreterType => typeof(CC531MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC531Provider provider)
		{
			base.UpdateMessageAttacheeCore(messageAttachee, provider);
			var cusEntry = CusEntryNumber.LoadOrCreate((BusinessObject)messageAttachee, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			cusEntry.CE_ExpiryDate = provider.SupplementaryDeclarationLodgementEnd;
		}
	}
}
