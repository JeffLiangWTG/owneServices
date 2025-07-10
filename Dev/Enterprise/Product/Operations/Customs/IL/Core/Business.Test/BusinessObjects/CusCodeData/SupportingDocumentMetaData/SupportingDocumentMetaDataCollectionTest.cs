using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(SupportingDocumentMetaDataCollection))]
	sealed class SupportingDocumentMetaDataCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			return supportingDocument.SupportingDocumentMetadataItems;
		}
	}
}
