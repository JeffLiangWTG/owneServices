using NUnit.Framework;
using static Enterprise.DocumentEngine.DataProviders.Testing.BusinessObjectReflectorTest;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	sealed class DocDataReflectorFilterTest : TestCase
	{
		public void TestIsAllowed_DocumentMacroIgnoreAttribute()
		{
			var passwordSalt = typeof(DummyPasswordStoredBODocSupportable).GetProperty("Dummy_PasswordSalt");
			var passwordHash = typeof(DummyPasswordStoredBODocSupportable).GetProperty("Dummy_PasswordHash");
			var filter = new DocDataReflectorFilter();

			AssertEquals("Dummy_PasswordSalt with DocumentMacroIgnoreAttribute should be ignored", false, filter.IsAllowed(passwordSalt));
			AssertEquals("Dummy_PasswordHash with DocumentMacroIgnoreAttribute should be ignored", false, filter.IsAllowed(passwordHash));
		}
	}
}
