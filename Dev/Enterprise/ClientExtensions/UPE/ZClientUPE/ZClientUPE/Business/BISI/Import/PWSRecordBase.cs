
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public abstract class PWSRecordBase : UPERecordBase
	{
		public PWSRecordBase(ZString recordData, INotifications notifications)
		{
			this.RecordData = recordData;
			this.Notifications = notifications;
			this.HasErrors = !ValidateRecordData();
		}

		protected abstract bool ValidateRecordData();

		public readonly bool HasErrors;
		protected readonly ZString RecordData;
		protected readonly INotifications Notifications;
	}
}
