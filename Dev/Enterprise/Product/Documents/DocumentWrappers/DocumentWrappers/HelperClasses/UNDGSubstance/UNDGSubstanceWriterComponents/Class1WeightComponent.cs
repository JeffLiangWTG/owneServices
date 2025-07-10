using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class Class1WeightComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var dataItem = wrapper?.DGData;
			if (dataItem == null)
			{
				return ZString.Empty;
			}

			if ((dataItem?.Subs?.DG_Class.ToString() ?? ZString.Empty) == "1")
			{
				if (!wrapper.Weight.Value.IsEmpty)
				{
					return $"Net Weight: {wrapper.Weight.InKilograms.Value} KG";
				}
			}

			return ZString.Empty;
		}
	}
}
