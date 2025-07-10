using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IComplianceWiseShipment
	{
		bool IsHighVolumeLowValue { get; }
		bool IsHighVolumeLowValueMaster { get; }
		ZBool IsBooking { get; }
		ZBool IsForwardRegistered { get; }
		ZGuid OneTimeQuotePK { get; }
	}
}
