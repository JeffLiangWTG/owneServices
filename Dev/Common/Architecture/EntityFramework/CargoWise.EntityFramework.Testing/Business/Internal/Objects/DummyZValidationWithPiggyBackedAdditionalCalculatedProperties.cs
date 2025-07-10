namespace CargoWise.EntityFramework.Testing
{
	internal class DummyZValidationWithPiggyBackedAdditionalCalculatedProperties : AutoDummyBizoValidation
	{
		public DummyZValidationWithPiggyBackedAdditionalCalculatedProperties(DummyWithPiggyBackedAdditionalCalculatedPropertiesValidation parent)
			: base(parent)
		{
		}

		new DummyWithPiggyBackedAdditionalCalculatedPropertiesValidation Parent
		{
			get { return (DummyWithPiggyBackedAdditionalCalculatedPropertiesValidation)base.Parent; }
		}

		#region Z0_CalculatedPropertyWithPiggyBackedValidation

		public void ValidateZ0_CalculatedPropertyWithPiggyBackedValidation()
		{
			ValidateCalculatedProperty(Parent.Z0_CalculatedPropertyWithPiggyBackedValidationInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test code")]
		void CheckZ0_CalculatedPropertyWithPiggyBackedValidation()
		{
			if (Parent.Z0_CalculatedPropertyWithPiggyBackedValidation == "error")
			{
				Parent.Z0_CalculatedPropertyWithPiggyBackedValidationInfo.AddError("error");
			}
		}

		#endregion
	}
}
