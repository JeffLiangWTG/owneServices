using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescription = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalDebitCredit = Enterprise.UniversalDataBuss.DataObjects.Accounting.DebitCredit;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class SAFTAdditionalDataCollector : ComplianceReportAdditionalDataCollectorBase<TransactionHeaderDetails>
	{
		public SAFTAdditionalDataCollector(AccComplianceReport report, ComplianceReportDataCollectionMode mode = ComplianceReportDataCollectionMode.None, ZGuid? selectedSupplierPK = null, string selectedSupplierCode = null)
			: base(report, mode)
		{
			if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				Argument.NotNull(selectedSupplierPK, nameof(selectedSupplierPK));
				Argument.NotNullOrEmpty(selectedSupplierCode, nameof(selectedSupplierCode));
			}

			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				SelectedSupplierPK = selectedSupplierPK.Value;
				SelectedSupplierCode = selectedSupplierCode;
				HeaderData = GetTransactionHeaderDetails(Report);
			}

			if (IsSAFTVersion110OrAbove)
			{
				DefaultReceiptAccountNumber = GetDefaultReceiptAccountNumber(Report.Factory, Report.Company.LocalCurrency.Code, Report.Company.PK);
				ContactStaff = GetContactStaff(Report.Factory, Report.ACR_SystemCreateUser);
				SetOrgContacts(Report);
				SetOrgFullNames(Report);

				if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFT1_30)
				{
					TaxAccountingBasis = GetTaxAccountingBasis();
					TaxAuthority = GetTaxAuthority();
					TaxTableDescription = GetTaxTableDescription();
					JournalType = GetJournalType();
					TaxRegistrationCode = GetTaxRegistrationCode();
					BusinessRegistrationCode = GetBusinessRegistrationCode();
				}
			}

			if (!ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly())
			{
				UsedGLAccounts = new HashSet<ZString>();
				LineData = GetTransactionLineDetails(Report);

				CountryCodesOfEuropeanUnion = GetCountryCodesOfEuropeanUnion(Report);

				if (IsAnySAFT)
				{
					GLAccounts = GetGLAccounts(Report);
					ActiveGLAccounts = GetActiveGLAccounts(Report);
					WithholdingTaxIDs = GetWithholdingTaxIDs(Report);

					OrgHeaderDetails = GetOrgHeaderDetails(Report);

					CustomerCodes = GetCustomerCodes(Report);
					SupplierCodes = GetSupplierCodes(Report);

					SalesInvoices = GetSalesInvoices(Report, HeaderData);
					SalesInvoicesTotals = GetSalesInvoicesTotals(Report, SalesInvoices, HeaderData);
					InvoicedTaxIDs = new HashSet<ZString>();
					CollectInvoicedTaxIDs(SalesInvoices);
					GetSalesInvoicesReferences(Report, SalesInvoices.Keys.AsEnumerable());
					GetCustomerSalesInvoices(SalesInvoices);
					Payments = GetPayments(Report);
					PaymentsTotals = GetPaymentsTotals(Payments);
				}
			}

			if (IsSAFTVersion110OrAbove)
			{
				SetOrgBalance(Report);
			}
		}

		protected override void MergeDataFromAnotherReportCore()
		{
			if (IsSAFTVersion110OrAbove)
			{
				ContactStaff = GetContactStaff(Report.Factory, Report.ACR_SystemCreateUser);
				SetOrgContacts(Report);
				SetOrgFullNames(Report);
			}

			var orgAddressesAndRegistrationNumbers = GetOrgAddressesAndTaxRegistrationNumber(Report);
			OrgAddresses = orgAddressesAndRegistrationNumbers.orgAddresses;              // Merge
			OrgTaxRegistrationNumberDetails = orgAddressesAndRegistrationNumbers.taxRegistrationNumberDetails;  // Merge
			LineData = GetTransactionLineDetails(Report);       // Override
			HeaderData = GetTransactionHeaderDetails(Report);   // Override
			TaxIDs = GetTaxIDs(Report);                         // Override as the rates could change

			if (IsAnySAFT)
			{
				ActiveGLAccounts = GetActiveGLAccounts(Report);
				WithholdingTaxIDs = GetWithholdingTaxIDs(Report);   // Override as the rates could change

				OrgHeaderDetails = GetOrgHeaderDetails(Report);     // Merge

				CustomerCodes = GetCustomerCodes(Report);           // Merge
				SupplierCodes = GetSupplierCodes(Report);           // Merge

				SalesInvoices = GetSalesInvoices(Report, HeaderData);                       // Override
				SalesInvoicesTotals = GetSalesInvoicesTotals(Report, SalesInvoices, HeaderData);    // Merge
				CollectInvoicedTaxIDs(SalesInvoices);
				GetSalesInvoicesReferences(Report, SalesInvoices.Keys.AsEnumerable());
				GetCustomerSalesInvoices(SalesInvoices);                        // Merge
				Payments = GetPayments(Report);                                 // Override
				PaymentsTotals = GetPaymentsTotals(Payments);                   // Merge
			}
		}

		ZGuid SelectedSupplierPK { get; set; } // For SAFTSelfBilling
		ZString SelectedSupplierCode { get; set; } // For SAFTSelfBilling
		internal HashSet<ZString> CustomerCodes { get; private set; }
		internal HashSet<ZString> SupplierCodes { get; private set; }

		internal Dictionary<ZString, GLAccount> GLAccounts { get; private set; }
		internal Dictionary<ZString, GLAccountDetails> ActiveGLAccounts { get; private set; }
		internal HashSet<ZString> UsedGLAccounts { get; private set; } // Collected while getting LineData

		internal Dictionary<ZGuid, IEnumerable<AccComplianceReportLine>> SalesInvoices { get; private set; }
		internal (ZInt NumberOfEntries, ZDecimal TotalDebit, ZDecimal TotalCredit) SalesInvoicesTotals { get; private set; }
		internal Dictionary<ZString, List<ZGuid>> CustomerSalesInvoices { get; private set; }
		internal Dictionary<ZGuid, AccComplianceReportLine> Payments { get; private set; }
		internal (ZInt NumberOfEntries, ZDecimal TotalDebit, ZDecimal TotalCredit) PaymentsTotals { get; private set; }

		internal HashSet<ZGuid> PostedWithoutIVASalesInvoicePKs { get; private set; }
		internal Dictionary<ZGuid, ZString> HeaderAuthorizationNumberReferences { get; private set; }

		#region SAFT 1_10 & 1_30

		internal ZString DefaultReceiptAccountNumber { get; private set; }
		internal GlbStaff ContactStaff { get; private set; }
		internal Dictionary<ZGuid, ZString> HeaderDescriptions { get; private set; }
		internal Dictionary<ZGuid, ZString> GLAccountTypes { get; private set; }
		internal Dictionary<ZGuid, ZString> TaxGroupCodes { get; private set; }
		internal Dictionary<ZString, OrgContactDependentCollection> OrgContacts { get; private set; }
		internal string TaxAccountingBasis { get; private set; }
		internal string TaxAuthority { get; private set; }
		internal string TaxTableDescription { get; private set; }
		internal string JournalType { get; private set; }
		internal string TaxRegistrationCode { get; private set; }
		internal string BusinessRegistrationCode { get; private set; }
		Dictionary<ZGuid, ZString> AlternateGLAccountMappings { get; set; }

		#endregion

		internal protected override Dictionary<ZGuid, TransactionHeaderDetails> GetTransactionHeaderDetails(AccComplianceReport report)
		{
			var result = new Dictionary<ZGuid, TransactionHeaderDetails>();

			if (ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly() || ShouldPopulateAdditionalHeaderDetails(report))
			{
				var newFactory = new BusinessObjectFactory();
				var details = new DynamicBusinessObjectCollection(newFactory);
				var extraColumnsBuilder = new ZStringBuilder();
				var extraConditionsBuilder = new ZStringBuilder();
				var extraTablesBuilder = new ZStringBuilder();

				if (!ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly())
				{
					extraColumnsBuilder.Append(@", header.AH_InvoiceDate, originalTransaction.AH_TransactionNum OriginalTransactionNumber, originalTransaction.AH_TransactionReference OriginalTransactionReference,
			header.AH_TransactionCategory,
			header.AH_ChequeOrReference, header.AH_ReceiptType, header.AH_AB, header.AH_OH,
			A1_BankAccount, A1_BankName, A1_RN_NKCountryCode, RN_Desc, 
			header.AH_IsCancelled, originalTransaction.AH_IsCancelled IsOriginalTransactionCancelled,
			header.AH_RX_NKTransactionCurrency, RX_Desc, CurrencyDecimals = LEN(RX_SubUnitRatio) - 1,
			header.AH_ExchangeRate, header.AH_OSTotal,
			header.AH_SystemLastEditUser LastEditUserCode, editUser.GS_FullName LastEditUserName, DATEADD(MINUTE, ISNULL(editTimeOffset.Offset, 0), header.AH_SystemLastEditTimeUtc) AS LastEditTime,
			header.AH_DigitalSignature_COMPRESSED, ISNULL(reversal.AH_IsCancelled, 0) IsReversedAmendment");

					extraTablesBuilder.Append(@"OUTER APPLY dbo.GetTimeZoneOffsetInMinutes(branch.GB_RL_NKHomePort, header.AH_SystemLastEditTimeUtc) editTimeOffset
			LEFT JOIN dbo.AccTransactionHeader originalTransaction ON header.AH_TransactionBelongsToGroup = originalTransaction.AH_PK
			LEFT JOIN dbo.RefCurrency ON header.AH_RX_NKTransactionCurrency = RX_Code
			LEFT JOIN dbo.OrgCompanyData ON OB_OH = header.AH_OH AND OB_GC = @OB_GC
			LEFT JOIN dbo.AccAPAccountDetails ON A1_OB = OB_PK AND header.AH_TransactionType = 'PAY' AND header.AH_Ledger = 'AP' AND A1_RX_NKAccountCurrency = header.AH_RX_NKTransactionCurrency AND A1_IsDefaultAccount = 1 AND A1_PaymentMethod = 'DDR'
			LEFT JOIN dbo.RefCountry ON A1_RN_NKCountryCode = RN_Code
			LEFT JOIN dbo.GlbStaff editUser ON editUser.GS_Code = header.AH_SystemLastEditUser
			LEFT JOIN dbo.AccTransactionHeader reversal ON header.AH_IsCancelled = 1 AND header.AH_TransactionBelongsToGroup IS NOT NULL AND reversal.AH_TransactionBelongsToGroup = header.AH_PK");
				}

				if (AdditionalDataProvider is IComplianceReportAdditionalDataHeaderDetailsProvider provider)
				{
					var tupleResult = provider.GetHeaderExtraColumnsAndConditions(DataCollectionMode);
					extraColumnsBuilder.Append(tupleResult.extraColumns);
					extraConditionsBuilder.Append(tupleResult.extraConditions);
				}

				var extraColumns = extraColumnsBuilder.ToString();
				var extraConditions = extraConditionsBuilder.ToString();
				var extraTables = extraTablesBuilder.ToString();

				var sql = FormattableString.Invariant($@"
		WITH
		AllHeaders
		AS
		(
			{(report.IsUsingGLDTablePrefix ? GetAllHeadersSqlGLD : GetAllHeadersSql )}
		),
		HeaderSequences
		AS
		(
			SELECT AH_PK, HeaderSequence = MAX(Sequence)
			FROM AllHeaders
			WHERE ParentTableCode = 'AH'
			GROUP BY AH_PK

			UNION ALL

			SELECT AH_PK, HeaderSequence = MIN(Sequence)
			FROM AllHeaders A1
			WHERE ParentTableCode = 'AL'
				AND NOT EXISTS (
					SELECT 1
					FROM AllHeaders A2
					WHERE ParentTableCode = 'AH' AND A2.AH_PK = A1.AH_PK
				)
			GROUP BY AH_PK
		)
		SELECT header.AH_PK, HeaderSequence,
		header.AH_Desc, header.AH_SystemCreateUser CreateUserCode, 
		createUser.GS_FullName CreateUserName, 
		DATEADD(MINUTE, ISNULL(createTimeOffset.Offset, 0), header.AH_SystemCreateTimeUtc) AS createTime
		{extraColumns}
		FROM HeaderSequences
			JOIN dbo.AccTransactionHeader header ON header.AH_PK = HeaderSequences.AH_PK
			LEFT JOIN dbo.GlbStaff createUser ON createUser.GS_Code = header.AH_SystemCreateUser
			LEFT JOIN dbo.GlbBranch branch ON branch.GB_PK = header.AH_GB
			OUTER APPLY dbo.GetTimeZoneOffsetInMinutes(branch.GB_RL_NKHomePort, header.AH_SystemCreateTimeUtc) createTimeOffset
		{extraTables}
		{extraConditions}
		");

				var parameters = new List<ZSqlParameter>();
				parameters.Add(ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK));
				parameters.Add(ZSqlParameter.New("@OB_GC", report.Company.PK, OrgCompanyDataSchema.OB_GC));
				if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
				{
					parameters.Add(ZSqlParameter.New("@SelectedSupplierPK", SelectedSupplierPK, AccTransactionHeaderSchema.AH_OH));
				}

				details.Load(sql, parameters.ToArray());

				ZInt lineNumOffSet = report.LineNumOffset;
				var isUsedHeaderDetailsProvider = AdditionalDataProvider is IComplianceReportAdditionalDataHeaderDetailsProvider;
				foreach (DynamicBusinessObject row in details)
				{
					var headerPK = (ZGuid)row["AH_PK"]; // Direct access to datRow is required
					var headerSequence = (ZInt)(row["HeaderSequence"]); // Direct access to datRow is required

					var detailsToAdd = new TransactionHeaderDetails()
					{
						HeaderSequence = headerSequence >= 0 ? (ZInt)(headerSequence + lineNumOffSet) : headerSequence,   // Direct access to datRow is required
						Description = (ZString)row[AccTransactionHeaderSchema.Constants.AH_Desc],
						CreateUserCode = (ZString)row["CreateUserCode"],    // Direct access to datRow is required
						CreateUserName = (ZString)row["CreateUserName"],    // Direct access to datRow is required
						CreateTime = isUsedHeaderDetailsProvider ? ((ZDateTime)row["HighPrecisionAddDateTime"]) : ((ZDateTime)row["CreateTime"])    // Direct access to datRow is required
					};
					if (!ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly())
					{
						detailsToAdd.InvoiceDate = (ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_InvoiceDate];
						detailsToAdd.OriginalTransactionNumber = (ZString)row["OriginalTransactionNumber"];  // Direct access to datRow is required
						detailsToAdd.OriginalTransactionReference = (ZString)row["OriginalTransactionReference"];  // Direct access to datRow is required
						detailsToAdd.TransactionCategory = (ZString)row[AccTransactionHeaderSchema.Constants.AH_TransactionCategory];
						detailsToAdd.ChequeOrReference = (ZString)row[AccTransactionHeaderSchema.Constants.AH_ChequeOrReference];
						detailsToAdd.ReceiptType = (ZString)row[AccTransactionHeaderSchema.Constants.AH_ReceiptType];
						detailsToAdd.TransactionCurrency = (ZString)row[AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency];
						detailsToAdd.TransactionCurrencyDesc = (ZString)row[RefCurrencySchema.Constants.RX_Desc];
						detailsToAdd.TransactionCurrencyDecimals = (ZInt)row["CurrencyDecimals"];   // Direct access to datRow is required
						detailsToAdd.ExchangeRate = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_ExchangeRate];
						detailsToAdd.OSTotal = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_OSTotal];
						detailsToAdd.DebitBankAccount = (ZGuid)row[AccTransactionHeaderSchema.Constants.AH_AB];
						detailsToAdd.OrgPK = (ZGuid)row[AccTransactionHeaderSchema.Constants.AH_OH];
						detailsToAdd.CreditBankAccount = (ZString)row[AccAPAccountDetailsSchema.Constants.A1_BankAccount];
						detailsToAdd.BankName = (ZString)row[AccAPAccountDetailsSchema.Constants.A1_BankName];
						detailsToAdd.CountryCode = (ZString)row[AccAPAccountDetailsSchema.Constants.A1_RN_NKCountryCode];
						detailsToAdd.CountryDesc = (ZString)row[RefCountrySchema.Constants.RN_Desc];   // Direct access to object required
						detailsToAdd.IsCancelled = (ZBool)row[AccTransactionHeaderSchema.Constants.AH_IsCancelled];
						detailsToAdd.IsOriginalTransactionCancelled = row["IsOriginalTransactionCancelled"] != DBNull.Value ? (ZBool?)row["IsOriginalTransactionCancelled"] : null;    // Direct access to datRow is required
						detailsToAdd.LastEditUserCode = (ZString)row["LastEditUserCode"];    // Direct access to datRow is required
						detailsToAdd.LastEditUserName = (ZString)row["LastEditUserName"];    // Direct access to datRow is required
						detailsToAdd.LastEditTime = ((ZDateTime)row["LastEditTime"]);    // Direct access to datRow is required
						detailsToAdd.DigitalSignature = !((ZBlob)row[AccTransactionHeaderSchema.Constants.AH_DigitalSignature_COMPRESSED]).IsEmpty ? Convert.ToBase64String((ZBlob)row[AccTransactionHeaderSchema.Constants.AH_DigitalSignature_COMPRESSED]) : String.Empty;
						detailsToAdd.IsReversedAmendment = (ZBool)row["IsReversedAmendment"];

						if (isUsedHeaderDetailsProvider)
						{
							detailsToAdd.InvoiceTerm = (ZString)row[AccTransactionHeaderSchema.Constants.AH_InvoiceTerm];
							detailsToAdd.InvoiceTermDays = (ZByte)row[AccTransactionHeaderSchema.Constants.AH_InvoiceTermDays];
							detailsToAdd.SourceReference = (ZString)row[AccTransactionHeaderReferenceSchema.Constants.AH1_Reference];
							detailsToAdd.OriginalReferenceSourceReference = (ZString)row["OriginalReferenceSourceReference"];
							detailsToAdd.OriginalReferenceComplianceSubType = (ZString)row["OriginalReferenceComplianceSubType"];
							detailsToAdd.OriginalReferenceReversalReason = (ZString)row[GenAddOnColumnSchema.Constants.XA_Data];
							detailsToAdd.OriginalReferenceStartDate = (ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_OriginalReferenceStartDate];
							detailsToAdd.OriginalReferenceEndDate = (ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_OriginalReferenceEndDate];
							detailsToAdd.AgreedPaymentMethodOverride = (ZString)row[AccTransactionHeaderSchema.Constants.AH_AgreedPaymentMethodOverride];
							detailsToAdd.DueDate = (ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_DueDate];
						}
					}

					result.Add(headerPK, detailsToAdd);

					if (IsSAFTVersion110OrAbove)
					{
						CollectHeaderDescriptions(headerPK, detailsToAdd.Description);
					}
				}
			}

			return result;
		}

		string GetAllHeadersSql => @"SELECT AH_PK, ParentTableCode = 'AH', Sequence = MIN(ACL_ReportSequence)
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccTransactionHeader ON AH_PK = ACL_ParentID 
			WHERE ACL_ACR_Report = @ReportPK AND ACL_ParentTableCode = 'AH'
			GROUP BY AH_PK

			UNION ALL

			SELECT DISTINCT AH_PK = AL_AH, ParentTableCode = 'AL', Sequence = ACL_ReportSequence
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccTransactionLines ON AL_PK = ACL_ParentID 
			WHERE ACL_ACR_Report = @ReportPK AND ACL_ParentTableCode = 'AL' AND AL_AH IS NOT NULL";

		string GetAllHeadersSqlGLD => @"SELECT AH_PK, ParentTableCode = 'AH', Sequence = MIN(ACL_ReportSequence)
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccGeneralLedgerData ON GLD_PK = ACL_ParentID
				JOIN AccTransactionHeader on AH_PK = GLD_AH_TransactionHeader
			WHERE ACL_ACR_Report = @ReportPK AND GLD_AH_TransactionHeader IS NOT NULL AND GLD_AL_TransactionLine is null and GLD_YC_CashBasisVAT IS NULL
			GROUP BY AH_PK

			UNION ALL

			SELECT AH_PK, ParentTableCode = 'AH', Sequence = MIN(ACL_ReportSequence)
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccGeneralLedgerData ON GLD_PK = ACL_ParentID
				JOIN AccTransactionHeader on AH_PK = GLD_AH_TransactionHeader
			WHERE ACL_ACR_Report = @ReportPK AND GLD_AH_TransactionHeader IS NOT NULL AND GLD_AL_TransactionLine IS NOT NULL AND GLD_YC_CashBasisVAT IS NULL
				AND AH_TransactionType IN ('INV', 'CRD', 'ADJ', 'DRC','DPY')
			GROUP BY AH_PK

			UNION ALL

			SELECT DISTINCT AH_PK = AL_AH, ParentTableCode = 'AL', Sequence = ACL_ReportSequence
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccGeneralLedgerData ON GLD_PK = ACL_ParentID
				JOIN dbo.AccTransactionLines ON AL_PK = GLD_AL_TransactionLine
			WHERE ACL_ACR_Report = @ReportPK AND GLD_AH_TransactionHeader IS NOT NULL AND GLD_AL_TransactionLine IS NOT NULL AND GLD_YC_CashBasisVAT is null AND AL_AH IS NOT NULL";

		Dictionary<ZString, GLAccount> GetGLAccounts(AccComplianceReport report)
		{
			var result = new Dictionary<ZString, GLAccount>();

			var glAccounts = report.Factory.Load<AccGLHeader>(new ZQuery());
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(report.Company.GC_RN_NKCountryCode) as IInstanceProvider<IReportSAFTWriter>)?.Get();

			foreach (var account in glAccounts)
			{
				var dataObject = new GLAccount()
				{
					AccountCode = account.AG_AccountNum,
					Description = account.AG_DescriptionMultilingual,
					CreationDate = account.AG_SystemCreateTimeUtc,
					LocalComplianceAccountCode = DataCollectionMode == ComplianceReportDataCollectionMode.SAFT1_10 && !(provider?.GetLocalLanguage ?? ZString.Empty).IsEmpty
						? AccGLAccountDescriptor.GetLocalAccountDescriptor(account.Factory, account.PK, provider.GetLocalLanguage)?.AJ_LocalAccountNumber ?? ZString.Empty
						: account.LocalAccountNumber,
					AccountType = new UniversalCodeDescription() { Code = account.AG_AccountType },
					ConsolidationAccountCode = account.ConsolidationNum?.AG_AccountNum,
					DebitCredit = account.AG_DebitCredit == "DEB" ? UniversalDebitCredit.Debit : UniversalDebitCredit.Credit
				};
				result[account.AG_AccountNum] = dataObject;
			}

			return result;
		}

		Dictionary<ZInt, TransactionLineDetails> GetTransactionLineDetails(AccComplianceReport report)
		{
			var result = new Dictionary<ZInt, TransactionLineDetails>();

			if (ShouldPopulateAdditionalHeaderDetails(report))
			{
				var newFactory = new BusinessObjectFactory();
				var details = new DynamicBusinessObjectCollection(newFactory);
				var extraJoin = string.Empty;
				var extraCondition = new ZStringBuilder();

				if (AdditionalDataProvider is IComplianceReportAdditionalDataLineDetailsProvider provider)
				{
					var tupleResult = provider.GetLineExtraJoinsAndConditions(DataCollectionMode);
					extraJoin = tupleResult.extraJoin;
					extraCondition.Append(tupleResult.extraConditions);
				}

				var sql = string.Format(report.IsUsingGLDTablePrefix ? GetTransactionLineDetailsSqlGLD : GetTransactionLineDetailsSqlDefault, extraJoin, extraCondition);

				var parameters = new List<ZSqlParameter>();
				parameters.Add(ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK));
				if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
				{
					parameters.Add(ZSqlParameter.New("@SelectedSupplierPK", SelectedSupplierPK, AccTransactionLinesSchema.AL_OH));
				}

				details.Load(sql, parameters.ToArray());

				ZInt lineNumOffSet = report.LineNumOffset;
				foreach (DynamicBusinessObject row in details)
				{
					var chargeCode = (ZString)row[AccChargeCodeSchema.Constants.AC_Code];
					var glAccountNumber = (ZString)row[AccGLHeaderSchema.Constants.AG_AccountNum];
					if (!chargeCode.IsEmpty)
					{
						UsedChargeCodes.Add(chargeCode);
					}
					else if (!glAccountNumber.IsEmpty)
					{
						UsedGLAccounts.Add(glAccountNumber);
					}

					if (IsSAFTVersion110OrAbove)
					{
						var glAccountPK = (ZGuid)row[AccGLHeaderSchema.Constants.PK];
						CollectGLAccountTypes(glAccountPK, (ZString)row[AccGLHeaderSchema.Constants.AG_AccountType]);

						var invMsgPK = (ZGuid)row[AccInvMsgSchema.Constants.PK];
						CollectTaxGroupCodes(invMsgPK, (ZString)row[AccInvMsgSchema.Constants.A9_TaxGroupCode]);
					}

					var reportSequence = (ZInt)(row["ACL_ReportSequence"]) + lineNumOffSet;
					if (!result.ContainsKey(reportSequence))
					{
						result.Add(reportSequence,
							new TransactionLineDetails()
							{
								LineSequence = (ZShort)row[AccTransactionLinesSchema.Constants.AL_Sequence],
								ChargeCode = chargeCode,
								GLAccountNum = glAccountNumber,
								TaxMsgTaxGroupCode = (ZString)row[AccInvMsgSchema.Constants.A9_TaxGroupCode],
								GSTVATBasis = (ZString)row[AccTransactionLinesSchema.Constants.AL_GSTVATBasis],
								TaxDate = (ZDateTime)row[AccTransactionLinesSchema.Constants.AL_TaxDate],
								Description = (ZString)row[AccTransactionLinesSchema.Constants.AL_Desc],
								OSExTaxAmount = (ZDecimal)row["AL_OSExTaxAmount"],  // Direct access to datRow is required
								WithholdingTaxPK = row[AccTransactionLinesSchema.Constants.AL_AW] != DBNull.Value ? (ZGuid)row[AccTransactionLinesSchema.Constants.AL_AW] : ZGuid.Empty,
								WithholdingTaxAmount = (ZDecimal)row[AccTransactionLinesSchema.Constants.AL_WithholdingTax],
							});
					}
				}
			}

			return result;
		}

		string GetTransactionLineDetailsSqlDefault =>
			@"SELECT DISTINCT ACL_ReportSequence, AL_Sequence, AL_GSTVATBasis, AL_Desc, CAST (AL_OSAmount - 
		CASE
			WHEN GC_IsReciprocal = 1 THEN
				ROUND(AL_GSTVAT / AL_ExchangeRate, LEN(RX_SubUnitRatio) - 1)
			ELSE
				ROUND(AL_GSTVAT * AL_ExchangeRate, LEN(RX_SubUnitRatio) - 1)
		END AS MONEY) AS AL_OSExTaxAmount,
		AL_TaxDate,
		AC_Code,
		AG_PK,
		AG_AccountType,
		AG_AccountNum,
		A9_PK,
		A9_TaxGroupCode,
		AL_AW,
		AL_WithholdingTax
