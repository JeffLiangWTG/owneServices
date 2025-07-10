using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.DataTransfer.Universal.Testing
{
	class UniversalCustomsDataObjectProviderTest : TestCase
	{
		public void TestTableSpecificCusReferenceTypeList()
		{
			var result = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty);
			AssertEquals(CusReferenceTypeList.Codes.FiscalReference, result.CodesAsString);
			result = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusInBondCargoDescSchema.Constants.Prefix, string.Empty);
			AssertEquals(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, result.CodesAsString);
		}
	}
}
