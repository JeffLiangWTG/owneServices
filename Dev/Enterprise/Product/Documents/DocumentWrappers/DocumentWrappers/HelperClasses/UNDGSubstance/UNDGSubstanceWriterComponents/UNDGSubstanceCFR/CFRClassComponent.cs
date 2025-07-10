using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class CFRClassComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var unnoWithVariant = wrapper
				.UNNumberWithVariant
				.ToUpperInvariant();

			if (unnoWithVariant == CombustibleLiquidCode)
			{
				return ZString.Empty;
			}

			var hazardClass = wrapper?.IMOClass ?? ZString.Empty;
			if (hazardClass.IsEmpty)
			{
				return ZString.Empty;
			}

			return $"class {wrapper?.IMOClass}";
		}

		const string CombustibleLiquidCode = "1993D";
	}
}
