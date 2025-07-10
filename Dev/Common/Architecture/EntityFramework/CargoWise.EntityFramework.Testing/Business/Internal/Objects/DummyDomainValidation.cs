namespace CargoWise.EntityFramework.Testing
{
	public class DummyDomainValidation : AutoDummyBizoValidation
	{
		public DummyDomainValidation(DummyWithValidation parent)
			: base(parent)
		{
		}

		new DummyWithValidation Parent
		{
			get { return (DummyWithValidation)base.Parent; }
		}

		protected override void CheckZ0_Decimal()
		{
			Parent.CheckZ0_DecimalDomainFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			Parent.CheckZ0_DecimalIsValidZDecimalDomainFired = true;
		}

		internal void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoDomainFired = true;
		}
	}
}
