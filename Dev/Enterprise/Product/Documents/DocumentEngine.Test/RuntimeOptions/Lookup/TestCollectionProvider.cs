using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class TestCollectionProvider : CollectionProvider
	{
		public TestCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			throw new DocumentEngineException("");
		}
	}
}
