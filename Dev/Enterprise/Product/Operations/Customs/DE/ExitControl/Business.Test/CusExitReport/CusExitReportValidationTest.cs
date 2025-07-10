using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class CusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_TransportMode_MandatoryWhenIsAutomatedValidationEnabledTra()
		{
			var (report, header) = CusExitReportTest.GetNewBusinessObject(Factory);
			CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);

			AssertEquals("Precondition: TRA Automated Validation", expected: true, header.IsAutomatedValidationEnabledTRA);
			report.CER_Type = DEExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportType_MandatoryWhenIsAutomatedValidationEnabledTra()
		{
			var (report, header) = CusExitReportTest.GetNewBusinessObject(Factory);
			CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);

			AssertEquals("Precondition: TRA Automated Validation", expected: true, header.IsAutomatedValidationEnabledTRA);
			report.CER_Type = DEExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TransportTypeInfo);
		}

		public void TestCheckCER_Location_MandatoryWhenIsAutomatedValidationEnabledTra()
		{
			var (report, header) = CusExitReportTest.GetNewBusinessObject(Factory);
			CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);

			AssertEquals("Precondition: TRA Automated Validation", expected: true, header.IsAutomatedValidationEnabledTRA);
			report.CER_Type = DEExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_LocationInfo);
		}

		public void TestCheckCER_DateTime()
		{
			var (report, header) = CusExitReportTest.GetNewBusinessObject(Factory);
			var targetInfo = report.CER_DateTimeInfo;
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;

			report.CER_Type = DEExitReportTypeList.Codes.ExitNotification;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			report.CER_Type = DEExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			report.CER_TransportType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		}

		public void TestCheckCER_OfficeOfExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Desc.");
			var sampleCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.Germany, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002702", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sampleCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.Germany, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002703", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Constants.CountryCodes.Germany);
			helper.CreateCusCodeListAttribute(sampleCodeList1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			(var report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var officeOfExportInfo = report.CER_OfficeOfExportInfo;
			var invalidMessageError = ListValidation.InvalidCodeMessageError;

			report.CER_OfficeOfExport = "123";
			AssertHasMessageError(officeOfExportInfo, invalidMessageError);

			report.CER_OfficeOfExport = sampleCodeList1.ZZD_Code;
			AssertNoMessageError(officeOfExportInfo, invalidMessageError);

			report.CER_OfficeOfExport = sampleCodeList2.ZZD_Code;
			AssertHasMessageError(officeOfExportInfo, invalidMessageError);
		}

		public void TestCheckCER_OfficeOfExport_EqualsCER_OfficeOfExit()
		{
			const string messageError = "The entered value must be different to the ‘Office of Exit’ if you want to forward the declaration to another customs office of exit.";
			(var report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var officeOfExportInfo = report.CER_OfficeOfExportInfo;
			report.CER_OfficeOfExit = "123";
			CombineAssertions(() =>
			{
				report.CER_OfficeOfExport = "321";
				AssertNoMessageError("Differ", officeOfExportInfo, messageError);

				report.CER_OfficeOfExport = "123";
				AssertHasMessageError("Equal", officeOfExportInfo, messageError);

				report.CER_OfficeOfExit = ZString.Empty;
				report.CER_OfficeOfExport = ZString.Empty;
				AssertNoMessageError("Both Empty", officeOfExportInfo, messageError);
			});
		}

		public void TestCheckCER_OfficeOfExport_DiffersFromCER_OfficeOfExit()
		{
			const string warning = "This field should only be filled if you want to forward the declaration to another customs office of exit.";
			(var report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			var officeOfExportInfo = report.CER_OfficeOfExportInfo;
			report.CER_OfficeOfExit = "123";
			CombineAssertions(() =>
			{
				report.CER_OfficeOfExport = "321";
				AssertHasWarning("Differ", officeOfExportInfo, warning);

				report.CER_OfficeOfExport = "123";
				AssertNoWarning("Equal", officeOfExportInfo, warning);

				report.CER_OfficeOfExport = ZString.Empty;
				AssertNoWarning("Differ - Empty", officeOfExportInfo, warning);
			});
		}

		public void TestCheckCER_TransportMode_MandatoryWhenTypeTRA()
		{
			(var report, _) = GetNewBusinessObjectTRA(Factory);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportType_MandatoryWhenTypeTRA()
		{
			(var report, _) = GetNewBusinessObjectTRA(Factory);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TransportTypeInfo);
		}

		public void TestCheckCER_Location_MandatoryWhenTypeTRA()
		{
			var (report, header) = GetNewBusinessObjectTRA(Factory);
			CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, enableAutomatedValidationTRA: true);

			AssertEquals("Precondition: TRA Automated Validation", expected: true, header.IsAutomatedValidationEnabledTRA);
			report.CER_Type = DEExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_LocationInfo);
		}

		public void TestCheckCER_DateTime_MandatoryWhenTRA()
		{
			(var report, _) = GetNewBusinessObjectTRA(Factory);
			var targetInfo = report.CER_DateTimeInfo;
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			report.CER_TransportType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		}

		static (CusExitReport report, CusExitHeader header) GetNewBusinessObjectTRA(BusinessObjectFactory factory)
		{
			var (report, header) = CusExitReportTest.GetNewBusinessObject(factory);
			report.CER_Type = DEExitReportTypeList.Codes.Transfer;
			return (report, header);
		}
	}
}
