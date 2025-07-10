using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class ExclusiveUseComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper?.DGData?.DI_IsExclusiveUse ?? false)
			{
				return (NoResString)"Exclusive Use";
			}

			return ZString.Empty;
		}
	}
}
