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

	[TestedType(typeof(ContactAcceptanceLogModuleFilter))]
	public class ContactAcceptanceLogModuleFilterTest : ModuleFilterTestCase<ContactAcceptanceLogModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ContactAcceptanceLogModuleFilter GetNewModuleFilter()
		{
			return new ContactAcceptanceLogModuleFilter(ModuleIDs.OrgContacts, OrgContactSchema.PK, EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, Factory, typeof(EdiUserAgreementAcceptanceLog));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ZString ExpectedDescription
		{
			get { return "Organization Contacts (Multiple)"; }
		}
	}
}
