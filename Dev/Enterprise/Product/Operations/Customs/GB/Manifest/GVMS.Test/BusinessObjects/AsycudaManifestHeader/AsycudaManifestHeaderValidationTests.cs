using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class AsycudaManifestHeaderValidationTests : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Trailer1RegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_Trailer1RegNo();
			AssertHasMessageError(header.AMA_Trailer1RegNoInfo, "You have not entered a Trailer 1.");

			header.AMA_Trailer1RegNo = "1";
			AssertNoMessageError(header.AMA_Trailer1RegNoInfo, "You have not entered a Trailer 1.");
		}

		public void TestCheckRouteId()
		{
			GVMSTestHelper.SetupPortsForRouteCalculation(Factory);
			GVMSTestHelper.SetupRoutesForRouteCalculation(Factory);
			GVMSTestHelper.SetUpCarriersForRouteCalculation(Factory);

			var header = Factory.New<AsycudaManifestHeader>();
			header.Validation.ValidateRouteId();
			AssertHasMessageError(header.RouteIdInfo, "You have not entered a Route ID.");

			header.RouteId = "1";
			header.Validation.ValidateRouteId();
			AssertNoMessageError(header.RouteIdInfo, "You have not entered a Route ID.");

			header.AMA_RL_NKPortOfLoading = "BEZEE";
			header.AMA_RL_NKPortOfDischarge = "GBHUL";
			header.AMA_CarrierCode = "ABC";

			header.RouteId = "R2";
			header.Validation.ValidateRouteId();

			AssertHasMessageErrorContaining("", header.RouteIdInfo, "CargoWise has calculated that the route code for BEZEE->GBHUL via carrier ABC should be R1");

			header.RouteId = "R1";
			header.Validation.ValidateRouteId();

			AssertNoMessageErrorContaining("", header.RouteIdInfo, "CargoWise has calculated that the route code for BEZEE->GBHUL via carrier ABC should be R1");
		}

		public void TestCheckHaulierType()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.Validation.ValidateHaulierType();
			AssertNoMessageErrors("No error when HaulierType is empty", header.HaulierTypeInfo);

			header.HaulierType = "XXX";
			header.Validation.ValidateHaulierType();
			AssertHasMessageErrorContaining(header.HaulierTypeInfo, ListValidation.InvalidCodeMessageError);

			header.HaulierType = GVMSHaulierType.Codes.Standard;
			header.Validation.ValidateHaulierType();
			AssertNoMessageErrorContaining(header.HaulierTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAMA_JobReference()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_JobReference();
			AssertHasMessageError(header.AMA_JobReferenceInfo, "There are no records in any of the three Customs References grids.");

			header.GvmsCustomsReferenceCollection.AddNew();
			header.Validation.ValidateAMA_JobReference();
			AssertNoMessageError(header.AMA_JobReferenceInfo, "There are no records in any of the three Customs References grids.");

			header.GvmsCustomsReferenceCollection.RemoveAndDeleteAll();
			header.GvmsTransitReferenceCollection.AddNew();
			header.Validation.ValidateAMA_JobReference();
			AssertNoMessageError(header.AMA_JobReferenceInfo, "There are no records in any of the three Customs References grids.");

			header.GvmsTransitReferenceCollection.RemoveAndDeleteAll();
			header.GvmsEidrAndOralReferenceCollection.AddNew();
			header.Validation.ValidateAMA_JobReference();
			AssertNoMessageError(header.AMA_JobReferenceInfo, "There are no records in any of the three Customs References grids.");
			AssertNoNotifications(header.AMA_JobReferenceInfo);
		}

		public void TestCheckAMA_Nature()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = "XYZ";
			AssertListValidationInvalidCodeMessageError(header.AMA_NatureInfo, true);

			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			AssertListValidationInvalidCodeMessageError(header.AMA_NatureInfo, false);

			header.AMA_Nature = ZString.Empty;
			AssertListValidationInvalidCodeMessageError(header.AMA_NatureInfo, false);
		}

		public void TestCheckAMA_CarrierCode()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "ABC";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			carrier.ZZ4_Description = "Carrier 1";
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, ZString.Empty);
			carrier.Attributes.AddNew("SEA", "SEA");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_CarrierCode = ZString.Empty;
			AssertNoMessageErrorContaining(header.AMA_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_CarrierCode = "A123";
			AssertHasMessageErrorContaining(header.AMA_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_CarrierCode = "ABC";
			AssertNoMessageErrorContaining(header.AMA_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "ABC";
			AssertNoNotifications(header.AMA_CustomsOfficeInfo);
		}

		public void TestValidateSingleICSCustomsDeclarations()
		{
			var msgError = "Only one master S&S record is allowed. If multiple references are needed, detail one against each row in the S&S Reference column";
			var header = Factory.New<AsycudaManifestHeader>();
			var ics1 = header.GvmsCustomsReferenceCollection.AddNew();
			ics1.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;

			var cds1 = header.GvmsCustomsReferenceCollection.AddNew();
			cds1.CSI_Code = GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn;

			header.Validation.ValidateAll();

			CombineAssertions("2 Rows no duplicates", () =>
			{
				AssertNoRowMessageError("ICS1", ics1, msgError);
				AssertNoRowMessageError("CDS1", cds1, msgError);
			});

			var ics2 = header.GvmsCustomsReferenceCollection.AddNew();
			ics2.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;

			header.Validation.ValidateAll();

			CombineAssertions("3 Rows 1 duplicate", () =>
			{
				AssertNoRowMessageError("ICS1", ics1, msgError);
				AssertNoRowMessageError("CDS1", cds1, msgError);
				AssertHasRowMessageError("ICS2", ics2, msgError);
			});

			var ics3 = header.GvmsCustomsReferenceCollection.AddNew();
			ics3.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;

			header.Validation.ValidateAll();

			CombineAssertions("4 Rows 2 duplicates", () =>
			{
				AssertNoRowMessageError("ICS1", ics1, msgError);
				AssertNoRowMessageError("CDS1", cds1, msgError);
				AssertHasRowMessageError("ICS2", ics2, msgError);
				AssertHasRowMessageError("ICS3", ics3, msgError);
			});

			ics2.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			ics3.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;

			header.Validation.ValidateAll();

			CombineAssertions("4 Rows no duplicates", () =>
			{
				AssertNoRowMessageError("ICS2", ics2, msgError);
				AssertNoRowMessageError("ICS3", ics3, msgError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GVMSTestHelper.SetupPorts(Factory);
		}
	}
}
