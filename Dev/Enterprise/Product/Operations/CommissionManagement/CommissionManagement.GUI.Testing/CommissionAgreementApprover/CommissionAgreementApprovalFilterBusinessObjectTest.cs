using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionAgreementApprovalFilterBusinessObject))]
	public class CommissionAgreementApprovalFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestOpportunityIdFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();

			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var opportunityIdFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.OpportunityId];

			AssertNotNull(opportunityIdFilter);
			AssertEquals("Opportunity Id", opportunityIdFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.OpportunitiesFilterCategory, opportunityIdFilter.Category);

			opportunityIdFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityIdFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			opportunityIdFilter.Property = opportunity1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityIdFilter.Property = opportunity2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityIdFilter.Property = opportunity3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestOpportunityStatusFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			agreement1.Opportunity.P8_Status = "WON";

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			agreement2.Opportunity.P8_Status = "ABA";

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			agreement3.Opportunity.P8_Status = "SUS";

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var opportunityStatusFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.OpportunityStatus];

			AssertNotNull(opportunityStatusFilter);
			AssertEquals("Opportunity Status", opportunityStatusFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.OpportunitiesFilterCategory, opportunityStatusFilter.Category);

			opportunityStatusFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityStatusFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			opportunityStatusFilter.Property = "WON";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityStatusFilter.Property = "ABA";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityStatusFilter.Property = "SUS";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			opportunityStatusFilter.Property = "LOS";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestCustomerFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var agreement1A = Factory.New<OrgCommissionAgreement>();
			agreement1A.CA0_OH_Customer = org1.PK;
			agreement1A.FillWithValidTestData();

			var agreement1B = Factory.New<OrgCommissionAgreement>();
			agreement1B.CA0_OH_Customer = org1.PK;
			agreement1B.FillWithValidTestData();

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_OH_Customer = org2.PK;
			agreement2.FillWithValidTestData();

			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.CA0_OH_Customer = org3.PK;
			agreement3.FillWithValidTestData();
			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var customerFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.Customer];

			AssertNotNull(customerFilter);
			AssertEquals("Customer", customerFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.AgreementsFilterCategory, customerFilter.Category);

			customerFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1A, agreement1B, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			customerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1A, agreement1B }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			customerFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			customerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestSalesPersonFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			agreement1.Opportunity.P8_GS_NKPrimarySalesPerson = "AAA";

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			agreement2.Opportunity.P8_GS_NKPrimarySalesPerson = "BBB";

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			agreement3.Opportunity.P8_GS_NKPrimarySalesPerson = "CCC";

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var salesPersonFilter = (ModuleNkFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.SalesPerson];

			AssertNotNull(salesPersonFilter);
			AssertEquals("Sales Person", salesPersonFilter.MultilingualDescription);
			AssertEquals(FilterCategories.Organisations, salesPersonFilter.Category);

			salesPersonFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salesPersonFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			salesPersonFilter.Property = "AAA";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salesPersonFilter.Property = "BBB";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salesPersonFilter.Property = "CCC";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salesPersonFilter.Property = "DDD";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestAgreementStatusFilter()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.FillWithValidTestData();
			agreement1.CA0_ReversedDateUtc = ZDate.Today;

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.FillWithValidTestData();
			agreement2.CA0_EffectiveDate = ZDate.Today.AddDays(-2);

			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.FillWithValidTestData();
			agreement3.CA0_ExpiredDate = ZDate.Today.AddDays(-2);
			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var agreementStatusFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.AgreementStatus];

			AssertNotNull(agreementStatusFilter);
			AssertEquals("Agreement Status", agreementStatusFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.AgreementsFilterCategory, agreementStatusFilter.Category);

			agreementStatusFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			agreementStatusFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			agreementStatusFilter.Property = "REV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			agreementStatusFilter.Property = "A/I";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			agreementStatusFilter.Property = "EXP";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestEffectiveDateFilter()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.FillWithValidTestData();
			agreement1.CA0_EffectiveDate = ZDate.Today;

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.FillWithValidTestData();
			agreement2.CA0_EffectiveDate = ZDate.Today.AddDays(-5);

			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.FillWithValidTestData();
			agreement3.CA0_EffectiveDate = ZDate.Today.AddDays(-10);
			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var effectiveDateFilter = (ModuleDateFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.EffectiveDate];

			AssertNotNull(effectiveDateFilter);
			AssertEquals("Effective Date", effectiveDateFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.AgreementsFilterCategory, effectiveDateFilter.Category);

			effectiveDateFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			effectiveDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(-15);
			effectiveDateFilter.Property2 = ZDateTime.Today;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(-8);
			effectiveDateFilter.Property2 = ZDateTime.Today;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(-2);
			effectiveDateFilter.Property2 = ZDateTime.Today;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestBasisFilter()
		{
			var agreementPrv = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementPrv.CA0_CommissionBasis = "PRV";

			var agreementRev = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementRev.CA0_CommissionBasis = "REV";

			var agreementPrv2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementPrv2.CA0_CommissionBasis = "PRV";

			var agreementRev2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreementRev2.CA0_CommissionBasis = "REV";
			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var basisFilter = (ModuleTextFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.Basis];

			AssertNotNull(basisFilter);
			AssertEquals("Basis", basisFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.CommissionsFilterCategory, basisFilter.Category);

			basisFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementRev, agreementPrv2, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			basisFilter.Property = "PRV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementPrv2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.Property = "REV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementRev, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			basisFilter.Property = "PRV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementRev, agreementRev2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			basisFilter.Property = "REV";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreementPrv, agreementPrv2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestEntityStaffFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			var recipient = agreement1.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "AAA";

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			var recipient2 = agreement2.Recipients.AddNew();
			recipient2.CAR_GS_NKStaff = "BBB";

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			var recipient3 = agreement3.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "CCC";

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var entityStaffFilter = (ModuleNkFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.EntityStaff];

			AssertNotNull(entityStaffFilter);
			AssertEquals("Entity Staff", entityStaffFilter.MultilingualDescription);
			AssertEquals(FilterCategories.Organisations, entityStaffFilter.Category);

			entityStaffFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityStaffFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			entityStaffFilter.Property = "AAA";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityStaffFilter.Property = "BBB";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityStaffFilter.Property = "CCC";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityStaffFilter.Property = "DDD";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestEntityOrganizationFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			company1.GC_OH_OrgProxy = org1.PK;
			branch1.GB_GC = company1.PK;
			staff1.GS_GB_HomeBranch = branch1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			company2.GC_OH_OrgProxy = org2.PK;
			branch2.GB_GC = company2.PK;
			staff2.GS_GB_HomeBranch = branch2.PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			company3.GC_OH_OrgProxy = org3.PK;
			branch3.GB_GC = company3.PK;
			staff3.GS_GB_HomeBranch = branch3.PK;

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			company4.GC_OH_OrgProxy = org4.PK;
			branch4.GB_GC = company4.PK;
			staff4.GS_GB_HomeBranch = branch4.PK;

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			var recipient = agreement1.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staff1.GS_Code;

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			var recipient2 = agreement2.Recipients.AddNew();
			recipient2.CAR_GS_NKStaff = staff2.GS_Code;

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			var recipient3 = agreement3.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = staff3.GS_Code;

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var entityOrganizationFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.EntityOrganization];

			AssertNotNull(entityOrganizationFilter);
			AssertEquals("Entity Organization", entityOrganizationFilter.MultilingualDescription);
			AssertEquals(FilterCategories.Organisations, entityOrganizationFilter.Category);

			entityOrganizationFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityOrganizationFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			entityOrganizationFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityOrganizationFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityOrganizationFilter.Property = org3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			entityOrganizationFilter.Property = org4.PK;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestSalesPersonBranchFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			staff1.GS_GB_HomeBranch = branch1.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			staff2.GS_GB_HomeBranch = branch2.PK;

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			staff3.GS_GB_HomeBranch = branch3.PK;

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			staff4.GS_GB_HomeBranch = branch4.PK;

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			agreement1.Opportunity.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			agreement2.Opportunity.P8_GS_NKPrimarySalesPerson = staff2.GS_Code;

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			agreement3.Opportunity.P8_GS_NKPrimarySalesPerson = staff3.GS_Code;

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var salespersonBranchFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.SalespersonBranch];
			AssertEquals(FilterCategories.Organisations, salespersonBranchFilter.Category);

			AssertNotNull(salespersonBranchFilter);
			AssertEquals("Sales Person Branch", salespersonBranchFilter.MultilingualDescription);

			salespersonBranchFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonBranchFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			salespersonBranchFilter.Property = branch1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonBranchFilter.Property = branch2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonBranchFilter.Property = branch3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonBranchFilter.Property = branch4.PK;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestSalesPersonDepartmentFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			staff1.GS_GE_HomeDepartment = department1.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			staff2.GS_GE_HomeDepartment = department2.PK;

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			staff3.GS_GE_HomeDepartment = department3.PK;

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			var department4 = Factory.NewWithValidTestData<GlbDepartment>();
			staff4.GS_GE_HomeDepartment = department4.PK;

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			agreement1.Opportunity.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			agreement2.Opportunity.P8_GS_NKPrimarySalesPerson = staff2.GS_Code;

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();
			agreement3.Opportunity.P8_GS_NKPrimarySalesPerson = staff3.GS_Code;

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var salespersonDepartmentFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.SalespersonDepartment];

			AssertNotNull(salespersonDepartmentFilter);
			AssertEquals("Sales Person Department", salespersonDepartmentFilter.MultilingualDescription);
			AssertEquals(FilterCategories.Organisations, salespersonDepartmentFilter.Category);

			salespersonDepartmentFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonDepartmentFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			salespersonDepartmentFilter.Property = department1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonDepartmentFilter.Property = department2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonDepartmentFilter.Property = department3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			salespersonDepartmentFilter.Property = department4.PK;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		public void TestCompanyFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_GC = company1.PK;

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			opportunity2.P8_GC = company2.PK;

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			opportunity3.P8_GC = company3.PK;

			var company4 = Factory.NewWithValidTestData<GlbCompany>();

			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			var agreement3 = opportunity3.CommissionAgreements.AddNew();
			agreement3.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = new CommissionAgreementApprovalFilterBusinessObject();
			var companyFilter = (ModuleGuidFilter)filterBizObj[CommissionAgreementApprovalFilterBusinessObject.FilterDescription.Company];

			AssertNotNull(companyFilter);
			AssertEquals("Company", companyFilter.MultilingualDescription);
			AssertEquals(CommissionAgreementApprovalFilterBusinessObject.OpportunitiesFilterCategory, companyFilter.Category);

			companyFilter.IsActive = true;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1, agreement2, agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			companyFilter.ComparisonOperator = ModuleFountainFilter.ComparisonConstants.Exact;
			companyFilter.Property = company1.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement1 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			companyFilter.Property = company2.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement2 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			companyFilter.Property = company3.PK;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { agreement3 }, Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));

			companyFilter.Property = company4.PK;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<OrgCommissionAgreement>(), Factory.Load<OrgCommissionAgreement>(filterBizObj.Filter));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommissionAgreementApprovalFilterBusinessObject();
		}

		#endregion
	}
}
