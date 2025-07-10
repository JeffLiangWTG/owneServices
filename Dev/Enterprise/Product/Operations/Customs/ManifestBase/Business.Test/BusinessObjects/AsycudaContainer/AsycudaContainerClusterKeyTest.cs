using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var manifestHeader = (AsycudaManifestHeader)NewParentObject();
			var container = manifestHeader.Containers.AddNew();

			return container;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var bill = ClusterKeyEntityToTest.Header.Bills.AddNew();
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			link.APC_ABL_Bill = bill.PK;
			link.APC_ACN_Container = ClusterKeyEntityToTest.PK;

			return new IClusterKeyWorker[] { link };
		}

		new AsycudaContainer ClusterKeyEntityToTest => (AsycudaContainer)base.ClusterKeyEntityToTest;
	}
}
