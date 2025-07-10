using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ValuationDeclarationCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPriceDeclarationItemCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var valuationDeclarationCodes = declaration.Invoices.AddNew().ValuationDeclarationCodes.AddNew();
			var lookups = valuationDeclarationCodes.Lookups;

			Assert(lookups.PriceDeclarationItemCodeList.ContainsCode("120"));
		}
	}
}
