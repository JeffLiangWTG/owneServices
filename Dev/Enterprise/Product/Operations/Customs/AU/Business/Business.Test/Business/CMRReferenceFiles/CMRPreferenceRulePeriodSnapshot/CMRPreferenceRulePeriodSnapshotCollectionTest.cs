using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceRulePeriodSnapshotCollection))]
	sealed class CMRPreferenceRulePeriodSnapshotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRPreferenceRulePeriodSnapshotCollection(Factory, null);
	}
}
