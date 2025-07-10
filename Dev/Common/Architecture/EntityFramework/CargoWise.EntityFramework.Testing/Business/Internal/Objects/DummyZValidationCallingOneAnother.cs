namespace CargoWise.EntityFramework.Testing
{
	internal class DummyZValidationCallingOneAnother : DummyZValidation
	{
		public DummyZValidationCallingOneAnother(DummyWithValidation parent)
			: base(parent)
		{
		}

		protected override void CheckZ0_VarCharMax()
		{
			base.CheckZ0_VarCharMax();
			ValidateZ0_Decimal();
		}

		protected override void CheckZ0_Decimal()
		{
			base.CheckZ0_Decimal();
			Parent.Z0_DecimalInfo.AddWarning("harbl");
		}
	}
}
