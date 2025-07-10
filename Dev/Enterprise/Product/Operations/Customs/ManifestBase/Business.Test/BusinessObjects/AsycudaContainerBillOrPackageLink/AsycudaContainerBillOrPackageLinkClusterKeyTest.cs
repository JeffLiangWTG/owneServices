using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaContainerBillOrPackageLinkClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var container = (AsycudaContainer)NewParentObject();
			var bill = container.Header.Bills.AddNew();
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			link.APC_ACN_Container = container.PK;
			link.APC_ABL_Bill = bill.PK;

			return link;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = manifestHeader.Containers.AddNew();

			return container;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;
	}
}
