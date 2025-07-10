using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusAddInfoTypes()
		{
			var cusAddInfoSupporter = (Integration.Customs.ICusAddInfoTypeSupporter)pivot;
			AssertEquals(typeof(TaxOnlyForPivot), cusAddInfoSupporter.GetCusAddInfoTypes()[CusAddInfoTypeAttribute.Codes.GBTax]);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)pivot;
			AssertEquals(typeof(PreviousDocument), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals(typeof(SupportingDocument), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(pivot.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(pivot.AdditionalInfos);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
		}

		public void TestErrorOnInvalidCI_ChildTypeValidation()
		{
			var pivot2 = product.PivotsForBinding.AddNew();
			AssertNoErrors("Pivot1 shouldn't have any notifications.", pivot);
			AssertNoErrors("Pivot2 shouldn't have any notifications.", pivot2.CI_ChildTypeInfo);

			pivot.CI_ChildType = "BTH";
			pivot2.CI_ChildType = "BTH";
			pivot.Validation.ValidateCI_ChildType();
			pivot2.Validation.ValidateCI_ChildType();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");

			pivot.CI_ChildType = "BTH";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateCI_ChildType();
			pivot2.Validation.ValidateCI_ChildType();
			AssertHasError(pivot.CI_ChildTypeInfo, "One organization cannot have a Type of 'IMP' and 'BTH', consider adding a type of 'EXP'.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "One organization cannot have a Type of 'IMP' and 'BTH', consider adding a type of 'EXP'.");

			pivot.CI_ChildType = "IMP";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateCI_ChildType();
			pivot2.Validation.ValidateCI_ChildType();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for IMP classifications. Duplicates are only allowed where Attributes are specified.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for IMP classifications. Duplicates are only allowed where Attributes are specified.");

			pivot.CI_ChildType = "EXP";
			pivot2.CI_ChildType = "EXP";
			pivot.Validation.ValidateCI_ChildType();
			pivot2.Validation.ValidateCI_ChildType();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");

			pivot.CI_ChildType = "EXP";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateCI_ChildType();
			pivot2.Validation.ValidateCI_ChildType();
			AssertNoErrors("Pivot1 shouldn't have any notifications.", pivot);
			AssertNoErrors("Pivot2 shouldn't have any notifications.", pivot2);
		}

		protected override BusinessObject GetNewBusinessObject() => pivot;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => pivot;

		protected override void SetUp()
		{
			product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PRODUCT";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
		}

		OrgSupplierPart product;
		CusClassPartPivot pivot;
	}
}
