using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.NCTS
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

			foreach (var action in SelectedSendingObjects.Cast<EU.NCTS.Business.NctsHeaderMessageSendingObject>())
			{
				var sender = new NctsMessageSender(new NctsMessageSendingAction(action));
				if (sender.Send() != null)
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
	}
}
