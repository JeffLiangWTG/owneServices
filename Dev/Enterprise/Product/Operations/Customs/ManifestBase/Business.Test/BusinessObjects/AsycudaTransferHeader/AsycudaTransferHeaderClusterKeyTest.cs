using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferHeader))]
	sealed class AsycudaTransferHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var arrivalHeader = (AsycudaArrivalHeader)NewParentObject();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = "D";

			return transferHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;

			return arrivalHeader;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var transferBill = ClusterKeyEntityToTest.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "BOL";

			return new IClusterKeyWorker[] { transferBill };
		}

		new AsycudaTransferHeader ClusterKeyEntityToTest => (AsycudaTransferHeader)base.ClusterKeyEntityToTest;
	}
}
