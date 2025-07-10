using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentSnapshotTariffGroup))]
	sealed class CMRTreatmentSnapshotTariffGroupTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentSnapshotTariffGroup.New(Factory);
	}
}
