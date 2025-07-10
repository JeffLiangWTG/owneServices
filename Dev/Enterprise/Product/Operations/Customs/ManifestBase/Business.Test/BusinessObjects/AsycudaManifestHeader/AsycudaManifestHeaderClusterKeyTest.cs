using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderClusterKeyTest : ClusterKeyMasterMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "0001";

			return header;
		}
	}
}
