using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.CountryCompliance.Interfaces.DataObjects;
using Enterprise.Accounting.Export.Business.TaxFramework;
using Enterprise.Accounting.Integration.CalculateTaxForCharge;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using AHFSchema = Enterprise.ZArchitecture.Schema.AccTransactionHeaderAuthorisationRecordSchema.Constants;
using AIBSchema = Enterprise.ZArchitecture.Schema.AccEInvoicingBatchSchema.Constants;
using AIPSchema = Enterprise.ZArchitecture.Schema.AccEInvoicingTransactionPivotSchema.Constants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using DataTransferConstants = Enterprise.Accounting.Integration.DataTransferConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using TaxGroupCodeType = Enterprise.UniversalDataBuss.DataObjects.Universal.TaxGroupCodeType;

namespace Enterprise.Accounting.Export.Business
{
	public class BatchExportDataAccess : BaseDataAccess
	{
		readonly ITaxAmountCalculator _taxAmountCalculator = ObjectFactory.Get<ITaxAmountCalculator>();
		public BatchExportDataAccess(System.Data.Common.DbConnection connection, DbTransaction transaction)
			: base(connection, transaction)
		{
			CommandTimeout = 1800;  //half an hour
		}

		public ZDateTime[] GetPeriodEndDatesBetween(ZDateTime startDate, ZDateTime endDate, ZGuid companyPK)
		{
			List<ZDateTime> dates = new List<ZDateTime>();

			string sqlText = string.Format(@"SELECT AM_EndDate
FROM dbo.AccPeriodManagement
WHERE AM_EndDate >= @startDate
AND AM_EndDate <= @endDate
AND AM_GC_Company = @companyPK");
			var command = GetCommand(sqlText,
				(Name: "@startDate", Value: startDate.ToDateTime()),
				(Name: "@endDate",	 Value: endDate.ToDateTime()),
				(Name: "@companyPK", Value: companyPK.ToGuid()));

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ZDateTime date = (ZDateTime)(DateTime)reader["AM_EndDate"];
					dates.Add(date);
				}
			}

			return dates.ToArray();
		}

		public List<GLPeriodRow> GetPeriodsContaining(ZDateTime startDate, ZDateTime endDate, ZGuid companyPK)
		{
			var result = new List<GLPeriodRow>();

			string sqlText = @"SELECT AM_Period, AM_StartDate, AM_EndDate, AM_Period
FROM dbo.AccPeriodManagement
WHERE AM_StartDate <= @endDate
AND AM_EndDate >= @startDate
AND AM_GC_Company = @companyPK";
			var command = GetCommand(sqlText,
				(Name: "@startDate", Value: startDate.ToDateTime()),
				(Name: "@endDate",	 Value: endDate.ToDateTime()),
				(Name: "@companyPK", Value: companyPK.ToGuid()));

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var period = new GLPeriodRow();
					period.CompanyPK = companyPK;
					period.Period = (ZInt)(int)reader["AM_Period"];
					period.StartDate = (ZDateTime)(DateTime)reader["AM_StartDate"];
					period.EndDate = (ZDateTime)(DateTime)reader["AM_EndDate"];
					result.Add(period);
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZString, Tuple<ZString, ZString>> GetLocalGLAccountsMapping(List<ZString> glAccounts, ZGuid companyPK)
		{
			var result = new Dictionary<ZString, Tuple<ZString, ZString>>();

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT AG_AccountNum, AJ_LocalAccountNumber, AJ_AccountDescription
FROM dbo.AccGLAccountDescriptor
JOIN dbo.AccGLDescriptorPivot ON YJ_AJ = AJ_PK
JOIN dbo.AccGLHeader ON YJ_AG = AG_PK
JOIN dbo.GlbCompany ON GC_PK = @companyPK
JOIN dbo.OrgHeader ON OH_PK = GC_OH_OrgProxy
WHERE AG_AccountNum IN ('{0}')
	AND (AJ_ReportType = 'COA')
	AND (AJ_RN_NKCountryOfCompliance = GC_RN_NKCountryCode OR AJ_Language = OH_Language)
ORDER BY AG_AccountNum,
	CASE 
		WHEN AJ_RN_NKCountryOfCompliance = GC_RN_NKCountryCode AND AJ_Language = OH_Language THEN 0 
		WHEN AJ_RN_NKCountryOfCompliance = GC_RN_NKCountryCode AND AJ_Language <> OH_Language THEN 1
		WHEN AJ_RN_NKCountryOfCompliance <> GC_RN_NKCountryCode AND AJ_Language = OH_Language THEN 2
	END", new ZStringBuilder(glAccounts).ToStringWithDelimiterBetweenAppends("','"));

			var command = GetCommand(sqlText, (Name: "@companyPK", Value: companyPK.ToGuid()));
			using (var reader = command.ExecuteReader())
			{
				string previousAccount = null;
				while (reader.Read())
				{
					var currentAccount = reader["AG_AccountNum"].ToString();
					if (currentAccount != previousAccount)
					{
						var localAccount = new Tuple<ZString, ZString>(reader["AJ_LocalAccountNumber"].ToString(), reader["AJ_AccountDescription"].ToString());
						result.Add(currentAccount, localAccount);
						previousAccount = currentAccount;
					}
				}
			}

			return result;
		}

		[SuppressMessage("Reference", "CW1060:Do not use System.DateTime.Now Rule", Justification = "local date time only for caching")]
		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "full name needed")]
		public HashSet<BatchRow> GetPotentialBatch(string companyCode, System.Data.Common.DbTransaction transaction)
		{
			var companyParameterName = "@CompanyCode_" + ParameterSuffixer.Instance.GetParameterSuffix(DateTime.Now, GlbCompanySchema.GC_Code, companyCode);

			var sqlTextBuilder = new ZStringBuilder((NoResString)"EXEC AccountingTransactionExportCreateBatch ");
			sqlTextBuilder.Append(companyParameterName);

			var parameters = new List<Action<DbParameter>>();
			parameters.Add(GetCompanyCodeSqlParameter(companyParameterName, companyCode));

			var hasHighWatermark = tryToAddHighWaterMarkDateParameterFromRegistryItem(AccountingTransactionExportServiceHighWaterMark); // universal export 
			if (!hasHighWatermark)
			{
				hasHighWatermark = tryToAddHighWaterMarkDateParameterFromRegistryItem(AccountingTransactionsExportHighWaterMark); // legacy export)
			}
			if (hasHighWatermark)
			{
				tryToAddStartPostDateParameterFromRegistryItem(ExportTransactionBatchStartPostDate); // apply start post date only  when we have a high watermark properly se up to cut off the old transactions
			}

			var command = GetCommandWithTransaction(sqlTextBuilder.ToString(), transaction, parameters.ToArray());
#if DEBUG
			var parametersTextBuilder = new ZStringBuilder();
			foreach(DbParameter p in command.Parameters)
			{
				if (parametersTextBuilder.Length > 0)
				{
					parametersTextBuilder.Append(" ");
				}
				parametersTextBuilder.Append(p.ParameterName + " = " + p.Value);
			}
			QueryForGetPotentialBatch = Tuple.Create(sqlTextBuilder.ToString(), parametersTextBuilder.ToStringWithDelimiterBetweenAppends(" "));
#endif
			return PopulateBatchRows(command);

			bool tryToAddHighWaterMarkDateParameterFromRegistryItem(string registryName)
			{
				DateTime registryDate;
				var result = tryToGetDateTimeParameterFromRegistryItem(registryName, out registryDate);
				if (result)
				{
					var highWaterMarkParameterName = "@HighWaterMark";
					var periodSuffix = GetPeriodIntervalIdentifier(registryDate, DateTime.Now);
					if (!string.IsNullOrEmpty(periodSuffix))
					{
						highWaterMarkParameterName += "_" + periodSuffix;
					}
					AddParameter(registryDate, highWaterMarkParameterName);
				}
				return result;
			}

			bool tryToAddStartPostDateParameterFromRegistryItem(string registryName)
			{
				var result = tryToGetDateTimeParameterFromRegistryItem(registryName, out DateTime registryDate);
				if (result)
				{
					AddParameter(registryDate, "@StartPostDate");
				}
				return result;
			}

			void AddParameter(DateTime date, string parameterName)
			{
				sqlTextBuilder.AppendFormat(", {0}", parameterName);
				parameters.Add(p => { p.ParameterName = parameterName; p.Value = date; });
			}

			bool tryToGetDateTimeParameterFromRegistryItem(string registryName, out DateTime registryDate)
			{
				registryDate = GetDateTimeFromStmData(companyCode, transaction, registryName);
				return registryDate != DateTime.MinValue;
			}
		}

#if DEBUG
		public Tuple<string, string> QueryForGetPotentialBatch { get; set; }
#endif

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Parameter strings")]
		string GetPeriodIntervalIdentifier(DateTime dateFrom, DateTime dateTo)
		{
			if (dateFrom > dateTo)
			{
				return string.Empty;
			}

			string result;
			var absDiffDays = Math.Abs((dateTo - dateFrom).TotalDays);
			if (absDiffDays < 1)
			{
				result = "Hours";
			}
			else if (absDiffDays < 7)
			{
				result = "Days";
			}
			else if (absDiffDays < 30)
			{
				result = "Weeks";
			}
			else if (absDiffDays < 90)
			{
				result = "Months";
			}
			else if (absDiffDays < 365)
			{
				result = "Quarters";
			}
			else
			{
				result = "Years";
			}

			return result;
		}

		DateTime GetDateTimeFromStmData(string companyCode, DbTransaction transaction, string name)
		{
			DateTime result = DateTime.MinValue;
			var sqlText = @"DECLARE @CompanyPK uniqueidentifier = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany WHERE GC_Code = @CompanyCode);
SELECT TOP 1 SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner = @CompanyPK;";

			using (var cmd = GetCommandWithTransaction(sqlText, transaction,
				GetSdNameSqlParameter("@SD_Name", name),
				GetCompanyCodeSqlParameter("@CompanyCode", companyCode)))
			{
				var binaryVal = cmd.ExecuteScalar() as byte[];
				if (binaryVal != null)
				{
					var valueAsString = Encoding.Unicode.GetString(binaryVal);
					if (!string.IsNullOrEmpty(valueAsString))
					{
						try
						{
							result = SqlFormatInfo.FromSqlDateTime(valueAsString);
						}
						catch (FormatException)
						{
							result = DateTime.MinValue;
						}
					}
				}
			}
			return result;
		}

		public void SetAccountingTransactionExportServiceHighWaterMark(DateTime highWaterMark, DbTransaction transaction, Guid companyPK)
		{
			var binaryValue = Encoding.Unicode.GetBytes(SqlFormatInfo.ToSqlDateTimeString(highWaterMark));

			var sqlText = @"
UPDATE dbo.StmData SET SD_BinaryValue = @SD_BinaryValue WHERE SD_Name = @SD_Name AND SD_Owner = @SD_Owner
IF (@@rowcount = 0)
BEGIN
	INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_IsLogged)
	VALUES (NEWID(), @SD_Name, @SD_Owner, @SD_Type, @SD_BinaryValue, @SD_IsLogged)
END";
			using (var cmd = GetCommandWithTransaction(sqlText, transaction, null))
			{
				cmd.AddParameters(GetSqlParameterBasedOnDbColumn("@SD_BinaryValue", binaryValue, StmDataSchema.SD_BinaryValue));
				cmd.AddParameters(GetSdNameSqlParameter("@SD_Name", AccountingTransactionExportServiceHighWaterMark));
				cmd.AddParameterWithValue("@SD_Owner", companyPK);
				cmd.AddParameters(GetSqlParameterBasedOnDbColumn("@SD_Type", "DT", StmDataSchema.SD_Type));
				cmd.AddParameterWithValue("@SD_IsLogged", true);
				cmd.ExecuteNonQuery();
			}
		}

		Action<DbParameter> GetSdNameSqlParameter(string name, string value) => GetSqlParameterBasedOnDbColumn(name, value, StmDataSchema.SD_Name);

		Action<DbParameter> GetCompanyCodeSqlParameter(string name, string value) => GetSqlParameterBasedOnDbColumn(name, value, GlbCompanySchema.GC_Code);

		Action<DbParameter> GetSqlParameterBasedOnDbColumn(string name, object value, SchemaColumn dbColumn)
		{
			return p =>
			{
				var parameter = new SqlParameterWrapper(p);
				parameter.ParameterName = name;
				parameter.Value = value ?? DBNull.Value;
				parameter.SqlDbType = dbColumn.SqlDbType;
				parameter.Size = dbColumn.MaxLength;
			};
		}

		protected const string AccountingTransactionExportServiceHighWaterMark = "AccountingTransactionExportServiceHighWaterMark";
		protected const string AccountingTransactionsExportHighWaterMark = "AccountingTransactionsExportHighWaterMark";
		protected const string ExportTransactionBatchStartPostDate = "ExportTransactionBatchStartPostDate";

		public DateTime GetUtcNow(DbTransaction transaction)
		{
			using (var cmd = GetCommandWithTransaction(utcSqlText, transaction))
			{
				return (DateTime)cmd.ExecuteScalar();
			}
		}

		const string utcSqlText = "SELECT GETUTCDATE()";

		#region Registry

		const string TaxMessageGroupsManagement = "TaxMessageGroupsManagement";
		const string EnableGovernmentChargeCode = "EnableGovernmentChargeCode";
		const string EnableSupplyTypeClassificationCodes = "EnableSupplyTypeClassificationCodes";
		const string SupplyTypeClassificationCodesList = "SupplyTypeClassificationCodesList";
		const string NoteGLAccountsStatisticalUnitsofMeasurement = "NoteGLAccountsStatisticalUnitsofMeasurement";
		const string TaxAuthorities = "TaxAuthorities";
		const string TaxSystems = "TaxSystems";
		const string IncludeRelatedJournals = "IncludeRelatedJournals";
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		bool GetEnableGovernmentChargeCode(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				EnableGovernmentChargeCode,
				(companyPK) => AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);

		bool GetEnableSupplyTypeClassificationCodes(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				EnableSupplyTypeClassificationCodes,
				(companyPK) => AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);

		CodeDescriptionBoolCollection GetSupplyTypeClassificationCodesList(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				SupplyTypeClassificationCodesList,
				(companyPK) => AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);

		CodeDescriptionBoolRelatedItemCollection GetTaxGroupCodes(string companyCode)
		{
			var companyPK = GetCompanyData(companyCode).CompanyPK;
			var registryName = TaxMessageGroupsManagement;

			return Factory.GetCachedValue(FormattableString.Invariant($"{companyCode}{registryName}"), () =>
			{
				var ret = new CodeDescriptionBoolRelatedItemCollection();
				if (ZArchitecture.Environment.Globals.IsWeb)
				{
					AccountingMasterFilesRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryName, TimeSpan.Zero);
				}

				var taxGroupCodeList = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				var returnList = taxGroupCodeList?.OfType<CodeDescriptionBoolRelatedItem>().Where(x => (x).Bool);
				ret.AddRange(returnList);

				return ret;
			});
		}

		ICodeDescriptionPairList GetNoteGLAccountsStatisticalUnitsofMeasurement(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				NoteGLAccountsStatisticalUnitsofMeasurement,
				(companyPK) => ObjectFactory.Get<IAccounting>().Registry.NoteGLAccountsStatisticalUnitsofMeasurement(companyPK)
			);
#if DEBUG
		public ICodeDescriptionPairList GetNoteGLAccountsStatisticalUnitsofMeasurement_ForTestOnly(string companyCode) => GetNoteGLAccountsStatisticalUnitsofMeasurement(companyCode);
#endif

		ICodeDescriptionPairList GetReversalReasonCodesList(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Name,
				(companyPK) => AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);
#if DEBUG
		public ICodeDescriptionPairList GetReversalReasonCodesList_ForTestOnly(string companyCode) => GetReversalReasonCodesList(companyCode);
#endif

		ICodeDescriptionPairList GetCreditNoteReasonCodesList(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Name,
				(companyPK) => AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);
#if DEBUG
		public ICodeDescriptionPairList GetCreditNoteReasonCodesList_ForTestOnly(string companyCode) => GetCreditNoteReasonCodesList(companyCode);
#endif

		ICodeDescriptionPairList GetAmendmentReasonCodesList(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Name,
				(companyPK) => AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);
#if DEBUG
		public ICodeDescriptionPairList GetAmendmentReasonCodesList_ForTestOnly(string companyCode) => GetAmendmentReasonCodesList(companyCode);
