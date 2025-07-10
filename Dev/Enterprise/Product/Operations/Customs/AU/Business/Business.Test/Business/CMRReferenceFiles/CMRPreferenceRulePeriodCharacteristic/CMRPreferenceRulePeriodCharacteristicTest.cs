using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceRulePeriodCharacteristic))]
	sealed class CMRPreferenceRulePeriodCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRPreferenceRulePeriodCharacteristic.New(Factory);
	}
}
