using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusEntryInstructionLookupsBaseOnlyTest : TestCaseWithFactory
	{
		public void TestProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC2", "22", "222", "CPC2 Desc", "IMP, EXP", group: "H2,H7");
			helper.CreateRefCusProcedure(currentCountry, "B", "CPC3", "33", "333", "CPC3 Desc", "EXP", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC2", "CPC2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC3", "CPC3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				var lookups = instruction.Lookups;
				var procedureCodeList = lookups.ProcedureCodeList;

				AssertEquals("Only IMP and H1 is included", "CPC1", procedureCodeList.CodesAsString);
				AssertSame("Cached", procedureCodeList, lookups.ProcedureCodeList);
			}
		}
	}
}
