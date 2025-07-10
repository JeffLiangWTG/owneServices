using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.HR.Test
{
	public class WorkingStaffTest : TestCaseWithFactory
	{
		public void TestWorkTime()
		{
			var staff38 = CreateStaff("AAA");
			SetWtg38HourWeekEightThirtyStart(staff38.WorkTimes);
			var staff40 = CreateStaff("BBB");
			Set40HourWeek9to5(staff40.WorkTimes);
			Factory.Save();
			var mon = new DateTime(2017, 10, 2);
			var tue = new DateTime(2017, 10, 3);
			var sat = new DateTime(2017, 10, 7);
			AssertEquals("Pre:", DayOfWeek.Monday, mon.DayOfWeek);
			AssertEquals("Pre:", DayOfWeek.Tuesday, tue.DayOfWeek);
			AssertEquals("Pre:", DayOfWeek.Saturday, sat.DayOfWeek);
			var workPattern38 = new WorkingStaff(staff38);
			var workPattern40 = new WorkingStaff(staff40);
			AssertEquals("38 Mon start", mon.AddHours(8.5), workPattern38.StartWorkTime(mon));
			AssertEquals("38 Mon end", mon.AddHours(16.5), workPattern38.EndWorkTime(mon));
			AssertEquals("38 Tue start", tue.AddHours(8.5), workPattern38.StartWorkTime(tue));
			AssertEquals("38 Tue end", tue.AddHours(17.5), workPattern38.EndWorkTime(tue));
			AssertEquals("38 Sat start", sat, workPattern38.StartWorkTime(sat));
			AssertEquals("38 Sat end", sat.AddMinutes(23 * 60 + 59), workPattern38.EndWorkTime(sat));
			AssertEquals("40 Mon start", mon.AddHours(9), workPattern40.StartWorkTime(mon));
			AssertEquals("40 Mon end", mon.AddHours(17), workPattern40.EndWorkTime(mon));
			AssertEquals("40 Tue start", tue.AddHours(9), workPattern40.StartWorkTime(tue));
			AssertEquals("40 Tue end", tue.AddHours(17), workPattern40.EndWorkTime(tue));
			AssertEquals("40 Sat start", sat, workPattern40.StartWorkTime(sat));
			AssertEquals("40 Sat end", sat.AddMinutes(23 * 60 + 59), workPattern40.EndWorkTime(sat));
		}

		public void TestTotalWorkingHours()
		{
			var staff38 = CreateStaff("AAA");
			SetWtg38HourWeekEightThirtyStart(staff38.WorkTimes);
			var workPattern38 = new WorkingStaff(staff38);
			AssertEquals("mon", 7m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 21)));
			AssertEquals("tue", 8m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 22)));
			AssertEquals("wed", 8m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 23)));
			AssertEquals("thu", 8m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 24)));
			AssertEquals("fri", 7m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 25)));
			AssertEquals("sat", 0m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 26)));
			AssertEquals("sun", 0m, workPattern38.TotalWorkingHours(new DateTime(2018, 5, 27)));
		}

		EDIGlbStaff CreateStaff(string code)
		{
			return CreateStaff(Factory, code, null, null);
		}

		public static EDIGlbStaff CreateStaff(BusinessObjectFactory factory, string code, string certificateType, string certificateNumber)
		{
			var staff = factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = code;
			staff.GS_FullName = code + "First " + code + "Last";
			if (!string.IsNullOrEmpty(certificateType))
			{
				var cert = staff.Certificates.AddNew();
				cert.XZ_Type = certificateType;
				cert.XZ_RN_NKCountryOfIssuance = "AU";
				cert.XZ_RefNumber = certificateNumber;
			}

			var work = staff.WorkTimes;
			Set40HourWeek9to5(work);
			return staff;
		}

		public static void Set40HourWeek9to5(GlbWorkTimeCollection work)
		{
			work.MondayWorkingHours = "                  ****************";
			work.TuesdayWorkingHours = "                  ****************";
			work.WednesdayWorkingHours = "                  ****************";
			work.ThursdayWorkingHours = "                  ****************";
			work.FridayWorkingHours = "                  ****************";
			work.SaturdayWorkingHours = "";
			work.SundayWorkingHours = "";
		}

		public static void SetWtg38HourWeekEightThirtyStart(GlbWorkTimeCollection work)
		{
			work.MondayWorkingHours = "                 ******  ********";
			work.TuesdayWorkingHours = "                 ******  **********";
			work.WednesdayWorkingHours = "                 ******  **********";
			work.ThursdayWorkingHours = "                 ******  **********";
			work.FridayWorkingHours = "                 ******  ********";
			work.SaturdayWorkingHours = "";
			work.SundayWorkingHours = "";
		}
	}
}
