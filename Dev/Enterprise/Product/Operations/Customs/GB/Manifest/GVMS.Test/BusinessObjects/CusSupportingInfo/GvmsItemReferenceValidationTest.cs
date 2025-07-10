using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GvmsItemReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber2()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var gvmsItemReference = header.GvmsCustomsReferenceCollection.AddNew();
			gvmsItemReference.Validation.ValidateCSI_ReferenceNumber2();
			AssertNoMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");

			gvmsItemReference.CSI_ReferenceNumber2 = "A";
			AssertNoMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");

			gvmsItemReference.CSI_ReferenceNumber2 = "a";
			AssertHasMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");

			gvmsItemReference.CSI_ReferenceNumber2 = "A1";
			AssertNoMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");

			gvmsItemReference.CSI_ReferenceNumber2 = "A1-";
			AssertNoMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");

			gvmsItemReference.CSI_ReferenceNumber2 = "A1-$";
			AssertHasMessageError(gvmsItemReference.CSI_ReferenceNumber2Info, "If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");
		}

		public void TestCheckCSI_Code()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			header.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			var gvmsItemReference = header.GvmsCustomsReferenceCollection.AddNew();

			gvmsItemReference.CSI_Code = GVMSCustomsReference.Codes.IndirectExportDeclarationEad;
			AssertNoMessageError(gvmsItemReference.CSI_CodeInfo, "EAD is not allowed for this manifest nature (direction)");
			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			gvmsItemReference.Validation.ValidateCSI_Code();
			AssertHasMessageError(gvmsItemReference.CSI_CodeInfo, "EAD is not allowed for this manifest nature (direction)");

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			header.EmptyVehicle = "";
			gvmsItemReference.CSI_Code = GVMSCustomsReference.Codes.ExemptGoods;
			AssertNoMessageError(gvmsItemReference.CSI_CodeInfo, "Type MT is only allowed for empty vehicles");
			gvmsItemReference.CSI_Code = GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle;
			AssertHasMessageError(gvmsItemReference.CSI_CodeInfo, "Type MT is only allowed for empty vehicles");

			AssertNoMessageError(gvmsItemReference.CSI_CodeInfo, "Only one reference of type MT is allowed");
			var gvmsItemReference2 = header.GvmsCustomsReferenceCollection.AddNew();
			gvmsItemReference2.CSI_Code = GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle;
			AssertHasMessageError(gvmsItemReference2.CSI_CodeInfo, "Only one reference of type MT is allowed");
		}

		public void TestCustomsReferenceCodeValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			VerifyValidationErrors(header.GvmsCustomsReferenceCollection.AddNew(), GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration, GVMSCustomsReference.Codes.EntryInDeclarantsRecord);
		}

		public void TestTransitReferenceCodeValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			VerifyValidationErrors(header.GvmsTransitReferenceCollection.AddNew(), GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber, GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration);
		}

		public void TestEidrReferenceCodeValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			VerifyValidationErrors(header.GvmsEidrAndOralReferenceCollection.AddNew(), GVMSCustomsReference.Codes.EntryInDeclarantsRecord, GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber);
		}

		void VerifyValidationErrors(GvmsItemReference item, string goodCode, string badButExistsCode)
		{
			item.Validation.ValidateCSI_Code();
			AssertHasMessageError(item.CSI_CodeInfo, MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(item.CSI_CodeInfo)));

			item.CSI_Code = goodCode;
			AssertNoMessageError(item.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			item.CSI_Code = "ZZX";
			AssertHasMessageError(item.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			item.CSI_Code = badButExistsCode;
			AssertHasMessageError(item.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
