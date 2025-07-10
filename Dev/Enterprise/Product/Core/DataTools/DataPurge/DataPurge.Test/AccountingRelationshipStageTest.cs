using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataPurge
{
	public class AccountingRelationshipStageTest : RelationshipStageTestCase
	{
		public void TestPurge_AccountingRelatedTables()
		{
			var companyPk = CreateCompany("TC1", "Test Company 1");
			var branchPk = CreateBranch("TB1", "Test Branch 1", companyPk);
			var departmentPk = CreateDepartment("TSD", "Test Department");
			var headerPk = ZGuid.NewZGuid();
			var batchPk = ZGuid.NewZGuid();

			var createSql = $@"
INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES ('{headerPk}', '{companyPk}', '{branchPk}', '{departmentPk}', getdate(), 'AR', 'INV');
INSERT dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_Status, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser, AIB_SystemLastEditTimeUtc, AIB_SystemLastEditUser) VALUES ('{batchPk}', '{companyPk}', 1, 'SNT', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_AIB, AIP_GC, AIP_ParentID, AIP_RN_NKCountryCode, AIP_SystemCreateTimeUtc, AIP_SystemLastEditTimeUtc) VALUES (NEWID(), '{batchPk}', '{companyPk}', '{headerPk}', 'MY', GetUtcDate(), GetUtcDate());
INSERT INTO dbo.AccTransactionHeaderAuthorisationRecord (AHF_PK, AHF_ParentId, AHF_DateTime, AHF_RecordType,AHF_ParentTableCode,AHF_Number,AHF_IDNumber,AHF_Counter,AHF_IDType,AHF_SystemCreateTimeUtc,AHF_SystemCreateUser,AHF_SystemLastEditTimeUtc,AHF_SystemLastEditUser)
VALUES (NEWID(),'{headerPk}',GETDATE(),'ZZZ','AH',123,123,1,'MYY',getdate(),'LZ',getdate(),'LZ');

INSERT dbo.AccBillingHeader (ABH_PK, ABH_IsValid, ABH_InternalReferenceNumber, ABH_BillingCode, ABH_ParentId, ABH_ParentTableCode, ABH_ParentReferenceNumber, ABH_EventType, ABH_EventTimeUtc, ABH_GS_NKEventUser, ABH_BillingCounter, ABH_GC_Company, ABH_SystemCreateTimeUtc, ABH_SystemCreateUser, ABH_SystemLastEditTimeUtc, ABH_SystemLastEditUser)
VALUES ('4A03BAD2-0087-4E9E-A3F2-AF49E916D36A', 0, '00001000', 'GSH', '9A9B98C4-0E59-4E47-9852-C35B0885852A', 'JK', 'C00696908', 'JRJ', '2022-08-22 14:59:00', 'E', 2, '{companyPk}', '2022-08-22 14:59:00', 'E', '2022-08-22 14:59:00', 'E');

INSERT dbo.AccBillingItem (ABI_PK, ABI_IsValid, ABI_ABH, ABI_ParentId, ABI_ParentTableCode, ABI_ParentReferenceNumber, ABI_SystemCreateTimeUtc, ABI_SystemCreateUser, ABI_SystemLastEditTimeUtc, ABI_SystemLastEditUser)
VALUES ('C16B8210-9589-4CBD-9DEF-C4E55B317F5F', 0, '4A03BAD2-0087-4E9E-A3F2-AF49E916D36A', 'D27139BA-4A26-4349-A697-79B7EB8821B6', 'JS', 'S54657693', '2022-08-22 14:59:00', 'E', '2022-08-22 14:59:00', 'E');

INSERT dbo.AccChargeCode (AC_PK, AC_Code, AC_Desc, AC_LocalLanguageDescription, AC_ChargeType, AC_MarginPercentage, AC_AW_WithholdingTaxRate, AC_AT_GSTRate, AC_AG_RevenueAccount, AC_AG_WIPAccount, AC_AG_CostAccount, AC_AG_AccrualAccount, AC_PrintSequence, AC_AR_SalesGroup, AC_AR_ExpenseGroup, AC_ChargeGroup, AC_ChargeSubGroup, AC_RateCalculator, AC_DepartmentFilterList, AC_IATA_ChargeCodeMap, AC_GC, AC_ENettChargeCodeMap, AC_ChargeOtherGroups, AC_GoodsServiceType, AC_AX_TaxOverrideGroup, AC_IsActive, AC_IsGroupageCharge, AC_ShowOnQuotation, AC_SuppressOnQuoteIfZero, AC_AllowDescriptionOvertype, AC_IsCommissionable, AC_InputGSTVATRecoverable, AC_EnergySourceGroup, AC_DefaultCommissionProduct, AC_DefaultCommissionService, AC_DefaultCommissionSubModule, AC_AC_RevenueChargeCode, AC_IsAdhocServiceCharge, AC_GovtChargeCode, AC_AG_CostClearingAccount, AC_AG_RevenueClearingAccount, AC_AutoVersion, AC_AG_DisbursementShortfallAccount, AC_AG_DisbursementSurplusAccount, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser)
VALUES ('69AF2F19-2245-4A21-A1D1-1BF9D443A729', 'CODE', 'TEST', '', 'REV', 0.00, null, null, null, null, null, null, 0, null, null, 'CLL', '', '', 'ALL', '', '{companyPk}', '', 'PRC', 'SRV', null, 1, 0, 1, 0, 1, 1, 1.0000, '', '', '', '', null, 0, '', null, null, 0, null, null, '2022-06-07 23:05:00', 'E', '2022-06-07 23:05:00', 'E');

INSERT dbo.AccChargeSupplyTypeOverride (ACS_PK, ACS_ParentTableCode, ACS_ParentID, ACS_JobType, ACS_TransportMode, ACS_Direction, ACS_IncoTerm, ACS_GE, ACS_SupplyType, ACS_SystemCreateTimeUtc, ACS_SystemCreateUser, ACS_SystemLastEditTimeUtc, ACS_SystemLastEditUser)
VALUES ('4A235588-BF95-4DB2-94A1-169B0E0EA4B3', 'AC', '69AF2F19-2245-4A21-A1D1-1BF9D443A729', 'ALL', 'ALL', 'ALL', 'ALL', null, 'LOC', '2023-11-13 02:37:00', 'E', '2023-11-13 02:39:00', 'E');

INSERT dbo.AccChargeTaxOverride (AO_PK, AO_Direction, AO_IncoTerm, AO_JobType, AO_CostSellAll, AO_Origin, AO_Destination, AO_TaxRegCntryOrGroup, AO_AT, AO_ParentTableCode, AO_ParentID, AO_HomeCountryOrZone, AO_TaxRegCntryOrZone, AO_A9_DefaultVATClass, AO_VATExemptOnExportCharges, AO_IsAnIndividual, AO_TransportMode, AO_OrganisationCategory, AO_SplitPaymentVATOrganisation, AO_GB, AO_CreateTaxRecord, AO_DefaultingRule, AO_TransactionContext, AO_CustomsStatus, AO_SystemCreateTimeUtc, AO_SystemCreateUser, AO_SystemLastEditTimeUtc, AO_SystemLastEditUser, AO_SupplyType, AO_DebtorRole)
VALUES ('7DB6EAA3-34D3-49B0-AC76-599BC9E41E58', 'ALL', 'ALL', 'ALL', 'REV', 'ALL', 'YE', 'ALL', null, 'AX', '6197EA9B-5DE6-4B14-AB80-01752670E2B2', '', '', null, 0, 0, 'ALL', 'ALL', 0, null, 1, 'NON', 'STD', '', '2022-08-22 14:59:00', 'E', '2022-08-22 14:59:00', 'E', '', '');

INSERT dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_RX_NKTransactionCurrency, CL0_TransactionAmount, CL0_CommissionType, CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount, CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup, CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_EntityPercentage, CL0_ShouldReinstate, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES ('4DA94CF5-7252-4633-84B8-6DE3F86FE4BE', 'D3C64FD9-ACF4-4D0E-989E-18735C12F795', 'CH0', null, 'AC', null, '', 0.0000, 'BON', 'AUD', 0.0000, 0, 0, 0.0000, 99.0000, '2016-12-14 07:15:00', '2016-12-14 07:15:00', null, null, null, '2015-10-14 05:57:00', 'F.L', 100.00, 1, '2015-10-14 05:57:00', 'E');

INSERT dbo.AccComplianceReport (ACR_PK, ACR_Description, ACR_ReportType, ACR_Periodicity, ACR_DateFrom, ACR_DateTo, ACR_PageNumberFrom, ACR_PageNumberTo, ACR_GC_Company, ACR_GB_Branch, ACR_Status, ACR_SystemCreateTimeUtc, ACR_SystemCreateUser, ACR_SystemLastEditTimeUtc, ACR_SystemLastEditUser, ACR_StatusMessage, ACR_ReferenceNumber)
VALUES ('909AF196-4086-4439-8E7B-0298E38B26BA', 'LIQ FEBRUARY 2024', 'LIQ', 'RNG', '2024-02-01', '2024-02-28', 0, 0, '{companyPk}', null, 'GEN', '2024-08-21 09:12:00', 'E', '2024-08-21 09:13:00', 'E', '', null);

INSERT dbo.AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence, ACL_ReportSubCode, ACL_SystemCreateTimeUtc, ACL_SystemCreateUser, ACL_SystemLastEditTimeUtc, ACL_SystemLastEditUser)
VALUES ('0326AD94-5CE6-4E03-9DF8-CA69E4E98EFB', 'E4321391-E794-46BD-A14A-555355E9A1D8', 'AL', '{companyPk}', '909AF196-4086-4439-8E7B-0298E38B26BA', 1, '', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00', 'E');

INSERT dbo.AccCurrencyAdjustmentQueue (ACA_ParentID, ACA_ParentTableCode, ACA_Date, ACA_GC)
VALUES ('3AFBF5E7-C6B9-4B69-A04B-0B9FD1E7671E', 'AM', '2020-04-30 16:40:22.060', '{companyPk}');

INSERT dbo.AccDraftInvoiceHeader (AIH_PK, AIH_AutoVersion, AIH_GC_Company, AIH_GB_Branch, AIH_GE_Department, AIH_TransactionType, AIH_TransactionNumber, AIH_TransactionDate, AIH_PostDate, AIH_DueDate, AIH_DocumentReceivedDate, AIH_OH_Creditor, AIH_OA_CreditorAddress, AIH_OC_CreditorContact, AIH_RX_NKTransactionCurrency, AIH_ExchangeRate, AIH_ExpectedOSExTaxAmount, AIH_ExpectedOSTaxAmount, AIH_ExpectedOSTotalAmount, AIH_Description, AIH_InternalReference, AIH_SystemCreateTimeUtc, AIH_SystemCreateUser, AIH_SystemLastEditTimeUtc, AIH_SystemLastEditUser, AIH_AH_PostedTransactionHeader, AIH_AH_OriginalTransaction, AIH_OriginalInvoiceDate, AIH_OriginalTransactionNum, AIH_Status)
VALUES ('6A34E7DD-DE54-4EAD-83AB-1AFE78EDFA68', 5, '{companyPk}', '{branchPk}', '{departmentPk}', 'INV', '6548.00', '2024-06-04', '2024-06-04', '2024-06-04', '2024-06-04', null, null, null, 'AUD', 1.000000000, 575.0000, 0.0000, 575.0000, 'AP Invoice', '00001000', '2024-06-04 03:34:00', 'E', '2024-11-26 09:15:00', 'CGZ', null, null, null, '', 'DFT');

INSERT dbo.AccDraftInvoiceJobCluster (AIC_PK, AIC_AIH_Header, AIC_Amount, AIC_RX_NKCurrency, AIC_GC_Company, AIC_SystemCreateTimeUtc, AIC_SystemCreateUser, AIC_SystemLastEditTimeUtc, AIC_SystemLastEditUser)
VALUES ('2DC260E3-A90F-4194-B011-35247AED884A', '6A34E7DD-DE54-4EAD-83AB-1AFE78EDFA68', 0.0000, 'EUR', '{companyPk}', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00', 'E');

INSERT dbo.AccDraftInvoiceJob (AIJ_PK, AIJ_AIC_Cluster, AIJ_ParentID, AIJ_ParentTableCode, AIJ_GC_Company, AIJ_SystemCreateTimeUtc, AIJ_SystemCreateUser, AIJ_SystemLastEditTimeUtc, AIJ_SystemLastEditUser)
VALUES ('20A859DD-C357-4331-B8DF-1A9A5410BBC9', '2DC260E3-A90F-4194-B011-35247AED884A', '94126C00-FAB7-4841-AE81-136DEBEBC710', 'JS', '{companyPk}', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00', 'E');

INSERT dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_Code2, JCF_Percentage, JCF_Amount, JCF_Number, JCF_Flag, JCF_Code3, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser, JCF_IncoTerm, JCF_SupplyType, JCF_InvoiceCurrencyType, JCF_RN_NKDestinationCountry, JCF_RN_NKOriginCountry, JCF_ExpiryDate, JCF_StartDate)
VALUES ('ACD9D6E6-2581-4550-B322-37EE11EEC5DA', 'ERT', null, '  ', '', null, 'SHP', 'ALL', 'ALL', 'BUY', 'TDR', 0.00, 0.0000, 0, 0, '', null, '', null, '', '', '', '', '', '', null, null);

INSERT dbo.AccJobConfigPivot (JCT_PK, JCT_JCF_JobConfig, JCT_Code, JCT_ParentId, JCT_ParentTableCode, JCT_SystemCreateTimeUtc, JCT_SystemCreateUser, JCT_SystemLastEditTimeUtc, JCT_SystemLastEditUser, JCT_ExpiryDate, JCT_ExRateType, JCT_StartDate)
VALUES ('BB14843A-B88F-406B-9E86-DA46166DCF46', 'ACD9D6E6-2581-4550-B322-37EE11EEC5DA', 'USD', null, '', '2024-08-21 02:03:00', 'E', '2024-08-21 02:03:00', 'E', null, 'BUY', null);

INSERT dbo.AccTaxConfiguration (ETC_PK, ETC_Code, ETC_Description, ETC_RN_NKCountry, ETC_TaxAuthorityCode, ETC_TaxSystemCode, ETC_Ledger, ETC_ParentId, ETC_ParentTableCode, ETC_IsActive, ETC_TaxRealisationMethod, ETC_TaxRecordCreationTrigger, ETC_CancellationPolicy, ETC_AG_LedgerControlAccount, ETC_AG_TaxControlAccount, ETC_AG_TaxExpenseAccount, ETC_AG_TaxPendingControlAccount, ETC_RecoveryMethod, ETC_SystemCreateTimeUtc, ETC_SystemCreateUser, ETC_SystemLastEditTimeUtc, ETC_SystemLastEditUser, ETC_TaxAmountRounding, ETC_ThresholdAmount, ETC_ThresholdMethod)
VALUES ('2CA20633-9A7F-464D-B9D2-229E9B8CF340', 'AR-SC-PAT-AR', 'ARBA', 'AR', 'SC', 'PAT', 'AR', '4745CE38-4BFA-4E94-8320-F107EEB2E216', 'GC', 1, 'PDT', 'PDT', 'NRE', null, null, null, null, 'NOR', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00', 'E', 'STD', 0.0000, 'NOT');

INSERT dbo.AccTransactionComplianceReportQueue (ACQ_PK, ACQ_ReportType, ACQ_GC_Company, ACQ_GB_Branch, ACQ_ParentID, ACQ_ParentTableCode, ACQ_ReportSubCode, ACQ_Date)
VALUES ('BBFF1F91-B15D-451E-88B6-C535E03EF465', 'DAD', '{companyPk}', '{branchPk}', '44FFF3A4-6964-432A-8A30-04E913DAF92F', 'AH', '*AR*INV*ARCtrl*Total', '2020-12-01');

INSERT dbo.AccTransactionHeaderSubAccount (AHS_PK, AHS_AH, AHS_SubClassParentTableCode, AHS_SubClassParentId, AHS_SystemCreateTimeUtc, AHS_SystemCreateUser, AHS_SystemLastEditTimeUtc, AHS_SystemLastEditUser)
VALUES ('15E9C11E-D1F1-404E-AF0D-11B3E250381A', '{headerPk}', 'GS', '16C625B9-C46B-4F0E-BF6F-A10EF3810EEC', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00', 'E');

INSERT dbo.AccTransactionLines (AL_PK, AL_LineType, AL_Sequence, AL_Desc, AL_LineAmount, AL_AT, AL_GSTVAT, AL_AW, AL_WithholdingTax, AL_UnitQty, AL_UnitPrice, AL_OSUnitPrice, AL_OSAmount, AL_ExchangeRate, AL_PostPeriod, AL_PostDate, AL_PostToGL, AL_ReversePeriod, AL_ReverseDate, AL_ReverseToGL, AL_ExportBatchNumber, AL_ExportReverseBatchNumber, AL_AH, AL_JH, AL_AC, AL_GE, AL_GB, AL_AG, AL_OH, AL_AG_PercentOf, AL_PercentageOfPeriod, AL_RevRecognitionType, AL_RX_NKTransactionCurrency, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_GSTVATBasis, AL_A9_VATClass, AL_PreventInvoicePrintGrouping, AL_IsFinalCharge, AL_InputGSTVATRecoverable, AL_GC, AL_GovtChargeCode, AL_GSTVATExtra, AL_TaxDate, AL_TaxExtraRateDenominator, AL_TaxExtraRateNumerator, AL_TaxRateDenominator, AL_TaxRateNumerator, AL_PlaceOfSupply, AL_PlaceOfSupplyType, AL_AutoVersion, AL_JBB, AL_SupplyType, AL_GB_TaxBranch)
VALUES ('863F3B02-572A-46D8-AED4-00993713F0D4', 'GJL', 1, 'MASTER JOURNAL FOR AR INV', -35000.0000, null, 0.0000, null, 0.0000, 0, 0.0000, 0.0000, -35000.0000, 1.000000000, 0, '2020-02-26 15:43:00', 'N', 0, '2020-02-26 15:43:00', 'N', 0, 0, '{headerPk}', null, null, '{departmentPk}', '{branchPk}', null, null, null, 0, 'IMM', 'AUD', '2020-02-26 04:46:00', 'E', '2020-02-26 04:46:00', 'E', 'A', null, 0, 1, 1.0000, '{companyPk}', '', 0.0000, '2020-02-26', 1, 0, 1, 0, '', '', 1, null, '', null);

INSERT dbo.AccTransactionLineSubAccount (AL1_PK, AL1_AL, AL1_SubClassParentTableCode, AL1_SubClassParentId, AL1_SystemCreateTimeUtc, AL1_SystemCreateUser, AL1_SystemLastEditTimeUtc, AL1_SystemLastEditUser)
VALUES ('1B283CD0-31B6-4869-961F-446293596193', '863F3B02-572A-46D8-AED4-00993713F0D4', 'AR', 'FB31DBCC-5952-44E1-ADFD-21870EE38F0F', null, '', null, '');

INSERT dbo.AccTransactionPostingToGLDQueue(APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_SystemCreateUser, APQ_SystemCreateTimeUtc)
VALUES ('{headerPk}', 'AH', '{companyPk}', '2024-11-25 23:55:00', 'E', '2024-11-25 23:55:00');
";
			TestConnection.ExecuteNonQuery(createSql);

			var accountingRelationships = new AccountingRelationshipStageForTest();
			accountingRelationships.SetupRelationships();
			AssertChild(1);

			var purger = new DataPurger
			{
				HasRatingInfoPurgeScript = true,
				HasProductInfoPurgeScript = true,
				HasNonSystemChageCodesPurgeScript = true,
				HasTariffInfoPurgeScript = true,
				HasQuotationsPurgeScript = true
			};
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts());
			AssertChild(0);
			void AssertChild(int count)
			{
				CombineAssertions(() =>
				{
					var tableSet = new SortedSet<string>();
					foreach (var pair in accountingRelationships.RelationshipMap)
					{
						foreach (var relationship in pair.Value)
						{
							if (relationship.ForcingTest)
							{
								tableSet.Add(relationship.ChildTableName);
							}
						}
					}

					foreach (var tableName in tableSet)
					{
						AssertTableCount(tableName, count);
					}
				});
			}
		}

		void AssertTableCount(string tableName, int expectedCount)
		{
			AssertEquals("Row count for table " + tableName, expectedCount, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName));
		}

		ZGuid CreateCompany(ZString code, ZString name)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name)
				VALUES (@pk, @code, @name)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@name", SqlDbType.NVarChar, GlbCompanySchema.GC_Name.MaxLength, name.ToString());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		ZGuid CreateBranch(ZString code, ZString name, ZGuid companyPk)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC)
				VALUES (@pk, @code, @name, @companyPk)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@name", SqlDbType.NVarChar, GlbBranchSchema.GB_BranchName.MaxLength, name.ToString());
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk.ToGuid());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		ZGuid CreateDepartment(ZString code, ZString description)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc)
				VALUES (@pk, @code, @description)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@description", SqlDbType.VarChar, GlbDepartmentSchema.GE_Desc.MaxLength, description.ToString());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;

		public override string[] TablePrefixes => ["Acc"];

		public override BusinessRelationshipStageForTest RelationshipStage { get; } = new AccountingRelationshipStageForTest();
	}
}
