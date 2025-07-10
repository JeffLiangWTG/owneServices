using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ProductConditionValidation : Customs.Business.CusCodeDataValidation
	{
		public ProductConditionValidation(ProductCondition parent)
			: base(parent)
		{
		}

		new ProductCondition Parent => (ProductCondition)base.Parent;

		protected override void CheckCY_Code()
		{
			base.CheckCY_Data();

			if (!Parent.CY_Code.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);

				var quarantineExDocLine = Parent.Parent;
				var quarantineExDocHeader = quarantineExDocLine?.QuarantineExDocHeader;
				if (quarantineExDocHeader != null)
				{
					if (!IsProduceTypeValidForProductConditions(quarantineExDocHeader.QH_ProduceType))
					{
						Parent.CY_CodeInfo.AddMessageError(ResString.GetMultilingualString("692A6804-F37E-4811-B720-200065886020", ProductConditionNotRequired));
					}
					else
					{
						foreach (var productCondition in quarantineExDocLine.ProductConditions)
						{
							if (productCondition != Parent && productCondition.CY_Code == Parent.CY_Code)
							{
								Parent.CY_CodeInfo.AddError(ResString.GetMultilingualString("AB96046F-7818-4881-ABA2-BA82C0F4E657", ProductConditionAlreadyEntered));
								break;
							}
						}
					}
				}
			}
		}

		const string ProductConditionAlreadyEntered = "This Product Condition has already been entered.";
		const string ProductConditionNotRequired = "Product Conditions may only be present when Produce Type is Horticulture or Grains and Plants.";

		bool IsProduceTypeValidForProductConditions(string produceType)
		{
			return produceType == EXDOCCommodityCodes.Codes.Horticulture || produceType == EXDOCCommodityCodes.Codes.GrainsAndPlants;
		}
	}
}
