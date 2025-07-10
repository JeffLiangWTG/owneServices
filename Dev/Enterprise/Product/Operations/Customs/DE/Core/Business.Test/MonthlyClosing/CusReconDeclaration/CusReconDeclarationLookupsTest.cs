using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusReconDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var statusCodeList = new ZString[] { UniversalReferenceConstants.EntryStatus.RC2, UniversalReferenceConstants.EntryStatus.REJ, UniversalReferenceConstants.EntryStatus.TX1,
				UniversalReferenceConstants.EntryStatus.TX2, UniversalReferenceConstants.EntryStatus.TX3, UniversalReferenceConstants.EntryStatus.TX4,
				UniversalReferenceConstants.EntryStatus.TX5, UniversalReferenceConstants.EntryStatus.TX6, UniversalReferenceConstants.EntryStatus.TRA,
				UniversalReferenceConstants.EntryStatus.ERR, UniversalReferenceConstants.EntryStatus.TXR };

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "RL1", "RL1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			foreach (var code in statusCodeList)
			{
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, code, code + " DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			Factory.Save();

			var list = declaration.Lookups.CustomsStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "ERR, RC2, REJ, TRA, TX1, TX2, TX3, TX4, TX5, TX6, TXR", list.CodesAsString);
				AssertSame("Cached", list, declaration.Lookups.CustomsStatusList);
			});
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var customsOfficeTestHelper = new CustomsOfficeCodeTestHelper(helper);

			customsOfficeTestHelper.CusofDE.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			customsOfficeTestHelper.CusofIT.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.TrueString);
			var cusof1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE111111", "DE111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusof2 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE222222", "DE222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, bool.FalseString);
			Factory.Save();

			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(cusof1.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(cusof2.PK);
			var zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofDE.PK);
			var zzCodeList4 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
			var completeFilter = declaration.Lookups.CustomsOfficeList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("zzCodeList1, no main office, valid country", false, zzCodeList1.MatchesFilter(completeFilter));
				AssertEquals("zzCodeList2, has main office invalid value, valid country", false, zzCodeList2.MatchesFilter(completeFilter));
				AssertEquals("zzCodeList3, has main office valid value, valid country", true, zzCodeList3.MatchesFilter(completeFilter));
				AssertEquals("zzCodeList4, has main office valid value, invalid country", false, zzCodeList4.MatchesFilter(completeFilter));
			});
		}

		public void TestDeclarationTypeList()
		{
			var list = declaration.Lookups.DeclarationTypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Codes", "AAV, AZ, AZL, VAV, VZA, VZL", list.CodesAsString);
				AssertSame("Cached", list, declaration.Lookups.DeclarationTypeList);
			});
		}

		public void TestAuthorizationList_VZA()
		{
			var permitOwner1 = Factory.NewWithValidTestData<OrgHeader>();
			var permitOwner2 = Factory.NewWithValidTestData<OrgHeader>();
			var permitOwner3 = Factory.NewWithValidTestData<OrgHeader>();

			var authorisationHeader1 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR12345678901");
			TestHelper.CreateAuthorisationRule(authorisationHeader1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);

			var authorisationHeader2 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678901");
			var authorisationHeader3 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678902");
			TestHelper.CreateAuthorisationRule(authorisationHeader3, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);

			var authorisationHeader4 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678903");
			TestHelper.CreateAuthorisationRule(authorisationHeader4, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			var authorisationHeader5 = TestHelper.CreateAuthorisationRecord(permitOwner2, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678904");
			TestHelper.CreateAuthorisationRule(authorisationHeader5, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			var authorisationHeader6 = TestHelper.CreateAuthorisationRecord(permitOwner3, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678905");
			TestHelper.CreateAuthorisationRule(authorisationHeader6, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);

			Factory.Save();

			declaration.CRD_OA_DeclarantAddress = permitOwner1.MainAddress.PK;
			declaration.CRD_OA_RepresentativeAddress = permitOwner2.MainAddress.PK;
			declaration.CRD_OA_BuyingAgentAddress = permitOwner3.MainAddress.PK;
			declaration.CRD_PeriodFrom = ZDate.Today;
			declaration.CRD_PeriodTo = ZDate.Today;

			CombineAssertions(() =>
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZA;
				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
				AssertEquals("Unmatched CPH_Type", false, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("Unmatched CPR_RuleCode", false, authorisationHeader2.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("Unmatched CPR_ValueFrom", false, authorisationHeader3.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, don't get from CRD_OA_RepresentativeAddress", false, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, don't get from CRD_OA_BuyingAgentAddress", false, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._2Direct;
				AssertEquals("CRD_DeclarantType is DIR, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, matched CRD_OA_RepresentativeAddress", true, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, don't get from CRD_OA_BuyingAgentAddress", false, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;
				AssertEquals("CRD_DeclarantType is IND, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, don't get from CRD_OA_RepresentativeAddress", false, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is IND, matched CRD_OA_BuyingAgentAddress", true, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
			});
		}

		public void TestAuthorizationList_AZ()
		{
			var permitOwner1 = Factory.NewWithValidTestData<OrgHeader>();
			var permitOwner2 = Factory.NewWithValidTestData<OrgHeader>();
			var permitOwner3 = Factory.NewWithValidTestData<OrgHeader>();

			var authorisationHeader1 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678901");
			TestHelper.CreateAuthorisationRule(authorisationHeader1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);

			var authorisationHeader2 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR12345678901");
			var authorisationHeader3 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DESDE12345678902");
			TestHelper.CreateAuthorisationRule(authorisationHeader3, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);

			var authorisationHeader4 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR12345678903");
			TestHelper.CreateAuthorisationRule(authorisationHeader4, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			var authorisationHeader5 = TestHelper.CreateAuthorisationRecord(permitOwner2, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR12345678904");
			TestHelper.CreateAuthorisationRule(authorisationHeader5, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			var authorisationHeader6 = TestHelper.CreateAuthorisationRecord(permitOwner3, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR2345678905");
			TestHelper.CreateAuthorisationRule(authorisationHeader6, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);

			Factory.Save();

			declaration.CRD_OA_DeclarantAddress = permitOwner1.MainAddress.PK;
			declaration.CRD_OA_RepresentativeAddress = permitOwner2.MainAddress.PK;
			declaration.CRD_OA_BuyingAgentAddress = permitOwner3.MainAddress.PK;
			declaration.CRD_PeriodFrom = ZDate.Today;
			declaration.CRD_PeriodTo = ZDate.Today;

			CombineAssertions(() =>
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZ;
				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
				AssertEquals("Unmatched CPH_Type", false, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("Unmatched CPR_RuleCode", false, authorisationHeader2.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("Unmatched CPR_ValueFrom", false, authorisationHeader3.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, don't get from CRD_OA_RepresentativeAddress", false, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is SEL, don't get from CRD_OA_BuyingAgentAddress", false, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._2Direct;
				AssertEquals("CRD_DeclarantType is DIR, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, matched CRD_OA_RepresentativeAddress", true, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, don't get from CRD_OA_BuyingAgentAddress", false, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;
				AssertEquals("CRD_DeclarantType is IND, matched CRD_OA_DeclarantAddress", true, authorisationHeader4.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is DIR, don't get from CRD_OA_RepresentativeAddress", false, authorisationHeader5.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("CRD_DeclarantType is IND, matched CRD_OA_BuyingAgentAddress", true, authorisationHeader6.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
			});
		}

		public void TestAuthorizationList_VAV_AAV()
		{
			var permitOwner1 = Factory.NewWithValidTestData<OrgHeader>();
			var authorisationHeader = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEIPO12345678901");
			var authorisationHeader2 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE12345678901");
			Factory.Save();

			declaration.CRD_OA_DeclarantAddress = permitOwner1.MainAddress.PK;
			declaration.CRD_PeriodFrom = ZDate.Today;
			declaration.CRD_PeriodTo = ZDate.Today;

			CombineAssertions(() =>
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VAV;
				AssertEquals("CRD_DeclarationType is VAV", true, authorisationHeader.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
				AssertEquals("Unmatched CPH_Type", false, authorisationHeader2.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AAV;
				AssertEquals("CRD_DeclarationType is AAV", true, authorisationHeader.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
				AssertEquals("Unmatched CRD_OA_DeclarantAddress", false, authorisationHeader.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_PeriodFrom = ZDate.Today.AddDays(2);
				AssertEquals("Unmatched CRD_PeriodFrom", false, authorisationHeader.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

				declaration.CRD_PeriodFrom = ZDate.Today;
				declaration.CRD_PeriodTo = ZDate.Today.AddDays(-2);
				AssertEquals("Unmatched CRD_PeriodTo", false, authorisationHeader.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
			});
		}

		public void TestAuthorizationList_VZL_AZL()
		{
			var permitOwner1 = Factory.NewWithValidTestData<OrgHeader>();
			var authorisationHeader1 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DECWP12345678901");
			var authorisationHeader2 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DECW112345678901");
			var authorisationHeader3 = TestHelper.CreateAuthorisationRecord(permitOwner1, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEIPO12345678901");

			Factory.Save();

			declaration.CRD_PeriodFrom = ZDate.Today;
			declaration.CRD_PeriodTo = ZDate.Today;

			TestAuthorizationListByDeclarationType(MonthlyClosingDeclarationTypeList.Codes.VZL);
			TestAuthorizationListByDeclarationType(MonthlyClosingDeclarationTypeList.Codes.AZL);

			void TestAuthorizationListByDeclarationType(ZString declarationType)
			{
				declaration.CRD_DeclarationType = declarationType;

				TestAuthorizationListByDeclarantType(RepresentationTypeList.Codes._1Self);
				TestAuthorizationListByDeclarantType(RepresentationTypeList.Codes._2Direct);
				TestAuthorizationListByDeclarantType(RepresentationTypeList.Codes._3Indirect);

				void TestAuthorizationListByDeclarantType(ZString declarantType)
				{
					declaration.CRD_DeclarantType = declarantType;
					declaration.CRD_OA_DeclarantAddress = permitOwner1.MainAddress.PK;
					declaration.CRD_OA_RepresentativeAddress = permitOwner1.MainAddress.PK;

					if (declarantType == RepresentationTypeList.Codes._1Self || declarantType == RepresentationTypeList.Codes._3Indirect)
					{
						CombineAssertions(() =>
						{
							AssertEquals($"{declarationType} {declarantType} CRD_DeclarationType is {declarationType} (CWP)", true, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
							AssertEquals($"{declarationType} {declarantType} CRD_DeclarationType is {declarationType} (CW1)", true, authorisationHeader2.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
							AssertEquals($"{declarationType} {declarantType} Unmatched CPH_Type", false, authorisationHeader3.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

							AssertEquals($"{declarationType} {declarantType} Matched CRD_OA_DeclarantAddress", true, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

							declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
							AssertEquals($"{declarationType} {declarantType} Unatched CRD_OA_DeclarantAddress", false, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

							declaration.CRD_OA_RepresentativeAddress = ZGuid.Empty;
							AssertEquals($"{declarationType} {declarantType} Unmatched CRD_OA_RepresentativeAddress", false, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
						});
					}
					else if (declarantType == RepresentationTypeList.Codes._2Direct)
					{
						CombineAssertions(() =>
						{
							AssertEquals($"{declarationType} {declarantType} CRD_DeclarationType is {declarationType} (CWP)", true, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
							AssertEquals($"{declarationType} {declarantType} CRD_DeclarationType is {declarationType} (CW1)", true, authorisationHeader2.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
							AssertEquals($"{declarationType} {declarantType} Unmatched CPH_Type", false, authorisationHeader3.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

							AssertEquals($"{declarationType} {declarantType} Matched CRD_OA_DeclarantAddress", true, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
							AssertEquals($"{declarationType} {declarantType} Matched CRD_OA_RepresentativeAddress", true, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));

							declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
							declaration.CRD_OA_RepresentativeAddress = ZGuid.Empty;
							AssertEquals($"{declarationType} {declarantType} Unmatched Addresses", false, authorisationHeader1.MatchesFilter(declaration.Lookups.AuthorizationList.CompleteFilter));
						});
					}
				}
			}
		}

		public void TestDeclarantTypeList()
		{
			var list = declaration.Lookups.DeclarantTypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Codes", "SEL, DIR, IND", list.CodesAsString);
				AssertSame("Cached", list, declaration.Lookups.DeclarantTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
		}
		CusReconDeclaration declaration;
	}
}
