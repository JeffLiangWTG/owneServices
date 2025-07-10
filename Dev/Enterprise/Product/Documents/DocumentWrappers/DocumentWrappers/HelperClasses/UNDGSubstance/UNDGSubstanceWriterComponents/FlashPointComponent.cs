using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class FlashPointComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (!wrapper?.FlashPoint.IsEmpty ?? false)
			{
				return $"({wrapper.FlashPoint}C c.c.)";
			}

			return ZString.Empty;
		}
	}
}
