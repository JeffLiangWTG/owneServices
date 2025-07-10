
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC
{
	public interface IJXCExportHeader : IBusiness
	{
		ZString FreightDest { get; }
		JASOrgHeader SendingForwarder { get; }
		JASOrgHeader ReceivingForwarder { get; }
	}

	public interface IJXCImportHeader : IBusiness
	{
		void SetDestinationForwarder(ZString destOfficeCode, ZString destNettingCode);
		void SetSendingForwarder(ZString sendingOfficeCode, ZString sendingNettingCode);
		void SetFreightDestination(ZString freightDestination);
	}
}

#region IJXCExportHeader & IJXCImportHeader Members
#endregion
