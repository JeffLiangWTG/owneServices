using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalRegNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalRegNumberType = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumberType;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	[Flags]
	public enum ComplianceReportDataCollectionMode
	{
		None = 0,
		TransactionBatch = 1 << 0,
		SAFT = 1 << 1,
		SAFTSelfBilling = 1 << 2,
		SAFT1_04 = SAFT + SAFTSelfBilling,
		AllSAFT = SAFT1_04 + SAFT1_10 + SAFT1_30,
		Esterometro = 1 << 3,
		Document = 1 << 4,
		SAFT1_10 = 1 << 5,
		JPKV7M = 1 << 6,
		OpenFormatSimplified = 1 << 7,
		SAFT1_30 = 1 << 8
	}

	public abstract class ComplianceReportAdditionalDataCollector : IComplianceReportAdditionalDataCollector
	{
		public ComplianceReportAdditionalDataCollector(AccComplianceReport report, ComplianceReportDataCollectionMode mode = ComplianceReportDataCollectionMode.None)
		{
			Argument.NotNull(report, nameof(report));
			if (!report.IsInDatabase)
			{
				throw new ArgumentException("Compliance Report must be saved to database", nameof(report));   // Dveloper error message
			}

			Report = report;
			DataCollectionMode = mode;
			DateFrom = Report.ACR_DateFrom;
			DateTo = Report.ACR_DateTo;

			AdditionalDataProvider = ComplianceReportAdditionalDataProviderHelper.GetComplianceReportAdditionalDataProvider(DataCollectionMode);

			if (DataCollectionMode != ComplianceReportDataCollectionMode.Document)
			{
				var orgAddressesAndRegistrationNumbers = GetOrgAddressesAndTaxRegistrationNumber(Report);
				OrgAddresses = orgAddressesAndRegistrationNumbers.orgAddresses;
				OrgTaxRegistrationNumberDetails = orgAddressesAndRegistrationNumbers.taxRegistrationNumberDetails;
				TaxIDs = GetTaxIDs(Report);
			}

			if (!ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly())
			{
				UsedChargeCodes = new HashSet<ZString>();
				ChargeCodes = GetChargeCodes(Report);

				if (DataCollectionMode == ComplianceReportDataCollectionMode.TransactionBatch)
				{
					DebitBankAccounts = GetDebitBankAccounts(Report);
				}
				else if (DataCollectionMode == ComplianceReportDataCollectionMode.Esterometro)
				{
					TaxMsgGroupCodes = GetTaxMsgGroupCodes(Report);
					//TO DO 
				}
			}
		}

		public void MergeDataFromAnotherReport(AccComplianceReport report)
		{
			Argument.NotNull(report, nameof(report));
			if (!report.IsInDatabase)
			{
				throw new ArgumentException("Compliance Report must be saved to database", nameof(report));   // Dveloper error message
			}
			if (Report.PK == report.PK)
			{
				throw new ArgumentException("Cannot merge Compliance Report data for the same report", nameof(report));   // Dveloper error message
			}
			Argument.Equals(Report.ACR_GC_Company, report.ACR_GC_Company);
			Argument.Equals(Report.ACR_GB_Branch, report.ACR_GB_Branch);
			Argument.Equals(Report.ACR_ReportType, report.ACR_ReportType);
			if (Report.ACR_DateTo != report.ACR_DateFrom.AddDays(-1))
			{
				throw new ArgumentException("Report to merge into Compliance Report data should continue date range of the current report", nameof(report));   // Dveloper error message
			}

			Report = report;
			DateTo = Report.ACR_DateTo;

			MergeDataFromAnotherReportCore();

			IsMergedData = true;
		}

		protected virtual void MergeDataFromAnotherReportCore()
		{
			throw new NotSupportedException($"Compliance Report does not support merging data from another report. Report type {Report.ACR_ReportType}, country {Report.ReportCountryCode}"); // Dveloper error message
		}

		public abstract IComplianceReportTransactionHeaderDetails GetTransactionHeader(ZGuid headerPK);

		protected AccComplianceReport Report { get; set; }
		internal ComplianceReportDataCollectionMode DataCollectionMode { get; }
		internal ZDate DateFrom { get; }
		internal ZDate DateTo { get; private set; }
		internal bool IsMergedData { get; private set; }

		internal IComplianceReportAdditionalDataProvider AdditionalDataProvider { get; }

		internal protected Dictionary<ZString, UniversalOrgAddress> OrgAddresses { get; protected set; }
		//internal Dictionary<ZString, OrgContactDependentCollection> OrgContacts { get; private set; }
		internal Dictionary<ZString, ZString> OrgFullNames { get; private set; }
		internal Dictionary<ZString, ZDecimal> OrgOpeningDebitBalances { get; private set; }
		internal Dictionary<ZString, ZDecimal> OrgOpeningCreditBalances { get; private set; }
		internal Dictionary<ZString, ZDecimal> OrgClosingDebitBalances { get; private set; }
		internal Dictionary<ZString, ZDecimal> OrgClosingCreditBalances { get; private set; }
		internal Dictionary<ZGuid, BankAccount> DebitBankAccounts { get; private set; }
		//internal Dictionary<ZGuid, TransactionHeaderDetails> HeaderData { get; private set; }
		internal Dictionary<ZInt, TransactionLineDetails> LineData { get; set; }
		internal Dictionary<ZString, ChargeCodeDetails> ChargeCodes { get; private set; }
		internal HashSet<ZString> UsedChargeCodes { get; private set; } // Collected while getting LineData
		internal protected Dictionary<ZString, TaxID> TaxIDs { get; protected set; }
		internal protected Dictionary<ZGuid, TaxID> WithholdingTaxIDs { get; protected set; }
		internal Dictionary<ZInt, ZString> TaxMsgGroupCodes { get; private set; }
		internal Dictionary<ZString, OrgHeaderDetails> OrgHeaderDetails { get; set; }
		internal protected Dictionary<ZString, (ZString, ZString)> OrgTaxRegistrationNumberDetails { get; protected set; }

		#region SAFT and SAFTSelfBilling

		protected HashSet<ZString> InvoicedTaxIDs { get; set; }
		protected HashSet<string> CountryCodesOfEuropeanUnion { get; set; }

		protected bool IsAnySAFT => (DataCollectionMode & ComplianceReportDataCollectionMode.AllSAFT) != 0;
		protected bool IsSAFTVersion110OrAbove => (DataCollectionMode == ComplianceReportDataCollectionMode.SAFT1_10 ||
			DataCollectionMode == ComplianceReportDataCollectionMode.SAFT1_30);
		protected string SAFTLedger => DataCollectionMode == ComplianceReportDataCollectionMode.SAFT
			? LedgerTypes.AccountsReceivable
			: DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling
				? LedgerTypes.AccountsPayable
				: string.Empty;

		#endregion

		internal TransactionLineDetails GetTransactionLine(ZInt sequence)
		{
			return LineData?.GetValueSafe(sequence);
		}

		internal ZString GetTaxMsgGroupCode(AccComplianceReportLine reportLine)
		{
			return TaxMsgGroupCodes.FirstOrDefault(x => x.Key == reportLine.ACL_ReportSequence + Report.LineNumOffset).Value;
		}

		protected (Dictionary<ZString, UniversalOrgAddress> orgAddresses, Dictionary<ZString, (ZString, ZString)> taxRegistrationNumberDetails) GetOrgAddressesAndTaxRegistrationNumber(AccComplianceReport report)
		{
			var orgAddresses = OrgAddresses ?? (OrgAddresses = new Dictionary<ZString, UniversalOrgAddress>());
			var taxRegistrationNumberDetails = OrgTaxRegistrationNumberDetails ?? (OrgTaxRegistrationNumberDetails = new Dictionary<ZString, (ZString, ZString)>());

			var orgHeaders = GetOrgHeadersExceptOrgAddress(report);
			foreach (var orgHeader in orgHeaders)
			{
				orgAddresses[orgHeader.OH_Code] = OrganizationAddressHelper.CreateOrgAddress(DefaultDataObjectWriterStrategy.Instance, orgHeader, report);
				if (CollectsTaxRegistrationNumbers)
				{
					taxRegistrationNumberDetails[orgHeader.OH_Code] = GetPreferredTaxRegistrationNumber(report, orgHeader);
				}
			}

			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				orgAddresses[report.Company.GC_Code] = CreateReportCompanyOrgAddress();
			}

			return (orgAddresses, taxRegistrationNumberDetails);
		}

		protected abstract bool CollectsTaxRegistrationNumbers { get; }

		protected virtual (ZString countryCode, ZString registrationNumber) GetPreferredTaxRegistrationNumber(AccComplianceReport report, OrgHeader orgHeader)
			=> orgHeader.GetCountryCodeAndTaxRegistrationWithoutPrefix(orgHeader.MainAddress);

		protected void SetOrgFullNames(AccComplianceReport report)
		{
			var orgFullNames = OrgFullNames ?? (OrgFullNames = new Dictionary<ZString, ZString>());
			var orgHeaders = GetOrgHeadersExceptOrgAddress(report);
			foreach (var orgHeader in orgHeaders)
			{
				orgFullNames[orgHeader.OH_Code] = orgHeader.OH_FullName;
			}

			return;
		}

		readonly List<string> ValidTransactionTypeList = new List<string>
		{
			TransactionTypes.Invoice,
			TransactionTypes.CreditNote,
			TransactionTypes.AdjustmentNote,
			TransactionTypes.Receipt,
			TransactionTypes.Payment,
			TransactionTypes.Journal,
			TransactionTypes.Contra,
			TransactionTypes.Transfer,
			TransactionTypes.Overpayment,
			TransactionTypes.Discount,
			TransactionTypes.ExchangeDifference
		};

		internal void SetOrgBalance(params AccComplianceReport[] reports)
		{
			if (!IsSAFTVersion110OrAbove)
			{
				return;
			}

			var firstReport = reports.First();
			var orgOpeningDebitBalances = OrgOpeningDebitBalances ?? (OrgOpeningDebitBalances = new Dictionary<ZString, ZDecimal>());
			var orgOpeningCreditBalances = OrgOpeningCreditBalances ?? (OrgOpeningCreditBalances = new Dictionary<ZString, ZDecimal>());
			var orgClosingDebitBalances = OrgClosingDebitBalances ?? (OrgClosingDebitBalances = new Dictionary<ZString, ZDecimal>());
			var orgClosingCreditBalances = OrgClosingCreditBalances ?? (OrgClosingCreditBalances = new Dictionary<ZString, ZDecimal>());
			var orgHeaders = reports.SelectMany(x => GetAllOrgHeaders(x)).DistinctBy(y => y.PK);
			var orgHeaderPKs = string.Join(",", orgHeaders.Select(x => FormattableString.Invariant($"'{x.PK}'"))); // false alert
			var validTransactionTypes = string.Join(",", ValidTransactionTypeList.Select(x => FormattableString.Invariant($"'{x}'"))); // false alertValidTransactionTypeList
			var dateFrom = firstReport.ACR_DateFrom;
			var dateTo = reports.Last().ACR_DateTo.AddDays(1);

			foreach (var date in new[] { dateFrom, dateTo })
			{
				var results = new DynamicBusinessObjectCollection(firstReport.Factory);
				var sql = FormattableString.Invariant($@"SELECT AH_OH, AH_Ledger, SUM(AH_InvoiceAmount + AH_GSTAmount) AS Balance FROM dbo.AccTransactionHeader
WHERE
AH_Ledger IN ('{LedgerTypes.AccountsReceivable}', '{LedgerTypes.AccountsPayable}')
AND AH_TransactionType IN ({validTransactionTypes})
AND AH_PostDate < '{date}'
AND AH_OH IN ({orgHeaderPKs})
AND AH_GC = '{firstReport.ACR_GC_Company}'
GROUP BY AH_Ledger, AH_OH");

				results.Load(sql);

				foreach (DynamicBusinessObject row in results)
				{
					var orgCode = orgHeaders.FirstOrDefault(x => x.PK == (ZGuid)row[AccTransactionHeaderSchema.AH_OH])?.OH_Code ?? ZString.Empty;
					var ledgerType = (ZString)row[AccTransactionHeaderSchema.AH_Ledger];
					var balance = Math.Abs((ZDecimal)row["Balance"]); // Direct access to database required

					if (!orgCode.IsEmpty)
					{
						if (ledgerType == LedgerTypes.AccountsReceivable)
						{
							if (date == dateFrom)
							{
								orgOpeningDebitBalances[orgCode] = balance;
							}
							else
							{
								orgClosingDebitBalances[orgCode] = balance;
							}
						}
						else
						{
							if (date == dateFrom)
							{
								orgOpeningCreditBalances[orgCode] = balance;
							}
							else
							{
								orgClosingCreditBalances[orgCode] = balance;
							}
						}
					}
				}
			}

			return;
		}

		protected OrgHeader[] GetOrgHeadersExceptOrgAddress(AccComplianceReport report)
		{
			orgHeaders = Array.Empty<OrgHeader>();

			var orgCodes = GetOrgCodes(report);
			if (OrgHeaderDetails != null) // Merging in report
			{
				orgCodes.ExceptWith(OrgAddresses.Keys); // Exclude Org Codes for which we already have Org Addresses
			}

			if (orgCodes.Any())
			{
				var orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, orgCodes);
				orgHeaders = report.Factory.Load<OrgHeader>(orgQuery);
			}

			return orgHeaders;
		}

		OrgHeader[] orgHeaders;

		OrgHeader[] GetAllOrgHeaders(AccComplianceReport report)
		{
			var orgHeaders = Array.Empty<OrgHeader>();

			var orgCodes = GetOrgCodes(report);
			if (orgCodes.Any())
			{
				var orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, orgCodes);
				orgHeaders = report.Factory.Load<OrgHeader>(orgQuery);
			}

			return orgHeaders;
		}

		HashSet<ZString> GetOrgCodes(AccComplianceReport report)
		{
			var orgCodes = new HashSet<ZString>();
			orgCodes.Add(report.Company.OrgProxy.OH_Code);
			if (report.Branch != null)
			{
				orgCodes.Add(report.Branch.OrgProxy.OH_Code);
			}

			AddMoreOrgCodes(orgCodes);

			orgCodes.UnionWith(report.ReportLines.Cast<AccComplianceReportLine>().Where(x => !x.OH_Code.IsEmpty).Select(x => x.OH_Code).Distinct());

			return orgCodes;
		}

		protected virtual void AddMoreOrgCodes(HashSet<ZString> orgCodes)
		{ }

		internal UniversalOrgAddress CreateReportCompanyOrgAddress()
		{
			var reportCompany = Report.Company;
			var result = new UniversalOrgAddress(DefaultDataObjectWriterStrategy.Instance)
			{
				OrganizationCode = reportCompany.GC_Code,
				CompanyName = reportCompany.GC_Name,
				Address1 = reportCompany.GC_Address1,
				Address2 = reportCompany.GC_Address2,
				City = reportCompany.City,
				Postcode = reportCompany.Postcode,
				Country = new UniversalCountry() { Code = reportCompany.GC_RN_NKCountryCode },
			};
			result.SetRegistrationNumberCollection(() => new List<UniversalRegNumber>()
			{
				new UniversalRegNumber()
				{
					CountryOfIssue = new UniversalCountry() { Code = reportCompany.GC_RN_NKCountryCode },
					Type = new UniversalRegNumberType() { Code = OrgCusCode.CodeTypes.IVA },
					Value = reportCompany.GC_BusinessRegNo
				}
			});
			return result;
		}

		protected HashSet<string> GetCountryCodesOfEuropeanUnion(AccComplianceReport report)
		{
			var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			var euCountries = new DynamicBusinessObjectCollection(report.Factory);

			var sql = @"SELECT RN_Code FROM dbo.RefCountry WHERE RN_EconomicGrouping = @EUGroupCode";
			var euGroupCode = ZSqlParameter.New("@EUGroupCode", EconomicGroupList.Codes.EuropeanUnion, RefCountrySchema.RN_EconomicGrouping);

			euCountries.Load(sql, new ZSqlParameter[] { euGroupCode });

			foreach (DynamicBusinessObject row in euCountries)
			{
				var countryCode = (ZString)row[RefCountrySchema.RN_Code];
				result.Add(countryCode);
			}

			return result;
		}

		Dictionary<ZString, ChargeCodeDetails> GetChargeCodes(AccComplianceReport report)
		{
			var useLocalChargeCodeDescription = IsAnySAFT && AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value;
			var result = ChargeCodes ?? (ChargeCodes = new Dictionary<ZString, ChargeCodeDetails>());

			var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, report.Company.PK);
			var chargeCodes = report.Factory.Load<AccChargeCode>(chargeCodeQuery);
			foreach (var chargeCode in chargeCodes)
			{
				var dataObject = new ChargeCode() { Code = chargeCode.AC_Code, Description = GetChargeCodeDescription(chargeCode, useLocalChargeCodeDescription) };
				result[chargeCode.AC_Code] = new ChargeCodeDetails() { ChargeCode = dataObject, GoodsServiceType = chargeCode.AC_GoodsServiceType };
			}

			return result;
		}

		string GetChargeCodeDescription(AccChargeCode chargeCode, bool useLocalChargeCodeDescription) => useLocalChargeCodeDescription && !chargeCode.AC_LocalLanguageDescription.IsEmpty
			? chargeCode.AC_LocalLanguageDescription : chargeCode.AC_DescMultilingual;

		protected Dictionary<ZString, TaxID> GetTaxIDs(AccComplianceReport report)
		{
			var result = new Dictionary<ZString, TaxID>();

			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, report.Company.GC_RN_NKCountryCode);
			var taxes = report.Factory.Load<AccTaxRate>(taxQuery);
			foreach (var tax in taxes)
			{
				var taxID = ComplianceReportWriter.GetNewTaxID(tax.AT_Code, tax.AT_Description, tax.AT_Type, tax.GetRate(report.ACR_DateTo));  // We have to provide an actual Rate at the end of report period
				result[tax.AT_Code] = taxID;
			}

			return result;
		}

		internal Dictionary<ZString, TaxID> GetInvoicedTaxIDs() => TaxIDs.Keys.Intersect(InvoicedTaxIDs).ToDictionary(x => x, x => TaxIDs[x]);

		protected Dictionary<ZGuid, TaxID> GetWithholdingTaxIDs(AccComplianceReport report)
		{
			var result = new Dictionary<ZGuid, TaxID>();

			var taxQuery = new ZQuery(AccWithholdingSchema.AW_RN_NKCountry, report.Company.GC_RN_NKCountryCode);
			// Should we also fiter by AW_GC?
			var taxes = report.Factory.Load<AccWithholding>(taxQuery);
			foreach (var tax in taxes)
			{
				var taxID = new TaxID() { TaxCode = tax.AW_Code, Description = tax.AW_Description, TaxRate = tax.AW_Rate };
				result[tax.PK] = taxID;
			}

			return result;
		}

		internal bool IsEUCountryCode(string countryCode) => CountryCodesOfEuropeanUnion?.Contains(countryCode) ?? false;

		Dictionary<ZGuid, BankAccount> GetDebitBankAccounts(AccComplianceReport report)
		{
			var result = new Dictionary<ZGuid, BankAccount>();

			var bankAccounts = new AccBankAccountCollection(report.Factory, report.Company);
			bankAccounts.Load();

			foreach (AccBankAccount account in bankAccounts)
			{
				var bankAccount = new BankAccount()
				{
					AccountNumber = account.AB_AccountNum,
					AccountType = BankAccountType.Debit,
					Country = new UniversalCountry() { Code = account.BankAccountCountry.Code, Name = account.BankAccountCountry.Description },
					BankName = account.AB_BankName
				};
				result[account.PK] = bankAccount;
			}

			return result;
		}

		internal Dictionary<ZString, OrgHeaderDetails> GetOrgHeaderDetails(AccComplianceReport report)
		{
			var result = OrgHeaderDetails ?? (OrgHeaderDetails = new Dictionary<ZString, OrgHeaderDetails>());

			if (ShouldPopulateOrgHeaderDetails(report))
			{
				var newFactory = new BusinessObjectFactory();
				var details = new DynamicBusinessObjectCollection(newFactory);

				var extraCondition = string.Empty;
				var whereCondition = string.Empty;

				if (IsAnySAFT)
				{
					extraCondition = FormattableString.Invariant($@"
		AND AH_Ledger = '{SAFTLedger}'
		AND AH_TransactionType IN('{TransactionTypes.Invoice}', '{TransactionTypes.CreditNote}')");  // Hardcoded part of SQL statement

					whereCondition = FormattableString.Invariant($"WHERE OB_ARCustomerSelfBillsRevenue = 1 OR OB_APCostsSelfBilled = 1");
				}

				var sql = FormattableString.Invariant($@"
		WITH
		ReportOrgPK
		AS
		(
			SELECT DISTINCT OrgPK = AH_OH 
			FROM dbo.ViewComplianceReportLine 
			WHERE ACL_ACR_Report = @ReportPK 
				AND AH_OH IS NOT NULL{extraCondition}
		)

		SELECT OH_Code, OB_ARCustomerSelfBillsRevenue, OB_APCostsSelfBilled, OB_APVATConfig, OB_ARConsolidatedAccountingCategory
		FROM ReportOrgPK
			JOIN dbo.OrgHeader ON OH_PK = ReportOrgPK.OrgPK
			JOIN dbo.OrgCompanyData ON OB_OH = ReportOrgPK.OrgPK AND OB_GC = @OB_GC
		{whereCondition}");

				var pk = ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK);
				var company = ZSqlParameter.New("@OB_GC", report.Company.PK, OrgCompanyDataSchema.OB_GC);

				details.Load(sql, new ZSqlParameter[] { pk, company });

				foreach (DynamicBusinessObject row in details)
				{
					var orgCode = (ZString)row[OrgHeaderSchema.OH_Code];
					if (!result.ContainsKey(orgCode))
					{
						result.Add(orgCode,
							new OrgHeaderDetails()
							{
								ARCustomerSelfBillsRevenue = (ZBool)row[OrgCompanyDataSchema.Constants.OB_ARCustomerSelfBillsRevenue],
								APCostsSelfBilled = (ZBool)row[OrgCompanyDataSchema.Constants.OB_APCostsSelfBilled],
								APVATConfig = (ZString)row[OrgCompanyDataSchema.Constants.OB_APVATConfig],
								ARConsolidatedAccountingCategory = (ZString)row[OrgCompanyDataSchema.Constants.OB_ARConsolidatedAccountingCategory],
							});
					}
				}
			}

			if (DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling && !result.ContainsKey(report.Company.GC_Code))
			{
				result.Add(report.Company.GC_Code,
					new OrgHeaderDetails()
					{
						ARCustomerSelfBillsRevenue = true
					});
			}

			return result;
		}

		Dictionary<ZInt, ZString> GetTaxMsgGroupCodes(AccComplianceReport report)
		{
			var result = new Dictionary<ZInt, ZString>();
			var newFactory = new BusinessObjectFactory();
			var details = new DynamicBusinessObjectCollection(newFactory);

			var sql = @"SELECT DISTINCT ACL_ReportSequence, A9_TaxGroupCode
FROM dbo.ViewComplianceReportLine
LEFT JOIN dbo.AccInvMsg ON A9_PK = AL_A9_VATClass
WHERE ACL_ACR_Report = @ReportPK";
			var pk = ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK);

			details.Load(sql, new ZSqlParameter[] { pk });

			var registryRows = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty);
			var offset = report.LineNumOffset;
			foreach (DynamicBusinessObject row in details)
			{
				var returnCode = ZString.Empty;
				var reportSequence = (ZInt)row["ACL_ReportSequence"] + offset;

				var taxGroupCode = (ZString)row[AccInvMsgSchema.Constants.A9_TaxGroupCode];
				if (!taxGroupCode.IsEmpty)
				{
					if (ShouldUseNewSchema)
					{
						var selRows = registryRows.OfType<CodeDescriptionBoolRelatedItem>().Where(x => (x).Code.Trim() == taxGroupCode);
						returnCode = selRows.Any() ? selRows.First().RelatedItemCode : taxGroupCode;
					}
					else
					{
						returnCode = taxGroupCode.Left(2);
					}
				}

				result.Add(reportSequence, returnCode);
			}

			return result;
		}

		internal static bool ShouldUseNewSchema
			=> AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.Value.CompareTo(ZDate.Today.ToDateTime()) <= 0;

		#region Helper methods

		internal static bool ShouldPopulateAdditionalHeaderDetails(AccComplianceReport report)
			=> report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping
			|| report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.TransactionHeaderWithLines
			|| report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.NoGrouping;

		internal static bool ShouldAddOpeningGLBalance(AccComplianceReport report)
			=> report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBook || report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		protected bool ShouldPopulateOrgHeaderDetails(AccComplianceReport report) =>
			DataCollectionMode == ComplianceReportDataCollectionMode.JPKV7M ||
			(
				(report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping || report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.NoGrouping)
				&& report.Company.Country.Code == Constants.CountryCodes.Portugal
			);

		protected bool ShouldPopulatAdditionalHeaderDetailsWithMinimalDataOnly() =>
			DataCollectionMode == ComplianceReportDataCollectionMode.JPKV7M ||
			(Report.SupportsDayBook &&
				(DataCollectionMode == ComplianceReportDataCollectionMode.Document ||
				(DataCollectionMode == ComplianceReportDataCollectionMode.TransactionBatch && !ShouldPopulateAdditionalHeaderDetails(Report))
			));

		#endregion

		#region Esterometro

		internal IEnumerable<IGrouping<ZGuid, AccComplianceReportLine>> InvoiceLines => invoiceLines ?? (invoiceLines = GetInvoiceLinesCore());
		IEnumerable<IGrouping<ZGuid, AccComplianceReportLine>> invoiceLines;

		IEnumerable<IGrouping<ZGuid, AccComplianceReportLine>> GetInvoiceLinesCore() =>
				Report.ReportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_Ledger == LedgerTypes.AccountsPayable
					&& (x.AH_TransactionType == TransactionTypes.Invoice || x.AH_TransactionType == TransactionTypes.CreditNote || x.AH_TransactionType == TransactionTypes.AdjustmentNote))
					.OrderBy(x => x.ACL_ReportSequence)
					.GroupBy(x => x.AH_PK).ToArray();

		#endregion
	}

	#region Additional Data

	internal class TransactionLineDetails
	{
		public ZShort LineSequence { get; set; }
		public ZString ChargeCode { get; set; }
		public ZString GLAccountNum { get; set; }
		public ZString TaxMsgTaxGroupCode { get; set; }
		public ZString GSTVATBasis { get; set; }
		public ZDateTime TaxDate { get; set; }
		public ZString Description { get; set; }
		public ZDecimal OSExTaxAmount { get; set; }
		public ZGuid WithholdingTaxPK { get; set; }
		public ZDecimal WithholdingTaxAmount { get; set; }
	}

	internal class TransactionHeaderDetails : IComplianceReportTransactionHeaderDetails
	{
		public ZInt HeaderSequence { get; set; }
		public ZDateTime InvoiceDate { get; set; }
		public ZString OriginalTransactionNumber { get; set; }
		public ZString OriginalTransactionReference { get; set; }
		public ZString TransactionCategory { get; set; }
		public ZString ChequeOrReference { get; set; }
		public ZString ReceiptType { get; set; }
		public ZString Description { get; set; }
		public ZString TransactionCurrency { get; set; }
		public ZString TransactionCurrencyDesc { get; set; }
		public ZInt TransactionCurrencyDecimals { get; set; }
		public ZDecimal ExchangeRate { get; set; }
		public ZDecimal OSTotal { get; set; }
		public ZGuid DebitBankAccount { get; set; }
		public ZGuid OrgPK { get; set; }
		public ZString CreditBankAccount { get; set; }
		public ZString BankName { get; set; }
		public ZString CountryCode { get; set; }
		public ZString CountryDesc { get; set; }
		public ZBool IsCancelled { get; set; }
		public ZBool? IsOriginalTransactionCancelled { get; set; }
		public ZString CreateUserCode { get; set; }
		public ZString CreateUserName { get; set; }
		public ZDateTime CreateTime { get; set; }
		public ZString LastEditUserCode { get; set; }
		public ZString LastEditUserName { get; set; }
		public ZDateTime LastEditTime { get; set; }
		public ZString DigitalSignature { get; set; }
		public ZBool IsReversedAmendment { get; set; }
		public ZString InvoiceTerm { get; set; }
		public ZByte InvoiceTermDays { get; set; }
		public ZString SourceReference { get; set; }
		public ZString OriginalReferenceSourceReference { get; set; }
		public ZString OriginalReferenceReversalReason { get; set; }
		public ZString OriginalReferenceComplianceSubType { get; set; }
		public ZDateTime OriginalReferenceStartDate { get; set; }
		public ZDateTime OriginalReferenceEndDate { get; set; }
		public ZString AgreedPaymentMethodOverride { get; set; }
		public ZDateTime DueDate { get; set; }

		public ZBool IsSelfBilling => TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling;
	}

	internal class OrgHeaderDetails
	{
		public ZBool ARCustomerSelfBillsRevenue { get; set; }
		public ZBool APCostsSelfBilled { get; set; }
		public ZString APVATConfig { get; set; }
		public ZString ARConsolidatedAccountingCategory { get; set; }
	}

	internal class ChargeCodeDetails
	{
		public ChargeCode ChargeCode { get; set; }
		public ZString GoodsServiceType { get; set; }
	}

	internal class GLAccountDetails
	{
		public GLAccount GLAccount { get; set; }
		public ZGuid GLAccountPK { get; set; }
		public ZString AccountNum { get; set; }
		public ZString Description { get; set; }
		public ZString AccountType { get; set; }
		public ZString ConsolidationAccountNum { get; set; }
		public ZDecimal OpeningDebitBalance { get; set; }
		public ZDecimal OpeningCreditBalance { get; set; }
		public ZDecimal ClosingDebitBalance { get; set; }
		public ZDecimal ClosingCreditBalance { get; set; }
		public ZDate CreationDate { get; set; }
		public string AlternateGLAccountNumber { get; set; }
	}

	#endregion
}
