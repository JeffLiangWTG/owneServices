using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStorageHeader))]
	public abstract class TemporaryStorageHeaderAbstractTest<T> : AsycudaManifestHeaderTest
		where T : TemporaryStorageHeader
	{
		public void TestEnsureCusGoodsLocationIsCorrectlySetup()
		{
			var header = (T)GetNewBusinessObjectForDeleteTest(Factory);
			var goodsLocationTypeSupporter = (ICusGoodsLocationTypeSupporter)header;
			var goodsLocationType = goodsLocationTypeSupporter.GoodsLocationType;
			var goodsLocationTypeFromTypeDecider = CusGoodsLocation.TypeDecider.GetTypeForCountryCode(header.AMA_RN_NKCountry);
			AssertEquals(
				$"TemporaryStorageHeader.GoodsLocationType and {typeof(CusGoodsLocationTypeDecider).FullName}.GetTypeForCountryCode({header.AMA_RN_NKCountry}) should match",
				goodsLocationTypeFromTypeDecider,
				goodsLocationType);
			var goodsLocation = header.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			goodsLocation.CGL_AdditionalIdentifier = "123";
			Factory.Save();
			AssertType(goodsLocationType, new BusinessObjectFactory().Load<TemporaryStorageHeader>(header.PK).GoodsLocation);
		}
	}
}
