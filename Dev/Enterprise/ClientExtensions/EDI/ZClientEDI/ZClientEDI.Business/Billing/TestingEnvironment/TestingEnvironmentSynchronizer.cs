using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.Billing.TestingEnvironment
{
	public class TestingEnvironmentSynchronizer : TableSynchronizer
	{
		public TestingEnvironmentSynchronizer(ILogger logger) : base(logger)
		{
		}

		public override void Synchronise()
		{
			if (new[]
			{
					EDIDataRegistry.Instance.EDIBillingTestingDatabaseServerName.Value,
					EDIDataRegistry.Instance.EDIBillingTestingDatabaseName.Value,
					EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin.Value.UserName.ToString(),
					EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin.Value.Password.ToString()
				}.Any(x => string.IsNullOrEmpty(x)))
			{
				Log(LogType.Error, $"Please check the registry items under category: {EDIDataRegistry.Instance.EDIBillingTestingDatabaseServerName.Category}.");
				return;
			}

			base.Synchronise();
		}

		protected override DbConnection NewTargetServerConnection()
		{
			return Db.NewExtraConnection(EDIDataRegistry.Instance.EDIBillingTestingDatabaseServerName.Value,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseName.Value,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin.Value.UserName,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin.Value.Password);
		}

		protected override string GetStagingTableColumnSelectQuery(TableColumn tableColumn)
		{
			//special cases
			switch (tableColumn.ColumnName)
			{
				case LicenceDatabaseSchema.Constants.LD_ETS_TrustedSystem:
					return $"{LicenceDatabaseSchema.Constants.LD_ETS_TrustedSystem} = NULL";
				case LicenceDatabaseSchema.Constants.LD_OA_SoftwareInstallAddressDetails:
					return $"{LicenceDatabaseSchema.Constants.LD_OA_SoftwareInstallAddressDetails} = NULL";
				case LicenceDatabaseSchema.Constants.LD_OC_LicenseeAdminContact:
					return $"{LicenceDatabaseSchema.Constants.LD_OC_LicenseeAdminContact} = NULL";
				case LicenceDatabaseSchema.Constants.LD_OC_ContractInstallerOrInternalTechContact:
					return $"{LicenceDatabaseSchema.Constants.LD_OC_ContractInstallerOrInternalTechContact} = NULL";
				case AccTransactionHeaderSchema.Constants.AH_JH:
					return $"{AccTransactionHeaderSchema.Constants.AH_JH} = NULL";
				case AccTransactionHeaderSchema.Constants.AH_OA_InvoiceAddressOverride:
					return $"{AccTransactionHeaderSchema.Constants.AH_OA_InvoiceAddressOverride} = NULL";
				case AccTransactionHeaderSchema.Constants.AH_OC_InvoiceContactOverride:
					return $"{AccTransactionHeaderSchema.Constants.AH_OC_InvoiceContactOverride} = NULL";
				case AccTransactionHeaderSchema.Constants.AH_XD_ComplianceBook:
					return $"{AccTransactionHeaderSchema.Constants.AH_XD_ComplianceBook} = NULL";
				case AccTransactionLinesSchema.Constants.AL_JBB:
					return $"{AccTransactionLinesSchema.Constants.AL_JBB} = NULL";
				case AccTransactionLinesSchema.Constants.AL_JH:
					return $"{AccTransactionLinesSchema.Constants.AL_JH} = NULL";
				case AccTransactionLinesSchema.Constants.AL_AW:
					return $"{AccTransactionLinesSchema.Constants.AL_AW} = NULL";
				default:
					break;
			}

			return ColumnMaskExclusions.Contains(tableColumn.ColumnName) ?
				base.GetStagingTableColumnSelectQuery(tableColumn) : GetStagingTableColumnSelectQueryWithMask(tableColumn);
		}

		protected override string GetStagingTableAdditionalWhereClause(DatabaseTable table)
		{
			switch (table.TableName)
			{
				case ClientChargeableUsageSchema.Constants.TableName:
					return ClientChargeableUsagePeriodStartFilter;
				case StmDataSchema.Constants.TableName:
					return $" {StmDataSchema.Constants.SD_Name} IN ( {string.Join(",\r\n", RegistryItems.Select(x => $"'{x.Name}'"))} ) ";
				case AccTransactionHeaderSchema.Constants.TableName:
					return $"( {AccTransactionHeaderSchema.Constants.PK} IN ( {ClientChargeableUsageU1_AH_InvoiceQuery} ) )";
				case AccTransactionLinesSchema.Constants.TableName:
					return $"( {AccTransactionLinesSchema.Constants.AL_AH} IN ( {ClientChargeableUsageU1_AH_InvoiceQuery} ) )";
				case EdiBilledUsageSchema.Constants.TableName:
					return $"( {EdiBilledUsageSchema.Constants.BU9_AH_Invoice} IN ( {ClientChargeableUsageU1_AH_InvoiceQuery} ) )";
				case EdiUsageInvoiceSchema.Constants.TableName:
					return $"( {EdiUsageInvoiceSchema.Constants.EUI_AH_Invoice} IN ( {ClientChargeableUsageU1_AH_InvoiceQuery} ) )";

				case EdiBilledDiscountSchema.Constants.TableName:
					return
$@"( {EdiBilledDiscountSchema.Constants.BD9_BU9_Usage}
	IN (
		SELECT {EdiBilledUsageSchema.Constants.PK}
		FROM dbo.{EdiBilledUsageSchema.Constants.TableName} WITH(NOLOCK)
		WHERE {EdiBilledUsageSchema.Constants.BU9_AH_Invoice}
			IN ( {ClientChargeableUsageU1_AH_InvoiceQuery} ) ) )";

				case StmALogSchema.Constants.TableName: //version surcharge
					return
$@"( {StmALogSchema.Constants.SL_Table} = '{LicenceDatabaseSchema.Constants.TableName}'
	AND {StmALogSchema.Constants.SL_SE_NKEvent} = 'EDT'
	AND {StmALogSchema.Constants.SL_PostedTimeUtc} > '{FirstDayOfMonth.AddYears(-1):yyyy-MM-dd}'
	AND {StmALogSchema.Constants.SL_Reference} LIKE '% Version: %' )";

				case ZZRefExchangeRateSchema.Constants.TableName:
					return $"( {ZZRefExchangeRateSchema.Constants.RE_ExpiryDate} > '{FirstDayOfMonth.AddYears(-1):yyyy-MM-dd}' )";

				default:
					return base.GetStagingTableAdditionalWhereClause(table);
			}
		}

		protected override void OnSynchronisingTargetTables(List<DatabaseTable> tablesToDelete)
		{
			TargetServerConnection.ExecuteNonQuery($@"
DELETE dbo.{StmDataSchema.Constants.TableName}
WHERE {StmDataSchema.Constants.SD_Name} IN
(
	SELECT {StmDataSchema.Constants.SD_Name} FROM dbo.{StagingTableNamePrefix}{StmDataSchema.Constants.TableName}
);

UPDATE dbo.{AccTransactionLinesSchema.Constants.TableName} SET {AccTransactionLinesSchema.Constants.AL_LineType} = 'NJL', {AccTransactionLinesSchema.Constants.AL_SystemLastEditUser} = '~BP', {AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc} = GETUTCDATE();
");

			var stmData = tablesToDelete.Find(x => x.TableName == StmDataSchema.Constants.TableName);
			if (stmData != null)
			{
				tablesToDelete.Remove(stmData);
			}
		}

		protected override void OnTargetTablesSynchronised()
		{
			//ARBalance.GetOutstandingBalance()
			var dummyInvoiceBalanceQuery = $@"
SELECT
AH_PK = NEWID(),
AH_Ledger = 'AR',
AH_TransactionType = 'INV',
AH_TransactionNum = CAST(NEWID() AS VARCHAR(38)),
AH_Desc = 'TestingEnvironmentSynchronizer - Dummy Invoice',
AH_InvoiceDate = '2020-1-1',
AH_OutstandingAmount,
AH_OH,
AH_GB,
AH_GC,
AH_GE
FROM
(
	SELECT AH_OH, AH_GC, AH_GB = MIN(AH_GB), AH_GE = MIN(AH_GE), AH_OutstandingAmount = SUM(AH_OutstandingAmount)
	FROM dbo.AccTransactionHeader WITH(NOLOCK)
	WHERE AH_IsCancelled <> 1 
		AND AH_Ledger = 'AR'
		AND NOT AH_TransactionType = 'INB'
		AND (AH_DueDate IS NULL OR AH_DueDate < GETDATE())
		AND AH_PK NOT IN
		(
			SELECT U1_AH_Invoice FROM dbo.ClientChargeableUsage WITH(NOLOCK)
			WHERE {ClientChargeableUsagePeriodStartFilter} AND U1_AH_Invoice IS NOT NULL
		)
		GROUP BY AH_OH, AH_GC
) AH
WHERE AH_OutstandingAmount <> 0;
";
			BulkCopyTable(AccTransactionHeaderSchema.Constants.TableName, dummyInvoiceBalanceQuery);
		}

		string GetStagingTableColumnSelectQueryWithMask(TableColumn tableColumn)
		{
			var isNumber = false;
			var isDate = false;
			var isString = false;
			var isBinary = false;
			var isGuid = false;
			var maskedValue = string.Empty;
			var shouldMask = false;

			switch (tableColumn.DataType)
			{
				case "bigint":
				case "bit":
				case "decimal":
				case "int":
				case "money":
				case "tinyint":
				case "smallint":
					{
						isNumber = true;
						maskedValue = "0";
						break;
					}
				case "date":
				case "datetime":
				case "datetime2":
				case "datetimeoffset":
				case "smalldatetime":
				case "timestamp":
					{
						isDate = true;
						maskedValue = "GETUTCDATE()";
						break;
					}
				case "char":
				case "nchar":
				case "varchar":
				case "nvarchar":
					{
						isString = true;
						maskedValue = "'S' + CONVERT(VARCHAR(10), ROW_NUMBER() OVER (ORDER BY (SELECT NULL)))";
						break;
					}
				case "binary":
				case "varbinary":
				case "image":
					{
						isBinary = true;
						maskedValue = "0x20";
						break;
					}
				case "xml":
					{
						isBinary = true;
						maskedValue = "'<a></a>'";
						break;
					}
				case "geography":
					{
						isBinary = true;
						maskedValue = "CONVERT(GEOGRAPHY, 'POINT EMPTY')";
						break;
					}
				case "uniqueidentifier":
					{
						isGuid = true;
						maskedValue = "NEWID()"; //random guid
						break;
					}
				default:
					{
						throw new NotImplementedException();
					}
			}

			if (isNumber || isDate || isGuid)
			{
				shouldMask = false;
			}
			else if (isBinary)
			{
				shouldMask = true;
			}
			else if (isString)
			{
				shouldMask = !(tableColumn.Length >= 0 && tableColumn.Length <= 5);
			}
			else
			{
				throw new NotImplementedException();
			}

			return shouldMask ? $"{tableColumn.ColumnName} = {maskedValue}" : tableColumn.ColumnName;
		}

		protected override IEnumerable<DatabaseTable> GetTablesToSync() =>
			new[]
			{
				AccBankAccountSchema.Constants.TableName,
				AccChargeCodeSchema.Constants.TableName,
				AccChargeTaxOverrideSchema.Constants.TableName,
				AccGLHeaderSchema.Constants.TableName,
				AccGroupsSchema.Constants.TableName,
				AccInvMsgSchema.Constants.TableName,
				AccOrgTaxConfigurationTemplateSchema.Constants.TableName,
				AccPeriodManagementSchema.Constants.TableName,
				AccTaxOverrideGroupSchema.Constants.TableName,
				AccTaxRateSchema.Constants.TableName,
				AccTransactionHeaderSchema.Constants.TableName,
				AccTransactionLinesSchema.Constants.TableName,
				ClientChargeableUsageSchema.Constants.TableName,
				ClientCompanySchema.Constants.TableName,
				ClientInvoiceDeliverySchema.Constants.TableName,
				ClientLicenceBillingSchema.Constants.TableName,
				ClientLicenceFeeSchema.Constants.TableName,
				ClientLicencePriceHeaderSchema.Constants.TableName,
				ClientLicencePriceItemSchema.Constants.TableName,
				ClientPremiumServiceSchema.Constants.TableName,
				"EdiDepositBalance",
				"EdiDepositBalanceChanges",
				"EdiLicenceDatabaseConsolidationHistory",
				EdiBilledUsageSchema.Constants.TableName,
				EdiBilledDiscountSchema.Constants.TableName,
				EdiLicenceSettingSchema.Constants.TableName,
				EdiOrgMembershipSchema.Constants.TableName,
				EdiPriceDiscountGroupMemberSchema.Constants.TableName,
				EdiPriceHeaderDiscountSchema.Constants.TableName,
				EdiPriceHeaderExchangeRateSchema.Constants.TableName,
				EdiPriceHeaderLinkSchema.Constants.TableName,
				EdiPriceItemRateSchema.Constants.TableName,
				EdiPriceUsageMappingSchema.Constants.TableName,
				EdiUsageInvoiceSchema.Constants.TableName,
				GlbBranchSchema.Constants.TableName,
				GlbCompanySchema.Constants.TableName,
				GlbDepartmentSchema.Constants.TableName,
				LicenceCompanySchema.Constants.TableName,
				LicenceDatabaseSchema.Constants.TableName,
				LicenceEnterpriseSchema.Constants.TableName,
				LicenceHeaderSchema.Constants.TableName,
				LicenceModulesSchema.Constants.TableName,
				OrgCompanyDataSchema.Constants.TableName,
				OrgCreditorGroupSchema.Constants.TableName,
				OrgDebtorGroupSchema.Constants.TableName,
				OrgHeaderSchema.Constants.TableName,
				RefShippingLineSchema.Constants.TableName,
				ReleaseBuildSchema.Constants.TableName,
				StmALogSchema.Constants.TableName,
				StmDataSchema.Constants.TableName,
				ZZRefExchangeRateSchema.Constants.TableName
			}
			.Select(x => new DatabaseTable(x));

		HashSet<string> ColumnMaskExclusions { get; } = new HashSet<string>(new[]
		{
			AccInvMsgSchema.Constants.A9_Code,
			AccInvMsgSchema.Constants.A9_Description,
			AccInvMsgSchema.Constants.A9_EnglishMsg,
			AccInvMsgSchema.Constants.A9_LocalMsg,
			AccBankAccountSchema.Constants.AB_Code,
			AccChargeCodeSchema.Constants.AC_Code,
			AccChargeCodeSchema.Constants.AC_DefaultCommissionProduct,
			AccChargeCodeSchema.Constants.AC_DefaultCommissionService,
			AccChargeCodeSchema.Constants.AC_DefaultCommissionSubModule,
			AccChargeCodeSchema.Constants.AC_DepartmentFilterList,
			AccChargeCodeSchema.Constants.AC_Desc,
			AccChargeCodeSchema.Constants.AC_GovtChargeCode,
			AccChargeCodeSchema.Constants.AC_LocalLanguageDescription,
			AccChargeCodeSchema.Constants.AC_ENettChargeCodeMap,
			AccTransactionHeaderSchema.Constants.AH_Desc,
			AccTransactionHeaderSchema.Constants.AH_TransactionNum,
			AccTransactionLinesSchema.Constants.AL_Desc,
			AccGroupsSchema.Constants.AR_Code,
			AccTaxRateSchema.Constants.AT_Code,
			AccTaxRateSchema.Constants.AT_Description,
			AccTaxRateSchema.Constants.AT_ReferenceExtraRateType,
			AccTaxRateSchema.Constants.AT_ReferenceRateType,
			AccTaxRateSchema.Constants.AT_TaxSystemCode,
			AccTaxOverrideGroupSchema.Constants.AX_Code,
			AccTaxOverrideGroupSchema.Constants.AX_Description,
			EdiBilledUsageSchema.Constants.BU9_UsageSubCode,
			/*EdiDepositBalance*/"DEB_ChargeCode",
			EdiOrgMembershipSchema.Constants.EOR_MembershipType,
			ClientLicencePriceHeaderSchema.Constants.L6_DiscountCode,
			ClientLicencePriceHeaderSchema.Constants.L6_PricelistVersion,
			ClientLicencePriceItemSchema.Constants.L7_ChargeBasis,
			ClientLicencePriceItemSchema.Constants.L7_ChargeCode,
			ClientLicencePriceItemSchema.Constants.L7_DepositChargeCode,
			ClientLicencePriceItemSchema.Constants.L7_Description,
			ClientLicencePriceItemSchema.Constants.L7_DiscountChargeCode,
			ClientLicencePriceItemSchema.Constants.L7_Language,
			ClientLicencePriceItemSchema.Constants.L7_PGM_DiscountGroupCode,
			ClientLicencePriceItemSchema.Constants.L7_Ref4,
			ClientLicenceFeeSchema.Constants.L8_ChargeCode,
			LicenceDatabaseSchema.Constants.LD_TenantID,
			LicenceEnterpriseSchema.Constants.LE_EnterpriseID,
			EdiLicenceSettingSchema.Constants.LS9_Name,
			AccOrgTaxConfigurationTemplateSchema.Constants.OCT_Code,
			OrgHeaderSchema.Constants.OH_Code,
			OrgHeaderSchema.Constants.OH_Language,
			EdiPriceDiscountGroupMemberSchema.Constants.PGM_GroupCode,
			EdiPriceHeaderDiscountSchema.Constants.PHD_ConfigXml,
			EdiPriceHeaderDiscountSchema.Constants.PHD_Name,
			EdiPriceHeaderDiscountSchema.Constants.PHD_Version,
			ZZRefExchangeRateSchema.Constants.RE_AsPublished,
			StmALogSchema.Constants.SL_Reference,
			StmDataSchema.Constants.SD_BinaryValue,
			StmDataSchema.Constants.SD_Name,
			ClientChargeableUsageSchema.Constants.U1_Reference1,
			ClientChargeableUsageSchema.Constants.U1_Reference2,
			ClientChargeableUsageSchema.Constants.U1_Reference3,
			ClientChargeableUsageSchema.Constants.U1_Reference4,
			ClientChargeableUsageSchema.Constants.U1_SubCode
		});

		IRegistryItem[] RegistryItems => new IRegistryItem[]
		{
			AccountingConfigurationRegistry.Instance.ARExceedCreditLimitGrantedEmailNotificationNote,
			AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration,
			AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning,
			AccountingConfigurationRegistry.Instance.CreditLimitWarningThreshold,
			AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup,
			AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure,
			AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault,
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality,
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate,
			AccountingConfigurationRegistry.Instance.GlobalCreditLimitWarningThreshold,
			AccountingConfigurationRegistry.Instance.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation,
			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation,
			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInGlobalCreditLimitCalculation,
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour,
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR,
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch,
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting,
			AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted,
			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables,
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation,
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables,
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables,
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables,
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration,
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration,
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch,
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration,
			AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList,
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod,
			AccountingMasterFilesRegistry.Instance.EReportingPivotPendingStatusDescription,
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations,
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule,
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality,
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode,
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature,
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes,
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting,
			AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation,
			AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions,
			AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds,
			BMSRegistry.Instance.WorkflowManagementMode,
			DocumentsDataRegistry.Instance.AddressPosition,
			DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat,
			DocumentsDataRegistry.Instance.LogDocumentRenderer,
			EDIDataRegistry.Instance.ABMCustomsWareMessagingChargeCode,
			EDIDataRegistry.Instance.ABMCustomsWareMessagingDiscountChargeCode,
			EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingChargeCode,
			EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingDiscountChargeCode,
			EDIDataRegistry.Instance.ABMMovementMessagingChargeCode,
			EDIDataRegistry.Instance.ABMMovementMessagingDiscountChargeCode,
			EDIDataRegistry.Instance.AirlineMessagingDiscountAndRemitChargeCode,
			EDIDataRegistry.Instance.AirlineMessagingFHLChargeCode,
			EDIDataRegistry.Instance.AirlineMessagingFSUChargeCode,
			EDIDataRegistry.Instance.AirlineMessagingFWBChargeCode,
			EDIDataRegistry.Instance.AirlineMessagingTraxonLicenceIdentifier,
			EDIDataRegistry.Instance.AllowedInvoicingBranches,
			EDIDataRegistry.Instance.BillingCountryGroups,
			EDIDataRegistry.Instance.BillingDbUsageCodesList,
			EDIDataRegistry.Instance.BillingPriceRoundingParams,
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists,
			EDIDataRegistry.Instance.BillingSystemChargeCodeMappings,
			EDIDataRegistry.Instance.BillingUnitCountAdjustments,
			EDIDataRegistry.Instance.BillingUsageCategoryCodes,
			EDIDataRegistry.Instance.BorderWisePurchasedGroups,
			EDIDataRegistry.Instance.ClientMappingBillingNames,
			EDIDataRegistry.Instance.CommentChargeCode,
			EDIDataRegistry.Instance.ConsolidatedBillingSettings,
			EDIDataRegistry.Instance.CountryTierPriceCodeMappings,
			EDIDataRegistry.Instance.DatabaseHostedLocations,
			EDIDataRegistry.Instance.EnableIncidentManagementGroupModule,
			EDIDataRegistry.Instance.EnableTaxProcessorForBilling,
			EDIDataRegistry.Instance.EnableTriageEngineModule,
			EDIDataRegistry.Instance.EnableInternalWorkItem,
			EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes,
			EDIDataRegistry.Instance.HandheldDevicePremiumTypes,
			EDIDataRegistry.Instance.HostingDataStorageChargeCode,
			EDIDataRegistry.Instance.HostingDiscountChargeCode,
			EDIDataRegistry.Instance.HostingDocsStorageChargeCode,
			EDIDataRegistry.Instance.HostingNonProductionStorageChargeCode,
			EDIDataRegistry.Instance.HostingPrintServersChargeCode,
			EDIDataRegistry.Instance.HostingRemoteDevicesChargeCode,
			EDIDataRegistry.Instance.HostingStorageBufferPercentage,
			EDIDataRegistry.Instance.HostingUltraFastStorageChargeCode,
			EDIDataRegistry.Instance.InvoiceAttachmentDocType,
			EDIDataRegistry.Instance.InvoicingProcessingFeeLookup,
			EDIDataRegistry.Instance.LegacyCr8ModuleMappings,
			EDIDataRegistry.Instance.LegacyCr9ModuleMappings,
			EDIDataRegistry.Instance.LegacyMenuSectionMappings,
			EDIDataRegistry.Instance.LicenceCountryLanguages,
			EDIDataRegistry.Instance.LicenceFeeTypes,
			EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction,
			EDIDataRegistry.Instance.MinimumAmountToBill,
			EDIDataRegistry.Instance.MonthlyUsageInvoiceComment,
			EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription,
			EDIDataRegistry.Instance.StlMonthlyUsageInvoiceDescription,
			EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode,
			EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment,
			EDIDataRegistry.Instance.NZCustomsJobChargeCode,
			EDIDataRegistry.Instance.NZCustomsMessageChargeCode,
			EDIDataRegistry.Instance.NonBilledEnterpriseCodes,
			EDIDataRegistry.Instance.OdplDepositChargeCode,
			EDIDataRegistry.Instance.OdplDiscountChargeCode,
			EDIDataRegistry.Instance.OdplHybridDiscountChargeCode,
			EDIDataRegistry.Instance.OdplHybridUsageChargeCode,
			EDIDataRegistry.Instance.OdplSurchargeChargeCode,
			EDIDataRegistry.Instance.OdplUsageChargeCode,
			EDIDataRegistry.Instance.OrgMembershipTypes,
			EDIDataRegistry.Instance.PrepaidBalanceChargeCode,
			EDIDataRegistry.Instance.PrepaymentInvoiceComment,
			EDIDataRegistry.Instance.PrepaymentMargin,
			EDIDataRegistry.Instance.PriceListDocType,
			EDIDataRegistry.Instance.ProcessingFeeExemptBillingSystems,
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings,
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings,
			EDIDataRegistry.Instance.ProductAreas,
			EDIDataRegistry.Instance.ProductDisplayCategories,
			EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode,
			EDIDataRegistry.Instance.RevisedPrepaymentBalanceDescription,
			EDIDataRegistry.Instance.SalesTaxRates,
			EDIDataRegistry.Instance.ServiceTypeCr8Mappings,
			EDIDataRegistry.Instance.ServiceTypeCr9Mappings,
			EDIDataRegistry.Instance.ServiceTypeMappings,
			EDIDataRegistry.Instance.ServiceTypes,
			EDIDataRegistry.Instance.SourceModules,
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier,
			EDIDataRegistry.Instance.StlDiscountSuspensionPolicyDefault,
			EDIDataRegistry.Instance.StlDiscountTypes,
			EDIDataRegistry.Instance.StlFreeTrialDiscounts,
			EDIDataRegistry.Instance.StlInvoiceSupportedLanguages,
			EDIDataRegistry.Instance.StlPriceListExchangeRateGroups,
			EDIDataRegistry.Instance.StlRawUsageReportRefCaption,
			EDIDataRegistry.Instance.StlSurchargeChargeCode,
			EDIDataRegistry.Instance.SystemProductMappings,
			EDIDataRegistry.Instance.Tier4CountryCodes,
			EDIDataRegistry.Instance.TransactionChargeCodes,
			EDIDataRegistry.Instance.TransactionDiscountChargeCodes,
			EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes,
			EDIDataRegistry.Instance.UsageBillingSettings,
			EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists,
			EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent,
			EDIDataRegistry.Instance.VersionSurchargeChargeCode,
			EDIDataRegistry.Instance.VersionSurchargeDescription,
			EDIDataRegistry.Instance.VersionSurchargePercent,
			EDIDataRegistry.Instance.WebSecurityProductModuleMappings,
			OrganisationsDataRegistry.Instance.MakeSalesRepMandatory,
			OrganisationsDataRegistry.Instance.RunCommissionCreationInBackground,
			RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization,
			RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation,
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany,
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates,
			WorkflowDataRegistry.Instance.EnableUniversalTemplates
		};

		readonly ZDateTime ZDateTimeNow = ZDateTime.Now;
		ZDateTime FirstDayOfMonth => new DateTime(ZDateTimeNow.Year, ZDateTimeNow.Month, 1);
		const int MaxUsageMonths = 3;
		string ClientChargeableUsagePeriodStartFilter =>
			$" ( {ClientChargeableUsageSchema.Constants.U1_PeriodStart} >= '{FirstDayOfMonth.AddMonths(-MaxUsageMonths):yyyy-MM-dd}' ) ";
		string ClientChargeableUsageU1_AH_InvoiceQuery =>
$@" SELECT {ClientChargeableUsageSchema.Constants.U1_AH_Invoice}
FROM dbo.{ClientChargeableUsageSchema.Constants.TableName}
WITH(NOLOCK) WHERE {ClientChargeableUsagePeriodStartFilter}  ";
	}
}
