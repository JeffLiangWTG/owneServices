using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExtendedHoursRequestLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Lookups.Declaration, declaration);
		}

		public void TestCodeDescriptionPairList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var line = header.ExtendedHoursRequestLines.AddNew();

			var bondedAreaCodeList = line.Lookups.BondedAreaCodeList;
			bondedAreaCodeList.Load();
			AssertEquals(1, bondedAreaCodeList.Count);
			Assert(bondedAreaCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "11111111"));
			Assert(bondedAreaCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "경의선철도 입출경검사장(지상)"));
		}
	}
}
