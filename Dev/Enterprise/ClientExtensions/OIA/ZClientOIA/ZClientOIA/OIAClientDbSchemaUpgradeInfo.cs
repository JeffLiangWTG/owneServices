using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.OIA
{
	internal class OIAClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		public ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray<DatabaseObjectCreateScript>.Empty;

		public ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutineCreationScripts { get; } = ImmutableArray.Create(
			// STORED PROCEDURES
			Client_OIA_GLTransactionsBatch
		);

		#region STORED PROC Client_OIA_GLTransactionsBatch

		/// <summary>
		/// Client Specific Extension to stored procedure GLTransactionsSPBatch, used to data export GL Journal information.
		/// </summary>
		static DatabaseViewAndRoutineCreateScript Client_OIA_GLTransactionsBatch
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_OIA_GLTransactionsBatch", @"
CREATE PROCEDURE Client_OIA_GLTransactionsBatch
	@CompanyPK uniqueidentifier, @StartPeriod int, @EndPeriod int, @StartDate datetime, @EndDate datetime,
	@StartGLAccountPK uniqueidentifier, @EndGLAccountPK uniqueidentifier,
	@BranchPK uniqueidentifier, @DepartmentPK uniqueidentifier, @DisplayDescription char(1),
	@TransactionCategory VARCHAR(100), @BatchNumberToGet int, @BatchNumberToSet int, @IncludeZeroBalance char(1), @IsExportingBatch char(1) = 'N'
AS
BEGIN
CREATE TABLE #OIAGLTRANSACTIONS
(
	PK UNIQUEIDENTIFIER,
	TransactionType Char(3) COLLATE database_default,  
	InvoiceDate				smalldatetime,
	PostDate				smalldatetime,
	DueDate					smalldatetime,
	Branch					CHAR(3)			COLLATE database_default,
	Department				CHAR(3)			COLLATE database_default,
	Ledger					CHAR(3)			COLLATE database_default,
	TransactionNum			VARCHAR(38)		COLLATE database_default,
	SecondRef				VARCHAR(38)		COLLATE database_default,
	TransactionDesc			NVARCHAR(1024)	COLLATE database_default,
	GLAccount				VARCHAR(12)		COLLATE database_default,
	GLAccountDesc			VARCHAR(50)		COLLATE database_default,
	Units					VARCHAR(3)		COLLATE database_default,
	Job						VARCHAR(50)		COLLATE database_default,
	Account					VARCHAR(12)		COLLATE database_default,
	ChargeCode				VARCHAR(10)		COLLATE database_default,
	ChargeCodeDescription	VARCHAR(80)		COLLATE database_default,
	LocalLanguageChargeCodeDescription	NVARCHAR(160) COLLATE database_default,
	Period					INT,
	ReversePeriod			INT,
	Amount					MONEY	DEFAULT 0,
	GSTAmount				MONEY	DEFAULT 0,
	Debit					MONEY,
	Credit					MONEY,
	Balance					MONEY	DEFAULT 0,
	IsLine					BIT		DEFAULT 0,
	TRPK					UNIQUEIDENTIFIER,
	OpeningPeriodDate		datetime,
	ClosingPeriodDate		datetime,
	OpeningBalance			MONEY	DEFAULT 0,
	ClosingBalance			MONEY	DEFAULT 0,
	IsControlTotal			BIT		DEFAULT 0,
	IsRevenueRecognition	BIT		DEFAULT 0,
	BatchNumber				INT,
	BatchType				CHAR(3)			COLLATE database_default,
	SubSelectID				INT,
	ParentTableCode			CHAR(4)			COLLATE database_default,
	PKFORBATCHING			UNIQUEIDENTIFIER,
	OsAmount				MONEY	DEFAULT 0,
	ExRate					MONEY	DEFAULT 1,
	Currency				CHAR(3)			COLLATE database_default,
	OsDebit					MONEY,
	OsCredit				MONEY,
	OsExTaxDebit            MONEY,
	OsExTaxCredit           MONEY,
	MultiSubAccountTypeCode varchar(300),
	OrganisationSubAccount varchar(12),
	SalesExpenseGroupsSubAccount varchar(10),
	StaffAndResourcesSubAccount varchar(3),
	StaffGroupSubAccount	varchar(15),
	ComplianceSubType		CHAR(3),
	ComplianceNumber		VARCHAR(38),
	JournalEntriesNumber	NVARCHAR(40)
)

INSERT INTO #OIAGLTRANSACTIONS EXEC GLTransactionsSP @CompanyPK, @StartPeriod, @EndPeriod, @StartDate, @EndDate, @StartGLAccountPK, @EndGLAccountPK, 
	@BranchPK, @DepartmentPK, @DisplayDescription, @TransactionCategory, @BatchNumberToGet, @BatchNumberToSet, @IncludeZeroBalance, 'N' 

SELECT PK, TransactionType, InvoiceDate, PostDate, DueDate, Branch, Department, Ledger, TransactionNum, SecondRef, TransactionDesc, GLAccount, 
	GLAccountDesc, Job, Account,	ChargeCode, ChargeCodeDescription, LocalLanguageChargeCodeDescription, Period, ReversePeriod, Amount, GSTAmount, 
	Debit, Credit, Balance, IsLine, TRPK, OpeningPeriodDate, ClosingPeriodDate, OpeningBalance, ClosingBalance, IsControlTotal, IsRevenueRecognition, 
	OH_Code as LocalClient, OJ_Code as ARGroup, JP_CustomAttrib1 as Text1, JP_CustomAttrib2 as Text2 from #OIAGLTRANSACTIONS
	Left Join dbo.AccTransactionLines on AccTransactionLines.AL_PK = PK
	Left Join dbo.JobHeader on  JobHeader.JH_PK = AccTransactionLines.AL_JH
	Left Join dbo.OrgAddress on OrgAddress.OA_PK = JobHeader.JH_OA_LocalChargesAddr
	Left Join dbo.OrgHeader on OrgHeader.OH_PK = OrgAddress.OA_OH
	Left Join dbo.JobShipment on JobShipment.JS_PK = JobHeader.JH_ParentID and JobHeader.JH_ParentTableCode = 'JS'
	Left Join dbo.JobDocsAndCartage	on JobDocsAndCartage.JP_ParentID = JobShipment.JS_PK and JobDocsAndCartage.JP_ParentTableCode = 'JS'
	Left Join dbo.OrgCompanyData on OrgCompanyData.OB_OH = OrgHeader.OH_PK and OB_IsDebtor = 1 AND OB_GC = @CompanyPK
	Left Join dbo.OrgDebtorGroup on OrgDebtorGroup.OJ_PK = OrgCompanyData.OB_OJ_ARDebtorGroup
END
", "DROP PROCEDURE Client_OIA_GLTransactionsBatch", DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion
	}
}
