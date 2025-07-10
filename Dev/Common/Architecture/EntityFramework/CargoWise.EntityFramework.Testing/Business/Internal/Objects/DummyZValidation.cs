namespace CargoWise.EntityFramework.Testing
{
	public class DummyZValidation : DummyBizoValidation
	{
		public DummyZValidation(DummyWithValidation parent)
			: base(parent)
		{
		}

		new DummyWithValidation Parent
		{
			get { return (DummyWithValidation)base.Parent; }
		}

		#region Z0_Decimal

		protected override void CheckZ0_Decimal()
		{
			base.CheckZ0_Decimal();
			Parent.CheckZ0_DecimalFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			base.CheckZ0_DecimalIsValidZDecimal();
			Parent.CheckZ0_DecimalIsValidZDecimalFired = true;
		}

		#endregion

		#region Z0_CalculatedProperty

		public void ValidateZ0_CalculatedProperty()
		{
			ValidateCalculatedProperty(Parent.Z0_CalculatedPropertyInfoInfo);
		}

		protected virtual void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoFired = true;
		}

		#endregion

		#region Z0_AnotherCalculatedProperty

		public void ValidateZ0_AnotherCalculatedProperty()
		{
			ValidateCalculatedProperty(Parent.Z0_AnotherCalculatedPropertyInfo);
		}

		protected virtual void CheckZ0_AnotherCalculatedProperty()
		{
			Parent.CheckZ0_AnotherCalculatedPropertyFired = true;
		}

		#endregion
	}
}
