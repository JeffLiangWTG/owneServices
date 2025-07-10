using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class GLAccountToLocalAccountMapping
	{
		public GLAccountToLocalAccountMapping(AccComplianceReport complianceReport, ZString language)
			: this(complianceReport, language, new HashSet<ZGuid>())
		{
		}

		public GLAccountToLocalAccountMapping(AccComplianceReport complianceReport, ZString language, HashSet<ZGuid> unmappedLocalAccounts)
		{
			ComplianceReport = Argument.NotNull(complianceReport, nameof(ComplianceReport));
			if (complianceReport.ReportBaseTablePrefix != ReportBaseTablePrefixListCodes.AllTransactions)
			{
				throw new ArgumentException("Only DayBook reports are supported.");
			}
			Language = language;
			UnmappedLocalAccounts = unmappedLocalAccounts ?? new HashSet<ZGuid>();
			ignoreLocalGlMapping = (bool)AccountingMasterFilesRegistry.Instance.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.GetValueWithFallbackDefault(complianceReport.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		internal readonly HashSet<ZGuid> UnmappedLocalAccounts;

		Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)> LocalAccountMapping => localAccountMapping ??= RetrieveAccountMapping(ignoreLocalGlMapping);
		Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)> localAccountMapping;

		HashSet<(ZString accountNumber, ZString accountDescription)> AccountsWithMissingMapping => accountsWithMissingMapping ?? (accountsWithMissingMapping = RetrieveGLAccountsWithMissingMapping());
		HashSet<(ZString accountNumber, ZString accountDescription)> accountsWithMissingMapping;

		readonly ControlAccountAndReportSubCodeMapping controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();
		readonly AccComplianceReport ComplianceReport;
		readonly ZString Language;
		readonly bool ignoreLocalGlMapping;

		// FEC: This method is called twice.
		// 1. run: UnmappedLocalAccounts is empty, AccountsWithMissingMapping is initialised based on AccGLAggregate
		// 2. run: UnmappedLocalAccounts may have been found by GetLocalAccount(...), calling RetrieveAccountsWithMissingMapping() adds them to AccountsWithMissingMapping
		// IDEA: This method is called only once.
		// 1. run: UnmappedLocalAccounts may have been found by GetLocalAccount(...), calling RetrieveAccountsWithMissingMapping() adds them to AccountsWithMissingMapping,
		//		   AccountsWithMissingMapping is initialised based on AccGLAggregate
		// Using HashSets avoids having duplicates in the collection.
		public bool ValidateMappingAndUpdateNotesAndStatus()
		{
			return ValidateMappingAndUpdateNotesAndStatus(serviceLogger: null, notesText: null, setErrorStatus: true, addAccountNumbers: true);
		}

		public bool ValidateMappingAndUpdateNotesAndStatus(ILogger serviceLogger)
		{
			return ValidateMappingAndUpdateNotesAndStatus(serviceLogger, notesText: null, setErrorStatus: true, addAccountNumbers: true);
		}

		public bool ValidateMappingAndUpdateNotesAndStatus(ILogger serviceLogger, ZStringBuilder notesText, bool setErrorStatus, bool addAccountNumbers)
		{
			ComplianceReport.DeleteNotes(DefaultDescription);

			if (UnmappedLocalAccounts.Count > 0)
			{
				RetrieveAccountsWithMissingMapping();
			}

			if (ignoreLocalGlMapping || AccountsWithMissingMapping.Count == 0)
			{
				return true;
			}

			AddNoteWithMappingList(notesText, addAccountNumbers);
			SetComplianceReportStatusMessage(serviceLogger, setErrorStatus);
			ComplianceReport.Factory.Save();

			return false;
		}
		public (string localAccountNo, string localAccountDescription) GetLocalAccount(ZGuid glAccountPK, ZString reportSubCode)
		{
			var controlAccountPK = controlAccountAndReportSubCodeMapping.AccountPkToSubCodesMapping.FirstOrDefault(x => x.Value.Contains(reportSubCode)).Key;

			var accountPK = (controlAccountPK == Guid.Empty) ? glAccountPK : new ZGuid(controlAccountPK);
			if (LocalAccountMapping.TryGetValue(accountPK, out var localAccount))
			{
				return localAccount;
			}

			UnmappedLocalAccounts.Add(accountPK);

			return ("", "");
		}

		static ZString ErrorMessage => Res.GetString("f8d7f94f-1a1e-4aa0-95a2-2881de6e6510", "Account mapping is incomplete. Please check the Notes tab for details.");
		static ZString DefaultNotesMessage => $"{Res.GetString("83E1A242-5C59-4CAE-8057-87AD0A216001", "This report requires a complete mapping of GL Accounts to local accounts but some mappings are not defined in MAINTAIN > ACCOUNT > GL MULTI-LANGUAGE MAPPING.")}{System.Environment.NewLine}";
		static ZString DefaultDescription => Res.GetString("49FFE109-AEDB-40B0-8E74-44F48FA67024", "Missing Local Account Mapping");

		void RetrieveAccountsWithMissingMapping()
		{
			var headersWithoutMapping = ComplianceReport.Factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, UnmappedLocalAccounts));
			foreach (var accGlHeader in headersWithoutMapping)
			{
				AccountsWithMissingMapping.Add((accGlHeader.AG_AccountNum, accGlHeader.AG_DescriptionMultilingual));
			}
		}

		Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)> RetrieveAccountMapping(bool ignoreLocalMapping) => ignoreLocalMapping ? RetrieveAllAccountMapping() : RetrieveLocalAccountMapping();

		Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)>  RetrieveAllAccountMapping()
		{
			var accountHeaders = new DynamicBusinessObjectCollection(ComplianceReport.Factory);
			var result = new Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)>();
			const string SQL = @"SELECT AG_AccountNum, AG_Description, AG_PK FROM AccGLHeader";
			accountHeaders.Load(SQL);

			foreach (var account in accountHeaders)
			{
				var accountPK = new ZGuid(account[AccGLHeaderSchema.PK]);
				result.Add(accountPK, (account[AccGLHeaderSchema.AG_AccountNum].ToString(), (NoResString)account[AccGLHeaderSchema.AG_Description].ToString()));
			}

			return result;
		}

		Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)> RetrieveLocalAccountMapping()
		{
			var accountHeaders = new DynamicBusinessObjectCollection(ComplianceReport.Factory);
			var result = new Dictionary<ZGuid, (string localAccountNo, string localAccountDescription)>();
			var parameters = new List<ZSqlParameter>();

			const string SQL = @"SELECT AG_PK, AJ_LocalAccountNumber, AJ_AccountDescription
									FROM  dbo.AccGLHeader
										INNER JOIN dbo.AccGLDescriptorPivot ON AccGLHeader.AG_PK = YJ_AG
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
									WHERE AJ_RN_NKCountryOfCompliance = @CountryCode
										AND AJ_Language = @LanguageCode";

			parameters.Add(ZSqlParameter.New("@CountryCode", ComplianceReport.ReportCountryCode, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance));
			parameters.Add(ZSqlParameter.New("@LanguageCode", Language, AccGLAccountDescriptorSchema.AJ_Language));
			accountHeaders.Load(SQL, parameters.ToArray());

			foreach (var account in accountHeaders)
			{
				var accountPK = new ZGuid(account[AccGLHeaderSchema.PK]);
				result.Add(accountPK, (account[AccGLAccountDescriptorSchema.AJ_LocalAccountNumber].ToString(), (NoResString)account[AccGLAccountDescriptorSchema.AJ_AccountDescription].ToString()));
			}

			return result;
		}

		HashSet<(ZString, ZString)> RetrieveGLAccountsWithMissingMapping()
		{
			// retrieve account numbers and descriptions of all accounts relevant for this compliance report that do not have a local account mapping
			const string SQL = @"WITH usedAccounts
									AS (
									SELECT DISTINCT AG_PK, AG_AccountNum, AG_Description
										FROM dbo.AccGLHeader
										JOIN dbo.AccGLAggregate ON AA_AG = AG_PK
										WHERE AA_GC = @companyPK
										AND AA_Period <= @lastRelevantPeriod
										AND AG_AccountType = 'BSH'

									UNION

									SELECT DISTINCT AG_PK, AG_AccountNum, AG_Description
										FROM dbo.AccGLHeader
										JOIN dbo.AccGLAggregate ON AA_AG = AG_PK
										WHERE AA_GC = @companyPK
										AND AA_Period >= @firstRelevantPeriod
										AND AA_Period <= @lastRelevantPeriod
										AND AG_AccountType = 'P&L'

									UNION

									SELECT AG_PK, AG_AccountNum, AG_Description
										FROM dbo.AccGLHeader
										WHERE AG_PK = @retainedEarningsPK
									)

									SELECT AG_AccountNum, AG_Description
									FROM usedAccounts
									WHERE AG_PK NOT IN (
											SELECT YJ_AG
											FROM dbo.AccGLDescriptorPivot
											INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
											WHERE AJ_RN_NKCountryOfCompliance = @countryCode
											AND AJ_Language = @languageCode )
									ORDER BY AG_AccountNum ASC";

			var periodCalculator = new AccountingPeriodCalculator(ComplianceReport.Factory);
			var reportStartPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime(), ComplianceReport.ACR_GC_Company);
			var reportEndPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateTo.ToDateTime(), ComplianceReport.ACR_GC_Company);
			var retainedEarningsPK = new Guid(AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value.ToString());

			var missingAccounts = new DynamicBusinessObjectCollection(ComplianceReport.Factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@companyPK", ComplianceReport.ACR_GC_Company, AccGLAggregateSchema.AA_GC);
			parameters.Add("@lastRelevantPeriod", reportEndPeriod, AccGLAggregateSchema.AA_Period);
			parameters.Add("@countryCode", ComplianceReport.ReportCountryCode, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance);
			parameters.Add("@languageCode", Language, AccGLAccountDescriptorSchema.AJ_Language);
			parameters.Add("@firstRelevantPeriod", reportStartPeriod, AccGLAggregateSchema.AA_Period);
			parameters.Add("@retainedEarningsPK", retainedEarningsPK, AccGLHeaderSchema.PK);
			missingAccounts.Load(SQL, parameters);

			var result = new HashSet<(ZString, ZString)>();
			foreach (DynamicBusinessObject account in missingAccounts)
			{
				result.Add((account["AG_AccountNum"].ToString(), account["AG_Description"].ToString()));
			}

			return result;
		}

		void AddNoteWithMappingList(ZStringBuilder notesText = null, bool addAccountNumbers = true)
		{
			var notesBuilder = notesText ?? new ZStringBuilder().Append(DefaultNotesMessage);
			if (addAccountNumbers)
			{
				AccountsWithMissingMapping.OrderBy(x => x.accountNumber).ForEach(account => notesBuilder.Append($"{account.accountNumber} - {account.accountDescription}"));
			}
			ComplianceReport.AddNote(DefaultDescription, notesBuilder.ToStringWithNewLineBetweenAppends());
		}

		void SetComplianceReportStatusMessage(ILogger serviceLogger, bool setErrorStatus = true)
		{
			if (setErrorStatus)
			{
				ComplianceReport.ACR_Status = AccComplianceReport.Status.ReportError;
			}
			ComplianceReport.ACR_StatusMessage = ErrorMessage;
			ComplianceReport.Logs.AddNew(Events.StatusUpdated, AccComplianceReport.StatusKey + ComplianceReport.ACR_Status);

			serviceLogger?.Log(LogType.Error, (NoResString)"Missing or incomplete local account mapping. Please check the Notes tab page of the compliance report.");
		}
	}
}
