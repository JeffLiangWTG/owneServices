using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class HRCQComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var isHighwayRouteControlledQuantity = wrapper?.DGData?.DI_IsHighwayRouteControlledQuantity ?? false;
			if (isHighwayRouteControlledQuantity)
			{
				return "HRCQ";
			}

			return ZString.Empty;
		}
	}
}
