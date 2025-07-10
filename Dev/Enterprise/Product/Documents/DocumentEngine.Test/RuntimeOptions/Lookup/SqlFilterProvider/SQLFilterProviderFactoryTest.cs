using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Exceptions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SQLFilterProviderFactoryTest : TestCaseWithFactory
	{
		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestCreateSQLFilterProviderWithInvalidRelationType()
		{
			SQLFilterProvider provider = SQLFilterProviderFactory.CreateSQLFilterProvider(null, null, "blah");
		}

		public void TestCreateSQLFilterProvider()
		{
			SQLFilterProvider provider = SQLFilterProviderFactory.CreateSQLFilterProvider(null, null, "SupplierPartOwner");
			AssertEquals(typeof(SupplierPartOwnerFilterProvider), provider.GetType());
			provider = SQLFilterProviderFactory.CreateSQLFilterProvider(null, null, "whsareaowner");
			AssertEquals(typeof(WhsAreaFilterProvider), provider.GetType());
			provider = SQLFilterProviderFactory.CreateSQLFilterProvider(null, null, "gllocalaccountcountry");
			AssertEquals(typeof(GLLocalAccCountryFilterProvider), provider.GetType());
		}
	}
}
