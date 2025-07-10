using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleLocationFilterTest : ModuleCodeFilterTest
	{
		#region Empty Location Comparison

		public void TestLocationCanFilterEmptyLocation()
		{
			Assert("Pre-condition: Z0_Code is nullable", DummyBizoSchema.Z0_Code.IsNullable);
			Assert("Pre-condition: Z0_Description is not nullable", !DummyBizoSchema.Z0_Description.IsNullable);

			var dummyWithCodeNullAndDescriptionEmpty = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithCodeNullAndDescriptionEmpty.Z0_Code = null;
			dummyWithCodeNullAndDescriptionEmpty.Z0_Description = "";

			var dummyWithBothEmpty = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithBothEmpty.Z0_Code = "";
			dummyWithBothEmpty.Z0_Description = "";

			var dummyWithEmptyCode = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithEmptyCode.Z0_Code = "";
			dummyWithEmptyCode.Z0_Description = "AUSYD";

			var dummyWithEmptyDescription = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithEmptyDescription.Z0_Code = "INIXE";
			dummyWithEmptyDescription.Z0_Description = "";

			var dummyWithNonEmpty = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithNonEmpty.Z0_Code = "INIXE";
			dummyWithNonEmpty.Z0_Description = "AUSYD";
			Factory.Save();

			var filter = Filter as ModuleLocationFilter;

			filter.Property1 = "";
			filter.Property2 = "";
			Assert("Pre-condition: IsEmptyProperty1ComparisonOperation should be false", !filter.IsEmptyProperty1ComparisonOperation);
			Assert("Pre-condition: IsEmptyProperty2ComparisonOperation should be false", !filter.IsEmptyProperty2ComparisonOperation);
			AssertCollection(new[] { dummyWithCodeNullAndDescriptionEmpty, dummyWithBothEmpty, dummyWithEmptyCode, dummyWithEmptyDescription, dummyWithNonEmpty });

			filter.IsEmptyProperty1ComparisonOperation = true;
			filter.IsEmptyProperty2ComparisonOperation = true;
			AssertCollection(new[] { dummyWithCodeNullAndDescriptionEmpty, dummyWithBothEmpty });

			filter.Property1 = "";
			filter.Property2 = "AUSYD";
			AssertCollection(new[] { dummyWithEmptyCode });

			filter.IsEmptyProperty1ComparisonOperation = false;
			AssertCollection(new[] { dummyWithEmptyCode, dummyWithNonEmpty });

			filter.Property1 = "INIXE";
			filter.Property2 = "";
			AssertCollection(new[] { dummyWithEmptyDescription });

			filter.IsEmptyProperty2ComparisonOperation = false;
			AssertCollection(new[] { dummyWithEmptyDescription, dummyWithNonEmpty });

			filter.Property1 = "INIXE";
			filter.Property2 = "AUSYD";
			AssertCollection(new[] { dummyWithNonEmpty });
		}

		#endregion

		#region International Zones Testing

		public void TestIncludesInternationalZonesWhenTypeIsIncluded()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "AUSR";
			dummy.Z0_Description = "USCA";
			Factory.Save();

			Filter.Property1 = "AUSR";
			Filter.Property2 = "";
			AssertCollection(dummy, true, "Should match by International Zone Code");

			Filter.Property1 = "";
			Filter.Property2 = "AUSR";
			AssertCollection(dummy, false);

			Filter.Property1 = "";
			Filter.Property2 = "USCA";
			AssertCollection(dummy, true, "Should match by International Zone Code");

			Filter.Property1 = "AUSR";
			Filter.Property2 = "USCA";
			AssertCollection(dummy, true, "Should match by International Zone Code");

			Filter.Property1 = "AUMEL";
			Filter.Property2 = "";
			AssertCollection(dummy, true, "Should find Melbourne in the Australian Zone");

			Filter.Property1 = "";
			Filter.Property2 = "AUMEL";
			AssertCollection(dummy, false);

			Filter.Property1 = "";
			Filter.Property2 = "USLAX";
			AssertCollection(dummy, true, "Should find LAX in the US West Coast Zone");

			Filter.Property1 = "AUMEL";
			Filter.Property2 = "USLAX";
			AssertCollection(dummy, true);

			Filter.Property1 = "HM"; //Heard Island and McDonald Islands
			Filter.Property2 = "";
			AssertCollection(dummy, true, "Should match by just country code");

			Filter.Property1 = "";
			Filter.Property2 = "HM";
			AssertCollection(dummy, false);
		}

		#endregion

		#region TestLocationCanFilterOn2LetterCountryCode

		public void TestLocationCanFilterOn2LetterCountryCode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "AUMEL";
			dummy.Z0_Description = "USLAX";
			Factory.Save();

			Filter.Property1 = "AUMEL";
			Filter.Property2 = "";
			AssertCollection(dummy, true);

			Filter.Property1 = "";
			Filter.Property2 = "AUMEL";
			AssertCollection(dummy, false);

			Filter.Property1 = "";
			Filter.Property2 = "USLAX";
			AssertCollection(dummy, true);

			Filter.Property1 = "AUMEL";
			Filter.Property2 = "USLAX";
			AssertCollection(dummy, true);

			Filter.Property1 = "AU";
			Filter.Property2 = "";
			AssertCollection(dummy, true);

			Filter.Property1 = "";
			Filter.Property2 = "AU";
			AssertCollection(dummy, false);

			Filter.Property1 = "";
			Filter.Property2 = "US";
			AssertCollection(dummy, true);

			Filter.Property1 = "AU";
			Filter.Property2 = "US";
			AssertCollection(dummy, true);
		}

		#endregion

		#region Implementation

		void AssertCollection(DummyBusinessObject dummy, bool isExpected, string message = "")
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(Filter.Query);

			if (isExpected)
			{
				AssertCollectionContains(message, dummy, collection);
			}
			else
			{
				AssertCollectionNotContains(message, dummy, collection);
			}
		}

		void AssertCollection(DummyBusinessObject[] dummyBusinessObjects, string message = "")
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(message, dummyBusinessObjects, collection);
		}

		#region Setup

		protected override ModuleCodeFilter GetNewModuleFilter()
		{
			return new ModuleLocationFilter("moo", DummyBizoSchema.Z0_Code, Locations, DummyBizoSchema.Z0_Description, Locations, true);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		IBusinessObjectCollection Locations
		{
			get
			{
				if (fLocations == null)
				{
					fLocations = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.ILocationCollection>(), new object[] { Factory });
				}
				return fLocations;
			}
		}

		IBusinessObjectCollection fLocations;

		#endregion

		#endregion
	}
}
