using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceSchemeRuleCollection))]
	sealed class CMRPreferenceSchemeRuleCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRPreferenceSchemeRuleCollection(Factory, null);
	}
}
