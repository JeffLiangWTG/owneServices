using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusOtherLawReferenceLookups))]
	sealed class CusOtherLawReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "TestDescritpion", Core.Constants.CountryCodes.Japan);
			var co1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "CO1", "Description1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(co1.PK, "IsImport", "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(co1.PK, "IsExport", "N");
			var co2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "CO2", "Description2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(co2.PK, "IsImport", "N");
			helper.CreateNewOrGetExistingCusCodeListAttribute(co2.PK, "IsExport", "Y");
			var co3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "CO3", "Description3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(co3.PK, "IsImport", "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(co3.PK, "IsExport", "N");
			var co4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "CO4", "Description4", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(co4.PK, "IsImport", "N");
			helper.CreateNewOrGetExistingCusCodeListAttribute(co4.PK, "IsExport", "Y");
			var ei = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "EI", "Description5", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(ei.PK, "IsImport", "N");
			helper.CreateNewOrGetExistingCusCodeListAttribute(ei.PK, "IsExport", "Y");
			Factory.Save();

			var instruction = Factory.New<CusEntryInstructionForTest>();
			instruction.CEI_Style = "A";
			var guarantees = new CusOtherLawReferenceCollection<CusOtherLawReference>(instruction);
			var cusOtherLawReference = guarantees.AddNew();
			instruction.CEI_SubStyle = "IMP";
			AssertContainsExactElementsInExactOrder(new[] { "CO3", "CO1" }, cusOtherLawReference.Lookups.ReferenceList.GetAllCodes());

			instruction.CEI_SubStyle = "EXP";
			AssertContainsExactElementsInExactOrder(new[] { "CO4", "EI", "CO2" }, cusOtherLawReference.Lookups.ReferenceList.GetAllCodes());

			instruction.CEI_SubStyle = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { "CO3", "CO4", "CO1", "CO2", "EI" }, cusOtherLawReference.Lookups.ReferenceList.GetAllCodes());
		}
	}
}
