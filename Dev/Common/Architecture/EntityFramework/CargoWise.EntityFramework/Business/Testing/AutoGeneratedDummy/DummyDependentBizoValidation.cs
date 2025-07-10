namespace CargoWise.EntityFramework.Testing
{
	public class DummyDependentBizoValidation : AutoDummyDependentBizoValidation
	{
		public DummyDependentBizoValidation(AutoDummyDependentBizo parent) : base(parent)
		{
		}

#if DEBUG

		protected override void CheckZD1_Code()
		{
			base.CheckZD1_Code();

			if (Parent.ZD1_Code == "error")
			{
				Parent.ZD1_CodeInfo.AddError("Error");
			}
		}

		protected override void CheckZD1_Number()
		{
			base.CheckZD1_Number();

			if (Parent.ZD1_Number == DummyDependantBusinessObject.ZD1_Number_WhenInError)
			{
				Parent.ZD1_NumberInfo.AddError("Error");
			}
		}

#endif
	}
}
