using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceRulePeriodSnapshot))]
	sealed class CMRPreferenceRulePeriodSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRPreferenceRulePeriodSnapshot.New(Factory);
	}
}
