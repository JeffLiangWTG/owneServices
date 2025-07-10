using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class IATAPSNComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var properShippingName = wrapper?.DGData?.TryGetPSNWithAdditionalTextIfSupportForNOS() ?? ZString.Empty;

			if (!properShippingName.IsEmpty
				&& !wrapper.TechnicalName.IsEmpty)
			{
				if (!wrapper.TechnicalName.IsEmpty)
				{
					return properShippingName + " (" + wrapper.TechnicalName + ")";
				}
			}

			return properShippingName;
		}
	}
}
