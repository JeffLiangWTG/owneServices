using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACFIAMiscCodesCollection))]
	sealed class CACFIAMiscCodesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CACFIAMiscCodesCollection>
	{
		ZZRefCusCodeListCombined CreateRefCusCodeListForCACFIAMiscCodes(BusinessObjectFactory factory, ZString code, string description = "")
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			cusCodeList.ZZD_CodeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CFIAMiscCodes;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);

			return cusCodeList;
		}

		public CACFIAMiscCodes CreateFIAMiscCodes(BusinessObjectFactory factory, ZString code, string description = "")
		{
			return new CACFIAMiscCodes(CreateRefCusCodeListForCACFIAMiscCodes(factory, code, description));
		}

		public void TestLoad()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateCusCodeType("SUBLC", "Sub Location Codes");
			helper.CreateCusCodeType("CUSOF", "Customs Office");
			helper.CreateCusCodeType("CFIAM", "CFIA Misc Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "1111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "2222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CUSOF", "3333", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, "CFIAM", "4444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "5555", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "6666", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			otherFactory.Save();

			var locationCollection = new CACFIAMiscCodesCollection(Factory);
			locationCollection.Load();
			var expectedCodes = new[] { "2222", "5555", "6666" };
			var actualCodes = locationCollection.Cast<CACFIAMiscCodes>().Select(c => c.Code);
			AssertEquals("CACFIAMiscCodesCollection.Count", 3, locationCollection.Count);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		protected override CACFIAMiscCodesCollection GetCollectionToTest()
		{
			return new CACFIAMiscCodesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CreateFIAMiscCodes(Factory, "XXX");
		}
	}
}
