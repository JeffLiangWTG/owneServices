namespace Enterprise.Client.EDI.Test
{
	using CargoWise.Types;
	using Enterprise.Client.EDI.UserManagement.Business;
	using Enterprise.Client.EDI.UserManagement.Module;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(OrganisationAcceptanceLogModuleFilter))]
	public class OrganisationAcceptanceLogModuleFilterTest : ModuleFilterTestCase<OrganisationAcceptanceLogModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrganisationAcceptanceLogModuleFilter GetNewModuleFilter()
		{
			return new OrganisationAcceptanceLogModuleFilter(ModuleIDs.Organisation, EdiUserAgreementAcceptanceLogSchema.PK, OrgContactSchema.OC_OH, Factory, typeof(EdiUserAgreementAcceptanceLog));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;

		protected override ZString ExpectedDescription
		{
			get { return "Organization (Multiple)"; }
		}

		protected override FilterCategory InitialTestCatergory => FilterCategories.Other;
	}
}
