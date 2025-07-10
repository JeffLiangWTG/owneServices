using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbConnectionConstantsTest : TestCase
	{
		public void TestApplicationNamesListIsExhaustive()
		{
			var expectedList = typeof(DbConnectionConstants.ApplicationNames)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(string))
				.Select(f => (string)f.GetValue(null));
			AssertContainsExactElementsInAnyOrder("ApplicationNamesList should contain all constants defined in static class ApplicationNames", expectedList, DbConnectionConstants.ApplicationNamesList);
		}
	}
}
