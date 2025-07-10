using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Misc.Testing
{
	[TestedType(typeof(SplitStringToGuidTable))]
	internal class SplitStringToGuidTableEDWTest : BiCreateScriptTest
	{
		public void TestUnquoteRemovesExtraChars()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "select value from [{0}].[dbo].[SplitStringToGuidTable]('`BDEE100E-2282-4CA1-A272-DCD86438C816`,CDEE100E-2282-4CA1-A272-DCD86438C816,',',','`')", ScriptDbName);

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("reading first row in the table", true, reader.Read());
				AssertEquals(Guid.Parse("bdee100e-2282-4ca1-a272-dcd86438c816"), reader[0]);

				AssertEquals("reading second row in the table", true, reader.Read());
				AssertEquals(Guid.Parse("cdee100e-2282-4ca1-a272-dcd86438c816"), reader[0]);

				AssertEquals("there should be no third row in the table", false, reader.Read()); //make sure empty string is not converted to anything
			}
		}

		public void TestSplitSingleElementString()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT value FROM [{0}].[dbo].[SplitStringToGuidTable]('AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE', ',', DEFAULT)", ScriptDbName);

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("First row", new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"), reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestElementsWithMoreOrLessThan36CharactersAreIgnored()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT value FROM [{0}].[dbo].[SplitStringToGuidTable]('123,12345678-1234-1234-1234-123456789012,,1234567890123456789012345678901234567,FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF,,', ',', DEFAULT)", ScriptDbName);

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("First row", new Guid("12345678-1234-1234-1234-123456789012"), reader[0]);
				AssertEquals("Reading second row in the table", true, reader.Read());
				AssertEquals("Second row", new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestSplitWithInvalidGuid()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT value FROM [{0}].[dbo].[SplitStringToGuidTable]('11111111-1111-1111-1111-111111111111,ZZZZZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZZZZZZZZZ', ',', DEFAULT)", ScriptDbName);

			AssertExceptionThrown<SqlException>(
				"Attempt to run SplitStringToGuidTable with an invalid Guid value",
				"Conversion failed when converting from a character string to uniqueidentifier.",
				() => TestConnection.ExecuteReader(query, (r) => { }));
		}

		public void TestSplitEmptyString()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT value FROM [{0}].[dbo].[SplitStringToGuidTable]('', DEFAULT, DEFAULT)", ScriptDbName);

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("There should be no rows in the table", false, reader.Read());
			}
		}

		public void TestSplitNullString()
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT value FROM [{0}].[dbo].[SplitStringToGuidTable](null, DEFAULT, DEFAULT)", ScriptDbName);

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("There should be no rows in the table", false, reader.Read());
			}
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
