using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CcsUkToCdsLocationConverterTest : TestCaseWithFactory
	{
		public void TestCalculateCdsLocationWithOutZZDB()
		{
			var converter = new CcsUkToCdsLocationConverter("test");
			AssertEquals("GB", converter.CalculateCdsLocation(Factory));
		}

		public void TestCalculateCdsLocationWithOutQUALF()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var codelist = helper.CreateNewOrGetExistingCusCodeList(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "MNCMANDHXCUK", "MNCMANDHXCUK", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CCSUK, "test");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			Factory.Save();
			var converter = new CcsUkToCdsLocationConverter("test");
			AssertEquals("GBAU  MNCMANDHXCUK", converter.CalculateCdsLocation(Factory));
		}

		public void TestCalculateCdsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var codelist = helper.CreateNewOrGetExistingCusCodeList(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "MNCMANDHXCUK", "MNCMANDHXCUK", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CCSUK, "test");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.QUALF, "QF");
			Factory.Save();
			var converter = new CcsUkToCdsLocationConverter("test");
			AssertEquals("GBAUQFMNCMANDHXCUK", converter.CalculateCdsLocation(Factory));
		}
	}
}
