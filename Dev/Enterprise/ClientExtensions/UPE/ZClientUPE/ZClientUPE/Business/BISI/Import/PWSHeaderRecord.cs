
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSHeaderRecord : PWSRecordBase
	{
		public PWSHeaderRecord(ZString recordData, INotifications notifications)
			: base(recordData, notifications)
		{
		}

		protected override bool ValidateRecordData()
		{
			bool result = true;
			if (RecordData.Length < 55)
			{
				WarningNotification warning = new WarningNotification("Header Record less than 55 characters");
				Notifications.Notify(warning);
				result = false;
			}
			return result;
		}
	}
}
