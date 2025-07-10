using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyWithExchangeRateBusinessObjectValidation : DummyBizoValidation
	{
		public DummyWithExchangeRateBusinessObjectValidation(DummyBusinessObject parent) : base(parent)
		{
		}

		protected override void CheckZ0_AnotherDecimal()
		{
			base.CheckZ0_AnotherDecimal();

			if (Parent.Z0_AnotherDecimal > 51)
			{
				Parent.Z0_AnotherDecimalInfo.AddError("Error");
			}
		}
	}
}
