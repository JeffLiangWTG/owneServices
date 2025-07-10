using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public static class EDWTestDataCreator
	{
		public static (Guid, long) CreateOrganisation(string code, string name, string closestPort = "", Guid? orgHeaderPK = null)
		{
			var organizationKey = 0L;
			var organisationPK = orgHeaderPK ?? Guid.NewGuid();
			var sql = $@"
DECLARE @OrganizationKey BIGINT
SELECT @OrganizationKey = ISNULL(MAX(OrganizationKey), 0) + 1
FROM {Db.EdwDatabaseName}.Organization.BAS__Organization

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization(OrganizationID, OrganizationKey, Code, FullName, ClosestPort)
VALUES (@organisationPK, @OrganizationKey, @code, @name, @closestPort)

SELECT @OrganizationKey";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, code);
				command.AddParameter("@name", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, name);
				command.AddParameter("@closestPort", SqlDbType.VarChar, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength, closestPort);
				organizationKey = (long)command.ExecuteScalar();
			}
			return (organisationPK, organizationKey);
		}

		public static (Guid, long) CreateOrgCusCode(Guid orgHeaderPK, string codeType, string customsRegNo, string countryCode)
		{
			var orgCusCodeKey = 0L;
			var orgCusCodePK = Guid.NewGuid();
			var sql = $@"
DECLARE @OrgCusCodeKey BIGINT
SELECT @OrgCusCodeKey = ISNULL(MAX(OrgCusCodeKey), 0) + 1
FROM {Db.EdwDatabaseName}.Organization.BAS__OrgCusCode

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__OrgCusCode(OrgCusCodeID, OrgCusCodeKey, OrganizationID, CodeType, CustomsRegNo, CodeCountry)
VALUES(@orgCusCodePK, @OrgCusCodeKey, @orgHeaderPK, @codeType, @customsRegNo, @countryCode)

SELECT @OrgCusCodeKey";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgCusCodePK", SqlDbType.UniqueIdentifier, orgCusCodePK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@codeType", SqlDbType.VarChar, OrgCusCodeSchema.OK_CodeType.MaxLength, codeType);
				command.AddParameter("@customsRegNo", SqlDbType.VarChar, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, customsRegNo);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength, countryCode);
				orgCusCodeKey = (long)command.ExecuteScalar();
			}
			return (orgCusCodePK, orgCusCodeKey);
		}

		public static (Guid, long) CreateCompany(string companyCode, string countryCode, string currencyCode)
		{
			var companyKey = 0L;
			var companyPK = Guid.NewGuid();
			var sql = $@"
DECLARE @CompanyKey BIGINT
SELECT @CompanyKey = ISNULL(MAX(CompanyKey), 0) + 1
FROM {Db.EdwDatabaseName}.Organization.BAS__Company

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Company (CompanyID, CompanyKey, CompanyCode, CountryCode, LocalCurrency)
VALUES (@companyPK, @CompanyKey, @companyCode, @countryCode, @currencyNK)

SELECT @CompanyKey";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@companyCode", SqlDbType.VarChar, GlbCompanySchema.GC_Code.MaxLength, companyCode);
				command.AddParameter("@countryCode", SqlDbType.VarChar, GlbCompanySchema.GC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@currencyNK", SqlDbType.VarChar, GlbCompanySchema.GC_RX_NKLocalCurrency.MaxLength, currencyCode);
				companyKey = (long)command.ExecuteScalar();
			}
			return (companyPK, companyKey);
		}

		public static (Guid, long) CreateBranch(Guid companyPK, string branchCode, string homePort)
			=> CreateBranch(companyPK, branchCode, homePort, DateTime.Now);

		public static (Guid, long) CreateBranch(Guid companyPK, string branchCode, string homePort, DateTime referenceDate)
		{
			var branchKey = 0L;
			var branchPK = Guid.NewGuid();
			var sql = $@"
DECLARE @BranchKey BIGINT
SELECT @BranchKey = ISNULL(MAX(BranchKey), 0) + 1
FROM {Db.EdwDatabaseName}.Organization.BAS__Branch

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Branch (BranchID, BranchKey, BranchCode, HomePort, CompanyID)
VALUES (@branchPK, @BranchKey, @branchCode, @homePort, @companyPK)

SELECT @BranchKey";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@branchCode", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, branchCode);
				command.AddParameter("@homePort", SqlDbType.VarChar, GlbBranchSchema.GB_RL_NKHomePort.MaxLength, homePort);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@startDate", SqlDbType.DateTime, new DateTime(referenceDate.Year, 1, 1));
				command.AddParameter("@endDate", SqlDbType.DateTime, new DateTime(referenceDate.Year, 12, 31, 23, 59, 59, 999));
				command.AddParameter("@timeNow", SqlDbType.DateTime, referenceDate);
				branchKey = (long)command.ExecuteScalar();
			}
			return (branchPK, branchKey);
		}

		public static (Guid, long) CreateDeclaration(string dataModel, string applicationCode, string customsOffice, string houseBillNo, string jobNo, string masterBillNo, long branchKey, long importerKey, long declarantAddressKey, string addInfo)
		{
			var declarationKey = 0L;
			var declarationPK = Guid.NewGuid();
			var sql = $@"
DECLARE @DeclarationKey BIGINT
SELECT @DeclarationKey = ISNULL(MAX(DeclarationKey), 0) + 1
FROM {Db.EdwDatabaseName}.Customs.BAS__Declaration

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__Declaration (DeclarationID, DeclarationKey, DataModel, ApplicationCode, CustomsOffice, HouseBillNumber, JobNumber, MasterBillNumber, BranchKey, ImporterKey, OrganizationAddressKey5, AddInfo)
VALUES (@declarationPK, @DeclarationKey, @dataModel, @applicationCode, @customsOffice, @houseBillNumber, @jobNumber, @masterBillNumber, @branchKey, @importerKey, @declarantAddressKey, @addInfo)

SELECT @DeclarationKey";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, JobDeclarationSchema.JE_DataModel.MaxLength, dataModel);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, JobDeclarationSchema.JE_ApplicationCode.MaxLength, applicationCode);
				command.AddParameter("@customsOffice", SqlDbType.VarChar, JobDeclarationSchema.JE_CustomsOffice.MaxLength, customsOffice);
				command.AddParameter("@houseBillNumber", SqlDbType.VarChar, JobDeclarationSchema.JE_HouseBill.MaxLength, houseBillNo);
				command.AddParameter("@jobNumber", SqlDbType.VarChar, JobDeclarationSchema.JE_DeclarationReference.MaxLength, jobNo);
				command.AddParameter("@masterBillNumber", SqlDbType.VarChar, JobDeclarationSchema.JE_MasterBill.MaxLength, masterBillNo);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branchKey);
				command.AddParameter("@importerKey", SqlDbType.BigInt, importerKey);
				command.AddParameter("@declarantAddressKey", SqlDbType.BigInt, declarantAddressKey);
				command.AddParameter("@addInfo", SqlDbType.VarChar, JobDeclarationSchema.JE_AddInfo.MaxLength, addInfo);
				declarationKey = (long)command.ExecuteScalar();
			}
			return (declarationPK, declarationKey);
		}

		public static (Guid, long) CreateCusEntryInstruction(string dataModel, long declarationKey, string description, string style, string addInfo)
		{
			var cusEntryInstructionKey = 0L;
			var cusEntryInstructionPK = Guid.NewGuid();
			var sql = $@"
DECLARE @CusEntryInstructionKey BIGINT
SELECT @CusEntryInstructionKey = ISNULL(MAX(CusEntryInstructionKey), 0) + 1
FROM {Db.EdwDatabaseName}.Customs.BAS__CusEntryInstruction

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__CusEntryInstruction (CusEntryInstructionID, CusEntryInstructionKey, DataModel, DeclarationKey, Description, Style, AddInfo)
VALUES (@cusEntryInstructionPK, @CusEntryInstructionKey, @dataModel, @declarationKey, @description, @style, @addInfo)

SELECT @CusEntryInstructionKey";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryInstructionPK", SqlDbType.UniqueIdentifier, cusEntryInstructionPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, CusEntryInstructionSchema.CEI_DataModel.MaxLength, dataModel);
				command.AddParameter("@declarationKey", SqlDbType.BigInt, declarationKey);
				command.AddParameter("@description", SqlDbType.NVarChar, CusEntryInstructionSchema.CEI_Description.MaxLength, description);
				command.AddParameter("@style", SqlDbType.VarChar, CusEntryInstructionSchema.CEI_Style.MaxLength, style);
				command.AddParameter("@addInfo", SqlDbType.VarChar, CusEntryInstructionSchema.CEI_AddInfo.MaxLength, addInfo);
				cusEntryInstructionKey = (long)command.ExecuteScalar();
			}
			return (cusEntryInstructionPK, cusEntryInstructionKey);
		}

		public static (Guid, long) CreateOrganisationAddress(long organizationKey, string code, string address1)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @OrganizationAddressKey BIGINT
