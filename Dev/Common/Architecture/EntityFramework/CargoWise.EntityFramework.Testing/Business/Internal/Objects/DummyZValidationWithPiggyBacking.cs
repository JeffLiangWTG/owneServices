namespace CargoWise.EntityFramework.Testing
{
	internal class DummyZValidationWithPiggyBacking : DummyZValidation
	{
		public DummyZValidationWithPiggyBacking(DummyWithValidation parent)
			: base(parent)
		{
			Add(new DummyZValidationForPiggyBacking(parent));
		}
	}
}
