using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCaptions()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			CombineAssertions(() =>
			{
				AssertEquals("EDoc caption", "eDoc", DataBoundResourceStrings.GetDataForProperty(supportingDocument.EDocInfo).Caption);
				AssertEquals("CSI_ReferenceNumber2 caption", "Customs Doc ID", DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_ReferenceNumber2Info).Caption);
			});
		}

		public void TestListAttributes()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(SupportingDocument), "EDoc", false, attrib => attrib.ListDataSourceMember == "Lookups.EDocList");
		}

		public void TestEDoc()
		{
			var factory = Factory;
			var supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
			var supportingDocumentPK = supportingDocument.PK;
			var loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("when eDoc is empty", loadResult);

			var docManagerInfo = jobDeclaration.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.docx", "CIV");

			supportingDocument.EDoc = eDoc1.UniqueKey;
			loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNotNull("when eDoc is not empty", loadResult);
			AssertEquals("PDF", loadResult.CSD_DocType);

			supportingDocument.EDoc = eDoc2.UniqueKey;
			var listOfPivot = factory.Load<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertEquals("when eDoc is updated", 1, listOfPivot.Length);
			loadResult = listOfPivot.Single();
			AssertEquals("DOCX", loadResult.CSD_DocType);

			supportingDocument.EDoc = ZGuid.Empty;
			loadResult = Factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("when eDoc is removed", loadResult);

			supportingDocument.EDoc = eDoc2.UniqueKey;
			listOfPivot = factory.Load<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertEquals("when eDoc is revive", 1, listOfPivot.Length);
		}

		public void TestDelete()
		{
			var factory = Factory;
			var jobDeclaration = factory.New<JobDeclaration>();
			var supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
			var supportingDocumentPK = supportingDocument.PK;
			var loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("[Pre-requisite]: pivot not exist", loadResult);

			var docManagerInfo = jobDeclaration.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			factory.Save();

			loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNotNull("pivot exist", loadResult);

			jobDeclaration.SupportingDocuments.RemoveAndDelete(supportingDocument);
			factory.Save();

			loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("pivot removed", loadResult);
		}

		public void TestICusCodeDataTypeSupporter_GetCusCodeDataTypes()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			var actualTypes = ((Integration.Customs.ICusCodeDataTypeSupporter)supportingDocument).GetCusCodeDataTypes();
			AssertEquals(typeof(SupportingDocumentMetaData), actualTypes["SUP"]);
		}

		public void TestICusCodeDataTypeSupporter_GetFetchStrategies()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;

			var expectedTypes = new[] { typeof(Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy) };
			var actualTypes = ((Integration.Customs.ICusCodeDataTypeSupporter)supportingDocument).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestSupportingDocumentMetaData()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			AssertType<SupportingDocumentMetaDataCollection>(supportingDocument.SupportingDocumentMetadataItems);
		}

		public void TestValidationType()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			AssertType<SupportingDocumentValidation>(supportingDocument.Validation);
		}

		public void TestReadOnlyFields()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			AssertForReadOnlyAttribute(supportingDocument.CSI_ReferenceNumber2Info);
		}

		public void TestUpdateSupportingDocumentMetaDatas()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code830 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "830", ZDateTime.BrettsBirthday, ZDateTime.Today);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("3", "Invoice Country", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MetaDatasMandatory", "Metadatas Mandatory", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);

			helper.CreateCusCodeListAttribute(code830.PK, "3", "", false);
			helper.CreateCusCodeListAttribute(code830.PK, "14", "", false);
			helper.CreateCusCodeListAttribute(code830.PK, "MetaDatasMandatory", "14", false);

			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "MetaDatasMandatory", "18", false);
			factory.Save();

			var supportingDocument = Factory.New<SupportingDocument>();
			AssertEquals(0, supportingDocument.SupportingDocumentMetadataItems.Count);

			supportingDocument.CSI_Code = "380";
			AssertEquals(2, supportingDocument.SupportingDocumentMetadataItems.Count);
			Assert("Importer VAT must not be mandatory when Supporting Document Type is 380", !supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().FirstOrDefault(x => x.CY_Code == "14").Mandatory);
			Assert("Exporter Name must be mandatory when Supporting Document Type is 380", supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().FirstOrDefault(x => x.CY_Code == "18").Mandatory);

			supportingDocument.CSI_Code = "830";
			AssertEquals(2, supportingDocument.SupportingDocumentMetadataItems.Count);
			Assert("Importer VAT must be mandatory when Supporting Document Type is 830", supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().FirstOrDefault(x => x.CY_Code == "14").Mandatory);
			Assert("Invoice Country must not be mandatory when Supporting Document Type is 830", !supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().FirstOrDefault(x => x.CY_Code == "3").Mandatory);
		}

		public void TestDocument()
		{
			var supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
			AssertNull("when EDoc is empty", supportingDocument.Document);
			var docManagerInfo = jobDeclaration.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			var document = supportingDocument.Document;
			AssertNotNull("when Document is not empty", document);
			AssertEquals("FileName", "Invoice1.pdf", document.FileName);
			AssertEquals("ImageData", "Hello world", Encoding.UTF8.GetString(document.ImageData));

			supportingDocument.EDoc = ZGuid.BrettsGuid;
			AssertNull("when EDoc not in the list", supportingDocument.Document);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return jobDeclaration.SupportingDocuments.AddNew();
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
			Factory.Save();
			yield return supportingDocument;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		}

		void AssertForReadOnlyAttribute(ZPropertyInfo propertyInfo)
		{
			var isReadOnly = propertyInfo.ReadOnly;
			AssertEquals($"{propertyInfo.Name} ReadOnly", true, isReadOnly);
		}

		JobDeclaration jobDeclaration;
	}
}
