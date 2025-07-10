namespace Enterprise.Client.EDI.Test
{
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.Client.EDI.UserManagement.Business;
	using Enterprise.Client.EDI.UserManagement.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EdiUserAgreementAcceptanceLogFilterBusinessObject))]
	public class EdiUserAgreementAcceptanceLogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		void PreparData(out EdiUserAgreementAcceptanceLog acceptanceLog)
		{
			var userAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			userAgreement.ERA_Title = "Data Collection Agreement";
			userAgreement.ERA_Type = "DCA";
			userAgreement.ERA_Content = "Content Collection Agreement";
			userAgreement.ERA_RN_NKCountryCode = "AU";
			userAgreement.ERA_VersionNumber = 12;

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_FullName = "Test User Account";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Contact Name Test";
			userAccount.EUA_OC_WebAccessContact = contact.PK;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Organisation Name Test";
			contact.OC_OH = organisation.PK;

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_Product = "LDP";
			userAccount.EUA_LD = database.PK;

			acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = userAgreement.PK;
			acceptanceLog.EUL_EUA = userAccount.PK;

			Factory.Save();
		}

		#region Text Filters

		public void TestTitleFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.Title];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Data";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "Strata";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestTypeFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.Type];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "DCA";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "ACA";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestContentFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.Content];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Content";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "Strata";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestContactNameFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.ContactName];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Contact Name";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "Strata";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestOrganisationNameFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.OrganisationName];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Organisation Name";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "Strata";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestProductCodeFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.ProductCode];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "LDP";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property = "AAA";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		#endregion

		#region Flag Filters

		public void TestIsActiveFilter()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", true, agreement1.ERA_IsActive);

			var acceptanceLog1 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog1.EUL_ERA = agreement1.PK;
			acceptanceLog1.EUL_EUA = Factory.NewWithValidTestData<EdiCustomerUserAccount>().PK;

			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_IsActive = false;
			AssertEquals("Precondition", false, agreement2.ERA_IsActive);

			var acceptanceLog2 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog2.EUL_ERA = agreement2.PK;
			acceptanceLog2.EUL_EUA = Factory.NewWithValidTestData<EdiCustomerUserAccount>().PK;

			Factory.Save();

			var flagsFilter = (ModuleFlagsFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.IsActive];
			flagsFilter.IsActive = true;
			flagsFilter.Property0 = true;

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement1", true, result.Contains(acceptanceLog1));
			AssertEquals("Should not contain agreement2", false, result.Contains(acceptanceLog2));

			flagsFilter.Property0 = false;
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should not contain agreement1", false, result.Contains(acceptanceLog1));
			AssertEquals("Should contain agreement2", true, result.Contains(acceptanceLog2));
		}

		#endregion

		#region Related Item Filters

		public void TestCountryFilter()
		{
			PreparData(out var acceptanceLog);

			var moduleNkFilter = (ModuleNkFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.AgreementCountry];
			moduleNkFilter.IsActive = true;
			moduleNkFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			moduleNkFilter.Property = "AU";

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			moduleNkFilter.Property = "NZ";
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		public void TestContactFilter()
		{
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Test contact name";
			contact1.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var acceptanceLog1 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog1.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog1.EUL_EUA = userAccount1.PK;

			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var acceptanceLog2 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog2.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog2.EUL_EUA = userAccount2.PK;

			Factory.Save();

			var contactFilter = (ModuleGuidFilter)FilterBizO["Organization Contacts (Multiple)"];
			contactFilter.IsActive = true;
			contactFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			contactFilter.SelectedFilters.AddTextFilterStrip("Name", "Test contact name");
			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain contact", true, result.Contains(acceptanceLog1));
			AssertEquals("Should not contain contact", false, result.Contains(acceptanceLog2));
		}

		public void TestOrganisationFilter()
		{
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test org1 full name";
			contact1.OC_OH = org1.PK;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var acceptanceLog1 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog1.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog1.EUL_EUA = userAccount1.PK;

			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			contact2.OC_OH = org2.PK;
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var acceptanceLog2 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog2.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog2.EUL_EUA = userAccount2.PK;

			Factory.Save();

			var contactFilter = (ModuleGuidFilter)FilterBizO["Organization (Multiple)"];
			contactFilter.IsActive = true;
			contactFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			contactFilter.SelectedFilters.AddTextFilterStrip("Name", "Test org1 full name");
			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain organisation", true, result.Contains(acceptanceLog1));
			AssertEquals("Should not contain organisation", false, result.Contains(acceptanceLog2));
		}

		#endregion

		#region Number Range Filters

		public void TestVersionNumberFilter()
		{
			PreparData(out var acceptanceLog);

			var titleFilter = (ModuleNumberRangeFilter)FilterBizO[EdiUserAgreementAcceptanceLogFilterBusinessObject.FilterDescription.VersionNumber];
			titleFilter.IsActive = true;
			titleFilter.Property1 = 9;
			titleFilter.Property2 = 12;

			var result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(acceptanceLog));

			titleFilter.Property2 = 11;
			result = new EdiUserAgreementAcceptanceLogCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(acceptanceLog));
		}

		#endregion

		#region Implementation

		EdiUserAgreementAcceptanceLogFilterBusinessObject FilterBizO
		{
			get { return (EdiUserAgreementAcceptanceLogFilterBusinessObject)CachedBusinessObject; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiUserAgreementAcceptanceLogFilterBusinessObject();
		}

		#endregion
	}
}
