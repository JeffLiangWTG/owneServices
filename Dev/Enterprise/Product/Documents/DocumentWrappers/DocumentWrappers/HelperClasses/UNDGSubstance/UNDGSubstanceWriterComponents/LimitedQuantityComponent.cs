using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class LimitedQuantityComponent : IUNDGSummaryWriterComponent
	{
		internal LimitedQuantityComponent(bool isDefault = true)
		{
			IsDefault = isDefault;
		}

		public bool IsDefault { get; }

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper?.IsLimitedQuantity ?? false)
			{
				return (NoResString)"LTD QTY";
			}

			return ZString.Empty;
		}
	}
}
