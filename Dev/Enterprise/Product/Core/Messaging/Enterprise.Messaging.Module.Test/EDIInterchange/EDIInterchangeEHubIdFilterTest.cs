using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIInterchangeEHubIdFilter))]
	sealed class EDIInterchangeEHubIdFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new EDIInterchangeEHubIdFilter(EDIInterchangeFilterBusinessObject.Descriptions.eHubFilter);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ZString ExpectedDescription => EDIInterchangeFilterBusinessObject.Descriptions.eHubFilter;

		public void TestEDIInterchangeEHubIdFilter()
		{
			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			Factory.Save();

			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var filter = (EDIInterchangeEHubIdFilter)interchangeFilterBusinessObject[EDIInterchangeFilterBusinessObject.Descriptions.eHubFilter];
			{
				filter.Property = "C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact positive", 1, interchanges.Length);
			}

			{
				filter.Property = "C94DC724-4943-47E1-AB1A-E0F8196D5F4D";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact negative", 0, interchanges.Length);
			}
		}

		public void TestEDIInterchangeEHubIdFilterValidationErrors()
		{
			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");
			Factory.Save();

			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var filter = (EDIInterchangeEHubIdFilter)interchangeFilterBusinessObject[EDIInterchangeFilterBusinessObject.Descriptions.eHubFilter];
			{
				filter.Property = "C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter.Validation.ValidateProperty();
				AssertNoErrors(filter.PropertyInfo);
			}

			{
				filter.Property = "C7E2E1A3";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter.Validation.ValidateProperty();
				AssertHasError(filter.PropertyInfo, "The value is in incorrect format.");
			}
		}
	}
}
