using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	public class CusClassPartPivotValidationTest : Customs.Business.Testing.CusClassPartPivotValidationTest
	{
		public void TestCheckCI_PrimaryPreference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);

			var cusPref = helper.CreatePreferenceForCountry("100", "Desc.", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC";
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();

			pivot.PreferenceCode = "100";
			AssertNoMessageErrorContaining(pivot.PreferenceCodeInfo, ListValidation.InvalidCodeMessageError);

			pivot.PreferenceCode = "000";
			AssertHasMessageErrorContaining(pivot.PreferenceCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestAttributesCanMakePivotUnique()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var partRelate1 = part.RelatedOrganisations.AddOwner(org1);
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationType.Both;
			pivot2.CI_ChildType = ClassificationType.Both;
			pivot1.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForNoneHTI);

			var pivot1Attr1 = pivot1.Attributes1.AddNew();
			pivot1Attr1.BG_AttributeValue1 = "1";
			var pivot2Attr1 = pivot2.Attributes1.AddNew();
			pivot2Attr1.BG_AttributeValue1 = "2";
			pivot1.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertNoNotifications(pivot1.CI_ChildTypeInfo);
			AssertNoNotifications(pivot2.CI_ChildTypeInfo);

			pivot1.Attributes1.RemoveAllFromRelationship();
			pivot2.Attributes1.RemoveAllFromRelationship();
			pivot1.CI_ChildType = ClassificationType.IMP;
			pivot2.CI_ChildType = ClassificationType.IMP;
			pivot1.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);

			pivot1Attr1 = pivot1.Attributes1.AddNew();
			pivot1Attr1.BG_AttributeValue1 = "1";
			pivot2Attr1 = pivot2.Attributes1.AddNew();
			pivot2Attr1.BG_AttributeValue1 = "2";
			pivot1.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertNoNotifications(pivot1.CI_ChildTypeInfo);
			AssertNoNotifications(pivot2.CI_ChildTypeInfo);
		}

		public void TestBaseValidationDoesntBreakEU()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var partRelate1 = part.RelatedOrganisations.AddOwner(org1);
			var pivot1 = part.PivotsForBinding.AddNew();

			AssertNoNotifications(pivot1);

			//CheckCI_CC & CheckCI_TariffNum both call ValidateClassTariff which could add an error and prevent EU declarations from being saved. ValidateClassTariff has been overridden in EU to prevent this.
			pivot1.CI_CC = ZGuid.Empty;
			pivot1.CI_CI_Parent = ZGuid.Empty;
			pivot1.CI_TariffNum = ZString.Empty;

			AssertNoNotifications(pivot1);
		}

		public void TestCheckCI_ChildType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			// Validation is suspended during setting of default value BTH, so call it explicitly
			pivot1.Validation.ValidateCI_ChildType();
			AssertHasErrorContaining(pivot1.CI_ChildTypeInfo, "combination");

			pivot1.CI_ChildType = ClassificationType.IMP;
			AssertHasError(pivot1.CI_ChildTypeInfo, "One organization cannot have a Type of 'IMP' and 'BTH', consider adding a type of 'EXP'.");

			pivot2.CI_ChildType = ClassificationType.EXP;
			pivot1.Validation.ValidateCI_ChildType();
			AssertNoNotifications("There should be no notifications.", pivot1.CI_ChildTypeInfo);

			pivot1.CI_ChildType = ClassificationType.Both;
			pivot2.CI_ChildType = ClassificationType.Both;

			var ou1 = part.RelatedOrganisations.AddNew();
			ou1.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var ou2 = part.RelatedOrganisations.AddNew();
			ou2.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			pivot1.CI_OH = ou1.OU_OH;
			pivot1.Validation.ValidateCI_ChildType();
			AssertNoNotifications("Pivot with unique OU shouldn't have notifications.", pivot1.CI_ChildTypeInfo);

			pivot2.CI_OH = ou1.OU_OH;
			pivot2.Validation.ValidateCI_ChildType();
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");

			pivot2.CI_OH = ou2.OU_OH;
			pivot2.Validation.ValidateCI_ChildType();
			AssertNoNotifications("Pivot with unique OU shouldn't have notifications.", pivot2.CI_ChildTypeInfo);
		}

		public void TestCheckCI_ZZF_NKTaxType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("GB1", 0.1, Env.CurrentCompany.Country.Code, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "GBDESC1");
			Factory.Save();

			var testItem = Factory.New<CusClassPartPivot>();
			testItem.Validation.ValidateCI_ZZF_NKTaxType();

			AssertNoMessageErrors(testItem.CI_ZZF_NKTaxTypeInfo);

			testItem.CI_ZZF_NKTaxType = "US1";
			AssertHasMessageErrorContaining(testItem.CI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);

			testItem.CI_ZZF_NKTaxType = "GB1";
			AssertNoMessageErrorContaining(testItem.CI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
