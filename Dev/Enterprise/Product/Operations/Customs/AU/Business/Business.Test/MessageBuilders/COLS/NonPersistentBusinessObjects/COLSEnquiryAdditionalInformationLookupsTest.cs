using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSEnquiryAdditionalInformationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEnquiryType()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var list = new COLSEnquiryAdditionalInformation(colsHeader).Lookups.EnquiryType;
			AssertType<COLSEnquiryTypeList>(list);
		}
	}
}
