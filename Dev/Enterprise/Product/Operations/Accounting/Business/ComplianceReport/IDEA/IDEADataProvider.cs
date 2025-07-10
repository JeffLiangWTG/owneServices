using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	public class IDEADataProvider
	{
		public IDEADataProvider(BusinessObjectFactory factory, AccComplianceReport complianceReport, ILogger serviceLogger)
		{
			Factory = factory;
			this.ComplianceReport = complianceReport;
			this.ServiceLogger = serviceLogger;
		}

		const int CommandTimeout = 1800;  // half an hour

		public const string TaxRegistrationCode = "STE";

		internal const string BalanceAccountsSQL = @"SELECT AG_AccountNum FROM (
														SELECT AG_AccountNum, SUM(AA_Amount) AS Balance
														FROM dbo.AccGLHeader INNER JOIN dbo.AccGLAggregate ON AA_AG = AG_PK
														WHERE AA_GC = @Company
														AND AA_Period < @StartPeriod
														AND AG_AccountType = 'BSH'
														GROUP BY AG_AccountNum
													) i
													WHERE Balance <> 0";

		internal const string SequenceGroupPlaceholder = "<sequence_group>";
		internal const string AccMovementsSQL = $@"DECLARE @ReportType CHAR(3) = 'IDE'

													SELECT CASE WHEN AH_Ledger = 'AP' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN AH_ConsolidatedInvoiceRef
															    ELSE IsNull(AH_TransactionNum, JH_JobNum)
														   END AS Belegnummer,
													AL_Sequence AS Pos,
													CASE WHEN AH_Ledger = 'AP' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN CONCAT(AH_Ledger, '{SequenceGroupPlaceholder}', AH_ConsolidatedInvoiceRef)
														 WHEN AH_Ledger = 'AR' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN CONCAT(AH_Ledger, '{SequenceGroupPlaceholder}', AH_TransactionNum)
														 WHEN AH_TransactionType = 'CTR' THEN CONCAT(AH_TransactionType, AH_TransactionNum)
														 ELSE CONCAT(IsNull(AH_Ledger, SubString(ACQ_ReportSubCode, 2, 2)), IsNull(AH_TransactionType, SubString(ACQ_ReportSubCode, 5, 3)), IsNull(AH_TransactionNum, JH_JobNum))
													END AS ErweiterteBelegnummer,
													JH_JobNum AS JobNummer,
													IsNull(AH_Desc, AL_Desc) AS Buchungstext,
													IsNull(AH_Ledger, SubString(ACQ_ReportSubCode, 2, 2)) AS Rechnungstyp,
													IsNull(AH_TransactionType, SubString(ACQ_ReportSubCode, 5, 3)) AS Transaktionsart,
													ACQ_Date AS Belegdatum,
													CASE WHEN AH_Ledger = 'GL' THEN NULL ELSE IsNull(AH_InvoiceDate, AL_PostDate) END AS Rechnungsdatum,
													CASE WHEN AH_Ledger = 'GL' THEN NULL ELSE AH_DueDate END AS Faelligkeit,
													IsNull(AH_SystemCreateTimeUtc, AL_SystemCreateTimeUtc) AS ErfassungsDatumZeit,
													IsNull(AH_RX_NKTransactionCurrency, AL_RX_NKTransactionCurrency) AS Belegwaehrung,
													OH_Code AS RechnungsStellerEmpfaenger,
													--
													AL_Desc AS Positionsbeschreibung,
													AL_RX_NKTransactionCurrency AS Positionswaehrung,
													CASE WHEN AL_LineAmount <= 0 THEN Abs(AL_LineAmount) ELSE 0 END AS Soll_Fibu,
													CASE WHEN AL_LineAmount >= 0 THEN AL_LineAmount ELSE 0 END AS Haben_Fibu,
													CAST(CASE WHEN AL_LineAmount <= 0 THEN Abs(AL_LineAmount * AL_ExchangeRate) ELSE 0 END AS money) AS Soll_Position,
													CAST(CASE WHEN AL_LineAmount >= 0 THEN AL_LineAmount * AL_ExchangeRate ELSE 0 END AS money) AS Haben_Position,
													Abs(AL_GSTVAT) AS Steuer_Fibu,
													Abs(CAST(AL_GSTVAT * AL_ExchangeRate AS money)) AS Steuer_Position,
													AL_ExchangeRate AS Wechselkurs,
													CASE WHEN AH_IsCancelled = 1 THEN 'Y' ELSE 'N' END AS Storniert,
													AL_TaxDate AS Leistungsdatum,
													--
													ChargeCode.AC_Code AS Zuschlag,
													ChargeCode.AC_Desc AS Zuschlagsbeschreibung,
													--
													GLD.AG_AccountNum AS Kontonr,
													--
													Tax.AT_Code AS Steuerschluessel,
													AL_TaxRateNumerator / AL_TaxRateDenominator AS Steuersatz,
													CASE WHEN AH_Ledger = 'GL' THEN ISNULL(AH_TransactionCategory, '') ELSE '' END AS Praesentationsjournal,
													--
													AM_Period % 100 AS Periode_Monat,
													AM_Period / 100 AS Periode_Jahr,
													ACQ_ReportSubCode

												FROM dbo.GetComplianceReportTransactionLinesData(@Company, @StartDate, @EndDate, @ReportType, @SubCodeToGLAccountMapping) AS GLD
												LEFT OUTER JOIN dbo.AccTransactionHeader Header ON AH_PK = AL_AH

												LEFT OUTER JOIN dbo.JobHeader JH ON JH_PK = AL_JH
												LEFT OUTER JOIN dbo.OrgHeader ON IsNull(AH_OH, AL_OH) = OH_PK
												LEFT OUTER JOIN dbo.AccTaxRate Tax ON AL_AT = AT_PK
												LEFT OUTER JOIN dbo.AccChargeCode ChargeCode ON AL_AC = AC_PK
												LEFT OUTER JOIN dbo.AccPeriodManagement ON AM_GC_Company = AL_GC AND ACQ_Date >= AM_StartDate AND ACQ_Date <= AM_EndDate
												--
												WHERE 
												IsNull(AC_ChargeType, '') <> 'CMT'

												UNION ALL

												SELECT CASE WHEN AH_Ledger = 'AP' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN AH_ConsolidatedInvoiceRef
															ELSE IsNull(AH_TransactionNum, JH_JobNum)
													   END AS Belegnummer,
													IsNull(AL_Sequence, 1) AS Pos,
													CASE WHEN AH_Ledger = 'AP' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN CONCAT(AH_Ledger, '{SequenceGroupPlaceholder}', AH_ConsolidatedInvoiceRef)
														 WHEN AH_Ledger = 'AR' AND AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN CONCAT(AH_Ledger, '{SequenceGroupPlaceholder}', AH_TransactionNum)
														 WHEN AH_TransactionType = 'CTR' THEN CONCAT(AH_TransactionType, AH_TransactionNum)
														 ELSE CONCAT(AH_Ledger, AH_TransactionType, AH_TransactionNum)
													END AS ErweiterteBelegnummer,
													JH_JobNum AS JobNummer,
													AH_Desc AS Buchungstext,
													AH_Ledger AS Rechnungstyp,
													AH_TransactionType AS Transaktionsart,
													ACQ_Date AS Belegdatum,
													CASE WHEN AH_Ledger = 'GL' THEN NULL ELSE AH_InvoiceDate END AS Rechnungsdatum,
													CASE WHEN AH_Ledger = 'GL' THEN NULL ELSE AH_DueDate END AS Faelligkeit,
													AH_SystemCreateTimeUtc AS ErfassungsDatumZeit,
													AH_RX_NKTransactionCurrency AS Belegwaehrung,
													OH_Code AS RechnungsStellerEmpfaenger,
													--
													IsNull(AL_Desc, AH_Desc) AS Positionsbeschreibung,
													IsNull(AL_RX_NKTransactionCurrency, AH_RX_NKTransactionCurrency) AS Positionswaehrung,
													CASE WHEN IsNull(AL_LineAmount, AH_InvoiceAmount) <= 0 THEN Abs(IsNull(AL_LineAmount, AH_InvoiceAmount)) ELSE 0 END AS Soll_Fibu,
													CASE WHEN IsNull(AL_LineAmount, AH_InvoiceAmount) >= 0 THEN IsNull(AL_LineAmount, AH_InvoiceAmount) ELSE 0 END AS Haben_Fibu,
													CAST(CASE WHEN IsNull(AL_LineAmount, AH_InvoiceAmount) <= 0 THEN Abs(IsNull(AL_LineAmount, AH_InvoiceAmount) * IsNull(AL_ExchangeRate, AH_ExchangeRate)) ELSE 0 END AS money) AS Soll_Position,
													CAST(CASE WHEN IsNull(AL_LineAmount, AH_InvoiceAmount) >= 0 THEN IsNull(AL_LineAmount, AH_InvoiceAmount) * IsNull(AL_ExchangeRate, AH_ExchangeRate) ELSE 0 END AS money) AS Haben_Position,
													Abs(IsNull(AL_GSTVAT, AH_GSTAmount)) AS Steuer_Fibu,
													Abs(CAST(IsNull(AL_GSTVAT, AH_GSTAmount) * IsNull(AL_ExchangeRate, AH_ExchangeRate) AS money)) AS Steuer_Position,
													IsNull(AL_ExchangeRate, AH_ExchangeRate) AS Wechselkurs,
													CASE WHEN AH_IsCancelled = 1 THEN 'Y' ELSE 'N' END AS Storniert,
													AL_TaxDate AS Leistungsdatum,
													--
													ChargeCode.AC_Code AS Zuschlag,
													ChargeCode.AC_Desc AS Zuschlagsbeschreibung,
													--
													GLD.AG_AccountNum AS Kontonr,
													--
													Tax.AT_Code AS Steuerschluessel,
													AL_TaxRateNumerator / AL_TaxRateDenominator AS Steuersatz,
													CASE WHEN AH_Ledger = 'GL' THEN ISNULL(AH_TransactionCategory, '') ELSE '' END AS Praesentationsjournal,
													--
													AM_Period % 100 AS Periode_Monat,
													AM_Period / 100 AS Periode_Jahr,
													ACQ_ReportSubCode
													
												FROM dbo.GetComplianceReportTransactionHeaderData(@Company, @StartDate, @EndDate, @ReportType, @SubCodeToGLAccountMapping) AS GLD
												LEFT OUTER JOIN dbo.AccTransactionLines Lines ON AH_PK = AL_AH 
												LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
												LEFT OUTER JOIN dbo.AccTaxRate Tax ON AL_AT = AT_PK
												LEFT OUTER JOIN dbo.AccChargeCode ChargeCode ON AL_AC = AC_PK
												LEFT OUTER JOIN JobHeader JH ON JH_PK = AL_JH
												LEFT OUTER JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC AND ACQ_Date >= AM_StartDate AND ACQ_Date <= AM_EndDate
												WHERE 
												IsNull(AC_ChargeType, '') <> 'CMT'";

		internal const string AccGeneralLedgerSQL = @"SELECT DISTINCT AG_AccountNum AS Kontonr,
															AG_Description AS Kontoname,
															AG_PK AS PKUsedForGermanAccountMapping,
															AG_AccountType AS Kontenhaupttyp,
															AG_DebitCredit AS SollHabenKZ,
															IsNull((SELECT AG_AccountNum FROM dbo.AccGLHeader p WHERE p.AG_PK = Account.AG_AG_PercentNum), '') AS ProzentKontonr,
															IsNull((SELECT AG_AccountNum FROM dbo.AccGLHeader c WHERE c.AG_PK = Account.AG_AG_ConsolidationNum), '') AS KonsolidierungsKontonr,
															CASE WHEN AG_ControlAccount = 0 THEN 'N' ELSE 'Y' END AS AbstimmKZ,
															AG_Column AS Kontentyp
														FROM dbo.AccGLHeader Account
														WHERE AG_AccountNum IN (@Accounts)";

		internal const string AccCompanySQL = @$"WITH pivotTable AS (SELECT OK_OH, OK_RN_NKCodeCountry, STE, UST FROM
														(SELECT OK_OH, OK_RN_NKCodeCountry, OK_CodeType, OK_CustomsRegNo FROM dbo.OrgCusCode) up
													   PIVOT(MAX(OK_CustomsRegNo)  FOR OK_CodeType IN (STE, UST)) AS pvt )
												SELECT TOP 1
														OH_FullName AS Unternehmensname,
														CASE WHEN UST IS NULL THEN
															CASE WHEN GC_BusinessRegNo<> '' THEN GC_BusinessRegNo ELSE GC_BusinessRegNo2 END
														ELSE
															UST END AS UStID,
														STE AS SteuerlicheIdentifikationsnummer,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', LTRIM(RTRIM(OA_Address1 + ' ' + OA_Address2)), NULL) AS Anschrift,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', OA_PostCode, NULL) AS Postleitzahl,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', OA_City, NULL) AS Ort,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', OA_State, NULL) AS Bundesland,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', OA_RN_NKCountryCode, NULL) AS Land,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', ISNULL(NULLIF(OA_Phone, ''), GC_Phone), GC_Phone) AS Telefon,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', ISNULL(NULLIF(OA_Fax, ''), GC_Fax), GC_Fax) AS Fax,
														IIF(ISNULL(PZ_AddressType, '') = 'OFC', ISNULL(NULLIF(OA_Email, ''), GC_Email), GC_Email) AS Email,
														ISNULL(PU_URL, GC_WebAddress)  AS Internet,
														GC_RX_NKLocalCurrency AS Fibuwaehrung
													FROM dbo.OrgHeader
														JOIN dbo.GlbCompany ON GC_OH_OrgProxy = OH_PK
														LEFT OUTER JOIN dbo.OrgAddress ON OrgAddress.OA_OH = OH_PK AND OA_IsActive = 1
														LEFT OUTER JOIN dbo.OrgAddressCapability ON OA_PK = PZ_OA
														LEFT OUTER JOIN pivotTable ON OK_OH = OH_PK AND OK_RN_NKCodeCountry = '{Core.Constants.CountryCodes.Germany}'
														LEFT OUTER JOIN dbo.OrgWebURL ON PU_OH = OH_PK
													WHERE GC_PK = @Company
													ORDER BY
														CASE WHEN PZ_AddressType = 'OFC' THEN 1 ELSE 0 END DESC,
														PZ_IsMainAddress DESC";

		internal const string OrganizationSQL = @$";WITH AllOrgsAddresses AS
												(SELECT OH_Code,
														OH_FullName,
														PZ_IsMainAddress,
														OA_PK,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_Address1 ELSE NULL END) OA_Address1,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_Address2 ELSE NULL END) OA_Address2,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_PostCode ELSE NULL END) OA_PostCode,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_City ELSE NULL END) OA_City,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_State ELSE NULL END) OA_State,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_RN_NKCountryCode ELSE NULL END) OA_RN_NKCountryCode,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_Phone ELSE NULL END) OA_Phone,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN OA_Fax ELSE NULL END) OA_Fax,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN VatRegistration.OK_CustomsRegNo ELSE NULL END) OK_CustomsRegNo,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN SteRegistration.OK_CustomsRegNo ELSE NULL END) OK_SteRegNo,
														(CASE WHEN IsNull(PZ_AddressType, '') = 'OFC' THEN 'OFC' ELSE '' END) AddressType
													FROM dbo.OrgHeader
														JOIN @TVP_Organizations on [Value] = OH_Code
														LEFT JOIN dbo.OrgAddress ON OA_OH = OH_PK
														LEFT JOIN dbo.OrgAddressCapability ON OA_PK = PZ_OA
														LEFT JOIN @TVP_CountryVatType ON Country = OA_RN_NKCountryCode 
														LEFT JOIN dbo.OrgCusCode VatRegistration ON VatRegistration.OK_OH = OH_PK AND VatRegistration.OK_RN_NKCodeCountry = OA_RN_NKCountryCode AND BusinessRegType = VatRegistration.OK_CodeType
														LEFT JOIN dbo.OrgCusCode SteRegistration ON SteRegistration.OK_OH = OH_PK AND SteRegistration.OK_RN_NKCodeCountry = '{Core.Constants.CountryCodes.Germany}' AND SteRegistration.OK_CodeType = @SteCodeType
												),
												AllOrgsAddressesOrdered AS
												(
													SELECT *,
														ROW_NUMBER() OVER (PARTITION BY OH_Code ORDER BY AddressType DESC, PZ_IsMainAddress DESC, OA_PK) rowNumber
													FROM AllOrgsAddresses
												)

												SELECT 
													OH_Code AS PKKtonr,
													OH_FullName AS Unternehmensname,
													LTRIM(RTRIM(OA_Address1 + ' ' + OA_Address2)) AS Anschrift,
													OA_PostCode AS Postleitzahl,
													OA_City AS Ort,
													OA_State AS Bundesland,
													OA_RN_NKCountryCode AS Land,
													OA_Phone AS Telefon,
													OA_Fax AS Fax,
													OK_CustomsRegNo AS UmsatzsteuerID,
													OK_SteRegNo AS SteuerlicheIdentifikationsnummer
												FROM AllOrgsAddressesOrdered
												WHERE rowNumber = 1";

		internal const string OpeningBalanceSQL = @"SELECT AG_AccountNum,
														Sum(AA_Amount) AS OpeningBalance
													FROM dbo.AccGLHeader Account INNER JOIN dbo.AccGLAggregate Aggr ON AG_PK = AA_AG
													WHERE AA_GC = @Company
													AND AA_Period < @StartPeriod
													AND AG_AccountNum IN (@Accounts)
													AND AG_AccountType <> 'P&L'
													GROUP BY AG_AccountNum";

		internal const string RetainedEarningsSQL = @"SELECT SUM(AA_Amount) Total
														FROM dbo.AccGLAggregate INNER JOIN dbo.AccGLHeader ON AA_AG = AG_PK
														WHERE AG_AccountType = 'P&L'
														AND AA_GC = @Company
														AND AA_Period < @StartPeriod
														AND AA_TransactionCategory = ''";

		readonly BusinessObjectFactory Factory;
		readonly AccComplianceReport ComplianceReport;
		readonly ILogger ServiceLogger;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetLoadTransactionsCommand(ZGuid company, ZDate startDate, ZDate endDate, DbConnection connection)
		{
			var result = connection.Command(AccMovementsSQL, CommandTimeout);
			result.AddParameterBasedOnDbColumn("@Company", company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			result.AddParameterBasedOnDbColumn("@StartDate", startDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);
			result.AddParameterBasedOnDbColumn("@EndDate", endDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);

			var controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();
			var mappingTable = controlAccountAndReportSubCodeMapping.AccountPkToSubCodesTable;

			result.AddTableValuedParameter("@SubCodeToGLAccountMapping", "dbo.TVP_CodeToGuidMapping", mappingTable);

			return result;
		}

		public string RetrieveTransactionDataAsCSV(IDEATaxAuditExport exporter, ZDate startDate, ZDate endDate, string filename, IDEATempFiles tempFiles, IDEAZipFile zipCreator, IDEAeDocs eDocs, int maxChunkSize)
		{
			var stringBuilder = new StringBuilder(maxChunkSize);
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var command = GetLoadTransactionsCommand(ComplianceReport.ACR_GC_Company.ToGuid(), startDate, endDate, connection))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					exporter.CreateAccountMovementsAsCSV(reader, stringBuilder, SequenceGroupPlaceholder);
					if (stringBuilder.Length >= maxChunkSize)
					{
						tempFiles.AddFile(filename, stringBuilder.ToString(), IDEAeDocs.MaxEdocSize, false, zipCreator, eDocs);
						stringBuilder.Clear();
					}
				}
			}
			return stringBuilder.ToString();
		}

		public DynamicBusinessObjectCollection RetrieveAccounts(HashSet<string> accountNumbers)
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			var accounts = "'" + string.Join("','", accountNumbers) + "'";
			var retrieveStatement = AccGeneralLedgerSQL.Replace("@Accounts", accounts);
			collection.Load(retrieveStatement, parameters);
			ServiceLogger.Log(LogType.Debug, Invariant($"Retrieved {collection.Count} records of type Accounts"));
			return collection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public DynamicBusinessObject[] RetrieveOrganizations(HashSet<string> organizationCodes)
		{
			using (var command = Db.Connection.Command(OrganizationSQL))
			{
				var organizations = GetDataTableForOrganizations(organizationCodes);
				var orgCusCodeHelper = new OrgCusCodeHelper();
				var countryVatType = orgCusCodeHelper.ConsumptionTaxRegistrationCodeForAllCountriesDataTable;
				command.AddTableValuedParameter("@TVP_Organizations", "dbo.TVP_nvarchar_12", organizations);
				command.AddTableValuedParameter("@TVP_CountryVatType", "dbo.TVP_CountryAndBusinessRegType", countryVatType);
				command.AddParameter("@SteCodeType", SqlDbType.Char, 3, TaxRegistrationCode);

				var dataTable =  DataUtils.GetDataTableFromCommand(command);
				ServiceLogger.Log(LogType.Debug, Invariant($"Retrieved {dataTable.Rows.Count} records of type Organizations"));

				DataRow[] rows = dataTable.Select("");

				DynamicBusinessObject[] bOs = new DynamicBusinessObject[rows.Length];
				for (int i = 0; i < rows.Length; i++)
				{
					bOs[i] = new DynamicBusinessObject(new BusinessObjectFactory(), rows[i]);
				}

				return bOs;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column name")]
		DataTable GetDataTableForOrganizations(HashSet<string> organizationCodes)
		{
			var result = new DataTable();
			result.Locale = CultureInfo.InvariantCulture;
			result.Columns.Add("Value", typeof(string));

			foreach (var entry in organizationCodes)
			{
				var row = result.NewRow();
				row["Value"] = entry;
				result.Rows.Add(row);
			}

			return result;
		}

		public DynamicBusinessObjectCollection RetrieveCompany()
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", ComplianceReport.ACR_GC_Company.ToGuid(), AccTransactionHeaderSchema.AH_GC);
			collection.Load(AccCompanySQL, parameters);
			ServiceLogger.Log(LogType.Debug, Invariant($"Retrieved {collection.Count} records of type Company"));
			return collection;
		}

		public Dictionary<ZString, ZDecimal> RetrieveOpeningBalanceForAccounts(IEnumerable<string> accountNumbers, string retainedEarningsAccount)
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var reportStartPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime(), ComplianceReport.ACR_GC_Company);
			var collection = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", ComplianceReport.ACR_GC_Company.ToGuid(), AccGLAggregateSchema.AA_GC);
			parameters.Add("@StartPeriod", reportStartPeriod, AccGLAggregateSchema.AA_Period);
			var accounts = "'" + string.Join("','", accountNumbers) + "'";
			var sql = OpeningBalanceSQL.Replace("@Accounts", accounts);
			collection.Load(sql, parameters);
			var openingBalance = collection.ToDictionary(k => (ZString)k["AG_AccountNum"], v => (ZDecimal)(-1 * (ZDecimal)v["OpeningBalance"]));

			var retainedEarnings = new DynamicBusinessObjectCollection(Factory);
			retainedEarnings.Load(RetainedEarningsSQL, parameters);
			if (retainedEarnings.Count == 1)
			{
				var amount = (ZDecimal)retainedEarnings[0]["Total"];
				openingBalance[retainedEarningsAccount] = -1 * amount;
			}

			return openingBalance;
		}

		public HashSet<string> GetBalanceAccountsWithNonZeroBalance()
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var reportStartPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime(), ComplianceReport.ACR_GC_Company);
			var balanceAccounts = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", ComplianceReport.ACR_GC_Company.ToGuid(), AccGLAggregateSchema.AA_GC);
			parameters.Add("@StartPeriod", reportStartPeriod, AccGLAggregateSchema.AA_Period);
			balanceAccounts.Load(BalanceAccountsSQL, parameters);
			return balanceAccounts.Select(v => (string)(ZString)v["AG_AccountNum"]).ToHashSet();
		}
	}
}
