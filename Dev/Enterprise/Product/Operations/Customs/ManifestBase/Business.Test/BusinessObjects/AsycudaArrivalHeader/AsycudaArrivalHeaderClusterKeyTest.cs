using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaArrivalHeader))]
	sealed class AsycudaArrivalHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var manifestHeader = (AsycudaManifestHeader)NewParentObject();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;

			return arrivalHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var transferHeader = ClusterKeyEntityToTest.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = "D";
			var arrivalLine = Factory.New<AsycudaArrivalLine>();
			arrivalLine.ATL_ATH = ClusterKeyEntityToTest.PK;

			return new IClusterKeyWorker[] { transferHeader, arrivalLine };
		}

		new AsycudaArrivalHeader ClusterKeyEntityToTest => (AsycudaArrivalHeader)base.ClusterKeyEntityToTest;
	}
}
