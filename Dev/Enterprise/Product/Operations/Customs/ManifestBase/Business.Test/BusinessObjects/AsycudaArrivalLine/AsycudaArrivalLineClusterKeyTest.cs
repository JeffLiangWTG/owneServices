using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaArrivalLine))]
	sealed class AsycudaArrivalLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var arrivalHeader = (AsycudaArrivalHeader)NewParentObject();
			var arrivalLine = Factory.New<AsycudaArrivalLine>();
			arrivalLine.ATL_ATH = arrivalHeader.PK;

			return arrivalLine;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;

			return arrivalHeader;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;
	}
}
