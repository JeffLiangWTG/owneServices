using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocumentLookups))]
	sealed class SupportingDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestUnitOfQuantityListDataGroupingCode()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var lookups = supportingDocument.Lookups;
			AssertEquals("UnitOfQuantityListDataGroupingCode", "IE", lookups.GetType().GetProperty("UnitOfQuantityListDataGroupingCode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lookups));
		}
	}
}
