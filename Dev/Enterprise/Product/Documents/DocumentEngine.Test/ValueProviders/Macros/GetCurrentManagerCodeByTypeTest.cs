using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetCurrentManagerCodeByType))]
	sealed class GetCurrentManagerCodeByTypeTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new GetCurrentManagerCodeByType();

		public void TestInvalidPkReturnsEmpty()
		{
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro("invalidpk", "DRM"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Could not convert to Guid : invalidpk", Report.ErrorManager.ToString());
		}

		public void TestValidPKFormatButNotMatchStaffReturnsEmpty()
		{
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro("C28C331A-61DB-49E6-ABE3-CEF07922998E", "DRM"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Staff not found: C28C331A-61DB-49E6-ABE3-CEF07922998E", Report.ErrorManager.ToString());
		}

		public void TestStaffWithNoManagerReturnsEmpty()
		{
			var staff = CreateStaff();
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithDifferentManagerTypeReturnsEmpty()
		{
			var staff = CreateStaff();
			var manager = CreateStaff("New Manager");
			LinkManager(staff, manager, "DRM");
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro(staff.PK, "XRM"));
		}

		public void TestStaffWithCorrectManagerTypeReturnsManagerCode()
		{
			var staff = CreateStaff();
			var manager = CreateStaff("New Manager");
			LinkManager(staff, manager, "DRM");
			AssertMacroResult(manager.GS_Code, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithCorrectManagerTypeButFutureEffectiveDateReturnsManagerCode()
		{
			var staff = CreateStaff();
			var futureEffectiveManager = LinkFutureEffectiveManager(staff, "DRM");
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithCorrectManagerTypeButIsPastManagerReturnsManagerCode()
		{
			var staff = CreateStaff();
			var pastManager = LinkPastManager(staff, "DRM");
			AssertMacroResult(string.Empty, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithCorrectManagerTypeAndHasCurrentManagerReturnsManagerCode()
		{
			var staff = CreateStaff();
			var weekOldCurrentManager = LinkWeekOldCurrentManager(staff, "DRM");
			AssertMacroResult(weekOldCurrentManager.GS_Code, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithCorrectManagerTypeAndHasCurrentManagerAndOtherInvalidManagersReturnsManagerCode()
		{
			var staff = CreateStaff();
			var pastManager = LinkPastManager(staff, "DRM");
			var futureEffectiveManager = LinkFutureEffectiveManager(staff, "DRM");
			var weekOldCurrentManager = LinkWeekOldCurrentManager(staff, "DRM");
			var pastManagerAAAType = LinkPastManager(staff, "AAA");
			var futureEffectiveManagerAAAType = LinkFutureEffectiveManager(staff, "AAA");
			var weekOldCurrentManagerAAAType = LinkWeekOldCurrentManager(staff, "AAA");
			AssertMacroResult(weekOldCurrentManager.GS_Code, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		public void TestStaffWithCorrectManagerTypeAndHasCurrentManagerEndingInAWeekAndOtherInvalidManagersReturnsManagerCode()
		{
			var staff = CreateStaff();
			var pastManager = LinkPastManager(staff, "DRM");
			var futureEffectiveManager = LinkFutureEffectiveManager(staff, "DRM");
			var weekOldWeekEndingCurrentManager = LinkNewManager(staff, "DRM", "Started a week ago, ends a week from now", effectiveDate: DateTime.Now.AddDays(-7), endDate: DateTime.Now.AddDays(7));
			var pastManagerAAAType = LinkPastManager(staff, "AAA");
			var futureEffectiveManagerAAAType = LinkFutureEffectiveManager(staff, "AAA");
			var weekOldCurrentManagerAAAType = LinkWeekOldCurrentManager(staff, "AAA");
			AssertMacroResult(weekOldWeekEndingCurrentManager.GS_Code, GetGetManagerByTypeMacro(staff.PK, "DRM"));
		}

		GlbStaff LinkWeekOldCurrentManager(GlbStaff staff, string type) => LinkNewManager(staff, type, "Week Old Current Manager", effectiveDate: DateTime.Now.AddDays(-7));

		GlbStaff LinkFutureEffectiveManager(GlbStaff staff, string type) => LinkNewManager(staff, type, "Future Effective Manager", effectiveDate: DateTime.Now.AddDays(1));

		GlbStaff LinkPastManager(GlbStaff staff, string type) => LinkNewManager(staff, type, "Past Manager", effectiveDate: DateTime.Now.AddDays(-7), endDate: DateTime.Now.AddDays(-1));

		GlbStaff LinkNewManager(GlbStaff staff, string type, string name, DateTime? effectiveDate = null, DateTime? endDate = null)
		{
			var manager = CreateStaff(name);
			LinkManager(staff, manager, type, effectiveDate: effectiveDate, endDatetime: endDate);
			return manager;
		}

		string GetGetManagerByTypeMacro(string pk, string type) => $"<GetCurrentManagerCodeByType(\"{pk}\",\"{type}\")>";

		string GetGetManagerByTypeMacro(ZGuid pk, string type) => GetGetManagerByTypeMacro(pk.ToString(), type);

		GlbStaff CreateStaff(string name = "New Staff")
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = name;

			Factory.Save();
			return staff;
		}

		GlbStaffManager LinkManager(GlbStaff staff, GlbStaff manager, string managerType, DateTime? effectiveDate = null, DateTime? endDatetime = null)
		{
			var managerLink = Factory.NewWithValidTestData<GlbStaffManager>();
			managerLink.GSM_GS_Manager = manager.PK;
			managerLink.GSM_GS_Staff = staff.PK;
			managerLink.GSM_ManagerType = managerType;
			if (effectiveDate != null)
			{
				managerLink.GSM_EffectiveDate = new ZDateTime(effectiveDate);
			}
			if (endDatetime != null)
			{
				managerLink.GSM_EndDate = new ZDateTime(endDatetime);
			}
			staff.Managers.Add(managerLink);

			Factory.Save();
			return managerLink;
		}

		void AssertMacroResult(string expectedResult, string macro)
		{
			AssertEquals("Expecting macro to translate", expectedResult, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
		}
	}
}
