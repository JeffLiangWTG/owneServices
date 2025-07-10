using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetStaffValueForPerson))]
	sealed class GetStaffValueForPersonTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new GetStaffValueForPerson();

		public void TestInvalidPkReturnsEmpty()
		{
			AssertMacroResult(string.Empty, GetMacro("invalidpk"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Could not convert to Guid : invalidpk", Report.ErrorManager.ToString());
		}

		public void TestValidPKFormatButPersonHasPersonButNoStaffReturnsEmpty()
		{
			var person = Create<GlbPerson>();
			Factory.Save();

			AssertMacroResult(string.Empty, GetMacro(person.PK));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Staff not found.", Report.ErrorManager.ToString());
		}

		public void TestValidPKFormatHasPersonAndStaffReturnsPK()
		{
			var person = Create<GlbPerson>();
			var staff = Create<GlbStaff>();
			staff.GS_PER = person.PK;
			Factory.Save();

			AssertMacroResult(staff.PK.ToString(), GetMacro(person.PK));
			AssertEquals(false, Report.ErrorManager.HasErrors);
		}

		public void TestValidPKFormatHasPersonAndStaffReturnsSpecifiedField()
		{
			var person = Create<GlbPerson>();
			var staff = Create<GlbStaff>();
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "the login name";
			Factory.Save();

			AssertMacroResult(staff.GS_LoginName, GetMacro(person.PK, nameof(GlbStaff.GS_LoginName)));
			AssertEquals(false, Report.ErrorManager.HasErrors);
		}

		public void TestValidPKFormatHasPersonAndStaffReturnsEmptyWhenInvalidField()
		{
			var person = Create<GlbPerson>();
			var staff = Create<GlbStaff>();
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "the login name";
			Factory.Save();

			AssertMacroResult(string.Empty, GetMacro(person.PK, "GS_FakeField"));
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Field not found.", Report.ErrorManager.ToString());
		}

		string GetMacro(string pk, string fieldname = null) => string.IsNullOrEmpty(fieldname) ? $"<GetStaffValueForPerson({pk})>" : $"<GetStaffValueForPerson({pk}, {fieldname})>";

		string GetMacro(ZGuid pk, string fieldname = null) => GetMacro(pk.ToString(), fieldname);

		T Create<T>() where T : class
		{
			var entity = Factory.New<T>();
			(entity as BusinessObject)?.FillWithValidTestData();
			return entity;
		}

		void AssertMacroResult(string expectedResult, string macro)
		{
			AssertEquals("Expecting macro to translate", expectedResult, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
		}
	}
}
