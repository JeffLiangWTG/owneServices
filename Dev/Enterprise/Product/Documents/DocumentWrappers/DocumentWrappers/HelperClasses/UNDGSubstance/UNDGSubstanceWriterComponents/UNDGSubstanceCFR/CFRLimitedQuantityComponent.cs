using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class CFRLimitedQuantityComponent : IUNDGSummaryWriterComponent
	{
		internal CFRLimitedQuantityComponent(bool isDefault = true)
		{
			IsDefault = isDefault;
		}

		public bool IsDefault { get; }

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var substance = wrapper?.DGData?.Substance;
			var isLimitedQuantity = wrapper?.IsLimitedQuantity ?? false;

			if (substance.DG_UNNO == "8000")
			{
				return (NoResString)"Limited Quantity";
			}
			if (isLimitedQuantity && IsRadioactiveWithClassOtherThanClass7(wrapper))
			{
				return (NoResString)"Limited quantity radioactive material";
			}
			else if (isLimitedQuantity)
			{
				return (NoResString)"LTD QTY";
			}

			return ZString.Empty;
		}

		bool IsRadioactiveWithClassOtherThanClass7(UNDGSubstanceWrapper wrapper)
		{
			if (wrapper?.DGData?.Substance?.StandardSubstance is UNDGSubstanceCFR cfrSubstance)
			{
				if (cfrSubstance == null)
				{
					return false;
				}
				return !cfrSubstance.CFR_PrimaryClass.Contains("7") && (cfrSubstance.CFR_SecondaryClass.Contains("7") || cfrSubstance.CFR_TertiaryClass.Contains("7"));
			}
			return false;
		}
	}
}
