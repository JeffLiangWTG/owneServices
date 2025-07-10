using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.Public.Glow.TestHelpers
{
	internal class ConsignmentBookingGeneratorForTests
	{
		readonly DbConnection connection;

		public ConsignmentBookingGeneratorForTests(DbConnection connection)
		{
			this.connection = connection;
		}

		public Guid GenerateAddress()
		{
			var orgPK = GenerateOrganisation();
			var addressPK = Guid.NewGuid();

			var query = string.Format(CultureInfo.InvariantCulture,
				"INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1)  VALUES ('{0}', '{1}', '{2}', '{3}')",
				addressPK,
				orgPK,
				"DDP",
				10.CharactersString());
			connection.ExecuteNonQuery(query);

			return addressPK;
		}

		public Guid GenerateOrganisation()
		{
			var pk = Guid.NewGuid();

			var query = string.Format(CultureInfo.InvariantCulture,
				"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsShippingProvider, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				pk,
				8.CharactersString(),
				10.CharactersString());
			connection.ExecuteNonQuery(query);

			return pk;
		}

		public Guid InsertOrgAddress(string orgCode, string address, string cityName, string stateCode, string postCode, string portName)
		{
			Guid orgPK = Guid.NewGuid();
			Guid addressPK = Guid.NewGuid();

			string sql = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) values ('{0}', '{1}', 'Test Organisation')
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_City, OA_State, OA_PostCode, OA_RL_NKRelatedPortCode)
				VALUES('{2}', '{0}', '{3}', '{4}', '{5}', '{6}', '{7}')",
				orgPK.ToString(), orgCode, addressPK.ToString(), address, cityName, stateCode, postCode, portName);
			connection.ExecuteNonQuery(sql);

			return addressPK;
		}

		public Guid GenerateJobShipment(string bookingReference, string bookingServiceLevel, Guid originDepot)
		{
			var shipmentPK = Guid.NewGuid();
			var commandText = @"INSERT dbo.JobShipment (JS_PK, JS_BookingReference, JS_OA_ExportReceivingDepot, JS_RS_NKServiceLevel)
								VALUES (@JS_PK, @JS_BookingReference, @JS_OA_ExportReceivingDepot, @JS_RS_NKServiceLevel)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@JS_PK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@JS_BookingReference", SqlDbType.VarChar, JobShipmentSchema.JS_BookingReference.MaxLength, bookingReference);
				command.AddParameter("@JS_RS_NKServiceLevel", SqlDbType.VarChar, JobShipmentSchema.JS_RS_NKServiceLevel.MaxLength, bookingServiceLevel);
				command.AddParameter("@JS_OA_ExportReceivingDepot", SqlDbType.UniqueIdentifier, originDepot);
				command.ExecuteNonQuery();
			}

			return shipmentPK;
		}

		public void UpdateJobShipmentExportReceivingDepot(Guid shipmntPK, Guid originDepot)
		{
			var commandText = @"
UPDATE dbo.JobShipment
SET
	JS_OA_ExportReceivingDepot = @JS_OA_ExportReceivingDepot,
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '~BP'
WHERE
	JS_PK = @JS_PK";
			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@JS_PK", SqlDbType.UniqueIdentifier, shipmntPK);
				command.AddParameter("@JS_OA_ExportReceivingDepot", SqlDbType.UniqueIdentifier, originDepot);
				command.ExecuteNonQuery();
			}
		}

		public Guid GenerateHVLVConsignmentHeader(int clusterKey, Guid shipmentPk, DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser, string jobNumber)
		{
			var consignmentHeaderPK = Guid.NewGuid();
			var commandText = @"INSERT dbo.HVLVConsignmentHeader (HCH_PK, HCH_ClusterKey, HCH_JS_Shipment, HCH_SystemCreateTimeUtc, HCH_SystemCreateUser,  HCH_SystemLastEditTimeUtc, HCH_SystemLastEditUser, HCH_JobNumber)
								VALUES (@HCH_PK, @HCH_ClusterKey, @HCH_JS_Shipment, @HCH_SystemCreateTimeUtc, @HCH_SystemCreateUser, @HCH_SystemLastEditTimeUtc, @HCH_SystemLastEditUser, @HCH_JobNumber)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@HCH_PK", SqlDbType.UniqueIdentifier, consignmentHeaderPK);
				command.AddParameter("@HCH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@HCH_JS_Shipment", SqlDbType.UniqueIdentifier, shipmentPk);
				command.AddParameter("@HCH_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@HCH_SystemCreateUser", SqlDbType.VarChar, HVLVConsignmentHeaderSchema.HCH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@HCH_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@HCH_SystemLastEditUser", SqlDbType.VarChar, HVLVConsignmentHeaderSchema.HCH_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@HCH_JobNumber", SqlDbType.VarChar, jobNumber);
				command.ExecuteNonQuery();
			}
			return consignmentHeaderPK;
		}

		public Guid NewHvlvBookingHeader(int clusterKey, string bookingReference, Guid billToParty, string bookingServiceLevel, Guid originDepot, int itemCount,
					DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser)
		{
			var pk = Guid.NewGuid();
			var commandText = @"INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_ClusterKey, HVH_BookingReference, HVH_OA_BillToParty, HVH_RS_NKBookingServiceLevel, HVH_OA_OriginDepot, HVH_SystemCreateTimeUtc, HVH_SystemCreateUser,  HVH_SystemLastEditTimeUtc, HVH_SystemLastEditUser)
								VALUES (@HVH_PK, @HVH_ClusterKey, @HVH_BookingReference, @HVH_OA_BillToParty, @HVH_RS_NKBookingServiceLevel, @HVH_OA_OriginDepot, @HVH_SystemCreateTimeUtc, @HVH_SystemCreateUser, @HVH_SystemLastEditTimeUtc, @HVH_SystemLastEditUser)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@HVH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@HVH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@HVH_BookingReference", SqlDbType.VarChar, HVLVBookingHeaderSchema.HVH_BookingReference.MaxLength, bookingReference);
				command.AddParameter("@HVH_OA_BillToParty", SqlDbType.UniqueIdentifier, billToParty);
				command.AddParameter("@HVH_RS_NKBookingServiceLevel", SqlDbType.VarChar, HVLVBookingHeaderSchema.HVH_RS_NKBookingServiceLevel.MaxLength, bookingServiceLevel);
				command.AddParameter("@HVH_OA_OriginDepot", SqlDbType.UniqueIdentifier, originDepot);
				command.AddParameter("@HVH_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@HVH_SystemCreateUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@HVH_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@HVH_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemLastEditUser.MaxLength, lastEditUser);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void UpdateHvlvBookingHeaderOriginDepot(Guid headerPK, int clusterKey, Guid originDepot)
		{
			var commandText = @"UPDATE dbo.HVLVBookingHeader SET HVH_OA_OriginDepot = @HVH_OA_OriginDepot WHERE HVH_PK = @HVH_PK AND HVH_ClusterKey = @HVH_ClusterKey";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@HVH_PK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@HVH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@HVH_OA_OriginDepot", SqlDbType.UniqueIdentifier, originDepot);
				command.ExecuteNonQuery();
			}
		}

		public Guid GenerateHvlvConsignment_WithBookingHeader(int clusterKey, Guid headerPK, string consignmentId, string waybillNumber, string shipperReference, int itemCount, string goodsDescription,
			string undgClass, string status,
			string consigneeName, string consigneeAddress1, string consigneeAddress2, string consigneeCity, string consigneeState, string consigneePostcode, string consigneeCountryCode,
			DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser)
		{
			var pk = Guid.NewGuid();
			var commandText = @"INSERT dbo.HVLVConsignment (
HVC_PK,
HVC_ClusterKey,
HVC_HVH_BookingHeader,
HVC_ConsignmentId,
HVC_WaybillNumber,
HVC_ShipperReference,
HVC_ItemCount,
HVC_GoodsDescription,
HVC_UndgClass,
HVC_Status,
HVC_ConsigneeName,
HVC_ConsigneeAddress1,
HVC_ConsigneeAddress2,
HVC_ConsigneeCity,
HVC_ConsigneeState,
HVC_ConsigneePostcode,
HVC_RN_NKConsigneeCountryCode,
HVC_SystemCreateTimeUtc,
HVC_SystemCreateUser,
HVC_SystemLastEditTimeUtc,
HVC_SystemLastEditUser
)
VALUES
(
@HVC_PK,
@HVC_ClusterKey,
@HVC_HVH_BookingHeader,
@HVC_ConsignmentId,
@HVC_WaybillNumber,
@HVC_ShipperReference,
@HVC_ItemCount,
@HVC_GoodsDescription,
@HVC_UndgClass,
@HVC_Status,
@HVC_ConsigneeName,
@HVC_ConsigneeAddress1,
@HVC_ConsigneeAddress2,
@HVC_ConsigneeCity,
@HVC_ConsigneeState,
@HVC_ConsigneePostcode,
@HVC_RN_NKConsigneeCountryCode,
@HVC_SystemCreateTimeUtc,
@HVC_SystemCreateUser,
@HVC_SystemLastEditTimeUtc,
@HVC_SystemLastEditUser)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@HVC_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@HVC_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@HVC_HVH_BookingHeader", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@HVC_ConsignmentId", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ConsignmentId.MaxLength, consignmentId);
				command.AddParameter("@HVC_WaybillNumber", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_WaybillNumber.MaxLength, waybillNumber);
				command.AddParameter("@HVC_ShipperReference", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ShipperReference.MaxLength, shipperReference);
				command.AddParameter("@HVC_ItemCount", SqlDbType.Int, itemCount);
				command.AddParameter("@HVC_GoodsDescription", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_GoodsDescription.MaxLength, goodsDescription);
				command.AddParameter("@HVC_UndgClass", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_UndgClass.MaxLength, undgClass);
				command.AddParameter("@HVC_Status", SqlDbType.Char, HVLVConsignmentSchema.HVC_Status.MaxLength, status);
				command.AddParameter("@HVC_ConsigneeName", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeName.MaxLength, consigneeName);
				command.AddParameter("@HVC_ConsigneeAddress1", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeAddress1.MaxLength, consigneeAddress1);
				command.AddParameter("@HVC_ConsigneeAddress2", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeAddress2.MaxLength, consigneeAddress2);
				command.AddParameter("@HVC_ConsigneeCity", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeCity.MaxLength, consigneeCity);
				command.AddParameter("@HVC_ConsigneeState", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeState.MaxLength, consigneeState);
				command.AddParameter("@HVC_ConsigneePostcode", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneePostcode.MaxLength, consigneePostcode);
				command.AddParameter("@HVC_RN_NKConsigneeCountryCode", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode.MaxLength, consigneeCountryCode);
				command.AddParameter("@HVC_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@HVC_SystemCreateUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@HVC_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@HVC_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemLastEditUser.MaxLength, lastEditUser);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid GenerateHvlvConsignment_WithConsignmentHeader(int clusterKey, Guid consignmentHeaderPK, string consignmentId, string waybillNumber, string shipperReference, int itemCount, string goodsDescription,
			string undgClass, string status,
			string consigneeName, string consigneeAddress1, string consigneeAddress2, string consigneeCity, string consigneeState, string consigneePostcode, string consigneeCountryCode,
			DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser)
		{
			var pk = Guid.NewGuid();
			var commandText = @"INSERT dbo.HVLVConsignment (
HVC_PK,
HVC_ClusterKey,
HVC_HCH_Header,
HVC_ConsignmentId,
HVC_WaybillNumber,
HVC_ShipperReference,
HVC_ItemCount,
HVC_GoodsDescription,
HVC_UndgClass,
HVC_Status,
HVC_ConsigneeName,
HVC_ConsigneeAddress1,
HVC_ConsigneeAddress2,
HVC_ConsigneeCity,
HVC_ConsigneeState,
HVC_ConsigneePostcode,
HVC_RN_NKConsigneeCountryCode,
HVC_SystemCreateTimeUtc,
HVC_SystemCreateUser,
HVC_SystemLastEditTimeUtc,
HVC_SystemLastEditUser
)
VALUES
(
@HVC_PK,
@HVC_ClusterKey,
@HVC_HCH_Header,
@HVC_ConsignmentId,
@HVC_WaybillNumber,
@HVC_ShipperReference,
@HVC_ItemCount,
@HVC_GoodsDescription,
@HVC_UndgClass,
@HVC_Status,
@HVC_ConsigneeName,
@HVC_ConsigneeAddress1,
@HVC_ConsigneeAddress2,
@HVC_ConsigneeCity,
@HVC_ConsigneeState,
@HVC_ConsigneePostcode,
@HVC_RN_NKConsigneeCountryCode,
@HVC_SystemCreateTimeUtc,
@HVC_SystemCreateUser,
@HVC_SystemLastEditTimeUtc,
@HVC_SystemLastEditUser)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@HVC_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@HVC_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@HVC_HCH_Header", SqlDbType.UniqueIdentifier, consignmentHeaderPK);
				command.AddParameter("@HVC_ConsignmentId", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ConsignmentId.MaxLength, consignmentId);
				command.AddParameter("@HVC_WaybillNumber", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_WaybillNumber.MaxLength, waybillNumber);
				command.AddParameter("@HVC_ShipperReference", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ShipperReference.MaxLength, shipperReference);
				command.AddParameter("@HVC_ItemCount", SqlDbType.Int, itemCount);
				command.AddParameter("@HVC_GoodsDescription", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_GoodsDescription.MaxLength, goodsDescription);
				command.AddParameter("@HVC_UndgClass", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_UndgClass.MaxLength, undgClass);
				command.AddParameter("@HVC_Status", SqlDbType.Char, HVLVConsignmentSchema.HVC_Status.MaxLength, status);
				command.AddParameter("@HVC_ConsigneeName", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeName.MaxLength, consigneeName);
				command.AddParameter("@HVC_ConsigneeAddress1", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeAddress1.MaxLength, consigneeAddress1);
				command.AddParameter("@HVC_ConsigneeAddress2", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeAddress2.MaxLength, consigneeAddress2);
				command.AddParameter("@HVC_ConsigneeCity", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeCity.MaxLength, consigneeCity);
				command.AddParameter("@HVC_ConsigneeState", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneeState.MaxLength, consigneeState);
				command.AddParameter("@HVC_ConsigneePostcode", SqlDbType.NVarChar, HVLVConsignmentSchema.HVC_ConsigneePostcode.MaxLength, consigneePostcode);
				command.AddParameter("@HVC_RN_NKConsigneeCountryCode", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode.MaxLength, consigneeCountryCode);
				command.AddParameter("@HVC_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@HVC_SystemCreateUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@HVC_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@HVC_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemLastEditUser.MaxLength, lastEditUser);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void SetupTestData()
		{
			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsShippingProvider, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
SELECT '9F9BF3F2-F10A-4610-87D7-EBF2E6114027', 'DESTDEP', 'Destination Depot Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'CA4D9B20-550C-4F3C-B862-3037884E6ABE', 'ORGDEP', 'Origin Depot Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '7DB81DC5-D521-4EF0-80DB-DADF10538019', 'ORGDEP2', 'Origin Depot Org 2', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '6EB24F27-9BC6-4D6C-84CA-EF10B5096EAC', 'ORGDEP3', 'Origin Depot Org 3', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '77EE15A4-849A-48D5-A7E8-9313840E729E', 'CARRIER1', 'Carrier 1 Org', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '01B40E8F-D697-4551-AE6B-E5606A86469A', 'CARRIER2', 'Carrier 2 Org', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '4ED2BF6C-A797-4946-9EC7-01D4DBF7A001', 'AGENT1', 'Agent 1 Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '9B108156-E021-4B7A-BD08-03FCA9FDB457', 'AGENT2', 'Agent 2 Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A', 'AGENT3', 'Agent 3 Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgAddress(OA_PK,OA_OH,OA_Code,OA_Address1,OA_SystemCreateTimeUtc,OA_SystemCreateUser,OA_SystemLastEditTimeUtc,OA_SystemLastEditUser)
SELECT '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE','9F9BF3F2-F10A-4610-87D7-EBF2E6114027','DDP','Destination Depot Org Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA','CA4D9B20-550C-4F3C-B862-3037884E6ABE','ODP','Origin Depot Org Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'C5583751-BC3B-49A2-AEA2-DA3272DC5F94','7DB81DC5-D521-4EF0-80DB-DADF10538019','OD2','Origin Depot Org 2 Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '55D0FD0D-98AB-4594-ABFC-B53F0FDC48DD','6EB24F27-9BC6-4D6C-84CA-EF10B5096EAC','OD3','Origin Depot Org 3 Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '9F627805-D342-4187-9484-F11E6FC6390F','77EE15A4-849A-48D5-A7E8-9313840E729E','CR1','Carrier 1 Org Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'BE064C94-B632-418E-946A-1D5D3DE30EFE','01B40E8F-D697-4551-AE6B-E5606A86469A','CR2','Carrier 2 Org Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH)
SELECT '4A90BFCB-5860-4893-9403-3700C0AA45C4','77EE15A4-849A-48D5-A7E8-9313840E729E' UNION ALL
SELECT '735D5DCD-6BB8-407D-9645-4C476B9F779A','01B40E8F-D697-4551-AE6B-E5606A86469A'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgCarrierServiceLevel (PL_PK, PL_Code, PL_CarrierServiceLevelDescription, PL_OM, PL_SystemCreateTimeUtc, PL_SystemCreateUser, PL_SystemLastEditTimeUtc, PL_SystemLastEditUser)
SELECT 'E0679085-E30E-43E0-A6E4-E3A17914212C','D2D','Door To Door', '4A90BFCB-5860-4893-9403-3700C0AA45C4', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '56A929E8-F898-4E10-8AB5-1E884398CE27','TSP','Transhipment', '4A90BFCB-5860-4893-9403-3700C0AA45C4', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '47A9E869-0B1A-4427-945E-C200DC77E35B','DEF','Deferred', '735D5DCD-6BB8-407D-9645-4C476B9F779A', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgCarrierAccount (OAN_PK, OAN_OH_Carrier, OAN_OH_BillToParty, OAN_AccountNumber, OAN_SystemCreateTimeUtc, OAN_SystemCreateUser, OAN_SystemLastEditTimeUtc, OAN_SystemLastEditUser)
SELECT '0624F85B-F260-4701-AE3A-5C4F92458649', '77EE15A4-849A-48D5-A7E8-9313840E729E', '77EE15A4-849A-48D5-A7E8-9313840E729E', '13579', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '2E19376C-739E-42CA-AA04-2048206694B5', '01B40E8F-D697-4551-AE6B-E5606A86469A', '01B40E8F-D697-4551-AE6B-E5606A86469A', '24680', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_UndgClass, TY_RatingFreightMode, TY_OA_DepotAddress, TY_OA_DispatchDepotAddress, TY_OH_CarrierBookingAgent)
SELECT 'AECB1469-104B-46ED-B67E-DA06C7B3C16B','DLV', '', '3', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA', NULL, '4ED2BF6C-A797-4946-9EC7-01D4DBF7A001' UNION ALL
SELECT '9CE3CA11-8E31-4F88-9624-49F362CF1904','DLV', 'STD', 'ALL', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA', NULL, '4ED2BF6C-A797-4946-9EC7-01D4DBF7A001' UNION ALL
SELECT '77E51A42-9969-42B8-9E3A-6A3C8BDCA94C','DLV', '', 'ALL', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA', NULL, '4ED2BF6C-A797-4946-9EC7-01D4DBF7A001' UNION ALL
SELECT 'CCE3FC09-4136-4DC9-9A90-80CAB00A4C3F','PIC', 'STD', '', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA', NULL, '4ED2BF6C-A797-4946-9EC7-01D4DBF7A001' UNION ALL
SELECT '3A18CD21-E848-4E6B-9BBB-0F948409B4DD','PIC', 'DIR', '', 'ALL', 'C5583751-BC3B-49A2-AEA2-DA3272DC5F94', NULL, '9B108156-E021-4B7A-BD08-03FCA9FDB457' UNION ALL
SELECT '2F1188EF-A3C9-4624-92EA-84133EFC70D5','PIC', 'D2D', '', 'ALL', 'C5583751-BC3B-49A2-AEA2-DA3272DC5F94', NULL, '9B108156-E021-4B7A-BD08-03FCA9FDB457' UNION ALL
SELECT '2FB208A1-624D-44BF-AF4C-551E3EB04C15','PIC', 'COU', '', 'ALL', '55D0FD0D-98AB-4594-ABFC-B53F0FDC48DD', NULL, 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A' UNION ALL
SELECT '61A33961-45FE-457A-BCEA-9F4972A25A59','PIC', 'COU', '', 'ALL', '55D0FD0D-98AB-4594-ABFC-B53F0FDC48DD', NULL, 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A' UNION ALL
SELECT 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C','DLV', 'DIR', '2', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE', NULL, 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A' UNION ALL
SELECT 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11','DLV', 'COU', '1', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE', NULL, 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A' UNION ALL
SELECT '24988795-58B3-411F-844C-51EB00016B72','DLV', 'DIR', '', 'AIR', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE', NULL, 'C4C74E6E-0780-47CF-A701-C0E9DBB93B7A'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportProvider (TP_PK, TP_RN_NKCountry, TP_OH_RelatedParty, TP_SystemCreateTimeUtc, TP_SystemCreateUser, TP_SystemLastEditTimeUtc, TP_SystemLastEditUser)
SELECT '968FA263-04D6-4D77-B15D-3D26ED22E08F','US','77EE15A4-849A-48D5-A7E8-9313840E729E', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '377657A5-8111-4CB9-80AF-FC0D941BA553','US','01B40E8F-D697-4551-AE6B-E5606A86469A', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZones (TZ_PK, TZ_ZoneName, TZ_TP, TZ_SystemCreateTimeUtc, TZ_SystemCreateUser, TZ_SystemLastEditTimeUtc, TZ_SystemLastEditUser)
SELECT '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C','Melbourne Metro', '968FA263-04D6-4D77-B15D-3D26ED22E08F', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98','Adelaide Metro', '968FA263-04D6-4D77-B15D-3D26ED22E08F', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '62D0F80C-23A5-4896-A302-DF5759EFEDC1','Sydney Metro', '377657A5-8111-4CB9-80AF-FC0D941BA553', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RefPostCode (RK_PK, RK_CityTownPostCode, RK_RN_NKCountry, RK_Lattitude, RK_Longitude)
SELECT '8F26A346-ECAB-4283-AAB4-E06339262D73','3500', 'AU', 101.2, 20.5 UNION ALL
SELECT 'ACAB8B06-8526-4401-AF5A-1E095A5C91BA','3600', 'AU', 121.2, 25.5");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RefCityTown (R9_PK, R9_InternationalName, R9_RW_NKState, R9_RN_NKCountry)
SELECT 'B62D5E90-DFC6-433A-A509-553E04300265','Adelaide City', 'SA', 'AU' UNION ALL
SELECT '3280E014-DA4C-4ED5-8B5F-0E4C67A9EE99','Sydney City', 'NSW', 'AU'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_R9_CityTown, TQ_FromPostCode, TQ_ToPostCode, TQ_RN_NKCountry, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser)
SELECT '29FC9164-FC58-4E6E-A6D3-783F6FB37123','5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', NULL , '3500', '3600', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'D9DAC3D7-74CC-45CB-9B54-B2C86A4CE1E4','5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', NULL , '4300', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '35FAD11E-9E83-4BA2-A1F6-6F3D56117F21', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'B62D5E90-DFC6-433A-A509-553E04300265', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'AC0E302D-5046-4643-9646-D5E72917C639', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', '3280E014-DA4C-4ED5-8B5F-0E4C67A9EE99', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel, TX_CarrierAccountNumber)
SELECT 'BCD539FF-5516-4676-BB0A-47B08D673960', 'AECB1469-104B-46ED-B67E-DA06C7B3C16B', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT 'A4CA942C-A7CC-46E8-9A1B-01F75FF76E3D', '9CE3CA11-8E31-4F88-9624-49F362CF1904', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT '7AE8E14D-8D98-4FE8-9C3F-EECA11B17D8F', '77E51A42-9969-42B8-9E3A-6A3C8BDCA94C', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT '52C114B0-E3BC-4ABD-9BE4-CC491FECDCA9', 'CCE3FC09-4136-4DC9-9A90-80CAB00A4C3F', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT 'A90871FD-257E-4DC6-8A02-0653EB981DFD', 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'D2D', '13579' UNION ALL
SELECT 'AF64148E-754B-4397-B78E-28A5DF9DFBE5', 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'TSP', '13579' UNION ALL
SELECT '5CFB04C4-2F1C-47F8-9B20-BA96FECCA961', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'D2D', '13579' UNION ALL
SELECT 'E24BA2E5-3869-4703-9AB0-FEA08AF6772B', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'TSP', '13579' UNION ALL
SELECT 'CC5CB590-B5D6-4BCD-A021-B59C6CC322B0', '3A18CD21-E848-4E6B-9BBB-0F948409B4DD', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT 'F67BC0F9-40EA-4A75-B078-3D2890398F6E', '2F1188EF-A3C9-4624-92EA-84133EFC70D5', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT 'DB1668C7-EF29-4A4C-BC5D-546A7A135660', '2FB208A1-624D-44BF-AF4C-551E3EB04C15', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT '8F8F9C41-C891-46F1-8F21-A34EF2335315', '61A33961-45FE-457A-BCEA-9F4972A25A59', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR', '' UNION ALL
SELECT '68CBF4C2-A1C9-48D8-96B9-88E078383CFF', '24988795-58B3-411F-844C-51EB00016B72', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', 'DEF', '24680'");
		}
	}
}
