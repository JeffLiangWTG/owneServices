using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingResponsibleAgreementsCalculatorTest : TestCaseWithFactory
	{
		public void TestGet_Company()
		{
			var ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licDatabase1 = ent.Databases.AddNew();
			licDatabase1.LD_ServerCode = "111";
			var licDatabase2 = ent.Databases.AddNew();
			licDatabase2.LD_ServerCode = "222";
			var company = ent.Companies.AddNew();
			company.LC_CompanyCode = "CO1";
			company.LC_LE = ent.PK;
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			company.LC_OH = customer.PK;
			var clientCompany1A = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1A.LCC_LD = licDatabase1.PK;
			var clientCompany1B = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1B.LCC_LD = licDatabase1.PK;
			var clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_LD = licDatabase2.PK;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);

			var agreement1 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement1.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement1.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");

			var agreement2 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customer.PK;
			agreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement2.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement2.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "AAA", "AAA", "ALL");
			var agreement2Customization = agreement2.GetOrCreateCustomization();
			agreement2Customization.EZN_IsAllCompanies = true;

			var agreement3 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customer.PK;
			agreement3.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement3.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement3.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement3, "AAA", "ALL", "ALL");
			var agreement3Customization = agreement3.GetOrCreateCustomization();
			agreement3Customization.EZN_IsAllCompanies = false;
			agreement3Customization.CompanyPivots.AddNew(clientCompany1A);

			var agreement4 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement4.CA0_Name = "#4";
			agreement4.CA0_OH_Customer = customer.PK;
			agreement4.CA0_CommissionStream = "WBP";
			agreement4.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement4.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement4.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement4, "AAA", "AAA", "AAA");

			var agreement5 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement5.CA0_Name = "#5";
			agreement5.CA0_OH_Customer = customer.PK;
			agreement5.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement5.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement5.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement5, "AAA", "ALL", "ALL");
			var agreement5Customization = agreement5.GetOrCreateCustomization();
			agreement5Customization.EZN_IsAllCompanies = false;
			agreement5Customization.CompanyPivots.AddNew(clientCompany1B);

			var agreement6 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement6.CA0_Name = "#6";
			agreement6.CA0_OH_Customer = customer.PK;
			agreement6.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement6.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement6.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement6, "BBB", "ALL", "ALL");
			var agreement6Customization = agreement6.GetOrCreateCustomization();
			agreement6Customization.EZN_IsAllCompanies = false;
			agreement6Customization.CompanyPivots.AddNew(clientCompany1B);

			var agreement7 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement7.CA0_Name = "#7";
			agreement7.CA0_OH_Customer = customer.PK;
			agreement7.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement7.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement7.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement7, "BBB", "BBB", "ALL");
			var agreement7Customization = agreement7.GetOrCreateCustomization();
			agreement7Customization.EZN_IsAllCompanies = false;
			agreement7Customization.CompanyPivots.AddNew(clientCompany1B);

			Factory.Save();

			var calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "AAA", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3,
					agreement4
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany1A.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany1A.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement5
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany1B.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3,
					agreement5
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany1B.PK, clientCompany1A.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "AAA", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement2,
					agreement4
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany2.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "BBB", "BBB", "BBB", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement7
				},
				calculator.GetForClientCompanies(new ZGuid[] { clientCompany1B.PK }).Values);
		}

		public void TestGet_Database()
		{
			var ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licDatabase1 = ent.Databases.AddNew();
			licDatabase1.LD_ServerCode = "111";
			var licDatabase2 = ent.Databases.AddNew();
			licDatabase2.LD_ServerCode = "222";
			var licDatabase3 = ent.Databases.AddNew();
			licDatabase3.LD_ServerCode = "333";
			var company = ent.Companies.AddNew();
			company.LC_CompanyCode = "CO1";
			company.LC_LE = ent.PK;
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			company.LC_OH = customer.PK;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);

			var agreement1 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement1.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement1.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");

			var agreement2 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customer.PK;
			agreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement2.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement2.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "AAA", "AAA", "ALL");
			var agreement2Customization = agreement2.GetOrCreateCustomization();
			agreement2Customization.EZN_IsAllDatabases = true;

			var agreement3 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customer.PK;
			agreement3.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement3.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement3.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement3, "AAA", "ALL", "ALL");
			var agreement3Customization = agreement3.GetOrCreateCustomization();
			agreement3Customization.EZN_IsAllDatabases = false;
			agreement3Customization.DatabasePivots.AddNew(licDatabase1);

			var agreement4 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement4.CA0_Name = "#4";
			agreement4.CA0_OH_Customer = customer.PK;
			agreement4.CA0_CommissionStream = "WBP";
			agreement4.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement4.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement4.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement4, "AAA", "AAA", "AAA");

			var agreement5 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement5.CA0_Name = "#5";
			agreement5.CA0_OH_Customer = customer.PK;
			agreement5.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement5.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement5.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement5, "AAA", "ALL", "ALL");
			var agreement5Customization = agreement5.GetOrCreateCustomization();
			agreement5Customization.EZN_IsAllDatabases = false;
			agreement5Customization.DatabasePivots.AddNew(licDatabase2);

			var agreement6 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement6.CA0_Name = "#6";
			agreement6.CA0_OH_Customer = customer.PK;
			agreement6.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement6.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement6.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement6, "BBB", "ALL", "ALL");
			var agreement6Customization = agreement6.GetOrCreateCustomization();
			agreement6Customization.EZN_IsAllDatabases = false;
			agreement6Customization.DatabasePivots.AddNew(licDatabase2);

			var agreement7 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement7.CA0_Name = "#7";
			agreement7.CA0_OH_Customer = customer.PK;
			agreement7.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement7.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement7.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement7, "BBB", "BBB", "ALL");
			var agreement7Customization = agreement7.GetOrCreateCustomization();
			agreement7Customization.EZN_IsAllDatabases = false;
			agreement7Customization.DatabasePivots.AddNew(licDatabase2);

			Factory.Save();

			var calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "AAA", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3,
					agreement4
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase1.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase1.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement5
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase2.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "ALL", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement3,
					agreement5
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase1.PK, licDatabase2.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "AAA", "AAA", "AAA", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement2,
					agreement4
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase3.PK }).Values);

			calculator = new BillingResponsibleAgreementsCalculator(Factory, customer.PK, "BBB", "BBB", "BBB", new ZDate(2020, 2, 2));
			AssertContainsExactElementsInAnyOrder("",
				x => x.CA0_Name,
				new[]
				{
					agreement7
				},
				calculator.GetForLicenseDatabases(new ZGuid[] { licDatabase2.PK }).Values);
		}
	}
}
