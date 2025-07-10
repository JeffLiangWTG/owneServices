using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;
using ExportBorderTransportMeansList = Enterprise.Customs.EU.Business.ExportBorderTransportMeansList;
using TransportMeansList = Enterprise.Customs.Business.TransportMeansList;
using TransportTypeList = Enterprise.Customs.Business.TransportTypeList;
using ValidationTestHelper = Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobDeclarationValidation))]
	sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		public void TestValidateOneMainPackInvoiceLinePerPackage()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine4.JI_LineNo = 4;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine4.JI_CEI = instruction.PK;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "VG";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "CT";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 20;
			package3.CW_PackType = "CT";
			package3.CW_HouseBill = bill.CU_BillUniqueCode;

			invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invoiceLine4.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine4.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;

			invoiceLine1.ZG_IsMainPack = true;
			invoiceLine2.ZG_IsMainPack = true;
			invoiceLine3.ZG_IsMainPack = true;
			invoiceLine4.ZG_IsMainPack = true;

			var message1 = "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. Only one of invoice 1 line 3, invoice 1 line 4 should be indicated as Is Main Pack.";
			var message2 = "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. One of invoice 1 line 3, invoice 1 line 4 should be indicated as Is Main Pack.";
			ValidateAll();
			AssertNoMessageErrors("Not Packed", invoiceLine1.ZG_IsMainPackInfo);
			AssertNoMessageErrors("Not Packed", invoiceLine2.ZG_IsMainPackInfo);
			AssertHasMessageError("Two main packs", invoiceLine3.ZG_IsMainPackInfo, message1);
			AssertHasMessageError("Two main packs", invoiceLine4.ZG_IsMainPackInfo, message1);

			invoiceLine3.ZG_IsMainPack = false;
			invoiceLine4.ZG_IsMainPack = true;
			ValidateAll();
			AssertNoMessageErrors("One main packs", invoiceLine3.ZG_IsMainPackInfo);
			AssertNoMessageErrors("One main packs", invoiceLine4.ZG_IsMainPackInfo);

			invoiceLine4.ZG_IsMainPack = false;
			ValidateAll();
			AssertHasMessageError("No main pack", invoiceLine3.ZG_IsMainPackInfo, message2);
			AssertHasMessageError("No main pack", invoiceLine4.ZG_IsMainPackInfo, message2);

			void ValidateAll()
			{
				declaration.Validation.ValidateAll();
				invoiceLine1.AddInfoValidation.ValidateAll();
				invoiceLine2.AddInfoValidation.ValidateAll();
				invoiceLine3.AddInfoValidation.ValidateAll();
				invoiceLine4.AddInfoValidation.ValidateAll();
			}
		}

		public void TestCheckJE_ContainerMode()
		{
			var targetInfo = declaration.JE_ContainerModeInfo;
			declaration.JE_ContainerMode = string.Empty;
			AssertHasMessageErrorContaining("JE_ContainerMode is mandatory", targetInfo, MandatoryValidation.YouHaveNotEntered);
			const string message = "At least one Container must be entered.";
			declaration.JE_ContainerMode = "NCT";
			AssertNoMessageErrorContaining("JE_ContainerMode is populated - should not have this message", targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Should not have error", targetInfo, message);
			declaration.JE_ContainerMode = "CNT";
			AssertHasMessageError("Should have error", targetInfo, message);
			declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoMessageErrorContaining("Should not have error", targetInfo, message);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXX", "CNT");
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			const string message = "Country of Export is mandatory unless Declaration Type is B4.";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_RL_NKOrigin = "IEDUB";
			AssertNoMessageError("No B4, not empty", declaration.JE_RL_NKOriginInfo, message);

			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageError("No B4, empty", declaration.JE_RL_NKOriginInfo, message);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B4;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoMessageError("Has B4, empty", declaration.JE_RL_NKOriginInfo, message);
		}

		public void TestCheckJE_GoodsOrigin()
		{
			const string message = "Country of Origin is required when Declaration Type is B2 or B3.";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			declaration.JE_GoodsOrigin = "IE";
			AssertNoMessageError("B2, not empty", declaration.JE_GoodsOriginInfo, message);

			declaration.JE_GoodsOrigin = ZString.Empty;
			AssertHasMessageError("B2, empty", declaration.JE_GoodsOriginInfo, message);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError("B1, empty", declaration.JE_GoodsOriginInfo, message);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertHasMessageError("B3, empty", declaration.JE_GoodsOriginInfo, message);

			declaration.JE_GoodsOrigin = "IE";
			AssertNoMessageError("B3, not empty", declaration.JE_GoodsOriginInfo, message);
		}

		public void TestCheckJE_LocationOtherInformation()
		{
			var targetInfo = declaration.JE_LocationOtherInformationInfo;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var goodsLocationType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			helper.CreateNewOrGetExistingCusCodeType(goodsLocationType, "Goods Location");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, goodsLocationType, "U", "Location Typ U", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertMandatoryWhenHasNonPreliminaryDeclaration(targetInfo, "Type is mandatory unless all Instruction Sub Style in 'D', 'F'.", "A");

				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXX", "U");
			});
		}

		public void TestCheckJE_LocationQualifier()
		{
			AssertMandatoryWhenHasNonPreliminaryDeclaration(declaration.JE_LocationQualifierInfo, "Qualifier is mandatory unless all Instruction Sub Style in 'D', 'F'.", "U");
		}

		public void TestCheckJE_LocationOfGoods()
		{
			var targetInfo = declaration.JE_LocationOfGoodsInfo;

			AssertMandatoryWhenHasNonPreliminaryDeclaration(targetInfo, "Goods Location is mandatory unless all Instruction Sub Style in 'D', 'F'.", "GB000000");

			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXXXXXXX", "IEDFA");
		}

		void AssertMandatoryWhenHasNonPreliminaryDeclaration(ZPropertyInfo propertyInfo, string message, ZString value)
		{
			Assert("value for test should not be empty", !value.IsEmpty);

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
			propertyInfo.Value = value;
			AssertNoMessageError("D, not empty", propertyInfo, message);

			propertyInfo.Value = ZString.Empty;
			AssertNoMessageError("D, empty", propertyInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			propertyInfo.Value = value;
			AssertNoMessageError("A, not empty", propertyInfo, message);

			propertyInfo.Value = ZString.Empty;
			AssertHasMessageError("A, empty", propertyInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
			propertyInfo.Value = value;
			AssertNoMessageError("F, not empty", propertyInfo, message);

			propertyInfo.Value = ZString.Empty;
			AssertNoMessageError("F, empty", propertyInfo, message);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestCheckJE_VesselName_MaxLength()
		{
			declaration.JE_VesselName = ZString.Empty.PadLeft(28, 'A');
			AssertHasWarning("28 characters", declaration.JE_VesselNameInfo, "The maximum length for [21] Vessel is 27 characters.");
		}

		public void TestCheckJE_VoyageFlightNo_Mandatory_BorderTransportMeansIs40()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_BorderTransportMeans = EU.Business.ExportBorderTransportMeansList.Codes._40;
			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageErrorContaining("JE_VoyageFlightNo is empty", declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageErrorContaining("JE_VoyageFlightNo is empty and JE_TransportMode isn't 'AIR'", declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageErrorContaining("JE_VoyageFlightNo is empty and ZG_BorderTransportMeans isn't '40'", declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				declaration.JE_VoyageFlightNo = "123456";
				AssertNoMessageErrorContaining("JE_VoyageFlightNo isn't empty", declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_JE_AircraftRegistration_Mandatory_BorderTransportMeansIs41()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_AircraftRegistration();
				AssertHasMessageErrorContaining("JE_AircraftRegistration is empty", declaration.JE_AircraftRegistrationInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.Validation.ValidateJE_AircraftRegistration();
				AssertNoMessageErrorContaining("JE_AircraftRegistration is empty and JE_TransportMode isn't 'AIR'", declaration.JE_AircraftRegistrationInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				declaration.Validation.ValidateJE_AircraftRegistration();
				AssertNoMessageErrorContaining("JE_AircraftRegistration is empty and ZG_BorderTransportMeans isn't '41'", declaration.JE_AircraftRegistrationInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				declaration.JE_VoyageFlightNo = "123456";
				AssertNoMessageErrorContaining("JE_AircraftRegistration isn't empty", declaration.JE_AircraftRegistrationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_TransportModeMandatory()
		{
			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.Validation.ValidateJE_TransportMode();
				AssertNoMessageErrorContaining("B1 Declaration. Additional Declaration Code not in (B, C, E, F) and JE_TransportMode is present", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = ZString.Empty;
				declaration.Validation.ValidateJE_TransportMode();
				AssertHasMessageErrorContaining("B1 Declaration. Additional Declaration Code not in (B, C, E, F) and JE_TransportMode is not present", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
				declaration.Validation.ValidateJE_TransportMode();
				AssertNoMessageErrorContaining("B1 Declaration. Additional Declaration Code in (B, C, E, F) and JE_TransportMode is not present", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
				declaration.Validation.ValidateJE_TransportMode();
				AssertHasMessageErrorContaining("B2 Declaration. Additional Declaration Code in (B, C, E, F) and JE_TransportMode is not present", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_TransportMode();
				AssertHasMessageErrorContaining("No entry instruction and JE_TransportMode is not present", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_TransportModeMandatory_MessageType()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			declaration.JE_TransportMode = ZString.Empty;
			AssertNoMessageErrorContaining("No validation for REX Je_MessageType and no B1 CustomsEntryInstuction", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			declaration.JE_TransportMode = ZString.Empty;
			AssertNoMessageErrorContaining("No validation for EXS Je_MessageType and no B1 CustomsEntryInstuction", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			declaration.JE_TransportMode = ZString.Empty;
			AssertNoMessageError("No validation for EXS Je_MessageType and B1 CustomsEntryInstuction", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			var declarationExp = Factory.New<JobDeclaration>();
			declarationExp.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var instructionExp = declarationExp.CustomsEntryInstructions.AddNew();
			instructionExp.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declarationExp.JE_TransportMode = ZString.Empty;
			AssertHasMessageErrorContaining("Validation for EXP Je_MessageType and B1 CustomsEntryInstuction", declarationExp.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TransportModeInland()
		{
			var targetInfo = declaration.JE_TransportModeInlandInfo;

			var notAllowedMessage = string.Format("When Office of Export equals Office of Exit or Office of Presentation, then {0} is not Allowed", targetInfo.HumanReadableName);
			var requiredMessage = string.Format("{0} is mandatory for indirect exits.", targetInfo.HumanReadableName);

			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("Empty job, No mandatory/no-allowed check.", targetInfo, notAllowedMessage);
			AssertNoMessageError("Empty job, No mandatory/no-allowed check.", targetInfo, requiredMessage);

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
			AssertNoMessageError("Empty offices, no not-allowed check.", targetInfo, notAllowedMessage);

			var presentationOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE000002");
			var officeOfExit = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE000003");
			declaration.JE_CustomsOffice = "IE000001";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("Different offices, no not-allowed check.", targetInfo, notAllowedMessage);
			AssertNoMessageError("No mandatory check for empty job.", targetInfo, requiredMessage);

			officeOfExit.CY_Data = "IE000001";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageError("officeOfExport == officeOfExit, not-allowed check should be proceeded.", targetInfo, notAllowedMessage);
			declaration.JE_TransportModeInland = string.Empty;
			AssertNoMessageError("officeOfExport == officeOfExit, not-allowed check should be proceeded(validation passes).", targetInfo, notAllowedMessage);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("An instruction with CEI_SubStyle B/C/D/E/F, no mandatory check.", targetInfo, requiredMessage);

			declaration.JE_CustomsOffice = string.Empty;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			instruction.CEI_SubStyle = string.Empty;
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("No Instruction with CEI_SubStyle B/C/D/E/F, ALL OFFICES EMPTY, no mandatory check.", targetInfo, requiredMessage);

			presentationOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE000001");
			officeOfExit = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE000001");
			declaration.JE_CustomsOffice = "IE000001";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("No Instruction with CEI_SubStyle B/C/D/E/F, ALL OFFICES THE SAME, no mandatory check.", targetInfo, requiredMessage);

			presentationOffice.CY_Data = "IE000002";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageError("No Instruction with CEI_SubStyle B/C/D/E/F, Presentation!=Export, EXPECTING mandatory check.", targetInfo, requiredMessage);
			presentationOffice.CY_Data = "IE000001";

			officeOfExit.CY_Data = "IE000002";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageError("No Instruction with CEI_SubStyle B/C/D/E/F, Exit!=Export, EXPECTING mandatory check.", targetInfo, requiredMessage);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			declaration.Validation.ValidateJE_TransportModeInland();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageError("Entry with CH_EntryStatus PRE, mandatory check shoudld be proceeded.", targetInfo, requiredMessage);
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
			AssertNoMessageError("Entry with CH_EntryStatus PRE, mandatory check shoudld be proceeded(validation passes).", targetInfo, requiredMessage);
		}

		public void TestCheckJE_OA_Representative()
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var targetInfo = declaration.JE_OA_RepresentativeInfo;

			Factory.Save();
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNoMessageErrors(targetInfo);

			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertHasMessageErrorContaining("An EORI number is required", targetInfo, "An EORI number is required");

			declaration.CustomsEntryHeaders.AddNew().CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("An EORI number existed", targetInfo, "An EORI number is required");
			AssertNoMessageErrorContaining("An EORI number existed", targetInfo, $"The EORI of the {targetInfo.HumanReadableName} must be different to the {declaration.JE_OA_DeclarantAddressInfo.HumanReadableName}");

			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("An EORI number existed", targetInfo, $"The EORI of the {targetInfo.HumanReadableName} must be different to the {declaration.JE_OA_DeclarantAddressInfo.HumanReadableName}");
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			declaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("An EORI number existed", targetInfo, $"The EORI of the {targetInfo.HumanReadableName} must be different to the {declaration.JE_OA_DeclarantAddressInfo.HumanReadableName}");
		}

		public void TestCheckJE_OA_Representative_NoAmend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var originalPresentative = Factory.NewWithValidTestData<OrgHeader>();
			originalPresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE001");

			var anotherExporterRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			anotherExporterRepresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE002");
			declaration.JE_OA_Representative = originalPresentative.MainAddress.PK;
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OA_Representative = anotherExporterRepresentative.MainAddress.PK;
			AssertHasMessageError("Amend Check", declaration.JE_OA_RepresentativeInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_OA_Representative = originalPresentative.MainAddress.PK;
			AssertNoMessageError("Amend Check passes.", declaration.JE_OA_RepresentativeInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var targetInfo = declaration.JE_OA_DeclarantAddressInfo;
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageErrorContaining("Empty check", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertHasMessageErrorContaining("An EORI number is required", targetInfo, "An EORI number is required");
			AssertNoMessageErrorContaining("Empty check", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.CustomsEntryHeaders.AddNew().CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("An EORI number existed", targetInfo, "An EORI number is required");
		}

		public void TestDeclarantEORIShouldMatchMessageSenderEORI()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var company = declaration.Company;
			var companyName = company.CompanyName;
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			companyCredential.GP_MailBoxID = "IEREG222";
			var declarantEORI = declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			Factory.Save();

			var targetInfo = declaration.JE_OA_DeclarantAddressInfo;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var messageError = $"Declarant's EORI 'DEREG222' is different to Company ({companyName})'s Message Sender EORI 'IEREG222'";
			AssertHasMessageError("An EORI number is required", targetInfo, messageError);

			declarantEORI.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJE_CustomsOffice()
		{
			var targetInfo = declaration.JE_CustomsOfficeInfo;
			declaration.JE_CustomsOffice = string.Empty;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining("JE_CustomsOffice required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "value";
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageErrorContaining("JE_CustomsOffice provided", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = string.Empty;
			declaration.CustomsEntryHeaders.AddNew().CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			Factory.Save();
		}

		public void TestCheckJE_OA_DeclarantAddress_NoAmend()
		{
			var originalDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			originalDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE001");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_OA_DeclarantAddress = originalDeclarant.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var anotherDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			anotherDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEEORIE002");
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OA_DeclarantAddress = anotherDeclarant.MainAddress.PK;
			AssertHasMessageError("Amend Check", declaration.JE_OA_DeclarantAddressInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_OA_DeclarantAddress = originalDeclarant.MainAddress.PK;
			AssertNoMessageError("Amend Check passes.", declaration.JE_OA_DeclarantAddressInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckJE_EntryStyleAndJE_CustomsOffice_NoAmend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "IEARK100";
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.ValidationModesCalculator.RecalculateValidationModes();
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			declaration.JE_CustomsOffice = "OFF001";
			AssertHasMessageError("Amend check", declaration.JE_EntryStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			AssertHasMessageError("Amend check", declaration.JE_CustomsOfficeInfo, CommonResStrings.ShouldNotAmendThisValue);

			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			declaration.JE_CustomsOffice = "IEARK100";
			AssertNoMessageError("Amend check(passes)", declaration.JE_EntryStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			AssertNoMessageError("Amend check(passes)", declaration.JE_CustomsOfficeInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestValidateAll_NoAmendCheckForDeletedOffices()
		{
			var officeShouldNotBeDeletedMessage = "These Customs Office(s) have been declared to Customs with the Declaration and therefore may not be deleted or changed";

			var instruction = declaration.CustomsEntryInstructions[0];
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			var officeOfPresentation = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "AT100000");
			var officeOfExit = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IEATH200");
			var supervisingOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IEORK100");
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			declaration.ValidationModesCalculator.RecalculateValidationModes();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(declaration, officeShouldNotBeDeletedMessage);

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "AT100000");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IEATH200");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IEORK100");
			declaration.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(declaration, officeShouldNotBeDeletedMessage);
		}

		public void TestCheckJE_CustomsOffice_PresentationOfficeRequired()
		{
			var presentationOfficeRequiredMessage = "Customs office of Presentation is required for centralized clearance (denoted via Authorization Type CCL).";
			var targetInfo = declaration.JE_CustomsOfficeInfo;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageError("Empty EntryInstruction list, no check of Presentation Office.", targetInfo, presentationOfficeRequiredMessage);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CusAuthorizationUsages.AddNew().AGC_Code = "XXX";
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageError("A EntryInstruction but without CCL Authorization, no check of Presentation Office.", targetInfo, presentationOfficeRequiredMessage);

			instruction.CusAuthorizationUsages.AddNew().AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
			var presentationOfficesList = declaration.CustomsOffices.Where(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation).ToList();
			presentationOfficesList.ForEach(x => declaration.CustomsOffices.RemoveAndDelete(x));
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageError("A EntryInstruction but without CCL Authorization, no check of Presentation Office.", targetInfo, presentationOfficeRequiredMessage);
		}

		public void TestCheckJE_TransportMeans_DirectExport()
		{
			declaration.JE_CustomsOffice = "IEDUB400";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IEDUB400");

			var targetInfo = declaration.JE_TransportMeansInfo;
			foreach (var transportModeInland in new TransportTypeList().GetAllCodes())
			{
				declaration.JE_TransportModeInland = transportModeInland;
				declaration.JE_TransportMeans = "AB";
				declaration.Validation.ValidateJE_TransportMeans();
				AssertHasMessageErrorContaining($"For {transportModeInland}, Inland Transport Mode is NOT required.", targetInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckJE_TransportMeans_IndirectExport()
		{
			var targetInfo = declaration.JE_TransportMeansInfo;
			var instruction = declaration.CustomsEntryInstructions[0];

			declaration.JE_CustomsOffice = "IEDUB400";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "ABCDE100");

			CombineAssertions(() =>
			{
				const string error = "The code you have selected is not in the list.";
				foreach (var (transportModeInland, validTransportMeans, transportMeansRequiredElementAttributesForDeclarationTypes) in TransportMeansOptions)
				{
					declaration.JE_TransportModeInland = transportModeInland;

					var notValidTransportMeans = new TransportMeansList().GetAllCodes().Except(validTransportMeans);

					foreach (var (declarationTypes, transportMeansRequiredElementAttribute) in transportMeansRequiredElementAttributesForDeclarationTypes)
					{
						foreach (var declarationType in declarationTypes)
						{
							instruction.CEI_Style = declarationType;

							if (transportMeansRequiredElementAttribute != RequiredElementAttribute.NotRequired)
							{
								foreach (string notValidValue in notValidTransportMeans)
								{
									declaration.JE_TransportMeans = notValidValue;
									declaration.Validation.ValidateJE_TransportMeans();
									AssertHasMessageError($"For {transportModeInland}, {notValidValue} should NOT be valid", targetInfo, error);
								}

								foreach (string validValue in validTransportMeans)
								{
									declaration.JE_TransportMeans = validValue;
									declaration.Validation.ValidateJE_TransportMeans();
									AssertNoMessageError($"For {transportModeInland}, {validValue} should be valid", targetInfo, error);
								}
							}

							if (transportMeansRequiredElementAttribute == RequiredElementAttribute.Optional)
							{
								declaration.JE_TransportMeans = string.Empty;
								declaration.Validation.ValidateJE_TransportMeans();
								AssertNoMessageErrorContaining($"For {transportModeInland} and declaration type {declarationType}, Inland Transport Mode is {transportMeansRequiredElementAttribute}.", targetInfo, MandatoryValidation.YouHaveNotEntered);
								declaration.JE_TransportMeans = "AB";
								declaration.Validation.ValidateJE_TransportMeans();
								AssertNoMessageErrorContaining($"For {transportModeInland} and declaration type {declarationType}, Inland Transport Mode is {transportMeansRequiredElementAttribute}.", targetInfo, MandatoryValidation.DoNotEntered);
							}
							else if (transportMeansRequiredElementAttribute == RequiredElementAttribute.Required)
							{
								declaration.JE_TransportMeans = string.Empty;
								declaration.Validation.ValidateJE_TransportMeans();
								AssertHasMessageErrorContaining($"For {transportModeInland} and declaration type {declarationType}, Inland Transport Mode is {transportMeansRequiredElementAttribute}.", targetInfo, MandatoryValidation.YouHaveNotEntered);
							}
							else if (transportMeansRequiredElementAttribute == RequiredElementAttribute.NotRequired)
							{
								declaration.JE_TransportMeans = "AB";
								declaration.Validation.ValidateJE_TransportMeans();
								AssertHasMessageErrorContaining($"For {transportModeInland} and declaration type {declarationType}, Inland Transport Mode is {transportMeansRequiredElementAttribute}.", targetInfo, MandatoryValidation.DoNotEntered);
							}
						}
					}
				}
			});
		}

		enum RequiredElementAttribute
		{
			Optional,
			Required,
			NotRequired
		}

		static (string TransportModeInland, string[] ValidTransportMeans, (string[] DeclarationTypes, RequiredElementAttribute TransportMeansRequired)[])[] TransportMeansOptions => new[]
				{
			(TransportTypeList.Codes.Air,
			new string[] { TransportMeansList.Codes.IataFlightNumber, TransportMeansList.Codes.RegistrationNumberOfTheAircraft },
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.InlandWaterwayTransport,
			new string[] { TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel },
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.OwnPropulsion,
			new TransportMeansList().GetAllCodes(),
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.Rail,
			new string[] { TransportMeansList.Codes.WagonNumber, TransportMeansList.Codes.TrainNumber },
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.Road,
			new string[] { TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle },
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.Sea,
			new string[] { TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Codes.NameOfTheSeaGoingVessel },
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3 }, RequiredElementAttribute.Required ),
						(new [] { ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.Mail,
			new TransportMeansList().GetAllCodes(),
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1 }, RequiredElementAttribute.Optional ),
				(new [] { ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3, ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 , ExportDeclarationTypeList.Codes.C1, }, RequiredElementAttribute.NotRequired ) }
			),
			(TransportTypeList.Codes.FixedTransportInstallations,
			new TransportMeansList().GetAllCodes(),
			new [] { (new [] { ExportDeclarationTypeList.Codes.B1 }, RequiredElementAttribute.Optional ),
				(new [] { ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Codes.B3, ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Codes.C1 , ExportDeclarationTypeList.Codes.C1, }, RequiredElementAttribute.NotRequired ) }
			),
		};

		public void TestCheckJE_TransportMeans_IndirectExport_ROA_SingleContract()
		{
			var targetInfo = declaration.JE_TransportMeansInfo;
			declaration.JE_CustomsOffice = "IEDUB400";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "ABCDE100");
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = "10000";

			string error = $"When a shipment has a single contract, then {targetInfo.HumanReadableName} is not Allowed";

			declaration.JE_TransportMeans = "AB";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageError("For ROA without a 'Single contract', Inland Transport Mode is allowed.", targetInfo, error);

			var singleContract = invoiceLine.AdditionalInfos.AddNew();
			singleContract.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			singleContract.CSI_Code = "30500";
			declaration.Validation.ValidateJE_TransportMeans();
			AssertHasMessageError($"For ROA with a 'Single contract', Inland Transport Mode is not allowed.", targetInfo, error);

			declaration.JE_TransportMeans = string.Empty;
			declaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageError("For ROA without a 'Single contract', when Inland Transport Mode is empty.", targetInfo, error);
		}

		public void TestCheckJE_LloydsIMO()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;

			var targetInfo = declaration.JE_LloydsIMOInfo;

			declaration.Validation.ValidateJE_LloydsIMO();
			AssertNoMessageErrorContaining("JE_LloydsIMO not mandatory when Border TOI not 10", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.Validation.ValidateJE_LloydsIMO();
			AssertNoMessageErrorContaining("JE_LloydsIMO not mandatory when Border TOI not 10", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.Validation.ValidateJE_LloydsIMO();
			AssertHasMessageErrorContaining("JE_LloydsIMO MANDATORY when Border TOI IS 10", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LloydsIMO = "LLOY";
			AssertNoMessageErrorContaining("JE_LloydsIMO MANDATORY when Border TOI IS 10(validation passed).", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TransportIDInland_Non_ROA()
		{
			declaration.Validation.ValidateJE_TransportIDInland();
			var targetInfo = declaration.JE_TransportIDInlandInfo;

			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("Inland Transport Mode NOT entered, no mandatory check on ID.", targetInfo, " is required when ");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertHasMessageErrorContaining("Inland Transport Mode entered, ID mandatory except for ROA.", targetInfo, " is required when ");
				declaration.JE_TransportIDInland = "TRIL001";
				AssertNoMessageErrorContaining("Inland Transport Mode entered, ID mandatory except for ROA(validation passes).", targetInfo, " is required when ");

				declaration.JE_Trailer1RegNo = "Trailer1";
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
				declaration.JE_TransportIDInland = string.Empty;
				AssertNoMessageErrorContaining("Inland Transport ID not mandatory for ROA.", targetInfo, " is required when ");

				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Ireland;
				declaration.JE_TransportIDInland = string.Empty;
				declaration.JE_TransportIDInland = "Trailer1";
				AssertNoMessageErrorContaining("Nationality entered, ID mandatory.", targetInfo, " is required when ");
			});
		}

		public void TestCheckJE_TransportIDInland_ROA()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_TransportIDInland();
			var targetInfo = declaration.JE_TransportIDInlandInfo;
			var transportIDInlandInfo = declaration.JE_TransportIDInlandInfo;
			var atLeastOneMessage = $"At least one of {transportIDInlandInfo.HumanReadableName} and the Trailer IDs is required.";

			CombineAssertions(() =>
			{
				AssertHasMessageError("At-least-one check against JE_TransportIDInland.", targetInfo, atLeastOneMessage);
				declaration.JE_Trailer2RegNo = "REG2";
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageError("At-least-one check against JE_TransportIDInland(JE_Trailer2RegNo entered, validation passes).", targetInfo, atLeastOneMessage);
				declaration.JE_Trailer2RegNo = string.Empty;

				declaration.JE_TransportIDInland = "TRS001";
				AssertNoMessageError("At-least-one check against JE_TransportIDInland(JE_TransportIDInland entered, validation passes).", targetInfo, atLeastOneMessage);
			});
		}

		public void TestCheckJE_TransportIDInland_MAI()
		{
			var targetInfo = declaration.JE_TransportIDInlandInfo;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;

			var errorDoNotEnter = $"Please do not enter {targetInfo.HumanReadableName} when {declaration.JE_TransportMeansInfo.HumanReadableName} does not have a value";

			declaration.Validation.ValidateJE_TransportIDInland();
			declaration.JE_TransportIDInland = "TRS001";
			AssertHasMessageError("Transport ID should not be entered", targetInfo, errorDoNotEnter);

			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = string.Empty;
			declaration.Validation.ValidateJE_TransportIDInland();
			AssertHasMessageErrorContaining(targetInfo, " is required when ");

			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "TRS001";
			declaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageError(targetInfo, errorDoNotEnter);
			AssertNoMessageErrorContaining(targetInfo, " is required when ");
		}

		public void TestCheckJE_TransportIDInland_FIX()
		{
			var targetInfo = declaration.JE_TransportIDInlandInfo;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;

			var errorDoNotEnter = $"Please do not enter {targetInfo.HumanReadableName} when {declaration.JE_TransportMeansInfo.HumanReadableName} does not have a value";

			declaration.Validation.ValidateJE_TransportIDInland();
			declaration.JE_TransportIDInland = "TRS001";
			AssertHasMessageError("Transport ID should not be entered", targetInfo, errorDoNotEnter);

			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = string.Empty;
			declaration.Validation.ValidateJE_TransportIDInland();
			AssertHasMessageErrorContaining(targetInfo, " is required when ");

			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "TRS001";
			declaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageError(targetInfo, errorDoNotEnter);
			AssertNoMessageErrorContaining(targetInfo, " is required when ");
		}

		public void TestCheckJE_Trailer1RegNo()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_Trailer1RegNo();
			var targetInfo = declaration.JE_Trailer1RegNoInfo;
			var transportIDInlandInfo = declaration.JE_TransportIDInlandInfo;
			var atLeastOneMessage = $"At least one of {transportIDInlandInfo.HumanReadableName} and the Trailer IDs is required.";
			var errorDoNotEnter = $"Please do not enter {targetInfo.HumanReadableName} when {declaration.JE_TransportMeansInfo.HumanReadableName} does not have a value";

			CombineAssertions(() =>
			{
				AssertNoMessageError("No at-least-one check against JE_Trailer1RegNo for SEA.", targetInfo, atLeastOneMessage);

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
				declaration.JE_TransportMeans = "AB";
				declaration.Validation.ValidateJE_Trailer1RegNo();
				AssertHasMessageError("At-least-one check against JE_Trailer1RegNo for ROA.", targetInfo, atLeastOneMessage);

				declaration.JE_Trailer1RegNo = "REG001";
				AssertNoMessageError("At-least-one check against JE_Trailer1RegNo for ROA(validation passes).", targetInfo, atLeastOneMessage);

				declaration.JE_TransportMeans = string.Empty;
				declaration.Validation.ValidateJE_Trailer1RegNo();
				AssertHasMessageError("JE_Trailer1RegNo should not be entered", targetInfo, errorDoNotEnter);
			});
		}

		public void TestCheckJE_Trailer2RegNo()
		{
			var targetInfo = declaration.JE_Trailer2RegNoInfo;
			var transportIDInlandInfo = declaration.JE_TransportIDInlandInfo;
			var atLeastOneMessage = $"At least one of {transportIDInlandInfo.HumanReadableName} and the Trailer IDs is required.";
			var errorDoNotEnter = $"Please do not enter {targetInfo.HumanReadableName} when {declaration.JE_TransportMeansInfo.HumanReadableName} does not have a value";

			CombineAssertions(() =>
			{
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
				declaration.JE_TransportMeans = "AB";
				declaration.Validation.ValidateJE_Trailer2RegNo();
				AssertHasMessageError("At-least-one check against JE_Trailer2RegNo for ROA.", targetInfo, atLeastOneMessage);

				declaration.JE_Trailer2RegNo = "REG001";
				AssertNoMessageError("At-least-one check against JE_Trailer2RegNo for ROA(validation passes).", targetInfo, atLeastOneMessage);

				declaration.JE_TransportMeans = string.Empty;
				declaration.Validation.ValidateJE_Trailer2RegNo();
				AssertHasMessageError("JE_Trailer2RegNo should not be entered", targetInfo, errorDoNotEnter);
			});
		}

		public void TestCheckJE_RN_NKTransportNationalityInland()
		{
			declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			var targetInfo = declaration.JE_RN_NKTransportNationalityInlandInfo;
			var message = " is required when ";

			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("ID NOT entered, NO check on Nationality.", targetInfo, message);
				declaration.JE_TransportIDInland = "TRS001";
				declaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertHasMessageErrorContaining("ID entered, Nationality mandatory.", targetInfo, message);
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Ireland;
				AssertNoMessageErrorContaining("ID entered, Nationality mandatory(validation passes).", targetInfo, message);
			});
		}

		public void TestCheckJE_RN_NKTrailer1Nationality()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			declaration.JE_Trailer1RegNo = "REG001";
			declaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertHasMessageErrorContaining("ID entered, Nationality mandatory.", declaration.JE_RN_NKTrailer1NationalityInfo, " is required when ");
		}

		public void TestCheckJE_RN_NKTrailer2Nationality()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			declaration.JE_Trailer2RegNo = "REG002";
			declaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertHasMessageErrorContaining("ID entered, Nationality mandatory.", declaration.JE_RN_NKTrailer2NationalityInfo, " is required when ");
		}

		public void TestCheckJE_MessageType_ExitOfficeRequired()
		{
			var targetInfo = declaration.JE_MessageTypeInfo;
			var exitOfficeRequiredMessage = "At least one office of type EXT is required in the Customs Offices grid.";

			declaration.Validation.ValidateJE_MessageType();
			AssertHasMessageError("Customs office of Exit Not Provided", targetInfo, exitOfficeRequiredMessage);

			_ = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE000003");
			declaration.Validation.ValidateJE_MessageType();
			AssertNoMessageError("Customs office of Exit", targetInfo, exitOfficeRequiredMessage);
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			var targetInfo = declaration.JE_OH_ShippingLineInfo;
			declaration.JE_EntryStyle = "EX";
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertHasMessageError("Carrier must be entered", targetInfo, MandatoryValidation.MustBeEnteredMessage(targetInfo.Description));

			var carrier = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = carrier.PK;
			AssertHasMessageError("EORI required for carrier", targetInfo, "Carrier must have an EORI.");

			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			var targetInfo = declaration.JE_RL_NKPortOfArrivalInfo;
			declaration.JE_EntryStyle = "EX";
			declaration.JE_RL_NKPortOfArrival = string.Empty;
			AssertHasMessageError("Port of Discharge is required", targetInfo, "Values are required in the Country of Routing of Consignment (Itinerary). Port of Discharge is automatically added to the list of countries specified in the itinerary. Additional countries should be entered on the Misc. Tab in the Itinerary Countries Grid.");
			declaration.JE_RL_NKPortOfArrival = "IRDAY";
			AssertNoMessageError("Port of Discharge has been entered", targetInfo, "Values are required in the Country of Routing of Consignment (Itinerary). Port of Discharge is automatically added to the list of countries specified in the itinerary. Additional countries should be entered on the Misc. Tab in the Itinerary Countries Grid.");
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;

		protected override JobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(declaration);
	}
}
