using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrumentCategoryPreferenceScheme))]
	sealed class CMRInstrumentCategoryPreferenceSchemeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRInstrumentCategoryPreferenceScheme.New(Factory);
	}
}
