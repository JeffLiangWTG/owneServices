using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();

			bill.ABL_ShipmentType = "IMP";
			previousDocument.CSI_Code = "AAD";
			AssertHasMessageError(previousDocument.CSI_CodeInfo, "This code is only applicable to exports.");

			bill.ABL_ShipmentType = "EXP";
			previousDocument.CSI_Code = "AAD";
			AssertNoMessageError(previousDocument.CSI_CodeInfo, "This code is only applicable to exports.");
		}
	}
}
