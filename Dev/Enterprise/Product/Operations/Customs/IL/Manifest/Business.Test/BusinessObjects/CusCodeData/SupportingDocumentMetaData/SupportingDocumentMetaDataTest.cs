using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentMetaData))]
	sealed class SupportingDocumentMetaDataTest : Customs.Business.Testing.CusCodeDataTest<SupportingDocumentMetaData>
	{
		public void TestLookups()
		{
			AssertType<SupportingDocumentMetaDataLookups>(supportingDocument.SupportingDocumentMetadataItems.AddNew().Lookups);
		}

		protected override IEnumerable<SupportingDocumentMetaData> GetBizObjsForCorrectlyTypeDecideTest(
			BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "380";

			yield return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, SupportingDocumentMetaData bizObj)
		{
			var parent = factory.Load<SupportingDocument>(bizObj.CY_ParentID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();
			return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();
			return supportingDocument.SupportingDocumentMetadataItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			header.AMA_ManifestType = "785";
			supportingDocument = header.Bills.AddNew().SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "380";
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		IDisposable disposableAction;
		SupportingDocument supportingDocument;
	}
}
