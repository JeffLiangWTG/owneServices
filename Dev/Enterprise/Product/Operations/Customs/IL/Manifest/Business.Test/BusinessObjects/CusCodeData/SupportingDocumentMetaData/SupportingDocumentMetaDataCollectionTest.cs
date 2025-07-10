using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentMetaDataCollection))]
	sealed class SupportingDocumentMetaDataCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAddNewType()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.AddNew();
			AssertType<SupportingDocumentMetaData>(supportingDocumentMetaData);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			return supportingDocument.SupportingDocumentMetadataItems;
		}
	}
}
