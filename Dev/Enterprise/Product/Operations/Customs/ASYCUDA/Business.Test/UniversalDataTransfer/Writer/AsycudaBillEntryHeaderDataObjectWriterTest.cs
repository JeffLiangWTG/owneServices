using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportAsycudaBillToEntryHeader()
		{
			PrepareCusCodeDataForTesting();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "NOT";
			bill.ABL_SenderReference = "SR001";
			bill.CustomsEntryNumber = "EN001";
			bill.CustomsEntryNumberType = "ENT";

			Factory.SaveForTesting();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);

			var writer = new AsycudaBillEntryHeaderDataObjectWriter<AsycudaBill>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var billEntryHeaderData = writer.GetDataObject(bill);

			AssertEquals(header.AMA_RN_NKCountry, billEntryHeaderData.Type.Code);
			AssertEquals("NOT", billEntryHeaderData.EntryStatus.Code);
			AssertEquals(1, billEntryHeaderData.CustomsReferenceCollection.Count);
			AssertEquals(Constants.CustomsReferenceType.ABL_SenderReferenceType, billEntryHeaderData.CustomsReferenceCollection[0].Type.Code);
			AssertEquals("SR001", billEntryHeaderData.CustomsReferenceCollection[0].Reference);
			AssertEquals(1, billEntryHeaderData.EntryNumberCollection.Count);
			AssertEquals("ENT", billEntryHeaderData.EntryNumberCollection[0].Type.Code);
			AssertEquals("EN001", billEntryHeaderData.EntryNumberCollection[0].Number);
		}
	}
}
