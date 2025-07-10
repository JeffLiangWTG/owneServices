using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class LQAbbreviatedComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper?.IsLimitedQuantity ?? false)
			{
				return "LQ";
			}

			return ZString.Empty;
		}
	}
}
