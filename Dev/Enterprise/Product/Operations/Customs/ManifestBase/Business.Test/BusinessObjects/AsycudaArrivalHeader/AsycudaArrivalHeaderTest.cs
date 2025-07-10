using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaArrivalHeader))]
	class AsycudaArrivalHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeaderAndClusterKey()
		{
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			AssertNull(arrivalHeader.Header);
			AssertEquals(0, arrivalHeader.ATH_ClusterKey);

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			AssertEquals(manifestHeader.PK, arrivalHeader.Header.PK);
			AssertEquals(0, arrivalHeader.ATH_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated OnSaving for manifestHeader.", 1, manifestHeader.AMA_ClusterKey);
			AssertEquals("ClusterKey generated OnSaving and same as manifestHeader.", 1, arrivalHeader.ATH_ClusterKey);
		}

		public void TestCreateNewAsycudaTransferHeaderCollection()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(AsycudaTransferHeaderCollection<AsycudaTransferHeader>), header.TransferHeaders.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "X";
			header.AMA_ClusterKey = 1;
			header.AMA_RN_NKCountry = "SB";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";

			var arrivalHeader = factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = header.PK;
			arrivalHeader.ATH_ETAAtDischargePort = DateTime.Today;

			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_ATH_ArrivalHeader = arrivalHeader.PK;
			transferHeader.ATF_TransferType = TransferTypeList.Codes.Domestic;

			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = Core.Constants.ShipmentTypes.StandardHouse;

			return arrivalHeader;
		}
	}
}
