
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSTrailerRecord : PWSRecordBase
	{
		public PWSTrailerRecord(ZString recordData, INotifications notifications)
			: base(recordData, notifications)
		{
		}

		protected override bool ValidateRecordData()
		{
			bool result = true;
			if (RecordData.Length < 20)
			{
				WarningNotification warning = new WarningNotification("Trailer Record less than 20 characters");
				Notifications.Notify(warning);
				result = false;
			}
			return result;
		}
	}
}
