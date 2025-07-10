using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Web.Business.TransactionPaymentDataAccess;

namespace Enterprise.Accounting.Web.Business
{
	public class InvoicePaymentDetailsUpdater
	{
		public InvoicePaymentDetailsUpdater(TransactionPaymentDataAccess dataAccess)
		{
			this.dataAccess = dataAccess;
		}

		readonly TransactionPaymentDataAccess dataAccess;

		public UpdateInvoicePaymentDetailsResponse PayTransaction(UpdateInvoicePaymentDetailsRequest request)
		{
			UpdateInvoicePaymentDetailsResponse result = new UpdateInvoicePaymentDetailsResponse();
			result.OriginalRequest = request;

			if (request.PaymentDate.HasValue && !dataAccess.DoesDateBelongToOpenAccountingPeriod(request.CompanyCode, request.PaymentDate.Value))
			{
				result.Succeeded = false;
				result.ErrorMessage = string.Format((NoResString)"Payment date {0:d} does not fall into an open accounting period for the {1} company.", request.PaymentDate.Value, request.CompanyCode);
				return result;
			}

			if (!string.IsNullOrEmpty(request.PaymentReference))
			{
				if (request.PaymentReference.Length > 20) // This is the current length in DB. May be extended in the future
				{
					result.Succeeded = false;
					result.ErrorMessage = (NoResString)"Payment Reference should be not longer than 20 characters.";
					return result;
				}
				else if (!(new Regex("^[-A-Za-z0-9 /]+$").IsMatch(request.PaymentReference)))
				{
					result.Succeeded = false;
					result.ErrorMessage = (NoResString)"Payment Reference should contain only numbers or letters.";
					return result;
				}
			}

			Guid transactionPK = Guid.Empty;
			Decimal totalAmount = Decimal.Zero;
			Decimal outstandingAmount = Decimal.Zero;
			string currencyCode = "";
			using (var reader = dataAccess.GetTransactionPaymentStatusReader(request))
			{
				if (reader.Read())
				{
					transactionPK = (Guid)reader["TransactionHeaderPK"];
					totalAmount = (Decimal)reader["InvoiceTotal"];
					outstandingAmount = (Decimal)reader["OutstandingAmount"];
					currencyCode = (string)reader["CurrencyCode"];
					object fullyPaidDateValue = reader["FullyPaidDate"];
					DateTime? fullyPaidDate = fullyPaidDateValue != null && fullyPaidDateValue != DBNull.Value ? (DateTime?)fullyPaidDateValue : null;

					if (request.AmountPaidInCompanyCurrency > 0)
					{
						if (fullyPaidDate.HasValue)
						{
							result.Succeeded = false;
							result.ErrorMessage = (NoResString)"Transaction has been already fully paid.";
							return result;
						}

						if (Math.Abs(outstandingAmount) < request.AmountPaidInCompanyCurrency)
						{
							result.Succeeded = false;
							result.ErrorMessage = (NoResString)"AmountPaidInCompanyCurrency exceeds Transaction's Outstanding Amount.";
							return result;
						}

						if (Math.Abs(outstandingAmount) == request.AmountPaidInCompanyCurrency && !request.PaymentDate.HasValue)
						{
							result.Succeeded = false;
							result.ErrorMessage = (NoResString)"Payment Date must be provided when transaction is fully paid.";
							return result;
						}
					}
					else
					{
						if (Math.Abs(outstandingAmount) + Math.Abs(request.AmountPaidInCompanyCurrency) > Math.Abs(totalAmount))
						{
							result.Succeeded = false;
							result.ErrorMessage = (NoResString)"Reversing payment cannot produce Outstanding Amount greater than Total Amount.";
							return result;
						}
					}

					if (reader.Read())
					{
						result.Succeeded = false;
						result.ErrorMessage = (NoResString)"More than one Transaction was found for the given key data.";
						return result;
					}
				}
				else
				{
					result.Succeeded = false;
					result.ErrorMessage = (NoResString)"No Transactions were found for the given key data.";
					return result;
				}
			}

			if (dataAccess.TransactionHasMatchLinks(transactionPK))
			{
				result.Succeeded = false;
				result.ErrorMessage = (NoResString)"Transaction already has Matching and cannot be paid via this Web Service.";
				return result;
			}

			Decimal newOutstandingAmount = outstandingAmount - request.AmountPaidInCompanyCurrency * Math.Sign(totalAmount);

			var info = CalculateOSOutstandingAmount(transactionPK, newOutstandingAmount);
			info.CurrentOutstandingAmount = outstandingAmount;
			info.NewOutstandingAmount = newOutstandingAmount;
			info.CurrencyCode = currencyCode;

			dataAccess.UpdateTransactionPaymentDetails(transactionPK, totalAmount, info, request.PaymentDate, request.PaymentReference);

			using (var reader = dataAccess.GetTransactionPaymentStatusReader(request))
			{
				if (reader.Read())
				{
					result.FillFromReader(reader);
					result.Succeeded = true;
				}
			}

			return result;
		}

		OutstandingAmountInfo CalculateOSOutstandingAmount(Guid transactionPK, ZDecimal localOutstandingAmount)
		{
			string currency = string.Empty;
			decimal osTotal = Decimal.Zero;
			decimal localTotal = Decimal.Zero;
			decimal currentOSOutstandingAmount = Decimal.Zero;
			Guid transactionBranch = Guid.Empty;
			Guid transactionDepartment = Guid.Empty;
			bool isApplicable = false;
			bool isFeatureEnabled = false;
			bool isFoundTransaction = false;

			using (var reader = dataAccess.GetTransactionPaymentStatusExtraInfoReader(transactionPK))
			{
				if (reader.Read())
				{
					isFoundTransaction = true;
					currency = (String)reader["AH_RX_NKTransactionCurrency"];
					osTotal = (Decimal)reader["AH_OSTotal"];
					localTotal = (Decimal)reader["AH_LocalTotal"];
					isApplicable = (bool)reader["AH_IsOSOutstandingAmountApplicable"];
					currentOSOutstandingAmount = (Decimal)reader["AH_OSOutstandingAmount"];
					transactionBranch = (Guid)reader["AH_GB"];
					transactionDepartment = (Guid)reader["AH_GE"];
					isFeatureEnabled = (int)reader["EnableNewOSOutstandingAmountFeature"] == 1;
				}
			}

			if (isFoundTransaction)
			{
				decimal newOSOutstandingAmount = currentOSOutstandingAmount;
				if (isFeatureEnabled)
				{
					using (CargoWise.Data.Db.DisposableActionForDbConnection())
					using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, transactionBranch, transactionDepartment)))
					{
						newOSOutstandingAmount = isApplicable && localOutstandingAmount != 0
							? TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(localTotal, osTotal, localOutstandingAmount, currency)
							: 0;
					}
				}

				return new OutstandingAmountInfo()
				{
					IsOSOutstandingAmountApplicable = isApplicable,
					IsEnableNewOSOutstandingAmountFeature = isFeatureEnabled,
					CurrentOSOutstandingAmount = currentOSOutstandingAmount,
					NewOSOutstandingAmount = newOSOutstandingAmount
				};
			}

			return new OutstandingAmountInfo();
		}
	}
}
