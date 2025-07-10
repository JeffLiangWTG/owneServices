using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public enum ReturnCode
	{
		Success = 0,
		Failure = 1,
		Warning = 2
	}

	public readonly struct ComplianceReportValidationResult
	{
		public ComplianceReportValidationResult(string notesDescription)
			: this(string.Empty, string.Empty, notesDescription, string.Empty, ReturnCode.Success)
		{ }

		public ComplianceReportValidationResult()
			: this(string.Empty, string.Empty, string.Empty, string.Empty, ReturnCode.Success)
		{ }

		public ComplianceReportValidationResult(string status, string statusMessage, string notesDescription, string notesText, ReturnCode returnCode)
		{
			Status = status;
			StatusMessage = statusMessage;
			NotesDescription = notesDescription;
			NotesText = notesText;
			ReturnCode = returnCode;
		}

		public string Status { get; }
		public string StatusMessage { get; }
		public string NotesDescription { get; }
		public string NotesText { get; }
		public ReturnCode ReturnCode { get; }
	}

	public sealed class AccComplianceReportProcessingInfoZM
	{
		public AccComplianceReportProcessingInfoZM(AccComplianceReport complianceReport)
		{
			ComplianceReport = Argument.NotNull(complianceReport, nameof(ComplianceReport));
		}

		public ComplianceReportValidationResult Validate()
		{
			return CheckDebtorsWithoutVATIDHavingRVSTaxID();
		}

		readonly AccComplianceReport ComplianceReport;
		internal readonly struct TransactionsOfDebtorsWithoutVATID
		{
			internal ZString Code { get; }
			internal ZString FullName { get; }
			internal ZString Transactions { get; }

			internal TransactionsOfDebtorsWithoutVATID(string code, string name, string transactions)
			{
				Code = code;
				FullName = name;
				Transactions = transactions;
			}
		}

		internal const string SQLForTransactionsOfDebtorsWithoutVATID = @"WITH relevantQueueRows
														AS
														(SELECT DISTINCT AL_AH
														   FROM dbo.AccTransactionComplianceReportQueue
														   JOIN dbo.AccTransactionLines ON ACQ_ParentID = AL_PK
														  WHERE ACQ_ReportType = @ReportType
															AND ACQ_Date BETWEEN @StartDate AND @EndDate
															AND ACQ_ParentTableCode = 'AL'
															AND ACQ_GC_Company = @CompanyPK
															AND ACQ_ReportSubCode = 'VAT ID missing')

														SELECT OH_Code,
															   OH_FullName,
															   CONCAT(
																	LEFT(STRING_AGG(AH_TransactionNum, ',') WITHIN GROUP (ORDER BY AH_TransactionNum ASC), 100), 
																	IIF(LEN(STRING_AGG(AH_TransactionNum, ',') WITHIN GROUP (ORDER BY AH_TransactionNum ASC)) > 100, ' ...', '')
																	) AS Transactions
														FROM (
															SELECT DISTINCT 
																   OH_Code, 
																   OH_FullName, 
																   AH_TransactionNum
															FROM relevantQueueRows
															JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
															JOIN dbo.OrgHeader ON AH_OH = OH_PK
															) innerTable
														GROUP BY OH_Code, OH_FullName";

		ComplianceReportValidationResult CheckDebtorsWithoutVATIDHavingRVSTaxID()
		{
			var notesText = new ZStringBuilder();
			var notesDescription = (NoResString)"Debtors without VAT ID";

			var debtorsWithoutVATID = RetrieveDebtorsWithoutVATID();
			if (debtorsWithoutVATID.Any())
			{
				notesText.Append($"{Res.GetString("B09F0185-F574-4BE9-AF94-8B4B1C845F2C", "These debtors have Accounts Receivable transactions (INV, CRD, ADJ) with reverse charge Tax ID but do not have a VAT ID assigned.")}");
				notesText.Append("");
				debtorsWithoutVATID.ForEach(debtor => notesText.Append($"{debtor.Code} - {debtor.FullName} : {debtor.Transactions}"));

				return new ComplianceReportValidationResult(AccComplianceReport.Status.ReportError, Res.GetString("73D614EA-33D7-4D6C-BEB3-F07F886E3E13", "Debtors without VAT ID found. Please check the Notes tab for details."), notesDescription, notesText.ToStringWithNewLineBetweenAppends(), returnCode: ReturnCode.Failure);
			}
			else
			{
				return new ComplianceReportValidationResult(notesDescription);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "ComplianceReportValidation")]
		IEnumerable<TransactionsOfDebtorsWithoutVATID> RetrieveDebtorsWithoutVATID()
		{
			var result = new List<TransactionsOfDebtorsWithoutVATID>();

			using (var command = Db.Connection.Command(SQLForTransactionsOfDebtorsWithoutVATID))
			{
				command.AddParameterBasedOnDbColumn("@CompanyPK", ComplianceReport.ACR_GC_Company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
				command.AddParameterBasedOnDbColumn("@StartDate", ComplianceReport.ACR_DateFrom.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);
				command.AddParameterBasedOnDbColumn("@EndDate", ComplianceReport.ACR_DateTo.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);
				command.AddParameterBasedOnDbColumn("@ReportType", ComplianceReport.ACR_ReportType.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);

				var dataTable = DataUtils.GetDataTableFromCommand(command);

				foreach (DataRow row in dataTable.Rows)
				{
					result.Add(new TransactionsOfDebtorsWithoutVATID(new ZString(row["OH_Code"]), new ZString(row["OH_FullName"]), new ZString(row["Transactions"])));
				}
			}

			return result;
		}
	}
}
