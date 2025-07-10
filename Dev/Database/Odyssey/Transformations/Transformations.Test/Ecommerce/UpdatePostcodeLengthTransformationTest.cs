using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(UpdatePostcodeLengthTransformation))]
	public class UpdatePostcodeLengthTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var tableColumnData = new string[][]
			{
				new string[] { CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_BuyerPostcode },
				new string[] { CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_SellerPostcode },
				new string[] { HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode },
				new string[] { HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode },
				new string[] { HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ReturnPostcode },
				new string[] { "HVLVDeliveryByArea", "HDB_ConsigneePostcode" }
			};

			foreach (var item in tableColumnData)
			{
				var tableName = "dbo." + item[0];
				var columnName = item[1];
				AssertColumnData(tableName, columnName);
			}
		}

		void AssertColumnData(string tableName, string columnName)
		{
			var selectSql = $"SELECT {columnName} FROM {tableName}";
			var selectResults = new List<string>();

			Db.Connection.ExecuteReader(
				selectSql,
				action: record =>
				{
					selectResults.Add((string)record[$"{columnName}"]);
				});

			selectResults.Sort();

			AssertEquals($"There should be 2 test samples from column '{columnName}' in table '{tableName}'", 2, selectResults.Count);
			AssertEquals($"Retrieved data from column '{columnName}' in table '{tableName}' should be truncated with max length 10", "123456789", selectResults[0]);
			AssertEquals($"Retrieved data from column '{columnName}' in table '{tableName}' should be truncated with max length 10", "1234567890", selectResults[1]);
			Assert($"Magic column 'CW!!Long_Postcode_Flag' should be deleted in table '{tableName}'", !DbObjectCreator.ColumnExists(Db.Connection, tableName, "CW!!Long_Postcode_Flag"));
			Assert($"Magic index '_WTG_Long_Postcode_Flag' should be deleted in table '{tableName}'", !DbObjectCreator.IndexExists(Db.Connection, tableName, "_WTG_Long_Postcode_Flag"));
		}

		void InsertJobShipment(Guid pk, string uniqueRef, string shipmentType, string transportMode)
		{
			var query = @"
INSERT INTO dbo.JobShipment
(JS_PK, JS_UniqueConsignRef, JS_ShipmentType, JS_TransportMode, JS_ShippedOnBoard, JS_IsForwardRegistered, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES
(@pk, @uniqueRef, @shipmentType, @transportMode, 'SHP', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, uniqueRef);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobShipmentSchema.JS_TransportMode.MaxLength, transportMode);
				command.AddParameter("@shipmentType", SqlDbType.VarChar, JobShipmentSchema.JS_ShipmentType.MaxLength, shipmentType);
				command.ExecuteNonQuery();
			}
		}

		void InsertCusISFHeader(Guid pk, string uniqueRef, Guid shipmentPK, string bf_BuyerPostcode, string bf_SellerPostcode)
		{
			Guid branchPK = Guid.Parse("1F9C0B82-7C24-4F4F-86B7-37E05B71895E");

			var query = @"
INSERT INTO dbo.CusISFHeader
(BF_PK, BF_JobReference, BF_JS_Shipment, BF_GB, BF_BuyerPostcode, BF_SellerPostcode, BF_SystemCreateTimeUtc, BF_SystemCreateUser, BF_SystemLastEditTimeUtc, BF_SystemLastEditUser)
VALUES
(@pk, @uniqueRef, @shipmentPK, @branchPK, @BF_BuyerPostcode, @BF_SellerPostcode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, CusISFHeaderSchema.BF_JobReference.MaxLength, uniqueRef);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@BF_BuyerPostcode", SqlDbType.NVarChar, bf_BuyerPostcode);
				command.AddParameter("@BF_SellerPostcode", SqlDbType.NVarChar, bf_SellerPostcode);
				command.ExecuteNonQuery();
			}
		}

		void InsertCusISFHeaderTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusISFHeaderSchema.Constants.TableName, "DF_CusISFHeader_BF_BuyerPostcode");
			DBTransformationTestHelper.DropConstraintIfExists(CusISFHeaderSchema.Constants.TableName, "DF_CusISFHeader_BF_SellerPostcode");

			DBTransformationTestHelper.DropColumnIfExists(CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_BuyerPostcode);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_BuyerPostcode, "nvarchar(20)");
			DBTransformationTestHelper.DropColumnIfExists(CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_SellerPostcode);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_SellerPostcode, "nvarchar(20)");

			var preInsertSql = @"
