using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateISFJobBF_JS_Shipment))]
	public class UpdateISFJobBF_JS_ShipmentTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var query = "SELECT count(BF_JS_Shipment) FROM CusISFHeader WHERE BF_JS_Shipment is not null";
			var result = Db.Connection.ExecuteScalar<int>(query);
			AssertEquals("If shipment is HVLV type ISF should keep BF_JS_Shipment", 2, result);

			query = "SELECT count(BF_PK) FROM CusISFHeader JOIN JobShipment ON BF_JS_Shipment = JS_PK WHERE JS_ShipmentType != 'HVL' and BF_JS_Shipment is not null ";
			result = Db.Connection.ExecuteScalar<int>(query);
			AssertEquals("If shipment is Not HVLV type BF_JS_Shipment is null", 0, result);
		}

		Guid s1PK = Guid.NewGuid();
		Guid s2PK = Guid.NewGuid();
		Guid s3PK = Guid.NewGuid();
		Guid s4PK = Guid.NewGuid();
		Guid isf1PK = Guid.NewGuid();
		Guid isf2PK = Guid.NewGuid();
		Guid isf3PK = Guid.NewGuid();
		Guid isf4PK = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var query = @"

			IF NOT EXISTS (SELECT null FROM dbo.GlbCompany WHERE GC_Code = 'UC!')
			BEGIN
				INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES('832CE00E-F95A-4897-87D1-826EFE93D6F8', 'US', 'USD', 'UC!', 'US company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			END
			IF NOT EXISTS(SELECT null FROM dbo.GlbBranch WHERE GB_Code = 'UB!')
			BEGIN
				INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES('1F9C0B82-7C24-4F4F-86B7-37E05B71895E', '832CE00E-F95A-4897-87D1-826EFE93D6F8', 'UB!', 'US', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			END";
			Db.Connection.ExecuteNonQuery(query);

			DeleteTestData();

			InsertJobShipment(s1PK, "S300055881", "HVL", "SEA");
			InsertJobShipment(s2PK, "S300055882", "HVL", "AIR");
			InsertJobShipment(s3PK, "S300055883", "STD", "SEA");
			InsertJobShipment(s4PK, "S300055884", "STD", "AIR");

			InsertCusISFHeader(isf1PK, "ISFTST001", s1PK);
			InsertCusISFHeader(isf2PK, "ISFTST002", s2PK);
			InsertCusISFHeader(isf3PK, "ISFTST003", s3PK);
			InsertCusISFHeader(isf4PK, "ISFTST004", s4PK);
		}

		void DeleteTestData()
		{
			var query = @"
				DELETE JobShipment WHERE JS_PK in (@s1PK, @s2PK, @s3PK, @s4PK)
				DELETE CusISFHeader WHERE BF_PK in (@isf1PK, @isf2PK, @isf3PK, @isf4PK)";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@s1PK", SqlDbType.UniqueIdentifier, s1PK);
				command.AddParameter("@s2PK", SqlDbType.UniqueIdentifier, s2PK);
				command.AddParameter("@s3PK", SqlDbType.UniqueIdentifier, s3PK);
				command.AddParameter("@s4PK", SqlDbType.UniqueIdentifier, s4PK);
				command.AddParameter("@isf1PK", SqlDbType.UniqueIdentifier, isf1PK);
				command.AddParameter("@isf2PK", SqlDbType.UniqueIdentifier, isf2PK);
				command.AddParameter("@isf3PK", SqlDbType.UniqueIdentifier, isf3PK);
				command.AddParameter("@isf4PK", SqlDbType.UniqueIdentifier, isf4PK);
				command.ExecuteNonQuery();
			}
		}

		void InsertJobShipment(Guid pk, string uniqueRef, string shipmentType, string transportMode)
		{
			var query = @"INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_ShipmentType, JS_TransportMode, JS_ShippedOnBoard, JS_IsForwardRegistered, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES(@pk, @uniqueRef, @shipmentType, @transportMode, 'SHP', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, uniqueRef);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobShipmentSchema.JS_TransportMode.MaxLength, transportMode);
				command.AddParameter("@shipmentType", SqlDbType.VarChar, JobShipmentSchema.JS_ShipmentType.MaxLength, shipmentType);
				command.ExecuteNonQuery();
			}
		}

		void InsertCusISFHeader(Guid pk, string uniqueRef, Guid shipmentPK)
		{
			Guid branchPK = Guid.Parse("1F9C0B82-7C24-4F4F-86B7-37E05B71895E");

			var query = @"
INSERT INTO [dbo].[CusISFHeader] ([BF_PK], [BF_JobReference], [BF_JS_Shipment], [BF_GB], [BF_SystemCreateTimeUtc], [BF_SystemCreateUser], [BF_SystemLastEditTimeUtc], [BF_SystemLastEditUser])
VALUES (@pk, @uniqueRef, @shipmentPK,@branchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, CusISFHeaderSchema.BF_JobReference.MaxLength, uniqueRef);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.ExecuteNonQuery();
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateISFJobBF_JS_Shipment();
	}
}
