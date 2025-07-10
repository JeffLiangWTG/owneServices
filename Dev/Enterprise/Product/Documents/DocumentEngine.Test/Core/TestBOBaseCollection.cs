using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TestBOBaseCollection : NonPersistentBusinessObjectCollection<BOBase>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BOBase();
		}
	}
}
