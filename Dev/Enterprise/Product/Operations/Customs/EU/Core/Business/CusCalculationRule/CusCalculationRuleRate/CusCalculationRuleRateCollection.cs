using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusCalculationRuleRateCollection : NonPersistentBusinessObjectCollection<CusCalculationRuleRate>
	{
		public CusCalculationRuleRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			MaxCountValidationEnable(50);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CusCalculationRuleRate(Factory);

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			((CusCalculationRuleRate)child).CusCalculationRuleRateCollection = this;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var rate = (CusCalculationRuleRate)bizOAdded;
			rate.Validation.ValidateFlatRate();
			rate.Validation.ValidateUplift();
		}
	}
}
