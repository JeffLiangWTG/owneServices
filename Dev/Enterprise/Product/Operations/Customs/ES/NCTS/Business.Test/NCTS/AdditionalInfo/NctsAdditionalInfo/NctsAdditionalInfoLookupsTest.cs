using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalInfoCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType("AINN", "ES Additional Infos - NCTS");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("ES");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);
			helper.CreateCusCodeList("ES", "AINN", "DG0", "EC export under restriction (Art. 843 R/EU 2454/93)", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", "AINN", "DG1", "Export from the EC subject to payment of duty (Article 843 R/EU 2454/93)", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", "AINN", "DG2", "Export", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();

			var additionalInfo = Factory.New<NctsAdditionalInfo>();
			AssertEquals("lookups.CodeList.CodesAsString", "DG0, DG1, DG2", ((CodeDescriptionPairList)additionalInfo.Lookups.CodeList).CodesAsString);
		}
	}
}
