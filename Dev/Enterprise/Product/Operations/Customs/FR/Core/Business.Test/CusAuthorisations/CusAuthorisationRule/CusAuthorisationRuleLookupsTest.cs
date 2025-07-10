using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	public class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		public void TestRuleCodeListType()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			AssertType<Customs.Business.CusAuthorisationRuleTypeList>(lookups.RuleCodeList);
		}

		public void TestRuleCodeListForCW1()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForCW2()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForCWP()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "CNT", "LOC", "PCD", "PCP", "PCV", "STO", "USE", "WAR" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForTST()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var list = cusAuthorisationRuleLookups.RuleCodeList;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "STO", "USE" }, list.GetAllCodes());
		}

		public void TestRuleCodeListForAUL()
		{
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
			var list = cusAuthorisationRuleLookups.RuleCodeList;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "OFC", "SUB" }, list.GetAllCodes());
		}

		public void TestRuleCodeListForIPO()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "PCD", "PCP", "PCV", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "PCD", "PCP", "PCV", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForTEA()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "PCD", "PCP", "PCV", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "PCD", "PCP", "PCV", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForOPO()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "AUT", "CLE", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "CLE", "STO", "WAR", "TRA", "LOC", "NAT", "OFC", "INF", "CON" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForOTO()
		{
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OtherThanOpo;
			var list = cusAuthorisationRuleLookups.RuleCodeList;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "STO", "OFC", "CON", "NAT", "CLE", "TRA", "INF" }, list.GetAllCodes());
		}

		public void TestRuleCodeListForTEE()
		{
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			var list = cusAuthorisationRuleLookups.RuleCodeList;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "STO", "OFC", "CON", "NAT", "CLE", "TRA", "INF" }, list.GetAllCodes());
		}

		public void TestRuleCodeListForEUS()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
			cusAuthorisationHeader.CPH_IsAdHoc = false;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "AUT" , "OFC", "STO", "CON", "PCP", "PCV", "PCD" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());

			cusAuthorisationHeader.CPH_IsAdHoc = true;
			AssertContainsExactElementsInAnyOrder(new[] { "LOC", "OFC", "STO", "CON", "PCP", "PCV", "PCD" }, cusAuthorisationRuleLookups.RuleCodeList.GetAllCodes());
		}

		public void TestGetValueListAuthorizationCON_EUS()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;

			AssertContainsExactElementsInAnyOrder("For End Use with rule code CON, the codes should exactly match ['C990', 'N990']", new[] { "C990", "N990" }, list.GetAllCodes());
			AssertEquals("For code 'C990', the description should be 'End Use – Ships and Offshore Platforms'", "End Use – Ships and Offshore Platforms", list.GetDescriptionFromCode("C990"));
			AssertEquals("For code 'N990', the description should be 'End Use – General'", "End Use – General", list.GetDescriptionFromCode("N990"));
		}

		public void TestGetValueListAuthorizationCON_IPO()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;

			AssertContainsExactElementsInAnyOrder("For Inward Processing with rule code CON, the codes should exactly match ['RLA', 'CNA', 'PRT', 'RMA', 'SUC', '324']", new[] { "RLA", "CNA", "PRT", "RMA", "SUC", "324" }, list.GetAllCodes());
			AssertEquals("For code 'RLA', the description should be 'Articles 169 à 173'", "Articles 169 à 173", list.GetDescriptionFromCode("RLA"));
			AssertEquals("For code 'CNA', the description should be 'Mise aux normes'", "Mise aux normes", list.GetDescriptionFromCode("CNA"));
			AssertEquals("For code 'PRT', the description should be 'Transformation'", "Transformation", list.GetDescriptionFromCode("PRT"));
			AssertEquals("For code 'RMA', the description should be 'Réparation'", "Réparation", list.GetDescriptionFromCode("RMA"));
			AssertEquals("For code 'SUC', the description should be 'Gestion par Numéro d’affaire'", "Gestion par Numéro d’affaire", list.GetDescriptionFromCode("SUC"));
			AssertEquals("For code '324', the description should be 'PA 324C'", "PA 324C", list.GetDescriptionFromCode("324"));
		}

		public void TestGetValueListAuthorizationUSE_CW1()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;
			AssertContainsExactElementsInAnyOrder(new[] { "ENE" }, list.GetAllCodes());
		}

		public void TestGetValueListAuthorizationUSE_CW2()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;
			AssertContainsExactElementsInAnyOrder(new[] { "ENE" }, list.GetAllCodes());
		}

		public void TestGetValueListAuthorizationUSE_CWP()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;
			AssertContainsExactElementsInAnyOrder(new[] { "PSA", "PAA", "U" }, list.GetAllCodes());
		}

		public void TestGetValueListAuthorizationUSE_TST()
		{
			cusAuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var list = (CodeDescriptionPairList)new CusAuthorisationRuleLookups(cusAuthorisationRule).ValueList;
			AssertContainsExactElementsInAnyOrder(new[] { "LAD", "IST" }, list.GetAllCodes());
		}

		public void TestRuleCodeListForTransit_IsNotUsingPhase4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase5Override, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				var list = cusAuthorisationRuleLookups.RuleCodeList;
				AssertContainsExactElementsInAnyOrder(new[] { "LOC" }, list.GetAllCodes());

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				list = cusAuthorisationRuleLookups.RuleCodeList;
				AssertContainsExactElementsInAnyOrder(new[] { "LOC" }, list.GetAllCodes());

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				list = cusAuthorisationRuleLookups.RuleCodeList;
				AssertContainsExactElementsInAnyOrder(new[] { "LOC" }, list.GetAllCodes());
			}
		}

		public void TestRuleCodeListForTransit_IsUsingPhase4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase5Override, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				var list = cusAuthorisationRuleLookups.RuleCodeList;
				AssertContainsExactElementsInAnyOrder(new[] { "LOC", "OFC" }, list.GetAllCodes());

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				list = cusAuthorisationRuleLookups.RuleCodeList;
				AssertContainsExactElementsInAnyOrder(new[] { "LOC", "OFC" }, list.GetAllCodes());
			}
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			cusAuthorisationHeader = cusAuthorisationRule.AuthorisationHeader;
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationRuleLookups = new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}
		CusAuthorisationRule cusAuthorisationRule;
		CusAuthorisationHeader cusAuthorisationHeader;
		CusAuthorisationRuleLookups cusAuthorisationRuleLookups;
	}
}
