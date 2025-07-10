using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(GetStaffWithSecurityContext))]
	class GetStaffWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestIsSelf()
		{
			var loggedInStaff = Guid.NewGuid();
			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{loggedInStaff}', 'foo', 'foo', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES (NEWID(), 'bar', 'bar', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			var actual = results
				.Select()
				.Select(x => $"{x[GlbStaffSchema.GS_Code.Name]}|{x["IsSelf"]}");
			AssertContainsExactElementsInAnyOrder(new[] { "foo|True", "bar|False" }, actual);
		}

		public void TestReturnsGlbStaffColumns()
		{
			var fromGlbStaff = GetColumNames("SELECT * FROM dbo.GlbStaff");
			var fromTvf = GetColumNames("SELECT * FROM GetStaffWithSecurityContext(NEWID())");

			var missingColumns = fromGlbStaff.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";
			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGeoLocationFieldIsConvertedToString()
		{
			var loggedInStaff = Guid.NewGuid();
			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_GeoLocation, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{loggedInStaff}', 'foo', 'foo', 0xE6100000010CC89299AE82EA42C089F6CA1F001F6240, GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			var actual = results
				.Select()
				.Select(x => $"{x["GS_GeoLocation"]}");
			AssertContainsExactElementsInAnyOrder(new[] { "POINT (144.96876516 -37.8321130990858)" }, actual);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(GlbStaffSchema.Constants.TableName);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		DataTable Execute(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM GetStaffWithSecurityContext(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		IEnumerable<string> GetColumNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}
	}
}
