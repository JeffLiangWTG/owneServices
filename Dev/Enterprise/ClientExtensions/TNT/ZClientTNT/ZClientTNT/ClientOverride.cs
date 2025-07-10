using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using Enterprise.Client.TNT;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Client.TNT.DocWrappers;
using Enterprise.Client.TNT.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		#region Instance

		public static ClientOverride Instance
		{
			get { return fInstance ?? (fInstance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride fInstance;

		#endregion

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			TNTAirCargoShipmentMenu.RegisterThisTypeOverride();
			TNTAirCargoMasterMenu.RegisterThisTypeOverride();
			TNTHawb.RegisterThisSubTypeOverride();
			TNTDocForwardingShipment.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.TNT; }
		}

		public override string ClientDisplayName
		{
			get { return "TNT"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return TNTDataRegistry.Instance; }
		}

		#region ControllerOverrides

		protected override ControllerOverrides GetControllerOverrides()
		{
			var controllerOverrides = new ControllerOverrides();

			ClientOverrideControllerID aUAirCargoID = new ClientOverrideControllerID(ControllerIDs.Customs.AU.AirCargo);
			ClientOverrideControllerInfo aUAirCargoControllerInfo = new ClientOverrideControllerInfo(
				aUAirCargoID, typeof(TNTAUCustomsAirCargoController).Assembly.FullName,
				typeof(TNTAUCustomsAirCargoController).FullName, Core.Constants.CountryCodes.Australia);
			controllerOverrides.AddControllerOverride(aUAirCargoControllerInfo);
			return controllerOverrides;
		}

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier consolID = new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol);
			ClientOverrideModuleInfo consolModuleInfo = new ClientOverrideModuleInfo(
				consolID, typeof(TNTConsolModuleOverride).Assembly.FullName,
				typeof(TNTConsolModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(consolModuleInfo);

			ClientOverrideModuleIdentifier aUAirCargoID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.AU.AirCargo);
			ClientOverrideModuleInfo aUAirCargoModuleInfo = new ClientOverrideModuleInfo(
				aUAirCargoID, typeof(TNTAirCargoModuleOverride).Assembly.FullName,
				typeof(TNTAirCargoModuleOverride).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(aUAirCargoModuleInfo);
			return moduleOverrides;
		}

		#endregion

		#region ClientDbSchemaUpgradeInfo

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		#region TableCreationScripts

		static ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray.Create(
			new DatabaseObjectCreateScript("ClientTntMawbInterface", CreateClientTntMawbInterface, DropClientTntMawbInterface)
		);

		#endregion

		#region ViewAndRoutinesCreationScripts

		static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts => ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("ClientTNT_DeleteSingleUnusedDeclaration", CreateClientTNT_DeleteSingleUnusedDeclaration(), DropClientTNT_DeleteSingleUnusedDeclaration, DbRoutineType.SqlProcedureTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientTNT_DeleteUnusedDeclarations", CreateClientTNT_DeleteUnusedDeclarations, DropClientTNT_DeleteUnusedDeclarations, DbRoutineType.SqlProcedureTypeDesc)
		);

		#endregion

		#region TNT Mawb Interface
		const string DropClientTntMawbInterface = "DROP TABLE ClientTntMawbInterface";
		const string CreateClientTntMawbInterface = @"
			CREATE TABLE ClientTntMawbInterface
			(
				T9_PK                uniqueIdentifier NOT NULL DEFAULT NEWID(),
				T9_MasterBill        varchar(20)      NOT NULL,
				T9_ConsolRef         varchar(35)      NOT NULL DEFAULT '',
				T9_Flight            varchar(10)      NOT NULL DEFAULT '',
				T9_Etd               varchar(20)      NOT NULL DEFAULT '',
				T9_PortOfLoading     varchar(10)      NOT NULL DEFAULT '',
				T9_PortOfDischarge   varchar(10)      NOT NULL DEFAULT '',
				T9_UploadedShipments int              NOT NULL DEFAULT 0,
				T9_UploadTime        datetime         NOT NULL DEFAULT getdate(),
				CONSTRAINT PK_UX__T9_PK PRIMARY KEY NONCLUSTERED (T9_PK)
			);
			CREATE UNIQUE INDEX NR_UX__T9_MasterBill_T9_ConsolRef ON ClientTntMawbInterface (T9_MasterBill, T9_ConsolRef);
			";
		#endregion

		#region Deleting Unused Declaration Store Procedure

		#region ClientTNT_DeleteSingleUnusedDeclaration
		const string DropClientTNT_DeleteSingleUnusedDeclaration = "DROP PROCEDURE ClientTNT_DeleteSingleUnusedDeclaration";
		static string CreateClientTNT_DeleteSingleUnusedDeclaration()
		{
			return @$"
CREATE PROCEDURE ClientTNT_DeleteSingleUnusedDeclaration
	@DeclarationRef uniqueidentifier
AS
BEGIN
	SET NOCOUNT OFF
	IF @DeclarationRef is null BEGIN
		PRINT '@DeclarationRef cannot be null'
	END

	IF (OBJECT_ID('tempdb..#JobComInvoiceHeadertemp') IS NOT NULL) BEGIN
		DROP TABLE #JobComInvoiceHeadertemp 
	END
	CREATE TABLE #JobComInvoiceHeadertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusContainertemp') IS NOT NULL) BEGIN
		DROP TABLE #CusContainertemp 
	END
	CREATE TABLE #CusContainertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusDecHouseBilltemp') IS NOT NULL) BEGIN
		DROP TABLE #CusDecHouseBilltemp 
	END
	CREATE TABLE #CusDecHouseBilltemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryCPDectemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryCPDectemp 
	END
	CREATE TABLE #CusEntryCPDectemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryHeadertemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryHeadertemp 
	END
	CREATE TABLE #CusEntryHeadertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusHAWBtemp') IS NOT NULL) BEGIN
		DROP TABLE #CusHAWBtemp 
	END
	CREATE TABLE #CusHAWBtemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobDecRefstemp') IS NOT NULL) BEGIN
		DROP TABLE #JobDecRefstemp 
	END
	CREATE TABLE #JobDecRefstemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobOrderHeadertemp') IS NOT NULL) BEGIN
		DROP TABLE #JobOrderHeadertemp 
	END
	CREATE TABLE #JobOrderHeadertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobComInvoiceLinetemp') IS NOT NULL) BEGIN
		DROP TABLE #JobComInvoiceLinetemp 
	END
	CREATE TABLE #JobComInvoiceLinetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#QuarantineExDocHeadertemp') IS NOT NULL) BEGIN
		DROP TABLE #QuarantineExDocHeadertemp 
	END
	CREATE TABLE #QuarantineExDocHeadertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobComInvoiceHeaderRefstemp') IS NOT NULL) BEGIN
		DROP TABLE #JobComInvoiceHeaderRefstemp 
	END
	CREATE TABLE #JobComInvoiceHeaderRefstemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusContainerInvoiceLinePivottemp') IS NOT NULL) BEGIN
		DROP TABLE #CusContainerInvoiceLinePivottemp 
	END
	CREATE TABLE #CusContainerInvoiceLinePivottemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusDecHouseContainerPivottemp') IS NOT NULL) BEGIN
		DROP TABLE #CusDecHouseContainerPivottemp 
	END
	CREATE TABLE #CusDecHouseContainerPivottemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryLinetemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryLinetemp 
	END
	CREATE TABLE #CusEntryLinetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryPayInfotemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryPayInfotemp 
	END
	CREATE TABLE #CusEntryPayInfotemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryHeaderChargestemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryHeaderChargestemp 
	END
	CREATE TABLE #CusEntryHeaderChargestemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobOrderLinetemp') IS NOT NULL) BEGIN
		DROP TABLE #JobOrderLinetemp 
	END
	CREATE TABLE #JobOrderLinetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobOrderContainertemp') IS NOT NULL) BEGIN
		DROP TABLE #JobOrderContainertemp 
	END
	CREATE TABLE #JobOrderContainertemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobComInvLineRefstemp') IS NOT NULL) BEGIN
		DROP TABLE #JobComInvLineRefstemp 
	END
	CREATE TABLE #JobComInvLineRefstemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#QuarantineExDocLinetemp') IS NOT NULL) BEGIN
		DROP TABLE #QuarantineExDocLinetemp 
	END
	CREATE TABLE #QuarantineExDocLinetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#QuarantineExDocShipsCompartmenttemp') IS NOT NULL) BEGIN
		DROP TABLE #QuarantineExDocShipsCompartmenttemp 
	END
	CREATE TABLE #QuarantineExDocShipsCompartmenttemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusDecHouseContainerPacktemp') IS NOT NULL) BEGIN
		DROP TABLE #CusDecHouseContainerPacktemp 
	END
	CREATE TABLE #CusDecHouseContainerPacktemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#CusEntryLineFeetemp') IS NOT NULL) BEGIN
		DROP TABLE #CusEntryLineFeetemp 
	END
	CREATE TABLE #CusEntryLineFeetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobOrderLineDeliverytemp') IS NOT NULL) BEGIN
		DROP TABLE #JobOrderLineDeliverytemp 
	END
	CREATE TABLE #JobOrderLineDeliverytemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#QuarantineExDocEstablishmentAndTimetemp') IS NOT NULL) BEGIN
		DROP TABLE #QuarantineExDocEstablishmentAndTimetemp 
	END
	CREATE TABLE #QuarantineExDocEstablishmentAndTimetemp(PK uniqueidentifier) 
	
	IF (OBJECT_ID('tempdb..#JobOrderLineDeliverContainertemp') IS NOT NULL) BEGIN
		DROP TABLE #JobOrderLineDeliverContainertemp 
	END
	CREATE TABLE #JobOrderLineDeliverContainertemp(PK uniqueidentifier) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceHeadertemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceHeadertemp (PK) 
		SELECT JZ_PK 
		FROM dbo.JobComInvoiceHeader
		WHERE JZ_JE = @DeclarationRef AND JZ_PK NOT IN (SELECT PK FROM #JobComInvoiceHeadertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusContainertemp with PKs of affected rows' 
	INSERT INTO #CusContainertemp (PK) 
		SELECT CO_PK 
		FROM dbo.CusContainer
		WHERE CO_JE = @DeclarationRef AND CO_PK NOT IN (SELECT PK FROM #CusContainertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusDecHouseBilltemp with PKs of affected rows' 
	INSERT INTO #CusDecHouseBilltemp (PK) 
		SELECT CU_PK 
		FROM dbo.CusDecHouseBill
		WHERE CU_JE = @DeclarationRef AND CU_PK NOT IN (SELECT PK FROM #CusDecHouseBilltemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryCPDectemp with PKs of affected rows' 
	INSERT INTO #CusEntryCPDectemp (PK) 
		SELECT ON_PK 
		FROM dbo.CusEntryCPDec
		WHERE ON_JE = @DeclarationRef AND ON_PK NOT IN (SELECT PK FROM #CusEntryCPDectemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryHeadertemp with PKs of affected rows' 
	INSERT INTO #CusEntryHeadertemp (PK) 
		SELECT CH_PK 
		FROM dbo.CusEntryHeader
		WHERE CH_JE = @DeclarationRef AND CH_PK NOT IN (SELECT PK FROM #CusEntryHeadertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusHAWBtemp with PKs of affected rows' 
	INSERT INTO #CusHAWBtemp (PK) 
		SELECT CS_PK 
		FROM dbo.CusHAWB
		WHERE CS_JE_CustomsFormalEntry = @DeclarationRef AND CS_PK NOT IN (SELECT PK FROM #CusHAWBtemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobDecRefstemp with PKs of affected rows' 
	INSERT INTO #JobDecRefstemp (PK) 
		SELECT J3_PK 
		FROM dbo.JobDecRefs
		WHERE J3_JE = @DeclarationRef AND J3_PK NOT IN (SELECT PK FROM #JobDecRefstemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobOrderHeadertemp with PKs of affected rows' 
	INSERT INTO #JobOrderHeadertemp (PK) 
		SELECT JD_PK 
		FROM dbo.JobOrderHeader
		WHERE JD_JE = @DeclarationRef AND JD_PK NOT IN (SELECT PK FROM #JobOrderHeadertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceLinetemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceLinetemp (PK) 
		SELECT JI_PK 
		FROM dbo.JobComInvoiceLine
		WHERE JI_JZ IN (SELECT PK FROM #JobComInvoiceHeadertemp) AND JI_PK NOT IN (SELECT PK FROM #JobComInvoiceLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #QuarantineExDocHeadertemp with PKs of affected rows' 
	INSERT INTO #QuarantineExDocHeadertemp (PK) 
		SELECT QH_PK 
		FROM dbo.QuarantineExDocHeader
		WHERE QH_JZ IN (SELECT PK FROM #JobComInvoiceHeadertemp) AND QH_PK NOT IN (SELECT PK FROM #QuarantineExDocHeadertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceHeaderRefstemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceHeaderRefstemp (PK) 
		SELECT J2_PK 
		FROM dbo.JobComInvoiceHeaderRefs
		WHERE J2_JZ IN (SELECT PK FROM #JobComInvoiceHeadertemp) AND J2_PK NOT IN (SELECT PK FROM #JobComInvoiceHeaderRefstemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusContainerInvoiceLinePivottemp with PKs of affected rows' 
	INSERT INTO #CusContainerInvoiceLinePivottemp (PK) 
		SELECT C2_PK 
		FROM dbo.CusContainerInvoiceLinePivot
		WHERE C2_CO IN (SELECT PK FROM #CusContainertemp) AND C2_PK NOT IN (SELECT PK FROM #CusContainerInvoiceLinePivottemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceLinetemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceLinetemp (PK) 
		SELECT JI_PK 
		FROM dbo.JobComInvoiceLine
		WHERE JI_CO IN (SELECT PK FROM #CusContainertemp) AND JI_PK NOT IN (SELECT PK FROM #JobComInvoiceLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusDecHouseContainerPivottemp with PKs of affected rows' 
	INSERT INTO #CusDecHouseContainerPivottemp (PK) 
		SELECT CR_PK 
		FROM dbo.CusDecHouseContainerPivot
		WHERE CR_CO_Container IN (SELECT PK FROM #CusContainertemp) AND CR_PK NOT IN (SELECT PK FROM #CusDecHouseContainerPivottemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusDecHouseContainerPivottemp with PKs of affected rows' 
	INSERT INTO #CusDecHouseContainerPivottemp (PK) 
		SELECT CR_PK 
		FROM dbo.CusDecHouseContainerPivot
		WHERE CR_CU_HouseBill IN (SELECT PK FROM #CusDecHouseBilltemp) AND CR_PK NOT IN (SELECT PK FROM #CusDecHouseContainerPivottemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceHeadertemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceHeadertemp (PK) 
		SELECT JZ_PK 
		FROM dbo.JobComInvoiceHeader
		WHERE JZ_CU_RelatedHouseBill IN (SELECT PK FROM #CusDecHouseBilltemp) AND JZ_PK NOT IN (SELECT PK FROM #JobComInvoiceHeadertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryLinetemp with PKs of affected rows' 
	INSERT INTO #CusEntryLinetemp (PK) 
		SELECT CL_PK 
		FROM dbo.CusEntryLine
		WHERE CL_CH IN (SELECT PK FROM #CusEntryHeadertemp) AND CL_PK NOT IN (SELECT PK FROM #CusEntryLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryCPDectemp with PKs of affected rows' 
	INSERT INTO #CusEntryCPDectemp (PK) 
		SELECT ON_PK 
		FROM dbo.CusEntryCPDec
		WHERE ON_CH IN (SELECT PK FROM #CusEntryHeadertemp) AND ON_PK NOT IN (SELECT PK FROM #CusEntryCPDectemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryPayInfotemp with PKs of affected rows' 
	INSERT INTO #CusEntryPayInfotemp (PK) 
		SELECT C9_PK 
		FROM dbo.CusEntryPayInfo
		WHERE C9_CH IN (SELECT PK FROM #CusEntryHeadertemp) AND C9_PK NOT IN (SELECT PK FROM #CusEntryPayInfotemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryHeaderChargestemp with PKs of affected rows' 
	INSERT INTO #CusEntryHeaderChargestemp (PK) 
		SELECT C1_PK 
		FROM dbo.CusEntryHeaderCharges
		WHERE C1_CH IN (SELECT PK FROM #CusEntryHeadertemp) AND C1_PK NOT IN (SELECT PK FROM #CusEntryHeaderChargestemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobOrderLinetemp with PKs of affected rows' 
	INSERT INTO #JobOrderLinetemp (PK) 
		SELECT JO_PK 
		FROM dbo.JobOrderLine
		WHERE JO_JD IN (SELECT PK FROM #JobOrderHeadertemp) AND JO_PK NOT IN (SELECT PK FROM #JobOrderLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobOrderContainertemp with PKs of affected rows' 
	INSERT INTO #JobOrderContainertemp (PK) 
		SELECT J1_PK 
		FROM dbo.JobOrderContainer
		WHERE J1_ParentID IN (SELECT PK FROM #JobOrderHeadertemp) AND J1_ParentTableCode = 'JD' AND J1_PK NOT IN (SELECT PK FROM #JobOrderContainertemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvLineRefstemp with PKs of affected rows' 
	INSERT INTO #JobComInvLineRefstemp (PK) 
		SELECT JG_PK 
		FROM dbo.JobComInvLineRefs
		WHERE JG_JI IN (SELECT PK FROM #JobComInvoiceLinetemp) AND JG_PK NOT IN (SELECT PK FROM #JobComInvLineRefstemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #QuarantineExDocLinetemp with PKs of affected rows' 
	INSERT INTO #QuarantineExDocLinetemp (PK) 
		SELECT QL_PK 
		FROM dbo.QuarantineExDocLine
		WHERE QL_JI IN (SELECT PK FROM #JobComInvoiceLinetemp) AND QL_PK NOT IN (SELECT PK FROM #QuarantineExDocLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusContainerInvoiceLinePivottemp with PKs of affected rows' 
	INSERT INTO #CusContainerInvoiceLinePivottemp (PK) 
		SELECT C2_PK 
		FROM dbo.CusContainerInvoiceLinePivot
		WHERE C2_JI IN (SELECT PK FROM #JobComInvoiceLinetemp) AND C2_PK NOT IN (SELECT PK FROM #CusContainerInvoiceLinePivottemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #QuarantineExDocShipsCompartmenttemp with PKs of affected rows' 
	INSERT INTO #QuarantineExDocShipsCompartmenttemp (PK) 
		SELECT QC_PK 
		FROM dbo.QuarantineExDocShipsCompartment
		WHERE QC_QH IN (SELECT PK FROM #QuarantineExDocHeadertemp) AND QC_PK NOT IN (SELECT PK FROM #QuarantineExDocShipsCompartmenttemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusDecHouseContainerPacktemp with PKs of affected rows' 
	INSERT INTO #CusDecHouseContainerPacktemp (PK) 
		SELECT CW_PK 
		FROM dbo.CusDecHouseContainerPack
		WHERE CW_CR_HouseContainer IN (SELECT PK FROM #CusDecHouseContainerPivottemp) AND CW_PK NOT IN (SELECT PK FROM #CusDecHouseContainerPacktemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryCPDectemp with PKs of affected rows' 
	INSERT INTO #CusEntryCPDectemp (PK) 
		SELECT ON_PK 
		FROM dbo.CusEntryCPDec
		WHERE ON_CL IN (SELECT PK FROM #CusEntryLinetemp) AND ON_PK NOT IN (SELECT PK FROM #CusEntryCPDectemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #CusEntryLineFeetemp with PKs of affected rows' 
	INSERT INTO #CusEntryLineFeetemp (PK) 
		SELECT CF_PK 
		FROM dbo.CusEntryLineFee
		WHERE CF_CL IN (SELECT PK FROM #CusEntryLinetemp) AND CF_PK NOT IN (SELECT PK FROM #CusEntryLineFeetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceLinetemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceLinetemp (PK) 
		SELECT JI_PK 
		FROM dbo.JobComInvoiceLine
		WHERE JI_CL IN (SELECT PK FROM #CusEntryLinetemp) AND JI_PK NOT IN (SELECT PK FROM #JobComInvoiceLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobComInvoiceLinetemp with PKs of affected rows' 
	INSERT INTO #JobComInvoiceLinetemp (PK) 
		SELECT JI_PK 
		FROM dbo.JobComInvoiceLine
		WHERE JI_JO IN (SELECT PK FROM #JobOrderLinetemp) AND JI_PK NOT IN (SELECT PK FROM #JobComInvoiceLinetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobOrderLineDeliverytemp with PKs of affected rows' 
	INSERT INTO #JobOrderLineDeliverytemp (PK) 
		SELECT J4_PK 
		FROM dbo.JobOrderLineDelivery
		WHERE J4_JO IN (SELECT PK FROM #JobOrderLinetemp) AND J4_PK NOT IN (SELECT PK FROM #JobOrderLineDeliverytemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #QuarantineExDocEstablishmentAndTimetemp with PKs of affected rows' 
	INSERT INTO #QuarantineExDocEstablishmentAndTimetemp (PK) 
		SELECT EE_PK 
		FROM dbo.QuarantineExDocEstablishmentAndTime
		WHERE EE_QL IN (SELECT PK FROM #QuarantineExDocLinetemp) AND EE_PK NOT IN (SELECT PK FROM #QuarantineExDocEstablishmentAndTimetemp) 
	
	PRINT cast(GetDate() as varchar) + ' Updating Temp Table #JobOrderLineDeliverContainertemp with PKs of affected rows' 
	INSERT INTO #JobOrderLineDeliverContainertemp (PK) 
		SELECT J5_PK 
		FROM dbo.JobOrderLineDeliverContainer
		WHERE J5_J4 IN (SELECT PK FROM #JobOrderLineDeliverytemp) AND J5_PK NOT IN (SELECT PK FROM #JobOrderLineDeliverContainertemp) 
	
	BEGIN TRANSACTION
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.QuarantineExDocEstablishmentAndTime'
	DELETE dbo.QuarantineExDocEstablishmentAndTime 
	WHERE EE_PK IN (SELECT PK FROM #QuarantineExDocEstablishmentAndTimetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobComInvLineRefs'
	DELETE dbo.JobComInvLineRefs 
	WHERE JG_PK IN (SELECT PK FROM #JobComInvLineRefstemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.QuarantineExDocLine'
	DELETE dbo.QuarantineExDocLine 
	WHERE QL_PK IN (SELECT PK FROM #QuarantineExDocLinetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusContainerInvoiceLinePivot'
	DELETE dbo.CusContainerInvoiceLinePivot 
	WHERE C2_PK IN (SELECT PK FROM #CusContainerInvoiceLinePivottemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.QuarantineExDocShipsCompartment'
	DELETE dbo.QuarantineExDocShipsCompartment 
	WHERE QC_PK IN (SELECT PK FROM #QuarantineExDocShipsCompartmenttemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobOrderLineDeliverContainer'
	DELETE dbo.JobOrderLineDeliverContainer 
	WHERE J5_PK IN (SELECT PK FROM #JobOrderLineDeliverContainertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusDecHouseContainerPack'
	DELETE dbo.CusDecHouseContainerPack 
	WHERE CW_PK IN (SELECT PK FROM #CusDecHouseContainerPacktemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobComInvoiceLine'
	DELETE dbo.JobComInvoiceLine 
	WHERE JI_PK IN (SELECT PK FROM #JobComInvoiceLinetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.QuarantineExDocHeader'
	DELETE dbo.QuarantineExDocHeader 
	WHERE QH_PK IN (SELECT PK FROM #QuarantineExDocHeadertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobComInvoiceHeaderRefs'
	DELETE dbo.JobComInvoiceHeaderRefs 
	WHERE J2_PK IN (SELECT PK FROM #JobComInvoiceHeaderRefstemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryCPDec'
	DELETE dbo.CusEntryCPDec 
	WHERE ON_PK IN (SELECT PK FROM #CusEntryCPDectemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryLineFee'
	DELETE dbo.CusEntryLineFee 
	WHERE CF_PK IN (SELECT PK FROM #CusEntryLineFeetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobOrderLineDelivery'
	DELETE dbo.JobOrderLineDelivery 
	WHERE J4_PK IN (SELECT PK FROM #JobOrderLineDeliverytemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusDecHouseContainerPivot'
	DELETE dbo.CusDecHouseContainerPivot 
	WHERE CR_PK IN (SELECT PK FROM #CusDecHouseContainerPivottemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobComInvoiceHeader'
	DELETE dbo.JobComInvoiceHeader 
	WHERE JZ_PK IN (SELECT PK FROM #JobComInvoiceHeadertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryLine'
	DELETE dbo.CusEntryLine 
	WHERE CL_PK IN (SELECT PK FROM #CusEntryLinetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryPayInfo'
	DELETE dbo.CusEntryPayInfo 
	WHERE C9_PK IN (SELECT PK FROM #CusEntryPayInfotemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryHeaderCharges'
	DELETE dbo.CusEntryHeaderCharges 
	WHERE C1_PK IN (SELECT PK FROM #CusEntryHeaderChargestemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobOrderLine'
	DELETE dbo.JobOrderLine 
	WHERE JO_PK IN (SELECT PK FROM #JobOrderLinetemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobOrderContainer'
	DELETE dbo.JobOrderContainer 
	WHERE J1_PK IN (SELECT PK FROM #JobOrderContainertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusContainer'
	DELETE dbo.CusContainer 
	WHERE CO_PK IN (SELECT PK FROM #CusContainertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusDecHouseBill'
	DELETE dbo.CusDecHouseBill 
	WHERE CU_PK IN (SELECT PK FROM #CusDecHouseBilltemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.CusEntryHeader'
	DELETE dbo.CusEntryHeader 
	WHERE CH_PK IN (SELECT PK FROM #CusEntryHeadertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' UPDATE dbo.CusHAWB''s refences to JobDeclaration'
	UPDATE dbo.CusHAWB 
	SET
		CS_JE_CustomsFormalEntry = NULL,
		CS_SystemLastEditTimeUtc = GETUTCDATE(),
		CS_SystemLastEditUser = '~BP'
	WHERE CS_PK IN (SELECT PK FROM #CusHAWBtemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobDecRefs'
	DELETE dbo.JobDecRefs 
	WHERE J3_PK IN (SELECT PK FROM #JobDecRefstemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobOrderHeader'
	DELETE dbo.JobOrderHeader 
	WHERE JD_PK IN (SELECT PK FROM #JobOrderHeadertemp)
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT cast(GetDate() as varchar) + ' DELETE dbo.JobDeclaration'
	DELETE dbo.JobDeclaration 
	WHERE JE_PK = @DeclarationRef
	IF (@@error != 0)
	BEGIN
		PRINT cast(GetDate() as varchar) + ' Purging Data is aborted as error was encoutered.'
		PRINT 'Begin Rollback changes'
		ROLLBACK
		PRINT 'Rollback Completed'
		RETURN
	END
	
	PRINT 'Purge Completed without errors' 
	PRINT 'Beging committing changes' 
	COMMIT
	PRINT 'Committing changes completed' 
END
";
		}
		#endregion

		#region ClientTNT_DeleteUnusedDeclarations
		const string DropClientTNT_DeleteUnusedDeclarations = "DROP PROCEDURE ClientTNT_DeleteUnusedDeclarations";
		const string CreateClientTNT_DeleteUnusedDeclarations = @"
CREATE PROCEDURE ClientTNT_DeleteUnusedDeclarations
	@JobNumber varchar(35),
	@ConsignorCode nvarchar(24),
	@ConsignorName nvarchar(100),
	@DateFrom DateTime,
	@DateTo DateTime
AS
BEGIN
	SET NOCOUNT OFF

	IF @JobNumber is null
	BEGIN 
		SET @JobNumber = ''
	END

	IF @ConsignorCode is null
	BEGIN 
		SET @ConsignorCode = ''
	END
	
	IF @ConsignorName is null
	BEGIN 
		SET @ConsignorName = ''
	END
	
	IF not (@JobNumber <> '' OR @ConsignorCode <> '' OR @ConsignorName <> '' OR @DateFrom IS NOT NULL OR @DateTO IS NOT NULL) BEGIN
		PRINT 'At least one of the following parameters need to be specified:
JobNumber, CosignorCode, ConsignorName OR DateFrom and DateTo'
		RETURN
	END
	
	IF ((@DateFrom IS NULL AND @DateTO IS NOT NULL) or (@DateFrom IS NOT NULL AND @DateTO IS NULL)) BEGIN
		PRINT 'Not all parameters have been specified:
For Date range, both DateFrom and DateTo needs to be specified'
		RETURN
	END
	
	IF (@DateFrom > @DateTO) BEGIN
		PRINT 'DateFrom cannot be greater than DateTo'
		RETURN
	END
	
	DECLARE @Pk varchar(128)
	DECLARE @DeclarationReference varchar(128)
	DECLARE @SqlText      nvarchar(1000)
	
	PRINT cast(GetDate() as varchar) + ' Selecting all JobDeclartions meeting criteria ' 

	set @SqlText = N'DECLARE JobDeclarationToBeDeleted INSENSITIVE CURSOR FOR
	SELECT JE_PK, JE_DeclarationReference
 	FROM dbo.JobDeclaration 
	WHERE ((SELECT COUNT(*) FROM dbo.StmAlog WHERE SL_Parent = JE_PK AND SL_SE_NKEvent NOT IN (''WTA'') GROUP BY SL_Parent) = 1)
		AND NOT EXISTS (SELECT null FROM dbo.JobHeader WHERE JH_ParentID = JE_PK)'
	IF @JobNumber <> '' BEGIN
		set @SqlText = @SqlText + N'
		AND JE_DeclarationReference = @JobNumber'
	END
	IF @DateFrom IS NOT NULL BEGIN
		set @SqlText = @SqlText + N'
		AND JE_SystemCreateTimeUtc >= @DateFrom'
	END
	IF @DateTo IS NOT NULL BEGIN
		set @SqlText = @SqlText + N'
		AND JE_SystemCreateTimeUtc <= @DateTo'
	END
	IF @ConsignorCode <> '' BEGIN
		set @SqlText = @SqlText + N'
		AND JE_OH_Supplier IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = @ConsignorCode)'
	END
	IF @ConsignorName <> '' BEGIN
		set @SqlText = @SqlText + N'
		AND JE_OH_Supplier IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_FullName = @ConsignorName)'
	END

	EXEC sp_executesql @SqlText, N'@JobNumber varchar(35), @ConsignorCode nvarchar(24), @ConsignorName nvarchar(100), @DateFrom DateTime, @DateTo DateTime',
	@JobNumber, @ConsignorCode, @ConsignorName, @DateFrom, @DateTo

	OPEN JobDeclarationToBeDeleted
	FETCH NEXT FROM JobDeclarationToBeDeleted INTO @Pk, @DeclarationReference
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @SqlText = 'ClientTNT_DeleteSingleUnusedDeclaration ''' + @Pk + ''''
		PRINT 'Starting Deleting JobDeclaration ' + @DeclarationReference
		EXEC sp_executesql @SqlText
		FETCH NEXT FROM JobDeclarationToBeDeleted INTO @Pk, @DeclarationReference
	END
	CLOSE JobDeclarationToBeDeleted
	DEALLOCATE JobDeclarationToBeDeleted
END";
		#endregion

		#endregion

		#endregion

		public override string HelpWebPage
		{
			get { return ""; }
		}

		#endregion

		public override IEnumerable<ILogSubscriber> LogSubscribers
		{
			get
			{
				if (TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.Value.Count > 0)
				{
					yield return new DeclarationCustomResponseSenderSubscriber();
				}
			}
		}
	}
}
