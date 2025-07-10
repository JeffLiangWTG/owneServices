using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IAgencyRegistry
	{
		bool PostBothPrepaidAndCollectShipmentRevenueCharges { get; }
		bool PostBothPrepaidAndCollectShipmentCostCharges { get; }
		bool ElectronicBookingAndShippingInstructions { get; }
		bool IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid principal);
		IRegistryItem UpdateEmptyReturnByWhenAvailabilityDatesChange { get; }
		ICodeDescriptionPairList ContainerCleanCodes { get; }
		ICodeDescriptionPairList ContainerDamageCodes { get; }
	}
}
