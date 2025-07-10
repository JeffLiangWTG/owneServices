using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectValidation : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectValidation
	{
		public TemporaryStorageMessageSendingObjectValidation(TemporaryStorageMessageSendingObject parent) : base(parent)
		{
		}

		protected new TemporaryStorageMessageSendingObject Parent => (TemporaryStorageMessageSendingObject)base.Parent;

		protected override bool ShouldCheckFallbackProcedure => !Parent.Header.IsUCC5;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			var sendingMessageType = Parent.MessageType.ToUpperInvariant();
			var customsStatus = Parent.Header.CustomsStatus.ToUpperInvariant();
			var info = Parent.MessageTypeInfo;

			switch (sendingMessageType)
			{
				case IETemporaryStorageMessageTypeList.Codes.Declaration:
					if (customsStatus == AISEntryStatusList.Codes.Registered || customsStatus == AISEntryStatusList.Codes.Accepted)
					{
						info.AddMessageError(Res.GetString("2258AED7-5E9D-4FBA-8256-4A20BBBE5E0B", "TS315 should not be sent when entry status is REG – Registered or ACC – Accepted."));
					}
					break;
				case IETemporaryStorageMessageTypeList.Codes.Amendment:
					if (!Parent.Header.IsUCC5)
					{
						AddMessageErrorIfCustomsStatusIsNotREG("TS313");
					}
					break;
				case IETemporaryStorageMessageTypeList.Codes.Invalidation:
					AddMessageErrorIfCustomsStatusIsNotREG("TS314");
					break;
				case IETemporaryStorageMessageTypeList.Codes.PresentationNotification:
					AddMessageErrorIfCustomsStatusIsNotREG("TS332");
					break;
			}

			void AddMessageErrorIfCustomsStatusIsNotREG(ZString messageTypeCode)
			{
				if (customsStatus != AISEntryStatusList.Codes.Registered)
				{
					info.AddMessageError(Res.GetString("D7F0DFC1-CC67-4448-97C4-5F70429628AA", "{0} should only be sent when entry status is REG – Registered.", messageTypeCode));
				}
			}
		}
	}
}
