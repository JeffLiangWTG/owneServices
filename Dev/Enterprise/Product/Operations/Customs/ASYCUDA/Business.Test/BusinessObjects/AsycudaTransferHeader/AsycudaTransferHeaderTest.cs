using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaTransferHeader))]
	sealed class AsycudaTransferHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateNewAsycudaTransferBillCollection()
		{
			var header = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>), header.TransferBills.GetType());
			AssertEquals(typeof(ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>), ((ManifestBase.AsycudaTransferHeader)header).TransferBills.GetType());
		}

		public void TestGetNewLookups()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(AsycudaTransferHeaderLookups), transferHeader.Lookups.GetType());
			AssertEquals(typeof(AsycudaTransferHeaderLookups), ((ManifestBase.AsycudaTransferHeader)transferHeader).Lookups.GetType());
		}

		public void TestGetNewValidation()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(AsycudaTransferHeaderValidation), transferHeader.Validation.GetType());
			AssertEquals(typeof(AsycudaTransferHeaderValidation), ((ManifestBase.AsycudaTransferHeader)transferHeader).Validation.GetType());
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
			return transferHeader;
		}
	}
}
