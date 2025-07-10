using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocument))]
	sealed class AdditionalDocumentTest : CusSupportingInfoTest<AdditionalDocument>
	{
		public void TestLookups()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			AssertType<AdditionalDocumentLookups>(additionalDocument.Lookups);
		}

		protected override IEnumerable<AdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			yield return bill.AdditionalDocuments.AddNew();
		}
	}
}
