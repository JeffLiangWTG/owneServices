using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	public class DummyBusinessObjectDomainValidationForTest : AutoDummyBizoValidation
	{
		public DummyBusinessObjectDomainValidationForTest(DummyBusinessObject parent) : base(parent)
		{
		}

		protected override void CheckZ0_AnotherDate()
		{
			new ValidationHelper().AddJXCWarning(Parent.Z0_AnotherDateInfo, "MEH MEH");
		}
	}
}
