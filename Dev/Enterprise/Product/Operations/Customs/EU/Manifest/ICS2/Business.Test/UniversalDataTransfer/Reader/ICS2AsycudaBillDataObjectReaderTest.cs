using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2AsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportTransportDocumentType()
		{
			var testHelper = new AsycudaManifestDataObjectReaderTestHelper();
			var billData = testHelper.SetupBill("BILL000001", null, null, 10m, "Goods Descrption", 1m, "Carrier Reference", "STD", "PPD");
			_ = billData.SetAddInfoCollection(() => [AddInfo.New(AsycudaBill.Schema.TransportDocumentType, TransportDocumentTypes.Codes.CL754_N714)]);

			var header = Factory.New<AsycudaManifestHeader>();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Latvia, Factory.BOFactory);
			var billBO = (AsycudaBill)new ICS2AsycudaBillDataObjectReader(billData, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertEquals("TransportDocumentType should be 'N714' after importing.", TransportDocumentTypes.Codes.CL754_N714, billBO.TransportDocumentType);
		}
	}
}
