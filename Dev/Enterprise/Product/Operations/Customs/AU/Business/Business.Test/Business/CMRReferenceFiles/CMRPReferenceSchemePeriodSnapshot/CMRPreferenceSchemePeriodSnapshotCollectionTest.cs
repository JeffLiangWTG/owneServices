using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceSchemePeriodSnapshotCollection))]
	sealed class CMRPreferenceSchemePeriodSnapshotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRPreferenceSchemePeriodSnapshotCollection(Factory, null);
	}
}
