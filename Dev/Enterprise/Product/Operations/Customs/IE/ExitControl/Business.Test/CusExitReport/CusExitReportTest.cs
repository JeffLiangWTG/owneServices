using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using CusGoodsLocationTypeList = Enterprise.Customs.Business.CusGoodsLocationTypeList;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReport))]
	sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCER_OfficeOfExitIsClearedifNeeded()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			report.CER_OfficeOfExit = "EX1";
			report.CER_EnquiryInformationCode = IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.WillNotExit;
			AssertEquals("ExitNotification - 1", "EX1", report.CER_OfficeOfExit);
			report.CER_EnquiryInformationCode = "!";
			AssertEquals("ExitNotification - !", "EX1", report.CER_OfficeOfExit);

			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			report.CER_OfficeOfExit = "EX1";
			report.CER_EnquiryInformationCode = IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.WillNotExit;
			AssertEquals("Office of Exit not allowed for Enquiry Information Code 1 - unset", ZString.Empty, report.CER_OfficeOfExit);

			report.CER_OfficeOfExit = "EX1";
			report.CER_EnquiryInformationCode = IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExpectedToExit;
			AssertEquals("Office of Exit not allowed for Enquiry Information Code 2 - unset", ZString.Empty, report.CER_OfficeOfExit);

			report.CER_OfficeOfExit = "EX1";
			report.CER_EnquiryInformationCode = IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedNoAlternativeEvidence;
			AssertEquals("Office of Exit required for Enquiry Information Code 3", "EX1", report.CER_OfficeOfExit);
		}

		public void TestCER_OfficeOfExport_ReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			foreach (var type in new[] { string.Empty, ExitReportTypeList.Codes.ExitNotification, ExitReportTypeList.Codes.Presentation, "!@" })
			{
				report.CER_Type = type;
				Assert("Office of Export should be readonly", report.CER_OfficeOfExportInfo.ReadOnly);
			}
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			Assert("Office of Export should be editable", !report.CER_OfficeOfExportInfo.ReadOnly);
		}

		public void TestIsOrganisationFieldsRequired()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = "ALT";
			Assert("ALT", report.IsOrganisationFieldsRequired);
			report.CER_Type = "PRE";
			Assert("PRE", !report.IsOrganisationFieldsRequired);
		}

		public void TestIsTransportFieldsRequired()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = "PRE";
			report.CER_Behavior = "DIS";
			AssertEquals("PRE, DIS", true, report.IsTransportFieldsRequired);
			report.CER_Type = "PRE";
			report.CER_Behavior = "STD";
			AssertEquals("PRE, STD", true, report.IsTransportFieldsRequired);
			report.CER_Type = "EXT";
			report.CER_Behavior = "DIS";
			AssertEquals("EXT, DIS", false, report.IsTransportFieldsRequired);
		}

		public void TestAlternativeEvidences()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<AlternativeEvidenceCollection<AlternativeEvidence>>(report.AlternativeEvidences);
		}

		public void TestCER_EnquiryInformationCode_ReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_EnquiryInformationCodeInfo.ReadOnly", true, report.CER_EnquiryInformationCodeInfo.ReadOnly);
			AssertEquals("report.IsCER_EnquiryInformationCodeRequired", false, report.IsCER_EnquiryInformationCodeRequired);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_EnquiryInformationCodeInfo.ReadOnly should be false", false, report.CER_EnquiryInformationCodeInfo.ReadOnly);
			AssertEquals("report.IsCER_EnquiryInformationCodeRequired should be true", true, report.IsCER_EnquiryInformationCodeRequired);
		}

		public void TestCER_AdditionalDeclarationType_DefaultValue()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			foreach (var type in new[] { string.Empty, ExitReportTypeList.Codes.Presentation, ExitReportTypeList.Codes.InformationOnNonExitedExport, "!@" })
			{
				report.CER_Type = type;
				AssertEquals(type, ZString.Empty, report.CER_AdditionalDeclarationType);
			}
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("ExitNotification Default Value", EU.Business.EntrySubStyleList.Codes.NormalDeclaration, report.CER_AdditionalDeclarationType);
			report.CER_AdditionalDeclarationType = "!";
			AssertEquals("ExitNotification Current Value", "!", report.CER_AdditionalDeclarationType);
		}

		public void TestCER_AdditionalDeclarationType_ReadOnly()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("ExitNotification", false, report.CER_AdditionalDeclarationTypeInfo.ReadOnly);
			foreach (var type in new[] { string.Empty, ExitReportTypeList.Codes.Presentation, ExitReportTypeList.Codes.InformationOnNonExitedExport, "!@" })
			{
				report.CER_Type = type;
				AssertEquals(type, true, report.CER_AdditionalDeclarationTypeInfo.ReadOnly);
			}
		}

		public void TestCER_AdditionalDeclarationType_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_AdditionalDeclarationTypeInfo, (string[])null, "Additional Declaration Type", mediumCaption: "Add. Dec. Type");
		}

		public void TestGetContainersOrEquipment()
		{
			(var report, var consignment1, var header) = GetNewBusinessObject(Factory);
			var container1 = header.CusExitContainers.AddNew();
			container1.CXN_IsEquipment = true;
			var container2 = header.CusExitContainers.AddNew();
			var container3 = header.CusExitContainers.AddNew();

			var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
			var pivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			pivot1.CNP_CXN_Container = container1.PK;
			var package1 = pivot1.Package;

			var consignment2 = header.CusExitConsignments.AddNew();
			var consignmentItem2 = consignment2.CusExitConsignmentItems.AddNew();
			var pivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			pivot2.CNP_CXN_Container = container2.PK;
			var package2 = pivot2.Package;

			var consignmentItem3 = consignment2.CusExitConsignmentItems.AddNew();
			var pivot3 = consignmentItem3.CusExitConsignmentPackagePivots.AddNew();
			pivot3.CNP_CXN_Container = container1.PK;
			var package3 = pivot3.Package;

			var item1 = report.CusExitReportItems.AddNew();
			item1.ERI_CCI_ConsignmentItem = consignmentItem1.PK;
			item1.ERI_CXP_Package = package1.PK;
			var item2 = report.CusExitReportItems.AddNew();
			item2.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			item2.ERI_CXP_Package = package2.PK;
			var item3 = report.CusExitReportItems.AddNew();
			item3.ERI_CCI_ConsignmentItem = consignmentItem3.PK;
			item3.ERI_CXP_Package = package3.PK;

			var containersOrEquipments = report.GetContainersOrEquipments();

			CombineAssertions(() =>
			{
				AssertEquals("container3 not linked to report", 2, containersOrEquipments.Length);
				AssertSame("container1", container1, containersOrEquipments[0].container);
				var consignmentItems1 = containersOrEquipments[0].consignmentItems;
				AssertEquals("2 linked consignmentItems", 2, consignmentItems1.Length);
				AssertSame("consignmentItem1", consignmentItem1, consignmentItems1[0]);
				AssertSame("consignmentItem3", consignmentItem3, consignmentItems1[1]);

				AssertSame("container2", container2, containersOrEquipments[1].container);
				var consignmentItems2 = containersOrEquipments[1].consignmentItems;
				AssertEquals("1 linked consignmentItems", 1, consignmentItems2.Length);
				AssertSame("consignmentItem2", consignmentItem2, consignmentItems2[0]);
			});
		}

		public void TestIsAlternativeEvidenceRequired()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("IsAlternativeEvidenceRequired", true, report.IsAlternativeEvidenceRequired);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("IsAlternativeEvidenceRequired", false, report.IsAlternativeEvidenceRequired);
		}

		public void TestIsInformationOnNonExitedExport()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport.ToUpper();
			AssertEquals(true, report.IsInformationOnNonExitedExport);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals(false, report.IsInformationOnNonExitedExport);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport.ToLower();
			AssertEquals(true, report.IsInformationOnNonExitedExport);
		}

		public void TestIsPresentation()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.Presentation.ToUpper();
			AssertEquals(true, report.IsPresentation);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals(false, report.IsPresentation);
			report.CER_Type = ExitReportTypeList.Codes.Presentation.ToLower();
			AssertEquals(true, report.IsPresentation);
		}

		public void TestIsExitNotification()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification.ToUpper();
			AssertEquals(true, report.IsExitNotification);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals(false, report.IsExitNotification);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification.ToLower();
			AssertEquals(true, report.IsExitNotification);
		}

		public void TestIsDiscrepancies()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies.ToUpper();
			AssertEquals(true, report.IsDiscrepancies);
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
			AssertEquals(false, report.IsDiscrepancies);
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies.ToLower();
			AssertEquals(true, report.IsDiscrepancies);
		}

		public void TestCER_Calc_Discrepancies_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_Calc_DiscrepanciesInfo, (string[])null, "Discrepancies");
		}

		public void TestCER_Calc_Discrepancies_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly", false, report.CER_Calc_DiscrepanciesInfo.ReadOnly);
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly is true when CER_Type = 'ALT'", true, report.CER_Calc_DiscrepanciesInfo.ReadOnly);
			AssertEquals(ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies, report.CER_Behavior);
		}

		public void TestCER_Calc_Discrepancies()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Calc_Discrepancies = ZBool.True;
			AssertEquals(ExitReportDiscrepancyTypeList.Codes.Discrepancies, report.CER_Behavior);
			report.CER_Calc_Discrepancies = ZBool.False;
			AssertEquals(ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies, report.CER_Behavior);
		}

		public void TestCER_OfficeOfExit_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_OfficeOfExitInfo, (string[])null, "Office of Exit");
		}

		public void TestCER_DateTimet_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_DateTimeInfo, (string[])null, "Exit/Arrival Date");
		}

		public void TestCER_Calc_FormattedDateTime_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_Calc_FormattedDateTimeInfo, (string[])null, "Exit/Arrival Date");
		}

		public void TestCER_Calc_FormattedDateTime()
		{
			CombineAssertions(() =>
			{
				(var report, _, _) = GetNewBusinessObject(Factory);
				report.CER_Type = ExitReportTypeList.Codes.Presentation;
				report.CER_DateTime = new ZDateTimeOffset(2022, 10, 18, 1, 25, 36, TimeSpan.FromHours(11));
				AssertEquals("PRE - Format", "18-Oct-22 01:25", report.CER_Calc_FormattedDateTime);
				report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
				AssertEquals("ALT - Format", "18-Oct-22", report.CER_Calc_FormattedDateTime);
				report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
				AssertEquals("EXT - Format", "18-Oct-22", report.CER_Calc_FormattedDateTime);
				report.CER_Calc_FormattedDateTime = "19-NOV-23";
				ZDateTime.TryParseExact("19-NOV-23", out var result, ZDateTime.ShortDateFormat);
				AssertEquals("EXT - CER_DateTime", new ZDateTimeOffset(result), report.CER_DateTime);
				report.CER_Type = ExitReportTypeList.Codes.Presentation;
				report.CER_Calc_FormattedDateTime = "25-DEC-21 15:45";
				ZDateTime.TryParseExact("25-DEC-21 15:45", out result, ZDateTime.LongTimeFormat);
				AssertEquals("PRE - CER_DateTime", new ZDateTimeOffset(result), report.CER_DateTime);
				report.CER_Calc_FormattedDateTime = "BLADS";
				ZDateTime.TryParseExact("25-DEC-21 15:45", out result, ZDateTime.LongTimeFormat);
				AssertEquals("Invalid - CER_DateTime", ZDateTimeOffset.Empty, report.CER_DateTime);
			});
		}

		public void TestCER_Calc_FormattedDateTime_FieldType()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("PRE - CER_DateTime_FieldType", nameof(ZArchitecture.FieldType.DateTime), report.CER_Calc_FormattedDateTime_FieldType);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("INF - CER_DateTime_FieldType", nameof(ZArchitecture.FieldType.Date), report.CER_Calc_FormattedDateTime_FieldType);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("EXT - CER_DateTime_FieldType", nameof(ZArchitecture.FieldType.Date), report.CER_Calc_FormattedDateTime_FieldType);
		}

		public void TestCER_Calc_TypeOfLocation_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_Calc_TypeOfLocationInfo, (string[])null, "Type of Location");
		}

		public void TestCER_Calc_TypeOfLocation_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Calc_TypeOfLocation = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_Calc_TypeOfLocationInfo.ReadOnly", false, report.CER_Calc_TypeOfLocationInfo.ReadOnly);
			AssertEquals(CusGoodsLocationTypeList.Codes.DesignatedLocation, report.CER_Calc_TypeOfLocation);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_Calc_TypeOfLocationInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Calc_TypeOfLocation);
			report.CER_Calc_TypeOfLocation = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_Calc_TypeOfLocationInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Calc_TypeOfLocation);
		}

		public void TestCER_Calc_UNLOCO_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_Calc_UNLOCOInfo, (string[])null, "UNLOCO");
		}

		public void TestCER_Calc_UNLOCO_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Calc_UNLOCO = "AUSYD";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_Calc_UNLOCOInfo.ReadOnly", false, report.CER_Calc_UNLOCOInfo.ReadOnly);
			AssertEquals("AUSYD", report.CER_Calc_UNLOCO);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_Calc_UNLOCOInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_Calc_UNLOCOInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Calc_UNLOCO);
			report.CER_Calc_UNLOCO = "AUSYD";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_Calc_UNLOCOInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_Calc_UNLOCOInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Calc_UNLOCO);
		}

		public void TestCER_Location_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_LocationInfo, (string[])null, "Arrival Notification Place", mediumCaption: "Arr. Notif. Place");
		}

		public void TestCER_Location_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Location = "A";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_LocationInfo.ReadOnly", false, report.CER_LocationInfo.ReadOnly);
			AssertEquals("A", report.CER_Location);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_LocationInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_LocationInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Location);
			report.CER_Location = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_LocationInfo.ReadOnly is true when CER_Type != 'PRE'", true, report.CER_LocationInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_Location);
		}

		public void TestCER_TransportMode_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_TransportMode = "A";
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_TransportModeInfo.ReadOnly", false, report.CER_TransportModeInfo.ReadOnly);
			AssertEquals("A", report.CER_TransportMode);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			AssertEquals("report.CER_TransportModeInfo.ReadOnly is true when not (CER_Type = 'ALT' AND CER_Behavior = 'DIS')", true, report.CER_TransportModeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportMode);
			report.CER_TransportMode = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_TransportModeInfo.ReadOnly is true when not (CER_Type = 'EXT' AND CER_Behavior = 'DIS')", true, report.CER_TransportModeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportMode);
			report.CER_TransportMode = "A";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			report.CER_TransportMode = "A";
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
			AssertEquals("report.CER_TransportModeInfo.ReadOnly is false when not (CER_Type = 'PRE' AND CER_Behavior = 'STD')", false, report.CER_TransportModeInfo.ReadOnly);
		}

		public void TestCER_TransportType_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_TransportType = "A";
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_TransportTypeInfo.ReadOnly", false, report.CER_TransportTypeInfo.ReadOnly);
			AssertEquals("A", report.CER_TransportType);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_TransportTypeInfo.ReadOnly is true when not (CER_Type = 'ALT' AND CER_Behavior = 'DIS')", true, report.CER_TransportTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportType);
			report.CER_TransportType = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_TransportTypeInfo.ReadOnly is true when not (CER_Type = 'EXT' AND CER_Behavior = 'DIS')", true, report.CER_TransportTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportType);
			report.CER_TransportType = "A";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
			AssertEquals("report.CER_TransportTypeInfo.ReadOnly is false when not (CER_Type = 'PRE' AND CER_Behavior = 'STD')", false, report.CER_TransportTypeInfo.ReadOnly);
		}

		public void TestCER_TransportID_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_TransportID = "A";
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_TransportIDInfo.ReadOnly", false, report.CER_TransportIDInfo.ReadOnly);
			AssertEquals("A", report.CER_TransportID);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_TransportIDInfo.ReadOnly is true when not (CER_Type = 'ALT' AND CER_Behavior = 'DIS')", true, report.CER_TransportIDInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportID);
			report.CER_TransportID = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_TransportIDInfo.ReadOnly is true when not (CER_Type = 'EXT' AND CER_Behavior = 'DIS')", true, report.CER_TransportIDInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_TransportID);
			report.CER_TransportID = "A";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
			AssertEquals("report.CER_TransportIDInfo.ReadOnly is false when not (CER_Type = 'PRE' AND CER_Behavior = 'STD')", false, report.CER_TransportIDInfo.ReadOnly);
		}

		public void TestCER_RN_NKTransportNationality_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_RN_NKTransportNationality = "A";
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_RN_NKTransportNationalityInfo.ReadOnly", false, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
			AssertEquals("A", report.CER_RN_NKTransportNationality);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_RN_NKTransportNationalityInfo.ReadOnly is true when not (CER_Type = 'ALT' AND CER_Behavior = 'DIS')", true, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_RN_NKTransportNationality);
			report.CER_RN_NKTransportNationality = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_RN_NKTransportNationalityInfo.ReadOnly is true when not (CER_Type = 'EXT' AND CER_Behavior = 'DIS')", true, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_RN_NKTransportNationality);
			report.CER_RN_NKTransportNationality = "A";
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
			AssertEquals("report.CER_RN_NKTransportNationalityInfo.ReadOnly is false when not (CER_Type = 'PRE' AND CER_Behavior = 'STD')", false, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
		}

		public void TestCusExitReportItems()
		{
			AssertType<CusExitReportItemCollection<CusExitReportItem>>(GetNewBusinessObject(Factory).report.CusExitReportItems);
		}

		public void TestCusExitReportItemPackages()
		{
			AssertType<CusExitReportItemCollection<CusExitReportItem>>(GetNewBusinessObject(Factory).report.CusExitReportItemPackages);
		}

		public void TestAdditonalInfos()
		{
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(GetNewBusinessObject(Factory).report.AdditionalInfos);
		}

		public void TestGoodsLocation()
		{
			AssertType<CusGoodsLocation>(GetNewBusinessObject(Factory).report.GoodsLocation);
		}

		public void TestCER_DeclarantType_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_DeclarantTypeInfo, (string[])null, "Representation Status", mediumCaption: "Rep. Status");
		}

		public void TestCER_DeclarantType_ReadOnlyAndClearance()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_DeclarantType = "A";
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.CER_DeclarantTypeInfo.ReadOnly", false, report.CER_DeclarantTypeInfo.ReadOnly);
			AssertEquals("A", report.CER_DeclarantType);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.CER_DeclarantTypeInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.CER_DeclarantTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_DeclarantType);
			report.CER_DeclarantType = "A";
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.CER_DeclarantTypeInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.CER_DeclarantTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, report.CER_DeclarantType);
		}

		public void TestDeclarantOrgPK_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.DeclarantOrgPKInfo, (string[])null, "Declarant");
		}

		public void TestDeclarantOrgPK_ReadOnlyAndClearance()
		{
			var org = Factory.New<OrgHeader>();
			(var report, _, _) = GetNewBusinessObject(Factory);
			var value = org.PK;
			report.DeclarantOrgPK = value;
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.DeclarantOrgPKInfo.ReadOnly", false, report.DeclarantOrgPKInfo.ReadOnly);
			AssertEquals(value, report.DeclarantOrgPK);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.DeclarantOrgPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.DeclarantOrgPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.DeclarantOrgPK);
			report.DeclarantOrgPK = value;
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.DeclarantOrgPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.DeclarantOrgPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.DeclarantOrgPK);
		}

		public void TestDeclarantAddressPK_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.DeclarantAddressPKInfo, (string[])null, "Declarant Address");
		}

		public void TestDeclarantAddressPK_ReadOnlyAndClearance()
		{
			var org = Factory.New<OrgHeader>();
			(var report, _, _) = GetNewBusinessObject(Factory);
			var value = org.MainAddress.PK;
			report.DeclarantAddressPK = value;
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.DeclarantAddressPKInfo.ReadOnly", false, report.DeclarantAddressPKInfo.ReadOnly);
			AssertEquals(value, report.DeclarantAddressPK);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.DeclarantAddressPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.DeclarantAddressPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.DeclarantAddressPK);
			report.DeclarantAddressPK = value;
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.DeclarantAddressPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.DeclarantAddressPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.DeclarantAddressPK);
		}

		public void TestRepresentativeOrgPK_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.RepresentativeOrgPKInfo, (string[])null, "Representative");
		}

		public void TestRepresentativeOrgPK_ReadOnlyAndClearance()
		{
			var org = Factory.New<OrgHeader>();
			(var report, _, _) = GetNewBusinessObject(Factory);
			var value = org.PK;
			report.RepresentativeOrgPK = value;
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.RepresentativeOrgPKInfo.ReadOnly", false, report.RepresentativeOrgPKInfo.ReadOnly);
			AssertEquals(value, report.RepresentativeOrgPK);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.RepresentativeOrgPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.RepresentativeOrgPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.RepresentativeOrgPK);
			report.RepresentativeOrgPK = value;
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.RepresentativeOrgPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.RepresentativeOrgPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.RepresentativeOrgPK);
		}

		public void TestRepresentativeAddressPK_Caption()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.RepresentativeAddressPKInfo, (string[])null, "Representative Address");
		}

		public void TestRepresentativeAddressPK_ReadOnlyAndClearance()
		{
			var org = Factory.New<OrgHeader>();
			(var report, _, _) = GetNewBusinessObject(Factory);
			var value = org.MainAddress.PK;
			report.RepresentativeAddressPK = value;
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("report.RepresentativeAddressPKInfo.ReadOnly", false, report.RepresentativeAddressPKInfo.ReadOnly);
			AssertEquals(value, report.RepresentativeAddressPK);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("report.RepresentativeAddressPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.RepresentativeAddressPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.RepresentativeAddressPK);
			report.RepresentativeAddressPK = value;
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("report.RepresentativeAddressPKInfo.ReadOnly is true when CER_Type != 'ALT'", true, report.RepresentativeAddressPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, report.RepresentativeAddressPK);
		}

		public void TestIMessageAttacheeMembers()
		{
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			(var report, var consignment, var header) = GetNewBusinessObject(Factory);
			header.CXH_GB_Branch = branch.PK;
			report.CER_MessageStatus = LogicalStatusList.Codes.Accepted;
			report.CER_Status = AESEntryStatusList.Codes.DiversionRequestRejected;
			consignment.CXC_MovementReference = "MRN1234";
			var cusAgent = Factory.New<GlbStaff>();
			cusAgent.GS_Code = "!2#";
			header.CXH_GS_NKCustomsAgent = "!2#";
			IMessageAttachee messageAttachee = report;
			CombineAssertions(() =>
			{
				AssertEquals("Branch", branch, messageAttachee.Branch);
				AssertEquals("CustomsAgent", cusAgent, messageAttachee.CustomsAgent);
				AssertEquals("RelatedJob", header, messageAttachee.RelatedJob);
				AssertEquals("LogicalStatus", LogicalStatusList.Codes.Accepted, messageAttachee.LogicalStatus);
				AssertEquals("EntryStatus", AESEntryStatusList.Codes.DiversionRequestRejected, messageAttachee.EntryStatus);
				AssertEquals("MovementReferenceNumber", "MRN1234", messageAttachee.MovementReferenceNumber);
			});
		}

		public void TestMessageStatusDescription()
		{
			(var report, _, var header) = GetNewBusinessObject(Factory);
			CombineAssertions(() =>
			{
				report.CER_MessageStatus = IELogicalStatusList.Codes.Sent;
				AssertEquals("SNT", IELogicalStatusList.Descriptions.Sent, report.MessageStatusDescription);

				report.CER_MessageStatus = IELogicalStatusList.Codes.Invalid;
				AssertEquals("INV", IELogicalStatusList.Descriptions.Invalid, report.MessageStatusDescription);

				report.CER_MessageStatus = IELogicalStatusList.Codes.SeeEntries;
				AssertEquals("MLT", IELogicalStatusList.Descriptions.SeeEntries, report.MessageStatusDescription);

				report.CER_MessageStatus = "XYZ";
				AssertEquals("XYZ", "Unknown", report.MessageStatusDescription);

				report.CER_MessageStatus = ZString.Empty;
				AssertEquals("Empty", ZString.Empty, report.MessageStatusDescription);
			});
		}

		public void TestStatusDescription()
		{
			(var report, _, var header) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				report.CER_Status = AESEntryStatusList.Codes.AmendmentRequested;
				AssertEquals("AMR", AESEntryStatusList.Descriptions.AmendmentRequested, report.StatusDescription);

				report.CER_Status = AESEntryStatusList.Codes.ReleasedForExport;
				AssertEquals("REL", AESEntryStatusList.Descriptions.ReleasedForExport, report.StatusDescription);

				report.CER_Status = AESEntryStatusList.Codes.ReleasedForExit;
				AssertEquals("EXR", AESEntryStatusList.Descriptions.ReleasedForExit, report.StatusDescription);

				report.CER_Status = "XYZ";
				AssertEquals("XYZ", "Unknown", report.StatusDescription);

				report.CER_Status = ZString.Empty;
				AssertEquals("Empty", ZString.Empty, report.StatusDescription);
			});
		}

		public void TestLookupsTypeList()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			var lookups = report.Lookups;
			var list = lookups.TypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("List", new[] { "EXT", "ALT", "PRE" }, list.GetAllCodes());
				AssertSame("Cached", list, report.Lookups.TypeList);
			});
		}

		public void TestValidation_Default()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ZString.Empty;
			AssertType<CusExitReportValidation>(report.Validation);
		}

		public void TestValidation_Presentation()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertType<PresentationCusExitReportValidation>(report.Validation);
		}

		public void TestValidation_ExitNotification()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertType<ExitNotificationCusExitReportValidation>(report.Validation);
		}

		public void TestLookups()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportLookups>(report.Lookups);
		}

		public void TestICusExitReportCorrectlySetup()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var iReport = newFactory.Load<Integration.Customs.IEExitControl.ICusExitReport>(report.PK);
			AssertType<CusExitReport>(iReport);
		}

		public void TestFetchStrategyType()
		{
			var report = GetNewBusinessObject(Factory).report;
			AssertType<CusExitReportFetchStrategy>("IE has CusExitReportFetchStrategy", report.FetchStrategy);
		}

		public void TestDelete()
		{
			var report = GetNewBusinessObject(Factory).report;
			var location = Factory.New<CusGoodsLocation>();
			location.CGL_ParentID = report.PK;

			report.Delete();
			AssertEquals("CusGoodsLocation should have been deleted.", true, location.IsDeleted);
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			AssertEquals(typeof(CusGoodsLocation), (report as ICusGoodsLocationTypeSupporter).GoodsLocationType);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var euHeader = Factory.New<EU.ExitControl.Business.CusExitHeader>();
				var euReport = euHeader.CusExitReports.AddNew();
				AssertEquals("We should implement it in EU.ExitControl and add CusGoodsLocationTypeDecider there", false, euReport is ICusGoodsLocationTypeSupporter);
			}
		}

		public void TestGetSupportingDocSendingObject()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			var docSendingObject = report.GetSupportingDocSendingObject();
			AssertType<DocumentSendingObject>(docSendingObject);
		}

		public void TestDocManagerSupports()
		{
			(var report, _, _) = GetNewBusinessObject(Factory);
			var docManagerSupports = report.DocManagerSupports;
			AssertContainsExactElementsInAnyOrder(new[] { report.Header }, docManagerSupports);

			report.Header.Parent = Factory.New<IE.Business.Declaration.JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new[] { report.Header, report.Header.Parent }, docManagerSupports);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

		public static (CusExitReport report, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var (header, consignment) = CusExitConsignmentTest.GetNewBusinessObject(factory);
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Today;
			report.CER_OfficeOfExit = "DE001";
			return (report, consignment, header);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new CusExitReportLightValidationTester(bizObjToTest);

		class CusExitReportLightValidationTester : LightValidationTester
		{
			public CusExitReportLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != AutoCusExitHeader.Schema.CXH_ParentID;
			}
		}
	}
}
