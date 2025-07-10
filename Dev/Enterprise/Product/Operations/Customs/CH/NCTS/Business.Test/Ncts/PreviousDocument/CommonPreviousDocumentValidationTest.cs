using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class CommonPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP70254;

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();

		var previousDocument = bill.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "XXXX";
		AssertHasRowMessageError("Bad document type", bill, messageError);

		previousDocument.CSI_Code = "SWEB";
		AssertNoRowMessageError("Good document type", bill, messageError);

		var bill2 = nctsHeader.Bills.AddNew();
		nctsHeader.PreviousDocuments.AddNew().CSI_Code = "XXXX";
		AssertNoRowMessageError("Validation not triggered by header document", bill2, messageError);

		var arrivalNctsHeader = Factory.New<NctsHeader>();
		arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill3 = arrivalNctsHeader.Bills.AddNew();
		bill3.PreviousDocuments.AddNew().CSI_Code = "XXXX";
		AssertNoRowMessageError("Validation not done for Arrival", bill, messageError);
	});
}
