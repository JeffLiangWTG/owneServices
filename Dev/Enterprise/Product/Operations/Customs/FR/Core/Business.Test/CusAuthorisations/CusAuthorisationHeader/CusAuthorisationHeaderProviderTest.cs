using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	public class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		public void TestOverrideEnableAdHoc()
		{
			Assert("EnableAdHoc should be true", AuthorisationHeaderProvider.EnableAdHoc);
		}

		public void TestAuthorizationTypesNeedAddress()
		{
			AssertCollectionContains(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation, AuthorisationHeaderProvider.AuthorizationTypesNeedAddress);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("DIE", "France IE", parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList("DIE", "AUTH", "C600", "IsImport", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			base.SetUp();
		}

		public void TestDeltaG_DeltaIEAuthorisationTypeList()
		{
			var authorisationTypeList = authorisationHeader.Lookups.AuthorisationTypeList;
			var authorisationTypeCodeDescriptionPairList = (ReadOnlyCodeDescriptionPairList)authorisationTypeList;
			AssertEquals("DeltaIE Code C600 is retrieved", expected: true, authorisationTypeCodeDescriptionPairList.ContainsCode("C600"));
			AssertEquals("DeltaG Code AUL is retrieved", expected: true, authorisationTypeCodeDescriptionPairList.ContainsCode("AUL"));
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForOPO_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW1_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW2_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCWP_LOC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			AssertRuleRequirement(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, validAuthorisationRuleRequirement.Single(x => x.RuleType == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_OFC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.OFC, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.OFC), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_STO()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.STO, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_CON()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.CON, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.CON), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_PCD()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCD, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCD), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_PCP()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCP, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCP), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_PCV()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCV, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCD), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForEUS_NAT()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.NAT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.NAT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW1_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW2_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCWP_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_PCD()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCD, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCD), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_PCP()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCP, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCP), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_PCV()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCV, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCV), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForOPO_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_AUT()
		{
			authorisationHeader.CPH_IsAdHoc = false;
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 1, 1, false);

			authorisationHeader.CPH_IsAdHoc = true;
			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.AUT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.AUT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_PCD()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCD, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCD), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_PCP()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCP, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCP), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_PCV()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.PCV, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.PCV), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW1_USE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.USE, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.USE), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCWP_USE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.USE, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.USE), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCW2_USE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.USE, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.USE), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTST_USE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.USE, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.USE), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForTEA_USE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.USE, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.USE), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForCNT()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, ZString.Empty);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.CNT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.CNT), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForSTO()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.STO, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForOFC()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.OFC, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.OFC), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForWAR()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, ZString.Empty);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.WAR, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.WAR), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForSUB()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, ZString.Empty);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.SUB, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.SUB), 0, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_TEA_OPO_STO()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.STO, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.STO, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.STO, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_TEA_OPO_NAT()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.NAT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.NAT), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.NAT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.NAT), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.NAT, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.NAT), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_TEA_OPO_CON()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.CON, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.CON), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.CON, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.CON), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.CON, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.CON), 1, 1, false);
		}

		public void TestGetNewAuthorisationRuleValidRepetitionForIPO_TEA_OPO_TRA()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);
		}

		public void TestHasAmessageErrorIfAllTheRuleAreNotPresentForIPO_TEA_OPO_TRA()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);

			validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.TRA, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.STO), 1, 1, false);
		}

		public void TestCusAuthorisationRuleTypeListForTransit_IsUsingPhase4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase5Override, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				AssertContainsExactElementsInAnyOrder(new ZString[] { CusAuthorisationRuleTypeList.Codes.Location, CusAuthorisationRuleTypeList.Codes.OFC }, AuthorisationHeaderProvider.CusAuthorisationRuleTypeListForTransit.GetAllCodes());
			}
		}

		public void TestCusAuthorisationRuleTypeListForTransit_IsNotUsingPhase4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase5Override, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertContainsExactElementsInAnyOrder(new ZString[] { CusAuthorisationRuleTypeList.Codes.Location }, AuthorisationHeaderProvider.CusAuthorisationRuleTypeListForTransit.GetAllCodes());
			}
		}

		public void TestCustomsNumberProvider()
		{
			authorisationHeader.CPH_Type = ZString.Empty;
			AssertNull("Empty CPH_Type has no CustomsNumberProvider", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertNull("Only TST type has CustomsNumberProvider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var provider = authorisationHeader.CustomsNumberProvider;
			AssertNotNull(provider);
			AssertEquals("Only TST type has CustomsNumberProvider.", "Enterprise.Customs.FR.GUI.TSTCustomsNumberViewStmNumsAuthorisationProvider", provider.GetType().FullName);

			authorisationHeader.CPH_Type = "TSX";
			AssertNull("TSX starts with TS, but we should not load a TS provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "T1X";
			AssertNull("T1X starts with T1, but we should not load a TS provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "USX";
			AssertNull("USX starts with US, but we should not load an US provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "US";
			AssertNull("US is not a valid provider type for authorisation, it's only for company, we should not load it.", authorisationHeader.CustomsNumberProvider);
		}

		public void TestCPH_NUMBER()
		{
			authorisationHeader.CPH_Type = ZString.Empty;
			AssertNull("Empty CPH_Type has no CustomsNumberProvider", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertNull("Only TST type has CustomsNumberProvider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var provider = authorisationHeader.CustomsNumberProvider;
			AssertNotNull(provider);
			AssertEquals("Only TST type has CustomsNumberProvider.", "Enterprise.Customs.FR.GUI.TSTCustomsNumberViewStmNumsAuthorisationProvider", provider.GetType().FullName);

			authorisationHeader.CPH_Type = "TSX";
			AssertNull("TSX starts with TS, but we should not load a TS provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "T1X";
			AssertNull("T1X starts with T1, but we should not load a TS provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "USX";
			AssertNull("USX starts with US, but we should not load an US provider.", authorisationHeader.CustomsNumberProvider);

			authorisationHeader.CPH_Type = "US";
			AssertNull("US is not a valid provider type for authorisation, it's only for company, we should not load it.", authorisationHeader.CustomsNumberProvider);
		}

		public void TestGetRuleValueFieldTypes()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();

			CombineAssertions(() =>
			{
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
				AssertEquals("RuleCode 'OFC'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
				AssertEquals("RuleCode 'USE'", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CNT;
				AssertEquals("RuleCode 'CNT'", nameof(FieldType.Integer), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CLE;
				AssertEquals("RuleCode 'CLE'", nameof(FieldType.Boolean), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.STO;
				AssertEquals("RuleCode 'STO'", nameof(FieldType.Integer), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.WAR;
				AssertEquals("RuleCode 'WAR'", nameof(FieldType.Integer), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.PCD;
				AssertEquals("RuleCode 'PCD'", nameof(FieldType.Decimal), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.PCP;
				AssertEquals("RuleCode 'PCP'", nameof(FieldType.Decimal), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.PCV;
				AssertEquals("RuleCode 'PCV'", nameof(FieldType.Decimal), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.NAT;
				AssertEquals("RuleCode 'NAT'", nameof(FieldType.TextMultiLine), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
				AssertEquals("RuleCode 'CON'", nameof(FieldType.TextMultiLine), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.TRA;
				AssertEquals("RuleCode 'TRA'", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.INF;
				AssertEquals("RuleCode 'INF'", nameof(FieldType.TextMultiLine), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
				AssertEquals("RuleCode 'AUT'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
			});
		}

		public void TestGetRuleValueFieldTypesFor_Loc()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with OPO", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.INF;
			AssertEquals("RuleCode 'INF' with OPO", nameof(FieldType.TextMultiLine), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with TEA", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with IPO", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OtherThanOpo;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with OTO", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with TEE", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with ACE", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with ACR", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with ACT", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode 'LOC' with SSE", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		}

		public void TestGetRuleValueFieldTypesFor_CON()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			AssertEquals("RuleCode 'CON' with OPO", nameof(FieldType.TextMultiLine), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			AssertEquals("RuleCode 'CON' with IPO", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			AssertEquals("RuleCode 'CON' with EUS", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		}

		public void TestGetRuleValueFromMaxLength()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			AssertEquals("RuleCode 'CON'", AutoCusPermitRule.Schema.CPR_ValueFromMaxLength, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule));
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.INF;
			AssertEquals("RuleCode 'INF'", AutoCusPermitRule.Schema.CPR_ValueFromMaxLength, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule));
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.NAT;
			AssertEquals("RuleCode 'NAT'", AutoCusPermitRule.Schema.CPR_ValueFromMaxLength, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule));
		}

		public void TestErrorTypeWhenRuleRequirementNotMet()
		{
			authorisationHeader.CPH_Type = "IPO";
			authorisationHeader.CPH_Number = ZString.Empty;
			AssertHasErrorContaining(authorisationHeader.CPH_NumberInfo, "You are required to have at least");
			AssertHasMessageErrorContaining(authorisationHeader.CPH_NumberInfo, "You are required to have at least");
		}

		public void TestGetRuleDescription_WhenRuleCodeIsCON()
		{
			authorisationRule.CPR_RuleCode = "CON";
			authorisationRule.CPR_ValueFrom = "RLA";
			AssertEquals("Correct Description", "Articles 169 à 173", AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
		}

		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.France;

		protected override Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);

		protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);

		protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);

		protected override Type ExpectedHeaderValidationType => typeof(CusAuthorisationHeaderValidation);

		protected override CodeDescriptionPairList ExpectedRuleCodeListForModule
		{
			get
			{
				var expectedList = new Customs.Business.CusAuthorisationRuleTypeList();
				expectedList.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
				expectedList.Sort();
				return expectedList;
			}
		}

		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
		{
			get
			{
				var authorizationTypes = new CusAuthorizationHeaderTypeList();
				var dieCodeList = RefCusCodeListTypes.GetCachedList(
							Factory,
							Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE,
							EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH,
							ZDateTime.Today
							);
				authorizationTypes.AddRangeOverwriteIfExists(dieCodeList);
				authorizationTypes.Sort();
				return authorizationTypes;
			}
		}

		public void TestDefaultCPHNumber()
		{
			AssertEquals("DefaultTemporaryAuthorizationNumber should be S/DECLARATION.", "S/DECLARATION", AuthorisationHeaderProvider.DefaultTemporaryAuthorizationNumber);
		}
	}
}
