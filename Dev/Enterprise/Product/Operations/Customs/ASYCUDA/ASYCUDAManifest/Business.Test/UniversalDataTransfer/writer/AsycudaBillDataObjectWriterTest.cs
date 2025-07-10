using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaBillDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestBDExportBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
			header.AMA_Nature = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BN001";
			bill.SADOfficeCode = "100";
			bill.SADRegistrationNumber = "12345";
			bill.SADRegistrationSerial = "C";
			bill.SADRegistrationDate = new ZDateTime(2023, 5, 29);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertExportFields("EGM only show for Export and Bangladesh", header, bill, true);

				header.AMA_Nature = "IMP";
				AssertExportFields("EGM only show for Export", header, bill, false);

				header.AMA_Nature = "EXP";
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Namibia;
				AssertExportFields("EGM only show for Bangladesh", header, bill, false);
			});
		}

		void AssertExportFields(string message, AsycudaManifestHeader header, AsycudaBill bill, bool hasEGMData)
		{
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);
			var writer = new AsycudaBillDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var billData = writer.GetDataObject(bill);

			AssertEquals("BN001", billData.WayBillNumber);

			var entryNumberCollection = billData.EntryNumberCollection;
			if (hasEGMData)
			{
				var entryNumber = entryNumberCollection.FirstOrDefault();
				AssertEquals("SADOfficeCode", "100", entryNumber.EntryLineReference);
				AssertEquals("SADRegistrationSerial", "C", entryNumber.Category);
				AssertEquals("SADRegistrationNumber", "12345", entryNumber.Number);
				AssertEquals("SADRegistrationDate", new ZDateTime(2023, 5, 29), entryNumber.IssueDate);
			}
			else
			{
				AssertEquals("No SADRegistrationDate", null, entryNumberCollection);
			}
		}
	}
}
