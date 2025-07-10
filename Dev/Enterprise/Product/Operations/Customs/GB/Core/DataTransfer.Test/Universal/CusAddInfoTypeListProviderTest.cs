using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class CusAddInfoTypeListProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCusAddInfoGetListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobDeclarationSchema.Constants.Prefix, "");
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties, "All Simple Properties", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties));
		}

		public void TestCusAddInfoGetListForCusEntryHeader()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(CusEntryHeaderSchema.Constants.Prefix, "");
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, "Maritime UCN That Is Held", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld));
		}

		public void TestCusAddInfoGetListForJobComInvoiceHeader()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobComInvoiceHeaderSchema.Constants.Prefix, "");
			AssertEquals(0, list.Count);
		}

		public void TestCusAddInfoGetListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix, "");
			AssertEquals(0, list.Count);
		}
	}
}
