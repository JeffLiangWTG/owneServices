using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ContentInformationTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var contentInformationType = Factory.CreateContentInformationType();
			var codeList = contentInformationType.Lookups.CY_CodeList;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(codeList.GetAllCodes(), Is.EquivalentTo(new[] { "01", "02", "04" }), "Collection correct");
				NUnit.Framework.Assert.That(Factory.GetCachedValue<ContentInfoTypeList>(), Is.SameAs(codeList), "Should be the cached");
			});
		}
	}
}
