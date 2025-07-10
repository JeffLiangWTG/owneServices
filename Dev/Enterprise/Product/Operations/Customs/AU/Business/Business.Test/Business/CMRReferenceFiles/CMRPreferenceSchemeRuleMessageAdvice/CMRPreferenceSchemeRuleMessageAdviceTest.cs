using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceSchemeRuleMessageAdvice))]
	sealed class CMRPreferenceSchemeRuleMessageAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRPreferenceSchemeRuleMessageAdvice.New(Factory);
	}
}
