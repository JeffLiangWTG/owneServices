using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaTransferBill))]
	sealed class AsycudaTransferBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewLookups()
		{
			var transferBill = Factory.New<AsycudaTransferBill>();
			AssertEquals(typeof(AsycudaTransferBillLookups), transferBill.Lookups.GetType());
			AssertEquals(typeof(AsycudaTransferBillLookups), ((ManifestBase.AsycudaTransferBill)transferBill).Lookups.GetType());
		}

		public void TestGetNewValidation()
		{
			var transferBill = Factory.New<AsycudaTransferBill>();
			AssertEquals(typeof(AsycudaTransferBillValidation), transferBill.Validation.GetType());
			AssertEquals(typeof(AsycudaTransferBillValidation), ((ManifestBase.AsycudaTransferBill)transferBill).Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";
			return transferBill;
		}
	}
}