SELECT @OrganizationAddressKey = ISNULL(MAX(OrganizationAddressKey), 0) + 1
FROM {Db.EdwDatabaseName}.Organization.BAS__OrganizationAddress

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__OrganizationAddress(OrganizationAddressID, OrganizationAddressKey, OrganizationKey, Code, Address1)
VALUES (@organizationAddressId, @OrganizationAddressKey, @organizationKey, @code, @address1)

SELECT @OrganizationAddressKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@organizationAddressId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@organizationKey", SqlDbType.BigInt, organizationKey);
				command.AddParameter("@code", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, code);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateShipment(string shipmentNumber, string transportMode, string packingMode, string origin, string destination, DateTime departureDate, DateTime arrivalDate, decimal actualChargeable)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @ShipmentKey BIGINT
SELECT @ShipmentKey = ISNULL(MAX(ShipmentKey), 0) + 1
FROM {Db.EdwDatabaseName}.InternationalLogistics.BAS__Shipment

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__Shipment(ShipmentID, ShipmentKey, JobNumber, TransportMode, PackingMode, PortOfOrigin, PortOfDestination, DepartureDate, ArrivalDate, ActualChargeable, IsForwardRegistered, IsCancelled)
VALUES (@shipmentId, @ShipmentKey, @shipmentNumber, @transportMode, @packingMode, @origin, @destination, @departureDate, @arrivalDate, @actualChargeable, 1, 0)

