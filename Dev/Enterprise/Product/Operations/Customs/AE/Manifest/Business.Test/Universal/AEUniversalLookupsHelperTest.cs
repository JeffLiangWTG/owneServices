using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AEUniversalLookupsHelperTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestGetUN20CodeCustomsValue() => CombineAssertions(() =>
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupUnitCodeMappings();
		NUnit.Framework.Assert.That(AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Factory, "KG"), NUnit.Framework.Is.EqualTo("KGM").Using(CustomComparers.TypeComparison), "Mass unit mapping");
		NUnit.Framework.Assert.That(AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Factory, "L"), NUnit.Framework.Is.EqualTo("LTR").Using(CustomComparers.TypeComparison), "Volume unit mapping");
		NUnit.Framework.Assert.That(AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Factory, "X").ToString(), NUnit.Framework.Is.Null.Or.Empty, "Unit without mapping - should be [null] or [empty]");
	});
}