FROM dbo.AccComplianceReportTransactionPivot
JOIN dbo.AccTransactionLines ON AL_PK = ACL_ParentID
JOIN dbo.GlbCompany ON AL_GC = GC_PK
JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
LEFT JOIN AccChargeCode ON AC_PK = AL_AC
LEFT JOIN AccGLHeader ON AG_PK = AL_AG
LEFT JOIN AccInvMsg ON A9_PK = AL_A9_VATClass
{0}
WHERE ACL_ACR_Report = @ReportPK
{1}";

		string GetTransactionLineDetailsSqlGLD =>
			@"SELECT DISTINCT ACL_ReportSequence, AL_Sequence, AL_GSTVATBasis, AL_Desc, CAST (AL_OSAmount - 
		CASE
			WHEN GC_IsReciprocal = 1 THEN
				ROUND(AL_GSTVAT / AL_ExchangeRate, LEN(RX_SubUnitRatio) - 1)
			ELSE
				ROUND(AL_GSTVAT * AL_ExchangeRate, LEN(RX_SubUnitRatio) - 1)
		END AS MONEY) AS AL_OSExTaxAmount,
		AL_TaxDate,
		AC_Code,
		AG_PK,
		AG_AccountType,
		AG_AccountNum,
		A9_PK,
		A9_TaxGroupCode,
		AL_AW,
		AL_WithholdingTax
