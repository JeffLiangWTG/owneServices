using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.JAS.Testing
{
	class DummyJASClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		public ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray<DatabaseObjectCreateScript>.Empty;
		public ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutineCreationScripts => TestProceduresCreationScripts;

		static ImmutableArray<DatabaseViewAndRoutineCreateScript> TestProceduresCreationScripts => ImmutableArray.Create(
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts,
			ClientInvertCognosRawAggregateSignageIfApplicable,
			ClientUpdateCognosRawAggregateRecordsWithALTAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts,
			ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts,
			ClientInsertCognosAccountsIntoCognosExportTempTable
		);

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromHeader") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromHeaderBank") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromDirectReceiptPaymentLine") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromDirectReceiptPaymentBank") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromInvCrdAdj") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromCFX") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromGLJournals") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromWIPACR") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromARAPControlAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromExchangeDiscountOverpaymentControlAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromGSTControlAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN" + CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams("GLAggregationFromWIPACRControlAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable

		static DatabaseViewAndRoutineCreateScript ClientInvertCognosRawAggregateSignageIfApplicable
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInvertCognosRawAggregateSignageIfApplicable", @"
CREATE PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable
AS
BEGIN" + CreateInsertScriptForTestWithoutParams("InvertCognosRawAggregateSignageIfApplicable") + " END"
							, "DROP PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts

		static DatabaseViewAndRoutineCreateScript ClientUpdateCognosRawAggregateRecordsWithALTAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUpdateCognosRawAggregateRecordsWithALTAccounts", @"
CREATE PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts
AS
BEGIN" + CreateInsertScriptForTestWithoutParams("UpdateCognosRawAggregateRecordsWithALTAccounts") + " END"
							, "DROP PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts(@PnLStartAccount varchar(10), @BSHStartAccount varchar(10))
AS
BEGIN" + CreateInsertScriptForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts() + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts
AS
BEGIN" + CreateInsertScriptForTestWithoutParams("GLAggregationFromCLNAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts
AS
BEGIN" + CreateInsertScriptForTestWithoutParams("FromSubClassificationAccounts") + " END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable

		static DatabaseViewAndRoutineCreateScript ClientInsertCognosAccountsIntoCognosExportTempTable
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertCognosAccountsIntoCognosExportTempTable", @"
CREATE PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable
AS
BEGIN" + CreateInsertScriptForTestWithoutParams("InsertCognosAccountsIntoCognosExportTempTable") + " END"
							, "DROP PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region Implementation

		static string CreateInsertScriptForTestWithoutParams(string procedureName)
		{
			return @"
INSERT INTO #CognosExport
SELECT  AJ_PK, '', '', '', '', 0, '', 0, ''
FROM    dbo.AccGLAccountDescriptor        
WHERE   AJ_AccountDescription = '" + procedureName + "'";
		}

		static string CreateInsertScriptForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts()
		{
			return @"
INSERT INTO #CognosExport
SELECT  AJ_PK, '', '', '', '', 0, '', 0, ''
FROM    dbo.AccGLAccountDescriptor
        INNER JOIN dbo.DummyBizO ON Z0_Guid = AJ_PK        
WHERE   AJ_AccountDescription = 'GLAggregationFromCFWAndTTLAccounts'        
        AND Z0_NVarChar >= @PnLStartAccount 
        AND Z0_NVarChar <= @BSHStartAccount";
		}

		static string CreateInsertScriptForTestWithCompanyPKandExportStartDateAsParams(string procedureName)
		{
			return @"
INSERT INTO #CognosExport
SELECT  AJ_PK, '', '', '', '', 0, '', 0, ''
FROM    dbo.AccGLAccountDescriptor
        INNER JOIN dbo.DummyBizO ON Z0_Guid = AJ_PK
        INNER JOIN dbo.GlbCompany ON Z0_FK_Code = GC_Code
WHERE   AJ_AccountDescription = '" + procedureName + @"'
        AND Z0_SmallDateTime <= @ExportStartDate
        AND GC_PK = @CompanyPK";
		}

		#endregion
	}
}
