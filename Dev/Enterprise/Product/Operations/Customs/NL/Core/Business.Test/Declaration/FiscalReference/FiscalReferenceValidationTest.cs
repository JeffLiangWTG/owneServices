using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class FiscalReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_Code()
	{
		var fr = Factory.New<FiscalReference>();

		AssertEquals(fr.CSI_Type, "FIS");
		fr.CSI_Code = string.Empty;
		fr.CSI_ReferenceNumber = "Anything";
		AssertHasMessageError(fr.CSI_CodeInfo, "Please enter a Type Code.");

		fr.CSI_Code = "FR1";
		fr.CSI_ReferenceNumber = "Anything";
		AssertNoMessageError(fr.CSI_CodeInfo, "Please enter a Type Code.");
	}

	public void TestCSI_ReferenceNumber()
	{
		var fr = Factory.New<FiscalReference>();

		AssertEquals(fr.CSI_Type, "FIS");
		fr.CSI_ReferenceNumber = string.Empty;
		AssertHasMessageError(fr.CSI_ReferenceNumberInfo, "Please enter a Holder EORI.");

		fr.CSI_Code = "FR1";
		fr.CSI_ReferenceNumber = "Anything";
		AssertNoMessageError(fr.CSI_ReferenceNumberInfo, "Please enter a Holder EORI.");
	}
}
