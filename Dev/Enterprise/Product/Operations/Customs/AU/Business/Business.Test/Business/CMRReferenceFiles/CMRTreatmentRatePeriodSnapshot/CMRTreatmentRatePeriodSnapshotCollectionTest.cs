using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodSnapshotCollection))]
	sealed class CMRTreatmentRatePeriodSnapshotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRTreatmentRatePeriodSnapshotCollection(Factory, null);
	}
}
