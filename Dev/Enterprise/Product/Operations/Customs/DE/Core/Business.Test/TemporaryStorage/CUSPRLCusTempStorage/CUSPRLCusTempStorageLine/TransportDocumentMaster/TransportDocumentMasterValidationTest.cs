using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class TransportDocumentMasterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code() => ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(transportDocumentMaster.CSI_CodeInfo, transportDocumentMaster.CSI_ReferenceNumberInfo, expectedMessage: MandatoryValidation.YouHaveNotEnteredMessage("Transport Document Master Type"));

		public void TestCheckCSI_Reference() => ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(transportDocumentMaster.CSI_ReferenceNumberInfo, transportDocumentMaster.CSI_CodeInfo, expectedMessage: MandatoryValidation.YouHaveNotEnteredMessage("Transport Document Master Reference Number"));

		protected override void SetUp()
		{
			base.SetUp();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var cusprlDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var cusprlLine = cusprlDec.CusTempStorageLines.AddNew();
			transportDocumentMaster = cusprlLine.TransportDocumentMaster;
		}
		TransportDocumentMaster transportDocumentMaster;
	}
}