SELECT @ShipmentKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@shipmentId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@shipmentNumber", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, shipmentNumber);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobShipmentSchema.JS_TransportMode.MaxLength, transportMode);
				command.AddParameter("@packingMode", SqlDbType.VarChar, JobShipmentSchema.JS_PackingMode.MaxLength, packingMode);
				command.AddParameter("@origin", SqlDbType.VarChar, JobShipmentSchema.JS_RL_NKOrigin.MaxLength, origin);
				command.AddParameter("@destination", SqlDbType.VarChar, JobShipmentSchema.JS_RL_NKDestination.MaxLength, destination);
				command.AddParameter("@departureDate", SqlDbType.SmallDateTime, departureDate);
				command.AddParameter("@arrivalDate", SqlDbType.SmallDateTime, arrivalDate);
				command.AddParameter("@actualChargeable", SqlDbType.Decimal, JobShipmentSchema.JS_ActualChargeable.MaxLength, actualChargeable);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateDocumentAndCartage(long shipmentKey, DateTime lclAvailableDate)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @DocumentAndCartageKey BIGINT
SELECT @DocumentAndCartageKey = ISNULL(MAX(DocumentAndCartageKey), 0) + 1
FROM {Db.EdwDatabaseName}.Customs.BAS__DocumentAndCartage

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__DocumentAndCartage(DocumentAndCartageID, DocumentAndCartageKey, ParentShipmentKey, LCLAvailable)
VALUES (@documentAndCartageId, @DocumentAndCartageKey, @shipmentKey, @lclAvailableDate)

SELECT @DocumentAndCartageKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@documentAndCartageId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@shipmentKey", SqlDbType.BigInt, shipmentKey);
				command.AddParameter("@lclAvailableDate", SqlDbType.SmallDateTime, lclAvailableDate);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateJobVoyage()
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @JobVoyageKey BIGINT
SELECT @JobVoyageKey = ISNULL(MAX(JobVoyageKey), 0) + 1
FROM {Db.EdwDatabaseName}.Sailing.BAS__JobVoyage

INSERT INTO {Db.EdwDatabaseName}.Sailing.BAS__JobVoyage(JobVoyageID, JobVoyageKey)
VALUES (@jobVoyageId, @JobVoyageKey)

SELECT @JobVoyageKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobVoyageId", SqlDbType.UniqueIdentifier, id);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateJobVoyOrigin(long jobVoyageKey)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @JobVoyOriginKey BIGINT
SELECT @JobVoyOriginKey = ISNULL(MAX(JobVoyOriginKey), 0) + 1
FROM {Db.EdwDatabaseName}.Sailing.BAS__JobVoyOrigin

INSERT INTO {Db.EdwDatabaseName}.Sailing.BAS__JobVoyOrigin(JobVoyOriginID, JobVoyOriginKey, JobVoyageKey)
VALUES (@jobVoyOriginId, @JobVoyOriginKey, @jobVoyageKey)

SELECT @JobVoyOriginKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobVoyOriginId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@jobVoyageKey", SqlDbType.BigInt, jobVoyageKey);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateJobVoyDestination(DateTime availablityDate)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @JobVoyDestinationKey BIGINT
SELECT @JobVoyDestinationKey = ISNULL(MAX(JobVoyDestinationKey), 0) + 1
FROM {Db.EdwDatabaseName}.Sailing.BAS__JobVoyDestination