IF NOT EXISTS (SELECT null FROM dbo.GlbCompany WHERE GC_Code = 'UC!')
BEGIN
	INSERT INTO dbo.GlbCompany
    (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
    VALUES
    ('832CE00E-F95A-4897-87D1-826EFE93D6F8', 'US', 'USD', 'UC!', 'US company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
END
IF NOT EXISTS(SELECT null FROM dbo.GlbBranch WHERE GB_Code = 'UB!')
BEGIN
	INSERT INTO dbo.GlbBranch
    (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
    VALUES
    ('1F9C0B82-7C24-4F4F-86B7-37E05B71895E', '832CE00E-F95A-4897-87D1-826EFE93D6F8', 'UB!', 'US', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
END";
			Db.Connection.ExecuteNonQuery(preInsertSql);

			var s1PK = Guid.NewGuid();
			var isf1PK = Guid.NewGuid();
			InsertJobShipment(s1PK, "S300055881", "HVL", "SEA");
			InsertCusISFHeader(isf1PK, "ISFTST001", s1PK, "123456789", "123456789");

			var s2PK = Guid.NewGuid();
			var isf2PK = Guid.NewGuid();
			InsertJobShipment(s2PK, "S300055882", "HVL", "AIR");
			InsertCusISFHeader(isf2PK, "ISFTST002", s2PK, "1234567890987654321", "1234567890987654321");
		}

		void InsertHVLVConsignmentTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(HVLVConsignmentSchema.Constants.TableName, "DF_HVLVConsignment_HVC_ConsigneePostcode");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVConsignmentSchema.Constants.TableName, "DF_HVLVConsignment_HVC_ShipperPostcode");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVConsignmentSchema.Constants.TableName, "DF_HVLVConsignment_HVC_ReturnPostcode");

			DBTransformationTestHelper.DropColumnIfExists(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode, "nvarchar(20)");
			DBTransformationTestHelper.DropColumnIfExists(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode, "varchar(20)");
			DBTransformationTestHelper.DropColumnIfExists(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ReturnPostcode);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ReturnPostcode, "nvarchar(20)");

			using (DataTransformationHelper.SuspendFkIfExists(HVLVConsignmentSchema.Constants.SqlSchemaName, HVLVConsignmentSchema.Constants.TableName, "HVLVConsignment_HVC_HVH_BookingHeader_FK2_HVLVBookingHeader_RRR_120N"))
			{
				testDataCreator = testDataCreator ?? new TransformationTestDataCreator();
				var headerPK = new Guid();
				testDataCreator.CreateHVLVConsignment(headerPK, reference: "", consignmentId: "Consignment1", user: "~BP", itemCount: 1, clusterKey: 100,
					consigneePostcode: "123456789", shipperPostcode: "123456789", returnPostcode: "123456789");
				testDataCreator.CreateHVLVConsignment(headerPK, reference: "", consignmentId: "Consignment1", user: "~BP", itemCount: 1, clusterKey: 100,
					consigneePostcode: "1234567890987654321", shipperPostcode: "1234567890987654321", returnPostcode: "1234567890987654321");
			}
		}

		void InsertHVLVDeliveryByAreaTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists("HVLVDeliveryByArea", "DF_HVLVDeliveryByArea_HDB_ConsigneePostcode");

			DBTransformationTestHelper.DropColumnIfExists("HVLVDeliveryByArea", "HDB_ConsigneePostcode");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "HVLVDeliveryByArea", "HDB_ConsigneePostcode", "nvarchar(20)");

			testDataCreator = testDataCreator ?? new TransformationTestDataCreator();
			testDataCreator.CreateHVLVHVLVDeliveryByArea(consigneePostcode: "123456789");
			testDataCreator.CreateHVLVHVLVDeliveryByArea(consigneePostcode: "1234567890987654321");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePostcodeLengthTransformation();

		TransformationTestDataCreator testDataCreator;

		protected override void PrepareTestData()
		{
			InsertCusISFHeaderTestData();
			InsertHVLVConsignmentTestData();
			InsertHVLVDeliveryByAreaTestData();
		}
	}
}
