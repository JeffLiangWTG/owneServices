using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Portals.GPS;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Portals.GPS.Testing
{
	[TestedType(typeof(UpdateGeoFencePolygonForOrgAddress))]
	sealed class UpdateGeoFencePolygonForOrgAddressTest : DbCreateScriptTest
	{
		const string StaffCode = "007";

		public void TestUpdateGeofenceForSuccess()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string expectedGeofenceShape = "POLYGON ((151.16913622099673 -33.734641483388543, 151.18561571318423 -33.791725187254883, 151.21445482451236 -33.748345039480611, 151.19660204130923 -33.722077967282068, 151.16913622099673 -33.734641483388543))";

			using (var command = Db.Connection.Command("UpdateGeoFencePolygonForOrgAddress"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@Geofencepolygon", SqlDbType.NVarChar, expectedGeofenceShape);
				command.AddParameter("@CurrentUser", SqlDbType.VarChar, StaffCode);
				command.AddOutputParameter("@Output", SqlDbType.Int, 4, 0, 0, DBNull.Value);
				command.ExecuteNonQuery();

				object objOutput = command.GetParameterValue("@Output");
				AssertEquals(1, ((objOutput == DBNull.Value) ? (int?)null : Convert.ToInt32(objOutput)));
			}
		}

		public void TestUpdateGeofenceForNonExistOrganisation()
		{
			var addressNewPK = Guid.NewGuid();

			string geofenceShape = "POLYGON ((80 50, 90 50, 90 25, 80 25, 80 50))";

			using (var command = Db.Connection.Command("UpdateGeoFencePolygonForOrgAddress"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, addressNewPK);
				command.AddParameter("@Geofencepolygon", SqlDbType.NVarChar, geofenceShape);
				command.AddParameter("@CurrentUser", SqlDbType.VarChar, StaffCode);
				command.AddOutputParameter("@Output", SqlDbType.Int, 4, 0, 0, DBNull.Value);
				command.ExecuteNonQuery();

				object objOutput = command.GetParameterValue("@Output");
				AssertEquals(0, ((objOutput == DBNull.Value) ? (int?)null : Convert.ToInt32(objOutput)));
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select OA_GeofencePolygon.ToString() as actual from dbo.OrgAddress where OA_PK = '{0}'", addressNewPK));

			AssertEquals(result.Rows.Count, 0);
		}

		public void TestUpdateGeofenceIfPointsAreClockwiseOrder()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string clockwiseGeofenceShape = "POLYGON((151.16913622099673 -33.73464148338854, 151.19660204130923 -33.72207796728207, 151.21445482451236 -33.74834503948061, 151.18561571318423 -33.79172518725488, 151.16913622099673 -33.73464148338854))";
			string expectedGeofenceShape = "POLYGON ((151.16913622099673 -33.734641483388543, 151.18561571318423 -33.791725187254883, 151.21445482451236 -33.748345039480611, 151.19660204130923 -33.722077967282068, 151.16913622099673 -33.734641483388543))";

			UpdateGeofenceForOrg(addressPK, clockwiseGeofenceShape);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select OA_GeofencePolygon.ToString() as actual from dbo.OrgAddress where OA_PK = '{0}'", addressPK));

			AssertEquals(result.Rows[0]["actual"].ToString(), expectedGeofenceShape);
		}

		public void TestUpdateGeofenceIfPointsAreAntiClockwiseOrder()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string expectedGeofenceShape = "POLYGON ((151.16913622099673 -33.734641483388543, 151.18561571318423 -33.791725187254883, 151.21445482451236 -33.748345039480611, 151.19660204130923 -33.722077967282068, 151.16913622099673 -33.734641483388543))";

			UpdateGeofenceForOrg(addressPK, expectedGeofenceShape);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select OA_GeofencePolygon.ToString() as actual from dbo.OrgAddress where OA_PK = '{0}'", addressPK));

			AssertEquals(result.Rows[0]["actual"].ToString(), expectedGeofenceShape);
		}

		public void TestUpdateGeofenceIfPointsInZigZagOrder()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string geofenceShape = "POLYGON((144.95574835010643 -37.67680246606508,144.9563706225979 -37.67663688039714,144.95636525817986 -37.677817200932886,144.95592537590142 -37.67624202077457,144.95574835010643 -37.67680246606508))";

			using (var command = Db.Connection.Command("UpdateGeoFencePolygonForOrgAddress"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@Geofencepolygon", SqlDbType.NVarChar, geofenceShape);
				command.AddParameter("@CurrentUser", SqlDbType.VarChar, StaffCode);
				command.AddOutputParameter("@Output", SqlDbType.Int, 4, 0, 0, DBNull.Value);
				command.ExecuteNonQuery();

				object objOutput = command.GetParameterValue("@Output");
				AssertEquals(-1, ((objOutput == DBNull.Value) ? (int?)null : Convert.ToInt32(objOutput)));
			}
		}

		public void TestUpdateGeofenceIfPolygonIsInvalid()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string geofenceShape = "POLYGON ((140.27985240253952 -21.0733445037261, 140.28197134766128 -21.07466598962628, 140.27985240253952 -21.0733445037261))";

			using (var command = Db.Connection.Command("UpdateGeoFencePolygonForOrgAddress"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@Geofencepolygon", SqlDbType.NVarChar, geofenceShape);
				command.AddParameter("@CurrentUser", SqlDbType.VarChar, StaffCode);
				command.AddOutputParameter("@Output", SqlDbType.Int, 4, 0, 0, DBNull.Value);
				command.ExecuteNonQuery();

				object objOutput = command.GetParameterValue("@Output");
				AssertEquals(-2, ((objOutput == DBNull.Value) ? (int?)null : Convert.ToInt32(objOutput)));
			}
		}

		public void TestUpdateGeofenceIfPolyginIsEmpty()
		{
			var organisationPK = TestDataCreator.CreateOrganisation("TestOrg", "Test organization", "AUSYD");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "TestCode", "Address1", "Address2", "CityA", "State", "123");
			TestDataCreator.CreateGlbStaff(StaffCode, "Bond");

			string expectedGeofenceShape = "POLYGON EMPTY";

			UpdateGeofenceForOrg(addressPK, expectedGeofenceShape);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select OA_GeofencePolygon.ToString() as actual from dbo.OrgAddress where OA_PK = '{0}'", addressPK));

			AssertEquals(result.Rows[0]["actual"].ToString(), expectedGeofenceShape);
		}

		static void UpdateGeofenceForOrg(Guid orgPK, string geofenceShape)
		{
			using (var command = Db.Connection.Command("UpdateGeoFencePolygonForOrgAddress"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@Geofencepolygon", SqlDbType.NVarChar, geofenceShape);
				command.AddParameter("@CurrentUser", SqlDbType.VarChar, StaffCode);
				command.AddOutputParameter("@Output", SqlDbType.Int, 0, 0, 0, DBNull.Value);

				command.ExecuteNonQuery();
			}
		}
	}
}
