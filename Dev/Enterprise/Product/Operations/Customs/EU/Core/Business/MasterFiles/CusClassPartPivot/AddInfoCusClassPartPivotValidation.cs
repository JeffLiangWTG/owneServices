using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.MasterFiles;

public class AddInfoCusClassPartPivotValidation : EUAddInfoValidation
{
	public AddInfoCusClassPartPivotValidation(AddInfoCusClassPartPivot addInfo)
		: base(addInfo)
	{
	}

	public new AddInfoCusClassPartPivot Parent
	{
		get { return (AddInfoCusClassPartPivot)base.Parent; }
	}

	protected override void CheckZG_GoodsCategory()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.Parent.CI_GoodsCategoryInfo);
	}
}
