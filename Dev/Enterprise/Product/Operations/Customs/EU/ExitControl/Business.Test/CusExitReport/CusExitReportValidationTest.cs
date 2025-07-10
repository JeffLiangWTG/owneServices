using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_EnquiryInformationCode()
		{
			AssertNoMessageError("Not Required", report.CER_EnquiryInformationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			var requiredReport = Factory.New<CusExitReportForTest>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(requiredReport.CER_EnquiryInformationCodeInfo);
		}

		public void TestCheckCER_Type_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TypeInfo, "XYZ", ExitReportTypeList.Codes.Presentation);
		}

		public void TestCheckCER_Type_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TypeInfo);
		}

		public void TestCheckCER_TransportMode_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportModeInfo, "XYZ", Customs.Business.TransportTypeList.Codes.Air);
		}

		public void TestCheckCER_TransportMode_NotMandatoryWhenTypePRE()
		{
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportMode_NotMandatoryWhenTypeEXT()
		{
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportType_ListValidation()
		{
			CombineAssertions(() =>
			{
				report.CER_TransportMode = TransportTypeList.Codes.Sea;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, CusExitReportTransportTypeList.Codes._21, CusExitReportTransportTypeList.Codes._10);

				report.CER_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, "XYZ", CusExitReportTransportTypeList.Codes._10);

				report.CER_TransportMode = TransportTypeList.Codes.Mail;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, "XYZ", CusExitReportTransportTypeList.Codes._10);

				report.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, "XYZ", CusExitReportTransportTypeList.Codes._10);

				report.CER_TransportMode = TransportTypeList.Codes.Rail;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Codes._21);

				report.CER_TransportMode = TransportTypeList.Codes.Road;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Codes._30);

				report.CER_TransportMode = TransportTypeList.Codes.Air;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Codes._40);

				report.CER_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				ValidationTestHelper.AssertErrorIfInvalidCode(report.CER_TransportTypeInfo, CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Codes._80);
			});
		}

		public void TestCheckCER_TransportType_NotMandatoryWhenTypePRE()
		{
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_TransportTypeInfo);
		}

		public void TestCheckCER_TransportType_NotMandatoryWhenTypeEXT()
		{
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_TransportTypeInfo);
		}

		public void TestCheckCER_TransportID_MandatoryWhenTransportTypeNotEmpty()
		{
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_TransportIDInfo);
		}

		public void TestCheckCER_TransportID_NotMandatoryWhenTransportTypeEmpty()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_TransportIDInfo);
		}

		public void TestCheckCER_TransportID_CharacterCasing()
		{
			const string message = "Must not contain lower case letters.";
			var targetInfo = report.CER_TransportIDInfo;
			CombineAssertions(() =>
			{
				foreach (var transportType in new CusExitReportTransportTypeList().GetAllCodes())
				{
					report.CER_TransportType = transportType;
					report.CER_TransportID = "lower";
					if (transportType.In(CusExitReportTransportTypeList.Codes._11, CusExitReportTransportTypeList.Codes._81))
					{
						AssertNoMessageError($"CER_TransportType={transportType}, CER_TransportID lower case", targetInfo, message);
					}
					else
					{
						AssertHasMessageError($"CER_TransportType={transportType}, CER_TransportID lower case", targetInfo, message);
					}

					report.CER_TransportID = "UPPER";
					AssertNoMessageError($"CER_TransportType={transportType}, CER_TransportID upper case", targetInfo, message);
				}

				report.CER_TransportType = ZString.Empty;
				report.CER_TransportID = "lower";
				AssertNoMessageError("CER_TransportType empty, CER_TransportID lower case", targetInfo, message);
			});
		}

		public void TestCheckCER_RN_NKTransportNationality_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(report.CER_RN_NKTransportNationalityInfo, "10", Core.Constants.CountryCodes.Germany);
		}

		public void TestCheckCER_RN_NKTransportNationality_MandatoryWhenTransportTypeNotEmpty()
		{
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(report.CER_RN_NKTransportNationalityInfo);
		}

		public void TestCheckCER_RN_NKTransportNationality_NotMandatoryWhenTransportTypeEmpty()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_RN_NKTransportNationalityInfo);
		}

		public void TestCheckCER_Location_NotMandatoryWhenTransportTypeEmpty()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(report.CER_LocationInfo);
		}

		public void TestCheckCER_CXC_Consignment()
		{
			report.CER_CXC_Consignment = ZGuid.Empty;
			AssertHasMessageError(report.CER_CXC_ConsignmentInfo, "An Exit Report must be linked to a Declaration/Entry.");
			report.CER_CXC_Consignment = header.CusExitConsignments.AddNew().PK;
			AssertNoMessageErrors(report.CER_CXC_ConsignmentInfo);
		}

		public void TestCheckCER_DateTime()
		{
			var targetInfo = report.CER_DateTimeInfo;
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			report.CER_TransportType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		}

		public void TestCheckCER_OfficeOfExit()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV11", "LV11 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(report.CER_OfficeOfExitInfo, "INV", "LV11");
		}

		public void TestCheckCER_CXC_Consignment_MrnAlreadyBeingUsed_NotUcc6() => CombineAssertions(() =>
		{
			var header = Factory.New<CusExitHeader>();
			var consignment1 = header.CusExitConsignments.AddNew();
			var consignment2 = header.CusExitConsignments.AddNew();

			consignment1.CXC_MovementReference = "MRN1234";
			consignment1.CXC_LocalReference = "1234";

			consignment2.CXC_MovementReference = "MRN1235";
			consignment2.CXC_LocalReference = "1235";

			AssertNotEquals("Prereq: different PK", consignment1.PK, consignment2.PK);

			var report1 = header.CusExitReports.AddNew();
			var report2 = header.CusExitReports.AddNew();

			report1.CER_CXC_Consignment = consignment1.PK;
			report2.CER_CXC_Consignment = consignment1.PK;
			report2.Validation.ValidateCER_CXC_Consignment();
			AssertNoNotifications(report2.CER_CXC_ConsignmentInfo);
		});

		public void TestCheckCER_CXC_Consignment_MrnAlreadyBeingUsed_Ucc6() => CombineAssertions(() =>
		{
			var header = Factory.GetUcc6ExitHeader();
			var consignment1 = header.CusExitConsignments.AddNew();
			var consignment2 = header.CusExitConsignments.AddNew();

			consignment1.CXC_MovementReference = "MRN1234";
			consignment1.CXC_LocalReference = "1234";

			consignment2.CXC_MovementReference = "MRN1235";
			consignment2.CXC_LocalReference = "1235";

			AssertNotEquals("Prereq: different PK", consignment1.PK, consignment2.PK);

			var report1 = header.CusExitReports.AddNew();
			var report2 = header.CusExitReports.AddNew();

			report1.CER_CXC_Consignment = consignment1.PK;
			report2.CER_CXC_Consignment = consignment1.PK;

			report2.Validation.ValidateCER_CXC_Consignment();
			AssertHasMessageErrorContaining(report2.CER_CXC_ConsignmentInfo, "This MRN is already being used in another declaration.");

			report2.CER_CXC_Consignment = consignment2.PK;
			report2.Validation.ValidateCER_CXC_Consignment();
			AssertNoMessageErrorContaining(report2.CER_CXC_ConsignmentInfo, "This MRN is already being used in another declaration.");
		});

		public void TestCheckCER_Location() => CombineAssertions(() =>
		{
			var report = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			report.CER_Location = ZString.Empty;
			AssertNoMessageErrorContaining(report.CER_LocationInfo, MandatoryValidation.YouHaveNotEntered);

			report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
			report.CER_Location = ZString.Empty;
			AssertHasMessageErrorContaining(report.CER_LocationInfo, MandatoryValidation.YouHaveNotEntered);

			report.CER_Location = "AH3";
			AssertNoMessageErrorContaining(report.CER_LocationInfo, MandatoryValidation.YouHaveNotEntered);
		});

		protected override void SetUp()
		{
			base.SetUp();
			(report, header) = CusExitReportTest.GetNewBusinessObject(Factory);
		}
		CusExitHeader header;
		CusExitReport report;

		class CusExitReportForTest : CusExitReport
		{
			public CusExitReportForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool CER_EnquiryInformationCode_ReadOnly => false;
		}
	}
}
