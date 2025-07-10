namespace CargoWise.EntityFramework.Testing
{
	internal class DummyDomainWithPiggybackedValidation : AutoDummyBizoValidation
	{
		public DummyDomainWithPiggybackedValidation(DummyWithValidation parent)
			: base(parent)
		{
			Add(new DummyDomainValidation(parent));
		}

		new DummyWithValidation Parent
		{
			get { return (DummyWithValidation)base.Parent; }
		}

		protected override void CheckZ0_Decimal()
		{
			Parent.CheckZ0_DecimalDomainWithPiggybackedFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			Parent.CheckZ0_DecimalIsValidZDecimalDomainWithPiggybackedFired = true;
		}

		internal void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoDomainWithPiggybackedFired = true;
		}
	}
}
