using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class UNNOComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper == null)
			{
				return ZString.Empty;
			}

			return (wrapper.DGData?.GetUnnoPrefix() ?? ZString.Empty)
				+ (wrapper.UNNumber);
		}
	}
}
