using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public abstract class ADDCVDElementStrategy : CommonAdditionalElementStrategy
	{
		public override bool IsMandatory => false;
		public override bool IsMergeKey => true;
		public override bool IsApplicable(EnteringOrExiting isEnteringOrExiting) => isEnteringOrExiting == EnteringOrExiting.Entering || isEnteringOrExiting == EnteringOrExiting.Both;

		public void AddMessageErrorIfNotNumberOrNotGreaterThanZero(ZString elementValue, ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			if (!ZDecimal.TryParse(elementValue, out var rate) || rate < 0)
			{
				propertyInfo.AddNotification(Res.GetString("2192E7C7-7043-45E5-8764-5288DD8408BD", "The value should be a number and greater than 0."), provider);
			}
		}
	}
}
