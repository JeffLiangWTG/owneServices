using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
	{
		public NctsHeaderMessageSendingObjectParent(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override bool SendAndSaveMessagesCore()
		{
			var result = false;
			var sentCount = 0;

			foreach (var messageSendingAction in SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>())
			{
				var sent = messageSendingAction.MessageType.ToString() switch
				{
					NctsMessageTypeList.Codes.DEPDAT => new DEPDATSender(messageSendingAction).Send(),
					NctsMessageTypeList.Codes.DESNOT => new DESNOTSender(messageSendingAction).Send(),
					NctsMessageTypeList.Codes.DESREM => new DESREMSender(messageSendingAction).Send(),
					_ => false,
				};

				if (sent)
				{
					sentCount++;
				}
			}

			if (sentCount > 0)
			{
				try
				{
					Factory.Save();
					result = true;
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return result;
		}

		protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore() => new NctsHeaderMessageSendingObjectCollection(Factory) { new NctsHeaderMessageSendingObject(NctsHeader) };
	}
}
