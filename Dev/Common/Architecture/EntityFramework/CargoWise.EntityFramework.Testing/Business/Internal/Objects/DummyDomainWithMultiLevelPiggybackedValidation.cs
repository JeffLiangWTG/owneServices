namespace CargoWise.EntityFramework.Testing
{
	public class DummyDomainWithMultiLevelPiggybackedValidation : AutoDummyBizoValidation
	{
		public DummyDomainWithMultiLevelPiggybackedValidation(DummyWithValidation parent)
			: base(parent)
		{
			Add(new DummyDomainWithPiggybackedValidation(parent));
		}

		new DummyWithValidation Parent
		{
			get { return (DummyWithValidation)base.Parent; }
		}

		protected override void CheckZ0_Decimal()
		{
			Parent.CheckZ0_DecimalDomainWithMultiLevelPiggybackedFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			Parent.CheckZ0_DecimalIsValidZDecimalDomainWithMultiLevelPiggybackedFired = true;
		}

		internal void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoDomainWithMultiLevelPiggybackedFired = true;
		}
	}
}
