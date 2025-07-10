using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationHeader))]
sealed class DeclarationActivationHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestGetNewValidation()
	{
		AssertType<DeclarationActivationHeaderValidation>(DeclarationActivationHeader.Validation);
	}

	public void TestCXH_OwnerReference() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CXH_OwnerReferenceInfo, caption: "Owners Reference", shortCaption: "Owners Ref.");
		AssertEquals("MaxLength", 35, DeclarationActivationHeader.CXH_OwnerReferenceInfo.MaxLength);
	});

	public void TestCXH_GS_NKCustomsAgent()
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CXH_GS_NKCustomsAgentInfo, caption: "Broker");
	}

	public void TestCER_TransportMode() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_TransportModeInfo, caption: "Transport Mode", shortCaption: "Trans. Mode");

		DeclarationActivationHeader.Report.CER_TransportMode = "X";
		AssertEquals("Getter delegate to CER_TransportMode", "X", DeclarationActivationHeader.CER_TransportMode);
		DeclarationActivationHeader.CER_TransportMode = "Y";
		AssertEquals("Setter delegate to CER_TransportMode", "Y", DeclarationActivationHeader.Report.CER_TransportMode);
	});

	public void TestCER_TransportType() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_TransportTypeInfo, caption: "Type of ID", shortCaption: "Typ. of ID");

		DeclarationActivationHeader.Report.CER_TransportType = "X";
		AssertEquals("Getter delegate to CER_TransportType", "X", DeclarationActivationHeader.CER_TransportType);
		DeclarationActivationHeader.CER_TransportType = "Y";
		AssertEquals("Setter delegate to CER_TransportType", "Y", DeclarationActivationHeader.Report.CER_TransportType);
	});

	public void TestCER_CustomsOfficeOfExport() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_OfficeOfExportInfo, caption: "Customs Office", shortCaption: "Customs Off.");

		DeclarationActivationHeader.Report.CER_OfficeOfExport = "X";
		AssertEquals("Getter delegate to CER_OfficeOfExport", "X", DeclarationActivationHeader.CER_OfficeOfExport);
		DeclarationActivationHeader.CER_OfficeOfExport = "Y";
		AssertEquals("Setter delegate to CER_OfficeOfExport", "Y", DeclarationActivationHeader.Report.CER_OfficeOfExport);
	});

	public void TestCER_Location() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_LocationInfo, caption: "Goods Location", shortCaption: "Goods Loc.");

		DeclarationActivationHeader.Report.CER_Location = "X";
		AssertEquals("Getter delegate to CER_Location", "X", DeclarationActivationHeader.CER_Location);
		DeclarationActivationHeader.CER_Location = "Y";
		AssertEquals("Setter delegate to CER_Location", "Y", DeclarationActivationHeader.Report.CER_Location);
	});

	public void TestCER_AdditionalDeclarationType() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_AdditionalDeclarationTypeInfo, caption: "Declaration Time Code", shortCaption: "Decl. Time Code");

		DeclarationActivationHeader.Report.CER_AdditionalDeclarationType = "X";
		AssertEquals("Getter delegate to CER_AdditionalDeclarationType", "X", DeclarationActivationHeader.CER_AdditionalDeclarationType);
		DeclarationActivationHeader.CER_AdditionalDeclarationType = "Y";
		AssertEquals("Setter delegate to CER_AdditionalDeclarationType", "Y", DeclarationActivationHeader.Report.CER_AdditionalDeclarationType);
	});

	public void TestCER_MessageStatus() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.CER_MessageStatus), caption: "Message Status", shortCaption: "Msg. Status");

		DeclarationActivationHeader.Report.CER_MessageStatus = "X";
		AssertEquals("Getter delegate to CER_MessageStatus", "X", DeclarationActivationHeader.CER_MessageStatus);
	});

	public void TestMessageStatusDescription() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.MessageStatusDescription), caption: "Message Status Description", shortCaption: "Msg. Status Desc.");

		DeclarationActivationHeader.Report.CER_MessageStatus = CHLogicalStatusList.Codes.Accepted;
		AssertEquals("Getter delegate to Report.MessageStatusDescription", CHLogicalStatusList.Descriptions.Accepted, DeclarationActivationHeader.MessageStatusDescription);
	});

	public void TestCER_Status() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.CER_Status), caption: "Customs Status", shortCaption: "Cust. Status");

		DeclarationActivationHeader.Report.CER_Status = "X";
		AssertEquals("Getter delegate to CER_Status", "X", DeclarationActivationHeader.CER_Status);
	});

	public void TestCustomsStatusDescription() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.CustomsStatusDescription), caption: "Customs Status Description", shortCaption: "Cust. Status Desc.");

		DeclarationActivationHeader.Report.CER_Status = AdditionalCHEntryStatusList.Codes.Active;
		AssertEquals("Getter delegate to Report.CustomsStatusDescription", AdditionalCHEntryStatusList.Descriptions.Active, DeclarationActivationHeader.CustomsStatusDescription);
	});

	public void TestCER_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_RN_NKTransportNationalityInfo, caption: "Nationality", shortCaption: "Nat.");

		DeclarationActivationHeader.Report.CER_RN_NKTransportNationality = "X";
		AssertEquals("Getter delegate to CER_RN_NKTransportNationality", "X", DeclarationActivationHeader.CER_RN_NKTransportNationality);
		DeclarationActivationHeader.CER_RN_NKTransportNationality = "Y";
		AssertEquals("Setter delegate to CER_RN_NKTransportNationality", "Y", DeclarationActivationHeader.Report.CER_RN_NKTransportNationality);
	});

	public void TestCER_TransportID() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_TransportIDInfo, caption: "Transport ID", shortCaption: "Trans. ID");

		DeclarationActivationHeader.Report.CER_TransportID = "X";
		AssertEquals("Getter delegate to CER_TransportID", "X", DeclarationActivationHeader.CER_TransportID);
		DeclarationActivationHeader.CER_TransportID = "Y";
		AssertEquals("Setter delegate to CER_TransportID", "Y", DeclarationActivationHeader.Report.CER_TransportID);
	});

	public void TestCER_Type() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CER_TypeInfo, caption: "Activation Type", shortCaption: "Act. Type");

		DeclarationActivationHeader.Report.CER_Type = "X";
		AssertEquals("Getter delegate to CER_Type", "X", DeclarationActivationHeader.CER_Type);
		DeclarationActivationHeader.CER_Type = "Y";
		AssertEquals("Setter delegate to CER_Type", "Y", DeclarationActivationHeader.Report.CER_Type);
	});

	public void TestTypeDescription() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.TypeDescription), caption: "Activation Type Description", shortCaption: "Act. Type Desc.");

		DeclarationActivationHeader.Report.CER_Type = DeclarationActivationHeader.Report.Lookups.ActivationTypeList[0].Code;
		AssertEquals("Getter delegate to Report.TypeDescription", ActivationTypeList.Descriptions.Edec, DeclarationActivationHeader.TypeDescription);
	});

	public void TestCXC_MovementReference() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.MovementReferenceInfo, caption: "Goods Declaration Ref. Nr.", shortCaption: "GDRN");

		DeclarationActivationHeader.Consignment.CXC_MovementReference = "X";
		AssertEquals("Getter delegate to CXC_MovementReference", "X", DeclarationActivationHeader.CXC_MovementReference);
		DeclarationActivationHeader.CXC_MovementReference = "Y";
		AssertEquals("Setter delegate to CXC_MovementReference", "Y", DeclarationActivationHeader.Consignment.CXC_MovementReference);
	});

	public void TestCXC_ReferenceNumber() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CXC_ReferenceNumberInfo, caption: "Air Waybill");

		DeclarationActivationHeader.Consignment.CXC_ReferenceNumber = "X";
		AssertEquals("Getter delegate to CXC_ReferenceNumber", "X", DeclarationActivationHeader.CXC_ReferenceNumber);
		DeclarationActivationHeader.CXC_ReferenceNumber = "Y";
		AssertEquals("Setter delegate to CXC_ReferenceNumber", "Y", DeclarationActivationHeader.Consignment.CXC_ReferenceNumber);
	});

	public void TestNextProcedure() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.NextProcedureInfo, caption: "Next Procedure", shortCaption: "Next Proc.");

		DeclarationActivationHeader.Report.NextProcedure = "X";
		AssertEquals("Getter delegate to NextProcedure", "X", DeclarationActivationHeader.NextProcedure);
		DeclarationActivationHeader.NextProcedure = "Y";
		AssertEquals("Setter delegate to NextProcedure", "Y", DeclarationActivationHeader.Report.NextProcedure);
	});

	public void TestCommunicationLanguage() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.CommunicationLanguageInfo, caption: "Language", shortCaption: "Lang.");

		DeclarationActivationHeader.Report.CommunicationLanguage = "X";
		AssertEquals("Getter delegate to CommunicationLanguage", "X", DeclarationActivationHeader.CommunicationLanguage);
		DeclarationActivationHeader.CommunicationLanguage = "Y";
		AssertEquals("Setter delegate to CommunicationLanguage", "Y", DeclarationActivationHeader.Report.CommunicationLanguage);
	});

	public void TestEdecOrginalTraderUID() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(DeclarationActivationHeader.EdecOriginalTraderUIDInfo, caption: "Original Trader UID", shortCaption: "Orig. Trader UID");

		DeclarationActivationHeader.Report.EdecOriginalTraderUID = "X";
		AssertEquals("Getter delegate to EdecOriginalTraderUID", "X", DeclarationActivationHeader.EdecOriginalTraderUID);
		DeclarationActivationHeader.EdecOriginalTraderUID = "Y";
		AssertEquals("Setter delegate to EdecOriginalTraderUID", "Y", DeclarationActivationHeader.Report.EdecOriginalTraderUID);
	});

	public void TestEdecOrginalTraderUID_default() => CombineAssertions(() =>
	{
		var exporterOrg1 = CreateOrganisation("EXP1", "UID1");
		var exporterOrg2 = CreateOrganisation("EXP2", "UID2");

		DeclarationActivationHeader.CXH_OH_Exporter = exporterOrg1.PK;
		AssertEquals("Default from Exporter", "UID1", DeclarationActivationHeader.EdecOriginalTraderUID);

		DeclarationActivationHeader.EdecOriginalTraderUID = ZString.Empty;
		DeclarationActivationHeader.CXH_OH_Exporter = exporterOrg2.PK;
		AssertEquals("Changed when Exporter changed", "UID2", DeclarationActivationHeader.EdecOriginalTraderUID);

		DeclarationActivationHeader.CXH_OH_Exporter = exporterOrg1.PK;
		AssertEquals("Not changed when not empty", "UID2", DeclarationActivationHeader.EdecOriginalTraderUID);

		DeclarationActivationHeader.EdecOriginalTraderUID = ZString.Empty;
		DeclarationActivationHeader.CXH_OH_Exporter = ZGuid.Empty;
		AssertEquals("No error when exporter is empty", ZString.Empty, DeclarationActivationHeader.EdecOriginalTraderUID);

		OrgHeader CreateOrganisation(string code, string uid)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, uid, Core.Constants.CountryCodes.Switzerland);
			return orgHeader;
		}
	});

	public void TestCXH_JobReference() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.CXH_JobReference), caption: "Job Number", shortCaption: "Job #");

		_ = DeclarationActivationHeader;
		Factory.Save();
		AssertEquals("1st", "DA00000001", DeclarationActivationHeader.CXH_JobReference);

		var declarationActivationHeader2 = CreateDeclarationActivationHeader(Factory);
		Factory.Save();
		AssertEquals("2nd", "DA00000002", declarationActivationHeader2.CXH_JobReference);
	});

	public void TestOnSavingForDelete() => CombineAssertions(() =>
	{
		var header = Factory.New<DeclarationActivationHeader>();
		var consignmentPK = header.Consignment.PK;
		var reportPK = header.Report.PK;
		Factory.Save();

		var newFactory1 = new BusinessObjectFactory();
		AssertNotNull("Consignment saved", newFactory1.Load<DeclarationActivationConsignment>(consignmentPK));
		AssertNotNull("Report saved", newFactory1.Load<DeclarationActivationReport>(reportPK));

		header.Delete();
		Factory.Save();

		var newFactory2 = new BusinessObjectFactory();
		AssertNull("Consignment deleted", newFactory2.Load<DeclarationActivationConsignment>(consignmentPK));
		AssertNull("Report deleted", newFactory2.Load<DeclarationActivationReport>(reportPK));
	});

	public void TestExporterCode() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.ExporterCode), caption: "Exporter/Client Code", shortCaption: "Exporter/Cli. Code");

		var orgExporter = Factory.New<OrgHeader>();
		orgExporter.OH_Code = "EXP1";
		AssertEquals("Empty", ZString.Empty, DeclarationActivationHeader.ExporterCode);
		DeclarationActivationHeader.CXH_OH_Exporter = orgExporter.PK;
		AssertEquals("Not empty", "EXP1", DeclarationActivationHeader.ExporterCode);
	});

	public void TestExporterName() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<DeclarationActivationHeader>(nameof(DeclarationActivationHeader.ExporterName), caption: "Exporter/Client Name", shortCaption: "Exporter/Cli. Name");

		var orgExporter = Factory.New<OrgHeader>();
		orgExporter.OH_FullName = "Exporter 1";
		AssertEquals("Empty", ZString.Empty, DeclarationActivationHeader.ExporterName);
		DeclarationActivationHeader.CXH_OH_Exporter = orgExporter.PK;
		AssertEquals("Not empty", "Exporter 1", DeclarationActivationHeader.ExporterName);
	});

	public void TestNextProcedureDescription()
	{
		new RefDataTestHelper(Factory).CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure).CreateCode("12").WithDescription("Desc12").Save();

		DeclarationActivationHeader.Report.NextProcedure = "12";
		AssertEquals("Desc12", DeclarationActivationHeader.NextProcedureDescription);
	}

	public void TestConsignment() => CombineAssertions(() =>
	{
		var header = Factory.New<DeclarationActivationHeader>();
		AssertNotNull("Consignment created", header.Consignment);
		AssertEquals("CXC_CXH_Header", header.PK, header.Consignment.CXC_CXH_Header);
		Factory.Save();
		var loadedHeader = new BusinessObjectFactory().Load<DeclarationActivationHeader>(header.PK);
		AssertEquals("Consignment reloaded", header.Consignment.PK, loadedHeader.Consignment.PK);
	});

	public void TestReport() => CombineAssertions(() =>
	{
		var header = Factory.New<DeclarationActivationHeader>();
		AssertNotNull("Consignment created", header.Report);
		AssertEquals("CER_CXH_Header", header.PK, header.Report.CER_CXH_Header);
		AssertEquals("CER_CXH_Header", header.Consignment.PK, header.Report.CER_CXC_Consignment);
		Factory.Save();
		var loadedHeader = new BusinessObjectFactory().Load<DeclarationActivationHeader>(header.PK);
		AssertEquals("Report reloaded", header.Report.PK, loadedHeader.Report.PK);
	});

	public void TestDateOfValuation() => CombineAssertions(() =>
	{
		AssertNotEquals("Pre-condition: Report has date", ZDateTime.Empty, DeclarationActivationHeader.DateOfValuation);
		AssertEquals("Getter delegated to Report.DateOfValuation", DeclarationActivationHeader.Report.DateOfValuation, DeclarationActivationHeader.DateOfValuation);
	});

	public void TestSetDefaults() => CombineAssertions(() =>
	{
		GlbStaff.CurrentUser.GS_Code = "B26";

		var header = Factory.New<DeclarationActivationHeader>();
		AssertEquals("CXH_ApplicationCode", CusExitHeaderApplicationCodeList.Codes.CHDeclarationActivation, header.CXH_ApplicationCode);
		AssertEquals("CXH_GS_NKCustomsAgent", "B26", header.CXH_GS_NKCustomsAgent);
	});

	protected override BusinessObject GetNewBusinessObject() => CreateDeclarationActivationHeader(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateDeclarationActivationHeader(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateDeclarationActivationHeader(Factory);

	DeclarationActivationHeader DeclarationActivationHeader => declarationActivationHeader ??= CreateDeclarationActivationHeader(Factory);
	DeclarationActivationHeader declarationActivationHeader;

	DeclarationActivationHeader CreateDeclarationActivationHeader(BusinessObjectFactory factory)
	{
		var header = factory.New<DeclarationActivationHeader>();
		_ = header.Consignment;
		_ = header.Report;
		return header;
	}
}
