using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSCusSupportingInfoTypeListProviderTest : TestCase
	{
		public void TestTableSpecificCusSupportingInfoTypeList()
		{
			var p = new EMCSCusSupportingInfoTypeListProvider();
			var list = p.TableSpecificCusSupportingInfoTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(2, list.Count);
			AssertEquals(CusSupportingInfoTypeList.Codes.ImportSad, CusSupportingInfoTypeList.Descriptions.ImportSad, list.GetDescriptionFromCode(CusSupportingInfoTypeList.Codes.ImportSad));
			AssertEquals(CusSupportingInfoTypeList.Codes.Certificate, CusSupportingInfoTypeList.Descriptions.Certificate, list.GetDescriptionFromCode(CusSupportingInfoTypeList.Codes.Certificate));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new EMCSUniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
