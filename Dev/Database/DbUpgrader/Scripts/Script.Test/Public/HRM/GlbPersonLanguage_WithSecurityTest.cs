using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Testing.Public.HRM.HRMDataHelpers;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlbPersonLanguage_WithSecurity))]
	class GlbPersonLanguage_WithSecurityTest : DbCreateScriptTest
	{
		readonly IEqualityComparer<DataRow> comparator = DataRowComparer.Default;

		public void TestEmptyStaff()
		{
			var rows = RunLanguagesQuery(Guid.Empty).Select();
			AssertEquals(0, rows.Length);
		}

		public void TestNoLanguages()
		{
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("JTK", "Jim Kirk");
			var rows = RunLanguagesQuery(loggedInStaffPK).Select();
			AssertEquals(0, rows.Length);
		}

		public void TestOneLanguage_DifferentStaff()
		{
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("JTK", "Jim Kirk");
			var personPK = TestDataCreator.CreateGlbPerson("Mr Spock");
			_ = TestDataCreator.CreateGlbStaff("SPK", "Mr Spock", personPK: personPK);

			var languagePK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, languagePK, personPK, "EN");

			var expected = new[]
			{
				(languagePK, personPK, "EN", false),
			};

			var rows = RunLanguagesQuery(loggedInStaffPK).Select();
			AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), rows);
		}

		public void TestMultipleLanguages_MatchingStaff()
		{
			var loggedInPersonPK = TestDataCreator.CreateGlbPerson("Jim Kirk");
			var personPK = TestDataCreator.CreateGlbPerson("Mr Spock");

			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("JTK", "Jim Kirk", personPK: loggedInPersonPK);
			_ = TestDataCreator.CreateGlbStaff("SPK", "Mr Spock", personPK: personPK);

			var englishPK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, englishPK, loggedInPersonPK, "EN");

			var dutchPK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, dutchPK, personPK, "NL");

			var arabicPK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, arabicPK, loggedInPersonPK, "AR");

			var expected = new[]
			{
				(englishPK, loggedInPersonPK, "EN", true),
				(dutchPK, personPK, "NL", false),
				(arabicPK, loggedInPersonPK, "AR", true),
			};

			var rows = RunLanguagesQuery(loggedInStaffPK).Select();
			AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), rows);
		}

		public void TestMultipleLanguages_NoMatchingStaff()
		{
			var loggedInPersonPK = TestDataCreator.CreateGlbPerson("Jim Kirk");
			var person1PK = TestDataCreator.CreateGlbPerson("Mr Spock");
			var person2PK = TestDataCreator.CreateGlbPerson("Leonard McCoy");

			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("JTK", "Jim Kirk", personPK: loggedInPersonPK);
			_ = TestDataCreator.CreateGlbStaff("SPK", "Mr Spock", personPK: person1PK);
			_ = TestDataCreator.CreateGlbStaff("LMC", "Leonard McCoy", personPK: person2PK);

			var englishPK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, englishPK, person2PK, "EN");

			var dutchPK = Guid.NewGuid();
			CreateGlbPersonLanguage(TestConnection, dutchPK, person1PK, "NL");

			var expected = new[]
			{
				(englishPK, person2PK, "EN", false),
				(dutchPK, person1PK, "NL", false),
			};

			var rows = RunLanguagesQuery(loggedInStaffPK).Select();
			AssertContainsExactElementsInAnyOrder(comparator, SerialiseExpected(expected), rows);
		}

		EnumerableRowCollection<DataRow> SerialiseExpected(IEnumerable<(Guid pk, Guid person, string language, bool isSelf)> records)
		{
			var expected = new DataTable();
			expected.Columns.Add(new DataColumn("G7_PK", typeof(Guid)));
			expected.Columns.Add(new DataColumn("G7_PER_Person", typeof(Guid)));
			expected.Columns.Add(new DataColumn("G7_Language", typeof(string)));
			expected.Columns.Add(new DataColumn("IsSelf", typeof(bool)));

			foreach (var (pk, staff, language, isSelf) in records)
			{
				var row = expected.NewRow();
				row["G7_PK"] = pk;
				row["G7_PER_Person"] = staff;
				row["G7_Language"] = language;
				row["IsSelf"] = isSelf;
				expected.Rows.Add(row);
			}

			return expected.AsEnumerable();
		}

		DataTable RunLanguagesQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command($"SELECT G7_PK, G7_PER_Person, G7_Language, IsSelf FROM GlbPersonLanguage_WithSecurity(@loggedInStaff)"))
			{
				command.AddParameterBasedOnDbColumn("@loggedInStaff", loggedInStaff, GlbStaffSchema.PK);
				return DataUtils.GetDataTableFromCommand(command);
			}
		}
	}
}
