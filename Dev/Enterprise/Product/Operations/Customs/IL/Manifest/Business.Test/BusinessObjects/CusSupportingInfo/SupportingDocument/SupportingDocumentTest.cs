using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCaptions()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;

			CombineAssertions(() =>
			{
				AssertEquals("CSI_AdditionalDescription caption", "Customs Notes", DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_AdditionalDescriptionInfo).Caption);
				AssertEquals("CSI_AdditionalDescription caption", "Customs Notes", DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_AdditionalDescriptionInfo).Caption);
			});
		}

		public void TestReadOnlyFields()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;

			AssertForReadOnlyAttribute(supportingDocument.CSI_AdditionalDescriptionInfo);
			AssertForReadOnlyAttribute(supportingDocument.CSI_StatusInfo);

			void AssertForReadOnlyAttribute(ZPropertyInfo propertyInfo)
			{
				var isReadOnly = propertyInfo.ReadOnly;
				AssertEquals($"{propertyInfo.Name} ReadOnly", true, isReadOnly);
			}
		}

		public void TestListFields()
		{
			var propertyInfo = typeof(SupportingDocument)
				.GetProperty(nameof(SupportingDocument.CSI_Code));

			var result = propertyInfo
				.GetCustomAttributes(typeof(ListAttribute), false)
				.SingleOrDefault() as ListAttribute;

			AssertNotNull(result);
			AssertEquals("Lookups.CodeList", result.ListDataSourceMember);
		}

		public void TestLookups()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			AssertType<SupportingDocumentLookups>(supportingDocument.Lookups);
		}

		public void TestDelete()
		{
			var factory = Factory;
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			var supportingDocumentPK = supportingDocument.PK;
			var loadResult = Factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("[Pre-requisite]: pivot not exist", loadResult);

			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			factory.Save();

			loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNotNull("pivot exist", loadResult);

			bill.SupportingDocuments.RemoveAndDelete(supportingDocument);
			factory.Save();

			loadResult = factory.LoadTop1<BaseCusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, supportingDocumentPK));
			AssertNull("pivot removed", loadResult);
		}

		public void TestSupportingDocumentMetaDatas()
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			AssertType<SupportingDocumentMetaDataCollection>(supportingDocument.SupportingDocumentMetadataItems);
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

		public void TestDocument()
		{
			var supportingDocument = bill.SupportingDocuments.AddNew();
			AssertNull("when EDoc is empty", supportingDocument.Document);
			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			var document = supportingDocument.Document;
			AssertNotNull("when Document is not empty", document);
			AssertEquals("FileName", "Invoice1.pdf", document.FileName);
			AssertEquals("ImageData", "Hello world", Encoding.UTF8.GetString(document.ImageData));

			supportingDocument.EDoc = ZGuid.BrettsGuid;
			AssertNull("when EDoc not in the list", supportingDocument.Document);
		}

		public void TestValidation()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			AssertType<SupportingDocumentValidation>("Validation must be of the expected type", supportingDocument.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return bill.SupportingDocuments.AddNew();
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var supportingDocument = GetNewBusinessObject() as SupportingDocument;
			Factory.Save();
			yield return supportingDocument;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			bill = header.Bills.AddNew();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		AsycudaBill bill;
		IDisposable disposableAction;
		AsycudaManifestHeader header;
	}
}
