using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	class CusReferenceTypeListProviderTest : TestCase
	{
		public void TestCusReferenceGetListForEntryInstruction()
		{
			var list = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty);
			AssertEquals(CusReferenceTypeList.Codes.FiscalReference, list.CodesAsString);
		}

		public void TestCusReferenceGetListForUnknown()
		{
			var list = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(JobDeclarationSchema.Constants.Prefix, string.Empty);
			AssertNull(list);
		}
	}
}
