using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface IAlertOrReject
	{
		ZBool RejectedFlag { get; }
		ZDateTime DateOfAlertOrRejection { get; }
		IEnumerable<IAlertOrRejectReason> AlertOrRejectionReasons { get; }
	}

	public interface IAlertOrRejectReason
	{
		ZString Reason { get; }
		ZString Information { get; }
	}
}
