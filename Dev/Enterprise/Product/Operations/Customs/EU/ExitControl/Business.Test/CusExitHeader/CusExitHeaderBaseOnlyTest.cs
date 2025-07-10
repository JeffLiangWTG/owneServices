using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusExitHeader))]
sealed class CusExitHeaderBaseOnlyTest : CusExitHeaderAbstractTest<CusExitHeader>
{
	public void TestValidation()
	{
		AssertType<CusExitHeaderValidation>(exitHeader.Validation);
	}

	public void TestLookups()
	{
		AssertType<CusExitHeaderLookups>(exitHeader.Lookups);
	}

	public void TestCusExitReports()
	{
		AssertType<ExitControlBase.Business.CusExitReportCollection<CusExitReport>>(exitHeader.CusExitReports);
	}

	public void TestCusExitContainers()
	{
		AssertType<ExitControlBase.Business.CusExitContainerCollection<CusExitContainer>>(exitHeader.CusExitContainers);
	}

	public void TestCusExitConsignments()
	{
		AssertType<ExitControlBase.Business.CusExitConsignmentCollection<CusExitConsignment>>(exitHeader.CusExitConsignments);
	}

	public void TestCusExitConsignmentPackages()
	{
		AssertType<ExitControlBase.Business.CusExitConsignmentPackageCollection<CusExitConsignmentPackage>>(exitHeader.CusExitConsignmentPackages);
	}

	public void TestIDocumentSupportable() => CombineAssertions(() =>
	{
		var header = Factory.New<CusExitHeader>();
		Assert("CusExitHeader implements IDocumentSupportable", header is IDocumentSupportable);
		AssertType<CusExitHeaderDocumentSupporter>(header.DocumentSupporter);
	});

	public void TestReferenceNumber() => CombineAssertions(() =>
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No Consignments", "", header.ReferenceNumber);

		header.CusExitConsignments.AddNew().CXC_ReferenceNumber = "A1";
		header.CusExitConsignments.AddNew().CXC_ReferenceNumber = "A1";

		AssertEquals("All same", "A1", header.ReferenceNumber);

		header.CusExitConsignments.AddNew().CXC_ReferenceNumber = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.ReferenceNumber);

		header.CusExitConsignments.AddNew().CXC_ReferenceNumber = "B1";

