using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol.Testing
{
	[TestedType(typeof(GetJobConsolsWithSecurityContext))]
	class GetJobConsolsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobConsolColumns()
		{
			var fromJobConsol = GetColumnNames("SELECT * FROM dbo.JobConsol");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobConsolsWithSecurityContext('')");

			var missingColumns = fromJobConsol.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestConsolsWithAllowedAddresses()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var consol1 = CreateConsolWithAddress(address1PK);
			var consol2 = CreateConsolWithAddress(address2PK);
			CreateConsolWithAddress(address3PK);

			var relatedAddressesList = new[] { address1PK, address2PK };
			var consols = GetJobConsolsWithSecurityContext(relatedAddressesList: relatedAddressesList);
			Assert(consols.Contains(consol1));
			Assert(consols.Contains(consol2));
			AssertEquals(2, consols.Count());
		}

		Guid CreateConsolWithAddress(Guid addressPK)
		{
			var consolPK = TestDataCreator.CreateJobConsol($"Consol {++consolCounter}");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobConsol SET JK_OA_UnpackDepotAddress='{addressPK}', JK_SystemLastEditTimeUtc = GETUTCDATE(), JK_SystemLastEditUser = '~BP' WHERE JK_PK='{consolPK}'");

			return consolPK;
		}

		IEnumerable<Guid> GetJobConsolsWithSecurityContext(
			IEnumerable<Guid> relatedAddressesList = null)
		{
			var relatedAddresses = relatedAddressesList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();

			using (var command = TestConnection.Command("SELECT * FROM GetJobConsolsWithSecurityContext(@addressesListForConsols)"))
			{
				command.AddParameter("@addressesListForConsols", SqlDbType.NVarChar, string.Join(",", relatedAddresses));

				var data = DataUtils.GetDataTableFromCommand(command);

				return data.AsEnumerable().Select(row => (Guid)row[JobConsolSchema.Constants.PK]);
			}
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader
					.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			consolCounter = 0;
		}

		int consolCounter;
	}
}
