using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceSchemePeriodSnapshot))]
	sealed class CMRPreferenceSchemePeriodSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRPreferenceSchemePeriodSnapshot.New(Factory);
	}
}
