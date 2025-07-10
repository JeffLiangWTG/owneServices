using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class PSNComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			return UNDGSubstanceWrapperHelper.GetPSNWithTechnicalNameOfSubstance(wrapper);
		}
	}
}
