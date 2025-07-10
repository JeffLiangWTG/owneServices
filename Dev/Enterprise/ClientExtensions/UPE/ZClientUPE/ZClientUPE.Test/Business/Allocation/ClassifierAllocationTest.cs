using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClassifierAllocation))]
	public class ClassifierAllocationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllocatedTo_ToFullName()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AA";
			staff.GS_FullName = "AAA";
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BB";
			staff.GS_FullName = "BBB";
			ClassifierAllocation.AllocatedTo = "AA";
			AssertEquals("AAA", ClassifierAllocation.AllocatedToFullName);
			ClassifierAllocation.AllocatedTo = "BB";
			AssertEquals("BBB", ClassifierAllocation.AllocatedToFullName);
		}

		[TestDate(2006, 3, 15)]
		public void TestNextWorkingDay()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AA";
			AssertEquals(ZDateTime.Empty, ClassifierAllocation.NextWorkingDay);
			GlbStaffHoliday holiday = staff.Holidays.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 16);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 17);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";
			AssertEquals(ZDateTime.Empty, ClassifierAllocation.NextWorkingDay);
			holiday = staff.Holidays.AddNew();
			holiday.GA_StartTime = new ZDateTime(2006, 3, 15);
			holiday.GA_EndTime = new ZDateTime(2006, 3, 16);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";
			ClassifierAllocation.AllocatedTo = "AA";
			AssertEquals(new ZDateTime(2006, 3, 17), ClassifierAllocation.NextWorkingDay);
		}

		ClassifierAllocation ClassifierAllocation
		{
			get
			{
				if (fClassifierAllocation == null)
				{
					fClassifierAllocation = new ClassifierAllocation(Factory);
				}

				return fClassifierAllocation;
			}
		}

		ClassifierAllocation fClassifierAllocation;
	}
}
