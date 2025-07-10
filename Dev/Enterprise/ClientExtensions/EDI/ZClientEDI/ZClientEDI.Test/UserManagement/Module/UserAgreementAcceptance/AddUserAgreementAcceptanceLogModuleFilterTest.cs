namespace Enterprise.Client.EDI.Test
{
	using CargoWise.Types;
	using Enterprise.Client.EDI.MasterFiles.Module;
	using Enterprise.Client.EDI.UserManagement.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(AddUserAgreementAcceptanceLogModuleFilter))]
	public class AddUserAgreementAcceptanceLogModuleFilterTest : ModuleFilterTestCase<AddUserAgreementAcceptanceLogModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override AddUserAgreementAcceptanceLogModuleFilter GetNewModuleFilter()
		{
			return new AddUserAgreementAcceptanceLogModuleFilter(Modules.ClientModuleRegistration.UserAgreementAcceptances, OrgContactSchema.PK, Factory, typeof(EdiUserAgreementAcceptanceLog));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ZString ExpectedDescription
		{
			get { return "User Agreement Acceptances (Multiple)"; }
		}
	}
}
