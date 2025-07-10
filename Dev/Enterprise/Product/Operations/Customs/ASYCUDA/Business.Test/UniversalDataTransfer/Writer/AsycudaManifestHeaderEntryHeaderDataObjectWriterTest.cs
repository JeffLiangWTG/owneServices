using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportHeaderToEntryHeader()
		{
			PrepareCusCodeDataForTesting();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			header.AMA_ManifestType = "MGI";
			header.RegistrationNumber = "REG001";
			header.RegistrationDate = new ZDateTime(2017, 1, 1);
			header.RegistrationStatus = "NOT";
			Factory.SaveForTesting();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);

			var writer = new AsycudaManifestHeaderEntryHeaderDataObjectWriter<AsycudaManifestHeader>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var manifestHeaderEntryHeaderData = writer.GetDataObject(header);

			AssertEquals(header.AMA_RN_NKCountry, manifestHeaderEntryHeaderData.Type.Code);
			AssertEquals(1, manifestHeaderEntryHeaderData.EntryNumberCollection.Count);
			AssertEquals(CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, manifestHeaderEntryHeaderData.EntryNumberCollection[0].Type.Code);
			AssertEquals("REG001", manifestHeaderEntryHeaderData.EntryNumberCollection[0].Number);
			AssertEquals(new ZDateTime(2017, 1, 1), manifestHeaderEntryHeaderData.EntryNumberCollection[0].IssueDate);
			AssertEquals("NOT", manifestHeaderEntryHeaderData.EntryNumberCollection[0].EntryStatus.Code);
		}
	}
}
