using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	class EntryHeaderFilterBusinessObjectLookupsTest : TestCaseWithFactory
	{
		[TestDate(2019, 12, 26)]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNJ", "Nanjing Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CSH", "Shanghai Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 25));
			helper.CreateNewOrGetExistingCusCodeList("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNY", "New York Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			Factory.Save();
			var customsOfficeList = lookups.CustomsOfficeList;
			customsOfficeList.Load();
			AssertEquals(1, customsOfficeList.Count);
			AssertEquals("Customs Office must be 'CNJ'", "CNJ", customsOfficeList[0].ZZD_Code);
		}

		public void TestMessageStatusList()
		{
			var messageStatusList = lookups.MessageStatusList();
			AssertEquals("Codes", "NOT, SNT, UNK, AWO, AWP, AWM, ACO, ACP, ACM, CLO, CLP, CLM, ERO, ERP, ERM", messageStatusList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var filterBo = new EntryHeaderFilterBusinessObject();
			lookups = new EntryHeaderFilterBusinessObjectLookups(filterBo);
		}

		EntryHeaderFilterBusinessObjectLookups lookups;
	}
}
