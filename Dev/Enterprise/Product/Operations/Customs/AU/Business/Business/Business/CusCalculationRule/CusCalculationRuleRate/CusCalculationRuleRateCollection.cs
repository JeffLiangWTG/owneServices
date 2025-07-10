using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusCalculationRuleRateCollection : NonPersistentBusinessObjectCollection<CusCalculationRuleRate>
	{
		public CusCalculationRuleRateCollection(BusinessObjectFactory factory, CusCalculationRule parent)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
			MaxCountValidationEnable(50);
		}

		public CusCalculationRule Parent { get; }

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
