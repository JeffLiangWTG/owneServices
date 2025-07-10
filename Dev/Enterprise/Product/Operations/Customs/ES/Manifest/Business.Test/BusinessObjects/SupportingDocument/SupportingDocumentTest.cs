using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestValidation()
		{
			var supDoc = (SupportingDocument)GetNewBusinessObject();
			AssertEquals("Validation", typeof(SupportingDocumentValidation), supDoc.Validation.GetType());
		}

		public void TestLookups()
		{
			var supDoc = (SupportingDocument)GetNewBusinessObject();
			AssertEquals("Lookups", typeof(SupportingDocumentLookups), supDoc.Lookups.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			var supDoc = bill.SupportingDocuments.AddNew();
			yield return supDoc;
		}
	}
}
