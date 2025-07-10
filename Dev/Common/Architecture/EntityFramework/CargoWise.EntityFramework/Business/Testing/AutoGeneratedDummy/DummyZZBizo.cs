using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyZZBizo : AutoZZDummyBizo
	{
		public DummyZZBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyZZBizoValidation(this);
		}

		protected override bool SupportsCloneCore() => true;
	}
}
