using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	public class FilterStripValidationTest : BusinessObjectValidationTestCase
	{
		#region TestFilterDescription

		public void TestFilterDescription()
		{
			Strip.FilterDescription = "";
			AssertNoErrors(Strip.FilterDescriptionInfo);

			Strip.FilterDescription = "Advent";
			AssertHasError(Strip.FilterDescriptionInfo, "Enter a valid selection.");

			Strip.FilterDescription = "Z0_Description";
			AssertNoErrors(Strip.FilterDescriptionInfo);
		}

		#endregion

		#region Implementation

		FilterStrip Strip
		{
			get
			{
				if (fStrip == null)
				{
					var moduleFilters = new ModuleFilterCollection();
					moduleFilters.AddFilter(ModuleFilter);

					fStrip = new FilterStrip(moduleFilters);
				}
				return fStrip;
			}
		}

		DummyModuleFilter ModuleFilter
		{
			get
			{
				if (fModuleFilter == null)
				{
					fModuleFilter = new DummyModuleFilter("Z0_Description", DummyBizoSchema.Z0_Description);
				}
				return fModuleFilter;
			}
		}

		FilterStrip fStrip;
		DummyModuleFilter fModuleFilter;

		#endregion
	}
}
