using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class CFRResidueLastContainedComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper) =>
			wrapper?.DGData?.DI_IsResidueLastContained ?? false
				? (NoResString)"RESIDUE: Last Contained * * *"
				: string.Empty;
	}
}


