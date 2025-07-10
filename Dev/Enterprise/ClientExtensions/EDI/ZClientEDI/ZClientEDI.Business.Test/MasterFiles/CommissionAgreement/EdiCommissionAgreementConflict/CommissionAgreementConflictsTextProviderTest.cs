using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementConflictsTextProviderTest : TestCaseWithFactory
	{
		#region New

		public void TestNew()
		{
			AssertType(typeof(EdiCommissionAgreementConflictsTextProvider), CommissionAgreementConflictsTextProvider.New());
		}

		#endregion

		#region ToDisplayList

		public void TestToDisplayList_SpecificCompaniesAndDatabases()
		{
			var databaseAAA = Factory.New<LicenceDatabase>();
			databaseAAA.LD_ServerCode = "AAA";
			var databaseBBB = Factory.New<LicenceDatabase>();
			databaseBBB.LD_ServerCode = "BBB";

			var company111 = Factory.New<ClientCompany>();
			company111.LCC_Code = "111";
			company111.LCC_Name = "WiseTech Global";
			var company222 = Factory.New<ClientCompany>();
			company222.LCC_Code = "222";
			company222.LCC_Name = "CargoWise";

			var winnerAgreement = Factory.New<EdiCommissionAgreement>();
			var customization = winnerAgreement.GetOrCreateCustomization();
			var entOdmAllWinnerAgreementItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(winnerAgreement, "ENT", "ODM", "ALL");
			var entStlAllWinnerAgreementItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(winnerAgreement, "ENT", "STL", "ALL");

			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00029769";
			var loserAgreement = opportunity.CommissionAgreements.AddNew();
			loserAgreement.CA0_Name = "#2";

			var conflicts = new ICommissionAgreementConflict[]
			{
				new CommissionAgreementItemAndCompanyConflict(entOdmAllWinnerAgreementItem, company111, loserAgreement),
				new CommissionAgreementItemAndCompanyConflict(entStlAllWinnerAgreementItem, company111, loserAgreement),

				new CommissionAgreementItemAndCompanyConflict(entStlAllWinnerAgreementItem, company222, loserAgreement),

				new CommissionAgreementItemAndCompanyAutoAddDatabaseConflict(entStlAllWinnerAgreementItem, null, loserAgreement),
				new CommissionAgreementItemAndCompanyAutoAddDatabaseConflict(entOdmAllWinnerAgreementItem, databaseBBB, loserAgreement),

				new CommissionAgreementItemAndCompanyAutoAddCountryConflict(entStlAllWinnerAgreementItem, null, "AU", loserAgreement),
				new CommissionAgreementItemAndCompanyAutoAddCountryConflict(entOdmAllWinnerAgreementItem, databaseAAA, "AU", loserAgreement),

				new CommissionAgreementItemAndDatabaseConflict(entOdmAllWinnerAgreementItem, databaseAAA, loserAgreement),
				new CommissionAgreementItemAndDatabaseConflict(entStlAllWinnerAgreementItem, databaseBBB, loserAgreement),
			};

			var textProvider = CommissionAgreementConflictsTextProvider.New();

			AssertMultilineASCIIEquals("ToDisplayList",
@"[CargoWise (222) Company Usages]
   ENT > STL > ALL
[WiseTech Global (111) Company Usages]
   ENT > ODM > ALL
   ENT > STL > ALL
[Usages of all newly created companies]
   ENT > STL > ALL
[Usages of newly created companies for BBB Database]
   ENT > ODM > ALL
[Usages of newly created AU companies]
   ENT > STL > ALL
[Usages of newly created AU companies for AAA Database]
   ENT > ODM > ALL
[AAA Database Usages]
   ENT > ODM > ALL
[BBB Database Usages]
   ENT > STL > ALL",
			textProvider.ToDisplayList(conflicts, 0, false));

			AssertMultilineASCIIEquals("ToDisplayList",
"<li>[CargoWise (222) Company Usages]<ul><li>ENT &gt; STL &gt; ALL</li></ul></li>"
+ "<li>[WiseTech Global (111) Company Usages]<ul><li>ENT &gt; ODM &gt; ALL</li><li>ENT &gt; STL &gt; ALL</li></ul></li>"
+ "<li>[Usages of all newly created companies]<ul><li>ENT &gt; STL &gt; ALL</li></ul></li>"
+ "<li>[Usages of newly created companies for BBB Database]<ul><li>ENT &gt; ODM &gt; ALL</li></ul></li>"
+ "<li>[Usages of newly created AU companies]<ul><li>ENT &gt; STL &gt; ALL</li></ul></li>"
+ "<li>[Usages of newly created AU companies for AAA Database]<ul><li>ENT &gt; ODM &gt; ALL</li></ul></li>"
+ "<li>[AAA Database Usages]<ul><li>ENT &gt; ODM &gt; ALL</li></ul></li>"
+ "<li>[BBB Database Usages]<ul><li>ENT &gt; STL &gt; ALL</li></ul></li>",
			textProvider.ToDisplayList(conflicts, 0, true));
		}

		public void TestToDisplayList_AllCompaniesAndDatabases()
		{
			var winnerAgreement = Factory.New<EdiCommissionAgreement>();
			var customization = winnerAgreement.GetOrCreateCustomization();
			var entOdmAllWinnerAgreementItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(winnerAgreement, "ENT", "ODM", "ALL");
			var entStlAllWinnerAgreementItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(winnerAgreement, "ENT", "STL", "ALL");

			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00029769";
			var loserAgreement = opportunity.CommissionAgreements.AddNew();
			loserAgreement.CA0_Name = "#2";

			var conflicts = new ICommissionAgreementConflict[]
			{
				new CommissionAgreementItemAndCompanyConflict(entOdmAllWinnerAgreementItem, null, loserAgreement),
				new CommissionAgreementItemAndCompanyConflict(entStlAllWinnerAgreementItem, null, loserAgreement),

				new CommissionAgreementItemAndDatabaseConflict(entOdmAllWinnerAgreementItem, null, loserAgreement),
				new CommissionAgreementItemAndDatabaseConflict(entStlAllWinnerAgreementItem, null, loserAgreement),
			};

			var textProvider = CommissionAgreementConflictsTextProvider.New();

			AssertMultilineASCIIEquals("ToDisplayList",
@"[All Company Usages]
   ENT > ODM > ALL
   ENT > STL > ALL
[All Database Usages]
   ENT > ODM > ALL
   ENT > STL > ALL",
			textProvider.ToDisplayList(conflicts, 0, false));

			AssertMultilineASCIIEquals("ToDisplayList",
"<li>[All Company Usages]<ul><li>ENT &gt; ODM &gt; ALL</li><li>ENT &gt; STL &gt; ALL</li></ul></li>"
+ "<li>[All Database Usages]<ul><li>ENT &gt; ODM &gt; ALL</li><li>ENT &gt; STL &gt; ALL</li></ul></li>",
			textProvider.ToDisplayList(conflicts, 0, true));
		}

		#endregion
	}
}