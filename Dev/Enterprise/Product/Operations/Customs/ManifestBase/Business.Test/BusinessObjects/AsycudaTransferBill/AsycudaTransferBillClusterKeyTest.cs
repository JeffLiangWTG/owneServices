using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferBill))]
	sealed class AsycudaTransferBillClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var transferHeader = (AsycudaTransferHeader)NewParentObject();
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "BOL";

			return transferBill;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = "D";

			return transferHeader;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;
	}
}
