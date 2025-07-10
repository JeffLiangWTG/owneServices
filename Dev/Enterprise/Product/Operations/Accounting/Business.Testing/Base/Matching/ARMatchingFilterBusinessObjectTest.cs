using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ARMatchingFilterBusinessObject))]
	public class ARMatchingFilterBusinessObjectTest : MatchingFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARMatchingFilterBusinessObject();
		}

		public override void TestDisbursementRelatingToFilter()
		{
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_TransactionNum = "00001000";

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_TransactionNum = "00001001";

			var headerReference = Factory.New<AccTransactionHeaderReference>();
			headerReference.AH1_AH = invoice2.PK;
			headerReference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice;
			headerReference.AH1_Reference = "00001000";

			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var disbursementRelatingToTransactions = (ModuleTextFilter)FilterBizO[MatchingFilterBusinessObject.DisbursementRelatingTo];
				disbursementRelatingToTransactions.SqlComparisonOperator = SQLComparisonOperator.Contains;
				disbursementRelatingToTransactions.Property = "00001000";
				disbursementRelatingToTransactions.IsActive = true;

				var results = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				results.Load();

				AssertEquals("Collection should contain one invoice", 1, results.Count);
				Assert("Collection contains invoice2", results.Contains(invoice2.PK));
				Assert("Collection doesn't contains invoice1", !results.Contains(invoice1.PK));
			}
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
			decoySettlementGroup.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			decoySettlementGroup.PR_FreightDirection = RelatedPartyDirectionList.Codes.AR;
			decoySettlementGroup.PR_OH_RelatedParty = org1.PK;
			decoySettlementGroup.PR_GC = anotherCompany.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			AssertEquals(1, FilterBizO.SettlementOrgInfos.Count);
			Assert(FilterBizO.SettlementOrgInfos.ContainsOrgPK(org1.PK));
		}

		public void TestSettlementOrgGrid()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = false;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.ARSettlementGroupPK = org1.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			AssertEquals(2, FilterBizO.SettlementOrgInfos.Count);
			AssertEquals(org1.PK, FilterBizO.SettlementOrgInfos[0].Organization);
			Assert(FilterBizO.SettlementOrgInfos[0].OrganizationInfo.ReadOnly);
			Assert(!FilterBizO.SettlementOrgInfos[0].ARLedgerInfo.ReadOnly);
			Assert(FilterBizO.SettlementOrgInfos[0].ARLedger);
			Assert(!FilterBizO.SettlementOrgInfos[0].APLedger);

			AssertEquals(org2.PK, FilterBizO.SettlementOrgInfos[1].Organization);

			FilterBizO.IncludeAllAR = false;
			Assert(!FilterBizO.SettlementOrgInfos[0].ARLedger);
			Assert(!FilterBizO.SettlementOrgInfos[1].ARLedger);

			FilterBizO.IncludeAllAP = true;
			Assert(FilterBizO.SettlementOrgInfos[0].APLedger);
			Assert(FilterBizO.SettlementOrgInfos[1].APLedger);

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

			org1.ARSettlementGroupPK = org3.PK;
			org2.ARSettlementGroupPK = org3.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org3.PK;
			Assert(FilterBizO.SettlementOrgInfos.ContainsOrgPK(org1.PK));
			Assert(FilterBizO.SettlementOrgInfos.ContainsOrgPK(org2.PK));

			FilterBizO.PrimaryOrganization = ZGuid.Empty;
			AssertEquals(0, FilterBizO.SettlementOrgInfos.Count);
		}
	}
}
