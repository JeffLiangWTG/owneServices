using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ContractRevocation5ULLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			var codeList = (CodeDescriptionPairList)contractRevocation5UL.Lookups.CodeList;
			AssertEquals(4, codeList.Count);
			Assert(codeList.ContainsCode(CancelReasonCodeList.Codes.A));
			Assert(codeList.ContainsCode(CancelReasonCodeList.Codes.B));
			Assert(codeList.ContainsCode(CancelReasonCodeList.Codes.C));
			Assert(codeList.ContainsCode(CancelReasonCodeList.Codes.D));
		}
	}
}
