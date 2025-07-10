using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Module.Testing
{
	[TestedType(typeof(EdiUserAgreementFilterBusinessObject))]
	public class EdiUserAgreementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters
		public void TestTitleFilter()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Title = "Data Collection Agreement";
			Factory.Save();
			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.Title];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Data";
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(agreement));
			titleFilter.Property = "Strata";
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(agreement));
		}

		public void TestTypeFilter()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = "DCA";
			Factory.Save();
			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.Type];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "DCA";
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(agreement));
			titleFilter.Property = "ACA";
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(agreement));
		}

		public void TestContentFilter()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Content = "Data Collection Agreement";
			Factory.Save();
			var titleFilter = (ModuleTextFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.Content];
			titleFilter.IsActive = true;
			titleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			titleFilter.Property = "Data";
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(agreement));
			titleFilter.Property = "Strata";
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(agreement));
		}

		#endregion
		#region Date Filters
		public void TestEffectiveDateFilter()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = "EUA";
			agreement1.ERA_RN_NKCountryCode = "AU";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(5);
			agreement2.ERA_Type = agreement1.ERA_Type;
			agreement2.ERA_RN_NKCountryCode = agreement1.ERA_RN_NKCountryCode;
			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
			agreement3.ERA_Type = agreement1.ERA_Type;
			agreement3.ERA_RN_NKCountryCode = "NZ";
			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(5);
			agreement4.ERA_Type = "DCA";
			agreement4.ERA_RN_NKCountryCode = "NZ";
			Factory.Save();
			var dateFilter = (ModuleDateFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.EffectiveDate];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.UtcNow.AddDays(-3);
			dateFilter.Property2 = ZDateTime.UtcNow.AddDays(-1);
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should not contain agreement1", false, result.Contains(agreement1));
			AssertEquals("Should not contain agreement2", false, result.Contains(agreement2));
			AssertEquals("Should not contain agreement3", false, result.Contains(agreement3));
			AssertEquals("Should not contain agreement4", false, result.Contains(agreement4));
			dateFilter.Property2 = ZDateTime.UtcNow.AddDays(1);
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement1", true, result.Contains(agreement1));
			AssertEquals("Should not contain agreement2", false, result.Contains(agreement2));
			AssertEquals("Should contain agreement3", true, result.Contains(agreement3));
			AssertEquals("Should not contain agreement4", false, result.Contains(agreement4));
			dateFilter.Property2 = ZDateTime.UtcNow.AddDays(10);
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement1", true, result.Contains(agreement1));
			AssertEquals("Should contain agreement2", true, result.Contains(agreement2));
			AssertEquals("Should contain agreement3", true, result.Contains(agreement3));
			AssertEquals("Should contain agreement4", true, result.Contains(agreement4));
			dateFilter.Property1 = ZDateTime.UtcNow.AddDays(7);
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should not contain agreement1", false, result.Contains(agreement1));
			AssertEquals("Should contain agreement2", true, result.Contains(agreement2));
			AssertEquals("Should contain agreement3", true, result.Contains(agreement3));
			AssertEquals("Should contain agreement4", true, result.Contains(agreement4));
		}

		#endregion
		#region Flag Filters
		public void TestIsActiveFilter()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", true, agreement1.ERA_IsActive);
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_IsActive = false;
			Factory.Save();
			var flagsFilter = (ModuleFlagsFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.IsActive];
			flagsFilter.IsActive = true;
			flagsFilter.Property0 = true;
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement1", true, result.Contains(agreement1));
			AssertEquals("Should not contain agreement2", false, result.Contains(agreement2));
			flagsFilter.Property0 = false;
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should not contain agreement1", false, result.Contains(agreement1));
			AssertEquals("Should contain agreement2", true, result.Contains(agreement2));
		}

		#endregion
		#region Related Item Filters
		public void TestCountryFilter()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_RN_NKCountryCode = "AU";
			Factory.Save();
			var moduleNkFilter = (ModuleNkFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.Country];
			moduleNkFilter.IsActive = true;
			moduleNkFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			moduleNkFilter.Property = "AU";
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(agreement));
			moduleNkFilter.Property = "NZ";
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(agreement));
		}

		#endregion
		#region Number Range Filters
		public void TestVersionNumberFilter()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_VersionNumber = 12;
			Factory.Save();
			var titleFilter = (ModuleNumberRangeFilter)FilterBizO[EdiUserAgreementFilterBusinessObject.FilterDescription.VersionNumber];
			titleFilter.IsActive = true;
			titleFilter.Property1 = 9;
			titleFilter.Property2 = 12;
			var result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", true, result.Contains(agreement));
			titleFilter.Property2 = 11;
			result = new EdiUserAgreementCollection(Factory, FilterBizO.Filter);
			AssertEquals("Should contain agreement", false, result.Contains(agreement));
		}

		#endregion
		#region Implementation
		EdiUserAgreementFilterBusinessObject FilterBizO
		{
			get
			{
				return (EdiUserAgreementFilterBusinessObject)CachedBusinessObject;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiUserAgreementFilterBusinessObject();
		}
		#endregion
	}
}
