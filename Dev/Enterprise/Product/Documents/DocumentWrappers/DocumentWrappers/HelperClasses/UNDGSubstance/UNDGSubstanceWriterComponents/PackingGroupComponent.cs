using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class PackingGroupComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (!wrapper?.PackingGroup.IsEmpty ?? false)
			{
				return $"PG {wrapper.PackingGroup}";
			}

			return ZString.Empty;
		}
	}
}
