using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing;

public class CusCodeDataTypeAndCodeListProviderTest : TestCase
{
	public void TestCusCodeDataGetTypeListForAsycudaManifestHeader()
	{
		var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(AsycudaManifestHeaderSchema.Constants.Prefix, "");
		AssertEquals(1, list.Count);
		AssertEquals(CusCodeDataTypeList.Codes.OfficeCode, CusCodeDataTypeList.Descriptions.OfficeCode, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.OfficeCode));
	}
}
