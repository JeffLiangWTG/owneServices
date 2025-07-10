using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface ICancelNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		INCTSCommonTransitOperationMRN TransitOperation { get; }
		ICancelNCTSInvalidation Invalidation { get; }
		ZString CustomsOfficeOfDeparture { get; }
		INCTSCommonHolderOfTheTransitProcedure HolderOfTheTransitProcedure { get; }
	}

	public interface ICancelNCTSInvalidation
	{
		ZBool IsInitiatedByCustoms { get; }
		ZString Justification { get; }
	}
}
