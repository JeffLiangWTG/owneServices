using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class MaterialFormDescriptionComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var materialFormDescription = wrapper?.DGData?.DI_MaterialFormDescription ?? ZString.Empty;
			return materialFormDescription;
		}
	}
}
