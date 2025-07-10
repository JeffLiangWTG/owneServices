using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(SupportingDocumentMetaData))]
	sealed class SupportingDocumentMetaDataTest : Customs.Business.Testing.CusCodeDataTest<SupportingDocumentMetaData>
	{
		public void TestLookups()
		{
			AssertType<SupportingDocumentMetaDataLookups>(SupportingDocumentMetaData.Lookups);
		}

		public void TestValidation()
		{
			AssertType<SupportingDocumentMetaDataValidation>(SupportingDocumentMetaData.Validation);
		}
		public void TestCaptions()
		{
			AssertCaptions("CY_Code", "Code");
			AssertCaptions("CY_Data", "Value");
		}

		public void TestReadOnlyFields()
		{
			AssertForReadOnlyAttribute(SupportingDocumentMetaData.MandatoryInfo);
		}

		public void TestMandatory()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MetaDatasMandatory", "Metadatas Mandatory", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);

			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "MetaDatasMandatory", "18", false);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "380";
			factory.Save();

			AssertEquals(2, supportingDocument.SupportingDocumentMetadataItems.Count);
			var supportingDocumentMetadataItem14 = supportingDocument.SupportingDocumentMetadataItems.GetFirstElementHaving("14");
			Assert("supportingDocumentMetadataItem 14 Mandatory is false", !supportingDocumentMetadataItem14.Mandatory);
			var supportingDocumentMetadataItem18 = supportingDocument.SupportingDocumentMetadataItems.GetFirstElementHaving("18");
			Assert("supportingDocumentMetadataItem 18 Mandatory is true", supportingDocumentMetadataItem18.Mandatory);

			var newFactory = new BusinessObjectFactory();
			var newSupportingDocumentMetadataItem14 = newFactory.Load<SupportingDocumentMetaData>(supportingDocumentMetadataItem14.PK);
			Assert("supportingDocumentMetadataItem 14 Mandatory is false when reload", !newSupportingDocumentMetadataItem14.Mandatory);
			var newSupportingDocumentMetadataItem18 = newFactory.Load<SupportingDocumentMetaData>(supportingDocumentMetadataItem18.PK);
			Assert("supportingDocumentMetadataItem 18 Mandatory is true when reload", newSupportingDocumentMetadataItem18.Mandatory);
		}

		protected override IEnumerable<SupportingDocumentMetaData> GetBizObjsForCorrectlyTypeDecideTest(
			BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "380";
			yield return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, SupportingDocumentMetaData bizObj)
		{
			var res = factory.Load<SupportingDocument>(bizObj.CY_ParentID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(SupportingDocumentMetaData), propertyName).Caption);
		}

		void AssertForReadOnlyAttribute(ZPropertyInfo propertyInfo)
		{
			var isReadOnly = propertyInfo.ReadOnly;
			AssertEquals($"{propertyInfo.Name} ReadOnly", true, isReadOnly);
		}

		SupportingDocumentMetaData SupportingDocumentMetaData => supportingDocumentMetaData ?? (supportingDocumentMetaData = Factory.New<SupportingDocumentMetaData>());
		SupportingDocumentMetaData supportingDocumentMetaData;
	}
}
