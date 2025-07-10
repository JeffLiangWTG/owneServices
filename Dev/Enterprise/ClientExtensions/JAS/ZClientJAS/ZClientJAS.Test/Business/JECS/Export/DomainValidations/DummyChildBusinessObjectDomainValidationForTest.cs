using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	public class DummyChildBusinessObjectDomainValidationForTest : AutoDummyBizoValidation
	{
		public DummyChildBusinessObjectDomainValidationForTest(DummyChildBusinessObject parent) : base(parent)
		{
		}

		protected override void CheckZ0_AnotherDecimal()
		{
			if (Parent.Z0_AnotherDecimal.IsEmpty)
			{
				Parent.Z0_AnotherDecimalInfo.AddError("HAHA");
			}
		}
	}
}
