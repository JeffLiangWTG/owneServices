using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			return new SupportingDocumentCollection(bill);
		}
	}
}
