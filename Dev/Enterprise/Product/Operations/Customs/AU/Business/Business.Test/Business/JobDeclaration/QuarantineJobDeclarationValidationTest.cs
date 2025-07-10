using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineJobDeclarationValidationTest : ExportJobDeclarationValidationTest
	{
		public override void TestMergeByForExport()
		{
			Assert("Quarantine declaration for AU does not merge", true);
		}

		public void TestCheckJE_MarksAndNumbersShort()
		{
			var validation = (Customs.Business.BaseJobDeclarationValidation)declaration.Validation;
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			declaration.JE_MarksAndNumbersShort = ZString.Empty;
			validation.ValidateJE_MarksAndNumbersShort();
			Assert("Print Auto and Marks empty", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			validation.ValidateJE_MarksAndNumbersShort();
			Assert("Print NR and Marks empty", declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());
			declaration.JE_MarksAndNumbersShort = "GREAT SOME MARKS";
			validation.ValidateJE_MarksAndNumbersShort();
			Assert("Print NR and Marks not empty", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());
		}

		public void TestCheckJE_OH_Forwarder()
		{
			Assert("Pre-Condition", !declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertNoMessageErrors("produce type not meat", declaration.JE_OH_ForwarderInfo);
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);
			declaration.JE_RL_NKFinalDestination = "JPAMX";
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertHasMessageError("produce type meat, detination JP", declaration.JE_OH_ForwarderInfo, "Forwarder must be entered for produce type meat to Japan or South Korea.");
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertNoMessageErrors("produce type meat, detination JP, forwarder and Importer", declaration.JE_OH_ForwarderInfo);
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, "Forwarder cannot be entered when Importer is blank.");
		}

		[TestDate(2004, 12, 12)]
		public override void TestValidateJE_ExportDate()
		{
			Assert("Pre-Condition", !declaration.JE_ExportDateInfo.HasMessageErrors());
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			declaration.Validation.ValidateJE_ExportDate();
			Assert("Certificate not required, empty export date", declaration.JE_ExportDateInfo.HasMessageErrors());
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			declaration.JE_ExportDate = ZDateTime.Now;
			declaration.Validation.ValidateJE_ExportDate();
			Assert("Certificate automatic, empty export date", !declaration.JE_ExportDateInfo.HasMessageErrors());
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			declaration.Validation.ValidateJE_ExportDate();
			Assert("Certificate not required, export datenot empty", !declaration.JE_ExportDateInfo.HasMessageErrors());
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			Assert("Export date is earlier than todays date", declaration.JE_ExportDateInfo.HasMessageErrors());
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(1);
			Assert("Export date is later than todays date", !declaration.JE_ExportDateInfo.HasMessageErrors());
		}

		[TestDate(2014, 11, 11, 12, 30, 0)]
		public void TestExportDate()
		{
			ZDateTime testDate = ZDateTime.Now;
			ZDateTime testDateToCheck = ZDateTime.Now.AddHours(5);
			Assert("Precondition test - previous validation code: ZDateTime comparison for the same date but with different time value can have the same date appearing to cause validation error saying it is earlier", testDate < testDateToCheck);

			testDate = new ZDateTime(2014, 11, 11, 00, 00, 00);
			testDateToCheck = new ZDateTime(2014, 11, 11, 00, 00, 01);
			Assert("Precondition test - previous validation code: same date value but validation error incorrectly saying it is earlier", testDate < testDateToCheck);

			declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			Assert("Export date is earlier than todays date", declaration.JE_ExportDateInfo.HasMessageErrors());
			declaration.JE_ExportDate = ZDateTime.Now;
			Assert("Departure date when set to todays date & validating the date component only (not comparing ZDateTime value) is NOT earlier than todays date", !declaration.JE_ExportDateInfo.HasMessageErrors());

			declaration.JE_ExportDate = ZDateTime.Now.AddHours(-3);
			Assert("Departure date when set to todays date & validating the date component only (not comparing ZDateTime value) is NOT earlier than todays date", !declaration.JE_ExportDateInfo.HasMessageErrors());
		}

		[TestDate(2021, 02, 06)] // My 60th birthday!! :o)
		public void TestExportDateValidationAccountForMessageType()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			quarantineHeader = helper.Header1.QuarantineExDocHeader;

			declaration.Validation.ValidateJE_ExportDate();
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			AssertHasMessageError("Export date is earlier than todays date", declaration.JE_ExportDateInfo, "Departure date cannot be earlier than todays date.");

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			declaration.Validation.ValidateJE_ExportDate();
			AssertHasMessageError("Export date still validates for this status condition", declaration.JE_ExportDateInfo, "Departure date cannot be earlier than todays date.");

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			declaration.Validation.ValidateJE_ExportDate();
			AssertNoMessageError("This error is only applicable for an order or lodge so we need to remove it from the send errors for other status types", declaration.JE_ExportDateInfo, "Departure date cannot be earlier than todays date.");
		}

		public void TestCheckJE_OH_Importer()
		{
			const string messageError = "An Importer should be specified for a Certificate Request AQS declaration, and it cannot be 'To Order'.";
			declaration.JE_ToOrder = true;
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);

			declaration.IsAQISCertificateRequest = true;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);

			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);

			declaration.JE_ToOrder = false;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			importer.OH_FullName = "012345678901234567890123456789012345";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);

			importer.MainAddress.OA_Address1 = "012345678901234567890123456789012345";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);

			importer.MainAddress.OA_Address2 = "012345678901234567890123456789012345";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);

			importer.OH_FullName = "Test Importer FullName 50 Character Limit - Australia Importer";
			importer.MainAddress.OA_Address1 = "Test Address 1 35 Character Limit - WiseTech ST";
			importer.MainAddress.OA_Address2 = "Test Address 2 35 Character Limit - W Building";
			importer.MainAddress.City = "Test City 35 Character Limit - Mamungkukumpurangku";
			importer.MainAddress.State = "Test State 20 Character";

			declaration.Validation.ValidateJE_OH_Importer();
			var importerWarningMessageHeader = "One or more segments of the Consignee address exceed the allowed limit and will be truncated in the message to EXDOC unless changed:";

			AssertEquals("Declaration Importer has warning", true, declaration.JE_OH_ImporterInfo.HasWarnings());
			var importerWarningMessage = declaration.JE_OH_ImporterInfo.Notifications.ToMessageListString();

			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, importerWarningMessageHeader);
			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, "Importer FullName exceeds the 50 character limit");
			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, "Importer Address 1 exceeds the 35 character limit");
			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, "Importer Address 2 exceeds the 35 character limit");
			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, "Importer City exceeds the 35 character limit");
			AssertHasWarningContaining(importerWarningMessage, declaration.JE_OH_ImporterInfo, "Importer State exceeds the 20 character limit");
		}

		public void TestCheckJE_OH_ImporterAddressWithinLimit()
		{
			declaration.IsAQISCertificateRequest = true;
			declaration.JE_OH_Importer = importer.PK;

			importer.OH_FullName = "01234567890123456789012345678901234567890123456789";
			importer.MainAddress.OA_Address1 = "01234567890123456789012345678901234";
			importer.MainAddress.OA_Address2 = "01234567890123456789012345678901234";
			importer.MainAddress.City = "01234567890123456789012345678901234";
			importer.MainAddress.State = "01234567890123456789";

			declaration.Validation.ValidateJE_OH_Importer();
			AssertEquals("Declaration Importer has no warning", false, declaration.JE_OH_ImporterInfo.HasWarnings());
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "12345678901234567890123456789012345";
			const string messageError = "The Shipping Line name must not be more than 35 characters long.";
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertNoMessageError(declaration.JE_OH_ShippingLineInfo, messageError);
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertNoMessageError(declaration.JE_OH_ShippingLineInfo, messageError);
			shippingLine.OH_FullName = "123456789012345678901234567890123456";
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertHasMessageError(declaration.JE_OH_ShippingLineInfo, messageError);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			Assert("Pre-Condition", !declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.QuarantineInvoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			declaration.QuarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, "You have not entered a Voyage");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, "You have not entered a Flight/Folio");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.QuarantineInvoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, "You have not entered a Voyage");
		}

		public void TestCheckJE_VesselName()
		{
			Assert("Pre-Condition", !declaration.JE_VesselNameInfo.HasMessageErrors());
			declaration.JE_VesselName = ZString.Empty;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.QuarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageError(declaration.JE_VesselNameInfo, "You have not entered a Vessel");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageError(declaration.JE_VesselNameInfo, "You have not entered a Vessel");
		}

		#region Implementation

		OrgHeader importer;
		OrgHeader forwarder;
		JobComInvoiceHeader invoiceHeader;
		QuarantineExDocHeader quarantineHeader;

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "BIG TEST FORWARDER";
			forwarder.OH_Code = "FWDR";
			importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "BIG TEST IMPORTER";
			importer.OH_Code = "IMPR";
			invoiceHeader = declaration.Invoices.AddNew();
		}

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new QuarantineJobDeclarationValidation(jobDeclaration);
		}

		#endregion
	}
}
