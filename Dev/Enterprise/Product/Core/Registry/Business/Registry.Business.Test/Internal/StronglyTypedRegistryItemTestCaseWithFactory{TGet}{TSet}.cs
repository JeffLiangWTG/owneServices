using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment.Testing;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class StronglyTypedRegistryItemTestCaseWithFactory<TGet, TSet> : StronglyTypedRegistryItemTestCase<TGet, TSet>
	{
		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}

		BusinessObjectFactory factory;
	}
}