INSERT INTO {Db.EdwDatabaseName}.Sailing.BAS__JobVoyDestination(JobVoyDestinationID, JobVoyDestinationKey, AvailabilityDate)
VALUES (@jobVoyDestinationId, @JobVoyDestinationKey, @availablityDate)

SELECT @JobVoyDestinationKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobVoyDestinationId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@availablityDate", SqlDbType.SmallDateTime, availablityDate);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateJobSailing(long originKey, long destinationKey, DateTime availablityDate)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @JobSailingKey BIGINT
SELECT @JobSailingKey = ISNULL(MAX(JobSailingKey), 0) + 1
FROM {Db.EdwDatabaseName}.Sailing.BAS__JobSailing

INSERT INTO {Db.EdwDatabaseName}.Sailing.BAS__JobSailing(JobSailingID, JobSailingKey, JobVoyOriginKey, JobVoyDestinationKey, DepotAvailabilityDate)
VALUES (@jobSailingId, @JobSailingKey, @originKey, @destinationKey, @availablityDate)

SELECT @JobSailingKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobSailingId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@originKey", SqlDbType.BigInt, originKey);
				command.AddParameter("@destinationKey", SqlDbType.BigInt, destinationKey);
				command.AddParameter("@availablityDate", SqlDbType.SmallDateTime, availablityDate);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateConsolidation(string jobNumber, string loadPort, string dischargePort)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @ConsolidationKey BIGINT
SELECT @ConsolidationKey = ISNULL(MAX(ConsolidationKey), 0) + 1
FROM {Db.EdwDatabaseName}.InternationalLogistics.BAS__Consolidation

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__Consolidation(ConsolidationID, ConsolidationKey, JobNumber, LoadPort, DischargePort)
VALUES (@consolidationId, @ConsolidationKey, @jobNumber, @loadPort, @dischargePort)

SELECT @ConsolidationKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@consolidationId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@jobNumber", SqlDbType.VarChar, JobConsolSchema.JK_UniqueConsignRef.MaxLength, jobNumber);
				command.AddParameter("@loadPort", SqlDbType.VarChar, JobConsolSchema.JK_RL_NKLoadPort.MaxLength, loadPort);
				command.AddParameter("@dischargePort", SqlDbType.VarChar, JobConsolSchema.JK_RL_NKDischargePort.MaxLength, dischargePort);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static long CreateShipmentMainConsol(Guid consolidationId, Guid shipmentId)
		{
			var key = 0L;
			var sql = $@"
DECLARE @ConsolidationShipmentPivotKey BIGINT
SELECT @ConsolidationShipmentPivotKey = ISNULL(MAX(ConsolidationShipmentPivotKey), 0) + 1
FROM {Db.EdwDatabaseName}.InternationalLogistics.GRP__ShipmentMainConsol

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.GRP__ShipmentMainConsol(ConsolidationShipmentPivotKey, ConsolidationID, ShipmentID)
VALUES (@ConsolidationShipmentPivotKey, @consolidationId, @shipmentId)

SELECT @ConsolidationShipmentPivotKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@consolidationId", SqlDbType.UniqueIdentifier, consolidationId);
				command.AddParameter("@shipmentId", SqlDbType.UniqueIdentifier, shipmentId);
				key = (long)command.ExecuteScalar();
			}

			return key;
		}

		public static (Guid, long) CreateConsolAndShipmentTransport(long consolidationKey, Guid consolidationId, long jobSailingKey, string parentType, DateTime etd, DateTime eta, int legOrder)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @ConsolAndShipmentTransportKey BIGINT
SELECT @ConsolAndShipmentTransportKey = ISNULL(MAX(ConsolAndShipmentTransportKey), 0) + 1
FROM {Db.EdwDatabaseName}.InternationalLogistics.BAS__ConsolAndShipmentTransport

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__ConsolAndShipmentTransport(ConsolAndShipmentTransportID, ConsolAndShipmentTransportKey, ConsolidationKey, JobSailingKey, ParentType, ParentGUID, ETD, ETA, LegOrder)
VALUES (@consolAndShipmentTransportId, @ConsolAndShipmentTransportKey, @consolidationKey, @jobSailingKey, @parentType, @consolidationId, @etd, @eta, @legOrder)

SELECT @ConsolAndShipmentTransportKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@consolAndShipmentTransportId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@consolidationId", SqlDbType.UniqueIdentifier, consolidationId);
				command.AddParameter("@consolidationKey", SqlDbType.BigInt, consolidationKey);
				command.AddParameter("@jobSailingKey", SqlDbType.BigInt, jobSailingKey);
				command.AddParameter("@parentType", SqlDbType.VarChar, JobConsolTransportSchema.JW_ParentType.MaxLength, parentType);
				command.AddParameter("@etd", SqlDbType.SmallDateTime, etd);
				command.AddParameter("@eta", SqlDbType.SmallDateTime, eta);
				command.AddParameter("@legOrder", SqlDbType.TinyInt, legOrder);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateHVLVConsignment()
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @HVLVConsignmentKey BIGINT
SELECT @HVLVConsignmentKey = ISNULL(MAX(HVLVConsignmentKey), 0) + 1
FROM {Db.EdwDatabaseName}.ETail.BAS__HVLVConsignment

INSERT INTO {Db.EdwDatabaseName}.ETail.BAS__HVLVConsignment(HVLVConsignmentID, HVLVConsignmentKey)
VALUES (@hvlvConsignmentId, @HVLVConsignmentKey)

SELECT @HVLVConsignmentKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@hvlvConsignmentId", SqlDbType.UniqueIdentifier, id);
				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateHVLVItem(long consignmentKey, long shipmentKey, Guid shipmentId, string status)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @HVLVItemKey BIGINT
SELECT @HVLVItemKey = ISNULL(MAX(HVLVItemKey), 0) + 1
FROM {Db.EdwDatabaseName}.ETail.BAS__HVLVItem

INSERT INTO {Db.EdwDatabaseName}.ETail.BAS__HVLVItem(HVLVItemID, HVLVItemKey, ConsignmentKey, ShipmentKey, ShipmentID, Status)
VALUES (@hvlvItemId, @HVLVItemKey, @consignmentKey, @shipmentKey, @shipmentId, @status)

SELECT @HVLVItemKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@hvlvItemId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@shipmentId", SqlDbType.UniqueIdentifier, shipmentId);
				command.AddParameter("@shipmentKey", SqlDbType.BigInt, shipmentKey);
				command.AddParameter("@consignmentKey", SqlDbType.BigInt, consignmentKey);
				command.AddParameter("@status", SqlDbType.VarChar, HVLVItemSchema.HVI_Status.MaxLength, status);

				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateDocAddress(long addressKey, Guid shipmentId, string addressType)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @DocAddressKey BIGINT
SELECT @DocAddressKey = ISNULL(MAX(DocAddressKey), 0) + 1
FROM {Db.EdwDatabaseName}.InternationalLogistics.BAS__DocAddress

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__DocAddress(DocAddressID, DocAddressKey, OrganizationAddressKey, ParentID, AddressType, AddressSequence, AddressOverride, ParentTableCode)
VALUES (@docAddressId, @DocAddressKey, @addressKey, @shipmentId, @addressType, 0, 0, 'JS')

SELECT @DocAddressKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@docAddressId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@shipmentId", SqlDbType.UniqueIdentifier, shipmentId);
				command.AddParameter("@addressKey", SqlDbType.BigInt, addressKey);
				command.AddParameter("@addressType", SqlDbType.VarChar, JobDocAddressSchema.E2_AddressType.MaxLength, addressType);

				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}

		public static (Guid, long) CreateJobHeader(string jobNum, Guid shipmentId, long localClientAddress, Guid companyId)
		{
			var key = 0L;
			var id = Guid.NewGuid();
			var sql = $@"
DECLARE @JobHeaderKey BIGINT
SELECT @JobHeaderKey = ISNULL(MAX(JobHeaderKey), 0) + 1
FROM {Db.EdwDatabaseName}.Finance.BAS__JobHeader

INSERT INTO {Db.EdwDatabaseName}.Finance.BAS__JobHeader(JobHeaderID, JobHeaderKey, ParentTableCode, ParentID, JobNo, CompanyID, LocalAgentAddressKey)
VALUES (@jobHeaderId, @JobHeaderKey, 'JS', @shipmentId, @jobNum, @companyId, @localClientAddress)

SELECT @JobHeaderKey";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobHeaderId", SqlDbType.UniqueIdentifier, id);
				command.AddParameter("@jobNum", SqlDbType.VarChar, JobHeaderSchema.JH_JobNum.MaxLength, jobNum);
				command.AddParameter("@shipmentId", SqlDbType.UniqueIdentifier, shipmentId);
				command.AddParameter("@localClientAddress", SqlDbType.BigInt, localClientAddress);
				command.AddParameter("@companyId", SqlDbType.UniqueIdentifier, companyId);

				key = (long)command.ExecuteScalar();
			}

			return (id, key);
		}
	}
}
