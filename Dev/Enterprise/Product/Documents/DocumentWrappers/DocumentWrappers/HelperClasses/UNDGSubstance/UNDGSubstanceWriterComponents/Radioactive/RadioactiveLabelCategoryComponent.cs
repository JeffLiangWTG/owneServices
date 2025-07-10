using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;

namespace Enterprise.DocumentWrappers
{
	class RadioactiveLabelCategoryComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var radioactiveLabelCategory = wrapper.DGData?.DI_RadioactiveLabelCategory ?? ZString.Empty;

			if (!radioactiveLabelCategory.IsEmpty)
			{
				var description = new RadioactiveLabelCategoryList()
					.GetDescriptionFromCode(radioactiveLabelCategory);

				return $"RADIOACTIVE {description?.ToUpper()} LABEL";
			}

			return ZString.Empty;
		}
	}
}
