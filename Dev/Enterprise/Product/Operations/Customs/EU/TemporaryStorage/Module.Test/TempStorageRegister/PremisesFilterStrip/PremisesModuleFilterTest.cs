using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(PremisesModuleFilter))]
	class PremisesModuleFilterTest : ModuleTextFilterTest
	{
		public void TestClearAndIsEmpty()
		{
			PremisesModuleFilter.PremisesType = "ADT";
			PremisesModuleFilter.PremisesCode = "Code";
			PremisesModuleFilter.PremisesLocation = "Location";

			CombineAssertions(() =>
			{
				AssertEquals("Filter is not empty", false, PremisesModuleFilter.IsEmpty);
				AssertEquals("Query is not empty", false, PremisesModuleFilter.Query.IsEmpty);

				PremisesModuleFilter.Clear();
				AssertEquals("Filter is empty", true, PremisesModuleFilter.IsEmpty);
				AssertEquals("Query is empty", true, PremisesModuleFilter.Query.IsEmpty);

				AssertEquals("PremisesType is cleared", ZString.Empty, PremisesModuleFilter.PremisesType);
				AssertEquals("PremisesCode is cleared", ZString.Empty, PremisesModuleFilter.PremisesCode);
				AssertEquals("PremisesLocation is cleared", ZString.Empty, PremisesModuleFilter.PremisesLocation);
			});
		}

		public void TestPremisesTypeList()
		{
			string expected = "ADT - Temporary Storage Warehouse\r\n" + "LAM - Export Storage Facility (LAME)";
			AssertEquals(expected, PremisesModuleFilter.PremisesTypesList.ElementsAsString);
		}

		public void TestPremisesCode_WhenUsingComparisonOperator_PremisesCodeInfoMayBeReadOnly()
		{
			var filter = new PremisesModuleFilter("Premises");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals("PremisesCode should be editable when using the 'exact' operator", false, filter.PremisesCodeInfo.ReadOnly);
			filter.PremisesCode = "abc";
			AssertEquals("abc", filter.PremisesCode);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("PremisesCode should be editable when using the 'not equal' operator", false, filter.PremisesCodeInfo.ReadOnly);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals(ZString.Empty, filter.PremisesCode);
			AssertEquals("PremisesCode should be read only when using the 'is blank' operator", true, filter.PremisesCodeInfo.ReadOnly);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals(ZString.Empty, filter.PremisesCode);
			AssertEquals("PremisesCode should be read only when using the 'is not blank' operator", true, filter.PremisesCodeInfo.ReadOnly);
		}

		public void TestPremisesCodeComparisonOperator_DefaultValue()
		{
			var filter = new PremisesModuleFilter("Premises");

			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, filter.ComparisonOperator);
		}

		public void TestPremisesCode_WhenIsBlankOrIsNotBlankComparison_PremisesCodeIsEmpty()
		{
			var filter = new PremisesModuleFilter("Premises");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.PremisesCode = "abc";
			AssertEquals("Precondition: PremisesCode is not an empty string", "abc", filter.PremisesCode);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("When using the IsBlank comparison operator, PremisesCode is an empty string", ZString.Empty, filter.PremisesCode);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.PremisesCode = "abc";
			AssertEquals("Precondition: PremisesCode is not an empty string", "abc", filter.PremisesCode);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals("When using the IsNotBlank comparison operator, PremisesCode is an empty string", ZString.Empty, filter.PremisesCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PremisesModuleFilter("Test");
		}

		PremisesModuleFilter PremisesModuleFilter
		{
			get
			{
				if (premisesModuleFilter == null)
				{
					premisesModuleFilter = (PremisesModuleFilter)GetNewBusinessObject();
				}

				return premisesModuleFilter;
			}
		}
		PremisesModuleFilter premisesModuleFilter;

		#endregion
	}
}
