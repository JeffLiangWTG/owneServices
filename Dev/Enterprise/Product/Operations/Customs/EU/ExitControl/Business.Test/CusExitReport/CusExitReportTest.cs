using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusExitReport))]
sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
{
	public void TestCusExitReportItems()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(report.CusExitReportItems);
	}

	public void TestIDocumentSupportable() => CombineAssertions(() =>
	{
		var (report, _) = GetNewBusinessObject(Factory);
		Assert("CusExitReport implements IDocumentSupportable", report is IDocumentSupportable);
		AssertType<CusExitReportDocumentSupporter>(report.DocumentSupporter);
	});

	public void TestUseDifferentErrorReportKey()
	{
		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		goodsLocationAddress.E2_GovRegNumType = "123";
		var address = Factory.New<JobDocAddress>();
		address.E2_GovRegNumType = "123";
		goodsLocationAddress.Delete();
		address.Delete();
		_ = address.IsPassportIDGovRegNumType;
		AssertEquals("ErrorReporter.LastKeyReported", @"E.MasterFiles.B.JobDocAddress.E2_GovRegNumType()
E.MasterFiles.B.JobDocAddress.E2_GovRegNumType()
E.MasterFiles.B.JobDocAdd", ErrorReporter.LastKeyReported);
		_ = goodsLocationAddress.IsPassportIDGovRegNumType;
		AssertEquals("ErrorReporter.LastKeyReported", @"E.Customs.EU.ExitControl.B.CusGoodsLocationAddress.E2_GovRegNumType()
E.MasterFiles.B.JobDocAddress.E2_GovRegNumType()
E.Cu", ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
	}

	public void TestCusGoodsLocationIsDeletedCorrectly()
	{
		var header = Factory.New<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var reportMock = Factory.NewMoq<CusExitReport>();
		var protectedMock = reportMock.Protected();
		protectedMock.Setup<bool>("IsUCC6Core").Returns(true);
		var report = reportMock.Object;
		report.CER_CXH_Header = header.PK;
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		var goodsLocation = report.GoodsLocation;
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		report = newFactory.Load<CusExitReport>(report.PK);
		report.Delete();
		newFactory.Save();
		AssertEquals("goodsLocation.IsDeleted", true, goodsLocation.IsDeleted);
	}

	public void TestHumanReadableName()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		header.CXH_JobReference = "123";
		AssertEquals("Exit Report 123", report.HumanReadableName);
	}

	public void TestCER_IsFinalized_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_IsFinalizedInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Finalization", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Final", resourceStringData.ShortCaption);
		});
	}

	public void TestIsAdditionalInfosRequiredForReportAndItems()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
		Assert("IsAlternativeEvidenceRequired", !report.IsAdditionalInfosRequiredForReportAndItems);
		report.CER_Type = ExitReportTypeList.Codes.Presentation;
		Assert("IsAlternativeEvidenceRequired", report.IsAdditionalInfosRequiredForReportAndItems);
	}

	public void TestCER_EnquiryInformationCode()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var info = report.CER_EnquiryInformationCodeInfo;
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Enquiry Info.", resourceStringData.Caption);
			Assert("ReadOnly", info.ReadOnly);
			Assert("Caption", !report.IsCER_EnquiryInformationCodeRequired);
		});
	}

	public void TestOfficeCodeType()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals(typeof(AlternativeEvidence), ((Integration.Customs.ICusCodeDataTypeSupporter)report).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.AlternativeEvidence]);
	}

	public void TestAlternativeEvidences()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var evidence1 = report.AlternativeEvidences.AddNew();
		var evidence2 = report.AlternativeEvidences.AddNew();
		var evidence3 = report.AlternativeEvidences.AddNew();
		var evidence4 = report.AlternativeEvidences.AddNew();

		AssertContainsExactElementsInAnyOrder(new[] { evidence1.PK, evidence2.PK, evidence3.PK, evidence4.PK }, report.AlternativeEvidences.Select(x => x.PK));
		Assert("CY_Type", report.AlternativeEvidences.All(x => x.CY_Type == "AEV"));
	}

	public void TestIsAlternativeEvidenceRequired()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
		Assert("IsAlternativeEvidenceRequired", !report.IsAlternativeEvidenceRequired);
	}

	public void TestDeclarant()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var declarant = report.Declarant;
		AssertEquals("DocAddressType", DocAddressType.Declarant, declarant.DocAddressType);
	}

	public void TestDeclarantJobDocAddressRequirement()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var representativeJobDocAddressRequirement = report.DeclarantJobDocAddressRequirement;
		AssertEquals("DefaultDocAddressType", DocAddressType.Declarant, representativeJobDocAddressRequirement.DefaultDocAddressType);
		AssertEquals("DefaultContactType", ContactType.NotifyParty, representativeJobDocAddressRequirement.DefaultContactType);
	}

	public void TestRepresentative()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var representative = report.Representative;
		AssertEquals("DocAddressType", DocAddressType.Representative, representative.DocAddressType);
	}

	public void TestRepresentativeJobDocAddressRequirement()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var declarantJobDocAddressRequirement = report.RepresentativeJobDocAddressRequirement;
		AssertEquals("DefaultDocAddressType", DocAddressType.Representative, declarantJobDocAddressRequirement.DefaultDocAddressType);
		AssertEquals("DefaultContactType", ContactType.Administration, declarantJobDocAddressRequirement.DefaultContactType);
	}

	public void TestDocAddresses()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var declarant = report.Declarant;
		var representative = report.Representative;
		var docAddress = report.DocAddresses.AddNew();
		var addresses = new[] { declarant, representative, docAddress };
		AssertContainsExactElementsInAnyOrder("DocAddresses", addresses, report.DocAddresses.Cast<JobDocAddress>());
		report.Delete();
		Assert("Should have delete all JobDocAddress", addresses.All(x => x.IsDeleted));
	}

	public void TestTypeDecider()
	{
		AssertType<CusExitReportTypeDecider>(CusExitReport.TypeDecider);
	}

	public void TestCanDelete()
	{
		CombineAssertions(() =>
		{
			var report = GetNewBusinessObject(Factory).report;
			AssertEquals("Can delete a report with no messages", true, report.CanDelete);
			var testMessage = report.Messages.AddNew();
			AssertEquals("Report has a message sent", 1, report.Messages.Count);
			AssertEquals("Can not delete a report with a message attached", false, report.CanDelete);
		});
	}

	public void TestReasonForNotAbleToDelete()
	{
		var report = GetNewBusinessObject(Factory).report;
		AssertEquals("Exit Report cannot be deleted as it has already been sent to Customs.", report.ReasonForNotAbleToDelete);
	}

	public void TestDelete()
	{
		var report = GetNewBusinessObject(Factory).report;
		var item = report.CusExitReportItems.AddNew();

		report.Delete();
		AssertEquals("CusExitReportItem should have been deleted.", true, item.IsDeleted);
	}

	public void TestCountryCode()
	{
		var compnay = Factory.New<GlbCompany>();
		compnay.GC_RN_NKCountryCode = "K!";
		var report = Factory.New<CusExitReport>();
		AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, report.CountryCode);
		var header = Factory.New<CusExitHeader>();
		header.CXH_GC_Company = ZGuid.Invalid;
		report.CER_CXH_Header = header.PK;
		AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, report.CountryCode);
		header.CXH_GC_Company = compnay.PK;
		AssertEquals("K!", report.CountryCode);
	}

	public void TestICusExitReportCorrectlySetup()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var iReport = newFactory.Load<Integration.Customs.EUExitControl.ICusExitReport>(report.PK);
		AssertType<CusExitReport>(iReport);
	}

	public void TestHeader()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		AssertEquals(header.PK, report.Header.PK);
	}

	public void TestConsignment()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		var consignment = header.CusExitConsignments.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		AssertSame(consignment, report.Consignment);
	}

	public void TestCusExitReportItemsForBinding()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		AssertEquals("When no report items", 0, report.CusExitReportItemsForBinding.Count);

		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem2.CCI_LineNumber = 3;
		var reportItem1 = report.CusExitReportItems.AddNew();
		reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		var reportItem2 = report.CusExitReportItems.AddNew();
		reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		var reportItem3 = report.CusExitReportItems.AddNew();
		reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
		AssertEquals("Before resetting AdditionalFilter", 0, report.CusExitReportItemsForBinding.Count);

		report.ResetCusExitReportItemsForBindingAdditionalFilter();
		CombineAssertions("After resetting AdditionalFilter", () =>
		{
			AssertContainsExactElementsInAnyOrder("Items", new[] { reportItem1.PK, reportItem3.PK }, report.CusExitReportItemsForBinding.Select(x => x.PK));
			AssertContainsExactElementsInExactOrder("OrderBy", new[] { (ZShort)1, (ZShort)3 }, report.CusExitReportItemsForBinding.Select(x => x.ConsignmentItemLineNumber));
		});
	}

	public void TestCusExitReportItemPackages()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
		var package2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();

		var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem2.CCI_LineNumber = 2;
		var package3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();

		var reportItem1 = report.CusExitReportItems.AddNew();
		reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		reportItem1.ERI_CXP_Package = package.PK;

		var reportItem2 = report.CusExitReportItems.AddNew();
		reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		reportItem2.ERI_CXP_Package = package2.PK;

		var reportItem3 = report.CusExitReportItems.AddNew();
		reportItem3.ERI_CCI_ConsignmentItem = consignmentItem.PK;

		var reportItem4 = report.CusExitReportItems.AddNew();
		reportItem4.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
		reportItem4.ERI_CXP_Package = package3.PK;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Items", new[] { reportItem1.PK, reportItem2.PK, reportItem4.PK }, report.CusExitReportItemPackages.Select(x => x.PK));
			AssertContainsExactElementsInExactOrder("OrderBy", new[] { (ZShort)1, (ZShort)1, (ZShort)2 }, report.CusExitReportItemPackages.Select(x => x.ConsignmentItemLineNumber));
		});
	}

	public void TestCER_Calc_Discrepancies_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_Calc_DiscrepanciesInfo, (string[])null, "Discrepancies");
	}

	public void TestCER_Calc_Discrepancies_ReadOnlyAndClearance()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly", false, report.CER_Calc_DiscrepanciesInfo.ReadOnly);
		report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
		report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
		AssertEquals("report.CER_Calc_DiscrepanciesInfo.ReadOnly is true when CER_Type = 'ALT'", true, report.CER_Calc_DiscrepanciesInfo.ReadOnly);
		AssertEquals(ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies, report.CER_Behavior);
	}

	public void TestCER_Calc_Discrepancies()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		report.CER_Calc_Discrepancies = ZBool.True;
		AssertEquals(ExitReportDiscrepancyTypeList.Codes.Discrepancies, report.CER_Behavior);
		report.CER_Calc_Discrepancies = ZBool.False;
		AssertEquals(ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies, report.CER_Behavior);
	}

	public void TestCER_CXC_Consignment_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(report.CER_CXC_ConsignmentInfo, (string[])null, "Entry/Consignment");
	}

	public void TestCER_CXC_Consignment_ReadOnly()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("CER_Status not set, CER_MessageStatus != SNT", false, report.CER_CXC_ConsignmentInfo.ReadOnly);
		report.CER_MessageStatus = "SNT";
		AssertEquals("CER_MessageStatus = SNT", true, report.CER_CXC_ConsignmentInfo.ReadOnly);
		report.CER_MessageStatus = "ACC";
		AssertEquals("CER_MessageStatus != SNT", false, report.CER_CXC_ConsignmentInfo.ReadOnly);
		report.CER_Status = "ACC";
		AssertEquals("CER_Status != empty", true, report.CER_CXC_ConsignmentInfo.ReadOnly);
	}

	public void TestMessages()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var message1 = Factory.New<EDIMessage>();
		message1.EM_LinkedObject = report;
		var message2 = Factory.New<EDIMessage>();
		message2.EM_LinkedObject = report;
		AssertContainsExactElementsInAnyOrder(new[] { message1, message2 }, report.Messages.Cast<EDIMessage>());
	}

	public void TestCER_Type_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_TypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Report Type", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Rpt. Type", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "Type", resourceStringData.ShortCaption);
		});
	}

	public void TestCER_TransportMode_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_TransportModeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Mode of Transport", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Mode of Trans.", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "M.O.T.", resourceStringData.ShortCaption);
		});
	}

	public void TestCER_TransportType_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_TransportTypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transport Type", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Trans. Type", resourceStringData.MediumCaption);
		});
	}

	public void TestCER_TransportID_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_TransportIDInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transport ID", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Trans. ID", resourceStringData.MediumCaption);
		});
	}

	public void TestCER_RN_NKTransportNationality_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_RN_NKTransportNationalityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transport Nationality", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Trans. Nationality", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "Nationality", resourceStringData.ShortCaption);
		});
	}

	public void TestCER_Location_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_LocationInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Location", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Loc.", resourceStringData.MediumCaption);
		});
	}

	public void TestCER_DateTime_Caption() => CombineAssertions(() =>
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_DateTimeInfo);
		AssertEquals("not ucc6 Caption", "Date & Time", resourceStringData.Caption);
		AssertEquals("not ucc6 ShortCaption", "Date", resourceStringData.ShortCaption);

		report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(report.CER_DateTimeInfo, CusExitReport.Ucc6CaptionKey);
		AssertEquals("ucc6 Caption", "Arrival Date", resourceStringData.Caption);
		AssertEquals("ucc6 ShortCaption", "Arr. Date", resourceStringData.ShortCaption);
	});

	public void TestCER_OfficeOfExit_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("Office of Exit", DataBoundResourceStrings.GetDataForProperty(report.CER_OfficeOfExitInfo).Caption);
	}

	public void TestCER_Status_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(report.CER_StatusInfo).Caption);
	}

	public void TestCER_Status_ReadOnly()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals(true, report.CER_StatusInfo.ReadOnly);
	}

	public void TestStatusDescription_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReport), nameof(CusExitReport.StatusDescription));
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Status Description", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Status Desc.", resourceStringData.MediumCaption);
		});
	}

	public void TestTypeDescription_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReport), nameof(CusExitReport.TypeDescription));
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Report Type Description", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Type Description", resourceStringData.MediumCaption);
		});
	}

	public void TestTypeDescription()
	{
		CombineAssertions(() =>
		{
			(var report, var header) = GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("Exit Notification", ExitReportTypeList.Descriptions.ExitNotification, report.TypeDescription);

			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertEquals("Information on Non Exited", ExitReportTypeList.Descriptions.InformationOnNonExitedExport, report.TypeDescription);

			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			AssertEquals("Presentation", ExitReportTypeList.Descriptions.Presentation, report.TypeDescription);

			report.CER_Type = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, report.TypeDescription);
		});
	}

	public void TestStatusDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "130", "Customs Status 130", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "200", "Customs Status 200", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		CombineAssertions(() =>
		{
			var exitReportWithoutHeader = Factory.New<CusExitReport>();
			(var report, _) = GetNewBusinessObject(Factory);

			exitReportWithoutHeader.CER_Status = "130";
			report.CER_Status = "130";
			AssertEquals("Valid status", "Customs Status 130", exitReportWithoutHeader.StatusDescription);
			AssertEquals("Valid status", "Customs Status 130", report.StatusDescription);

			exitReportWithoutHeader.CER_Status = "200";
			report.CER_Status = "200";
			AssertEquals("Invalid status", ZString.Empty, exitReportWithoutHeader.StatusDescription);
			AssertEquals("Invalid status", ZString.Empty, report.StatusDescription);

			exitReportWithoutHeader.CER_Status = ZString.Empty;
			report.CER_Status = ZString.Empty;
			AssertEquals("Empty status", ZString.Empty, exitReportWithoutHeader.StatusDescription);
			AssertEquals("Empty status", ZString.Empty, report.StatusDescription);
		});
	}

	public void TestCER_MessageStatus_Caption()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(report.CER_MessageStatusInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Message Status", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Msg. Status", resourceStringData.MediumCaption);
		});
	}

	public void TestCER_MessageStatus_ReadOnly()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals(true, report.CER_MessageStatusInfo.ReadOnly);
	}

	public void TestMessageStatusDescription_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReport), nameof(CusExitReport.MessageStatusDescription));
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Message Status Description", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Msg. Status Description", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "Msg. Status Desc.", resourceStringData.ShortCaption);
		});
	}

	public void TestMessageStatusDescription()
	{
		CombineAssertions(() =>
		{
			(var report, _) = GetNewBusinessObject(Factory);
			report.CER_MessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("SNT", LogicalStatusList.Descriptions.Sent, report.MessageStatusDescription);

			report.CER_MessageStatus = LogicalStatusList.Codes.Error;
			AssertEquals("ERR", LogicalStatusList.Descriptions.Error, report.MessageStatusDescription);

			report.CER_MessageStatus = LogicalStatusList.Codes.Accepted;
			AssertEquals("ACC", LogicalStatusList.Descriptions.Accepted, report.MessageStatusDescription);

			report.CER_MessageStatus = LogicalStatusList.Codes.Failed;
			AssertEquals("FAL", LogicalStatusList.Descriptions.Failed, report.MessageStatusDescription);

			report.CER_MessageStatus = LogicalStatusList.Codes.Invalid;
			AssertEquals("INV", LogicalStatusList.Descriptions.Invalid, report.MessageStatusDescription);

			report.CER_MessageStatus = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, report.MessageStatusDescription);
		});
	}

	public void TestIsPackageRelatedToConsignmentItem()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		report.HasChanges = false;

		report.IsPackageRelatedToConsignmentItem = ZBool.True;
		AssertEquals("Suspend setting HasChanges", false, report.HasChanges);
	}

	public void TestIsPackageRelatedToConsignmentItem_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReport), nameof(CusExitReport.IsPackageRelatedToConsignmentItem));
		AssertEquals("Show only packages related to the item", resourceStringData.Caption);
	}

	public void TestIsPackageRelatedToConsignmentItem_DefaultChecked()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("Default true", true, report.IsPackageRelatedToConsignmentItem);
		report.IsPackageRelatedToConsignmentItem = false;
		AssertEquals("After setting", false, report.IsPackageRelatedToConsignmentItem);
	}

	public void TestUpdateAdditionalFilterForCusExitReportItemPackages()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var guid = ZGuid.NewZGuid();
		report.UpdateAdditionalFilterForCusExitReportItemPackages(guid);
		var filterString = report.CusExitReportItemPackages.CompleteFilter.GetAsWhereClause(true);
		AssertContains($"{CusExitReportItemSchema.Constants.ERI_CCI_ConsignmentItem} = CONVERT('{guid}', 'System.Guid')", filterString, true);
	}

	public void TestClearAdditionalFilterForCusExitReportItemPackages()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var guid = ZGuid.NewZGuid();
		report.UpdateAdditionalFilterForCusExitReportItemPackages(guid);

		report.ClearAdditionalFilterForCusExitReportItemPackages();
		var filterString = report.CusExitReportItemPackages.CompleteFilter.GetAsWhereClause(true);
		AssertNotContains($"{CusExitReportItemSchema.Constants.ERI_CCI_ConsignmentItem} =", filterString, true);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)report).GetCusSupportingInfoTypes();
		AssertEquals(1, supportingInfoTypes.Count);
		AssertEquals(typeof(AdditionalInfo), supportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestAdditionalInfos()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertType<AdditionalInfoCollection<AdditionalInfo>>(report.AdditionalInfos);
	}

	public void TestCusAuthorizationUsages()
	{
		(var item, _) = GetNewBusinessObject(Factory);
		var initialUsages = item.CusAuthorizationUsages;
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport>>("Type of CusAuthorizationUsageCollection.", initialUsages);
		AddNewAuthorization("123", "321");
		AddNewAuthorization("456", "654");
		Factory.Save();
		AssertEquals("Save authorization usages.", 2, initialUsages.Count);

		var newFactory = new BusinessObjectFactory();
		var loadedUsages = newFactory.Load<CusExitReport>(item.PK).CusAuthorizationUsages;
		AssertEquals("Number of loaded authorization usages equals to saved.", 2, loadedUsages.Count);
		Assert("Loaded items are equal to saved.", initialUsages.Any(x => loadedUsages.Any(y => x.PK == y.PK && x.AGC_Code == y.AGC_Code && x.AGC_Number == y.AGC_Number)));
		loadedUsages.Delete();
		Assert("All authorization usages are marked as deleted.", loadedUsages.All(x => x.IsDeleted));
		void AddNewAuthorization(string code, string number)
		{
			var authorizationUsage = initialUsages.AddNew();
			authorizationUsage.AGC_Code = code;
			authorizationUsage.AGC_Number = number;
		}
	}

	public void TestValidationModes()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertEquals("NONE for report", ValidationModes.None, report.ValidationModes);
	}

	public void TestSetReportBehaviorFromConsignmentItems()
	{
		var (consignment, header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.SetReportBehaviorFromConsignmentItems();
		AssertEquals("No consignment item selected", CusExitReportBehaviorList.Codes.STD, report.CER_Behavior);

		consignmentItem.CCI_Calc_ShouldReportItem = true;
		report.SetReportBehaviorFromConsignmentItems();
		AssertEquals("Has consignment item selected", CusExitReportBehaviorList.Codes.DIS, report.CER_Behavior);
	}

	public void TestDefaultDataFromParent_Shipment_WhenDeclarationExists()
	{
		var header = CusExitHeaderAbstractTest<CusExitHeader>.GetNewBusinessObject(Factory);

		var reportMock = Factory.NewMoq<CusExitReport>();
		reportMock.CallBase = true;
		var report = reportMock.Object;

		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		header.Parent = shipment;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var protectedMock = reportMock.Protected();
		protectedMock.Setup("DefaultDataFromShipment", true, shipment).Verifiable();
		header.CusExitReports.Add(report);

		report.DefaultDataFromParent();
		protectedMock.Verify("DefaultDataFromShipment", Times.Once(), shipment);

		Assert("DefaultDataFromParent() should call DefaultDataFromShipment()", true);
	}

	public void TestDefaultTransportTypeOnModeOfTransportChange()
	{
		CombineAssertions(() =>
		{
			var (report, _) = GetNewBusinessObject(Factory);
			report.CER_TransportType = "1";
			report.CER_TransportMode = "2";
			AssertEquals("No change", "1", report.CER_TransportType);
			foreach (var (motValue, expectedTransportType) in GetTestData())
			{
				report.CER_TransportMode = motValue;
				AssertEquals($"For CER_TransportMode: {motValue}", expectedTransportType, report.CER_TransportType);
			}
		});

		IEnumerable<(string motValue, string expectedTransportType)> GetTestData()
		{
			yield return (TransportTypeList.Codes.Air, CusExitReportTransportTypeList.Codes._40);
			yield return (TransportTypeList.Codes.FixedTransportInstallations, string.Empty);
			yield return (TransportTypeList.Codes.InlandWaterwayTransport, CusExitReportTransportTypeList.Codes._80);
			yield return (TransportTypeList.Codes.OwnPropulsion, string.Empty);
			yield return (TransportTypeList.Codes.Mail, string.Empty);
			yield return (TransportTypeList.Codes.Rail, CusExitReportTransportTypeList.Codes._21);
			yield return (TransportTypeList.Codes.Road, CusExitReportTransportTypeList.Codes._30);
			yield return (TransportTypeList.Codes.Sea, CusExitReportTransportTypeList.Codes._10);
		}
	}

	public void TestProcessHandlingInfo() => AssertType<CusExitReportProcessHandlingInfo>(Factory.New<CusExitReport>().ProcessHandlingInfo);

	public void TestIsUCC6() => CombineAssertions(() =>
	{
		var objectForTest = GetNewBusinessObject() as CusExitReport;
		AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

		AssertEquals("No Parent = not UCC6", Factory.New<CusExitReport>().IsUCC6, false);

		var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
	});

	public void TestLookups() => CombineAssertions(() =>
	{
		var objectForTest = GetNewBusinessObject() as CusExitReport;
		AssertType<CusExitReportLookups>("not ucc6", objectForTest.Lookups);

		var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertType<CusExitReportUcc6Lookups>("ucc6", ucc6ObjectForTest.Lookups);
	});

	public void TestValidation() => CombineAssertions(() =>
	{
		var objectForTest = GetNewBusinessObject() as CusExitReport;
		AssertType<CusExitReportValidation>("not ucc6", objectForTest.Validation);

		var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertType<CusExitReportValidation>("ucc6 - Validation Decider should be different, not validation class", ucc6ObjectForTest.Validation);
	});

	public void TestValidationDecider() => CombineAssertions(() =>
	{
		var item = GetNewBusinessObject() as CusExitReport;
		AssertNull("not ucc6", item.ValidationDecider);

		item = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertType<CusExitReportUcc6ValidationDecider>("ucc6", item.ValidationDecider);
	});

	public void TestIsAccepted() => CombineAssertions(() =>
	{
		var report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertIsAccepted(ZString.Empty, false);
		AssertIsAccepted("CLP", false);
		AssertIsAccepted("ERR", false);
		AssertIsAccepted("AAA", false);
		AssertIsAccepted("RCE", false);
		AssertIsAccepted(AESEntryStatusList.Codes.ReleasedForExit, true);
		AssertIsAccepted(AESEntryStatusList.Codes.ControlledForExit, true);
		AssertIsAccepted(AESEntryStatusList.Codes.Refused, true);

		void AssertIsAccepted(string status, bool expected)
		{
			report.CER_Status = status;
			var expectedMessagePart = expected ? "accepted" : "not accepted";
			AssertEquals($"With status {status} report is {expectedMessagePart}", expected, report.IsAccepted);
		}
	});

	public void TestIsSentOrAccepted() => CombineAssertions(() =>
	{
		var report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertIsSentOrAccepted(ZString.Empty, false, LogicalStatusList.Codes.Accepted);
		AssertIsSentOrAccepted(ZString.Empty, true, LogicalStatusList.Codes.Sent);
		AssertIsSentOrAccepted(ZString.Empty, false);
		AssertIsSentOrAccepted(AESEntryStatusList.Codes.ReleasedForExit, true);
		AssertIsSentOrAccepted("CLP", false);
		AssertIsSentOrAccepted(AESEntryStatusList.Codes.ControlledForExit, true);
		AssertIsSentOrAccepted("ERR", false);
		AssertIsSentOrAccepted(AESEntryStatusList.Codes.Refused, true);
		AssertIsSentOrAccepted("AAA", false);
		AssertIsSentOrAccepted("RCE", false);

		void AssertIsSentOrAccepted(string status, bool expected, string messageStatus = "")
		{
			report.CER_Status = status;
			report.CER_MessageStatus = messageStatus;
			var expectedMessagePart = expected ? "accepted" : "not sent or accepted";
			AssertEquals($"With status : [{status}] and message status : [{messageStatus}] report is {expectedMessagePart}", expected, report.IsSentOrAccepted);
		}
	});

	public void TestIsSentOrNotEmpty() => CombineAssertions(() =>
	{
		var report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		AssertIsSentOrNotEmpty(false, ZString.Empty, LogicalStatusList.Codes.Accepted);
		AssertIsSentOrNotEmpty(true, ZString.Empty, LogicalStatusList.Codes.Sent);
		AssertIsSentOrNotEmpty(false, ZString.Empty, ZString.Empty);
		AssertIsSentOrNotEmpty(true, "AAA", ZString.Empty);

		void AssertIsSentOrNotEmpty(bool expected, string status = "", string messageStatus = "")
		{
			report.CER_MessageStatus = messageStatus;
			report.CER_Status = status;
			AssertEquals($"With message status : [{messageStatus}] report and customs status : [{status}]", expected, report.IsSentOrNotEmpty);
		}
	});

	public void TestSetReadOnlyTab() => CombineAssertions(() =>
	{
		var report1 = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		var exitReportItem1 = report1.CusExitReportItems.AddNew();
		exitReportItem1.ERI_CCI_ConsignmentItem = ZGuid.NewZGuid();

		var report2 = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		var exitReportItem2 = report2.CusExitReportItems.AddNew();
		exitReportItem2.ERI_CCI_ConsignmentItem = ZGuid.NewZGuid();

		var additionalInfo1 = exitReportItem1.AdditionalInfos.AddNew();
		var additionalInfo2 = exitReportItem2.AdditionalInfos.AddNew();

		report1.CER_Calc_Discrepancies = false;
		report2.CER_Calc_Discrepancies = false;
		AssertEquals("when CER_Calc_Discrepancies is false in the first line, additionalInfo1 is readonly", true, additionalInfo1.CusExitReportItem.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false in the second line, additionalInfo2 is readonly", true, additionalInfo2.CusExitReportItem.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false in the first line, exitReportItem1 is readonly", true, exitReportItem1.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false in the second line, exitReportItem3 is readonly", true, exitReportItem2.ReadOnly);

		report1.CER_Calc_Discrepancies = true;
		AssertEquals("when CER_Calc_Discrepancies is true in the first line, additionalInfo1 is not readonly", false, additionalInfo1.CusExitReportItem.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false in the second line, additionalInfo2 is readonly", true, additionalInfo2.CusExitReportItem.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is true in the first line, exitReportItem1 is not readonly", false, exitReportItem1.ReadOnly);
		AssertEquals("when CER_Calc_Discrepancies is false in the second line, exitReportItem3 is readonly", true, exitReportItem2.ReadOnly);
	});

	public void TestMultipleKeysToUse() => CombineAssertions(() =>
	{
		AssertEquals("Ucc6 Length", Factory.GetUcc6ExitHeader().CusExitReports.AddNew().MultipleKeysToUse.Count, 1);
		AssertEquals("Ucc6", Factory.GetUcc6ExitHeader().CusExitReports.AddNew().MultipleKeysToUse[0], CusExitReport.Ucc6CaptionKey);

		AssertEquals("not Ucc6 Length", Factory.New<CusExitReport>().MultipleKeysToUse.Count, 1);
		AssertEquals("not Ucc6", Factory.New<CusExitReport>().MultipleKeysToUse[0], CusExitReport.NotUcc6CaptionKey);
	});

	public void TestCER_DateTime_ReadOnly() => CombineAssertions(() =>
	{
		AssertEquals("Ucc6", Factory.GetUcc6ExitHeader().CusExitReports.AddNew().CER_DateTimeInfo.ReadOnly, true);
		AssertEquals("not Ucc6", Factory.New<CusExitReport>().CER_DateTimeInfo.ReadOnly, false);
	});

	public void TestCER_TransportType_ReadOnly() => CombineAssertions(() =>
	{
		var report = Factory.New<CusExitReport>();
		report.CER_Calc_Discrepancies = true;
		AssertEquals("not Ucc6", false, report.CER_TransportTypeInfo.ReadOnly);

		report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		report.CER_Calc_Discrepancies = false;
		AssertEquals("Ucc6", true, report.CER_TransportTypeInfo.ReadOnly);

		report.CER_Calc_Discrepancies = true;
		AssertEquals("Ucc6 - not discrepancies", false, report.CER_TransportTypeInfo.ReadOnly);
	});

	public void TestCER_TransportID_ReadOnly() => CombineAssertions(() =>
	{
		var report = Factory.New<CusExitReport>();
		report.CER_Calc_Discrepancies = true;
		AssertEquals("not Ucc6", false, report.CER_TransportIDInfo.ReadOnly);

		report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		report.CER_Calc_Discrepancies = false;
		AssertEquals("Ucc6", true, report.CER_TransportIDInfo.ReadOnly);

		report.CER_Calc_Discrepancies = true;
		AssertEquals("Ucc6 - not discrepancies", false, report.CER_TransportIDInfo.ReadOnly);
	});

	public void TestCER_RN_NKTransportNationality_ReadOnly() => CombineAssertions(() =>
	{
		var report = Factory.New<CusExitReport>();
		report.CER_Calc_Discrepancies = true;
		AssertEquals("not Ucc6", false, report.CER_RN_NKTransportNationalityInfo.ReadOnly);

		report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		report.CER_Calc_Discrepancies = false;
		AssertEquals("Ucc6", true, report.CER_RN_NKTransportNationalityInfo.ReadOnly);

		report.CER_Calc_Discrepancies = true;
		AssertEquals("Ucc6 - not discrepancies", false, report.CER_RN_NKTransportNationalityInfo.ReadOnly);
	});

	public void TestGoodsLocationDescription_Caption() => CombineAssertions(() =>
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReport), nameof(CusExitReport.GoodsLocationDescription));

		AssertEquals("Caption", "Location of Goods", captionResourceString.Caption);
		AssertEquals("ShortCaption", "Location", captionResourceString.ShortCaption);
	});

	public void TestGoodsLocation() => CombineAssertions(() =>
	{
		var exitHeader = Factory.GetUcc6ExitHeader();
		var exitReport = exitHeader.CusExitReports.AddNew();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		exitReport.CER_CXC_Consignment = exitConsignment.PK;
		var goodsLocation = exitReport.GoodsLocation;

		AssertType<CusGoodsLocation>(goodsLocation);
		AssertEquals("CGL_ParentID", exitReport.PK, goodsLocation.CGL_ParentID);
		AssertEquals("CGL_ParentTableCode", CusExitReportSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
		AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.ExitControl, goodsLocation.CGL_LocationUse);
		AssertSame("Cached", goodsLocation, exitReport.GoodsLocation);
		AssertEquals("IsRegisteredEditableChildObject", true, exitReport.IsRegisteredEditableChildObject(goodsLocation));
	});

	public void TestGoodsLocationDescription() => CombineAssertions(() =>
	{
		var exitHeader = Factory.GetUcc6ExitHeader();
		var exitReport = exitHeader.CusExitReports.AddNew();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		exitReport.CER_CXC_Consignment = exitConsignment.PK;
		AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, exitReport.GoodsLocationDescription);

		var goodsLocation = exitReport.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
		AssertEquals("GoodsLocationDescription when there's GoodsLocation", "Z", exitReport.GoodsLocationDescription);
	});

	public void TestValidateGoodsLocationDescription()
	{
		var exitReport = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		exitReport.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		exitReport.ValidateGoodsLocationDescription();
		AssertHasMessageError(exitReport.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		var exitReport = GetNewBusinessObject(Factory).report;
		AssertEquals("LVEXTC", (exitReport as ICusGoodsLocationProvider).ProviderKey);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).report;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).report;

	public static (CusExitReport report, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = CusExitHeaderAbstractTest<CusExitHeader>.GetNewBusinessObject(factory);
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		return (report, header);
	}
}