		AssertEquals("Multiple", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.ReferenceNumber);
	});

	public void TestUniqueConsignmentReference() => CombineAssertions(() =>
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No Consignments", "", header.UniqueConsignmentReference);

		header.CusExitConsignments.AddNew().CXC_UniqueConsignmentReference = "A1";
		header.CusExitConsignments.AddNew().CXC_UniqueConsignmentReference = "A1";

		AssertEquals("All same", "A1", header.UniqueConsignmentReference);

		header.CusExitConsignments.AddNew().CXC_UniqueConsignmentReference = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.UniqueConsignmentReference);

		header.CusExitConsignments.AddNew().CXC_UniqueConsignmentReference = "B1";

		AssertEquals("Multiple", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.UniqueConsignmentReference);
	});

	public void TestConsignment() => CombineAssertions(() =>
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No reports", "", header.Consignment);

		var c1 = header.CusExitConsignments.AddNew();
		c1.CXC_MovementReference = "Ref1";
		var c2 = header.CusExitConsignments.AddNew();
		c2.CXC_MovementReference = "Ref1";

		header.CusExitReports.AddNew().CER_CXC_Consignment = c1.PK;
		header.CusExitReports.AddNew().CER_CXC_Consignment = c2.PK;

		AssertEquals("All same", "MRN:Ref1", header.Consignment);

		header.CusExitReports.AddNew();

		AssertEquals("All same with empty", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.Consignment);
		var c3 = header.CusExitConsignments.AddNew();
		c3.CXC_LocalReference = "Ref1";
		header.CusExitReports.AddNew().CER_CXC_Consignment = c3.PK;

		AssertEquals("Multiple", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.Consignment);
	});

	public void TestStatus()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No reports", "", header.Status);

		header.CusExitReports.AddNew().CER_Status = "A1";
		header.CusExitReports.AddNew().CER_Status = "A1";

		AssertEquals("All same", "A1", header.Status);

		var report = header.CusExitReports.AddNew();
		report.CER_Status = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Codes.MultipleEntryStatus, header.Status);

		report.CER_Status = "B1";

		AssertEquals("Multiple", CommonEntryStatusList.Codes.MultipleEntryStatus, header.Status);
	}

	public void TestStatusDescription()
	{
		var header = Factory.New<CusExitHeader>();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "130", "Customs Status 130", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "131", "Customs Status 130", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "200", "Customs Status 200", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		AssertEquals("No reports", "", header.StatusDescription);

		header.CusExitReports.AddNew().CER_Status = "130";
		header.CusExitReports.AddNew().CER_Status = "130";

		AssertEquals("All same", "Customs Status 130", header.StatusDescription);

		var report = header.CusExitReports.AddNew();
		report.CER_Status = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.StatusDescription);

		report.CER_Status = "200";

		AssertEquals("All same with unknown", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.StatusDescription);

		report.CER_Status = "131";

		AssertEquals("Multiple", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.StatusDescription);
	}

	public void TestMessageStatus()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No reports", "", header.MessageStatus);

		header.CusExitReports.AddNew().CER_MessageStatus = "A1";
		header.CusExitReports.AddNew().CER_MessageStatus = "A1";

		AssertEquals("All same", "A1", header.MessageStatus);

		var report = header.CusExitReports.AddNew();
		report.CER_MessageStatus = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Codes.MultipleEntryStatus, header.MessageStatus);

		report.CER_MessageStatus = "B1";

		AssertEquals("Multiple", CommonEntryStatusList.Codes.MultipleEntryStatus, header.MessageStatus);
	}

	public void TestMessageStatusDescription()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("No reports", "", header.MessageStatusDescription);

		header.CusExitReports.AddNew().CER_MessageStatus = LogicalStatusList.Codes.Sent;
		header.CusExitReports.AddNew().CER_MessageStatus = LogicalStatusList.Codes.Sent;

		AssertEquals("All same", LogicalStatusList.Descriptions.Sent, header.MessageStatusDescription);

		var report = header.CusExitReports.AddNew();
		report.CER_MessageStatus = "";

		AssertEquals("All same with empty", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.MessageStatusDescription);

		report.CER_MessageStatus = "XXX";

		AssertEquals("All same with unknown", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.MessageStatusDescription);

		report.CER_MessageStatus = LogicalStatusList.Codes.Failed;

		AssertEquals("Multiple", CommonEntryStatusList.Descriptions.MultipleEntryStatus, header.MessageStatusDescription);
	}

	public void TestCarrierZAddressWithContact()
	{
		var header = Factory.New<CusExitHeader>();
		AssertNotNull(header.CarrierZAddressWithContact);
	}

	public void TestTransportModeConverter()
	{
		var header = Factory.New<CusExitHeader>();
		AssertType<TransportModeTranslator>(header.TransportModeTranslator);
	}

	public void TestCXH_JobReferenceReadOnly()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("defualt", false, header.CXH_JobReferenceInfo.ReadOnly);

		var declartion = Factory.New<JobDeclaration>();
		header.CXH_ParentID = declartion.PK;
		header.CXH_ParentTableCode = "JE";
		AssertEquals("When the exit header is plugged into parent, the CXH_JobReference should be parent's JobReference and readonly", true, header.CXH_JobReferenceInfo.ReadOnly);
	}

	public void TestHumanReadableName()
	{
		exitHeader.CXH_JobReference = "123";
		AssertEquals("Exit Control 123", exitHeader.HumanReadableName);
	}

	public void TestIRelatedJobMembers()
	{
		var headerMock = Factory.NewMoq<CusExitHeader>();
		var header = headerMock.Object;
		var lookupsMock = new Mock<ExitControlBase.Business.CusExitHeaderLookups>(header);
		headerMock.Protected().Setup<ExitControlBase.Business.CusExitHeaderLookups>("GetNewLookups").Returns(lookupsMock.Object);
		header.CXH_JobReference = "ABC123";
		IRelatedJob relatedJob = header;
		CombineAssertions(() =>
		{
			AssertEquals("relatedJob.JobNumber", "ABC123", relatedJob.JobNumber);
			AssertEquals("relatedJob.JobDescription", "Exit Control ABC123", relatedJob.JobDescription);
			AssertEquals("relatedJob.ControllerID", ControllerIDs.Customs.EU.ExitControl, relatedJob.ControllerID);
			AssertEquals("relatedJob.BusinessObjectPK", header.PK.ToGuid(), relatedJob.BusinessObjectPK);
		});
	}

	public void TestTypeDecider()
	{
		AssertType<CusExitHeaderTypeDecider>(CusExitHeader.TypeDecider);
	}

	public void TestCarrier_Caption()
	{
		AssertEquals("Carrier", DataBoundResourceStrings.GetDataForProperty(typeof(CusExitHeader), nameof(CusExitHeader.CXH_OA_Carrier)).Caption);
	}

	public void TestCarrierCode_Caption()
	{
		AssertEquals("Carrier", DataBoundResourceStrings.GetDataForProperty(typeof(CusExitHeader), nameof(CusExitHeader.CarrierCode)).Caption);
	}

	public void TestExporterCode_Caption()
	{
		AssertEquals("Exporter", DataBoundResourceStrings.GetDataForProperty(typeof(CusExitHeader), nameof(CusExitHeader.ExporterCode)).Caption);
	}

	public void TestCarrierCode()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG123";

		CombineAssertions(() =>
		{
			AssertEquals("CXH_OA_Carrier null", string.Empty, exitHeader.CarrierCode);

			exitHeader.CXH_OA_Carrier = orgHeader.MainAddress.PK;
			AssertEquals("CXH_OA_Carrier set", "ORG123", exitHeader.CarrierCode);
		});
	}

	public void TestExporterCode()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG123";

		CombineAssertions(() =>
		{
			AssertEquals("CXH_OH_Exporter null", string.Empty, exitHeader.ExporterCode);

			exitHeader.CXH_OH_Exporter = orgHeader.PK;
			AssertEquals("CXH_OH_Exporter set", "ORG123", exitHeader.ExporterCode);
		});
	}

	public void TestCarrierName()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG123";
		orgHeader.OH_FullName = "Test Carrier";

		CombineAssertions(() =>
		{
			AssertEquals("CXH_OA_Carrier null", string.Empty, exitHeader.CarrierName);

			exitHeader.CXH_OA_Carrier = orgHeader.MainAddress.PK;
			AssertEquals("CXH_OA_Carrier set", "Test Carrier", exitHeader.CarrierName);
		});
	}

	public void TestCXH_OA_Carrier_ZAddress_DefaultAddressType()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG123";
		orgHeader.OH_FullName = "Test Carrier";
		exitHeader.CXH_OA_Carrier = orgHeader.MainAddress.PK;
		AssertEquals("Carrier - default address type", exitHeader.CXH_OA_Carrier_ZAddress.DefaultAddressType, AddressType.OFC);
	}

	public void TestExporterName()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG123";
		orgHeader.OH_FullName = "Test Carrier";

		CombineAssertions(() =>
		{
			AssertEquals("CXH_OH_Exporter null", string.Empty, exitHeader.ExporterName);

			exitHeader.CXH_OH_Exporter = orgHeader.PK;
			AssertEquals("CXH_OH_Exporter set", "Test Carrier", exitHeader.ExporterName);
		});
	}

	public void TestBranchCode()
	{
		var branch = Factory.New<GlbBranch>();
		branch.GB_Code = "TST";

		exitHeader.CXH_GB_Branch = Guid.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("CXH_GB_Branch is not set", string.Empty, exitHeader.BranchCode);

			exitHeader.CXH_GB_Branch = branch.PK;
			AssertEquals("CXH_GB_Branch is set", "TST", exitHeader.BranchCode);
		});
	}

	public void TestBranchName()
	{
		var branch = Factory.New<GlbBranch>();
		branch.GB_Code = "TST";
		branch.GB_BranchName = "Test Branch 1";

		exitHeader.CXH_GB_Branch = Guid.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("CXH_GB_Branch is not set", string.Empty, exitHeader.BranchName);

			exitHeader.CXH_GB_Branch = branch.PK;
			AssertEquals("CXH_GB_Branch is set", "Test Branch 1", exitHeader.BranchName);
		});
	}

	public void TestBroker()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		exitHeader.CXH_GS_NKCustomsAgent = "ABC";
		AssertEquals("Equality check : Values should be equal", exitHeader.CXH_GS_NKCustomsAgent, exitHeader.Broker);
	}

	public void TestBroker_Captions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Broker", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerInfo).Caption);
			AssertEquals("Broker", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerInfo).MediumCaption);
			AssertEquals("Broker", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerInfo).ShortCaption);
		});
	}

	public void TestBrokerName()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "XYZ";
		staff.GS_FullName = "XYZName";
		Factory.Save();

		CombineAssertions(() =>
		{
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = "XYZ";
			AssertEquals("Broker with matching name exist", "XYZName", exitHeader.BrokerName);

			exitHeader.CXH_GS_NKCustomsAgent = "ABC";
			AssertNotEquals("Broker with matching name does not exist", "ABCName", exitHeader.BrokerName);
		});
	}

	public void TestBrokerName_Captions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Broker Name", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerNameInfo).Caption);
			AssertEquals("Broker Name", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerNameInfo).MediumCaption);
			AssertEquals("Broker Name", DataBoundResourceStrings.GetDataForProperty(exitHeader.BrokerNameInfo).ShortCaption);
		});
	}

	public void TestCXH_JobReference_Caption()
	{
		AssertEquals("Job Number", DataBoundResourceStrings.GetDataForProperty(exitHeader.CXH_JobReferenceInfo).Caption);
	}

	public void TestIsAutoLogged()
	{
		AssertEquals(true, exitHeader.IsAutoAdminBusinessObjectLoggerEnabled);
	}

	public void TestParent()
	{
		CombineAssertions(() =>
		{
			AssertNull("Parent", exitHeader.Parent);

			var jobDeclaration1 = Factory.New<JobDeclaration>();
			exitHeader.CXH_ParentID = jobDeclaration1.PK;
			exitHeader.CXH_ParentTableCode = jobDeclaration1.TablePrefix;
			AssertSame("Parent after setting CXH_ParentID and CXH_ParentTableCode", jobDeclaration1, exitHeader.Parent);

			var jobDeclaration2 = Factory.New<JobDeclaration>();
			exitHeader.Parent = jobDeclaration2;
			AssertEquals("CXH_ParentID after setting Parent", jobDeclaration2.PK, exitHeader.CXH_ParentID);
			AssertEquals("CXH_ParentTableCode after setting Parent", jobDeclaration2.TablePrefix, exitHeader.CXH_ParentTableCode);
		});
	}

	public void TestParent_ExitReportsMarkAsNeedingValidation()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		var exitReport = exitHeader.CusExitReports.AddNew();
		CombineAssertions(() =>
		{
			exitReport.MarkLightValidationAsValidForTesting();

			exitHeader.Parent = Factory.New<DummyBusinessObject>();
			AssertEquals(expected: false, exitReport.LightValidationIsValid);
		});
	}

	public void TestDeclaration()
	{
		CombineAssertions(() =>
		{
			AssertNull("Declaration", exitHeader.Declaration);

			var declaration = Factory.New<JobDeclaration>();
			exitHeader.Parent = declaration;
			AssertEquals("Declaration when CXH_ParentTableCode = 'JE'", declaration, exitHeader.Declaration);

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.Parent = shipment;
			AssertNull("Declaration when CXH_ParentTableCode <> 'JE'", exitHeader.Declaration);
		});
	}

	public void TestShipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Parent", null, exitHeader.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.Parent = shipment;
			AssertEquals("Parent is Shipment", shipment, exitHeader.Shipment);

			exitHeader.Parent = Factory.New<JobDeclaration>();
			AssertEquals("Parent is not Shipment", null, exitHeader.Shipment);
		});
	}

	public void TestCountryCode()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;

		CombineAssertions(() =>
		{
			exitHeader.CXH_GC_Company = company.PK;
			AssertEquals("CountryCode when Company isn't null", Core.Constants.CountryCodes.France, exitHeader.CountryCode);

			exitHeader.CXH_GC_Company = Guid.Empty;
			AssertEquals("CountryCode when Company is null", Core.Constants.CountryCodes.Latvia, exitHeader.CountryCode);
		});
	}

	public void TestSetDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CXH_GB_Branch", GlbBranch.CurrentBranch.PK, exitHeader.CXH_GB_Branch);
			AssertEquals("CXH_GC_Company", GlbBranch.CurrentBranch.GB_GC, exitHeader.CXH_GC_Company);
			AssertEquals("CXH_ApplicationCode", "XIT", exitHeader.CXH_ApplicationCode);
		});
	}

	public void TestExitHeaderJobReferenceAssignedOnSaving()
	{
		exitHeader.CXH_JobReference = ZString.Empty;
		Factory.Save();
		Assert("Reference number starts with E", exitHeader.CXH_JobReference.StartsWith("E"));
	}

	public void TestExitHeaderJobReferenceAssignedOnSaving_PlugIntoDeclaration()
	{
		var declartion = Factory.New<JobDeclaration>();
		declartion.JE_DeclarationReference = "DEC";
		var header = Factory.New<CusExitHeader>();
		header.CXH_ParentID = declartion.PK;
		header.CXH_ParentTableCode = "JE";
		Factory.Save();
		AssertEquals("Reference number set as JE_DeclarationReference", "DEC", header.CXH_JobReference);
	}

	public void TestExitHeaderJobReferenceAssignedOnSaving_PlugIntoShipment()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_UniqueConsignRef = "SHP";
		var header = Factory.New<CusExitHeader>();
		header.CXH_ParentID = shipment.PK;
		header.CXH_ParentTableCode = "JS";
		Factory.Save();
		AssertEquals("Reference number set as JS_UniqueConsignRef", "SHP", header.CXH_JobReference);
	}

	public void TestExitHeaderJobReferenceAssignedOnSaving_PlugIntoConsol()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_UniqueConsignRef = "CON";
		var header = Factory.New<CusExitHeader>();
		header.CXH_ParentID = consol.PK;
		header.CXH_ParentTableCode = "JK";
		Factory.Save();
		AssertEquals("Reference number set as JK_UniqueConsignRef", "CON", header.CXH_JobReference);
	}

	public void TestValidationModes()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("NONE for header", ValidationModes.None, header.ValidationModes);
	}

	public void TestCusExitReportStatus()
	{
		var report1 = Factory.New<CusExitReport>();
		report1.CER_CXH_Header = exitHeader.PK;
		var report2 = Factory.New<CusExitReport>();
		report2.CER_CXH_Header = exitHeader.PK;
		var report3 = Factory.New<CusExitReport>();
		report3.CER_CXH_Header = Factory.New<CusExitHeader>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { report1.PK, report2.PK }, exitHeader.CusExitReportStatus.Select(x => x.PK));
	}

	public void TestDefaultDataFromParent()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();
		var mrn1 = CusEntryNumber.New(entryHeader1, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
		var mrn2 = CusEntryNumber.New(entryHeader2, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration.CountryCode);
		mrn1.CE_EntryNum = "MRN1";
		mrn2.CE_EntryNum = "MRN2";

		CombineAssertions(() =>
		{
			var consignments = exitHeader.CusExitConsignments;
			var consignment1 = consignments.AddNew();
			consignment1.CXC_MovementReference = "MRN1";
			var consignment2 = consignments.AddNew();
			consignment2.CXC_MovementReference = "MRN3";
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(includeHeaderData: false);
			AssertContainsExactElementsInAnyOrder(new[] { "MRN1", "MRN2", "MRN3" }, exitHeader.CusExitConsignments.Select(x => x.CXC_MovementReference));
		});
	}

	public void TestDefaultDataFromParent_IncludeHeaderData()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		var orgHeader = Factory.New<OrgHeader>();
		declaration.JE_OH_ShippingLine = orgHeader.PK;

		var exitHeader = Factory.New<CusExitHeader>();
		exitHeader.Parent = declaration;
		exitHeader.DefaultDataFromParent(true);
		CombineAssertions(() =>
		{
			AssertEquals("CXH_OH_Exporter", supplier.PK, exitHeader.CXH_OH_Exporter);
			AssertEquals("CXH_OA_Carrier", orgHeader.MainAddress.PK, exitHeader.CXH_OA_Carrier);
		});
	}

	public void TestDefaultDataFromParent_IncludeHeaderData_CXH_JobReference()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "B001232";
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_UniqueConsignRef = "S00001001";

		CombineAssertions(() =>
		{
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(true);
			AssertEquals("CXH_JobReference from declaration", "B001232", exitHeader.CXH_JobReference);

			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			exitHeader.DefaultDataFromParent(true);
			AssertEquals("No linked declaration, CXH_JobReference from shipment", "S00001001", exitHeader.CXH_JobReference);

			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = ZString.Empty;
			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			exitHeader.DefaultDataFromParent(true);
			AssertEquals("Has linked declaration, CXH_JobReference from shipment", "S00001001", exitHeader.CXH_JobReference);
		});
	}

	public void TestDefaultDataFromParent_CXH_GB_Branch()
	{
		var company = Factory.New<GlbCompany>();
		var branch = company.Branches.AddNew();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GB = branch.PK;
		CombineAssertions(() =>
		{
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(includeHeaderData: false);
			AssertEquals("CXH_GB_Branch from Global", GlbBranch.CurrentBranch.PK, exitHeader.CXH_GB_Branch);

			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(includeHeaderData: true);
			AssertEquals("CXH_GB_Branch from declaration", branch.PK, exitHeader.CXH_GB_Branch);
		});
	}

	public void TestDefaultDataFromParent_Shipment_WhenDeclarationExists()
	{
		var exitHeaderMock = Factory.NewMoq<CusExitHeader>();
		exitHeaderMock.CallBase = true;
		var exitHeader = exitHeaderMock.Object;

		var shipment = Factory.New<ForwardingShipment>();
		exitHeader.Parent = shipment;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var protectedMock = exitHeaderMock.Protected();
		protectedMock.Setup("CreateConsignmentFromShipment", true, shipment).Verifiable();

		exitHeader.DefaultDataFromParent(false);
		protectedMock.Verify("CreateConsignmentFromShipment", Times.Never(), shipment);

		Assert("DefaultDataFromParent() should not call CreateConsignmentFromShipment()", true);
	}

	public void TestWorkflowInformationProvider()
	{
		AssertType(typeof(CusExitHeaderWorkflowInformationProvider), exitHeader.GetWorkflowInformationProvider());
	}

	public void TestWorkflowItems_CusExitHeader()
	{
		AssertType(typeof(CusExitHeaderProcessTaskCollection), exitHeader.WorkflowItems);
	}

	public void TestWorkflowItems_Declaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		exitHeader.Parent = declaration;
		AssertType<Customs.Business.JobDeclarationProcessTaskCollection<JobDeclaration>>(exitHeader.WorkflowItems);
	}

	public void TestWorkflowItems_Shipment()
	{
		var shipment = Factory.New<ForwardingShipment>();
		exitHeader.Parent = shipment;
		AssertType<ForwardingShipmentProcessTaskCollection>(exitHeader.WorkflowItems);
	}

	public void TestTemplateSelectionCriteria()
	{
		AssertType(typeof(ColumnValueRanker), exitHeader.GetTemplateSelectionCriteria());
	}

	public void TestWorkflowType()
	{
		AssertEquals(WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode, exitHeader.WorkflowType);
	}

	public void TestBusinessObjectsWithRelatedEvents()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		var exitReport = exitHeader.CusExitReports.AddNew();
		AssertCollectionContains(exitReport, exitHeader.BusinessObjectsWithRelatedEvents);
	}

	public void TestIsUCC6() => CombineAssertions(() =>
	{
		AssertEquals("not ucc6", false, Factory.New<CusExitHeader>().IsUCC6);
		AssertEquals("ucc6", true, Factory.GetUcc6ExitHeader().IsUCC6);
	});

	public void TestJobHeaderCompany()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		IWorkflowTriggerEventSource source = exitHeader;

		AssertSame(exitHeader.Company, source.JobHeaderCompany);
	}

	public void TestParentWorkflowProviders()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		IWorkflowTriggerEventSource source = exitHeader;

		CombineAssertions(() =>
		{
			AssertEquals(0, source.ParentWorkflowProviders.Count);

			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.Parent = shipment;
			AssertContainsExactElementsInAnyOrder(new[] { shipment }, source.ParentWorkflowProviders);

			var declaration = Factory.New<JobDeclaration>();
			exitHeader.Parent = declaration;
			AssertContainsExactElementsInAnyOrder(new[] { declaration }, source.ParentWorkflowProviders);
		});
	}
}
