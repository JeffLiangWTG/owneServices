
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public enum ShipmentStatusType
	{
		Unknown,
		CargoReport,
		Commercial,
		Declaration,
		Special
	}

	public interface IShipmentStatusData : ILineKey
	{
		ZString ShipmentStatus { get; }
		ZString HoldReasonCode { get; }
		ZBool InspectIndicator { get; }
		ZBool AddressCorrectionIndicator { get; }
		ZDateTime ImportReleaseDate { get; }
		ZString CustomsRefNo { get; }
		ZString BrokerCode { get; }
		ZString Remarks { get; }
		ZString ExceptionStatusCode { get; }
		ZString ExceptionResolutionCode { get; }
		ShipmentStatusType ShipmentStatusType { get; }
		ZDateTime StatusChangeDate { get; }
		bool HasReasonOrResolutionCode { get; }
	}
}
