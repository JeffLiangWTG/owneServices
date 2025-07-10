using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationValidation))]
	class JobDeclarationValidationBaseOnlyTest : JobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_UCR()
		{
			var dec1 = Factory.New<JobDeclaration>();
			Factory.Save();
			dec1.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			dec1.JE_UCR = ZString.Empty;
			dec1.Validation.ValidateJE_UCR();
			AssertNoMessageErrorContaining("Do not perform validation on UCR when Export", dec1.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);

			dec1.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			dec1.JE_UCR = ZString.Empty;
			dec1.Validation.ValidateJE_UCR();
			AssertNoMessageErrorContaining("Do not perform validation on UCR when Import", dec1.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);

			dec1.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			dec1.JE_UCR = ZString.Empty;
			dec1.Validation.ValidateJE_UCR();
			AssertHasMessageErrorContaining("Perform validation on UCR when MiscellaneousCustoms", dec1.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateZG_SpecificCircumstanceIndicatorForListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var specificCircumstanceIndicatorType = Constants.RefCusCodeListTypes.SpecificCircumstanceIndicatorType;
			helper.CreateNewOrGetExistingCusCodeType(specificCircumstanceIndicatorType, "Ireland SpecificeCircumstanceIndicatorType");
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			var irelandCode = Core.Constants.CountryCodes.Ireland;
			helper.CreateNewOrGetExistingDataGrouping(irelandCode, parent: grouping);
			helper.CreateCusCodeList(irelandCode, specificCircumstanceIndicatorType, "A20", "Ireland SpecificeCircumstanceIndicatorType Type 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, "@", "A20");
		}

		public void TestValidateInvoiceLinesForOverlappingPackSets()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "CT";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "CT";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;

			var message = "Where there are Invoice Lines that have at least one common package, then the Invoice Lines must be linked to the same set of packages.";

			var lineLinkPachage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			lineLinkPachage1.IsLinked = true;
			var line2LinkPachage2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
			line2LinkPachage2.IsLinked = true;
			ValidateAll();
			CombineAssertions("No common pack", () =>
			{
				AssertNoRowMessageError("Pack1", invoiceLine, message);
				AssertNoRowMessageError("Pack2", invoiceLine2, message);
			});

			var lineLinkPachage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			lineLinkPachage2.IsLinked = true;
			ValidateAll();
			CombineAssertions("Not all packs are in common", () =>
			{
				AssertHasRowMessageError("Pack1 and Pack2", invoiceLine, message);
				AssertHasRowMessageError("Pack2", invoiceLine2, message);
			});

			var line2LinkPachage1 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			line2LinkPachage1.IsLinked = true;
			ValidateAll();
			CombineAssertions("All packs are in common", () =>
			{
				AssertNoRowMessageError("Pack1 and Pack2", invoiceLine, message);
				AssertNoRowMessageError("Pack1 and Pack2", invoiceLine2, message);
			});

			void ValidateAll()
			{
				declaration.Validation.ValidateAll();
				invoiceLine.Validation.ValidateAll();
				invoiceLine2.Validation.ValidateAll();
			}
		}

		public void TestEUD_AgreedPlaceCode_Export()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.EUD_AgreedPlaceCode = ZString.Empty;
			AssertNoMessageErrors("Do not perform validation on EUD_AgreedPlaceCode when declaration.isExport == true", declaration.EUD_AgreedPlaceCodeInfo);
		}

		public void TestEUD_AgreedPlaceCode_UCC5Import()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				declaration.EUD_AgreedPlaceCode = ZString.Empty;
				AssertNoMessageErrors("Do not perform validation on EUD_AgreedPlaceCode when declaration.isExport == true", declaration.EUD_AgreedPlaceCodeInfo);
			}
		}

		public void TestCheckJE_TransportMeans()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = "OWN";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TransportMeansInfo);
		}

		public void TestCheckJE_TransportModeInland_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_TransportModeInlandInfo, "!", Core.Constants.TransportModes.FixedTransportInstallations);
		}

		public void TestCheckJE_RN_NKTransportNationality_NotMandatory_H2_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			declaration.JE_RN_NKTransportNationality = string.Empty;

			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_RN_NKTransportNationalityInfo,
				"Nationality rule should not trigger when DeclarationType is 'H2' and Nationality is empty for imports (UCC5).");
		}

		public void TestCheckJE_RN_NKTransportNationality_Mandatory()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_RN_NKTransportNationalityInfo);
		}

		public void TestCheckJE_RN_NKTransportNationality_NotMandatory_FIX() => AssertJE_RN_NKTransportNationality_NotMandatory(Core.Constants.TransportModes.FixedTransportInstallations);

		public void TestCheckJE_RN_NKTransportNationality_NotMandatory_MAI() => AssertJE_RN_NKTransportNationality_NotMandatory(Core.Constants.TransportModes.Mail);

		public void TestCheckJE_RN_NKTransportNationality_NotMandatory_RAI() => AssertJE_RN_NKTransportNationality_NotMandatory(Core.Constants.TransportModes.Rail);

		void AssertJE_RN_NKTransportNationality_NotMandatory(string transportMode)
		{
			declaration.JE_TransportMode = transportMode;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_RN_NKTransportNationalityInfo);
		}

		public void TestCheckJE_CustomsOffice_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IE000001",
				description: "Customs Office IE000001",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
			Factory.Save();

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_CustomsOfficeInfo, "XXX", "IE000001");

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_CustomsOfficeInfo, "XXX", "IE000001");
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsOfficeInfo, "XXX", "IE000001");
		}

		public void TestCheckJE_LocationOfGoods_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOfGoodsInfo, "XXXXXXXX", "IEDFA");
		}

		public void TestCheckJE_LocationQualifier_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.RefCusCodeListTypes.IrelandQualifierType, "Qualifier Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Constants.RefCusCodeListTypes.IrelandQualifierType, "A", "Qualifier Type A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationQualifierInfo, "XXX", "A");
		}

		public void TestCheckJE_LocationOtherInformation_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var goodsLocationType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			helper.CreateNewOrGetExistingCusCodeType(goodsLocationType, "Goods Location");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, goodsLocationType, "U", "Location Typ U", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOtherInformationInfo, "XXX", "U");
		}

		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationValidation GetValidation() => new JobDeclarationValidation(declaration);
	}
}
