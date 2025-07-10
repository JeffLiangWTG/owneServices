using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class ComplianceReportTransactionQueuer<T> : IComplianceReportQueuer
		where T : TransactionHeader, ISupportQueueingForComplianceReports
	{
		public ComplianceReportTransactionQueuer(T transaction)
		{
			Transaction = transaction;
		}

		T Transaction { get; }

		public void TryToQueueForComplianceReports()
		{
			var reports = ComplianceReportTransactionQueueingHelper.GetComplianceReportsOfCompanyCountry();

			var exemptGroupingCodes = new string[] {
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TransactionPayments
			};
			string[] exemptReportCodes = null;

			var reportsToCheckMatching = reports.Where(report => !(exemptGroupingCodes.Contains(report.ReportLineGrouping.ToString())
				|| (exemptReportCodes ?? (exemptReportCodes = ComplianceReportTransactionQueueingHelper.GetExemptReportCodes(Transaction))).Contains(report.ReportCode.ToString())));

			foreach (var report in reportsToCheckMatching)
			{
				var setting = ComplianceReportTransactionQueueingHelper.GetMatchingReportComplianceRuleSetting(Transaction.ComplianceMatchingRule, report);

				if (setting != null)
				{
					var dateSource = TransactionDateProvider.ConvertReportingDate(setting.ReportingDate);

					if (report.ReportBaseTablePrefix == AccTransactionHeaderSchema.Constants.Prefix)
					{
						QueueHeaderForComplianceReportCore(report.ReportCode, Transaction, dateSource);
					}

					if (report.ReportBaseTablePrefix == AccTransactionLinesSchema.Constants.Prefix)
					{
						if (Transaction is TransactionHeaderWithLines transactionHeaderWithLines)
						{
							if (report.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.OrganisationBLCode &&
								report.Country == Core.Constants.CountryCodes.Italy)
							{
								QueueLinesForComplianceReportWithBLSubCode(report.ReportCode, transactionHeaderWithLines, dateSource);
							}
							else if (report.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.OrganisationSubCode)
							{
								QueueLinesForComplianceReportWithChargeSubCode(report.ReportCode, report.RecipientOrgPK, transactionHeaderWithLines, dateSource);
							}
							else if (report.ReportLineGrouping == ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TransactionHeaderWithLines)
							{
								QueueLinesForComplianceReportWithLineNumber(report.ReportCode, transactionHeaderWithLines, dateSource);
							}
							else
							{
								QueueLinesForComplianceReportCore(report.ReportCode, transactionHeaderWithLines, dateSource);
							}
						}
					}
				}
			}
		}

		void QueueLinesForComplianceReportWithBLSubCode(ZString reportCode, TransactionHeaderWithLines transactionWithLines, TransactionDateProvider.DateKind date)
		{
			var applicableLines = transactionWithLines.Lines.Cast<TransactionLine>().Where(x => !x.AL_LineAmount.IsEmpty || !x.AL_GSTVAT.IsEmpty).ToArray();
			var reportingDate = transactionWithLines.GetTransactionDate(date);

			QueueLinesForComplianceReportWithSubCode(reportCode, applicableLines, reportingDate, (line) =>
			{
				if (line.TaxRate != null &&
					(line.TaxRate.AT_Type == AccTaxRate.Types.Rated || line.TaxRate.AT_Type == AccTaxRate.Types.CapitalRated || line.TaxRate.AT_Type == AccTaxRate.Types.Exempt))
				{
					if (transactionWithLines.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						return "BL003";
					}
					else if (transactionWithLines.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						return "BL006";
					}
				}
				else
				{
					if (transactionWithLines.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						return "BL004";
					}
					else if (transactionWithLines.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						return "BL007";
					}
				}

				return string.Empty;
			});
		}

		void QueueLinesForComplianceReportWithChargeSubCode(ZString reportCode, ZGuid recipientCodeMappingOrgPK, TransactionHeaderWithLines transactionWithLines, TransactionDateProvider.DateKind date)
		{
			var chargeCodeMapping = ComplianceReportTransactionQueueingHelper.BuildChangeCodeToReportSubCodeMap(transactionWithLines.Factory, recipientCodeMappingOrgPK);
			var applicableLines = transactionWithLines.Lines.Cast<TransactionLine>().Where(x => x.ChargeCode != null && chargeCodeMapping.ContainsKey(x.ChargeCode.AC_Code)).ToArray();
			var reportingDate = transactionWithLines.GetTransactionDate(date);

			QueueLinesForComplianceReportWithSubCode(reportCode, applicableLines, reportingDate, (line) => chargeCodeMapping[line.ChargeCode.AC_Code].ToString());
		}

		void QueueLinesForComplianceReportWithLineNumber(ZString reportCode, TransactionHeaderWithLines transactionWithLines, TransactionDateProvider.DateKind date)
		{
			var lineArray = transactionWithLines.Lines.ToArray<TransactionLine>();
			var reportingDate = transactionWithLines.GetTransactionDate(date);

			var uniqueSequence = lineArray.Select(line => line.AL_Sequence).Distinct().Count() == lineArray.Length;
			if (uniqueSequence)
			{
				QueueLinesForComplianceReportWithSubCode(reportCode, lineArray, reportingDate, (line) => line.AL_Sequence.ToString("D5", CultureInfo.InvariantCulture));
			}
			else
			{
				int sequence = 1;
				var reSequenced = lineArray.OrderBy(line => line, lineArray.CreateFuncComparerForElements((x1, x2) =>
				{
					var jobNumber1 = new ZString(x1.Job?.JH_JobNum);
					var jobNumber2 = new ZString(x2.Job?.JH_JobNum);
					var result = jobNumber1.CompareTo(jobNumber2);
					if (result == 0)
					{
						result = x1.AL_Sequence.CompareTo(x2.AL_Sequence);
					}
					return result;
				})).ToDictionary((line) => line, (x) => sequence++);

				QueueLinesForComplianceReportWithSubCode(reportCode, lineArray, reportingDate, (line) => reSequenced[line].ToString("D5", CultureInfo.InvariantCulture));
			}
		}

		void QueueLinesForComplianceReportWithSubCode(ZString reportCode, TransactionLine[] applicableLines, ZDate reportingDate, Func<TransactionLine, string> getSubCode)
		{
			if (applicableLines.Any())
			{
				QueueLinesForComplianceReportCore(reportCode, applicableLines, reportingDate, getSubCode);
			}
		}

		void QueueLinesForComplianceReportCore(ZString reportCode, TransactionHeaderWithLines transactionWithLines, TransactionDateProvider.DateKind date, Func<TransactionLine, string> getSubCode = null)
		{
			var applicableLines = transactionWithLines.Lines.Cast<TransactionLine>().ToArray();
			var reportingDate = transactionWithLines.GetTransactionDate(date);

			QueueLinesForComplianceReportCore(reportCode, applicableLines, reportingDate, getSubCode);
		}

		void QueueLinesForComplianceReportCore(ZString reportCode, IEnumerable<TransactionLine> applicableLines, ZDate reportingDate, Func<TransactionLine, string> getSubCode = null)
		{
			foreach (var lineBatch in AccountingUtils.ChunksOf(applicableLines.Where(x => !x.IsCommentCharge), LineQueueBatchSize))
			{
				QueueLineBatchForComplianceReportCore(lineBatch, reportCode, reportingDate, getSubCode);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void QueueLineBatchForComplianceReportCore(IList<TransactionLine> lineBatch, ZString reportCode, ZDate reportingDate, Func<TransactionLine, string> getSubCode = null)
		{
			var factory = lineBatch.First().Factory;
			var sqlBuilder = new ZStringBuilder();
			var parameters = new List<ZSqlParameter>();
			var i = 0;

			ValidateReportingDate(reportingDate, reportCode);
			sqlBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES ",
						AccTransactionComplianceReportQueueSchema.Constants.TableName,
						AccTransactionComplianceReportQueueSchema.Constants.PK,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_GB_Branch,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date,
						AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportSubCode));

			foreach (var line in lineBatch)
			{
				sqlBuilder.AppendLine((i > 0 ? "," : "") +
					string.Format(CultureInfo.InvariantCulture, (NoResString)"(NEWID(), @CompanyPK, @BranchPK, @ReportType, @ParentTableCode, @ParentPK{0}, @Date{0}, @SubCode{0})", i)); // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI

				parameters.Add(ZSqlParameter.New(string.Format(CultureInfo.InvariantCulture, "@ParentPK{0}", i), line.PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID));
				parameters.Add(ZSqlParameter.New(string.Format(CultureInfo.InvariantCulture, "@Date{0}", i)/*As we apply the same date, we can use the same parameter name*/, reportingDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date));
				parameters.Add(ZSqlParameter.New(string.Format(CultureInfo.InvariantCulture, "@SubCode{0}", i), getSubCode != null ? getSubCode(line) : string.Empty, AccTransactionComplianceReportQueueSchema.ACQ_ReportSubCode));
				i++;
			}

			var command = ((IDbConnected)factory).Connection.Command(sqlBuilder.ToString());    // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI

			command.AddParameterBasedOnDbColumn("@CompanyPK", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@BranchPK", GlbBranch.CurrentBranch.PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GB_Branch);
			command.AddParameterBasedOnDbColumn("@ReportType", reportCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);
			command.AddParameterBasedOnDbColumn("@ParentTableCode", AccTransactionLinesSchema.Constants.Prefix, AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode);
			foreach (var param in parameters)
			{
				command.AddParameterBasedOnDbColumn(param.ParameterName, param.Value, param.SchemaColumn);
			}

			command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void QueueHeaderForComplianceReportCore(ZString reportCode, TransactionHeader transaction, TransactionDateProvider.DateKind date)
		{
			var reportingDate = transaction.GetTransactionDate(date);
			ValidateReportingDate(reportingDate, reportCode);
			var sql = FormattableString.Invariant($@"INSERT INTO {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName} (
				{AccTransactionComplianceReportQueueSchema.Constants.PK}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_GB_Branch}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID}
				, {AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date}) 
				VALUES (newid(), @CompanyPK, @BranchPK, @ReportType, @ParentTableCode, @ParentPK, @Date)");

			var command = ((IDbConnected)transaction.Factory).Connection.Command(sql);  // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI
			command.AddParameterBasedOnDbColumn("@CompanyPK", Environment.Env.CurrentCompany.PK, AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@BranchPK", Environment.Env.CurrentBranch.PK, AccTransactionComplianceReportQueueSchema.ACQ_GB_Branch);
			command.AddParameterBasedOnDbColumn("@ReportType", reportCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);
			command.AddParameterBasedOnDbColumn("@ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode);
			command.AddParameterBasedOnDbColumn("@ParentPK", transaction.PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID);
			command.AddParameterBasedOnDbColumn("@Date", transaction.GetTransactionDate(date).ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);

			command.ExecuteNonQuery();
		}

		const int LineQueueBatchSize = 200;

		void ValidateReportingDate(ZDate reportingDate, string reportCode)
		{
			if (reportingDate.IsEmpty)
			{
				throw new ApplicationException($"Cannot obtain date for queueing compliance report of type {reportCode}. Please contact support.");
			}
		}
	}
}
