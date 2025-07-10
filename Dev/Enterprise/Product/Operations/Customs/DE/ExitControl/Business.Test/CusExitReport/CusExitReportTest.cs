using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReport))]
	sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultExitOffice()
		{
			var orgHeader = CreateOrgHeaderForTest();

			(var report, _) = GetNewBusinessObject(Factory);

			report.Location = orgHeader.OH_Code;
			AssertEquals("CER_OfficeOfExit is not empty", "DE001", report.CER_OfficeOfExit);
			report.Location = ZString.Empty;

			orgHeader.CustomsCodes.AddNew("CEX", "54321", CountryCodes.Germany);
			report.CER_OfficeOfExit = ZString.Empty;
			report.Location = orgHeader.OH_Code;
			AssertEquals("Default Exit Office set", "54321", report.CER_OfficeOfExit);

			report.Location = ZString.Empty;
			report.CER_OfficeOfExit = "DE1234";
			report.Location = orgHeader.OH_Code;
			AssertEquals("Existing Exit Office not changed", "DE1234", report.CER_OfficeOfExit);
		}

		public void TestUpdateCER_Location()
		{
			(var report, _) = GetNewBusinessObject(Factory);

			AssertEquals(ZString.Empty, report.CER_Location);

			var orgHeader = CreateOrgHeaderForTest();
			report.Location = orgHeader.OH_Code;
			AssertEquals("FanTianWa,SB1,123456,CaoXian", report.CER_Location);

			report.Location = "Address,Entered Manually";
			AssertEquals("Address entered manually", "Address,Entered Manually", report.CER_Location);

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "KouShuiWa";
			orgHeader.OH_Code = "EMOJI";
			var address1 = orgHeader1.Addresses.AddNew();
			address1.OA_Address1 = "NB1";
			address1.AddAddressType(OrgAddressType.Office);
			address1.OA_PostCode = "123456";
			address1.OA_City = "CaoXian";
			report.Location = orgHeader1.OH_Code;
			AssertEquals("No pickup address", ZString.Empty, report.CER_Location);
		}

		public void TestLocationTrimming()
		{
			(var report, _) = GetNewBusinessObject(Factory);

			var orgHeader = CreateOrgHeaderForTest();
			var address = orgHeader.Addresses.OfType<OrgAddress>().Single(x => x.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Pickup));
			orgHeader.OH_FullName = (ZString)"Name".PadRight(100, 'N');
			address.OA_Address1 = (ZString)"Address".PadRight(50, 'A');
			address.OA_PostCode = (ZString)"PostCode".PadRight(10, 'P');
			address.OA_City = (ZString)"City".PadRight(50, 'C');
			report.Location = orgHeader.OH_Code;
			var expectedLocation = $"{orgHeader.OH_FullName.Left(43)},{address.OA_Address1},PostCodeP,{address.OA_City.Left(35)}";
			AssertEquals("Company name trimmed", expectedLocation, report.CER_Location);

			report.Location = ZString.Empty;
			address.OA_Address1 = "A";
			address.OA_PostCode = "P";
			address.OA_City = "C";

			report.Location = orgHeader.OH_Code;
			AssertEquals("Company name full", $"{orgHeader.OH_FullName},A,P,C", report.CER_Location);
		}

		OrgHeader CreateOrgHeaderForTest()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FanTianWa";
			orgHeader.OH_Code = "YYDS";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
			var address1 = orgHeader.Addresses.AddNew();
			address1.OA_Address1 = "SB1";
			address1.AddAddressType(OrgAddressType.Pickup);
			address1.OA_PostCode = "123456";
			address1.OA_City = "CaoXian";

			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_Address1 = "SB2";
			address2.AddAddressType(OrgAddressType.Office);
			address2.OA_PostCode = "654321";
			address2.OA_City = "TokyoBoom";
			return orgHeader;
		}

		public void TestValidation()
		{
			(var report, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportValidation>(report.Validation);
		}

		public void TestLookups()
		{
			(var report, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportLookups>(report.Lookups);
		}

		public void TestICusExitReportCorrectlySetup()
		{
			(var report, _) = GetNewBusinessObject(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var iReport = newFactory.Load<Integration.Customs.DEExitControl.ICusExitReport>(report.PK);
			AssertType<CusExitReport>(iReport);
		}

		public void TestValidateDeclarantWithoutAddressDoesNotThrowException()
		{
			AssertNoExceptionThrown(() =>
			{
				(var report, _) = GetNewBusinessObject(Factory);
				var declarant = Factory.New<OrgHeader>();
				report.Declarant.OrganisationPK = declarant.PK;
				report.Declarant.E2_OA_Address = ZGuid.Empty;
			});
		}

		public void TestValidateDeclarant_ValidEORIDetails()
		{
			const string messageErrorEORIDetails = "Declarant is missing EORI number and branch.";
			const string messageErrorEORINumber = "Declarant is missing EORI number.";
			const string messageErrorEORIBranch = "Declarant is missing EORI branch.";

			TestHelper.CreateCL010CoutryList(Factory);

			(var report, _) = GetNewBusinessObject(Factory);
			var declarant = Factory.New<OrgHeader>();

			var declarantAddressWithEORiBranchConnected = declarant.Addresses.AddNew();
			var eoriBranchOrgCusCode = Factory.New<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(declarant.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, "0000", declarantAddressWithEORiBranchConnected.PK);
			var declarantDocAddressWithEORIBranch = Factory.New<JobDocAddress>();
			declarantDocAddressWithEORIBranch.E2_OA_Address = declarantAddressWithEORiBranchConnected.PK;

			var declarantAddressWithoutEORiBranchConnected = declarant.Addresses.AddNew();
			var declarantDocAddressWithoutEORIBranch = Factory.New<JobDocAddress>();
			declarantDocAddressWithoutEORIBranch.E2_OA_Address = declarantAddressWithoutEORiBranchConnected.PK;

			var representative = Factory.New<OrgHeader>();
			var representativeAddress = representative.MainAddress;
			var representativeDocAddress = Factory.New<JobDocAddress>();
			representativeDocAddress.E2_OA_Address = representativeAddress.PK;

			var targetInfo = report.Declarant.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				report.Declarant.E2_OA_Address = declarantAddressWithoutEORiBranchConnected.PK; //Setting address triggers Declarant.Validation.ValidateOrganisationPK()
				AssertHasMessageError("Representative empty, EORIDetails missing: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("Representative empty, EORIDetails missing: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("Representative empty, EORIDetails missing: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);
				report.Representative.OrganisationPK = representative.PK;
				report.Declarant.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Representative captured, EORIDetails missing: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				report.Representative.OrganisationPK = ZGuid.Empty;

				declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Germany);
				report.Declarant.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Representative empty, HasEoriNumber: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("Representative empty, HasEoriNumber: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertHasMessageError("Representative empty, HasEoriNumber: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);
				report.Representative.OrganisationPK = representative.PK;
				report.Declarant.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Representative captured, HasEoriNumber: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);
				report.Representative.OrganisationPK = ZGuid.Empty;

				report.Declarant.E2_OA_Address = declarantAddressWithEORiBranchConnected.PK;
				AssertNoMessageError("Representative empty, HasEoriDetails: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("Representative empty, HasEoriDetails: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("Representative empty, HasEoriDetails: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);

				declarant.DeleteSingleEORINumber();
				report.Declarant.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Representative empty, HasEORIBranch: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertHasMessageError("Representative empty, HasEORIBranch: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("Representative empty, HasEORIBranch: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);
				report.Representative.OrganisationPK = representative.PK;
				report.Declarant.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Representative captured,  HasEORIBranch: EORINumber Message Error", targetInfo, messageErrorEORINumber);
			});
		}

		public void TestValidateRepresentativeWithoutAddressDoesNotThrowException()
		{
			AssertNoExceptionThrown(() =>
			{
				(var report, _) = GetNewBusinessObject(Factory);
				var representative = Factory.New<OrgHeader>();
				report.Representative.OrganisationPK = representative.PK;
				report.Representative.E2_OA_Address = ZGuid.Empty;
			});
		}

		public void TestValidateRepresentative_ValidEORIDetails()
		{
			const string messageErrorEORIDetails = "Representative is missing EORI number and branch.";
			const string messageErrorEORINumber = "Representative is missing EORI number.";
			const string messageErrorEORIBranch = "Representative is missing EORI branch.";

			TestHelper.CreateCL010CoutryList(Factory);

			(var report, _) = GetNewBusinessObject(Factory);
			var representative = Factory.New<OrgHeader>();

			var representativeAddressWithEORiBranchConnected = representative.Addresses.AddNew();
			var eoriBranchOrgCusCode = Factory.New<OrgCusCode>();
			eoriBranchOrgCusCode.ModifyOrgCusCode(representative.PK, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, "0000", representativeAddressWithEORiBranchConnected.PK);
			var representativeDocAddressWithEORIBranch = Factory.New<JobDocAddress>();
			representativeDocAddressWithEORIBranch.E2_OA_Address = representativeAddressWithEORiBranchConnected.PK;

			var representativeAddressWithoutEORiBranchConnected = representative.Addresses.AddNew();
			var representativeDocAddressWithoutEORIBranch = Factory.New<JobDocAddress>();
			representativeDocAddressWithoutEORIBranch.E2_OA_Address = representativeAddressWithoutEORiBranchConnected.PK;

			var targetInfo = report.Representative.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				report.Representative.E2_OA_Address = representativeAddressWithoutEORiBranchConnected.PK; //Setting address triggers Representative.Validation.ValidateOrganisationPK()
				AssertHasMessageError("EORIDetails missing: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("EORIDetails missing: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("EORIDetails missing: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);

				representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Germany);
				report.Representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError("HasEoriNumber: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("HasEoriNumber: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertHasMessageError("HasEoriNumber: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);

				report.Representative.E2_OA_Address = representativeAddressWithEORiBranchConnected.PK;
				AssertNoMessageError("HasEoriDetails: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertNoMessageError("HasEoriDetails: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("HasEoriDetails: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);

				representative.DeleteSingleEORINumber();
				report.Representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError("HasEoriNumber: EORIDetails Message Error", targetInfo, messageErrorEORIDetails);
				AssertHasMessageError("HasEoriNumber: EORINumber Message Error", targetInfo, messageErrorEORINumber);
				AssertNoMessageError("HasEoriNumber: EORIBranch Message Error", targetInfo, messageErrorEORIBranch);
			});
		}

		public void TestCER_OfficeOfExportLongCaption()
		{
			var (report, _) = GetNewBusinessObject(Factory);
			var officeOfExportProperty = DataBoundResourceStrings.GetDataForProperty(report.CER_OfficeOfExportInfo);

			CombineAssertions(() =>
			{
				AssertEquals("OfficeOfExport Long Caption", "Intended Office of Exit", officeOfExportProperty.Caption);
				AssertEquals("OfficeOfExport Medium Caption", "Int. Office of Exit", officeOfExportProperty.MediumCaption);
				AssertEquals("OfficeOfExport Short Caption", "Int. Office", officeOfExportProperty.ShortCaption);
			});
		}

		public void TestSetReportBehaviorFromConsignmentItems()
		{
			var (report, header) = GetNewBusinessObject(Factory);
			var consignment = header.CusExitConsignments.AddNew();
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();

			report.CER_CXC_Consignment = consignment.PK;
			report.SetReportBehaviorFromConsignmentItems();
			AssertEquals("No consignment item selected", CusExitReportBehaviorList.Codes.STD, report.CER_Behavior);

			consignmentItem.CCI_Calc_ShouldReportItem = true;
			report.SetReportBehaviorFromConsignmentItems();
			AssertEquals("Has consignment item selected", CusExitReportBehaviorList.Codes.STD, report.CER_Behavior);
		}

		public void TestCER_DateTimeCaptions()
		{
			var (report, _) = GetNewBusinessObject(Factory);
			var cerDateTimeProperty = DataBoundResourceStrings.GetDataForProperty(report.CER_DateTimeInfo);

			CombineAssertions(() =>
			{
				AssertEquals("CER_DateTime Long Caption", "Exit Date & Time", cerDateTimeProperty.Caption);
				AssertEquals("CER_DateTime Medium Caption", "Exit Date", cerDateTimeProperty.MediumCaption);
				AssertEquals("CER_DateTime Short Caption", "Date", cerDateTimeProperty.ShortCaption);
			});
		}

		public void TestDefaultDataFromParent()
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			var jobDeclaration = Factory.New<JobDeclaration>();
			header.Parent = jobDeclaration;

			var report = header.CusExitReports.AddNew();
			report.DefaultDataFromParent();

			CombineAssertions(() =>
			{
				AssertEquals("No Customs Office on Parent Declaration", ZString.Empty, report.CER_OfficeOfExit);

				jobDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "XYZ456");
				report.DefaultDataFromParent();

				AssertEquals("No Customs Office with Type == 'EXT' on Parent Declaration", ZString.Empty, report.CER_OfficeOfExit);

				jobDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "ABCD1234");
				report.DefaultDataFromParent();

				AssertEquals("Customs Office of Exit copied from parent Declaration", "ABCD1234", report.CER_OfficeOfExit);
			});
		}

		public void TestDefaultDataFromParentDeclaration_CusOfficeOfExitAlreadySet()
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			var jobDeclaration = Factory.New<JobDeclaration>();
			header.Parent = jobDeclaration;

			jobDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "ABCD1234");

			var report = header.CusExitReports.AddNew();
			report.DefaultDataFromParent();

			CombineAssertions(() =>
			{
				AssertEquals("Customs Office of Exit copied from parent Declaration", "ABCD1234", report.CER_OfficeOfExit);

				report.CER_OfficeOfExit = "DEFG7890";
				report.DefaultDataFromParent();

				AssertEquals("Customs Office of Exit is not copied from parent Declaration when CER_OfficeOfExit already has a value", "DEFG7890", report.CER_OfficeOfExit);
			});
		}

		public void TestDefaultFlightDataFromShipment_CER_RN_NKTransportNationality()
		{
			CreateUNLOCOsForTest();
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			var transport = consol.Transports.AddNew("DE123", "US123");
			transport.JW_VoyageFlight = "LH428";

			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			header.Parent = shipment;
			var report = header.CusExitReports.AddNew();
			report.DefaultDataFromParent();

			CombineAssertions(() =>
			{
				AssertEquals("Defaulted", "DE", report.CER_RN_NKTransportNationality);
				report.CER_RN_NKTransportNationality = "UK";
				report.DefaultDataFromParent();
				AssertEquals("Not Defaulted when set", "UK", report.CER_RN_NKTransportNationality);
			});
		}

		public void TestDefaultDataFromParent_Shipment()
		{
			CreateUNLOCOsForTest();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "US123";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "DE123";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			var transport = consol.Transports.AddNew("DE123", "US123");
			transport.JW_TransportMode = "AIR";
			transport.JW_VoyageFlight = "LH428";
			transport.JW_ETDForBinding = new ZDateTime(2023, 07, 26, 12, 45, 00);

			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			header.Parent = shipment;
			var report = header.CusExitReports.AddNew();
			report.DefaultDataFromParent();

			CombineAssertions(() =>
			{
				AssertEquals("CER_TransportMode", "AIR", report.CER_TransportMode);
				AssertEquals("CER_TransportID", "LH428", report.CER_TransportID);
				AssertEquals("CER_TransportType", "40", report.CER_TransportType);
				AssertEquals("CER_RN_NKTransportNationality", "DE", report.CER_RN_NKTransportNationality);
				AssertEquals("CER_DateTime", new ZDateTimeOffset(2023, 07, 26, 14, 45, 00), report.CER_DateTime);
			});
		}

		void CreateUNLOCOsForTest()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			var unitedStates = RefCountry.LoadFromCountryCode(Factory, "US");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			helper.CreateUnlocoIfNotExists("US123", unitedStates);
			Factory.Save();
		}

		public void TestDefaultDataFromParent_Shipment_WhenNoMatchedTransport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USCLT";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "USCLT";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "AIR";

			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			header.Parent = shipment;
			var report = header.CusExitReports.AddNew();
			report.DefaultDataFromParent();
			AssertEquals(ZString.Empty, report.CER_TransportMode);
		}

		public void TestIsAutomatedValidationEnabledTRA()
		{
			var (report, header) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, false);
				AssertEquals("Precondition: TRA Automated Validation header false", false, header.IsAutomatedValidationEnabledTRA);
				AssertEquals("IsAutomatedValidationEnabledTRA false", false, report.IsAutomatedValidationEnabledTRA);

				CusExitHeaderTest.SetEnableAutomatedValidationTRA(header, true);
				AssertEquals("Precondition: TRA Automated Validation header true", true, header.IsAutomatedValidationEnabledTRA);
				AssertEquals("IsAutomatedValidationEnabledTRA true", true, report.IsAutomatedValidationEnabledTRA);
			});
		}

		public void TestDefaultingIsFinalized()
		{
			var (_, header) = GetNewBusinessObject(Factory);

			AssertEquals("Report is finalized", true, header.CusExitReports.AddNew().CER_IsFinalized);
		}

		public void TestISupportAutoSendExitReportTransferMessage() => Assert(typeof(ISupportAutoSendExitReportTransferMessage).IsAssignableFrom(typeof(CusExitReport)));

		public void TestGetSendExitReportTransferMessageProcessor()
		{
			var exitReport = Factory.New<CusExitReport>();
			AssertType<SendExitReportTransferMessageProcessor>(exitReport.GetSendExitReportTransferMessageProcessor(exitReport));
		}

		public void TestRegistryBranchPK()
		{
			var (report, header) = GetNewBusinessObject(Factory);
			var branch = Factory.New<GlbBranch>();
			header.CXH_GB_Branch = branch.PK;

			CombineAssertions(() =>
			{
				AssertEquals("CXH_GB_Branch", report.RegistryBranchPK, branch.PK);

				header.CXH_GB_Branch = ZGuid.Empty;
				AssertEquals("Fallback", GlbBranch.CurrentBranch.PK.ToGuid(), report.RegistryBranchPK);
			});
		}

		public void TestCreateStmProcessQueueProcessor()
		{
			var exitReport = Factory.New<CusExitReport>();
			AssertType<CustomsStmProcessQueueCreatorProcessor>(exitReport.CreateStmProcessQueueProcessor(exitReport, "TRA"));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

		internal static (CusExitReport report, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Today;
			report.CER_OfficeOfExit = "DE001";
			return (report, header);
		}
	}
}
