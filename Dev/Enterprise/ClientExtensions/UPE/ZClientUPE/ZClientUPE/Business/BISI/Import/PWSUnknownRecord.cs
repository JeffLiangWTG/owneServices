
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSUnknownRecord : PWSRecordBase
	{
		public PWSUnknownRecord(ZString recordData, INotifications notifications)
			: base(recordData, notifications)
		{
		}

		protected override bool ValidateRecordData()
		{
			return true;
		}
	}
}
