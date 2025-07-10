using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmActivityLogFilterProvider))]
	sealed class StmActivityLogFilterProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStaffList()
		{
			IGlbStaff staff = Factory.New<IGlbStaff>();
			StmActivityLogCollectionByStaff coll = new StmActivityLogCollectionByStaff(staff);
			StmActivityLogFilterProvider provider = new StmActivityLogFilterProvider(coll);
			AssertNotNull(provider.Staff);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, provider.ActivityLogFilterStaffInfo.MaxLength);
		}

		public void TestActivityLogFilterTypes_AllTypesFitField()
		{
			var staff = Factory.New<IGlbStaff>();
			var collection = new StmActivityLogCollectionByStaff(staff);
			var provider = new StmActivityLogFilterProvider(collection);
			var filterActivityTypes = provider.ActivityLogFilterTypes;

			foreach (CodeDescriptionPair activityType in filterActivityTypes)
			{
				provider.ActivityLogFilterType = activityType.Code;
				provider.RunPreSaveValidation();
				AssertNoErrors("We should be able to assign '" + activityType.Code + "' to the ActivityType field, and yet...", provider.ActivityLogFilterTypeInfo);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			StmActivityLogCollectionByParent coll = new StmActivityLogCollectionByParent(dummy);
			return new StmActivityLogFilterProvider(coll);
		}

		public void TestActivityLogFilterDateFrom()
		{
			var staff = Factory.New<IGlbStaff>();
			var coll = new StmActivityLogCollectionByStaff(staff);
			var provider = new StmActivityLogFilterProvider(coll);
			provider.ActivityLogFilterDateFrom = ZDateTime.Invalid;
			AssertHasError(provider.ActivityLogFilterDateFromInfo, "The 'From' date field is invalid!");
			provider.ActivityLogFilterDateFrom = ZDateTime.Today;
			AssertNoErrors(provider.ActivityLogFilterDateFromInfo);

			TestActivityLogFilterFromValidation(provider, 10, 10);
			TestActivityLogFilterFromValidation(provider, 15, 20);
			TestActivityLogFilterFromValidation(provider, 100, 0);
		}

		public void TestActivityLogFilterDateTo()
		{
			var staff = Factory.New<IGlbStaff>();
			var coll = new StmActivityLogCollectionByStaff(staff);
			var provider = new StmActivityLogFilterProvider(coll);
			provider.ActivityLogFilterDateTo = ZDateTime.Invalid;
			AssertHasError(provider.ActivityLogFilterDateToInfo, "The 'To' date field is invalid!");
			provider.ActivityLogFilterDateTo = ZDateTime.Today;
			AssertNoErrors(provider.ActivityLogFilterDateToInfo);

			TestActivityLogFilterToValidation(provider, 10, 10);
			TestActivityLogFilterToValidation(provider, 15, 20);
			TestActivityLogFilterToValidation(provider, 100, 0);
		}

		void TestActivityLogFilterFromValidation(StmActivityLogFilterProvider provider, int numberOfPastYears, int activityLogMaximumPastYears)
		{
			var pastDate = ZDateTime.Today.AddYears(-1 * numberOfPastYears);
			var dateTimeString = pastDate.ToString("dd-MMM-yyyy");
			var currentLimit = ObjectFactory.Get<ISystemDataRegistry>().ActivityLogMaximumPastYears;

			if (currentLimit < numberOfPastYears)
			{
				provider.ActivityLogFilterDateFrom = pastDate;
				var pastDateErrorText = $"The date '{dateTimeString}' is more than {currentLimit} years old and thus is not valid.";
				AssertHasError(provider.ActivityLogFilterDateFromInfo, pastDateErrorText);
			}
			SystemDataRegistryForTest.Get().ActivityLogMaximumPastYears = activityLogMaximumPastYears;
			provider.ActivityLogFilterDateFrom = pastDate;
			AssertNoErrors(provider.ActivityLogFilterDateFromInfo);
		}

		void TestActivityLogFilterToValidation(StmActivityLogFilterProvider provider, int numberOfPastYears, int activityLogMaximumPastYears)
		{
			var pastDate = ZDateTime.Today.AddYears(-1 * numberOfPastYears);
			var dateTimeString = pastDate.ToString("dd-MMM-yyyy");
			var currentLimit = ObjectFactory.Get<ISystemDataRegistry>().ActivityLogMaximumPastYears;

			if (currentLimit < numberOfPastYears)
			{
				provider.ActivityLogFilterDateTo = pastDate;
				var pastDateErrorText = $"The date '{dateTimeString}' is more than {currentLimit} years old and thus is not valid.";
				AssertHasError(provider.ActivityLogFilterDateToInfo, pastDateErrorText);
			}
			SystemDataRegistryForTest.Get().ActivityLogMaximumPastYears = activityLogMaximumPastYears;
			provider.ActivityLogFilterDateTo = pastDate;
			AssertNoErrors(provider.ActivityLogFilterDateToInfo);
		}
	}
}
