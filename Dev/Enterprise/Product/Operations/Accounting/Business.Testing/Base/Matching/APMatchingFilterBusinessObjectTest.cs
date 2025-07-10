using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(APMatchingFilterBusinessObject))]
	public class APMatchingFilterBusinessObjectTest : MatchingFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APMatchingFilterBusinessObject();
		}

		public override void TestDisbursementRelatingToFilter()
		{
			var disbursementRelatingToTransactions = (ModuleTextFilter)FilterBizO[MatchingFilterBusinessObject.DisbursementRelatingTo];
			AssertNull("AR Matching has no DisbursementRelatingTo filter.", disbursementRelatingToTransactions);
		}

		public void TestSettlementOrgGridIsCompanySpecific()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = false;

			var companyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(companyFilter);

			var decoySettlementGroup = Factory.New<OrgRelatedParty>();
			decoySettlementGroup.PR_OH_Parent = org2.PK;
			decoySettlementGroup.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			decoySettlementGroup.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			decoySettlementGroup.PR_OH_RelatedParty = org1.PK;
			decoySettlementGroup.PR_GC = anotherCompany.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			AssertEquals(1, FilterBizO.SettlementOrgInfos.Count);
		}

		public void TestSettlementOrgGrid()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCreditor = true;
			org1.OH_IsDebtor = false;

			org2.APSettlementGroupPK = org1.PK;
			org2.OH_IsDebtor = true;

			Factory.Save();
			FilterBizO.PrimaryOrganization = org1.PK;

			AssertEquals(2, FilterBizO.SettlementOrgInfos.Count);
			AssertEquals(org1.PK, FilterBizO.SettlementOrgInfos[0].Organization);
			Assert(FilterBizO.SettlementOrgInfos[0].OrganizationInfo.ReadOnly);
			Assert(FilterBizO.SettlementOrgInfos[0].APLedger);
			Assert(!FilterBizO.SettlementOrgInfos[0].ARLedger);

			AssertEquals(org2.PK, FilterBizO.SettlementOrgInfos[1].Organization);
			Assert(!FilterBizO.SettlementOrgInfos[1].APLedger);
			Assert(FilterBizO.SettlementOrgInfos[1].ARLedger);

			FilterBizO.IncludeAllAR = true;

			Assert(FilterBizO.SettlementOrgInfos[0].ARLedger);
			Assert(FilterBizO.SettlementOrgInfos[1].ARLedger);

			FilterBizO.IncludeAllAP = false;
			Assert(!FilterBizO.SettlementOrgInfos[0].APLedger);
			Assert(!FilterBizO.SettlementOrgInfos[1].APLedger);
		}

		public void TestSettlementOrganizationsArePopulated()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			org1.APSettlementGroupPK = org3.PK;
			org2.APSettlementGroupPK = org3.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org3.PK;

			Assert(FilterBizO.SettlementOrgInfos.ContainsOrgPK(org1.PK));
			Assert(FilterBizO.SettlementOrgInfos.ContainsOrgPK(org2.PK));

			FilterBizO.PrimaryOrganization = ZGuid.Empty;
			AssertEquals(0, FilterBizO.SettlementOrgInfos.Count);
		}

		public void TestSetFilterForDBReloadIsNoResultQuery()
		{
			var filterBizO = new APMatchingFilterBusinessObject();
			filterBizO.SetFilterForDBReloadIsNoResultQuery();
			Assert(filterBizO.FilterForDBReload.IsNoResultQuery);
		}
	}
}
