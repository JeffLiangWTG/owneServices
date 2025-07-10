using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocument))]
	sealed class AdditionalDocumentTest : CusSupportingInfoTest<AdditionalDocument>
	{
		public void TestValidation()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			AssertType<AdditionalDocumentValidation>(additionalDocument.Validation);
		}

		public void TestLookups()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			AssertType<AdditionalDocumentLookups>(additionalDocument.Lookups);
		}

		protected override IEnumerable<AdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			yield return header.AdditionalDocuments.AddNew();
		}
	}
}
