using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[HttpContextEnabledTest]
	class FilterBusinessObjectFactoryTest : TransactionedTestCase
	{
		public void TestInitialiseWebFilterBusinessObjectFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(factory);
			AssertEquals(factory, filterFactory.Factory);
		}

		protected virtual WebFilterBusinessObjectFactory GetNewFilterBusinessObjectFactory()
		{
			return new WebFilterBusinessObjectFactory(new BusinessObjectFactory());
		}
	}
}
