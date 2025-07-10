using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public partial class AccComplianceReport
	{
		internal IReadOnlyList<DataColumn> GetReportLineColumns()
		{
			var result = new List<DataColumn>();
			var namesToExclude = GetColumnNamesToExclude();

			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.PK, typeof(Guid), namesToExclude); // Required for a NonPersistentBusinessObject
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.ACL_ReportSequence, typeof(int), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.OH_Code, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.OH_FullName, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.OrgCountryCode, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.GC_RN_NKCountryCode, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.OK_CustomsRegNo, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.RepCountryRegNo, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_PK, typeof(Guid), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_Ledger, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_TransactionType, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.PostDate, typeof(DateTime), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.InvoiceDate, typeof(DateTime), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_TransactionNum, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_TransactionReference, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_ComplianceSubType, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_InvoiceAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AH_GSTAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.SPVTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.ReportSubCode, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AG_AccountNum, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AG_Description, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AT_Type, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AT_Code, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AT_ExtraTaxRateType, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.TaxMessage, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AL_A9_VATClass, typeof(Guid), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.GLAccountPK, typeof(Guid), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.AL_TaxRate, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.ComplianceSequence, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLine.Schema.TaxGroupCode, typeof(string), namesToExclude);
			// Shared with AccComplianceReportLineBase
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GoodsExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GoodsTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.ServiceExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.ServiceTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TotalExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TotalTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxRecoverableAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxReverseChargeAmount, typeof(decimal), namesToExclude);

			return result.AsReadOnly();
		}

		internal IReadOnlyList<DataColumn> GetReportTotalLineColumns()
		{
			return GetReportLineBaseColumns();
		}

		IReadOnlyList<DataColumn> GetReportLineBaseColumns()
		{
			var result = new List<DataColumn>();
			var namesToExclude = GetColumnNamesToExclude();

			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.PK, typeof(Guid), namesToExclude); // Required for a NonPersistentBusinessObject
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GoodsExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GoodsTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.ServiceExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.ServiceTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TotalExTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TotalTaxAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxRecoverableAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.TaxReverseChargeAmount, typeof(decimal), namesToExclude);
			// The columns below are specific for Totals and could be moved out of Base
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.PreCalculatedAmount, typeof(decimal), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.Comment, typeof(string), namesToExclude);
			AddColumnIfNotExcluded(result, AccComplianceReportLineBase.Schema.LineNum, typeof(int), namesToExclude);

			return result.AsReadOnly();
		}

		void AddColumnIfNotExcluded(List<DataColumn> columns, string columnName, Type dataType, HashSet<string> excludedColumnNames = null)
		{
			if (!(excludedColumnNames?.Contains(columnName) ?? false))
			{
				columns.Add(new DataColumn(columnName, dataType));
			}
		}

		public bool IsComplianceDocumentHeaderReport => ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

		internal bool IsPaymentReportGrouping => ReportLineGrouping == ReportLineGroupingListCodes.TransactionPayments
			|| ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable
			|| ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesAll;

		internal bool IsPaymentTimesReportGrouping => ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable
			|| ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesAll;

		internal bool IsPaymentTimesAllReportGouping => ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesAll;

		public HashSet<string> GetColumnNamesToExclude(bool gui = false)
		{
			var result = new List<string>();

			if (!SupportsDayBook)
			{
				result.AddRange(new string[] {
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.GLAccountPK,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					});
			}

			if (IsComplianceDocumentHeaderReport)
			{
				result.AddRange(new string[] {
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.AL_A9_VATClass,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						AccComplianceReportLine.Schema.TaxGroupCode,
			});
			}

			if (IsPaymentReportGrouping)
			{
				result.AddRange(new string[] {
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.AL_A9_VATClass,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						AccComplianceReportLine.Schema.TaxGroupCode,
					});
				if (gui)
				{
					result.Add(AccComplianceReportLine.Schema.ReportSubCode);
				}
			}

			if (IsUsingGLDTablePrefix)
			{
				result.Add(AccComplianceReportLine.Schema.AT_Code);
				result.Add(AccComplianceReportLine.Schema.AT_Type);
				result.Add(AccComplianceReportLineBase.Schema.GoodsExTaxAmount);
				result.Add(AccComplianceReportLineBase.Schema.ServiceExTaxAmount);
				result.Add(AccComplianceReportLineBase.Schema.ServiceTaxAmount);
				result.Add(AccComplianceReportLineBase.Schema.TotalExTaxAmount);
				result.Add(AccComplianceReportLineBase.Schema.TotalTaxAmount);
				result.Add(AccComplianceReportLineBase.Schema.TaxRecoverableAmount);
				result.Add(AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount);
				result.Add(AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount);
				result.Add(AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount);
				result.Add(AccComplianceReportLine.Schema.AH_TransactionReference);
				result.Add(AccComplianceReportLine.Schema.TaxMessage);
				result.Add(AccComplianceReportLine.Schema.GB_Code);
				result.Add(AccComplianceReportLine.Schema.GE_Code);
				result.Add(AccComplianceReportLine.Schema.OK_CustomsRegNo);
				result.Add(AccComplianceReportLine.Schema.ReportSubCode);
				result.Add(AccComplianceReportLine.Schema.AH_ComplianceSubType);
				result.Add(AccComplianceReportLine.Schema.ComplianceSequence);
				result.Add(AccComplianceReportLineBase.Schema.GoodsTaxAmount);
				result.Add(AccComplianceReportLine.Schema.RepCountryRegNo);
			}

			if (!IsPaymentTimesReportGrouping)
			{
				result.Add(AccComplianceReportLineBase.Schema.PreCalculatedAmount);
				addCalculatedColumns();
			}
			else if (gui)
			{
				if (IsPTRS2024AllPayments)
				{
					// Columns not used by the PTRS TCP report
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange),
						nameof(AccComplianceReportLine.DaysRange30_60),
					});
				}
				else
				{
					// These columns only used by the PTRS TCP report
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.IsSmallBusiness),
						nameof(AccComplianceReportLine.IsFullyPaid),
					});
				}

				if (IsPTRS2024SmallBusiness) // It is PTRS 2024 Small Business which uses new columns 
				{
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.DaysRange),
					});
				}
				else // It is PTRS but not the PTRS 2024 Small Business which does not use new columns 
				{
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange30_60),
					});
				}
			}

			if (IsPaymentTimesAllReportGouping)
			{
				addCalculatedColumnsNotUsedByTCP();
			}

			addGuidColumnsForGui();

			void addCalculatedColumns()
			{
				if (gui)
				{
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange),
						nameof(AccComplianceReportLine.DaysRange30_60),
						nameof(AccComplianceReportLine.IsSmallBusiness),
						nameof(AccComplianceReportLine.IsFullyPaid),
					});
				}
			}

			void addCalculatedColumnsNotUsedByTCP()
			{
				if (gui)
				{
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange),
						nameof(AccComplianceReportLine.DaysRange30_60),
					});
				}
			}

			void addGuidColumnsForGui()
			{
				if (gui)
				{
					result.AddRange(new string[] {
						nameof(AccComplianceReportLine.AL_A9_VATClass),
						nameof(AccComplianceReportLine.GLAccountPK)
					});
				}
			}

			return result.ToHashSet();
		}

		string GetSqlForColumnName(string columnName)
		{
			switch (columnName)
			{
				case AccComplianceReportLine.Schema.GeneralLedgerAmountCR:
					return (NoResString)"CASE WHEN GeneralLedgerAmount < 0 THEN -GeneralLedgerAmount ELSE 0 END AS GeneralLedgerAmountCR";
				case AccComplianceReportLine.Schema.GeneralLedgerAmountDR:
					return (NoResString)"CASE WHEN GeneralLedgerAmount > 0 THEN GeneralLedgerAmount ELSE 0 END AS GeneralLedgerAmountDR";
				default:
					return columnName;
			}
		}
	}
}