#endif

		ICodeDescriptionPairList GetTaxAuthorities(string countryCode)
			=> GetTaxFrameworkValueFromRegistryWithWebCacheInvalidation(
				countryCode,
				TaxAuthorities,
				() => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxAuthorities(countryCode)
			);

		ICodeDescriptionPairList GetTaxSystems(string countryCode)
			=> GetTaxFrameworkValueFromRegistryWithWebCacheInvalidation(
				countryCode,
				TaxSystems,
				() => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(countryCode)
			);

		bool GetIncludeRelatedJournalsInUniversalXMLTransaction()
			=> GetValueFromSystemRegistryWithWebCacheInvalidation(
				IncludeRelatedJournals,
				() => AccountingMasterFilesRegistry.Instance.IncludeRelatedJournalsInUniversalXMLTransaction.Value
			);

		#region Registry Helpers

		T GetTaxFrameworkValueFromRegistryWithWebCacheInvalidation<T>(string countryCode, string registryName, Func<T> registryGetter)
		{
			var cacheKey = registryName + "." + countryCode;
			return Factory.GetCachedValue(cacheKey, () =>
			{
				if (ZArchitecture.Environment.Globals.IsWeb)
				{
					AccountingMasterFilesRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryName, TimeSpan.Zero);
				}

				return registryGetter();
			});
		}

		T GetValueFromCompanyRegistryWithWebCacheInvalidation<T>(string companyCode, string registryName, Func<Guid, T> registryGetter)
		{
			var companyPK = GetCompanyData(companyCode).CompanyPK;
			var cacheKey = registryName + "." + companyCode;
			return Factory.GetCachedValue(cacheKey, () =>
			{
				if (ZArchitecture.Environment.Globals.IsWeb)
				{
					AccountingMasterFilesRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryName, TimeSpan.Zero);
				}

				return registryGetter(companyPK);
			});
		}

		T GetValueFromSystemRegistryWithWebCacheInvalidation<T>(string registryName, Func<T> registryGetter)
		{
			var cacheKey = registryName;
			return Factory.GetCachedValue(cacheKey, () =>
			{
				if (ZArchitecture.Environment.Globals.IsWeb)
				{
					AccountingMasterFilesRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryName, TimeSpan.Zero);
				}

				return registryGetter();
			});
		}

		#endregion

		ICodeDescriptionPairList GetEInvoicingReversalCodesList(string companyCode)
			=> GetValueFromCompanyRegistryWithWebCacheInvalidation(
				companyCode,
				AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.Name,
				(companyPK) => AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
			);

		#endregion

		public HashSet<BatchRow> GetMockBatchForTransaction(Guid transactionPK)
		{
			string sqlText = "EXEC AccountingTransactionExportCreateMockBatchForTransaction @transactionPK";
			var command = GetCommand(sqlText, ("@transactionPK", transactionPK));

			return PopulateBatchRows(command);
		}

		HashSet<BatchRow> PopulateBatchRows(System.Data.Common.DbCommand command)
		{
			var batchRows = new HashSet<BatchRow>();

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					BatchRow batchRow = new BatchRow();
					batchRow.ParentID = (Guid)reader["ParentID"];
					batchRow.ParentTableCode = (string)reader["ParentTableCode"];
					batchRow.RowType = (string)reader["Type"];
					batchRow.Sequence = Convert.ToInt32((short)reader["Sequence"]);
					object value = reader["AH_PK"];
					batchRow.AH_PK = value == DBNull.Value ? Guid.Empty : (Guid)value;
					batchRows.Add(batchRow);
				}
			}

			return batchRows;
		}

		public void SaveBatch(List<BatchRow> batchRows, long batchNumber, DbTransaction transaction)
		{
			int i = 0;
			while (i < batchRows.Count)
			{
				string sqlText = @"INSERT INTO dbo.GenExportBatchSequence (XB_PK, XB_Type, XB_BatchNumber, XB_Sequence, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
								VALUES ";

				for (int j = 0; j < 1000 && i < batchRows.Count; j++)
				{
					BatchRow batchRow = batchRows[i];
					batchRow.Sequence = i + 1;
					sqlText += string.Format(CultureInfo.InvariantCulture, "(NEWID(), '{0}', {1}, {2}, '{3}', '{4}', '{5}', '~BP')", batchRow.RowType, batchNumber, batchRow.Sequence, batchRow.ParentTableCode, batchRow.ParentID.ToString(), SqlFormatInfo.ToSqlDateTimeString(GetUtcNow(transaction)));
					if ((i != batchRows.Count - 1) && (j != 999))
					{
						sqlText += ", \r\n";
					}
					i++;
				}

				using (var command = GetCommandWithTransaction(sqlText, transaction))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		public HashSet<BatchRow> GetBatch(string companyCode, long batchNumber)
		{
			var batchRows = new HashSet<BatchRow>();
			var companyParameterName = "@CompanyCode_" + ParameterSuffixer.Instance.GetParameterSuffix(DateTime.Now, GlbCompanySchema.GC_Code, companyCode);

			var sqlText = string.Format(CultureInfo.InvariantCulture, "EXEC AccountingTransactionExportGetBatch {0}, @batchNumber", companyParameterName);
			var dbParameters = new List<Action<DbParameter>>
			{
				GetCompanyCodeSqlParameter(companyParameterName, companyCode),
				new Action<DbParameter>(p => { p.ParameterName = "@batchNumber"; p.Value = batchNumber; })
			};
			var command = GetCommand(sqlText, dbParameters.ToArray());
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var batchRow = new BatchRow();
					batchRow.RowType = (string)reader["XB_Type"];
					batchRow.Sequence = (int)reader["XB_Sequence"];
					batchRow.ParentTableCode = (string)reader["XB_ParentTableCode"];
					batchRow.ParentID = (Guid)reader["XB_ParentID"];
					batchRows.Add(batchRow);
				}
			}

			return batchRows;
		}

		#region LoadCompany

		public CompanyRow LoadCompany(string companyCode)
		{
			CompanyRow company = new CompanyRow();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			using (var reader = GetCompanyReader(companyCode))
			{
				if (reader != null)
				{
					company.PK = (Guid)reader["GC_PK"];
					company.GC_Code = companyCode;
					company.GC_Name = (string)reader["GC_Name"];
					company.GC_OH_OrgProxy = reader["GC_OH_OrgProxy"] == DBNull.Value ? Guid.Empty : (Guid)reader["GC_OH_OrgProxy"];
					company.GC_RN_NKCountryCode = (string)reader["GC_RN_NKCountryCode"];

					var country = new CountryRow();
					country.RN_Code = company.GC_RN_NKCountryCode;
					country.PK = reader["RN_PK"] == DBNull.Value ? Guid.Empty : (Guid)reader["RN_PK"];
					country.RN_Desc = reader["RN_Desc"] == DBNull.Value ? null : (string)reader["RN_Desc"];
					company.Country = country;

					var currency = new CurrencyRow();
					currency.PK = reader["RX_PK"] == DBNull.Value ? Guid.Empty : (Guid)reader["RX_PK"];
					currency.RX_Code = company.GC_RN_NKCountryCode;
					currency.RX_Desc = reader["RX_Desc"] == DBNull.Value ? null : (string)reader["RX_Desc"];
					int subUnits = reader["RX_SubUnitRatio"] == DBNull.Value ? 0 : (int)reader["RX_SubUnitRatio"];
					while (subUnits > 1)
					{
						subUnits = subUnits / 10;
						currency.Decimals++;
					}
					company.Currency = currency;

					company.LicenceKeyIdentifier = enterpriseCode + companyCode + serverCode;

					return company;
				}

				return null;
			}
		}

		#endregion

		public Dictionary<Guid, TransactionHeaderRow> GetTransactionHeaders(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber)
		{
			return GetTransactionHeadersCore(writerStrategy, companyCode, batchNumber);
		}

		public Dictionary<Guid, TransactionHeaderRow> GetByPKTransactionHeaderPopulatingTransactionInfo(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, Guid transactionHeaderPK, TransactionInfo infoToPopulate = null)
		{
			return GetTransactionHeadersCore(writerStrategy, companyCode, -1, transactionHeaderPK, infoToPopulate);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		Dictionary<Guid, TransactionHeaderRow> GetTransactionHeadersCore(IDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, Guid? transactionHeaderPK = null, TransactionInfo infoToPopulate = null)
		{
			Dictionary<Guid, TransactionHeaderRow> headers = new Dictionary<Guid, TransactionHeaderRow>();

			var sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetHeaders", companyCode, batchNumber, true, transactionHeaderPK);

			var placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(companyCode);

			var matchStatuses = GetMatchStatus(companyCode);
			var matchStatusReasons = GetMatchStatusReason(companyCode);
			var amendmentReasonCodeList = GetAmendmentReasonCodesList(companyCode);
			var reversalCodeList = GetReversalReasonCodesList(companyCode);
			var creditNoteCodeList = GetCreditNoteReasonCodesList(companyCode);
			var originalTransactionPK = (Guid?)null;
			var organizationPK = (ZGuid?)null;
			var branchPK = (ZGuid?)null;

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					TransactionHeaderRow header = null;
					if ((Guid)reader["AH_PK"] == transactionHeaderPK)
					{
						header = new TransactionHeaderRow(infoToPopulate);
					}
					else
					{
						header = new TransactionHeaderRow(new TransactionInfo(writerStrategy));
					}

					header.PK = (Guid)reader["AH_PK"];
					if (reader["OG_Code"] != DBNull.Value)
					{
						header.Info.APAccountGroup = new CodeDescriptionPair();
						header.Info.APAccountGroup.Code = (ZString?)reader["OG_Code"].ToString().Trim();
						header.Info.APAccountGroup.Description = (ZString?)reader["OG_Desc"].ToString().Trim();
					}
					if (reader["OJ_Code"] != DBNull.Value)
					{
						header.Info.ARAccountGroup = new CodeDescriptionPair();
						header.Info.ARAccountGroup.Code = (ZString?)reader["OJ_Code"].ToString().Trim();
						header.Info.ARAccountGroup.Description = (ZString?)reader["OJ_Desc"].ToString().Trim();
					}
					header.Info.BankAccount = reader["AB_Code"].ToNullableZString();
					header.Info.Branch = new Branch();
					header.Info.Branch.Code = reader["GB_Code"].ToNullableZString();
					header.Info.Branch.Name = reader["GB_BranchName"].ToNullableZString();
					header.Info.PlaceOfIssue = reader["GB_City"].ToNullableZString();
					header.Info.Category = reader["AH_TransactionCategory"].ToNullableZString();
					header.Info.CheckDrawer = reader["AH_ChequeDrawer"].ToNullableZString();
					header.Info.CheckNumberOrPaymentRef = reader["AH_ChequeOrReference"].ToNullableZString();
					header.Info.CreateTime = reader["AH_SystemCreateTimeUtc"].ToNullableZDateTime();
					var createUserCode = reader["AH_SystemCreateUser"].ToNullableZString();
					if (createUserCode.HasValue)
					{
						header.Info.CreateUser = new StaffUsingAttributes() { Code = createUserCode };
					}
					header.Info.DateClearedInCashBook = reader["AH_DateClearedInCashBook"].ToNullableZDateTime();
					header.Info.Department = new Department();
					header.Info.Department.Code = reader["GE_Code"].ToNullableZString();
					header.Info.Department.Name = reader["GE_Desc"].ToNullableZString();
					header.Info.Description = reader["AH_Desc"].ToNullableZString();
					header.Info.DrawerBank = reader["AH_DrawerBank"].ToNullableZString();
					header.Info.DrawerBranch = reader["AH_DrawerBranch"].ToNullableZString();
					header.Info.DueDate = reader["AH_DueDate"].ToNullableZDateTime();
					header.Info.FullyPaidDate = reader["AH_FullyPaidDate"].ToNullableZDateTime();
					var ah_TransactionReference = reader["AH_TransactionReference"] as string;
					header.Info.TransactionReference = ah_TransactionReference == null || string.IsNullOrWhiteSpace((ah_TransactionReference).Trim()) ? null : (ZString?)ah_TransactionReference;
					header.Info.GovernmentAllocatedID = reader["AH_GovernmentAllocatedID"].ToNullableZString();
					var ah_ComplianceSubType = reader["AH_ComplianceSubType"] as string;
					header.Info.ComplianceSubType = ah_ComplianceSubType == null || string.IsNullOrWhiteSpace((ah_ComplianceSubType).Trim()) ? null : (ZString?)ah_ComplianceSubType;
					var ah_InvoiceTerm = reader["AH_InvoiceTerm"].ToNullableZString();
					header.Info.InvoiceTerm = ah_InvoiceTerm.HasValue ? new InvoiceTermTypeConverter().ToEnumValue(ah_InvoiceTerm.Value) : null;
					header.Info.InvoiceTermDays = reader["AH_InvoiceTermDays"].ToNullableZInt();
					header.Info.IsCancelled = reader["AH_IsCancelled"].ToNullableZBool();
					header.Info.IsCreatedByMatchingProcess = reader["AH_TransactionCreatedByMatching"].ToNullableZBool();
					header.Info.IsPrinted = reader["AH_InvoicePrinted"].ToNullableZBool();
					header.Info.Job = new EntityReference();
					header.Info.Job.Key = reader["JH_JobNum"].ToNullableZString();
					header.Info.Job.Type = AccountingDataTransferConstants.DataContextTypeString.Job;
					header.Info.JobInvoiceNumber = reader["AH_ConsolidatedInvoiceRef"].ToNullableZString();
					header.Info.Ledger = reader["AH_Ledger"].ToNullableZString();
					header.Info.LocalCurrency = new Currency();
					header.Info.LocalCurrency.Code = reader["GC_RX_NKLocalCurrency"].ToNullableZString();
					header.Info.LocalCurrency.Description = reader["Local_RX_Desc"].ToNullableZString();
					header.Info.LocalExVATAmount = reader["AH_InvoiceAmount"].ToNullableZDecimal();
					header.Info.LocalVATAmount = reader["AH_GSTAmount"].ToNullableZDecimal();
					header.Info.LocalTotal = reader["AH_LocalTotal"].ToNullableZDecimal();
					header.Info.Number = reader["AH_TransactionNum"].ToNullableZString();
					header.Info.NumberOfSupportingDocuments = reader["AH_NumberOfSupportingDocuments"].ToNullableZInt();
					header.Info.OSCurrency = new Currency();
					header.Info.OSCurrency.Code = reader["AH_RX_NKTransactionCurrency"].ToNullableZString();
					header.Info.OSCurrency.Description = reader["RX_Desc"].ToNullableZString();

					decimal aH_ExchangeRate = (decimal)reader["AH_ExchangeRate"];
					int subUnitRatio = reader["RX_SubUnitRatio"] == DBNull.Value ? 2 : (int)reader["RX_SubUnitRatio"];
					var aH_OSOtherTax = reader["AH_OSTaxAmountOtherTaxes"].ToNullableZDecimal();
					var aH_OSTaxAmountRounded = Utilities.Round(reader["AH_OSTaxAmount"].ToNullableZDecimal() ?? new ZDecimal(0.0000m), ExchangeRate.Decimals(subUnitRatio));
					var aH_OSTotalAmount = reader["AH_OSTotal"].ToNullableZDecimal();
					decimal aH_OSExTaxAmount = ((aH_OSTotalAmount ?? new ZDecimal(0.0000m)) - (aH_OSOtherTax ?? new ZDecimal(0.0000m)) - aH_OSTaxAmountRounded);

					header.Info.OSExGSTVATAmount = aH_OSExTaxAmount;
					header.Info.OSTotal = aH_OSTotalAmount;
					header.Info.OSGSTVATAmount = aH_OSTaxAmountRounded;
					var localWHTAmount = reader["AH_WithholdingTax"].ToNullableZDecimal();
					if (localWHTAmount.HasValue)
					{
						header.Info.LocalWHTAmount = localWHTAmount;
						bool isReciprocal = (bool)reader["GC_IsReciprocal"];
						header.Info.OSWHTAmount = ExchangeRate.LocalToForeign(localWHTAmount.Value, aH_ExchangeRate, subUnitRatio, isReciprocal);
					}
					header.Info.OutstandingAmount = reader["AH_OutstandingAmount"].ToNullableZDecimal();
					header.Info.OSTaxTransactionsAmount = aH_OSOtherTax;
					header.Info.LocalTaxTransactionsAmount = reader["AH_LocalTaxAmountOtherTaxes"].ToNullableZDecimal();
					header.Info.ExchangeRate = Utilities.Round(aH_ExchangeRate, 6);

					header.Info.PaymentOrReceiptType = null;
					var ah_ReceiptType = reader["AH_ReceiptType"].ToNullableZString();
					header.Info.PaymentOrReceiptType = ah_ReceiptType.HasValue ? new PaymentOrReceiptTypeConverter().ToEnumValue(ah_ReceiptType.Value) : null;

					header.Info.PostDate = reader["AH_PostDate"].ToNullableZDateTime();
					header.Info.ReceiptOrDirectDebitNumber = reader["AH_ReceiptBatchNo"].ToNullableZString();
					header.Info.RequisitionDate = reader["AH_RequisitionDate"].ToNullableZDateTime();
					header.Info.RequisitionStatus = reader["AH_RequisitionStatus"].ToNullableZString();
					header.Info.TransactionDate = reader["AH_InvoiceDate"].ToNullableZDateTime();

					var ah_TransactionType = reader["AH_TransactionType"].ToNullableZString();
					header.Info.TransactionType = ah_TransactionType.HasValue ? new TransactionTypeConverter().ToEnumValue(ah_TransactionType.Value) : null;
					header.Info.DocumentReceivedDate = reader["AH_DocumentReceivedDate"].ToNullableZDateTime();

					header.Info.ExternalCreditorCode = reader["ExternalCreditorCode"].ToNullableZString();
					header.Info.ExternalDebtorCode = reader["ExternalDebtorCode"].ToNullableZString();

					header.Info.AgreedPaymentMethod = reader["AH_AgreedPaymentMethodOverride"].ToNullableZString();

					var matchStatus = reader["AH_MatchStatus"].ToNullableZString();
					if (!string.IsNullOrEmpty(matchStatus))
					{
						header.Info.MatchStatus = new CodeDescriptionPair();
						header.Info.MatchStatus.Code = matchStatus;
						if (matchStatuses.TryGetValue(matchStatus, out string description))
						{
							header.Info.MatchStatus.Description = description;
						}
					}

					var matchStatusReason = reader["AH_MatchStatusReasonCode"].ToNullableZString();
					if (!string.IsNullOrEmpty(matchStatusReason))
					{
						header.Info.MatchStatusReason = new CodeDescriptionPair();
						header.Info.MatchStatusReason.Code = matchStatusReason;
						if (matchStatusReasons.TryGetValue(matchStatusReason, out string description))
						{
							header.Info.MatchStatusReason.Description = description;
						}
					}

					var headerPlaceOfSupply = placeOfSupplyHelper.GetPlaceOfSupply(reader["AH_PlaceOfSupply"].ToNullableZString(), reader["AH_PlaceOfSupplyType"].ToNullableZString());
					if (headerPlaceOfSupply != null)
					{
						header.Info.PlaceOfSupply = headerPlaceOfSupply;
					}

					var taxBranchCode = reader["TaxBranchCode"].ToNullableZString();
					var taxBranchName = reader["TaxBranchName"].ToNullableZString();

					if (taxBranchCode.HasValue || taxBranchName.HasValue)
					{
						header.Info.TaxBranch = new Branch()
						{
							Code = taxBranchCode,
							Name = taxBranchName
						};
					}
					organizationPK = reader["AH_OH"].ToNullableZGuid();
					branchPK = reader["AH_GB"].ToNullableZGuid();

					if ((string)header.Info.Ledger == LedgerTypes.AccountsReceivable || (string)header.Info.Ledger == LedgerTypes.AccountsPayable)
					{
						if (header.Info.TransactionType == TransactionType.INV || header.Info.TransactionType == TransactionType.CRD || header.Info.TransactionType == TransactionType.ADJ)
						{
							var originalTransactionNumber = reader["OriginalTransactionNumber"].ToNullableZString();

							var isCreditNote = header.Info.TransactionType == TransactionType.CRD;

							var originalTransactionNumberOverride = reader["AH_OriginalTransactionNum"].ToNullableZString();
							var originalTransactionDateOverride = reader["AH_OriginalInvoiceDate"].ToNullableZDateTime();
							var originalTransactionNumberOverrideHasNonEmptyValue = originalTransactionNumberOverride.HasValue && !originalTransactionNumberOverride.Value.IsEmpty;

							if (originalTransactionNumber.HasValue && !originalTransactionNumber.Value.IsEmpty
								|| (isCreditNote && (originalTransactionNumberOverrideHasNonEmptyValue || originalTransactionDateOverride.HasValue)))
							{
								header.Info.OriginalReference = new OriginalReference(writerStrategy);
								if (originalTransactionNumber.HasValue && !originalTransactionNumber.Value.IsEmpty)
								{
									header.Info.OriginalReference.OriginalTransactionNumber = originalTransactionNumber;
								}
								else if (isCreditNote && originalTransactionNumberOverrideHasNonEmptyValue)
								{
									header.Info.OriginalReference.OriginalTransactionNumber = originalTransactionNumberOverride;
								}

								header.Info.OriginalReference.OriginalTransactionJobInvoiceNumber = reader["OriginalTransactionJobInvoiceNumber"].ToNullableZString();

								var originalTransactionDate = reader["OriginalTransactionDate"].ToNullableZDateTime();
								if (originalTransactionDate.HasValue)
								{
									header.Info.OriginalReference.OriginalTransactionDate = originalTransactionDate;
								}
								else if (isCreditNote && originalTransactionDateOverride.HasValue)
								{
									header.Info.OriginalReference.OriginalTransactionDate = originalTransactionDateOverride;
								}

								var originalTransactionReference = reader["OriginalTransactionReference"].ToNullableZString();
								if (originalTransactionReference.HasValue && !originalTransactionReference.Value.IsEmpty)
								{
									header.Info.OriginalReference.OriginalTransactionReference = originalTransactionReference;
								}

								var originalTransactionComplianceSubtype = reader["OriginalTransactionComplianceSubType"].ToNullableZString();
								if (originalTransactionComplianceSubtype.HasValue && !originalTransactionComplianceSubtype.Value.IsEmpty)
								{
									header.Info.OriginalReference.OriginalTransactionComplianceSubType = originalTransactionComplianceSubtype;
								}

								if (transactionHeaderPK != null)
								{
									originalTransactionPK = reader["AH_TransactionBelongsToGroup"] as Guid?;
								}

								if ((string)header.Info.Ledger == LedgerTypes.AccountsReceivable)
								{
									var originalTransactionAmendingReversingCode = reader["AH_ReceiptType"].ToNullableZString();
									var isReversal = reader["AH_IsCancelled"].ToNullableZBool();

									if (originalTransactionAmendingReversingCode.HasValue && isReversal.HasValue)
									{
										ZString code = ZString.Empty, description = ZString.Empty;

										if (isReversal.Value)
										{
											if (reversalCodeList.ContainsCode(originalTransactionAmendingReversingCode.Value))
											{
												code = originalTransactionAmendingReversingCode.Value;
												description = reversalCodeList.GetDescriptionFromCode(code);
											}
										}
										else
										{
											if (amendmentReasonCodeList.ContainsCode(originalTransactionAmendingReversingCode.Value))
											{
												code = originalTransactionAmendingReversingCode.Value;
												description = amendmentReasonCodeList.GetDescriptionFromCode(code);
											}
										}

										if (!code.IsEmpty)
										{
											header.Info.OriginalReference.OriginalTransactionAmendingReversingReason = new CodeDescriptionPair();
											header.Info.OriginalReference.OriginalTransactionAmendingReversingReason.Code = code;
											header.Info.OriginalReference.OriginalTransactionAmendingReversingReason.Description = description;
										}
									}
								}
							}
						}
					}

					if (header.Info.IsCancelled != null && header.Info.IsCancelled.Value)
					{
						var thisTransactionReceiptType = ah_ReceiptType.GetValueOrDefault();
						var subsequentTranactionReceiptType = reader["SubsequentTransactionReceiptType"].ToNullableZString().GetValueOrDefault();
						var receiptTypeForCancellationReason = !subsequentTranactionReceiptType.IsEmpty
							? subsequentTranactionReceiptType   // Has a reversal transaction
							: thisTransactionReceiptType;       // Is reversal transaction
						if (!receiptTypeForCancellationReason.IsEmpty)
						{
							header.Info.CancelReason = GetCancelReason(receiptTypeForCancellationReason, reversalCodeList, creditNoteCodeList);
						}
					}

					var ag_AccountNum = reader["AG_AccountNum"];
					var ag_Description = reader["AG_Description"];
					if (ag_AccountNum != DBNull.Value && ag_Description != DBNull.Value)
					{
						header.GLAccount = new GLAccount();
						header.GLAccount.AccountCode = (ZString?)(string)ag_AccountNum;
						header.GLAccount.Description = (ZString?)(string)ag_Description;
					}
					header.Organization = new OrganizationReference();
					header.Organization.Key = reader["OH_Code"] == DBNull.Value ? null : ((ZString?)(string)reader["OH_Code"]).Value.TrimEnd();
					header.Organization.Type = nameof(DataContextType.Organization);
					header.BankGLAccount = new GLAccount();
					header.BankGLAccount.AccountCode = reader["bankGLAccountNum"].ToNullableZString();
					header.BankGLAccount.Description = reader["bankGLDescription"].ToNullableZString();

					header.TransactionCount = (byte)reader["AH_TransactionCount"];

					headers.Add(header.PK, header);
				}
			}

			var orgAddresses = GetOrgAddresses(companyCode, batchNumber, "AccountingTransactionExportGetAddresses", transactionHeaderPK);
			var sendingOrgAddresses = GetOrgAddresses(companyCode, batchNumber, "AccountingTransactionExportGetSenderAddresses", transactionHeaderPK);
			var taxBranchOrgAddresses = new Dictionary<Guid, OrganizationAddress>();
			if (headers.Any(x => x.Value.Info.TaxBranch != null))
			{
				taxBranchOrgAddresses = GetOrgAddresses(companyCode, batchNumber, "AccountingTransactionExportGetTaxBranchAddresses", transactionHeaderPK);
			}

			var allOrgAddressesFromAllSources = orgAddresses.Values
					.Concat(sendingOrgAddresses.Values)
					.Concat(taxBranchOrgAddresses.Values);
			var registrationNumbers = GetRegistrationNumbers(GetUniqueListOfOrgCodes(allOrgAddressesFromAllSources));

			var bankAccounts = new Dictionary<string, List<BankAccount>>();

			var exporterExemptionDocumentTracking = GetExporterExemptionDocumentDetails(writerStrategy, companyCode, batchNumber, transactionHeaderPK);

			var transactionHeaderReferences = GetInvoiceRemittances(companyCode, headers.Select(x => x.Key).ToArray());

			var countryCode = GetCompanyData(companyCode).CountryCode;

			var includeGovernmentBatchReference = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IFeatureConstants>(countryCode)?.GetFeatureContants<SupportedFeatures>().Features.Contains(Features.IncludeGovernmentBatchReferenceInXUT) ?? false;

			foreach (TransactionHeaderRow header in headers.Values)
			{
				var sendingOrganizationAddress = GetOrganizationAddress(sendingOrgAddresses, header.PK, registrationNumbers);
				if (sendingOrganizationAddress != null)
				{
					header.Info.BranchAddress = sendingOrganizationAddress;
				}

				if (transactionHeaderReferences.TryGetValue(header.PK, out InvoiceRemittance invoiceRemittance))
				{
					header.Info.SetInvoiceRemittanceCollection(() => new List<InvoiceRemittance> { invoiceRemittance });
				}

				if (header.Organization.Key.HasValue && !((ZString)header.Organization.Key.Value).IsEmpty)
				{
					var organizationAddress = GetOrganizationAddress(orgAddresses, header.PK, registrationNumbers);
					if (organizationAddress != null)
					{
						header.Info.OrganizationAddress = organizationAddress;
					}

					if ((string)header.Info.Ledger == LedgerTypes.AccountsReceivable &&
						(header.Info.TransactionType == TransactionType.INV || header.Info.TransactionType == TransactionType.CRD || header.Info.TransactionType == TransactionType.ADJ))
					{
						List<BankAccount> orgAccountDetails;
						if (!bankAccounts.TryGetValue(header.Organization.Key, out orgAccountDetails))
						{
							orgAccountDetails = GetOrgARAccountDetails(header.Organization.Key, companyCode);
							bankAccounts.Add(header.Organization.Key, orgAccountDetails);
						}

						if (orgAccountDetails != null && orgAccountDetails.Count > 0)
						{
							header.Info.SetBankAccountCollection(() =>
							{
								var bank = new List<BankAccount>();
								foreach (var accountDetail in orgAccountDetails)
								{
									bank.Add(accountDetail);
								}
								return bank;
							});
						}

						var bankAccount = GetSellersBankAccount(organizationPK, header.Info.OSCurrency.Code.ToString(), branchPK, factory);
						if (bankAccount != null)
						{
							header.Info.SetSellersBankAccountCollection(() =>
							{
								var bank = new List<BankAccount>();
								bank.Add(bankAccount);
								return bank;
							});
						}
						
						Dictionary<Guid, DocumentTracking> listDocumentTracking;
						if (exporterExemptionDocumentTracking.TryGetValue(header.PK, out listDocumentTracking))
						{
							header.Info.SetOrganizationDocumentTrackingCollection(() => listDocumentTracking.Values.ToList());
						}
					}
				}
				if (header.Info.TaxBranch != null)
				{
					var taxBranchOrganizationAddress = GetOrganizationAddress(taxBranchOrgAddresses, header.PK, registrationNumbers);
					if (taxBranchOrganizationAddress != null)
					{
						header.Info.TaxBranchAddress = taxBranchOrganizationAddress;
					}
				}

				header.Info.SetAuthorizationDetailCollection(() => LoadAuthorizationDetails(transactionHeaderPK, includeGovernmentBatchReference));
				header.Info.OriginalReference?.SetAuthorizationDetailCollection(() => LoadAuthorizationDetails(originalTransactionPK, includeGovernmentBatchReference));
				header.Info.SetTransactionHeaderReferenceCollection(() => LoadTransactionHeaderReference(transactionHeaderPK, companyCode));
			}

			return headers;
		}

		Dictionary<string, List<RegistrationNumber>> GetRegistrationNumbers(string[] orgCodes)
		{
			var registrationNumbers = new Dictionary<string, List<RegistrationNumber>>();

			using (var table = new DataTable())
			{
				table.Locale = CultureInfo.InvariantCulture;
				table.Columns.Add((NoResString)"Value", typeof(string));

				foreach (string value in orgCodes)
				{
					table.Rows.Add(value);
				}

				List<RegistrationNumber> orgRegistrationNumbers = new List<RegistrationNumber>();
				var sqlQuery = "SELECT OH_Code, OK_RN_NKCodeCountry, RN_Desc, OK_CodeType, OK_CustomsRegNo FROM csfn_GetRegistrationNumbers(@OrgCodes) ORDER BY OH_Code";

				using (var command = GetCommand(sqlQuery))
				{
					command.AddTableValuedParameter("@OrgCodes", TVPHelper.TVP_nvarchar, table);

					var orgCode = string.Empty;
					var previousOrgCode = string.Empty;
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							orgCode = reader["OH_Code"].ToNullableZString();

							if (!registrationNumbers.ContainsKey(orgCode))
							{
								if (!string.IsNullOrEmpty(previousOrgCode) && !orgCode.Equals(previousOrgCode) && !registrationNumbers.ContainsKey(previousOrgCode))
								{
									registrationNumbers.Add(previousOrgCode, orgRegistrationNumbers);
								}
								orgRegistrationNumbers = new List<RegistrationNumber>();
							}
							Country country = new Country();
							country.Code = reader["OK_RN_NKCodeCountry"].ToNullableZString();
							country.Name = reader["RN_Desc"].ToNullableZString();
							RegistrationNumberType type = new RegistrationNumberType();
							type.Code = reader["OK_CodeType"].ToNullableZString();
							type.Description = null;
							var value = reader["OK_CustomsRegNo"].ToNullableZString();
							RegistrationNumber regNum = new RegistrationNumber() { CountryOfIssue = country, Type = type, Value = value };

							orgRegistrationNumbers.Add(regNum);
							previousOrgCode = orgCode;

							if (!registrationNumbers.ContainsKey(orgCode))
							{
								registrationNumbers.Add(orgCode, orgRegistrationNumbers);
							}
						}
					}
				}
			}

			return registrationNumbers;
		}

		public Dictionary<string, List<RegistrationNumber>> GetRegistrationNumbers_ForTestOnly(string[] orgCodes) => GetRegistrationNumbers(orgCodes);

		public Dictionary<Guid, Dictionary<Guid, DocumentTracking>> GetExporterExemptionDocumentDetails(IDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, Guid? transactionHeaderPK = null)
		{
			var result = new Dictionary<Guid, Dictionary<Guid, DocumentTracking>>();
			var readedDocumentTrackings = new Dictionary<Guid, DocumentTracking>();

			string sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetExporterExemptionDocumentDetails", companyCode, batchNumber, useOptimizedStoredProceduresForBatchExport: false, transactionHeaderPK);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					Guid headerPK = (Guid)reader[AccTransactionHeaderSchema.Constants.PK];
					Guid documentTrackingPK = (Guid)reader[JobRequiredDocumentSchema.Constants.PK];

					DocumentTracking documentTracking = null;

					if (!result.ContainsKey(headerPK))
					{
						readedDocumentTrackings = new Dictionary<Guid, DocumentTracking>();
						result.Add(headerPK, readedDocumentTrackings);
					}
					else
					{
						readedDocumentTrackings = result[headerPK];
					}

					if (!readedDocumentTrackings.ContainsKey(documentTrackingPK))
					{
						documentTracking = PopulateDocumentTracking(writerStrategy, reader);
						readedDocumentTrackings.Add(documentTrackingPK, documentTracking);
					}
					else
					{
						documentTracking = readedDocumentTrackings[documentTrackingPK];
					}

					if (documentTracking != null)
					{
						void GetAndAddNonNullAttributeToCollection(List<DocumentTrackingAttribute> collection)
						{
							var attribute = GetDocumentTrackingAttribute(reader);
							if (attribute != null)
							{
								collection.Add(attribute);
							}
						}

						if (documentTracking.DocumentTrackingAttributeCollection == null)
						{
							documentTracking.SetDocumentTrackingAttributeCollection(() =>
							{
								var collection = new List<DocumentTrackingAttribute>();
								GetAndAddNonNullAttributeToCollection(collection);

								return collection;
							});
						}
						else
						{
							GetAndAddNonNullAttributeToCollection(documentTracking.DocumentTrackingAttributeCollection);
						}
					}
				}
			}

			return result;
		}

		static DocumentTracking PopulateDocumentTracking(IDataObjectWriterStrategy writerStrategy, DbDataReader reader)
		{
			var documentTracking = new DocumentTracking(writerStrategy);
			var documentNumber = reader["EQ_DocNumber"].ToNullableZString();
			if (!string.IsNullOrEmpty(documentNumber))
			{
				documentTracking.DocumentNumber = documentNumber;
			}

			var category = reader["EQ_DocCategory"].ToNullableZString();
			if (!string.IsNullOrEmpty(category))
			{
				documentTracking.Category = new CodeDescriptionPair();
				documentTracking.Category.Code = category;
				documentTracking.Category.Description = JobRequiredDocumentLookups.AllCategoryType_List.GetDescriptionFromCode(category);
			}

			var docType = reader["EQ_DocType"].ToNullableZString();
			if (!string.IsNullOrEmpty(docType))
			{
				documentTracking.DocumentType = new CodeDescriptionPair();
				documentTracking.DocumentType.Code = docType;
				documentTracking.DocumentType.Description = reader["RT_Desc"].ToNullableZString();
			}

			var docPeriod = reader["EQ_DocPeriod"].ToNullableZString();
			if (!string.IsNullOrEmpty(docPeriod))
			{
				documentTracking.DocumentPeriod = new CodeDescriptionPair();
				documentTracking.DocumentPeriod.Code = docPeriod;
				documentTracking.DocumentPeriod.Description = JobRequiredDocumentLookups.DocumentPeriod_List.GetDescriptionFromCode(docPeriod);
			}

			var docUsage = reader["EQ_DocUsage"].ToNullableZString();
			if (!string.IsNullOrEmpty(docUsage))
			{
				documentTracking.DocumentUsage = new CodeDescriptionPair();
				documentTracking.DocumentUsage.Code = docUsage;
				documentTracking.DocumentUsage.Description = JobRequiredDocumentLookups.DocUsage_List.GetDescriptionFromCode(docUsage);
			}

			var receivedDate = reader["EQ_DateReceived"].ToNullableZDateTimeOffset(); // Direct access to database required
			if (receivedDate != null)
			{
				documentTracking.ReceivedDate = receivedDate.Value.ToZDateTime();
			}

			var validToDate = reader["EQ_ValidToDate"].ToNullableZDateTime();
			if (validToDate != null)
			{
				documentTracking.ValidToDate = validToDate;
			}

			var countryCode = reader["EQ_RN_NKRelatedCountry"].ToNullableZString();
			if (!string.IsNullOrEmpty(countryCode))
			{
				documentTracking.Country = new Country();
				documentTracking.Country.Code = countryCode;
				documentTracking.Country.Name = reader["RN_Desc"].ToNullableZString();
			}

			var documentNote = reader["EQ_DocumentNotes"].ToNullableZString();
			if (!string.IsNullOrEmpty(documentNote))
			{
				documentTracking.DocumentNote = documentNote;
			}

			var isCreditControlDocument = reader["EQ_CreditControlDoc"].ToNullableZBool();
			if (isCreditControlDocument != null)
			{
				documentTracking.IsDocumentCreditControl = isCreditControlDocument;
			}

			var isOriginalDocumentRequired = reader["EQ_OriginalDocRequired"].ToNullableZBool();
			if (isOriginalDocumentRequired != null)
			{
				documentTracking.IsOriginalDocumentRequired = isOriginalDocumentRequired;
			}

			var receiverFromBroker = reader["EQ_RcvFromCustomsBroker"].ToNullableZDateTime();
			if (receiverFromBroker != null)
			{
				documentTracking.ReceivedFromCustomsBroker = receiverFromBroker;
			}

			var sentToBroker = reader["EQ_SntToCustomsBroker"].ToNullableZDateTime();
			if (sentToBroker != null)
			{
				documentTracking.SentToCustomsBroker = sentToBroker;
			}

			var returnToShipper = reader["EQ_ReturnToShipper"].ToNullableZDateTime();
			if (returnToShipper != null)
			{
				documentTracking.ReturnToShipper = returnToShipper;
			}

			return documentTracking;
		}

		static DocumentTrackingAttribute GetDocumentTrackingAttribute(DbDataReader reader)
		{
			var attributeName = reader["D0_AttribName"].ToNullableZString();
			var attributeValue = reader["D0_AttribValue"].ToNullableZString();

			if (!string.IsNullOrEmpty(attributeName))
			{
				var attribute = new DocumentTrackingAttribute();
				attribute.Type = attributeName;
				attribute.Value = attributeValue;

				return attribute;
			}

			return null;
		}

		OrganizationAddress GetOrganizationAddress(Dictionary<Guid, OrganizationAddress> orgAddresses, Guid transactionHeaderPk,
			Dictionary<string, List<RegistrationNumber>> registrationNumbers)
		{
			OrganizationAddress organizationAddress;
			if (orgAddresses.TryGetValue(transactionHeaderPk, out organizationAddress))
			{
				List<RegistrationNumber> orgRegistrationNumbers;
				if (registrationNumbers.TryGetValue(organizationAddress.OrganizationCode, out orgRegistrationNumbers))
				{
					if (orgRegistrationNumbers != null && orgRegistrationNumbers.Any())
					{
						organizationAddress.SetRegistrationNumberCollection(() =>
						{
							var result = new List<RegistrationNumber>();
							result.AddRange(orgRegistrationNumbers);
							return result;
						});
					}
				}
			}
			return organizationAddress;
		}

		string[] GetUniqueListOfOrgCodes(IEnumerable<OrganizationAddress> allOrgAddressesFromAllSources) => allOrgAddressesFromAllSources
			.Where(i => i.OrganizationCode.HasValue)
			.Select(i => i.OrganizationCode.Value.ToString())
			.Distinct()
			.ToArray();

		[SuppressMessage("CargoWiseOne", "CW1110:DoNotUseColumnNamesDirectly", Justification = "Baseline")]
		public Dictionary<Guid, List<RelatedJournalLineRow>> GetJournalLines(string companyCode, long batchNumber, Guid? transactionHeaderPK = null)
		{
			var result = new Dictionary<Guid, List<RelatedJournalLineRow>>();

			if (GetIncludeRelatedJournalsInUniversalXMLTransaction())
			{
				var sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetRelatedJournals", companyCode, batchNumber, true, transactionHeaderPK);

				using (var command = GetCommand(sqlText))
				using (var reader = command.ExecuteReader())
				{
					var headerPK = Guid.Empty;
					List<RelatedJournalLineRow> journalLineRowList = null;
					while (reader.Read())
					{
						var journalLineRow = new RelatedJournalLineRow();
						journalLineRow.PK = (Guid)reader["AH_PK"];
						journalLineRow.Ledger = (ZString)(string)reader["AH_Ledger"];
						journalLineRow.TransactionCategory = (ZString)(string)reader["AH_TransactionCategory"];
						journalLineRow.TransactionType = (ZString)(string)reader["AH_TransactionType"];

						journalLineRow.DueDate = reader["AH_DueDate"].ToNullableZDateTime();
						journalLineRow.ExchangeRate = reader["AH_ExchangeRate"].ToNullableZDecimal();
						journalLineRow.FullyPaidDate = reader["AH_FullyPaidDate"].ToNullableZDateTime();

						var ag_AccountNum = reader["AG_AccountNum"];
						var ag_Description = reader["AG_Description"];
						if (ag_AccountNum != DBNull.Value && ag_Description != DBNull.Value)
						{
							journalLineRow.GLAccount = new GLAccount();
							journalLineRow.GLAccount.AccountCode = (ZString?)(string)ag_AccountNum;
							journalLineRow.GLAccount.Description = (ZString?)(string)ag_Description;
						}

						journalLineRow.LocalTotal = reader["AH_LocalTotal"].ToNullableZDecimal();

						journalLineRow.LocalCurrency = new Currency();
						journalLineRow.LocalCurrency.Code = reader["Local_RX_Code"].ToNullableZString();
						journalLineRow.LocalCurrency.Description = reader["Local_RX_Desc"].ToNullableZString();

						journalLineRow.Organization = new OrganizationReference();
						journalLineRow.Organization.Key = reader["OH_Code"] == DBNull.Value ? null : ((ZString?)(string)reader["OH_Code"]).Value.TrimEnd();
						journalLineRow.Organization.Type = nameof(DataContextType.Organization);

						journalLineRow.OSCurrency = new Currency();
						journalLineRow.OSCurrency.Code = reader["OS_RX_Code"].ToNullableZString();
						journalLineRow.OSCurrency.Description = reader["OS_RX_Desc"].ToNullableZString();

						journalLineRow.IsCancelled = reader["AH_IsCancelled"].ToNullableZBool();
						journalLineRow.OSTotal = reader["AH_OSTotal"].ToNullableZDecimal();
						journalLineRow.OutstandingAmount = reader["AH_OutstandingAmount"].ToNullableZDecimal();
						journalLineRow.PostDate = reader["AH_PostDate"].ToNullableZDateTime();
						journalLineRow.InvoiceDate = reader["AH_InvoiceDate"].ToNullableZDateTime();
						journalLineRow.Description = reader["AH_Desc"].ToNullableZString();

						if (headerPK != (Guid)reader["AH_TransactionBelongsToGroup"])
						{
							headerPK = (Guid)reader["AH_TransactionBelongsToGroup"];
							journalLineRowList = new List<RelatedJournalLineRow>();
							result.Add(headerPK, journalLineRowList);
						}

						journalLineRowList.Add(journalLineRow);
					}
				}
			}

			return result;
		}

		public string[] GetUniqueListOfOrgCodes_ForTestOnly(IEnumerable<OrganizationAddress> allOrgAddressesFromAllSources) => GetUniqueListOfOrgCodes(allOrgAddressesFromAllSources);

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public Dictionary<Guid, TransactionLineRow> GetTransactionLines(string companyCode, long batchNumber, Guid? transactionHeaderPK = null)
		{
			bool enableGovernmentChargeCode = GetEnableGovernmentChargeCode(companyCode);
			var enableSupplyTypeClassificationCodes = GetEnableSupplyTypeClassificationCodes(companyCode);
			var supplyTypeClassificationCodesList = enableSupplyTypeClassificationCodes ? GetSupplyTypeClassificationCodesList(companyCode) : null;
			var noteGLAccountsStatisticalUnitsofMeasurement = GetNoteGLAccountsStatisticalUnitsofMeasurement(companyCode);
			var goodServiceTypesList = new GoodServiceTypes();
			var chargeTypesList = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
			var taxGroupCodes = GetTaxGroupCodes(companyCode);
			var lines = new Dictionary<Guid, TransactionLineRow>();
			var argumentsForCostTaxCalculation = new Dictionary<Guid, (ZDecimal? chargeAmount, ZString? chargeCurrency, Guid? taxRatePK, ZDecimal? chargeTaxRate, ZDecimal? chargeEffectiveTaxRate, Guid? chargeCodePK, Guid? jr_E6, ZString? countryCode, Guid? companyPK)>();
			var argumentsForTaxCalculation = new Dictionary<Guid, (ZString? lineType, ZString? lineCurrencyCode, ZDecimal? lineOSAmount, ZDecimal? lineLocalTaxAmount,
																	ZString? chargeCurrency, ZDecimal? chargeExchangeRate, ZDecimal? chargeOSAmount, ZDecimal? chargeLocalAmount,
																	Guid? chargeTaxPK, ZDecimal? chargeTaxRate, ZDecimal? chargeEffectiveTaxRate, Guid? chargeChargeCode, Guid? chargeCompany, ZString? countryCode,
																	ZString? localCurrencyCode, ZString? chargeInvoiceType, ZString? chargeSellInvoiceCurrency)>();
			var lineRelatedPKs = new Dictionary<Guid, (Guid chargePK, Guid consolCostPK)>();

			var placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(companyCode);

			var sqlText = SqlTextForProcedureCallWithDate("AccountingTransactionExportGetLines", companyCode, batchNumber, ZDateTime.Today, true, transactionHeaderPK);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					TransactionLineRow line = new TransactionLineRow();

					PopulateTransactionLineRow_Part1(reader, line);

					if (reader["ChargeCurrency"] != DBNull.Value)
					{
						line.ChargeCurrency = new Currency();
						line.ChargeCurrency.Code = reader["ChargeCurrency"] == DBNull.Value ? null : (ZString?)(string)reader["ChargeCurrency"];
						line.ChargeCurrency.Description = reader["ChargeCurrencyDesc"] == DBNull.Value ? null : (ZString?)(string)reader["ChargeCurrencyDesc"];
						line.ChargeExchangeRate = reader["ChargeExchangeRate"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["ChargeExchangeRate"];
					}

					line.ChargeAmount = reader["ChargeAmount"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["ChargeAmount"];
					var countryCode = reader["AT_RN_NKCountry"].ToNullableZString();

					var chargeTaxPK = reader["ChargeAT_PK"] == DBNull.Value ? null : (Guid?)reader["ChargeAT_PK"];
					var chargeChargeCode = reader["ChargeChargeCode"] == DBNull.Value ? null : (Guid?)reader["ChargeChargeCode"];
					var chargeCompany = reader["ChargeCompany"] == DBNull.Value ? null : (Guid?)reader["ChargeCompany"];

					line.ChargeTaxID = GetTaxID(reader, (NoResString)"Charge");
					line.ChargeTaxMessageID = GetTaxMessageID(reader, (NoResString)"Charge");

					var consolCostPK = reader["E6_PK"] == DBNull.Value ? Guid.Empty : (Guid)reader["E6_PK"];
					Guid jobChargePK = Guid.Empty;
					if (consolCostPK == Guid.Empty && reader["JR_PK"] != DBNull.Value)
					{
						jobChargePK = (Guid)reader["JR_PK"];
					}
					lineRelatedPKs.Add(line.PK, (jobChargePK, consolCostPK));

					var chargeTaxRate = line.ChargeTaxID == null ? null : (ZDecimal?)AccTaxRate.GetRate(line.ChargeTaxID.TaxType?.Code ?? ZString.Empty, line.ChargeTaxID.TaxRate ?? ZDecimal.Zero);
					var chargeEffectiveTaxRate = line.ChargeTaxID == null ? null : (ZDecimal?)AccTaxRate.GetEffectiveExtraRate(line.ChargeTaxID.TaxType?.Code ?? ZString.Empty, line.ChargeTaxID.TaxRate ?? ZDecimal.Zero, line.ChargeTaxID.ExtraTaxType?.Code ?? ZString.Empty, line.ChargeTaxID.ExtraTaxRate ?? ZDecimal.Zero);

					var localCurrencyCode = reader["GC_RX_NKLocalCurrency"].ToNullableZString();
					var chargeInvoiceType = reader["chargeInvoiceType"].ToNullableZString();
					var chargeSellInvoiceCurrency = reader["chargeSellInvoiceCurrency"].ToNullableZString();

					argumentsForTaxCalculation.Add(line.PK, (line.LineType, line.OSCurrency.Code, line.OSAmount, line.GSTVAT,
						line.ChargeCurrency?.Code, line.ChargeExchangeRate, line.ChargeAmount,
						reader["ChargeLocalAmount"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["ChargeLocalAmount"],
					chargeTaxPK, chargeTaxRate, chargeEffectiveTaxRate,
						chargeChargeCode, chargeCompany, countryCode, localCurrencyCode, chargeInvoiceType, chargeSellInvoiceCurrency));

					var isCostTaxAmountOverridden = reader["JR_IsCostTaxAmountOverridden"] == DBNull.Value ? null : (ZBool?)(bool)reader["JR_IsCostTaxAmountOverridden"];
					if (line.LineType.HasValue && isCostTaxAmountOverridden.HasValue)
					{
						if (!isCostTaxAmountOverridden.Value)
						{
							var jr_E6 = reader["ChargeJR_E6"] == DBNull.Value ? null : (Guid?)reader["ChargeJR_E6"];
							argumentsForCostTaxCalculation.Add(line.PK, (line.ChargeAmount, line.ChargeCurrency?.Code,
								chargeTaxPK, chargeTaxRate, chargeEffectiveTaxRate,
								chargeChargeCode, jr_E6, countryCode, chargeCompany));
						}
						else
						{
							line.ChargeGST = reader["ChargeGST"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["ChargeGST"];
						}
					}

					var linePlaceOfSupply = placeOfSupplyHelper.GetPlaceOfSupply(
						reader["AL_PlaceOfSupply"] == DBNull.Value ? null : (ZString?)(string)reader["AL_PlaceOfSupply"],
						reader["AL_PlaceOfSupplyType"] == DBNull.Value ? null : (ZString?)(string)reader["AL_PlaceOfSupplyType"]);

					if (linePlaceOfSupply != null)
					{
						line.PlaceOfSupply = linePlaceOfSupply;
					}

					PopulateTransactionLineRow_Part2(reader, line, countryCode, taxGroupCodes, noteGLAccountsStatisticalUnitsofMeasurement, goodServiceTypesList, chargeTypesList);

					PopulateTransactionLineRow_Part3(enableGovernmentChargeCode, reader, line);

					if (enableSupplyTypeClassificationCodes)
					{
						var supplyTypeCode = reader["AL_SupplyType"].ToNullableZString() ?? ZString.Empty;
						var supplyType = supplyTypeClassificationCodesList.FindByCode(supplyTypeCode);
						line.SupplyType = new CodeDescriptionPair
						{
							Code = supplyType?.Code ?? supplyTypeCode,
							Description = supplyType?.Description ?? ZString.Empty
						};
					}

					var taxBranchCode = reader["TaxBranchCode"].ToNullableZString();
					var taxBranchName = reader["TaxBranchName"].ToNullableZString();

					if (taxBranchCode.HasValue || taxBranchName.HasValue)
					{
						line.TaxBranch = new Branch()
						{
							Code = taxBranchCode,
							Name = taxBranchName
						};
					}

					PopulateTransactionLineRow_Taxes(reader, line, countryCode.GetValueOrDefault(),
						(int)reader["OSSubUnitRatio"],
						(bool)reader["GC_IsReciprocal"],
						_taxAmountCalculator);

					lines.Add(line.PK, line);
				}
			}

			PopulateSubAccountCollection(lines);
			PopulateRatingBasisCollection(lines, lineRelatedPKs);

			object cache = null;
			ICalculateOSSellTaxForCharge taxCalculator_Sell = null;
			foreach (var pk in lines.Keys)
			{
				var args = argumentsForTaxCalculation[pk];
				if (args.chargeCurrency.HasValue && (args.lineType.Value == "REV" || args.lineType.Value == "WIP"))
				{
					var arlineDetails = new LineDetails
					{
						Type = args.lineType.Value,
						CurrencyCode = args.lineCurrencyCode.Value,
						LocalTaxAmount = args.lineLocalTaxAmount.Value,
						OSAmount = args.lineOSAmount.Value
					};

					var chargeSellDetails = new ChargeSellDetails
					{
						InvoiceType = args.chargeInvoiceType.GetValueOrDefault(),
						SellExRate = args.chargeExchangeRate.GetValueOrDefault(),
						SellCurrencyCode = args.chargeCurrency.GetValueOrDefault(),
						SellInvoiceCurrencyCode = args.chargeSellInvoiceCurrency.GetValueOrDefault(),
						LocalCurrencyCode = args.localCurrencyCode.GetValueOrDefault(),
						SellOSAmount = args.chargeOSAmount.GetValueOrDefault(),
						LocalSellAmount = args.chargeLocalAmount.GetValueOrDefault(),
						SellTaxRatePK = args.chargeTaxPK.GetValueOrDefault(),
						TaxRate = args.chargeTaxRate.GetValueOrDefault(),
						EffectiveExtraTaxRate = args.chargeEffectiveTaxRate.GetValueOrDefault(),
						ChargeCodePK = args.chargeChargeCode.GetValueOrDefault(),
						CompanyPK = args.chargeCompany.GetValueOrDefault(),
						CountryCode = args.countryCode.GetValueOrDefault()
					};

					taxCalculator_Sell = taxCalculator_Sell ?? ObjectFactory.Get<ICalculateOSSellTaxForCharge>();
					lines[pk].ChargeGST = !args.chargeTaxPK.HasValue ? 0 : taxCalculator_Sell.CalculateOSSellGSTAmount(arlineDetails, chargeSellDetails, ref cache);
				}
			}

			ICalculateOSCostTaxForCharge taxCalculator_Cost = null;
			foreach (var pk in argumentsForCostTaxCalculation.Keys)
			{
				var args = argumentsForCostTaxCalculation[pk];
				if (args.chargeCurrency.HasValue)
				{
					taxCalculator_Cost = taxCalculator_Cost ?? ObjectFactory.Get<ICalculateOSCostTaxForCharge>();
					lines[pk].ChargeGST = !args.taxRatePK.HasValue ? 0 : taxCalculator_Cost.CalculateOSCostGSTAmountWhenNotOverridden
						(args.chargeAmount.Value, args.chargeCurrency.Value, args.taxRatePK.Value, args.chargeTaxRate, args.chargeEffectiveTaxRate, args.chargeCodePK.Value, args.jr_E6 ?? Guid.Empty, args.countryCode, args.companyPK.Value, ref cache);
				}
			}

			sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT 
	YC_AL_TransactionLine, 
	YC_PostDate, 
	YC_TaxAmount
FROM 
	AccountingTransactionExportGetCashBasisVATLines('{0}', {1}{2})
option(recompile)", companyCode, batchNumber.ToString(CultureInfo.InvariantCulture), transactionHeaderPK == null ? (NoResString)", DEFAULT" : string.Format(CultureInfo.InvariantCulture, ", '{0}'", transactionHeaderPK));

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					Guid linePK = (Guid)reader["YC_AL_TransactionLine"];
					TransactionLineRow row = null;
					if (!lines.TryGetValue(linePK, out row))
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to find TransactionLine: {0} for Cash Basis VAT", linePK.ToString()));
					}

					var cashBasisVATLine = new CashBasisVATRow();
					cashBasisVATLine.LinePK = linePK;
					cashBasisVATLine.PostDate = (ZDateTime)(DateTime)reader["YC_PostDate"];
					cashBasisVATLine.TaxAmount = (ZDecimal)(decimal)reader["YC_TaxAmount"];

					row.CashBasisVATLines.Add(cashBasisVATLine);
				}
			}

			return lines;
		}

		public List<TransactionLineRow> ExportLineRowsOfRelatedJournal(Guid relatedJournalPk)
		{
			var transactionLineRowList = new List<TransactionLineRow>();
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT 
AG_AccountNum,
AG_Description,
AL_LineAmount,
AL_PostDate,
AL_PostPeriod,
AL_RX_NKTransactionCurrency
FROM 
	dbo.AccTransactionLines
LEFT JOIN
	dbo.AccGLHeader
ON AccGLHeader.AG_PK = AccTransactionLines.AL_AG
WHERE AccTransactionLines.AL_AH = '{0}'
", relatedJournalPk);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var transactionLineRow = new TransactionLineRow();
					transactionLineRow.GLAccount = new GLAccount();
					transactionLineRow.GLAccount.AccountCode = (ZString?)(string)reader["AG_AccountNum"];
					transactionLineRow.GLAccount.Description = (ZString?)(string)reader["AG_Description"];
					transactionLineRow.LineAmount = (ZDecimal?)(decimal)reader["AL_LineAmount"];
					transactionLineRow.PostDate = (ZDateTime?)(DateTime)reader["AL_PostDate"];
					transactionLineRow.LocalCurrency = new Currency();
					transactionLineRow.LocalCurrency.Code = (ZString?)(string)reader["AL_RX_NKTransactionCurrency"];
					transactionLineRowList.Add(transactionLineRow);
				}
			}

			return transactionLineRowList;
		}

		static void PopulateTransactionLineRow_Part1(DbDataReader reader, TransactionLineRow line)
		{
			line.BatchSequence = reader["BatchSequence"] == DBNull.Value ? null : (ZInt?)(int)reader["BatchSequence"];
			line.OriginalBatchNumber = reader["OriginalBatchNumber"] == DBNull.Value ? null : (ZInt?)(int)reader["OriginalBatchNumber"];
			line.OriginalBatchSequence = reader["OriginalBatchSequence"] == DBNull.Value ? null : (ZInt?)(int)reader["OriginalBatchSequence"];

			line.PK = (Guid)reader["AL_PK"];
			object value = reader["AL_AH"];
			line.TransactionHeader = value == DBNull.Value ? Guid.Empty : (Guid)value;
			line.LineType = (string)reader["AL_LineType"];
			value = reader["AL_PostDate"];
			line.PostDate = value == DBNull.Value ? null : (ZDateTime?)(DateTime)value;
			value = reader["AL_ReverseDate"];
			line.ReverseDate = value == DBNull.Value ? null : (ZDateTime?)(DateTime)value;
			value = reader["D3_RecognitionDate"];
			line.JobRecognitionDate = value == DBNull.Value ? null : (ZDateTime?)(DateTime)value;
			value = reader["AL_TaxDate"];
			line.TaxDate = value == DBNull.Value ? null : (ZDate?)(DateTime)value;
			line.LineAmount = reader["AL_LineAmount"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["AL_LineAmount"];

			line.Sequence = reader["AL_Sequence"] == DBNull.Value ? null : (ZInt?)(short)reader["AL_Sequence"];
			line.Description = reader["AL_Desc"] == DBNull.Value ? null : (ZString?)(string)reader["AL_Desc"];
			line.LocalCurrency = new Currency();
			line.LocalCurrency.Code = reader["GC_RX_NKLocalCurrency"] == DBNull.Value ? null : (ZString?)(string)reader["GC_RX_NKLocalCurrency"];
			line.LocalCurrency.Description = reader["localCurrencyDesc"] == DBNull.Value ? null : (ZString?)(string)reader["localCurrencyDesc"];
			line.LocalCurrencySubUnitRatio = (int)reader["localSubUnitRatio"];
			line.GSTVAT = reader["AL_GSTVAT"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["AL_GSTVAT"];
			line.GSTVATBasis = reader["AL_GSTVATBasis"] == DBNull.Value ? null : (ZString?)(string)reader["AL_GSTVATBasis"];
			line.WHTTax = reader["AL_WithholdingTax"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["AL_WithholdingTax"];
			line.OSCurrency = new Currency();
			line.OSCurrency.Code = reader["AL_RX_NKTransactionCurrency"] == DBNull.Value ? null : (ZString?)(string)reader["AL_RX_NKTransactionCurrency"];
			line.OSCurrency.Description = reader["OSCurrencyDesc"] == DBNull.Value ? null : (ZString?)(string)reader["OSCurrencyDesc"];
			line.OSAmount = reader["AL_OSAmount"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["AL_OSAmount"];
			line.CashAdvanceAmount = reader["CashAdvanceReceived"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["CashAdvanceReceived"];
		}

		static void PopulateTransactionLineRow_Part2(
			DbDataReader reader,
			TransactionLineRow line,
			ZString? countryCode,
			ICodeDescriptionBoolRelatedItemList taxGroupCodes,
			ICodeDescriptionPairList noteGLAccountsStatisticalUnitsofMeasurement,
			ICodeDescriptionPairList goodServiceTypes,
			ICodeDescriptionPairList chargeTypes)
		{
			line.ChargeWithholdingTaxID = GetWithholdingTaxID(reader, (NoResString)"Charge");
			line.TaxMessageID = GetTaxMessageID(reader, "", countryCode, taxGroupCodes);
			if (reader["AG_AccountNum"] != DBNull.Value && reader["AG_Description"] != DBNull.Value)
			{
				line.GLAccount = new GLAccount();
				line.GLAccount.AccountCode = (ZString?)(string)reader["AG_AccountNum"];
				line.GLAccount.Description = (ZString?)(string)reader["AG_Description"];

				var statisticalUnit = reader["AG_StatisticalUnits"] == DBNull.Value || line.LineType.Value != TransactionTypes.GLNoteJournal ?
					null : reader["AG_StatisticalUnits"].ToNullableZString();
				line.GLAccount.StatisticalUnit = statisticalUnit.HasValue ?
					new CodeDescriptionPair
					{
						Code = statisticalUnit,
						Description = noteGLAccountsStatisticalUnitsofMeasurement?.GetDescriptionFromCode(statisticalUnit)
					}
				: null;
			}
			if (reader["AC_Code"] != DBNull.Value && reader["AC_Desc"] != DBNull.Value)
			{
				line.ChargeCode = new ChargeCode();
				line.ChargeCode.Code = reader["AC_Code"].ToNullableZString();
				line.ChargeCode.Description = reader["AC_Desc"].ToNullableZString();
				var goodServiceTypeCode = reader["AC_GoodsServiceType"].ToNullableZString();
				if (goodServiceTypeCode.HasValue)
				{
					line.ChargeCode.Class = new CodeDescriptionPair() { Code = goodServiceTypeCode, Description = goodServiceTypes.GetDescriptionFromCode(goodServiceTypeCode.Value) };
				}
				var chargeTypeCode = reader["AC_ChargeType"].ToNullableZString();
				if (chargeTypeCode.HasValue)
				{
					line.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = chargeTypeCode, Description = chargeTypes.GetDescriptionFromCode(chargeTypeCode.Value) };
				}
			}
			line.Job = new EntityReference();
			line.Job.Key = reader["JH_JobNum"] == DBNull.Value ? null : (ZString?)(string)reader["JH_JobNum"];
			line.Job.Type = AccountingDataTransferConstants.DataContextTypeString.Job;
		}

		static void PopulateTransactionLineRow_Part3(
			bool enableGovernmentChargeCode,
			DbDataReader reader,
			TransactionLineRow line)
		{
			if (reader["ConsolSource"] != DBNull.Value)
			{
				line.CostSource = new EntityReference();
				line.CostSource.Key = (ZString?)(string)reader["ConsolSource"];
				if (reader["ConsolType"] != DBNull.Value)
				{
					var context = GenericConsolHelper.GetDataContextTypeByParentTableCode((string)reader["ConsolType"]);
					if (context != null)
					{
						line.CostSource.Type = context.ToString();
					}
				}
			}
			line.Branch = new Branch();
			line.Branch.Code = reader["GB_Code"] == DBNull.Value ? null : (ZString?)(string)reader["GB_Code"];
			line.Branch.Name = reader["GB_BranchName"] == DBNull.Value ? null : (ZString?)(string)reader["GB_BranchName"];
			line.Department = new Department();
			line.Department.Code = reader["GE_Code"] == DBNull.Value ? null : (ZString?)(string)reader["GE_Code"];
			line.Department.Name = reader["GE_Desc"] == DBNull.Value ? null : (ZString?)(string)reader["GE_Desc"];
			line.Organization = new OrganizationReference();
			line.Organization.Key = reader["OH_Code"] == DBNull.Value ? null : ((ZString?)(string)reader["OH_Code"]).Value.TrimEnd();
			line.Organization.Type = nameof(DataContextType.Organization);
			line.IsFinalCharge = reader["AL_IsFinalCharge"] == DBNull.Value ? null : new ZBool(reader["AL_IsFinalCharge"]);
			if (enableGovernmentChargeCode)
			{
				line.GovtChargeCode = reader["AL_GovtChargeCode"].ToNullableZString();
			}

			var al_RevRecognitionType = reader["AL_RevRecognitionType"].ToNullableZString();
			line.RevRecognitionType = al_RevRecognitionType.HasValue ? new RevenueRecognitionTypeConverter().ToEnumValue(al_RevRecognitionType.Value) : null;

			line.ExternalCreditorCode = reader["ExternalCreditorCode"].ToNullableZString();
			line.ExternalDebtorCode = reader["ExternalDebtorCode"].ToNullableZString();
		}

		static void PopulateTransactionLineRow_Taxes(DbDataReader reader, TransactionLineRow line, ZString countryCode, int subUnitRatio, bool isReciprocal, ITaxAmountCalculator taxAmountCalculator)
		{
			decimal aL_ExchangeRate = (decimal)reader["AL_ExchangeRate"];

			(line.OSTaxAmount, line.OSExTaxAmount) = taxAmountCalculator.SplitOSTotalToTaxAndExTaxAmounts(
				line.GSTVAT.Value,
				() => ExchangeRate.LocalToForeign(line.LineAmount.Value, aL_ExchangeRate, subUnitRatio, isReciprocal),
				line.OSAmount.Value);

			if (line.WHTTax.HasValue)
			{
				line.OSWHTTax = ExchangeRate.LocalToForeign(line.WHTTax.Value, aL_ExchangeRate, subUnitRatio, isReciprocal);
			}
			line.TaxID = GetTaxID(reader);
			if (line.TaxID != null && line.TaxID.ExtraTaxType != null)
			{
				var extraTaxRateType = line.TaxID.ExtraTaxType.Code ?? ZString.Empty;
				if (!string.IsNullOrEmpty(extraTaxRateType))
				{
					var taxRateType = line.TaxID.TaxType.Code ?? ZString.Empty;

					var osExtraVATAmount = AccTaxRate.GetIsMexicoNeedExtraType(countryCode, extraTaxRateType)
						? ObjectFactory.Get<ITaxAmountCalculator>().GetExtraTaxAmountFromExTaxAmount((ZDecimal)line.OSExTaxAmount, AccTaxRate.GetEffectiveExtraRate(taxRateType, line.TaxID.TaxRate.Value, extraTaxRateType, line.TaxID.ExtraTaxRate.Value))
						: TaxRate.GetExtraTaxAmountFromTaxAmount((ZDecimal)line.OSTaxAmount, line.TaxID.TaxRate.Value, taxRateType, line.TaxID.ExtraTaxRate.Value, extraTaxRateType, countryCode, line.OSAmount.Value);

					line.OSExtraVATAmount = Utilities.Round(osExtraVATAmount, ExchangeRate.Decimals(subUnitRatio));

					ZDecimal? localExtraVATAmount = reader["AL_GSTVATExtra"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["AL_GSTVATExtra"];
					if (!localExtraVATAmount.HasValue || localExtraVATAmount.Value.IsEmpty || !AccTaxRate.GetIsLocalExtraTaxAmountValuePersistent(countryCode, extraTaxRateType))
					{
						var localExtraVATAmountCalc = isReciprocal ? line.OSExtraVATAmount.Value * aL_ExchangeRate : line.OSExtraVATAmount.Value / aL_ExchangeRate;
						line.LocalExtraVATAmount = Utilities.Round(localExtraVATAmountCalc, ExchangeRate.Decimals(line.LocalCurrencySubUnitRatio));
					}
					else
					{
						line.LocalExtraVATAmount = Utilities.Round(localExtraVATAmount.Value, ExchangeRate.Decimals(line.LocalCurrencySubUnitRatio));
						if (!AccTaxRate.GetIsIndiaStateTax(countryCode, taxRateType, extraTaxRateType)) //For India State Tax we cannot apply the high precision exchange rate to Local Extra Tax rate to get the OS Extra tax rate. The reason is for India OS State tax component needs to be the same as the Central Tax component. It is only guranteed if and only if the tax is recalculated.
						{
							line.OSExtraVATAmount = ExchangeRate.LocalToForeign(localExtraVATAmount.Value, aL_ExchangeRate, subUnitRatio, isReciprocal);
						}
					}
				}
			}

			line.WithholdingTaxID = GetWithholdingTaxID(reader);
			line.CompanyPK = reader["GC_PK"] == DBNull.Value ? Guid.Empty : (Guid)reader["GC_PK"];
			line.BranchPK = reader["AL_GB"] == DBNull.Value ? Guid.Empty : (Guid)reader["AL_GB"];
			line.DepartmentPK = reader["AL_GE"] == DBNull.Value ? Guid.Empty : (Guid)reader["AL_GE"];
			line.IsCashBasisVATOnly = reader["IsCashBasisVATOnly"] != DBNull.Value && (bool)reader["IsCashBasisVATOnly"];
			line.InputGSTVATRecoverable = (decimal)reader["InputGSTVATRecoverable"];
			line.GSTVATRecoverable = reader["GSTVATRecoverable"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["GSTVATRecoverable"];
			line.GSTVATNotRecoverable = reader["GSTVATNotRecoverable"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader["GSTVATNotRecoverable"];
		}

		static TaxID GetTaxID(DbDataReader reader, string prefix = "")
		{
			TaxID result = null;
			if (reader[prefix + "AT_Code"] != DBNull.Value)
			{
				var taxRate = reader[prefix + "AT_Rate"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader[prefix + "AT_Rate"];
				result = new TaxID
				{
					TaxCode = reader[prefix + "AT_Code"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AT_Code"],
					Description = reader[prefix + "AT_Description"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AT_Description"],
					TaxRate = taxRate != null ? taxRate.Value.Normalize() : ZDecimal.Zero
				};

				var taxType = reader[prefix + "AT_Type"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AT_Type"];
				if (result.TaxRate.HasValue && !string.IsNullOrWhiteSpace(taxType))
				{
					result.TaxType = new CodeDescriptionPair { Code = taxType };
				}

				var extraTaxRateType = reader[prefix + "AT_ExtraTaxRateType"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AT_ExtraTaxRateType"];

				if (result.TaxRate.HasValue && !string.IsNullOrWhiteSpace(extraTaxRateType))
				{
					var extraTaxRateInDb = reader[prefix + "AT_ExtraRate"] == DBNull.Value ? 0 : (ZDecimal?)(decimal)reader[prefix + "AT_ExtraRate"];

					var extraTaxRate = AccTaxRate.GetExtraRate(result.TaxRate.Value, extraTaxRateType.Value, extraTaxRateInDb.Value);
					result.ExtraTaxRate = extraTaxRate.Normalize();

					result.ExtraTaxType = new CodeDescriptionPair
					{
						Code = extraTaxRateType
					};
				}
			}
			return result;
		}

		static TaxID GetWithholdingTaxID(DbDataReader reader, string prefix = "")
		{
			TaxID result = null;
			if (reader[prefix + "AW_Code"] != DBNull.Value)
			{
				result = new TaxID();
				result.TaxCode = reader[prefix + "AW_Code"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AW_Code"];
				result.Description = reader[prefix + "AW_Description"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "AW_Description"];
				result.TaxRate = reader[prefix + "AW_Rate"] == DBNull.Value ? null : (ZDecimal?)(decimal)reader[prefix + "AW_Rate"];
			}

			return result;
		}

		static TaxMessageID GetTaxMessageID(DbDataReader reader, string prefix, string countryCode = "", ICodeDescriptionBoolRelatedItemList taxGroupCodes = null)
		{
			TaxMessageID result = null;
			if (reader[prefix + "A9_Code"] != DBNull.Value)
			{
				result = new TaxMessageID();
				result.TaxMessageCode = reader[prefix + "A9_Code"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "A9_Code"];
				result.Description = reader[prefix + "A9_Description"] == DBNull.Value ? null : (ZString?)(string)reader[prefix + "A9_Description"];
				if (string.IsNullOrEmpty(prefix))
				{
					var shouldExportEnglishTaxMessage = reader["A9_IsShownOnDocuments"] != DBNull.Value && (bool)reader["A9_IsShownOnDocuments"];
					if (shouldExportEnglishTaxMessage)
					{
						result.EnglishTaxMessage = reader["A9_EnglishMsg"] == DBNull.Value ? null : (ZString?)(string)reader["A9_EnglishMsg"];
					}

					var shouldTriggerTaxExemption = reader["A9_IsTriggerExemptionMessage"] != DBNull.Value && (bool)reader["A9_IsTriggerExemptionMessage"];
					if (shouldTriggerTaxExemption)
					{
						result.VATGSTExemptionDocumentType = new CodeDescriptionPair();
						result.VATGSTExemptionDocumentType.Code = Core.Constants.RefDocTypes.VATExporterExemption;
						result.VATGSTExemptionDocumentType.Description = Core.Constants.RefDocTypeDescriptions.VATExporterExemption;
					}

					var taxGroupCode = reader["A9_TaxGroupCode"] == DBNull.Value ? ZString.Empty : (ZString?)(string)reader["A9_TaxGroupCode"];
					if (taxGroupCode.HasValue && !taxGroupCode.Value.IsEmpty)
					{
						result.TaxGroupCode = new TaxGroupCodeType();
						result.TaxGroupCode.Code = taxGroupCode;

						if (taxGroupCodes == null || taxGroupCodes.Count == 0)
						{
							taxGroupCodes = CountryComplianceFactory.GetITaxMessageGroupProvider(countryCode)?.GetTaxMessageGroup();
						}

						result.TaxGroupCode.Description = taxGroupCodes?.GetDescriptionFromCode(taxGroupCode.Value);
						result.TaxGroupCode.GovernmentCode = taxGroupCodes?.GetRelatedItemFromCode(taxGroupCode.Value);
						if (string.IsNullOrWhiteSpace(result.TaxGroupCode.GovernmentCode))
						{
							result.TaxGroupCode.GovernmentCode = null;
						}
					}
				}
			}

			return result;
		}

		Dictionary<Guid, OrganizationAddress> GetOrgAddresses(string companyCode, long batchNumber, ZString dbFunctionName, Guid? transactionHeaderPK = null)
		{
			Dictionary<Guid, OrganizationAddress> orgAddresses = new Dictionary<Guid, OrganizationAddress>();

			var sqlText = SqlTextForProcedureCall(dbFunctionName, companyCode, batchNumber, true, transactionHeaderPK);

			using (var reader = GetCommand(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					Guid headerPK = (Guid)reader["AH_PK"];
					ZString orgCode = (ZString)reader["OH_Code"].ToString().Trim();
					OrganizationAddress orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
					orgAddress.AddressType = OrgConstants.AddressType.Office;
					orgAddress.AddressOverride = ZBool.False;
					orgAddress.OrganizationCode = orgCode;
					orgAddress.Port = new UNLOCO();
					orgAddress.Port.Code = reader["RL_Code"].ToNullableZString();
					orgAddress.Port.Name = reader["RL_PortName"].ToNullableZString();
					ZString? companyNameOverride = reader["OA_CompanyNameOverride"].ToNullableZString();
					orgAddress.CompanyName = (companyNameOverride.HasValue && !companyNameOverride.Value.IsEmpty) ? companyNameOverride : reader["OH_FullName"].ToNullableZString();
					orgAddress.Country = new Country();
					orgAddress.Country.Code = reader["RN_Code"].ToNullableZString();
					orgAddress.Country.Name = reader["RN_Desc"].ToNullableZString();
					if (reader["OH_ScreeningStatus"] != DBNull.Value)
					{
						orgAddress.ScreeningStatus = new CodeDescriptionPair();
						orgAddress.ScreeningStatus.Code = reader["OH_ScreeningStatus"].ToNullableZString();
					}

					orgAddress.AddressShortCode = reader["OA_Code"].ToNullableZString();
					orgAddress.Address1 = reader["OA_Address1"].ToNullableZString();
					orgAddress.Address2 = reader["OA_Address2"].ToNullableZString();
					ZString? additionalAddressInformation = reader["OA_AdditionalAddressInformation"].ToNullableZString();
					if (additionalAddressInformation.HasValue && !additionalAddressInformation.Value.IsEmpty)
					{
						orgAddress.AdditionalAddressInformation = additionalAddressInformation;
					}

					orgAddress.City = reader["OA_City"].ToNullableZString();
					orgAddress.Postcode = reader["OA_PostCode"].ToNullableZString();
					orgAddress.State = reader["OA_State"].ToNullableZString();

					orgAddress.Contact = reader["OC_ContactName"].ToNullableZString();
					orgAddress.Email = reader["OC_Email"].ToNullableZString() ?? reader["OA_Email"].ToNullableZString();
					orgAddress.Fax = reader["OC_Fax"].ToNullableZString() ?? reader["OA_Fax"].ToNullableZString();
					orgAddress.Phone = reader["OC_Phone"].ToNullableZString() ?? reader["OA_Phone"].ToNullableZString();

					if (!orgAddresses.ContainsKey(headerPK))
					{
						orgAddresses.Add(headerPK, orgAddress);
					}
				}
			}

			foreach (var orgAddress in orgAddresses.Values.Where(o => o.ScreeningStatus != null))
			{
				orgAddress.ScreeningStatus.Description = OrgHeaderLookups.GetScreeningStatusesList(null).GetDescriptionFromCode(orgAddress.ScreeningStatus.Code);
			}

			return orgAddresses;
		}

		List<BankAccount> GetOrgARAccountDetails(string orgCode, string companyCode)
		{
			List<BankAccount> orgAccountDetails = new List<BankAccount>();

			string sqlText = @"
SELECT A1_IsDefaultAccount, A1_PaymentMethod, A1_RX_NKAccountCurrency, A1_AccountName, A1_BankName, A1_BankSwift, A1_BankBsb, A1_BankAccount, A1_RN_NKCountryCode, RN_Desc, A1_IBANNumber
	FROM dbo.AccAPAccountDetails 
	INNER JOIN dbo.OrgCompanyData ON A1_OB = OB_PK
	INNER JOIN dbo.OrgHeader ON OB_OH = OH_PK
	INNER JOIN dbo.GlbCompany ON  OB_GC = GC_PK
	LEFT JOIN dbo.RefCountry ON RN_Code = A1_RN_NKCountryCode
	WHERE OH_Code  = @OrgCode
	AND GC_Code = @CompanyCode
	AND A1_PaymentMethod IN ('TAX', 'CRQ')";

			var paramOrgCode = GetSqlParameterBasedOnDbColumn("@OrgCode", orgCode, OrgHeaderSchema.OH_Code);
			var paramCompanyCode = GetCompanyCodeSqlParameter("@CompanyCode", companyCode);

			using (var reader = GetCommand(sqlText, new [] { paramOrgCode, paramCompanyCode }).ExecuteReader())
			{
				while (reader.Read())
				{
					BankAccount accountDetail = new BankAccount();
					accountDetail.IsDefaultAccount = reader["A1_IsDefaultAccount"].ToNullableZBool();
					accountDetail.Method = reader["A1_PaymentMethod"].ToNullableZString();
					accountDetail.Currency = reader["A1_RX_NKAccountCurrency"].ToNullableZString();
					accountDetail.AccountName = reader["A1_AccountName"].ToNullableZString();
					accountDetail.BankName = reader["A1_BankName"].ToNullableZString();
					accountDetail.BankSwift = reader["A1_BankSwift"].ToNullableZString();
					accountDetail.BankBranch = reader["A1_BankBsb"].ToNullableZString();
					accountDetail.AccountNumber = reader["A1_BankAccount"].ToNullableZString();
					var countryName = reader["RN_Desc"].ToNullableZString();
					if (countryName.HasValue)
					{
						accountDetail.Country = new Country() { Code = reader["A1_RN_NKCountryCode"].ToNullableZString(), Name = countryName.Value };
					}
					accountDetail.IBANNumber = reader["A1_IBANNumber"].ToNullableZString();
					orgAccountDetails.Add(accountDetail);
				}
			}

			return orgAccountDetails;
		}

		public GLAccount GetCFXAccount(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			GLAccount result = null;

			string sqlText = string.Format("EXEC GetCFXAccount '{0}', '{1}', '{2}'", companyPK, branchPK, departmentPK);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					result = new GLAccount();
					result.AccountCode = reader["AG_AccountNum"] == DBNull.Value ? ZString.Empty : (ZString?)(string)reader["AG_AccountNum"];
					result.Description = reader["AG_Description"] == DBNull.Value ? ZString.Empty : (ZString?)(string)reader["AG_Description"];
				}
			}

			return result;
		}

		const string MatchStatus = "MatchStatus";
		const string MatchStatusReason = "MatchStatusReason";

		Dictionary<string, string> GetMatchStatus(string companyCode)
		{
			var result = GetMatchStatusCore(MatchStatus, companyCode);
			result.Add("UAC", ResString.GetMultilingualString("542977D7-521B-4D3C-98B6-BCD2F56D0BF5", "Unallocated"));
			return result;
		}

		Dictionary<string, string> GetMatchStatusReason(string companyCode)
		{
			var result = GetMatchStatusCore(MatchStatusReason, companyCode);
			result.Add("ADV", ResString.GetMultilingualString("57735CB8-6530-4AFF-A3D1-6649AF08F72B", "Receipt/Payment in advance"));
			return result;
		}

		Dictionary<string, string> GetMatchStatusCore(string name, string companyCode)
		{
			var sqlText = @"DECLARE @CompanyPK UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany WHERE GC_Code = @CompanyCode);
IF EXISTS (SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner = @CompanyPK)
	BEGIN
		SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner = @CompanyPK
	END
ELSE
	BEGIN
		SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner IS NULL
	END";

			var pairs = new Dictionary<string, string>();
			using (var cmd = GetCommand(sqlText, new[]
				{
					GetSdNameSqlParameter("@SD_Name", name),
					GetCompanyCodeSqlParameter("@CompanyCode", companyCode)
				}))
			{
				var binaryVal = cmd.ExecuteScalar() as byte[];
				if (binaryVal != null)
				{
					using (var readStream = new MemoryStream(binaryVal))
					{
						var document = new XmlDocument();
						document.Load(readStream);

						var nodes = document.SelectNodes("SystemDefinableCodeDescriptionBoolCollection/SystemDefinableCodeDescriptionBool");

						foreach (XmlNode node in nodes)
						{
							pairs.Add(node["Code"].InnerText, node["Description"].InnerText);
						}
					}
				}
			}
			return pairs;
		}

		Dictionary<Guid, InvoiceRemittance> GetInvoiceRemittances(string companyCode, Guid[] headerPKs)
		{
			var transactionHeaderReferences = new Dictionary<Guid, InvoiceRemittance>();
			if (headerPKs.Length == 0)
			{
				return transactionHeaderReferences;
			}

			var remittanceConfiguration = GetRemittanceConfigurationFromStmData(companyCode);

			string sqlText = @"SELECT AH_Ledger, AH_InvoicePaymentReferenceCode, AH1_AH, AH1_Type, AH1_Reference, OB_ARClientNumber
								FROM dbo.AccTransactionHeader
								INNER JOIN dbo.AccTransactionHeaderReference ON AH_PK = AH1_AH
								LEFT JOIN dbo.OrgCompanyData ON AH_OH = OB_OH AND AH_GC = OB_GC
								WHERE AH1_AH IN (SELECT value FROM @pks)";

			using (var command = GetCommand(sqlText))
			{
				command.AddTableValuedParameter((NoResString)"@pks", "dbo.TVP_uniqueidentifier", headerPKs);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var headerPK = (Guid)reader["AH1_AH"];
						var ledger = reader["AH_Ledger"].ToString();
						var reference = reader["AH1_Reference"].ToString();
						var referenceType = reader["AH1_Type"].ToString();// Direct access to database required
						if (ledger == LedgerTypes.AccountsReceivable)
						{
							var code = reader["AH_InvoicePaymentReferenceCode"].ToString();
							if (!string.IsNullOrEmpty(code) && remittanceConfiguration.TryGetValue(code, out InvoiceRemittance configuration))
							{
								if (!transactionHeaderReferences.TryGetValue(headerPK, out InvoiceRemittance remittance))
								{
									remittance = new InvoiceRemittance();
									transactionHeaderReferences.Add(headerPK, remittance);

									remittance.Type = configuration.Type;
									remittance.BillerCode = configuration.BillerCode;
									remittance.BillerAccountNumber = configuration.BillerAccountNumber;
									remittance.Message = configuration.Message;
								}

								remittance.DebtorClientNumber = reader["OB_ARClientNumber"].ToNullableZString();

								if (referenceType == "ITR")
								{
									remittance.InvoiceTransactionReference = reference;
								}
								else if (referenceType == "IRR")
								{
									remittance.InvoiceRemittanceReference = reference;
								}
							}
						}
						else if (ledger == LedgerTypes.AccountsPayable && !string.IsNullOrEmpty(reference)
							&& referenceType == "IRR"
							&& !transactionHeaderReferences.ContainsKey(headerPK))
						{
							var remittance = new InvoiceRemittance();
							remittance.InvoiceRemittanceReference = reference;
							transactionHeaderReferences.Add(headerPK, remittance);
						}
					}
				}
			}

			return transactionHeaderReferences;
		}

		const string InvoiceRemittanceConfiguration = "InvoiceRemittanceConfiguration";

		Dictionary<string, InvoiceRemittance> GetRemittanceConfigurationFromStmData(string companyCode)
		{
			var dictCodeDescription = new Dictionary<string, InvoiceRemittance>();
			var sqlText = @"DECLARE @CompanyPK uniqueidentifier = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany WHERE GC_Code = @CompanyCode);
SELECT TOP 1 SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner = @CompanyPK;";

			using (var cmd = GetCommand(sqlText, new[]
				{
					GetSdNameSqlParameter("@SD_Name", InvoiceRemittanceConfiguration),
					GetCompanyCodeSqlParameter("@CompanyCode", companyCode)
				}))
			{
				var binaryVal = cmd.ExecuteScalar() as byte[];
				if (binaryVal != null)
				{
					using (var readStream = new MemoryStream(binaryVal))
					{
						var document = new XmlDocument();
						document.Load(readStream);

						var nodes = document.SelectNodes("ArrayOfInvoiceRemittanceConfiguration/InvoiceRemittanceConfiguration");

						foreach (XmlNode node in nodes)
						{
							var invoiceRemittance = new InvoiceRemittance();
							invoiceRemittance.Type = new CodeDescriptionPair { Code = node["Code"].InnerText, Description = node["Description"].InnerText };
							invoiceRemittance.BillerCode = node["BillerCode"].InnerText;
							invoiceRemittance.BillerAccountNumber = node["BillerAccountNumber"].InnerText;
							invoiceRemittance.Message = node["Message"].InnerText;
							dictCodeDescription.Add(node["Code"].InnerText, invoiceRemittance);
						}
					}
				}
			}
			return dictCodeDescription;
		}

		void PopulateSubAccountCollection(Dictionary<Guid, TransactionLineRow> lines)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	AL1_AL,
	SubAccountType = subAccount.SubAccountType,
	SubAccountTypeDescription = subAccount.SubAccountFullTypeName,
	SubAccountCode = subAccount.SubAccountCode
FROM dbo.AccTransactionLineSubAccount
CROSS APPLY
	GetSubAccountDetails(AL1_SubClassParentTableCode, AL1_SubClassParentId) AS subAccount
WHERE AL1_AL IN (SELECT Value FROM @LinePKS)
");
			using (var command = GetCommand(sqlText))
			{
				command.AddTableValuedParameter("@LinePKS", "dbo.TVP_uniqueidentifier", lines.Keys);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var subAccountRowValue = reader["SubAccountCode"];
						var subAccountTypeRowValue = reader["SubAccountType"];
						var subAccountDescriptionRowValue = reader["SubAccountTypeDescription"];
						if (subAccountRowValue != DBNull.Value && subAccountTypeRowValue != DBNull.Value)
						{
							var subAccountString = (ZString)subAccountRowValue.ToString();
							var subAccountTypeString = (ZString)subAccountTypeRowValue.ToString();
							if (!subAccountString.IsEmpty && !subAccountTypeString.IsEmpty)
							{
								var linePK = reader["AL1_AL"] == DBNull.Value ? Guid.Empty : (Guid)reader["AL1_AL"];

								var subAccount = new SubAccount();
								subAccount.Code = subAccountString.Trim();

								subAccount.Type = new CodeDescriptionPair();
								subAccount.Type.Code = subAccountTypeString.Trim();
								subAccount.Type.Description = subAccountDescriptionRowValue == DBNull.Value ? null : (ZString?)subAccountDescriptionRowValue.ToString().Trim();
								lines[linePK].SubAccountCollection.Add(subAccount);
							}
						}
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateRatingBasisCollection(Dictionary<Guid, TransactionLineRow> lines, Dictionary<Guid, (Guid jobChargePK, Guid consolCostPK)> lineRelatedPKs)
		{
			if (lineRelatedPKs.Count == 0)
			{
				return;
			}

			var jobChargePKs = new HashSet<Guid>();
			var consolCostPKs = new HashSet<Guid>();
			foreach (var relatedPK in lineRelatedPKs.Values)
			{
				if (relatedPK.jobChargePK != Guid.Empty)
				{
					jobChargePKs.Add(relatedPK.jobChargePK);
				}
				else if (relatedPK.consolCostPK != Guid.Empty)
				{
					consolCostPKs.Add(relatedPK.consolCostPK);
				}
			}
			var isJobChargePKEmpty = jobChargePKs.Count == 0;
			var isConsolCostPKEmpty = consolCostPKs.Count == 0;

			if (isJobChargePKEmpty && isConsolCostPKEmpty)
			{
				return;
			}

			var paymentBasisCollection = new Dictionary<(Guid jobChargePK, Guid consolCostPK), List<GenericJobPaymentBasis>>();
			var sqlText = string.Format(CultureInfo.InvariantCulture,
	@"SELECT JobChargePK, ConsolCostPK,
PBS_IsCost,
PBS_AdapterID, PBS_AdapterType,
PBS_ChargeableDescription,
PBS_MinRate, PBS_MaxRate,
PBS_FlatRate, PBS_PerUnitRate,
PBS_ChargeableAmount, PBS_ChargeableUnit, PBS_ChargeableUnitType,
PBS_RateUnit, PBS_RateUnitType,
PBS_RX_NKRateCurrency, RX_Desc
FROM 
	dbo.GetJobPaymentBasis(@JobChargePKs, @ConsolCostPKs, @IsJobChargePKEmpty, @IsConsolCostPKEmpty)
OPTION(RECOMPILE)");
			using (var command = GetCommand(sqlText))
			{
				command.AddTableValuedParameter("@JobChargePKs", "dbo.TVP_uniqueidentifier", jobChargePKs);
				command.AddTableValuedParameter("@ConsolCostPKs", "dbo.TVP_uniqueidentifier", consolCostPKs);
				command.AddParameterWithValue("@IsJobChargePKEmpty", isJobChargePKEmpty);
				command.AddParameterWithValue("@IsConsolCostPKEmpty", isConsolCostPKEmpty);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var jobChargePK = reader["JobChargePK"] == DBNull.Value ? Guid.Empty : (Guid)reader["JobChargePK"];
						var consolCostPK = reader["ConsolCostPK"] == DBNull.Value ? Guid.Empty : (Guid)reader["ConsolCostPK"];
						var genericPaymentBasis = new GenericJobPaymentBasis();
						genericPaymentBasis.JobChargePK = jobChargePK;
						genericPaymentBasis.ConsolCostPK = consolCostPK;
						genericPaymentBasis.IsCost = reader["PBS_IsCost"] != DBNull.Value && (bool)reader["PBS_IsCost"];
						genericPaymentBasis.AdapterID = reader["PBS_AdapterID"] == DBNull.Value ? string.Empty : (string)reader["PBS_AdapterID"];
						genericPaymentBasis.AdapterType = reader["PBS_AdapterType"] == DBNull.Value ? string.Empty : (string)reader["PBS_AdapterType"];
						genericPaymentBasis.ChargeableDescription = reader["PBS_ChargeableDescription"] == DBNull.Value ? string.Empty : (string)reader["PBS_ChargeableDescription"];
						genericPaymentBasis.MinRate = reader["PBS_MinRate"] == DBNull.Value ? decimal.Zero : (decimal)reader["PBS_MinRate"];
						genericPaymentBasis.MaxRate = reader["PBS_MaxRate"] == DBNull.Value ? decimal.Zero : (decimal)reader["PBS_MaxRate"];
						genericPaymentBasis.FlatRate = reader["PBS_FlatRate"] == DBNull.Value ? decimal.Zero : (decimal)reader["PBS_FlatRate"];
						genericPaymentBasis.PerUnitRate = reader["PBS_PerUnitRate"] == DBNull.Value ? decimal.Zero : (decimal)reader["PBS_PerUnitRate"];
						genericPaymentBasis.ChargeableAmount = reader["PBS_ChargeableAmount"] == DBNull.Value ? decimal.Zero : (decimal)reader["PBS_ChargeableAmount"];
						genericPaymentBasis.ChargeableUnit = reader["PBS_ChargeableUnit"] == DBNull.Value ? string.Empty : (string)reader["PBS_ChargeableUnit"];
						genericPaymentBasis.ChargeableUnitType = reader["PBS_ChargeableUnitType"] == DBNull.Value ? string.Empty : (string)reader["PBS_ChargeableUnitType"];
						genericPaymentBasis.RateUnit = reader["PBS_RateUnit"] == DBNull.Value ? string.Empty : (string)reader["PBS_RateUnit"];
						genericPaymentBasis.RateUnitType = reader["PBS_RateUnitType"] == DBNull.Value ? string.Empty : (string)reader["PBS_RateUnitType"];
						genericPaymentBasis.RateCurrency = reader["PBS_RX_NKRateCurrency"] == DBNull.Value ? string.Empty : (string)reader["PBS_RX_NKRateCurrency"];
						genericPaymentBasis.RateCurrencyDescription = reader["RX_Desc"] == DBNull.Value ? string.Empty : (string)reader["RX_Desc"];
						var basisParentID = (jobChargePK, consolCostPK);
						List<GenericJobPaymentBasis> listGenericJobPaymentBasis;
						if (paymentBasisCollection.TryGetValue(basisParentID, out listGenericJobPaymentBasis))
						{
							listGenericJobPaymentBasis.Add(genericPaymentBasis);
						}
						else
						{
							paymentBasisCollection.Add(basisParentID, new List<GenericJobPaymentBasis>() { genericPaymentBasis });
						}
					}
				}
			}

			foreach (Guid linePK in lineRelatedPKs.Keys)
			{
				List<GenericJobPaymentBasis> listGenericJobPaymentBasis;
				if (paymentBasisCollection.TryGetValue(lineRelatedPKs[linePK], out listGenericJobPaymentBasis))
				{
					var lineRow = lines[linePK];
					if (lineRow.LineType.HasValue)
					{
						if (lineRow.LineType.Value == "CST" || lineRow.LineType.Value == "ACR")
						{
							listGenericJobPaymentBasis = listGenericJobPaymentBasis.Where(x => x.IsCost).ToList();
						}
						else if (lineRow.LineType.Value == "REV" || lineRow.LineType.Value == "WIP")
						{
							listGenericJobPaymentBasis = listGenericJobPaymentBasis.Where(x => !x.IsCost).ToList();
						}
					}

					var ratingBasisCollection = new List<RatingBasis>();
					foreach (var basis in listGenericJobPaymentBasis)
					{
						var universalPaymentBasis = JobPaymentBasisHelper.PopulateUniversalPaymentBases(basis);

						ratingBasisCollection.Add(universalPaymentBasis);
					}

					lineRow.RatingBasisCollection = ratingBasisCollection;
				}
			}
		}

		#region Tax Transactions

		public (Dictionary<ZGuid, TaxGLMovementRow> taxGLMovementRows, Dictionary<ZGuid, ZGuid> taxTransactionGLMovementLink) GetTaxGLMovements(ZString companyCode, long batchNumber, ZGuid? transactionHeaderPK = null)
		{
			var taxGLMovementRows = new Dictionary<ZGuid, TaxGLMovementRow>();
			var taxTransactionGLMovementLink = new Dictionary<ZGuid, ZGuid>();

			var sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetTaxGLMovements", companyCode, batchNumber, useOptimizedStoredProceduresForBatchExport: true, transactionHeaderPK);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var taxTransactionPK = (Guid)reader["ATT_PK"];

					var glMovementPK = (Guid)reader["ATM_PK"];

					var taxGLMovementRow = new TaxGLMovementRow();
					taxGLMovementRow.DebitAccountNumber = reader["DebitAccountNum"].ToNullableZString();
					taxGLMovementRow.DebitAccountDescription = reader["DebitAccountDescription"].ToNullableZString();

					taxGLMovementRow.CreditAccountNumber = reader["CreditAccountNum"].ToNullableZString();
					taxGLMovementRow.CreditAccountDescription = reader["CreditAccountDescription"].ToNullableZString();

					taxGLMovementRow.PostingAmount = reader["PostingAmount"].ToNullableZDecimal();
					taxGLMovementRow.PostingDate = reader["PostingDate"].ToNullableZDate();
					taxGLMovementRow.PostingPeriod = reader["PostingPeriod"].ToNullableZInt32();
					taxGLMovementRow.PostingCurrencyCode = reader["PostingCurrencyCode"].ToNullableZString();
					taxGLMovementRow.PostingCurrencyName = reader["PostingCurrencyName"].ToNullableZString();

					if (!taxGLMovementRows.ContainsKey(glMovementPK))
					{
						taxGLMovementRows[glMovementPK] = taxGLMovementRow;
					}

					if (!taxTransactionGLMovementLink.ContainsKey(glMovementPK))
					{
						taxTransactionGLMovementLink[glMovementPK] = taxTransactionPK;
					}
				}
			}

			return (taxGLMovementRows, taxTransactionGLMovementLink);
		}

		public Dictionary<ZGuid, TaxTransactionRow> GetTaxTransactions(IDataObjectWriterStrategy writerStrategy, ZString companyCode, long batchNumber, ZGuid? transactionHeaderPK = null)
		{
			var countryCode = GetCompanyData(companyCode).CountryCode;
			var taxAuthorities = GetTaxAuthorities(countryCode);
			var taxSystems = GetTaxSystems(countryCode);

			var taxTransactionRows = new Dictionary<ZGuid, TaxTransactionRow>();

			var sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetTaxTransactions", companyCode, batchNumber, useOptimizedStoredProceduresForBatchExport: true, transactionHeaderPK);

			int sequence = 1;
			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var taxTransactionPK = (Guid)reader["ATT_PK"];

					if (!taxTransactionRows.TryGetValue(taxTransactionPK, out var taxTransactionRow))
					{
						taxTransactionRow = PopulateTaxTransaction(reader, sequence, taxAuthorities, taxSystems, writerStrategy);
						taxTransactionRows[taxTransactionPK] = taxTransactionRow;
						sequence++;
					}
				}
			}

			return taxTransactionRows;
		}

		TaxTransactionRow PopulateTaxTransaction(DbDataReader reader, int sequence, ICodeDescriptionPairList taxAuthorities, ICodeDescriptionPairList taxSystems, IDataObjectWriterStrategy writerStrategy)
		{
			var taxTransactionRow = new TaxTransactionRow();
			taxTransactionRow.TaxTransaction = new TaxTransaction(writerStrategy);

			taxTransactionRow.TaxTransaction.Link = sequence;
			taxTransactionRow.TransactionHeaderPK = (Guid)reader["AH_PK"];

			var taxSuperTypeCode = reader["TaxSuperType"].ToNullableZString();
			taxTransactionRow.TaxTransaction.TaxSuperType = new CodeDescriptionPair();
			taxTransactionRow.TaxTransaction.TaxSuperType.Code = taxSuperTypeCode;
			taxTransactionRow.TaxTransaction.TaxSuperType.Description = new TaxSuperTypeList().GetDescriptionFromCode(taxSuperTypeCode);

			var taxSystemCode = reader["TaxSystemCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.TaxSystem = new CodeDescriptionPair10Char();
			taxTransactionRow.TaxTransaction.TaxSystem.Code = taxSystemCode;
			taxTransactionRow.TaxTransaction.TaxSystem.Description = taxSystems.GetDescriptionFromCode(taxSystemCode);

			var taxAuthorityCode = reader["TaxAuthorityCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.TaxAuthority = new CodeDescriptionPair10Char();
			taxTransactionRow.TaxTransaction.TaxAuthority.Code = taxAuthorityCode;
			taxTransactionRow.TaxTransaction.TaxAuthority.Description = taxAuthorities.GetDescriptionFromCode(taxAuthorityCode);

			taxTransactionRow.TaxTransaction.TaxConfiguration = new CodeDescriptionPair30Char();
			taxTransactionRow.TaxTransaction.TaxConfiguration.Code = reader["TaxConfigurationCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.TaxConfiguration.Description = reader["TaxConfigurationDescription"].ToNullableZString();

			taxTransactionRow.TaxTransaction.Ledger = reader["Ledger"].ToNullableZString();

			var realizationBasisCode = reader["RealizationBasis"].ToNullableZString();
			taxTransactionRow.TaxTransaction.RealizationBasis = new CodeDescriptionPair();
			taxTransactionRow.TaxTransaction.RealizationBasis.Code = realizationBasisCode;
			taxTransactionRow.TaxTransaction.RealizationBasis.Description = new TaxBasisList().GetDescriptionFromCode(realizationBasisCode);

			taxTransactionRow.TaxTransaction.Branch = new Branch();
			taxTransactionRow.TaxTransaction.Branch.Code = reader["BranchCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.Branch.Name = reader["BranchName"].ToNullableZString();

			taxTransactionRow.TaxTransaction.Department = new Department();
			taxTransactionRow.TaxTransaction.Department.Code = reader["DepartmentCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.Department.Name = reader["DepartmentName"].ToNullableZString();

			taxTransactionRow.TaxTransaction.PostDate = reader["PostDate"].ToNullableZDate();
			taxTransactionRow.TaxTransaction.TaxRealizedDate = reader["TaxRealizationDate"].ToNullableZDate();
			taxTransactionRow.TaxTransaction.TaxDate = reader["TaxDate"].ToNullableZDate();

			var serviceCode = reader["ServiceCode"].ToNullableZString();
			if (serviceCode.HasValue && !serviceCode.Value.IsEmpty)
			{
				taxTransactionRow.TaxTransaction.ServiceCode = new CodeDescriptionPair20Char();
				taxTransactionRow.TaxTransaction.ServiceCode.Code = serviceCode;
				taxTransactionRow.TaxTransaction.ServiceCode.Description = reader["ServiceCodeDescription"].ToNullableZString();
			}

			taxTransactionRow.TaxTransaction.TaxID = GetTaxID(reader, "");
			taxTransactionRow.TaxTransaction.TaxMessageID = GetTaxMessageID(reader, "");

			taxTransactionRow.TaxTransaction.OSCurrency = new Currency();
			taxTransactionRow.TaxTransaction.OSCurrency.Code = reader["CurrencyCode"].ToNullableZString();
			taxTransactionRow.TaxTransaction.OSCurrency.Description = reader["CurrencyDescription"].ToNullableZString();

			taxTransactionRow.TaxTransaction.OSTaxAmount = reader["OSTaxAmount"].ToNullableZDecimal();
			taxTransactionRow.TaxTransaction.LocalTaxAmount = reader["LocalTaxAmount"].ToNullableZDecimal();
			taxTransactionRow.TaxTransaction.OSTaxBase = reader["OSTaxBaseAmount"].ToNullableZDecimal();
			taxTransactionRow.TaxTransaction.LocalTaxBase = reader["LocalTaxBaseAmount"].ToNullableZDecimal();

			taxTransactionRow.TaxTransaction.IsCancelled = reader["IsCancelled"].ToNullableZBool();
			taxTransactionRow.TaxTransaction.IncludedInTransactionTotal = reader["IncludedInTransactionTotal"].ToNullableZBool();

			var matchingJournalPK = reader["MatchingTransactionPK"].ToNullableZGuid();
			if (matchingJournalPK != null)
			{
				taxTransactionRow.TaxTransaction.SetRealizationJournalReferenceCollection(() =>
				{
					var realizationJournalReferenceCollection = new List<RealizationJournalReference>();

					var realizationJournal = new RealizationJournalReference();
					realizationJournal.Ledger = reader["MatchingTransactionLedger"].ToNullableZString();
					var ah_TransactionType = reader["MatchingTransactionType"].ToNullableZString();
					realizationJournal.TransactionType = ah_TransactionType.HasValue ? new TransactionTypeConverter().ToEnumValue(ah_TransactionType.Value) : null;
					realizationJournal.TransactionNumber = reader["MatchingTransactionNumber"].ToNullableZString();
					realizationJournal.Category = reader["MatchingTransactionCategory"].ToNullableZString();
					realizationJournal.Description = reader["MatchingTransactionDescription"].ToNullableZString();

					realizationJournalReferenceCollection.Add(realizationJournal);

					return realizationJournalReferenceCollection;
				});
			}

			return taxTransactionRow;
		}

		public (Dictionary<ZGuid, List<TaxLink>>, Dictionary<ZGuid, List<ZGuid>>) GetTaxTransactionLinks(ZString companyCode, long batchNumber, Dictionary<ZGuid, TaxTransactionRow> taxTransactionRows, ZGuid? transactionHeaderPK = null)
		{
			var taxLinksAgainstLinePK = new Dictionary<ZGuid, List<TaxLink>>();
			var linePksAgainstTaxPK = new Dictionary<ZGuid, List<ZGuid>>();

			var sqlText = SqlTextForProcedureCall("AccountingTransactionExportGetTaxTransactionLinks", companyCode, batchNumber, useOptimizedStoredProceduresForBatchExport: true, transactionHeaderPK);

			using (var command = GetCommand(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var taxTransactionPK = (Guid)reader["ATP_ATT"];
					var linePK = (Guid)reader["ATP_AL_TransactionLine"];

					var taxTransactionLink = new TaxLink();
					var taxTransactionRow = taxTransactionRows[taxTransactionPK];
					taxTransactionLink.TaxTransactionLink = taxTransactionRow.TaxTransaction.Link;
					taxTransactionLink.LocalTaxAmount = reader["ATP_LocalTaxAmount"].ToNullableZDecimal();

					if (!taxLinksAgainstLinePK.ContainsKey(linePK))
					{
						taxLinksAgainstLinePK.Add(linePK, new List<TaxLink>());
					}

					taxLinksAgainstLinePK[linePK].Add(taxTransactionLink);

					if (!linePksAgainstTaxPK.ContainsKey(taxTransactionPK))
					{
						linePksAgainstTaxPK.Add(taxTransactionPK, new List<ZGuid>());
					}
					linePksAgainstTaxPK[taxTransactionPK].Add(linePK);
				}
			}

			foreach (var list in taxLinksAgainstLinePK)
			{
				list.Value.Sort((x, y) => x.TaxTransactionLink.Value.CompareTo(y.TaxTransactionLink.Value));
			}

			return (taxLinksAgainstLinePK, linePksAgainstTaxPK);
		}
		#endregion

		static CodeDescriptionPair GetCancelReason(ZString ah_ReceiptType, ICodeDescriptionPairList reversalCodeList, ICodeDescriptionPairList creditNoteCodeList)
		{
			var reversalDescription = reversalCodeList.GetDescriptionFromCode(ah_ReceiptType);
			if (!string.IsNullOrEmpty(reversalDescription))
			{
				return new CodeDescriptionPair() { Code = ah_ReceiptType, Description = reversalDescription };
			}

			var creditNoteDescription = creditNoteCodeList.GetDescriptionFromCode(ah_ReceiptType);
			if (!string.IsNullOrEmpty(creditNoteDescription))
			{
				return new CodeDescriptionPair() { Code = ah_ReceiptType, Description = creditNoteDescription };
			}

			return new CodeDescriptionPair() { Code = ah_ReceiptType };
		}

		List<AuthorizationDetails> LoadAuthorizationDetails(Guid? transactionPk, bool includeGovernmentBatchReference)
		{
			if (transactionPk == null)
			{
				// Not implemented for universal transaction batch.
				return null;
			}

			var sql = GetAuthorizationDetailsQuery();
			var resultAndRecordType = new List<(AuthorizationDetails details, ZString? recordType)>();
			using (var command = GetCommand(sql))
			{
				command.AddParameterWithValue("@PK", transactionPk.Value);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var recordType = reader[AHFSchema.AHF_RecordType].ToNullableZString();
						var details = new AuthorizationDetails
						{
							Purpose = new CodeDescriptionPair4Char()
							{
								Code = DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode,
								Description = DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleDescription,
							},

							GovernmentNumber = reader[AHFSchema.AHF_Number].ToNullableZString().ToNullZStringIfEmpty(),
							GovernmentCounter = reader[AHFSchema.AHF_Counter].ToNullableZString().ToNullZStringIfEmpty(),
							Date = reader[AHFSchema.AHF_DateTime].ToNullableZDateTimeOffset(),
							URL = reader[AHFSchema.AHF_VerificationUrl].ToNullableZString().ToNullZStringIfEmpty(),
							PublicKey = ZCompressor.GetUncompressedVersion(reader[AHFSchema.AHF_PublicKey], AHFSchema.AHF_PublicKey).ToNullableStream(),
							SharedSpecialData = ZCompressor.GetUncompressedVersion(reader[AHFSchema.AHF_AuthorisationData], AHFSchema.AHF_AuthorisationData).ToNullableStream(),
							SharedTransactionHash = ZCompressor.GetUncompressedVersion(reader[AHFSchema.AHF_ITransactionHash], AHFSchema.AHF_ITransactionHash).ToNullableStream(),
							DebtorRegistrationNumber = reader[AHFSchema.AHF_DebtorNumber].ToNullableZString().ToNullZStringIfEmpty(),
							IssuerCertificateID = reader[AHFSchema.AHF_IssuerCertificateIdentifier].ToNullableZString().ToNullZStringIfEmpty(),
							PlaceOfIssue = reader[AHFSchema.AHF_PlaceOfIssue].ToNullableZString().ToNullZStringIfEmpty(),
							IssuerSpecialData = ZCompressor.GetUncompressedVersion(reader[AHFSchema.AHF_IssuerAuthorizationData], AHFSchema.AHF_IssuerAuthorizationData).ToNullableStream(),
							GovernmentBatchReference = reader[AIBSchema.AIB_GovernmentAllocatedNumber].ToNullableZString().ToNullZStringIfEmpty()
						};

						if (details.GovernmentNumber.GetValueOrDefault() == DataTransferConstants.AccTransactionHeaderAuthorisationRecord.NullPlaceholderForNVarchar)
						{
							details.GovernmentNumber = null;
						}
						if (details.GovernmentCounter.GetValueOrDefault() == DataTransferConstants.AccTransactionHeaderAuthorisationRecord.NullPlaceholderForNVarchar)
						{
							details.GovernmentCounter = null;
						}
						if (details.Date.GetValueOrDefault() <= DataTransferConstants.AccTransactionHeaderAuthorisationRecord.NullPlaceholderForDateTime)
						{
							details.Date = null;
						}

						if (!AllFieldsNull(details))
						{
							resultAndRecordType.Add((details, recordType));
						}
					}
				}
			}

			var dataHelper = new AuthorizationDataHelper(this);
			foreach (var x in resultAndRecordType)
			{
				x.details.Country = dataHelper.LoadCountryOrNull(x.recordType, transactionPk.Value);  // Must be done after previous DataReader is closed, as this does a DB query.
				x.details.Version = dataHelper.GetVersion(x.recordType);
			}

			return resultAndRecordType.Any() ? resultAndRecordType.ConvertAll(x => x.details) : null;

			bool AllFieldsNull(AuthorizationDetails details)
				=> details.GovernmentNumber == (ZString?)null
				&& details.GovernmentCounter == (ZString?)null
				&& details.Date == null
				&& details.URL == (ZString?)null
				&& details.PublicKey == null
				&& details.SharedSpecialData == null
				&& details.SharedTransactionHash == null
				&& details.DebtorRegistrationNumber == (ZString?)null
				&& details.IssuerCertificateID == (ZString?)null
				&& details.PlaceOfIssue == (ZString?)null
				&& details.IssuerSpecialData == null
				&& details.GovernmentBatchReference == (ZString?)null;

			string GetAuthorizationDetailsQuery()
			{
				var includeGovernmentBatchReferenceSQLQuery = @$"
MostRecentPivot AS
(
	SELECT TOP 1 {AIPSchema.AIP_AIB},
		{AIPSchema.AIP_ParentID} 
	FROM dbo.AccEInvoicingTransactionPivot
	WHERE AIP_ActionType in ({string.Join(",", EInvoicingPivotActionType.CommandActionTypes.Select(item => $"'{item}'"))})
	  AND AIP_ParentID = @PK
	ORDER BY 
		CASE
			WHEN AIP_Status != '{EInvoicingBatchState.Discarded}' THEN 0
			ELSE 1 
		END,
		CASE
			WHEN AIP_LastSentTimeUtc IS NULL THEN AIP_SystemCreateTimeUtc
			ELSE AIP_LastSentTimeUtc
		END DESC
),
EInvoicingBatch AS
(
	SELECT {AIBSchema.AIB_GovernmentAllocatedNumber},
		{AIPSchema.AIP_ParentID} 
	FROM MostRecentPivot JOIN dbo.AccEInvoicingBatch
	ON AIP_AIB = AIB_PK
)
";
				var notIncludeGovernmentBatchReferenceSQLQuery = @$"
EInvoicingBatch AS
(
	SELECT {AIBSchema.AIB_GovernmentAllocatedNumber} = null, {AIPSchema.AIP_ParentID} = CONVERT(uniqueidentifier,@PK)
)
";
				var intermediateSQLquery = includeGovernmentBatchReference ? includeGovernmentBatchReferenceSQLQuery : notIncludeGovernmentBatchReferenceSQLQuery;
				var sql =
$@"WITH AuthorisationRecord AS
(
	SELECT {AHFSchema.AHF_RecordType},
		{AHFSchema.AHF_Number},
		{AHFSchema.AHF_Counter},
		{AHFSchema.AHF_DateTime},
		{AHFSchema.AHF_VerificationUrl},
		{AHFSchema.AHF_PublicKey},
		{AHFSchema.AHF_AuthorisationData},
		{AHFSchema.AHF_ITransactionHash},
		{AHFSchema.AHF_DebtorNumber},
		{AHFSchema.AHF_IssuerCertificateIdentifier},
		{AHFSchema.AHF_PlaceOfIssue},
		{AHFSchema.AHF_IssuerAuthorizationData},
		{AHFSchema.AHF_ParentId}
	FROM 
		dbo.AccTransactionHeaderAuthorisationRecord
	WHERE 
		AHF_ParentId = @PK
		AND AHF_ParentTableCode = 'AH'
),

{intermediateSQLquery}

SELECT 
	AHF_RecordType,
	AHF_Number,
	AHF_Counter,
	AHF_DateTime,
	AHF_VerificationUrl,
	AHF_PublicKey,
	AHF_AuthorisationData,
	AHF_ITransactionHash,
	AHF_DebtorNumber,
	AHF_IssuerCertificateIdentifier,
	AHF_PlaceOfIssue,
	AHF_IssuerAuthorizationData,
	AIB_GovernmentAllocatedNumber
FROM
	AuthorisationRecord
	FULL JOIN EInvoicingBatch on AIP_ParentID = AHF_ParentId";
				return sql;
			}
		}

		BankAccount GetSellersBankAccount(ZGuid? organisation, ZString currency, ZGuid? branch, BusinessObjectFactory factory)
		{
			var glbBranch = factory.Load<GlbBranch>(branch.GetValueOrDefault());

			var accBankAccount = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organisation.GetValueOrDefault(), currency, glbBranch, factory);
			if (accBankAccount != null)
			{
				var bankAccount = new BankAccount();
				bankAccount.Currency = accBankAccount.AB_RX_NKAccountCurrency;
				bankAccount.AccountName = accBankAccount.AB_BankAccountName;
				bankAccount.BankName = accBankAccount.AB_BankName;
				bankAccount.BankSwift = accBankAccount.AB_SWIFT;
				bankAccount.BankBranch = accBankAccount.AB_BSB;
				bankAccount.AccountNumber = accBankAccount.AB_AccountNum;
				bankAccount.IBANNumber = accBankAccount.IBAN;
				bankAccount.BankAccountCode = accBankAccount.AB_Code;
				bankAccount.BankAccountDescription = accBankAccount.AB_Desc;
				bankAccount.BankAddress = accBankAccount.AB_BankAddress;
				bankAccount.BankUniqueAccNo = accBankAccount.AB_FullAccountNumber;
				bankAccount.Country = Country.New(accBankAccount.BankAccountCountry);

				return bankAccount;
			}

			return null;
		}

		List<TransactionHeaderReference> LoadTransactionHeaderReference(Guid? transactionPk, string companyCode)
		{
			if (transactionPk == null)
			{
				// Not applicable for universal transaction batch.
				return null;
			}

			var transactionHeaderReferences = new List<TransactionHeaderReference>();
			var transactionHeaderReferenceTypes = new AccTransactionHeaderReferenceTypesList();
			var eInvoicingReversalCodesList = GetEInvoicingReversalCodesList(companyCode);

			var sqlText = @"
	SELECT AH1_Type, AH1_Reference
	FROM dbo.AccTransactionHeaderReference
	WHERE AH1_AH = @transactionPk AND AH1_Reference <> ''";

			using (var command = GetCommand(sqlText))
			{
				command.AddParameterWithValue("@transactionPk", transactionPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var type = reader["AH1_Type"].ToString();
						var reference = reader["AH1_Reference"].ToString();

						var transactionHeaderReference = new TransactionHeaderReference
						{
							Type = type,
							Reference = reference,
							TypeDescription = transactionHeaderReferenceTypes.GetDescriptionFromCode(type),
							ReferenceDescription = type == AccTransactionHeaderReferenceTypesList.EINV_REVERSAL_CODE.Code ? eInvoicingReversalCodesList?.GetDescriptionFromCode(reference) : null,
						};

						transactionHeaderReferences.Add(transactionHeaderReference);
					}
				}
			}

			return transactionHeaderReferences.Any() ? transactionHeaderReferences : null;
		}

		#region SuppressResourceStringsCheckRegion
		// Reason: Direct access to database required. All strings are part of SQL queries

		static string SqlTextForProcedureCall(string baseProcedureName, ZString companyCode, long batchNumber, bool useOptimizedStoredProceduresForBatchExport, ZGuid? transactionHeaderPK)
			=> SqlTextForProcedureCall(baseProcedureName, (string)companyCode, batchNumber, useOptimizedStoredProceduresForBatchExport, transactionHeaderPK.HasValue ? transactionHeaderPK.Value.ToGuid() : null);

		static string SqlTextForProcedureCall(string baseProcedureName, string companyCode, long batchNumber, bool useOptimizedStoredProceduresForBatchExport, Guid? transactionHeaderPK)
		{
			if (useOptimizedStoredProceduresForBatchExport)
			{
				return transactionHeaderPK.HasValue
					? FormattableString.Invariant($"EXEC {baseProcedureName}Single '{transactionHeaderPK}'")
					: FormattableString.Invariant($"EXEC {baseProcedureName} '{companyCode}', {batchNumber}");
			}
			else
			{
				var transactionHeaderValue = transactionHeaderPK.HasValue
					? $"'{transactionHeaderPK}'"
					: "NULL";

				return FormattableString.Invariant($"EXEC {baseProcedureName} '{companyCode}', {batchNumber}, {transactionHeaderValue}");
			}
		}

		static string SqlTextForProcedureCallWithDate(string baseProcedureName, string companyCode, long batchNumber, ZDateTime today, bool useOptimizedStoredProceduresForBatchExport, Guid? transactionHeaderPK)
		{
			if (useOptimizedStoredProceduresForBatchExport && transactionHeaderPK.HasValue)
			{
				return FormattableString.Invariant($"EXEC {baseProcedureName}Single '{transactionHeaderPK}', '{today.SqlFormat}'");
			}
			else if (transactionHeaderPK.HasValue)
			{
				return FormattableString.Invariant($"EXEC {baseProcedureName} '{companyCode}', {batchNumber}, '{today.SqlFormat}', '{transactionHeaderPK}'");
			}
			else
			{
				return FormattableString.Invariant($"EXEC {baseProcedureName} '{companyCode}', {batchNumber}, '{today.SqlFormat}', NULL");
			}
		}

		#endregion
	}

	static class ObjectValueExtensions
	{
		internal static bool IsDBNull(this object value)
		{
			return value == DBNull.Value;
		}

		internal static ZGuid? ToNullableZGuid(this object value)
		{
			return IsDBNull(value) ? null : (ZGuid?)((Guid)value);
		}

		internal static ZString? ToNullableZString(this object value)
		{
			return IsDBNull(value) ? null : (ZString?)((string)value).Trim();
		}

		internal static ZString? ToNullZStringIfEmpty(this ZString? value)
		{
			return value.GetValueOrDefault().IsEmpty ? null : value;
		}

		internal static ZDateTime? ToNullableZDateTime(this object value)
		{
			return IsDBNull(value) ? null : (ZDateTime?)(DateTime)value;
		}

		internal static ZDate? ToNullableZDate(this object value)
		{
			return IsDBNull(value) ? null : (ZDate?)(DateTime)value;
		}

		internal static ZDateTimeOffset? ToNullableZDateTimeOffset(this object value)
		{
			return IsDBNull(value) ? null : (ZDateTimeOffset?)(DateTimeOffset)value;
		}

		internal static ZInt? ToNullableZInt(this object value)
		{
			return IsDBNull(value) ? null : (ZInt?)(byte)value;
		}

		internal static ZInt? ToNullableZInt32(this object value)
		{
			return IsDBNull(value) ? null : (ZInt?)(int)value;
		}

		internal static ZBool? ToNullableZBool(this object value)
		{
			return IsDBNull(value) ? null : new ZBool(value);
		}

		internal static ZDecimal? ToNullableZDecimal(this object value)
		{
			return IsDBNull(value) ? null : (ZDecimal?)(decimal)value;
		}

		internal static CargoWise.IO.SubStreamableStream ToNullableStream(this object value)
		{
			return IsDBNull(value) || ((byte[])value).Length == 0
				? null
				: (CargoWise.IO.SubStreamableStream)new MemoryStream((byte[])value);
		}
	}
}
