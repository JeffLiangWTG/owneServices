using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class CertificateOfOriginTypeListTest : TestCaseWithFactory
	{
		public void TestIsSmallAmountGoods()
		{
			AssertEquals("IsSmallAmountGoods should return true for X.", true, CertificateOfOriginTypeList.IsSmallAmountGoods(CertificateOfOriginTypeList.Codes.SmallAmountGoods));
			AssertEquals("IsSmallAmountGoods should return false for C.", false, CertificateOfOriginTypeList.IsSmallAmountGoods(CertificateOfOriginTypeList.Codes.CertificateOfOrigin));
			AssertEquals("IsSmallAmountGoods should return false for D.", false, CertificateOfOriginTypeList.IsSmallAmountGoods(CertificateOfOriginTypeList.Codes.DeclarationOfOrigin));
			AssertEquals("IsSmallAmountGoods should return false for unexpected input.", false, CertificateOfOriginTypeList.IsSmallAmountGoods("ZZZ"));
		}
	}
}
