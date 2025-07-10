using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffRatePeriodSnapshotCollection))]
	sealed class CMRTariffRatePeriodSnapshotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRTariffRatePeriodSnapshotCollection(Factory, null);
	}
}
