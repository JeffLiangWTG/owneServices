using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	class GetOrgPKsNearTheGeoLocationIntegrationTest : TransactionedTestCase
	{
		public void TestGetOrgPKsNearTheGeoLocation_OrgClosestPort()
		{
			var unloco = CreateDummyUNLOCO();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsGlobalAccount = true;
			orgHeader.OH_RL_NKClosestPort = unloco.RL_Code;
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "";
			Factory.Save();
			InsertGenSpatialData(RefUNLOCOSchema.Constants.Prefix, unloco.PK, RefUNLOCOSchema.RL_GeoLocation.Name);

			AssertNotEquals(orgHeader.OH_RL_NKClosestPort.ToString(), Db.Connection.ExecuteScalar($"SELECT OA_RL_NKRelatedPortCode FROM dbo.OrgAddress WITH(NOLOCK) WHERE OA_PK = '{orgHeader.MainAddress.PK}'").ToString());
			AssertFindResult(orgHeader.PK.ToGuid());
		}

		public void TestGetOrgPKsNearTheGeoLocation_OrgAddressClosestPort()
		{
			var unloco = CreateDummyUNLOCO();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			address.OA_RL_NKRelatedPortCode = unloco.RL_Code;
			Factory.Save();
			InsertGenSpatialData(RefUNLOCOSchema.Constants.Prefix, unloco.PK, RefUNLOCOSchema.RL_GeoLocation.Name);

			AssertNotEquals(orgHeader.OH_RL_NKClosestPort, address.OA_RL_NKRelatedPortCode);
			AssertFindResult(orgHeader.PK.ToGuid());
		}

		public void TestGetOrgPKsNearTheGeoLocation_OrgAddressClosestPort_OrgAddressIsNotActive()
		{
			var unloco = CreateDummyUNLOCO();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			address.OA_RL_NKRelatedPortCode = unloco.RL_Code;
			address.OA_IsActive = false;
			Factory.Save();
			InsertGenSpatialData(RefUNLOCOSchema.Constants.Prefix, unloco.PK, RefUNLOCOSchema.RL_GeoLocation.Name);

			AssertNotEquals(orgHeader.OH_RL_NKClosestPort, address.OA_RL_NKRelatedPortCode);
			AssertFindResult(false, orgHeader.PK.ToGuid(), 2);
		}

		public void TestGetOrgPKsNearTheGeoLocation_OrgAddressGeoLocation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			address.OA_GeoLocation = new ZGeography("POINT (1 1)");
			Factory.Save();
			InsertGenSpatialData(OrgAddressSchema.Constants.Prefix, address.PK, OrgAddressSchema.OA_GeoLocation.Name);

			AssertFindResult(orgHeader.PK.ToGuid());
		}

		public void TestGetOrgPKsNearTheGeoLocation_OrgAddressGeoLocation_OrgAddressIsNotActive()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			address.OA_GeoLocation = new ZGeography("POINT (1 1)");
			address.OA_IsActive = false;
			Factory.Save();
			InsertGenSpatialData(OrgAddressSchema.Constants.Prefix, address.PK, OrgAddressSchema.OA_GeoLocation.Name);

			AssertFindResult(false, orgHeader.PK.ToGuid(), 2);
		}

		void AssertFindResult(Guid orgPK)
		{
			CombineAssertions(() =>
			{
				AssertFindResult(false, orgPK);
				AssertFindResult(true, orgPK);
			});
		}

		void AssertFindResult(bool shouldFind, Guid orgPK, int distanceWhenShouldFindIsFalse = 1)
		{
			var command = Db.Connection.Command("SELECT OH_PK FROM GetOrgPKsNearTheGeoLocation(@geoLocation, @distance)");
			command.AddParameter("@geoLocation", System.Data.SqlDbType.VarChar, new ZGeography("POINT (1.00001 1.00001)").AsText());
			command.AddParameter("@distance", System.Data.SqlDbType.Float, shouldFind ? 2 : distanceWhenShouldFindIsFalse);
			using (var reader = command.ExecuteReader())
			{
				var foundTheOrg = false;
				while (reader.Read())
				{
					var pk = reader.GetGuid(0);
					if (pk == orgPK)
					{
						foundTheOrg = true;
						break;
					}
				}

				AssertEquals(shouldFind, foundTheOrg);
			}
		}

		void InsertGenSpatialData(string tableCode, ZGuid parentID, string column)
		{
			Db.Connection.ExecuteNonQuery($@"IF NOT EXISTS (SELECT NULL FROM dbo.GenSpatialData WHERE SPD_ParentID = '{parentID}')
BEGIN
	INSERT INTO dbo.GenSpatialData (SPD_ParentTableCode, SPD_ParentID, SPD_Column, SPD_Geography) VALUES ('{tableCode}', '{parentID}', '{column}', geography::STGeomFromText('POINT (1 1)', 4326))
END");
		}

		RefUNLOCO CreateDummyUNLOCO()
		{
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = "AU";
			unloco.RL_GeoLocation = new ZGeography("POINT (1 1)");
			unloco.RL_Code = "AUDUM";

			return unloco;
		}

		BusinessObjectFactory _factory;
		BusinessObjectFactory Factory => _factory ?? (_factory = new BusinessObjectFactory());
	}
}

