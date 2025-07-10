using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IContainsSuppressedFields
	{
		ZBool SuppressFlightDetails { get; }
		ZString MasterBill_OrSuppressed { get; }
		ZString MasterBillAndIssueDate_OrSuppressed { get; }
		ZString TransportInfo_OrSuppressed { get; }
		ZString ETD_OrSuppressed { get; }
		ZString ATD_OrSuppressed { get; }
		ZString LoadingETD_OrSuppressed { get; }
		ZString LoadingATD_OrSuppressed { get; }
		ZString CarrierName_OrSuppressed { get; }
		ZString CarrierCCC_OrSuppressed { get; }
		ZString SuppressFlightDetailsFooter { get; }
	}
}
