using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetPropertyFromEntity))]
	sealed class GetPropertyFromEntityTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new GetPropertyFromEntity();

		public void TestGetStringProperty()
		{
			var holiday = GetHoliday();
			holiday.GA_ApprovalStatus = "XYZ";
			Factory.Save();

			AssertMacroResult("XYZ", GetPropertyFromEntityMacro("GlbStaffHoliday", holiday.PK, "GA_ApprovalStatus"));
		}

		public void TestGetDateProperty()
		{
			var holiday = GetHoliday();
			holiday.GA_EndTime = new ZDateTime(2021, 12, 12, 1, 2, 0);
			Factory.Save();

			AssertMacroResult(new ZDateTime(2021, 12, 12, 1, 2, 0), GetPropertyFromEntityMacro("GlbStaffHoliday", holiday.PK, "GA_EndTime"));
		}

		public void TestGetNumberProperty()
		{
			var holiday = GetHoliday();
			holiday.GA_DaysLeaveTaken = 3;
			Factory.Save();

			AssertMacroResult(new ZDecimal(3.00), GetPropertyFromEntityMacro("GlbStaffHoliday", holiday.PK, "GA_DaysLeaveTaken"));
		}

		public void TestCouldNotLoadType()
		{
			var holiday = GetHoliday();
			holiday.GA_DaysLeaveTaken = 3;
			Factory.Save();

			AssertMacroResult(string.Empty, GetPropertyFromEntityMacro("NotGlbStaffHoliday", holiday.PK, "GA_DaysLeaveTaken"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Could not load type: NotGlbStaffHoliday", Report.ErrorManager.ToString());
		}

		public void TestNotAValidPK()
		{
			AssertMacroResult(string.Empty, GetPropertyFromEntityMacro("GlbStaffHoliday", "notapk", "GA_DaysLeaveTaken"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Could not parse Guid: notapk", Report.ErrorManager.ToString());
		}

		public void TestInvalidPropertyName()
		{
			var holiday = GetHoliday();
			holiday.GA_DaysLeaveTaken = 3;
			Factory.Save();

			AssertMacroResult(string.Empty, GetPropertyFromEntityMacro("GlbStaffHoliday", holiday.PK, "NotGA_DaysLeaveTaken"));
			Assert(Report.ErrorManager.HasErrors);
			AssertStartsWith("Critical Exception Message"
				, "Severity: [Warning (without error report)] Message: [Error in GetPropertyFromEntity Macro: Property could not be found: NotGA_DaysLeaveTaken"
				, Report.ErrorManager.ToString());
		}

		public void TestGetPropertyFromGlbBranch()
		{
			var glbBranch = GlbBranch.CurrentBranch;
			AssertMacroResult(glbBranch.GB_Code, GetPropertyFromEntityMacro("GlbBranch", glbBranch.PK, "GB_Code"));
			AssertMacroResult(glbBranch.GB_Email, GetPropertyFromEntityMacro("GlbBranch", glbBranch.PK, "GB_Email"));
			AssertMacroResult(glbBranch.GB_Email, GetPropertyFromEntityMacro("GlbBranch", "", "GB_Email"));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public void TestGetPropertyFromGlbDepartment()
		{
			var department = GlbDepartment.CurrentDepartment;
			AssertMacroResult(department.GE_Code, GetPropertyFromEntityMacro("GlbDepartment", department.PK, "GE_Code"));
			AssertMacroResult(department.GE_Desc, GetPropertyFromEntityMacro("GlbDepartment", department.PK, "GE_Desc"));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public void TestGetPropertyFromAccGLHeader()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "20.00.000";
			glHeader.AG_AccountType = "P&L";
			Factory.Save();
			AssertMacroResult(glHeader.AG_AccountNum, GetPropertyFromEntityMacro("AccGLHeader", glHeader.PK, "AG_AccountNum"));
			AssertMacroResult(glHeader.AG_AccountType, GetPropertyFromEntityMacro("AccGLHeader", glHeader.PK, "AG_AccountType"));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public void TestGetPropertyWhenNoBizO()
		{
			AssertMacroResult(string.Empty, GetPropertyFromEntityMacro("GlbStaffHoliday", ZGuid.Empty, "GA_ApprovalStatus"));
			AssertStartsWith("Critical Exception Message"
				, $"Severity: [Warning (without error report)] Message: [Error in GetPropertyFromEntity Macro: Could not find GlbStaffHoliday. PK: {ZGuid.Empty}"
				, Report.ErrorManager.ToString());
		}

		GlbStaffHoliday GetHoliday()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			staff.Holidays.Add(holiday);
			return holiday;
		}

		string GetPropertyFromEntityMacro(string entityName, string pk, string property) => $"<GetPropertyFromEntity(\"{entityName}\",\"{pk}\",\"{property}\")>";

		string GetPropertyFromEntityMacro(string entityName, ZGuid pk, string property) => GetPropertyFromEntityMacro(entityName, pk.ToString(), property);

		void AssertMacroResult(object expectedResult, string macro) => AssertEquals("Expecting macro to translate", expectedResult, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
	}
}
