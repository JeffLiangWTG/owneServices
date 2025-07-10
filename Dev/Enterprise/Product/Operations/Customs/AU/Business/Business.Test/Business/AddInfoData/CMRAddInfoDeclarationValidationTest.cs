using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRAddInfoDeclarationValidationTest : AUAddInfoValidationTest
	{
		public void TestSettingCustomsShipClearsVesselNotification()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_VesselName();
			Assert("Pre-condition", declaration.JE_VesselNameInfo.HasNotifications());
			declaration.ZA_CustShipNo_Hidden = "XX";
			Assert("Notification cleared", !declaration.JE_VesselNameInfo.HasNotifications());
		}

		public void TestFirstPaidUnderProtestValidation()
		{
			declaration.AddInfo.ZA_FPUP_Hidden = "12345678";
			AssertEquals("First Paid Under Protest", true, declaration.AddInfo.ZA_FPUP_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_FPUP_Hidden = "123456789";
			AssertEquals("First Paid Under Protest", false, declaration.AddInfo.ZA_FPUP_HiddenInfo.HasMessageErrors());
		}

		public void TestValidationBetweenFirstPaidUnderProtestAndPUPStatement()
		{
			IMDJobDeclarationValidation declarationValidation = new IMDJobDeclarationValidation(declaration);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_PUP = "Y";
			declaration.JE_PaidUnderProtestStatement = "Test";
			AssertNoMessageErrors("Pup statement", declaration.JE_PaidUnderProtestStatementInfo);
			AssertNoMessageErrors("First Paid Under Protest", declaration.AddInfo.ZA_FPUP_HiddenInfo);

			declaration.AddInfo.ZA_FPUP_Hidden = "123456789";
			declarationValidation.ValidateJE_PaidUnderProtestStatement();
			AssertHasMessageErrors("First Paid Under Protest", declaration.AddInfo.ZA_FPUP_HiddenInfo);
			AssertHasMessageErrors("Pup statement", declaration.JE_PaidUnderProtestStatementInfo);

			invoiceLine.AddInfo.ZA_PUP = ZString.Empty;
			declaration.JE_PaidUnderProtestStatement = ZString.Empty;
			declaration.AddInfo.Validation.ValidateZA_FPUP_Hidden();
			AssertNoMessageErrors("Pup statement", declaration.JE_PaidUnderProtestStatementInfo);
			AssertNoMessageErrors("First Paid Under Protest", declaration.AddInfo.ZA_FPUP_HiddenInfo);

			invoiceLine.AddInfo.ZA_PUP = "Y";
			declaration.JE_PaidUnderProtestStatement = "Test";
			declaration.AddInfo.Validation.ValidateZA_FPUP_Hidden();
			AssertHasMessageErrors("First Paid Under Protest", declaration.AddInfo.ZA_FPUP_HiddenInfo);
			AssertHasMessageErrors("Pup statement", declaration.JE_PaidUnderProtestStatementInfo);

			declaration.AddInfo.ZA_FPUP_Hidden = ZString.Empty;
			declarationValidation.ValidateJE_PaidUnderProtestStatement();
			AssertNoMessageErrors("Pup statement", declaration.JE_PaidUnderProtestStatementInfo);
			AssertNoMessageErrors("First Paid Under Protest", declaration.AddInfo.ZA_FPUP_HiddenInfo);
		}

		public void TestCheckNillReturnIndicator()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;

			declaration.NilReturnInd = false;
			AssertEquals("NillReturnIndicator has no errors", false, declaration.AddInfo.ZA_NilReturnInd_HiddenInfo.HasMessageErrors());
			declaration.NilReturnInd = true;
			AssertEquals("NillReturnIndicator has error", true, declaration.AddInfo.ZA_NilReturnInd_HiddenInfo.HasMessageErrors());
			declaration.JE_SettlementPeriodType = "SW";
			AssertEquals("NillReturnIndicator has no errors", false, declaration.AddInfo.ZA_NilReturnInd_HiddenInfo.HasMessageErrors());

			declaration.NilReturnInd = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.NilReturnInd = true;
			AssertHasMessageErrorContaining(declaration.NilReturnIndInfo, "A Nil Return declaration cannot contain any invoice lines");
		}

		public void TestCheckZA_HART_Hidden()
		{
			CMRCodeLists code = CMRCodeLists.New(Factory);
			code.CI_Code = "Origin";
			code.CI_CodeType = "HEADARSTYP";
			code.CI_Startdate = ZDateTime.Today;
			code.CI_Description = "Description";

			AssertEquals("No errors", false, declaration.AddInfo.ZA_HART_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_HART_Hidden = "RRR";
			AssertEquals("Errors", true, declaration.AddInfo.ZA_HART_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_HART_Hidden = "Origin";
			AssertEquals("No Errors", false, declaration.AddInfo.ZA_HART_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_HART_Hidden = ZString.Empty;
			AssertNoMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);

			declaration.AddInfo.ZA_HART_Hidden = "Origin";
			AssertNoMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);

			declaration.JE_AmberStatement = "Foo";
			declaration.AddInfo.ZA_HART_Hidden = ZString.Empty;
			AssertHasMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);

			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AMB = "C";
			declaration.AddInfo.ZA_HART_Hidden = ZString.Empty;
			AssertNoMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);

			invoiceLine.AddInfo.ZA_AMB = "";
			declaration.AddInfo.ZA_HART_Hidden = ZString.Empty;
			AssertHasMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);

			declaration.AddInfo.ZA_HART_Hidden = "Origin";
			AssertNoMessageErrors(declaration.AddInfo.ZA_HART_HiddenInfo);
		}

		public void TestUnaccompaniedPersonalEffectsIdValidation()
		{
			declaration.AddInfo.ZA_UPE_Hidden = "12345678";

			AssertEquals("Unaccompanied Personal Effects Id", true, declaration.AddInfo.ZA_UPE_HiddenInfo.HasMessageErrors());

			declaration.AddInfo.ZA_UPE_Hidden = "123456789";
			AssertEquals("Unaccompanied Personal Effects Id", false, declaration.AddInfo.ZA_UPE_HiddenInfo.HasMessageErrors());
		}

		public void TestSettingInspectionLocationValidatesDeliveryPostCode()
		{
			var refTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDateTime(2020, 1, 1);
			var endDate = new ZDateTime(2076, 6, 6);
			var postcode1 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "123X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode1.PK, "PostcodeDeliveryClassification", "SPLIT");
			Factory.Save();

			using (declaration.GetValidationSuspender())
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				OrgHeader importer = OrgHeader.New(Factory);
				importer.OH_IsConsignee = true;
				declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
				declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
				declaration.ImporterDeliveryAddress.E2_Postcode = "123X";
				AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
				concernType2.Code = "RURL";
			}
			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");

			declaration.AddInfo.ZA_AQISInspectLocation_Hidden = "";

			AssertHasMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");

			declaration.AddInfo.ZA_AQISInspectLocation_Hidden = "INSPECTION LOCATION";

			AssertNoMessageErrorContaining(declaration.AddInfo.ZA_AQISInspectLocation_HiddenInfo, "Delivery address post code is listed as SPLIT");
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.E2_PostcodeInfo, "Delivery address post code is listed as SPLIT");
		}

		public void TestCheckZA_SettlementPeriodType_Hidden_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(addInfo.ZA_SettlementPeriodType_HiddenInfo, "??", "SW");
		}

		public void TestCheckZA_SettlementPeriodType_Hidden_ValidateZA_NilReturnInd_Hidden() => CombineAssertions(() =>
		{
			const string settlementIndicatorMessage = "Nil Return Indicator may only be set when the Settlement Type is selected.";
			const string invoiceLineMessage = "A Nil Return declaration cannot contain any invoice lines.";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertNoMessageError("ZA_NilReturnInd_Hidden is not set when Weekly Settlement Indicator is not set", addInfo.ZA_NilReturnInd_HiddenInfo, settlementIndicatorMessage);
			AssertNoMessageError("ZA_NilReturnInd_Hidden is not set when there's invoice line", addInfo.ZA_NilReturnInd_HiddenInfo, invoiceLineMessage);

			addInfo.ZA_NilReturnInd_Hidden = "Y";
			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertHasMessageError("ZA_NilReturnInd_Hidden is set when Weekly Settlement Indicator is not set", addInfo.ZA_NilReturnInd_HiddenInfo, settlementIndicatorMessage);
			AssertHasMessageError("ZA_NilReturnInd_Hidden is set when there's invoice line", addInfo.ZA_NilReturnInd_HiddenInfo, invoiceLineMessage);

			invoiceLine.Delete();
			addInfo.ZA_SettlementPeriodType_Hidden = "SM";
			AssertNoMessageError("ZA_NilReturnInd_Hidden is set when Weekly Settlement Indicator is also set", addInfo.ZA_NilReturnInd_HiddenInfo, settlementIndicatorMessage);
			AssertNoMessageError("ZA_NilReturnInd_Hidden is set when there's no invoice line", addInfo.ZA_NilReturnInd_HiddenInfo, invoiceLineMessage);
		});

		public void TestCheckZA_SettlementPeriodType_Hidden_ValidateZA_SettlementPeriodStartDate_Hidden() => CombineAssertions(() =>
		{
			const string startDateRequiredMessage = "Settlement Period Start Date is required if Settlement Type is selected.";
			const string invalidMonthlyStartDate = "Settlement Period Start Date must be the 1st day of a month if Monthly Settlement Type is selected.";
			const string invalidQuarterlyStartDate = "Settlement Period Start Date must be the 1st day of January, April, July or October if Quarterly Settlement Type is selected.";

			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertNoNotifications("Settlement Period Start Date is not set when Settlement Type is not selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo);

			addInfo.ZA_SettlementPeriodType_Hidden = "SW";
			AssertHasMessageError("Settlement Period Start Date is not set when Weekly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodType_Hidden = "WEK";
			AssertHasMessageError("Settlement Period Start Date is not set when Weekly (Legacy) Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 06, 15);
			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertNoMessageError("Settlement Period Start Date is set when Weekly (Legacy) Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodType_Hidden = "SM";
			AssertHasMessageError("Settlement Period Start Date is not 1st day of a month when Monthly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidMonthlyStartDate);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 06, 1);
			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertNoMessageError("Settlement Period Start Date is 1st day of a month when Monthly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidMonthlyStartDate);

			addInfo.ZA_SettlementPeriodType_Hidden = "SQ";
			AssertHasMessageError("Settlement Period Start Date is 1st day of Jan/Apr/Jul/Oct when Quarterly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidQuarterlyStartDate);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 07, 1);
			addInfo.Validation.ValidateZA_SettlementPeriodType_Hidden();
			AssertNoMessageError("Settlement Period Start Date is 1st day of Jan/Apr/Jul/Oct when Quarterly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidQuarterlyStartDate);
		});

		public void TestCheckZA_SettlementPeriodStartDate_Hidden() => CombineAssertions(() =>
		{
			const string startDateRequiredMessage = "Settlement Period Start Date is required if Settlement Type is selected.";
			const string invalidMonthlyStartDate = "Settlement Period Start Date must be the 1st day of a month if Monthly Settlement Type is selected.";
			const string invalidQuarterlyStartDate = "Settlement Period Start Date must be the 1st day of January, April, July or October if Quarterly Settlement Type is selected.";

			addInfo.Validation.ValidateZA_SettlementPeriodStartDate_Hidden();
			AssertNoNotifications("Settlement Period Start Date is not set when Settlement Type is not selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo);

			addInfo.ZA_SettlementPeriodType_Hidden = "SW";
			addInfo.Validation.ValidateZA_SettlementPeriodStartDate_Hidden();
			AssertHasMessageError("Settlement Period Start Date is not set when Weekly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodType_Hidden = "WEK";
			addInfo.Validation.ValidateZA_SettlementPeriodStartDate_Hidden();
			AssertHasMessageError("Settlement Period Start Date is not set when Weekly (Legacy) Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 06, 15);
			AssertNoMessageError("Settlement Period Start Date is set when Weekly (Legacy) Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, startDateRequiredMessage);

			addInfo.ZA_SettlementPeriodType_Hidden = "SM";
			addInfo.Validation.ValidateZA_SettlementPeriodStartDate_Hidden();
			AssertHasMessageError("Settlement Period Start Date is not 1st day of a month when Monthly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidMonthlyStartDate);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 06, 1);
			AssertNoMessageError("Settlement Period Start Date is 1st day of a month when Monthly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidMonthlyStartDate);

			addInfo.ZA_SettlementPeriodType_Hidden = "SQ";
			addInfo.Validation.ValidateZA_SettlementPeriodStartDate_Hidden();
			AssertHasMessageError("Settlement Period Start Date is 1st day of Jan/Apr/Jul/Oct when Quarterly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidQuarterlyStartDate);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 07, 1);
			AssertNoMessageError("Settlement Period Start Date is 1st day of Jan/Apr/Jul/Oct when Quarterly Settlement Type is selected", addInfo.ZA_SettlementPeriodStartDate_HiddenInfo, invalidQuarterlyStartDate);
		});

		public void TestCheckZA_SettlementPeriodEndDate_Hidden() => CombineAssertions(() =>
		{
			const string message = "Settlement Period End Date cannot be earlier than the Settlement Period Start Date.";
			addInfo.ZA_SettlementPeriodEndDate_Hidden = new ZDateTime(2023, 07, 18);
			AssertNoMessageError("Start date not specified", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = ZDateTime.Invalid;
			addInfo.Validation.ValidateZA_SettlementPeriodEndDate_Hidden();
			AssertNoMessageError("Start date invalid", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 07, 18);
			addInfo.Validation.ValidateZA_SettlementPeriodEndDate_Hidden();
			AssertNoMessageError("Start date equals end date", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);

			addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 07, 19);
			addInfo.Validation.ValidateZA_SettlementPeriodEndDate_Hidden();
			AssertHasMessageError("Start date after end date", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);

			addInfo.ZA_SettlementPeriodEndDate_Hidden = ZDateTime.Empty;
			AssertNoMessageError("End date not specified", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);

			addInfo.ZA_SettlementPeriodEndDate_Hidden = ZDateTime.Invalid;
			AssertNoMessageError("End date invalid", addInfo.ZA_SettlementPeriodEndDate_HiddenInfo, message);
		});

		public void TestCheckZA_EDITransmitDate()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				addInfo.ZA_EDITransmitDate = ZDateTime.Invalid;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Empty;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Today;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Today.AddDays(-1);
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Today.AddDays(1);
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.Validation.ValidateZA_EDITransmitDate();
				AssertNoMessageError("Departure date validation", declaration.JE_ExportDateInfo, "Please enter a Date of Export.");
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				addInfo.ZA_EDITransmitDate = ZDateTime.Invalid;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Empty;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Today;
				AssertNoMessageErrors(addInfo.ZA_EDITransmitDateInfo);

				addInfo.ZA_EDITransmitDate = ZDateTime.Today.AddDays(1);
				AssertNoMessageError("can Submit", addInfo.ZA_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");
				AssertNoMessageError("can Submit", declaration.JE_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");

				addInfo.ZA_EDITransmitDate = ZDateTime.Today.AddDays(-1);
				AssertHasMessageError("cannot Submit", addInfo.ZA_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");
				AssertHasMessageError("cannot Submit", declaration.JE_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");

				var entry = addInfo.JobDeclaration.ActiveEntryHeaders.AddNew();
				entry.CH_Status = Common.AU.CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
				addInfo.Validation.ValidateZA_EDITransmitDate();
				AssertNoMessageError("can Submit", addInfo.ZA_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");
				AssertNoMessageError("can Submit", declaration.JE_EDITransmitDateInfo, "You cannot Submit to Customs if your EDI Transmit Date is in the past.");

				addInfo.Validation.ValidateZA_EDITransmitDate();
				AssertHasMessageError("Departure date validation", declaration.JE_ExportDateInfo, "Please enter a Date of Export.");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			addInfo = declaration.AddInfo;
		}
	}
}
