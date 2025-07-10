namespace CargoWise.EntityFramework.Testing
{
	internal class DummyZValidationWithMultiLevelPiggyBacking : DummyZValidation
	{
		public DummyZValidationWithMultiLevelPiggyBacking(DummyWithValidation parent)
			: base(parent)
		{
			Add(new DummyZValidationForPiggyBacking(parent));
		}

		new DummyWithValidation Parent
		{
			get { return (DummyWithValidation)base.Parent; }
		}

		protected override void CheckZ0_Decimal()
		{
			Parent.CheckZ0_DecimalMultiLevelPiggyBackedFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			Parent.CheckZ0_DecimalIsValidZDecimalMultiLevelPiggyBackedFired = true;
		}

		protected override void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoMultiLevelPiggybackedFired = true;
		}
	}
}
