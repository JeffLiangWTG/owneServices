using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public class SupplierBookingGeneratorForTests
	{
		readonly DbConnection connection;

		public SupplierBookingGeneratorForTests(DbConnection connection)
		{
			this.connection = connection;
		}

		public Guid GenerateAddress()
		{
			var orgPK = GenerateOrganisation();
			var addressPK = Guid.NewGuid();

			var query = string.Format(
				"INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)  VALUES ('{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
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

			var query = string.Format(
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

			string sql = string.Format(@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{0}', '{1}', 'Test Organisation', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_City, OA_State, OA_PostCode, OA_RL_NKRelatedPortCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
				VALUES('{2}', '{0}', '{3}', '{4}', '{5}', '{6}', '{7}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				orgPK.ToString(), orgCode, addressPK.ToString(), address, cityName, stateCode, postCode, portName);
			connection.ExecuteNonQuery(sql);

			return addressPK;
		}

		public Guid InsertJobDocAddress(string address, string cityName, string stateCode, string postCode, string countryCode, string addressType, Guid bookingLinePK)
		{
			Guid jobDocAddressPK = Guid.NewGuid();

			string sql = string.Format(@"
				INSERT INTO dbo.JobDocAddress(E2_PK, E2_Address1, E2_City, E2_State, E2_PostCode, E2_RN_NKCountryCode, E2_AddressType, E2_ParentID, E2_ParentTableCode)
				VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', 'DL')",
				jobDocAddressPK.ToString(), address, cityName, stateCode, postCode, countryCode, addressType, bookingLinePK.ToString());
			connection.ExecuteNonQuery(sql);

			return jobDocAddressPK;
		}

		public Guid NewSupplierBookingHeader(string supplierReference, bool isApproved, Guid consignor, Guid dispatchAddress, DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser)
		{
			var pk = Guid.NewGuid();
			var commandText = (dispatchAddress != Guid.Empty)
				? "INSERT dbo.SupplierBookingHeader (DH_PK, DH_SupplierReference, DH_IsShipperApproved, DH_OA_Consignor, DH_OA_DispatchAddress, DH_SystemCreateTimeUtc, DH_SystemCreateUser, DH_SystemLastEditTimeUtc, DH_SystemLastEditUSer) VALUES (@DH_PK, @DH_SupplierReference, @DH_IsShipperApproved, @DH_OA_Consignor, @DH_OA_DispatchAddress, @DH_SystemCreateTimeUtc, @DH_SystemCreateUser, @DH_SystemLastEditTimeUtc, @DH_SystemLastEditUSer)"
				: "INSERT dbo.SupplierBookingHeader (DH_PK, DH_SupplierReference, DH_IsShipperApproved, DH_OA_Consignor, DH_OA_DispatchAddress, DH_SystemCreateTimeUtc, DH_SystemCreateUser, DH_SystemLastEditTimeUtc, DH_SystemLastEditUSer) VALUES (@DH_PK, @DH_SupplierReference, @DH_IsShipperApproved, @DH_OA_Consignor, @DH_OA_DispatchAddress, @DH_SystemCreateTimeUtc, @DH_SystemCreateUser, @DH_SystemLastEditTimeUtc, @DH_SystemLastEditUSer)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@DH_SupplierReference", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SupplierReference.MaxLength, supplierReference);
				command.AddParameter("@DH_IsShipperApproved", SqlDbType.Bit, isApproved);
				command.AddParameter("@DH_OA_Consignor", SqlDbType.UniqueIdentifier, consignor);
				command.AddParameter("@DH_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@DH_SystemCreateUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@DH_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@DH_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@DH_GrossWeightInKg", SqlDbType.Money, 0);
				command.AddParameter("@DH_CubicInM3", SqlDbType.Money, 0);
				command.AddParameter("@DH_PiecesManifested", SqlDbType.Int, 0);

				if (dispatchAddress != Guid.Empty)
				{
					command.AddParameter("@DH_OA_DispatchAddress", SqlDbType.UniqueIdentifier, dispatchAddress);
				}

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid NewSupplierBookingHeader(string supplierReference, bool isApproved, Guid consignor, DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser)
		{
			var pk = Guid.NewGuid();
			var commandText = "INSERT dbo.SupplierBookingHeader (DH_PK, DH_SupplierReference, DH_IsShipperApproved, DH_OA_Consignor, DH_SystemCreateTimeUtc, DH_SystemCreateUser, DH_SystemLastEditTimeUtc, DH_SystemLastEditUSer) VALUES (@DH_PK, @DH_SupplierReference, @DH_IsShipperApproved, @DH_OA_Consignor, @DH_SystemCreateTimeUtc, @DH_SystemCreateUser, @DH_SystemLastEditTimeUtc, @DH_SystemLastEditUSer)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@DH_SupplierReference", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SupplierReference.MaxLength, supplierReference);
				command.AddParameter("@DH_IsShipperApproved", SqlDbType.Bit, isApproved);
				command.AddParameter("@DH_OA_Consignor", SqlDbType.UniqueIdentifier, consignor);
				command.AddParameter("@DH_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@DH_SystemCreateUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@DH_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@DH_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingHeaderSchema.DH_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@DH_GrossWeightInKg", SqlDbType.Money, 0);
				command.AddParameter("@DH_CubicInM3", SqlDbType.Money, 0);
				command.AddParameter("@DH_PiecesManifested", SqlDbType.Int, 0);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid NewSupplierBookingLine(Guid headerPk, string consigneeReference, decimal goodsValue, string goodsValueCurrency,
										   double grossWeight, string grossWeightUQ, double cubic, string cubicUQ, string goodsDescription,
										   string marksAndNumbers, int piecesManifested,
										   DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser,
										   string origin, string destination,
										   string address, string city, string state, string postCode, string countryCode,
										   string serviceLevel = "", string status = "", string packType = "")
		{
			var pk = Guid.NewGuid();
			var commandText = @"INSERT dbo.SupplierBookingLine (
DL_PK, DL_DH_BookingHeader
,DL_ConsigneeReference
,DL_GoodsValue
,DL_RX_NKGoodsValueCurrency
,DL_GrossWeight
,DL_GrossWeightUQ
,DL_Cubic
,DL_CubicUQ
,DL_GoodsDescription
,DL_MarksAndNumbers
,DL_PiecesManifested
,DL_SystemCreateTimeUtc
,DL_SystemCreateUser
,DL_SystemLastEditTimeUtc
,DL_SystemLastEditUSer
,DL_RS_NKServiceLevel
,DL_Status
,DL_ConsigneeAddress1
,DL_ConsigneeCity
,DL_ConsigneeState
,DL_ConsigneePostCode
,DL_RN_NKConsigneeCountryCode
)
VALUES 
(
@DL_PK
,@DL_DH_BookingHeader
,@DL_ConsigneeReference
,@DL_GoodsValue
,@DL_RX_NKGoodsValueCurrency
,@DL_GrossWeight
,@DL_GrossWeightUQ
,@DL_Cubic
,@DL_CubicUQ
,@DL_GoodsDescription
,@DL_MarksAndNumbers
,@DL_PiecesManifested
,@DL_SystemCreateTimeUtc
,@DL_SystemCreateUser
,@DL_SystemLastEditTimeUtc
,@DL_SystemLastEditUSer
,@DL_RS_NKServiceLevel
,@DL_Status
,@DL_ConsigneeAddress1
,@DL_ConsigneeCity
,@DL_ConsigneeState
,@DL_ConsigneePostCode
,@DL_RN_NKConsigneeCountryCode
)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DL_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@DL_DH_BookingHeader", SqlDbType.UniqueIdentifier, headerPk);
				command.AddParameter("@DL_ConsigneeReference", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneeReference.MaxLength, consigneeReference);
				command.AddParameter("@DL_GoodsValue", SqlDbType.Money, goodsValue);
				command.AddParameter("@DL_RX_NKGoodsValueCurrency", SqlDbType.VarChar, SupplierBookingLineSchema.DL_RX_NKGoodsValueCurrency.MaxLength, goodsValueCurrency);
				command.AddParameter("@DL_GrossWeight", SqlDbType.Float, grossWeight);
				command.AddParameter("@DL_GrossWeightUQ", SqlDbType.VarChar, SupplierBookingLineSchema.DL_GrossWeightUQ.MaxLength, grossWeightUQ);
				command.AddParameter("@DL_Cubic", SqlDbType.Float, cubic);
				command.AddParameter("@DL_CubicUQ", SqlDbType.VarChar, SupplierBookingLineSchema.DL_CubicUQ.MaxLength, cubicUQ);
				command.AddParameter("@DL_GoodsDescription", SqlDbType.VarChar, SupplierBookingLineSchema.DL_GoodsDescription.MaxLength, goodsDescription);
				command.AddParameter("@DL_MarksAndNumbers", SqlDbType.VarChar, SupplierBookingLineSchema.DL_MarksAndNumbers.MaxLength, marksAndNumbers);
				command.AddParameter("@DL_PiecesManifested", SqlDbType.Int, piecesManifested);
				command.AddParameter("@DL_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@DL_SystemCreateUser", SqlDbType.VarChar, SupplierBookingLineSchema.DL_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@DL_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@DL_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingLineSchema.DL_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@DL_RS_NKServiceLevel", SqlDbType.VarChar, SupplierBookingLineSchema.DL_RS_NKServiceLevel.MaxLength, serviceLevel);
				command.AddParameter("@DL_Status", SqlDbType.VarChar, SupplierBookingLineSchema.DL_Status.MaxLength, status);
				command.AddParameter("@DL_F3_NKPackType", SqlDbType.VarChar, SupplierBookingLineSchema.DL_F3_NKPackType.MaxLength, packType);

				command.AddParameter("@DL_ConsigneeAddress1", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneeAddress1.MaxLength, address);
				command.AddParameter("@DL_ConsigneeCity", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneeCity.MaxLength, city);
				command.AddParameter("@DL_ConsigneeState", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneeState.MaxLength, state);
				command.AddParameter("@DL_ConsigneePostCode", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneePostCode.MaxLength, postCode);
				command.AddParameter("@DL_RN_NKConsigneeCountryCode", SqlDbType.VarChar, SupplierBookingLineSchema.DL_RN_NKConsigneeCountryCode.MaxLength, countryCode);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid NewSupplierBookingLine(Guid headerPk, string consigneeReference, decimal goodsValue, string goodsValueCurrency,
										   double grossWeight, string grossWeightUQ, double cubic, string cubicUQ, string goodsDescription,
										   string marksAndNumbers, int piecesManifested,
										   DateTime createTime, string createUser, DateTime lastEditTime, string lastEditUser,
										   string origin, string destination, string serviceLevel = "", string status = "", string packType = "")
		{
			var pk = Guid.NewGuid();
			var commandText = "INSERT dbo.SupplierBookingLine (DL_PK, DL_DH_BookingHeader, DL_ConsigneeReference, DL_GoodsValue, DL_RX_NKGoodsValueCurrency, DL_GrossWeight, DL_GrossWeightUQ, DL_Cubic, DL_CubicUQ, DL_GoodsDescription, DL_MarksAndNumbers, DL_PiecesManifested, DL_SystemCreateTimeUtc, DL_SystemCreateUser, DL_SystemLastEditTimeUtc, DL_SystemLastEditUSer, DL_RS_NKServiceLevel, DL_Status) VALUES (@DL_PK, @DL_DH_BookingHeader, @DL_ConsigneeReference, @DL_GoodsValue, @DL_RX_NKGoodsValueCurrency, @DL_GrossWeight, @DL_GrossWeightUQ, @DL_Cubic, @DL_CubicUQ, @DL_GoodsDescription, @DL_MarksAndNumbers, @DL_PiecesManifested, @DL_SystemCreateTimeUtc, @DL_SystemCreateUser, @DL_SystemLastEditTimeUtc, @DL_SystemLastEditUSer, @DL_RS_NKServiceLevel, @DL_Status)";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DL_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@DL_DH_BookingHeader", SqlDbType.UniqueIdentifier, headerPk);
				command.AddParameter("@DL_ConsigneeReference", SqlDbType.VarChar, SupplierBookingLineSchema.DL_ConsigneeReference.MaxLength, consigneeReference);
				command.AddParameter("@DL_GoodsValue", SqlDbType.Money, goodsValue);
				command.AddParameter("@DL_RX_NKGoodsValueCurrency", SqlDbType.VarChar, SupplierBookingLineSchema.DL_RX_NKGoodsValueCurrency.MaxLength, goodsValueCurrency);
				command.AddParameter("@DL_GrossWeight", SqlDbType.Float, grossWeight);
				command.AddParameter("@DL_GrossWeightUQ", SqlDbType.VarChar, SupplierBookingLineSchema.DL_GrossWeightUQ.MaxLength, grossWeightUQ);
				command.AddParameter("@DL_Cubic", SqlDbType.Float, cubic);
				command.AddParameter("@DL_CubicUQ", SqlDbType.VarChar, SupplierBookingLineSchema.DL_CubicUQ.MaxLength, cubicUQ);
				command.AddParameter("@DL_GoodsDescription", SqlDbType.VarChar, SupplierBookingLineSchema.DL_GoodsDescription.MaxLength, goodsDescription);
				command.AddParameter("@DL_MarksAndNumbers", SqlDbType.VarChar, SupplierBookingLineSchema.DL_MarksAndNumbers.MaxLength, marksAndNumbers);
				command.AddParameter("@DL_PiecesManifested", SqlDbType.Int, piecesManifested);
				command.AddParameter("@DL_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@DL_SystemCreateUser", SqlDbType.VarChar, SupplierBookingLineSchema.DL_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@DL_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@DL_SystemLastEditUser", SqlDbType.VarChar, SupplierBookingLineSchema.DL_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@DL_RS_NKServiceLevel", SqlDbType.VarChar, SupplierBookingLineSchema.DL_RS_NKServiceLevel.MaxLength, serviceLevel);
				command.AddParameter("@DL_Status", SqlDbType.VarChar, SupplierBookingLineSchema.DL_Status.MaxLength, status);
				command.AddParameter("@DL_F3_NKPackType", SqlDbType.VarChar, SupplierBookingLineSchema.DL_F3_NKPackType.MaxLength, packType);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void SetOriginDepot(Guid headerPk, Guid originDepot)
		{
			var commandText = @"UPDATE dbo.SupplierBookingHeader
				SET
					DH_OA_OriginDepot = @DH_OA_OriginDepot
				WHERE 
					DH_PK = @DH_PK";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DH_OA_OriginDepot", SqlDbType.UniqueIdentifier, originDepot);
				command.AddParameter("@DH_PK", SqlDbType.UniqueIdentifier, headerPk);
				command.ExecuteNonQuery();
			}
		}

		public void ResetOriginDepotToBlank(Guid headerPk)
		{
			var commandText = @"UPDATE dbo.SupplierBookingHeader
				SET
					DH_OA_OriginDepot = @DH_OA_OriginDepot
				WHERE 
					DH_PK = @DH_PK";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DH_OA_OriginDepot", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@DH_PK", SqlDbType.UniqueIdentifier, headerPk);
				command.ExecuteNonQuery();
			}
		}

		public void SetDestinationDepotAndCarrierInfo(Guid linePk, Guid destinationDepotPk, Guid carrierPk, string serviceLevel)
		{
			var commandText = @"UPDATE dbo.SupplierBookingLine
				SET
					DL_OA_DestinationDepot = @DL_OA_DestinationDepot,
					DL_OH_LastMileCarrier = @DL_OH_LastMileCarrier,
					DL_PL_NKCarrierServiceLevel = @DL_PL_NKCarrierServiceLevel
				WHERE
					DL_PK = @DL_PK";

			using (var command = connection.Command(commandText))
			{
				command.AddParameter("@DL_OA_DestinationDepot", SqlDbType.UniqueIdentifier, destinationDepotPk);
				command.AddParameter("@DL_PK", SqlDbType.UniqueIdentifier, linePk);
				command.AddParameter("@DL_OH_LastMileCarrier", SqlDbType.UniqueIdentifier, carrierPk);
				command.AddParameter("@DL_PL_NKCarrierServiceLevel", SqlDbType.VarChar, SupplierBookingLineSchema.DL_PL_NKCarrierServiceLevel.MaxLength, serviceLevel);
				command.ExecuteNonQuery();
			}
		}

		public void SetupTestData(bool includeBlankServiceLevel = false, bool includeBlankAndNonMatchingServiceLevel = false, bool includeInActiveTransportProvider = false, bool includeInActiveTransportZones = false)
		{
			connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsShippingProvider, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) 
SELECT '9F9BF3F2-F10A-4610-87D7-EBF2E6114027', 'DESTDEP', 'Destination Depot Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'CA4D9B20-550C-4F3C-B862-3037884E6ABE', 'ORGDEP', 'Origin Depot Org', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '7DB81DC5-D521-4EF0-80DB-DADF10538019', 'ORGDEP2', 'Origin Depot Org 2', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '6EB24F27-9BC6-4D6C-84CA-EF10B5096EAC', 'ORGDEP3', 'Origin Depot Org 3', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '77EE15A4-849A-48D5-A7E8-9313840E729E', 'CARRIER1', 'Carrier 1 Org', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '01B40E8F-D697-4551-AE6B-E5606A86469A', 'CARRIER2', 'Carrier 2 Org', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

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

			connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress) 
SELECT 'AECB1469-104B-46ED-B67E-DA06C7B3C16B','PIC', 'DIR', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA' UNION ALL
SELECT '9CE3CA11-8E31-4F88-9624-49F362CF1904','PIC', 'COU', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA' UNION ALL
SELECT '77E51A42-9969-42B8-9E3A-6A3C8BDCA94C','PIC', 'COU', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA' UNION ALL
SELECT 'CCE3FC09-4136-4DC9-9A90-80CAB00A4C3F','PIC', 'STD', 'ALL', '0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA' UNION ALL
SELECT '3A18CD21-E848-4E6B-9BBB-0F948409B4DD','PIC', 'DIR', 'ALL', 'C5583751-BC3B-49A2-AEA2-DA3272DC5F94' UNION ALL
SELECT '2F1188EF-A3C9-4624-92EA-84133EFC70D5','PIC', 'D2D', 'ALL', 'C5583751-BC3B-49A2-AEA2-DA3272DC5F94' UNION ALL
SELECT '2FB208A1-624D-44BF-AF4C-551E3EB04C15','PIC', 'COU', 'ALL', '55D0FD0D-98AB-4594-ABFC-B53F0FDC48DD' UNION ALL
SELECT '61A33961-45FE-457A-BCEA-9F4972A25A59','PIC', 'COU', 'ALL', '55D0FD0D-98AB-4594-ABFC-B53F0FDC48DD' UNION ALL
SELECT 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C','DLV', 'DIR', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE' UNION ALL
SELECT 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11','DLV', 'COU', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE' UNION ALL
SELECT '24988795-58B3-411F-844C-51EB00016B72','DLV', 'DIR', 'AIR', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportProvider (TP_PK, TP_RN_NKCountry, TP_SystemCreateTimeUtc, TP_SystemCreateUser, TP_SystemLastEditTimeUtc, TP_SystemLastEditUser)
SELECT '968FA263-04D6-4D77-B15D-3D26ED22E08F','US', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '377657A5-8111-4CB9-80AF-FC0D941BA553','US', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

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
SELECT '35FAD11E-9E83-4BA2-A1F6-6F3D56117F21', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'B62D5E90-DFC6-433A-A509-553E04300265', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'AC0E302D-5046-4643-9646-D5E72917C639', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', '3280E014-DA4C-4ED5-8B5F-0E4C67A9EE99', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel)
SELECT 'BCD539FF-5516-4676-BB0A-47B08D673960', 'AECB1469-104B-46ED-B67E-DA06C7B3C16B', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT 'A4CA942C-A7CC-46E8-9A1B-01F75FF76E3D', '9CE3CA11-8E31-4F88-9624-49F362CF1904', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '7AE8E14D-8D98-4FE8-9C3F-EECA11B17D8F', '77E51A42-9969-42B8-9E3A-6A3C8BDCA94C', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '52C114B0-E3BC-4ABD-9BE4-CC491FECDCA9', 'CCE3FC09-4136-4DC9-9A90-80CAB00A4C3F', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT 'A90871FD-257E-4DC6-8A02-0653EB981DFD', 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'D2D' UNION ALL
SELECT 'AF64148E-754B-4397-B78E-28A5DF9DFBE5', 'EF9FBC0B-72BC-4027-97FB-48CA82C36F11', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'TSP' UNION ALL
SELECT '5CFB04C4-2F1C-47F8-9B20-BA96FECCA961', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'D2D' UNION ALL
SELECT 'E24BA2E5-3869-4703-9AB0-FEA08AF6772B', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'TSP' UNION ALL
SELECT 'CC5CB590-B5D6-4BCD-A021-B59C6CC322B0', '3A18CD21-E848-4E6B-9BBB-0F948409B4DD', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT 'F67BC0F9-40EA-4A75-B078-3D2890398F6E', '2F1188EF-A3C9-4624-92EA-84133EFC70D5', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT 'DB1668C7-EF29-4A4C-BC5D-546A7A135660', '2FB208A1-624D-44BF-AF4C-551E3EB04C15', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '8F8F9C41-C891-46F1-8F21-A34EF2335315', '61A33961-45FE-457A-BCEA-9F4972A25A59', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '68CBF4C2-A1C9-48D8-96B9-88E078383CFF', '24988795-58B3-411F-844C-51EB00016B72', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', 'DEF'");

			if (includeBlankAndNonMatchingServiceLevel)
			{
				connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsShippingProvider, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) 
SELECT '0C4D983C-2482-42B8-9D1A-AB1F029EEA71', 'ORGDEP4', 'Origin Depot Org 4', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgAddress(OA_PK,OA_OH,OA_Code,OA_Address1,OA_SystemCreateTimeUtc,OA_SystemCreateUser,OA_SystemLastEditTimeUtc,OA_SystemLastEditUser) 
SELECT 'DB4F3821-A996-4CC2-AFCE-D3EBE01E408C','0C4D983C-2482-42B8-9D1A-AB1F029EEA71','OD4','Origin Depot Org 4 Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress) 
SELECT 'A382866C-8387-47FE-967D-7F1D31FEAB1C','DLV', 'EXP', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE' UNION ALL
SELECT '83E8A4EE-8C74-434A-9481-A90F3D613497','PIC', 'COU', 'ALL', 'DB4F3821-A996-4CC2-AFCE-D3EBE01E408C' UNION ALL
SELECT 'B52BF16E-80CA-49B7-8A83-D39C52602C82','PIC', 'COU', 'ALL', 'DB4F3821-A996-4CC2-AFCE-D3EBE01E408C' UNION ALL
SELECT '79DE7942-CE80-4380-80DF-3A173D571016','PIC', '',	'ALL', 'DB4F3821-A996-4CC2-AFCE-D3EBE01E408C'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel)
SELECT '34BBB8E2-D133-4EDC-9423-22EB66106371', 'A382866C-8387-47FE-967D-7F1D31FEAB1C', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', 'DEF' UNION ALL
SELECT 'A9C0F5E9-D080-45FB-8CD6-F3785EEAEDF6', '83E8A4EE-8C74-434A-9481-A90F3D613497', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '213FC614-7942-4DB0-A36C-DD7DCE194F04', 'B52BF16E-80CA-49B7-8A83-D39C52602C82', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR' UNION ALL
SELECT '0A18E8D5-E520-4A53-9876-793D873472D8', '79DE7942-CE80-4380-80DF-3A173D571016', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR'");
			}

			if (includeBlankServiceLevel)
			{
				connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsShippingProvider, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) 
SELECT '0A1506BA-9DB8-452F-A96C-8F822193C8A5', 'ORGDEP5', 'Origin Depot Org 5', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.OrgAddress(OA_PK,OA_OH,OA_Code,OA_Address1,OA_SystemCreateTimeUtc,OA_SystemCreateUser,OA_SystemLastEditTimeUtc,OA_SystemLastEditUser) 
SELECT 'A680DFB8-60F9-4C59-A885-AA1EB15D21E5','0A1506BA-9DB8-452F-A96C-8F822193C8A5','OD5','Origin Depot Org 5 Address', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress) 
SELECT 'D83AA8F6-4714-4846-9F4E-82BF1031EACC','DLV', 'EXP', 'ALL', '9DBE20CF-1573-4440-A43E-2A07ECCDDEFE' UNION ALL
SELECT 'AD9AD432-C594-4677-8A89-57D99DAFBE05','PIC', '',	'ALL', 'A680DFB8-60F9-4C59-A885-AA1EB15D21E5'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel)
SELECT '43C1CD0C-D977-4967-B26F-ADEB4E117E68', 'D83AA8F6-4714-4846-9F4E-82BF1031EACC', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', 'DEF' UNION ALL
SELECT 'A1175804-AE69-4B7E-927A-0122CD6F636A', 'AD9AD432-C594-4677-8A89-57D99DAFBE05', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'DIR'");
			}

			if (includeInActiveTransportProvider)
			{
				connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportProvider (TP_PK, TP_RN_NKCountry, TP_IsActive, TP_SystemCreateTimeUtc, TP_SystemCreateUser, TP_SystemLastEditTimeUtc, TP_SystemLastEditUser)
SELECT '5618a4d9-7fff-4dcd-a11f-362b094a3be6','AU', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZones (TZ_PK, TZ_ZoneName, TZ_TP, TZ_SystemCreateTimeUtc, TZ_SystemCreateUser, TZ_SystemLastEditTimeUtc, TZ_SystemLastEditUser) 
SELECT '272495a6-17cf-41bb-aab7-a39f5685b24f','Active Melbourne Metro', '5618a4d9-7fff-4dcd-a11f-362b094a3be6', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '683c6af6-3c37-4bb8-bd2f-3ca66ac29696','Active Adelaide Metro', '5618a4d9-7fff-4dcd-a11f-362b094a3be6', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_R9_CityTown, TQ_FromPostCode, TQ_ToPostCode, TQ_RN_NKCountry, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser) 
SELECT '73f61a2c-f75d-4ac3-8d60-6d5bc6ef922d','272495a6-17cf-41bb-aab7-a39f5685b24f', NULL , '3700', '3800', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '59428244-9a71-43d7-8425-b49777d56422', '683c6af6-3c37-4bb8-bd2f-3ca66ac29696', NULL , '3600', '3800', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' ");
			}

			if (includeInActiveTransportZones)
			{
				connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZones (TZ_PK, TZ_ZoneName, TZ_TP, TZ_IsActive, TZ_SystemCreateTimeUtc, TZ_SystemCreateUser, TZ_SystemLastEditTimeUtc, TZ_SystemLastEditUser) 
SELECT '2161fdf6-090d-4bd6-a7d0-53b6df150cda','InActive Melbourne Metro', '377657A5-8111-4CB9-80AF-FC0D941BA553', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'a9eb65ad-828c-4a2b-8963-076f390a4b75','Active Adelaide Metro', '377657A5-8111-4CB9-80AF-FC0D941BA553', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

				connection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_R9_CityTown, TQ_FromPostCode, TQ_ToPostCode, TQ_RN_NKCountry, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser) 
SELECT 'd41340c2-4a0b-4587-8712-d6da38be00fc','2161fdf6-090d-4bd6-a7d0-53b6df150cda', NULL , '3700', '3800', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'a2d8a753-3039-4d15-9872-8894f3b8a7b8','a9eb65ad-828c-4a2b-8963-076f390a4b75', NULL , '3600', '3800', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");
			}
		}
	}
}
