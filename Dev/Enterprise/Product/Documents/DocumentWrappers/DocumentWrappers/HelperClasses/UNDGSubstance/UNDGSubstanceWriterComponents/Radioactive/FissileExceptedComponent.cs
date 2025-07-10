using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class FissileExceptedComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var isFissileExcepted = wrapper?.DGData?.DI_IsFissileExcepted ?? false;
			if (isFissileExcepted)
			{
				return (NoResString)"Fissile Excepted";
			}

			return ZString.Empty;
		}
	}
}
