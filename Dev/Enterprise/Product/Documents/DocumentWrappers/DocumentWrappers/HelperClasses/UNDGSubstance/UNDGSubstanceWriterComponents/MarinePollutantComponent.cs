using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class MarinePollutantComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			return wrapper?.MarinePollutantWarning ?? ZString.Empty;
		}
	}
}
