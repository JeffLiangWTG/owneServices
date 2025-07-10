using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferBill))]
	public class AsycudaTransferBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKey()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			var transferHeader = Factory.NewWithValidTestData<AsycudaTransferHeader>();
			transferHeader.ATF_ATH_ArrivalHeader = arrivalHeader.PK;
			transferHeader.ATF_TransferType = "D";
			var transferBill = Factory.New<AsycudaTransferBill>();
			transferBill.ATB_ATF_TransferHeader = transferHeader.PK;
			transferBill.ATB_BillOfLadingType = "STD";
			AssertEquals(0, transferBill.ATB_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, transferBill.ATB_ClusterKey);
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
			var outtturnHeader = factory.New<AsycudaArrivalHeader>();
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			outtturnHeader.ATH_ETAAtDischargePort = DateTime.Today;

			var transferHeader = outtturnHeader.TransferHeaders.AddNew();
			transferHeader.ATF_ATH_ArrivalHeader = outtturnHeader.PK;
			transferHeader.ATF_TransferType = TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = Core.Constants.ShipmentTypes.StandardHouse;
			return transferBill;
		}
	}
}