FROM dbo.AccComplianceReportTransactionPivot
JOIN dbo.AccGeneralLedgerData ON GLD_PK = ACL_ParentID
JOIN dbo.AccTransactionLines ON AL_PK = GLD_AL_TransactionLine
JOIN dbo.GlbCompany ON AL_GC = GC_PK
JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
LEFT JOIN AccChargeCode ON AC_PK = AL_AC
LEFT JOIN AccGLHeader ON AG_PK = AL_AG
LEFT JOIN AccInvMsg ON A9_PK = AL_A9_VATClass
{0}
WHERE ACL_ACR_Report = @ReportPK AND GLD_AL_TransactionLine IS NOT NULL AND GLD_YC_CashBasisVAT IS NULL
{1}";

		#region SAFT-PT

		Dictionary<ZString, GLAccountDetails> GetActiveGLAccounts(AccComplianceReport report)
		{
			var movementDetails = report.GLMovementDetails.Cast<GeneralLedgerBalanceLine>();
			if (IsSAFTVersion110OrAbove)
			{
				movementDetails = report.GLMovementDetailsExport.Cast<GeneralLedgerBalanceLine>();
			}

			var result = ActiveGLAccounts ?? (ActiveGLAccounts = new Dictionary<ZString, GLAccountDetails>());

			Func<ZDecimal, ZDecimal, ZDecimal> calculateTotal = (total, oppositeTotal) =>
			{
				return total > oppositeTotal ? total - oppositeTotal : decimal.Zero;
			};

			var activeGLAccounts = from openingBalance in report.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>()
								   join balanceMovement in movementDetails
								   on openingBalance.AG_AccountNum equals balanceMovement.AG_AccountNum
								   where !(openingBalance.GeneralLedgerAmountDR.IsEmpty && openingBalance.GeneralLedgerAmountCR.IsEmpty && balanceMovement.GeneralLedgerAmountDR.IsEmpty && balanceMovement.GeneralLedgerAmountCR.IsEmpty)
								   select new GLAccountDetails()
								   {
									   GLAccount = GLAccounts[openingBalance.AG_AccountNum],
									   GLAccountPK = openingBalance.AG_PK,
									   OpeningDebitBalance = openingBalance.GeneralLedgerAmountDR,
									   OpeningCreditBalance = openingBalance.GeneralLedgerAmountCR,
									   ClosingDebitBalance = calculateTotal(openingBalance.GeneralLedgerAmountDR + balanceMovement.GeneralLedgerAmountDR, openingBalance.GeneralLedgerAmountCR + balanceMovement.GeneralLedgerAmountCR),
									   ClosingCreditBalance = calculateTotal(openingBalance.GeneralLedgerAmountCR + balanceMovement.GeneralLedgerAmountCR, openingBalance.GeneralLedgerAmountDR + balanceMovement.GeneralLedgerAmountDR)
								   };

			CollectAlternateGLAccountMappings(report);

			foreach (var account in activeGLAccounts)
			{
				var accountCode = account.GLAccount.AccountCode.Value;
				if (result.ContainsKey(accountCode))
				{
					result[accountCode].ClosingDebitBalance = account.ClosingDebitBalance;
					result[accountCode].ClosingCreditBalance = account.ClosingCreditBalance;
				}
				else
				{
					account.AlternateGLAccountNumber = AlternateGLAccountMappings?.GetValueSafe(account.GLAccountPK) ?? string.Empty;
					result.Add(accountCode, account);
				}
			}

			return result;
		}

		HashSet<ZString> GetCustomerCodes(AccComplianceReport report)
		{
			var result = CustomerCodes ?? (CustomerCodes = new HashSet<ZString>());

			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				result.Add(report.Company.GC_Code);
			}
			else
			{
				result.UnionWith(report.ReportLines.Cast<AccComplianceReportLine>().Where(x => !x.OH_Code.IsEmpty
				   && (AdditionalDataProvider?.GetIsValidForCustomers(x.AH_Ledger, x.AH_TransactionType) ?? true))
				   .Select(x => x.OH_Code).Distinct());
			}

			return result;
		}

		HashSet<ZString> GetSupplierCodes(AccComplianceReport report)
		{
			var result = SupplierCodes ?? (SupplierCodes = new HashSet<ZString>());

			result.UnionWith(report.ReportLines.Cast<AccComplianceReportLine>().Where(x => !x.OH_Code.IsEmpty && CheckSupplierCodeIfApplicable(x)
				&& (AdditionalDataProvider?.GetIsValidForSuppliers(x.AH_Ledger, x.AH_TransactionType) ?? true))
				.Select(x => x.OH_Code).Distinct());

			return result;
		}

		protected override void AddMoreOrgCodes(HashSet<ZString> orgCodes)
		{
			base.AddMoreOrgCodes(orgCodes);
			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				orgCodes.Add(SelectedSupplierCode);
			}
		}

		internal UniversalOrgAddress CompanyOrgAddress
		{
			get
			{
				UniversalOrgAddress result = null;

				if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFT || IsSAFTVersion110OrAbove)
				{
					result = CreateReportCompanyOrgAddress();
				}
				else if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
				{
					OrgAddresses.TryGetValue(SelectedSupplierCode, out result);
				}

				return result;
			}
		}

		protected override bool CollectsTaxRegistrationNumbers
			=> DataCollectionMode == ComplianceReportDataCollectionMode.Esterometro || IsAnySAFT;

		void GetCustomerSalesInvoices(Dictionary<ZGuid, IEnumerable<AccComplianceReportLine>> salesInvoices)
		{
			var result = CustomerSalesInvoices ?? (CustomerSalesInvoices = new Dictionary<ZString, List<ZGuid>>());

			foreach (var salesInvoice in salesInvoices)
			{
				var customerCode = salesInvoice.Value.Select(x => x.OH_Code).FirstOrDefault();
				var salesInvoicePK = salesInvoice.Key;
				if (result.ContainsKey(customerCode))
				{
					result[customerCode].Add(salesInvoicePK);
				}
				else
				{
					result[customerCode] = new List<ZGuid> { salesInvoicePK };
				}
			}
		}

		Dictionary<ZGuid, IEnumerable<AccComplianceReportLine>> GetSalesInvoices(AccComplianceReport report, Dictionary<ZGuid, TransactionHeaderDetails> headerData)
		{
			var invoices = report.ReportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_Ledger == SAFTLedger && CheckSupplierCodeIfApplicable(x)
				&& (x.AH_TransactionType == TransactionTypes.Invoice || x.AH_TransactionType == TransactionTypes.CreditNote))
				.GroupBy(x => x.AH_PK).ToDictionary(x => x.Key, x => x.OrderBy(z => z.ACL_ReportSequence).AsEnumerable());

			bool checkSelfBillingOnHeader(TransactionHeaderDetails header) => header != null
				&& ((DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling && header.IsSelfBilling) || (DataCollectionMode == ComplianceReportDataCollectionMode.SAFT && !header.IsSelfBilling));

			return invoices.Where(x =>
				headerData.ContainsKey(x.Key)
				&& checkSelfBillingOnHeader(headerData[x.Key])
				&& (report.IsTransactionLinesBased || headerData[x.Key].HeaderSequence >= 0)
			).ToDictionary(x => x.Key, x => x.Value);
		}

		void GetSalesInvoicesReferences(AccComplianceReport report, IEnumerable<ZGuid> salesInvoicesPKs)
		{
			var collection = GetHeaderReferences(report.Factory, salesInvoicesPKs);
			GetPostedWithoutIVASalesInvoicePKs(collection);    // Merge
			GetHeaderAuthorizationNumberReferences(collection);    // Override
		}

		void GetPostedWithoutIVASalesInvoicePKs(DynamicBusinessObjectCollection collection)
		{
			var referencesType = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IVA;
			var result = PostedWithoutIVASalesInvoicePKs ?? (PostedWithoutIVASalesInvoicePKs = new HashSet<ZGuid>());
			result.UnionWith
			(
				collection.Where(x => ((ZString)x[AccTransactionHeaderReferenceSchema.AH1_Type]).Equals(referencesType))
					.Select(x => (ZGuid)x[AccTransactionHeaderReferenceSchema.AH1_AH]).Distinct()
			);
		}

		void GetHeaderAuthorizationNumberReferences(DynamicBusinessObjectCollection collection)
		{
			var referencesType = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH;
			HeaderAuthorizationNumberReferences = new Dictionary<ZGuid, ZString>();
			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFT)
			{
				collection.Where(x => ((ZString)x[AccTransactionHeaderReferenceSchema.AH1_Type]).Equals(referencesType))
					.ForEach(x => HeaderAuthorizationNumberReferences[(ZGuid)x[AccTransactionHeaderReferenceSchema.AH1_AH]] = (ZString)x[AccTransactionHeaderReferenceSchema.AH1_Reference]);
			}
		}

		DynamicBusinessObjectCollection GetHeaderReferences(BusinessObjectFactory factory, IEnumerable<ZGuid> salesInvoicesPKs)
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			var parameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@HeaderPKs", salesInvoicesPKs.ToArray(), AccTransactionHeaderReferenceSchema.AH1_AH, true)
			};

			var sqlText = $@"SELECT AH1_AH, AH1_Type, AH1_Reference
								FROM dbo.AccTransactionHeaderReference
								WHERE
									AH1_AH IN (SELECT Value FROM @HeaderPKs) AND
									AH1_Type IN ('{AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH}', '{AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IVA}') "; // Hardcoded part of SQL statement
			collection.Load(sqlText, parameters);
			return collection;
		}

		bool CheckSupplierCodeIfApplicable(AccComplianceReportLine line) => DataCollectionMode != ComplianceReportDataCollectionMode.SAFTSelfBilling || line.OH_Code == SelectedSupplierCode;

		(ZInt NumberOfEntries, ZDecimal TotalDebit, ZDecimal TotalCredit) GetSalesInvoicesTotals(AccComplianceReport report, Dictionary<ZGuid, IEnumerable<AccComplianceReportLine>> invoices, Dictionary<ZGuid, TransactionHeaderDetails> headerData)
		{
			var linesForTotals = report.IsTransactionLinesBased
				? invoices.SelectMany(x => x.Value).Select(x => new { Sequence = x.ACL_ReportSequence, TransactionType = x.AH_TransactionType, Amount = x.TotalExTaxAmount })
					.GroupBy(x => x.Sequence).ToDictionary(x => x.Key, x => new { TransactionType = x.First().TransactionType, Amount = (ZDecimal)x.Sum(line => line.Amount) })
				: invoices.SelectMany(x => x.Value.Where(y => y.ACL_ReportSequence == headerData[y.AH_PK].HeaderSequence))
					.Select(x => new { Sequence = x.ACL_ReportSequence, TransactionType = x.AH_TransactionType, Amount = x.TotalExTaxAmount })
					.GroupBy(x => x.Sequence).ToDictionary(x => x.Key, x => new { TransactionType = x.First().TransactionType, Amount = x.First().Amount });

			return (SalesInvoicesTotals.NumberOfEntries + new ZInt(invoices.Count),
					SalesInvoicesTotals.TotalDebit + new ZDecimal(linesForTotals.Where(x => x.Value.TransactionType == TransactionTypes.CreditNote).Sum(x => Math.Abs(x.Value.Amount))),
					SalesInvoicesTotals.TotalCredit + new ZDecimal(linesForTotals.Where(x => x.Value.TransactionType == TransactionTypes.Invoice).Sum(x => Math.Abs(x.Value.Amount))));
		}

		void CollectInvoicedTaxIDs(Dictionary<ZGuid, IEnumerable<AccComplianceReportLine>> salesInvoices)
		{
			salesInvoices.SelectMany(x => x.Value).Where(x => !x.AT_Code.IsEmpty).Select(x => x.AT_Code).Distinct().ForEach(x => InvoicedTaxIDs.Add(x));
		}

		Dictionary<ZGuid, AccComplianceReportLine> GetPayments(AccComplianceReport report)
		{
			return report.ReportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_Ledger == LedgerTypes.AccountsReceivable && (x.AH_TransactionType == TransactionTypes.Receipt))
				.GroupBy(x => x.AH_PK).ToDictionary(x => x.Key, x => x.OrderBy(z => z.ACL_ReportSequence).First());
		}

		(ZInt NumberOfEntries, ZDecimal TotalDebit, ZDecimal TotalCredit) GetPaymentsTotals(Dictionary<ZGuid, AccComplianceReportLine> payments)
		{
			return (PaymentsTotals.NumberOfEntries + new ZInt(payments.Keys.Count),
					PaymentsTotals.TotalDebit + new ZDecimal(payments.Where(x => Math.Sign(x.Value.TotalExTaxAmount) == -1).Sum(x => -x.Value.TotalExTaxAmount)),
					PaymentsTotals.TotalCredit + new ZDecimal(payments.Where(x => Math.Sign(x.Value.TotalExTaxAmount) == 1).Sum(x => x.Value.TotalExTaxAmount)));
		}

		#endregion

		#region SAFT 1_10 & 1_30

		ZString GetDefaultReceiptAccountNumber(BusinessObjectFactory factory, ZString localCurrencyCode, ZGuid companyPK)
		{
			if (fBankAccounts == null)
			{
				var filter = new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True);
				filter.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, localCurrencyCode);
				filter.AddToFilter(AccBankAccountSchema.AB_GC, companyPK);
				filter.AddToFilter(AccBankAccountSchema.AB_GB, null);
				fBankAccounts = factory.Load<AccBankAccount>(filter);
			}

			return fBankAccounts?.FirstOrDefault()?.AB_AccountNum ?? ZString.Empty;
		}
		AccBankAccount[] fBankAccounts;

		GlbStaff GetContactStaff(BusinessObjectFactory factory, ZString staffCode)
		{
			return fContact ?? (fContact = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode));
		}
		GlbStaff fContact;

		void CollectHeaderDescriptions(ZGuid headerPK, ZString description)
		{
			var result = HeaderDescriptions ?? (HeaderDescriptions = new Dictionary<ZGuid, ZString>());

			if (!result.ContainsKey(headerPK))
			{
				result.Add(headerPK, description);
			}
		}

		void CollectGLAccountTypes(ZGuid accountPK, ZString accountType)
		{
			var result = GLAccountTypes ?? (GLAccountTypes = new Dictionary<ZGuid, ZString>());

			if (!result.ContainsKey(accountPK))
			{
				result.Add(accountPK, accountType);
			}
		}

		void CollectTaxGroupCodes(ZGuid invMsgPK, ZString taxGroupCode)
		{
			var result = TaxGroupCodes ?? (TaxGroupCodes = new Dictionary<ZGuid, ZString>());

			if (!result.ContainsKey(invMsgPK))
			{
				result.Add(invMsgPK, taxGroupCode);
			}
		}

		void SetOrgContacts(AccComplianceReport report)
		{
			var orgContacts = OrgContacts ?? (OrgContacts = new Dictionary<ZString, OrgContactDependentCollection>());
			var orgHeaders = GetOrgHeadersExceptOrgAddress(report);
			foreach (var orgHeader in orgHeaders)
			{
				orgContacts[orgHeader.OH_Code] = orgHeader.ContactsActive;
			}

			return;
		}

		string GetTaxAccountingBasis() => SAFTComplianceReport130DataProvider.TaxAccountingBasis;
		string GetTaxAuthority() => SAFTComplianceReport130DataProvider.TaxAuthority;
		string GetTaxTableDescription() => SAFTComplianceReport130DataProvider.TaxTableDescription;
		string GetJournalType() => SAFTComplianceReport130DataProvider.JournalType;
		string GetTaxRegistrationCode() => ComplianceInfo.GetConsumptionTaxRegistrationCode();
		string GetBusinessRegistrationCode() => ComplianceInfo.GetBusinessRegistrationCode();

		ISAFTComplianceReport130DataProvider SAFTComplianceReport130DataProvider => ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>()
						.GetFeatureInterface<ISAFTComplianceReport130DataProvider>(Report.Company.GC_RN_NKCountryCode);

		ICountryComplianceInfo ComplianceInfo => ObjectFactory.Get<ICountryComplianceFactoryIntegration>().GetICountryComplianceInfo(Report.Company.GC_RN_NKCountryCode);

		void CollectAlternateGLAccountMappings(AccComplianceReport report)
		{
			var alternateChartId = AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);
			bool hasValidAlternateChart = alternateChartId != Guid.Empty;
			bool isSAFT1_30 = DataCollectionMode == ComplianceReportDataCollectionMode.SAFT1_30;
			bool mappingsAlreadyCollected = AlternateGLAccountMappings != null;

			if (!isSAFT1_30 || !hasValidAlternateChart || mappingsAlreadyCollected)
			{
				return;
			}

			var alternateGLAccounts = report.Factory.Load<AccAlternateGLAccount>(
				new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, alternateChartId))
				.ToDictionary(a => a.PK, a => a.AccountNumWithSeparator);

			var alternateGLAccountAttributes = report.Factory.Load<AccAlternateGLAccountAttribute>(
				new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, alternateChartId))
				.GroupBy(a => a.AAA_AG_GLHeader)
				.ToDictionary(g => g.Key, g => g.First().AAA_AGA_AlternateGLAccount);

			AlternateGLAccountMappings = new Dictionary<ZGuid, ZString>();

			foreach (var attribute in alternateGLAccountAttributes)
			{
				if (alternateGLAccounts.TryGetValue(attribute.Value, out var alternateGLNumber))
				{
					var glAccountPK = attribute.Key;
					AlternateGLAccountMappings[glAccountPK] = alternateGLNumber;
				}
			}
		}

		#endregion
	}
}
