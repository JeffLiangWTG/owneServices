using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class EMSComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;
		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			return wrapper.EMSCode;
		}
	}
}
