using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework.Statistics;
using Enterprise.Accounting.Export.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Web.Business
{
	public interface ITransactionNaturalKeys
	{
		string OrgCode { get; set; }
		string CompanyCode { get; set; }
		string AccLedger { get; set; }
		string TransactionType { get; set; }
		string TransactionNumber { get; set; }
		string JobTransactionNumber { get; set; }
		string InternalReference { get; set; }

		List<string> ValidTransactionTypes { get; }

		string TransactionTypeHint { get; }

		string Validate();
	}

	public static class CoreITransactionRequestExtensions
	{
		public static string ValidateCore(this ITransactionNaturalKeys keys)
		{
			StringBuilder errors = new StringBuilder();

			if (string.IsNullOrEmpty(keys.CompanyCode))
			{
				errors.AppendLine((NoResString)"CompanyCode cannot be empty. Please, use " + Enterprise.Core.Constants.ProductName + (NoResString)" Company Code.");
			}
			if (string.IsNullOrEmpty(keys.TransactionType) || !keys.ValidTransactionTypes.Contains(keys.TransactionType.ToUpper()))
			{
				errors.AppendLine(keys.TransactionTypeHint);
			}

			string ledgerHint = (NoResString)"Specify 'AR' for Accounts Receivable or 'AP' for Accounts Payable.";
			if (string.IsNullOrEmpty(keys.AccLedger))
			{
				errors.AppendLine((NoResString)"AccLedger cannot be empty. " + ledgerHint);
			}
			else if (keys.AccLedger.ToUpper() != "AR" && keys.AccLedger.ToUpper() != "AP")
			{
				errors.AppendLine((NoResString)"Invalid AccLedger code. " + ledgerHint);
			}
			else if (keys.AccLedger.ToUpper() == "AR" && string.IsNullOrEmpty(keys.TransactionNumber) && string.IsNullOrEmpty(keys.JobTransactionNumber))
			{
				errors.AppendLine((NoResString)"For the 'AR' AccLedger a TransactionNumber or a JobTransactionNumber should be provided.");
			}
			else if (keys.AccLedger.ToUpper() == "AP" && (string.IsNullOrEmpty(keys.OrgCode) || (string.IsNullOrEmpty(keys.TransactionNumber) && string.IsNullOrEmpty(keys.InternalReference))))
			{
				errors.AppendLine((NoResString)"For the 'AP' AccLedger OrgCode and either TransactionNumber or InternalReference should be provided.");
			}

			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}

		public static void FillFromReader(this ITransactionNaturalKeys keys, SqlDataReader reader)
		{
			keys.OrgCode = reader["OrgCode"].ToString();
			keys.CompanyCode = reader["CompanyCode"].ToString();
			keys.AccLedger = reader["Ledger"].ToString();
			keys.TransactionType = reader["TransactionType"].ToString();
			keys.TransactionNumber = reader["TransactionNumber"].ToString();
			keys.JobTransactionNumber = reader["JobTransactionNumber"].ToString();
			keys.InternalReference = reader["InternalReference"].ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public static System.Data.Common.DbCommand GenerateCheckTransactionPaymentStatusQuery(this ITransactionNaturalKeys request, BaseDataAccess dataAccess)
		{
			var timeNow = DateTime.UtcNow;
			var orgCode = "@OrgCode_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNow, OrgHeaderSchema.OH_Code, request.OrgCode);
			var companyCode = "@CompanyCode_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNow, GlbCompanySchema.GC_Code, request.CompanyCode);

			string command = string.Format(CultureInfo.InvariantCulture, $"EXEC dbo.CheckTransactionPaymentStatus {orgCode}, {companyCode}, @AccLedger, @TransactionType, @TransactionNumber, @JobTransactionNumber, @InternalReference, NULL");

			var parameters = new List<Action<System.Data.Common.DbParameter>>
			{
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = orgCode;
					parameter.Value = request.OrgCode;
					parameter.SqlDbType = System.Data.SqlDbType.NVarChar;
					parameter.Size = 254;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = companyCode;
					parameter.Value = request.CompanyCode;
					parameter.SqlDbType = System.Data.SqlDbType.Char;
					parameter.Size = 3;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = "@AccLedger";
					parameter.Value = request.AccLedger;
					parameter.SqlDbType = System.Data.SqlDbType.Char;
					parameter.Size = 2;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = "@TransactionType";
					parameter.Value = request.TransactionType;
					parameter.SqlDbType = System.Data.SqlDbType.Char;
					parameter.Size = 3;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = "@TransactionNumber";
					parameter.Value = !string.IsNullOrEmpty(request.TransactionNumber) ? request.TransactionNumber : DBNull.Value;
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Size = 38;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = "@JobTransactionNumber";
					parameter.Value = !string.IsNullOrEmpty(request.JobTransactionNumber) ? request.JobTransactionNumber : DBNull.Value;
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Size = 38;
				},
				(p) =>
				{
					var parameter = new SqlParameterWrapper(p);
					parameter.ParameterName = "@InternalReference";
					parameter.Value = !string.IsNullOrEmpty(request.InternalReference) ? request.InternalReference : DBNull.Value;
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Size = 38;
				}
			};

			return dataAccess.GetCommand(command, parameters.ToArray());
		}
	}
}
