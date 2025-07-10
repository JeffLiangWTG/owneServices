using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEADPScoring))]
	public class UPEADPScoringTest : EnterpriseBusinessObjectTestCase
	{
		#region Business Object Overrides
		[TestDate(2006, 5, 1)]
		public void TestT4_IsStaffWorkingDay_ForWorkingDay()
		{
			SetCurrentUserWorkingHoursAndSave();
			AssertEquals("Is a working day for the test", true, GlbStaff.CurrentUser.IsWorkingToday);
			ADPScoring.Factory.Save();
			AssertEquals("T4_IsStaffWorkingDay should set to true on a weekday (work day)", true, ADPScoring.T4_IsStaffWorkingDay);
		}

		[TestDate(2006, 5, 6)]
		public void TestT4_IsStaffWorkingDay_ForNonWorkingDay()
		{
			SetCurrentUserWorkingHoursAndSave();
			AssertEquals("Not a working day for the test", false, GlbStaff.CurrentUser.IsWorkingToday);
			ADPScoring.Factory.Save();
			AssertEquals("T4_IsStaffWorkingDay should set to false on a weekend (non-work day)", false, ADPScoring.T4_IsStaffWorkingDay);
		}

		void SetCurrentUserWorkingHoursAndSave()
		{
			GlbStaff currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.WorkTimes.MondayWorkingHours = "******************";
			Factory.Save();
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		UPEADPScoring ADPScoring
		{
			get
			{
				return new UPEADPScoring.Loader(Factory).LoadOrCreate();
			}
		}
		#endregion
	}
}
