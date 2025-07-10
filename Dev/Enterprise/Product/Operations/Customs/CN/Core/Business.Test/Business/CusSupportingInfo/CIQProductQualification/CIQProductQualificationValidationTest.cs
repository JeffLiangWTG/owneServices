using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CIQProductQualificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var tetItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testItem1 = tetItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testInfo1 = testItem1.CSI_CodeInfo;
			testItem1.Validation.ValidateCSI_Code();
			AssertHasErrorContaining(testInfo1, MandatoryValidation.MustBeEntered);
			testItem1.CSI_Code = "XXX";
			AssertHasMessageErrorContaining(testInfo1, ListValidation.InvalidCodeMessageError);
			testItem1.CSI_Code = "203";
			AssertNoMessageErrors(testInfo1);
			testItem1.CSI_Code = "408";
			AssertHasMessageErrorContaining(testInfo1, "VIN data is required for the selected Product Qualification Type.");
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var testItem2 = tetItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testInfo2 = testItem2.CSI_CodeInfo;
			testItem2.Validation.ValidateCSI_Code();
			AssertHasErrorContaining(testInfo2, MandatoryValidation.MustBeEntered);
			testItem2.CSI_Code = "XXX";
			AssertHasMessageErrorContaining(testInfo2, ListValidation.InvalidCodeMessageError);
			testItem2.CSI_Code = "103";
			AssertNoMessageErrors(testInfo2);
			testItem2.Parent.VINDataCollection.AddNew();
			testItem2.Validation.ValidateCSI_Code();
			AssertNoMessageErrors(testInfo2);
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var pq2 = testItem1.Parent.CIQProductQualifications.AddNew();
			AssertNoMessageErrors(pq2.CSI_CodeInfo);
			pq2.CSI_Code = "603";
			pq2.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(pq2.CSI_CodeInfo, "Only one of 408/409/603 should be entered.");
			pq2.CSI_Code = "408";
			AssertHasWarningContaining(pq2.CSI_CodeInfo, "The Product Qualification Type has been duplicated.");
			var invoiceLine = tetItems.InvoiceLine;
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			var cargoAttribute = Factory.New<CargoAttribute>();
			cargoAttribute.CY_ParentID = invoiceLine.PK;
			cargoAttribute.CY_ParentTableCode = invoiceLine.TablePrefix;
			cargoAttribute.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			cargoAttribute.CY_Code = CargoAttributeList.Codes._21;
			invoiceLine.CargoAttributes.Add(cargoAttribute);
			var firstAttribute = invoiceLine.CargoAttributes[0];
			var qualification = invoiceLine.CIQProductQualifications.AddNew();
			qualification.CSI_Code = ProductQualificationCodeList.Codes._422;
			qualification.Validation.ValidateCSI_Code();
			AssertHasWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 20 needs to be selected for this Product Qualification");
			firstAttribute.CY_Code = CargoAttributeList.Codes._20;
			qualification.Validation.ValidateCSI_Code();
			AssertNoWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 20 needs to be selected for this Product Qualification");
			firstAttribute.CY_Code = CargoAttributeList.Codes._21;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._401;
			qualification.Validation.ValidateCSI_Code();
			AssertNoWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 20 needs to be selected for this Product Qualification");
			firstAttribute.CY_Code = CargoAttributeList.Codes._20;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._402;
			qualification.Validation.ValidateCSI_Code();
			AssertHasWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 21 needs to be selected for this Product Qualification");
			qualification.CSI_Code = ProductQualificationCodeList.Codes._423;
			qualification.Validation.ValidateCSI_Code();
			AssertHasWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 21 needs to be selected for this Product Qualification");
			firstAttribute.CY_Code = CargoAttributeList.Codes._21;
			qualification.Validation.ValidateCSI_Code();
			AssertNoWarningContaining(qualification.CSI_CodeInfo, "Cargo Attribute 21 needs to be selected for this Product Qualification");
		}

		public void TestCheckCSI_CodeSupportingProductQualificationCode()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			var qualification = invoiceLine.CIQProductQualifications.AddNew();
			testItems.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			testItems.JobDeclaration.JE_InspectionInvolved = true;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._103;
			qualification.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(qualification.CSI_CodeInfo, "This Product Qualification Type is not supported in two-step declaration clearance mode.");
			testItems.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			testItems.JobDeclaration.JE_InspectionInvolved = true;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._330;
			qualification.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining(qualification.CSI_CodeInfo, "This Product Qualification Type is not supported in two-step declaration clearance mode.");
			testItems.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			testItems.JobDeclaration.JE_InspectionInvolved = true;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._103;
			qualification.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining(qualification.CSI_CodeInfo, "This Product Qualification Type is not supported in two-step declaration clearance mode.");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testItem = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testInfo = testItem.CSI_ReferenceNumberInfo;
			testItem.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
			testItem.CSI_ReferenceNumber = "DOC1001";
			AssertNoMessageErrors(testInfo);
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "030", "Metres", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testItem = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testInfo = testItem.CSI_UnitOfQuantityInfo;
			testItem.CSI_Quantity = 0;
			testItem.CSI_UnitOfQuantity = "";
			testItem.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrors(testInfo);
			testItem.CSI_Quantity = 2;
			testItem.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(testInfo, "Unit is required when quantity is greater than 0");
			AssertEquals(1, testItem.Lookups.UnitOfMeasurementList.Count);
			testItem.CSI_UnitOfQuantity = CustomsUnitOfMeasurementListHelper.Codes.Metres;
			AssertNoMessageErrors(testInfo);
			testItem.CSI_UnitOfQuantity = "AAa";
			AssertHasMessageErrorContaining(testInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidationModeProvider()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var testItem1 = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			var validation = testItem1.Validation as CIQProductQualificationValidation;

			ValidationExtensionsTest.AssertValidationModeProvider(testItems.JobDeclaration, validation.ValidationModeProvider);

			var testItem2 = Factory.New<CIQProductQualification>();
			AssertNull((testItem2.Validation as CIQProductQualificationValidation).ValidationModeProvider);
		}
	}
}
