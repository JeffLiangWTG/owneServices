using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Accounting.Export.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Web.Business
{
	public class TransactionPaymentDataAccess : BaseDataAccess
	{
		public TransactionPaymentDataAccess(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction) : base(connection, transaction)
		{
		}

		public TransactionPaymentDataAccess(System.Data.Common.DbConnection connection) : base(connection)
		{
		}

		public bool GetBoolRegistryValue(string registryKey, string companyCode = null, bool defaultValue = false)
		{
			bool forCompany = !string.IsNullOrEmpty(companyCode);
			string sql = string.Format("SELECT TOP 1 CAST(CAST(SD_BinaryValue AS BINARY(10)) AS NVARCHAR(5)) FROM dbo.StmData {0} WHERE SD_Name = @RegistryKey {1}",
				forCompany ? "JOIN dbo.GlbCompany ON SD_Owner = GC_PK AND GC_Code = @CompanyCode" : "",
				forCompany ? "" : "AND SD_Owner IS NULL");

			var parameters = new List<(string Name, object Value)>() { new ("@RegistryKey", registryKey) };
			if (forCompany)
			{
				parameters.Add(new ("@CompanyCode", companyCode));
			}

			using (var cmd = GetCommand(sql, parameters.ToArray()))
			{
				object result = cmd.ExecuteScalar();
				return result != null ? bool.Parse(result.ToString()) : defaultValue;
			}
		}

		public bool TransactionHasMatchLinks(Guid transactionPK)
		{
			const string command = "SELECT TOP 1 AP_PK FROM dbo.AccTransactionMatchLink WHERE AP_AH = @PK";
			var parameters = new List<(string Name, object Value)> { new ("@PK", transactionPK) };

			using (var cmd = GetCommand(command, parameters.ToArray()))
			{
				object result = cmd.ExecuteScalar();
				return result != null && result != DBNull.Value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Standalone Web application without access to ZTypes")]
		public void UpdateTransactionPaymentDetails(Guid transactionPK, decimal totalAmount, OutstandingAmountInfo outstandingAmountInfo, DateTime? fullyPaidDate, string paymentReference)
		{
			var info = outstandingAmountInfo;
			var isFullyPaid = info.NewOutstandingAmount == decimal.Zero;
			if (!isFullyPaid && Math.Sign(totalAmount) != Math.Sign(info.NewOutstandingAmount))
			{
				throw new ArgumentException("Outstanding Amount must has the same sign as Total Amount");
			}
			if (Math.Abs(info.NewOutstandingAmount) > Math.Abs(totalAmount))
			{
				throw new ArgumentException("Outstanding Amount must not exceed Total Amount");
			}
			if (isFullyPaid && !fullyPaidDate.HasValue)
			{
				throw new ArgumentException("PaymentDate must be provided when transaction is fully paid.");
			}

			var hasPayRef = !string.IsNullOrEmpty(paymentReference);

			var commandRoot = info.IsEnableNewOSOutstandingAmountFeature ? UpdatePaymentDetailNewSql : UpdatePaymentDetailOldSql;
			var command = string.Format(commandRoot, hasPayRef ? ", AH_ChequeOrReference = @PayRef" : "");

			var parameters = new List<(string Name, object Value)>
			{
				new("@NewOutstandingAmount", info.NewOutstandingAmount),
				new("@PK", transactionPK),
				new("@Date", isFullyPaid && fullyPaidDate.HasValue ? fullyPaidDate.Value : DBNull.Value),
				new("@OldOutstandingAmount", info.CurrentOutstandingAmount),
				new("@IsOSOutstandingAmountApplicable", info.IsOSOutstandingAmountApplicable),
				new("@OldOSOutstandingAmount", info.CurrentOSOutstandingAmount),
				new("@NewOSOutstandingAmount", info.NewOSOutstandingAmount),
				new("@TotalAmount", totalAmount)
			};
			if (hasPayRef)
			{
				parameters.Add(new ("@PayRef", paymentReference));
			}

			string logCommand = @"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser)
VALUES (@LogPK, 'AccTransactionHeader', @ParentPK, 'N', @Details, @UtcDate, 'EDT', @EventDate, 'ZZ')";

			var logParams = new List<(string Name, object Value)>
			{
				new("@LogPK", Guid.NewGuid()),
				new("@ParentPK", transactionPK),
				new("@UtcDate", DateTime.Now.ToUniversalTime()),
				new("@EventDate", DateTime.Now)
			};

			string details = isFullyPaid ?
				string.Format((NoResString)"Fully paid with Payment Date {0} by Transaction Payment Web Service|Fully Matched", fullyPaidDate.Value.ToShortDateString()) :
				string.Format((NoResString)"Paid to {0} {1} Outstanding Amount by Transaction Payment Web Service", info.CurrencyCode, info.NewOutstandingAmount.ToString("0.00")) +
				(hasPayRef ? (NoResString)" Payment Reference :" + paymentReference : "");
			logParams.Add(new ("@Details", details));

			if (Transaction != null)
			{
				using (var cmd = GetCommand(command, Transaction, parameters.ToArray()))
				{
					cmd.ExecuteNonQuery();
				}

				using (var cmd = GetCommand(logCommand, Transaction, logParams.ToArray()))
				{
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				using (var updateTransaction = Connection.BeginTransaction())
				{
					using (var cmd = GetCommand(command, updateTransaction, parameters.ToArray()))
					{
						cmd.ExecuteNonQuery();
					}

					using (var cmd = GetCommand(logCommand, updateTransaction, logParams.ToArray()))
					{
						cmd.ExecuteNonQuery();
					}

					updateTransaction.Commit();
				}
			}
		}

		public System.Data.Common.DbDataReader GetTransactionPaymentStatusReader(ITransactionNaturalKeys request)
		{
			return request.GenerateCheckTransactionPaymentStatusQuery(this).ExecuteReader();
		}

		internal System.Data.Common.DbDataReader GetTransactionPaymentStatusExtraInfoReader(Guid aH_PK)
		{
			string command = string.Format(CultureInfo.InvariantCulture, $@"
SELECT TOP(1)
	AH_GB,
	AH_GE,
	AH_LocalTotal,
	AH_OSTotal,
	AH_RX_NKTransactionCurrency,
	AH_IsOSOutstandingAmountApplicable,
	AH_OSOutstandingAmount,
	IIF(EXISTS(SELECT TOP(1) 1 FROM dbo.StmData WHERE SD_Name = 'EnableNewOSOutstandingAmountFeature' AND CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'true'), 1, 0)
		AS EnableNewOSOutstandingAmountFeature
FROM dbo.AccTransactionHeader
WHERE AH_PK = @AH_PK
");
			var parameters = new List<(string Name, object Value)>() {
				new ("@AH_PK", aH_PK)
			};
			return GetCommand(command, parameters.ToArray()).ExecuteReader();
		}

		public int? TryGetCompanyCurrencySubUnitRatio(string companyCode)
		{
			using (var companyReader = GetCompanyReader(companyCode))
			{
				return (int?)companyReader?[RefCurrencySchema.RX_SubUnitRatio.Name];
			}
		}

		public class OutstandingAmountInfo
		{
			public decimal CurrentOutstandingAmount { get; set; }
			public decimal NewOutstandingAmount { get; set; }
			public decimal CurrentOSOutstandingAmount { get; set; }
			public decimal NewOSOutstandingAmount { get; set; }
			public bool IsEnableNewOSOutstandingAmountFeature { get; set; }
			public bool IsOSOutstandingAmountApplicable { get; set; }
			public string CurrencyCode { get; set; }
		}

		const string UpdatePaymentDetailOldSql = @"
UPDATE dbo.AccTransactionHeader 
SET
	AH_OutstandingAmount = @NewOutstandingAmount,
	AH_FullyPaidDate = @Date {0},
	AH_SystemLastEditTimeUtc = GETUTCDATE(),
	AH_SystemLastEditUser = '~BP'
WHERE
	AH_PK = @PK AND
	AH_OutstandingAmount = @OldOutstandingAmount AND
	(AH_InvoiceAmount + AH_GSTAmount) = @TotalAmount
IF @@ROWCOUNT = 0 RAISERROR('ConcurrencyError', 16, 1)";

		const string UpdatePaymentDetailNewSql = @"
UPDATE dbo.AccTransactionHeader 
SET
	AH_OutstandingAmount = @NewOutstandingAmount,
	AH_OSOutstandingAmount = @NewOSOutstandingAmount,
	AH_FullyPaidDate = @Date {0},
	AH_SystemLastEditTimeUtc = GETUTCDATE(),
	AH_SystemLastEditUser = '~BP'
WHERE
	AH_PK = @PK AND
	AH_OutstandingAmount = @OldOutstandingAmount AND
	AH_OSOutstandingAmount = @OldOSOutstandingAmount AND
	AH_IsOSOutstandingAmountApplicable = @IsOSOutstandingAmountApplicable AND
	(AH_InvoiceAmount + AH_GSTAmount) = @TotalAmount
IF @@ROWCOUNT = 0 RAISERROR('ConcurrencyError', 16, 1)";
	}
}
