using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class RadioactiveTransportIndexComponent : IUNDGSummaryWriterComponent
	{
		public bool IsDefault => true;

		public ZString Write(UNDGSubstanceWrapper wrapper)
		{
			var transportIndex = wrapper.DGData?.DI_RadioactiveTransportIndex ?? ZDecimal.Zero;
			if (!transportIndex.IsEmpty)
			{
				return $"TI = {transportIndex}";
			}

			return ZString.Empty;
		}
	}
}
