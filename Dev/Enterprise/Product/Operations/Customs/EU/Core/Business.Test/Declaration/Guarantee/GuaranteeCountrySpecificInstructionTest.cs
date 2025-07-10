using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetRuleCodeList()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var listRule = guarantee.CountrySpecificInstruction.GetRuleCodeList("", "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.INV }, listRule.GetAllCodes());

			listRule = guarantee.CountrySpecificInstruction.GetRuleCodeList(EUGuaranteeTypeList.Codes.TRA, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.PCP, PermitRuleCodeList.Codes.PCD, PermitRuleCodeList.Codes.PCV }, listRule.GetAllCodes());

			listRule = guarantee.CountrySpecificInstruction.GetRuleCodeList(EUGuaranteeTypeList.Codes.COD, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.INV, PermitRuleCodeList.Codes.PCP, PermitRuleCodeList.Codes.PCD, PermitRuleCodeList.Codes.PCV }, listRule.GetAllCodes());

			listRule = guarantee.CountrySpecificInstruction.GetRuleCodeList(EUGuaranteeTypeList.Codes.IMP, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.INV }, listRule.GetAllCodes());

			listRule = guarantee.CountrySpecificInstruction.GetRuleCodeList(EUGuaranteeTypeList.Codes.TST, "");
			AssertContainsExactElementsInAnyOrder(new string[] { PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Codes.ADD, PermitRuleCodeList.Codes.LAP, PermitRuleCodeList.Codes.CUS, PermitRuleCodeList.Codes.INV }, listRule.GetAllCodes());
		}

		public void TestGetRuleCodeListForModule()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var listrule = guarantee.CountrySpecificInstruction.GetRuleCodeListForModule().GetAllCodes();
			AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.ADD));
			AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.INV));
			AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.LAP));
			AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.CUS));
		}

		public void TestGetValueFromFieldType()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertEquals(nameof(FieldType.TextCodeFindBox), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.ADD));
			AssertEquals(nameof(FieldType.TextCodeFindBox), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.CUS));
			AssertEquals(nameof(FieldType.TextCodeFindBox), guarantee.CountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.TSP));
			AssertEquals(nameof(FieldType.Text), guarantee.CountrySpecificInstruction.GetValueFromFieldType("XXX"));
		}

		public void TestGetMatchingType()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertEquals(PermitMatchingType.SingleValue, guarantee.CountrySpecificInstruction.GetMatchingType(PermitRuleCodeList.Codes.ADD));
			AssertEquals(PermitMatchingType.Range, guarantee.CountrySpecificInstruction.GetMatchingType("XXX"));
		}

		[StressTest]
		public void TestGetLookupList_WhenPermitHeaderIsNull()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();

			var instruction = Customs.Business.GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var addresses = (BusinessObjectCollection)instruction.GetLookupList(null, PermitRuleCodeList.Codes.ADD);
			addresses.Load();
			AssertCollectionContains("It should return all addresses if the permit header is null.", org.MainAddress, addresses);
			AssertCollectionContains("It should return all addresses if the permit header is null.", address1, addresses);
		}

		[StressTest]
		public void TestGetLookupList_WhenPermitHolderIsNull()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			AssertNull(guarantee.PermitHolder);
			var instruction = guarantee.CountrySpecificInstruction;
			var addresses = (BusinessObjectCollection)instruction.GetLookupList(guarantee, PermitRuleCodeList.Codes.ADD);
			addresses.Load();
			AssertEquals("It should return none address if the permit holder is null.", 0, addresses.Count);
		}

		public void TestGetLookupList_WhenPermitHolderIsValid()
		{
			var org = Factory.New<OrgHeader>();
			var orgAddress1 = org.Addresses.AddNew();

			var holder = Factory.New<OrgHeader>();
			var holderAddress = holder.Addresses.AddNew();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_OH_PermitHolder = holder.PK;
			var instruction = guarantee.CountrySpecificInstruction;
			var addresses = (BusinessObjectCollection)instruction.GetLookupList(guarantee, PermitRuleCodeList.Codes.ADD);
			addresses.Load();
			AssertContainsExactElementsInAnyOrder("It should return those addresses on the permit holder only.", new[] { holder.MainAddress, holderAddress }, addresses);
		}

		public void TestGetLookupList_Premises()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var instruction = guarantee.CountrySpecificInstruction;
			var permises = instruction.GetLookupList(guarantee, PermitRuleCodeList.Codes.TSP);
			AssertEquals(ObjectFactory.GetType<ICusTempStorageRegPremisesCollection>(), permises.GetType());
		}
	}
}
