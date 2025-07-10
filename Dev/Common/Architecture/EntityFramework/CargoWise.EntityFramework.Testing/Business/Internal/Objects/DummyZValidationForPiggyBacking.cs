namespace CargoWise.EntityFramework.Testing
{
	internal class DummyZValidationForPiggyBacking : AutoDummyBizoValidation
	{
		public DummyZValidationForPiggyBacking(DummyWithValidation parent)
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
			if (Parent.Z0_Decimal < 0M)
			{
				Parent.Z0_DecimalInfo.AddError("Decimal cannot be negative!");
			}
			Parent.CheckZ0_DecimalPiggyBackedFired = true;
		}

		protected override void CheckZ0_DecimalIsValidZDecimal()
		{
			Parent.CheckZ0_DecimalIsValidZDecimalPiggyBackedFired = true;
		}

		#endregion

		internal void CheckZ0_CalculatedPropertyInfo()
		{
			Parent.CheckZ0_CalculatedPropertyInfoPiggybackedFired = true;
		}
	}
}
