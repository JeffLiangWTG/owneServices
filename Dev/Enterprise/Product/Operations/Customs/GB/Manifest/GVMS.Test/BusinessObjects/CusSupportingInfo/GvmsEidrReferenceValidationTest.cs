using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing;

public class GvmsEidrReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
		var gvmsEidrReference = header.GvmsEidrAndOralReferenceCollection.AddNew();
		gvmsEidrReference.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
		gvmsEidrReference.CSI_ReferenceNumber = "XI1234567890";
		AssertNoMessageError(gvmsEidrReference.CSI_ReferenceNumberInfo, "For the selected nature and declaration type, only XI-prefixed EORIs are allowed");

		gvmsEidrReference.CSI_ReferenceNumber = "AB1234567890";
		AssertHasMessageError(gvmsEidrReference.CSI_ReferenceNumberInfo, "For the selected nature and declaration type, only XI-prefixed EORIs are allowed");

		gvmsEidrReference.CSI_Code = GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration;
		gvmsEidrReference.CSI_ReferenceNumber = "XI1234567890";
		AssertNoMessageError(gvmsEidrReference.CSI_ReferenceNumberInfo, "For the selected nature and declaration type, only XI-prefixed EORIs are allowed");

		gvmsEidrReference.CSI_ReferenceNumber = "AB1234567890";
		AssertHasMessageError(gvmsEidrReference.CSI_ReferenceNumberInfo, "For the selected nature and declaration type, only XI-prefixed EORIs are allowed");

		gvmsEidrReference.CSI_Code = GVMSCustomsReference.Codes.OralDeclaration;
		gvmsEidrReference.CSI_ReferenceNumber = "AB1234567890";
		AssertNoMessageError(gvmsEidrReference.CSI_ReferenceNumberInfo, "For the selected nature and declaration type, only XI-prefixed EORIs are allowed");
	}

	public void TestCheckCSI_Code()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
		var gvmsEidrReference = header.GvmsEidrAndOralReferenceCollection.AddNew();
		gvmsEidrReference.CSI_Code = GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration;
		AssertNoMessageError(gvmsEidrReference.CSI_CodeInfo, "IMS declarations are only allowed for GB-to-NI (nature G2N) movements");

		header.AMA_Nature = GVMSManifestNature.Codes.Export;
		gvmsEidrReference.Validation.ValidateCSI_Code();
		AssertHasMessageError(gvmsEidrReference.CSI_CodeInfo, "IMS declarations are only allowed for GB-to-NI (nature G2N) movements");
	}
}

