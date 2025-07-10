using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrumentPreferenceScheme))]
	sealed class CMRInstrumentPreferenceSchemeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRInstrumentPreferenceScheme.New(Factory);
	}
}
