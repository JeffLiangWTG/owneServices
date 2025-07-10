using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferHeader))]
	public class AsycudaTransferHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKey()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			var transferHeader = Factory.NewWithValidTestData<AsycudaTransferHeader>();
			transferHeader.ATF_ATH_ArrivalHeader = arrivalHeader.PK;
			transferHeader.ATF_TransferType = "D";
			AssertEquals(0, transferHeader.ATF_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, transferHeader.ATF_ClusterKey);
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

			return transferHeader;
		}
	}
}
