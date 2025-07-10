using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class DirectDebitFileFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public DirectDebitFileFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			ModuleFountainFilter batchNumberFilter = filters.AddFountainFilter("Batch Number", AccTransactionHeaderSchema.AH_TransactionNum, "");
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|BatchNumber", "Batch Number");
			batchNumberFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);

			filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccTransactionHeaderSchema.AH_AB, BankList).MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|BankAccount", "Bank Account");

			ModuleFountainFilter paymentTransactionNoFilter = filters.AddFountainFilter("Payment Transaction #", GetTransactionNumberQuery, "");
			paymentTransactionNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|PaymentTransactionNumber", "Payment Transaction #");

			ModuleNumberFilter paymentTransactionReferenceNoFilter = filters.AddNumberFilter("Payment Transaction Reference #", GetTransactionReferenceNumberQuery);
			paymentTransactionReferenceNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|PaymentTransactionReferenceNumber", "Payment Transaction Reference #");

			paymentTransactionNoFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			paymentTransactionReferenceNoFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);

			filters.AddDateFilter("Batch Date", AccTransactionHeaderSchema.AH_PostDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|BatchDate", "Batch Date");
			filters.AddNumberRangeFilter("Total Batch Amount", AccTransactionHeaderSchema.AH_OSTotal).MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|TotalBatchAmount", "Total Batch Amount");
			filters.AddNumberRangeFilter("Payment Transaction Amount", GetPaymentTransactionAmountQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|PaymentTransactionAmount", "Payment Transaction Amount");
			filters.AddTextFilter("Type", GetPaymentTypeQuery, TypeList).MultilingualDescription = ResString.GetMultilingualString("9d4c8f7d-56d5-4ca8-b6eb-d581095135e0", "Type");

			return filters;
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString transactionNumber)
		{
			// note that the comparisonOperator is unused and not available to the user

			// return any DDB that has one or more PAYs with AH_TransactionNum = 'blah'
			var sql = @"
					AH_PK IN
					(
						SELECT Batch.AH_PK FROM dbo.AccTransactionHeader AS Batch

						JOIN dbo.GlbBranch AS BatchBranch
						ON AH_GB = BatchBranch.GB_PK

						JOIN dbo.AccTransactionHeader AS Payment
						ON Payment.AH_ReceiptBatchNo = Batch.AH_TransactionNum

						JOIN dbo.GlbBranch AS PaymentBranch
						ON Payment.AH_GB = PaymentBranch.GB_PK

						WHERE PaymentBranch.GB_GC = BatchBranch.GB_GC
						AND Payment.AH_TransactionType IN ('{0}', '{1}')
						AND Payment.AH_TransactionNum {2} {3} {4}
					)
				";

			var paramName = "@TransactionNumber";
			return GetQueryTunedForMultiSearch(ref transactionNumber, AccTransactionHeaderSchema.AH_TransactionNum, sql, paramName, "");
		}

		ZQuery GetTransactionReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString chequeOrReference)
		{
			// note that the comparisonOperator is unused and not available to the user

			// return any DDB that has one or more PAYs with AH_ChequeOrReference = 'blah'
			var sql = @"
				AH_PK IN
                    (
                        SELECT Batch.AH_PK FROM dbo.AccTransactionHeader AS Batch

                        JOIN dbo.GlbBranch AS BatchBranch
                        ON AH_GB = BatchBranch.GB_PK

                        JOIN dbo.AccTransactionHeader AS Payment
                        ON Payment.AH_ReceiptBatchNo = Batch.AH_TransactionNum

                        JOIN dbo.GlbBranch AS PaymentBranch
                        ON Payment.AH_GB = PaymentBranch.GB_PK

                        WHERE PaymentBranch.GB_GC = BatchBranch.GB_GC
                        AND Payment.AH_TransactionType IN ('{0}', '{1}')
                        AND Payment.AH_ChequeOrReference {2} {3} {4}
                    )
				";

			var paramName = "@ChequeOrReference";

			return GetQueryTunedForMultiSearch(ref chequeOrReference, AccTransactionHeaderSchema.AH_ChequeOrReference, sql, paramName, !chequeOrReference.IsEmpty ? " AND Payment.AH_ChequeOrReference <> '' " : "");
		}

		static ZQuery GetQueryTunedForMultiSearch(ref ZString value, SchemaStringColumn column, string sql, string paramName, string extraFilter)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(DirectDebitBatchHeader));

			if (!AccountingUtils.AnyNumberNotExceedingMaxLength(ref value, column.MaxLength))
			{
				result.IsNoResultQuery = true;
				return result;
			}

			string multiReference = ZString.Empty;
			var usingMultiSearch = false;
			var multiSearchSeparator = RawDataRegistry.Instance.MultiSearchSeparator.Value;
			if (!string.IsNullOrEmpty(multiSearchSeparator) && value.Contains(multiSearchSeparator, StringComparison.Ordinal))
			{
				multiReference = "('" + value.Split(multiSearchSeparator).Select(a => DataUtils.EscapeSingleQuotes(a)).Aggregate((x, y) => x + "', '" + y) + "')";
				usingMultiSearch = true;
			}

			string queryText = string.Format(CultureInfo.InvariantCulture, sql,
				TransactionTypes.Payment, TransactionTypes.DirectPayment,
				usingMultiSearch ? "IN" : "=", usingMultiSearch ? multiReference : paramName,
				extraFilter);

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			if (!usingMultiSearch)
			{
				queryParams.Add(paramName, value, column);
			}

			result.AddFilterAndZSQLParameterCollection(queryText, queryParams);

			return result;
		}

		ZQuery GetPaymentTransactionAmountQuery(INumericZType paymentFrom, INumericZType paymentTo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(DirectDebitBatchHeader));

			// return any DBB that has a PAY where the AH_InvoiceAmount is within the range
			string queryText = string.Format(CultureInfo.InvariantCulture,
				@"
                    AH_PK IN
                    (
                        SELECT Batch.AH_PK FROM dbo.AccTransactionHeader AS Batch

                        JOIN dbo.GlbBranch AS BatchBranch
                        ON AH_GB = BatchBranch.GB_PK

                        JOIN dbo.AccTransactionHeader AS Payment
                        ON Payment.AH_ReceiptBatchNo = Batch.AH_TransactionNum

                        JOIN dbo.GlbBranch AS PaymentBranch
                        ON Payment.AH_GB = PaymentBranch.GB_PK

                        WHERE PaymentBranch.GB_GC = BatchBranch.GB_GC
                        AND 
						(
							(Payment.AH_TransactionType = '{0}' AND Payment.AH_OSTotal >= @PaymentFrom AND Payment.AH_OSTotal <= @PaymentTo)
							OR
							(Payment.AH_TransactionType = '{1}' AND -Payment.AH_OSTotal >= @PaymentFrom AND -Payment.AH_OSTotal <= @PaymentTo)
						)
                    )
                ", ZArchitecture.Core.TransactionTypes.Payment, ZArchitecture.Core.TransactionTypes.DirectPayment);

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			queryParams.Add("@PaymentFrom", paymentFrom, AccTransactionHeaderSchema.AH_InvoiceAmount);
			queryParams.Add("@PaymentTo", paymentTo, AccTransactionHeaderSchema.AH_InvoiceAmount);

			result.AddFilterAndZSQLParameterCollection(queryText, queryParams);

			return result;
		}

		ZQuery GetPaymentTypeQuery(ZString paymentType)
		{
			ZQuery result = new ZQuery();
			if (paymentType == ReceiptTypes.eNettDirectDebit)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.eNettDirectDebit);
			}
			else if (paymentType == ReceiptTypes.DirectDebit)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, new[] { ReceiptTypes.DirectDebit, ReceiptTypes.NonRolledUpBatch });
			}

			return result;
		}

		#region Bind List

		#region Bank

		AccBankAccountCollection fBankList;
		public AccBankAccountCollection BankList
		{
			get
			{
				if (fBankList == null)
				{
					fBankList = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
				}
				return fBankList;
			}
		}

		#endregion

		#region Type

		CodeDescriptionPairList fTypeList;
		CodeDescriptionPairList TypeList
		{
			get
			{
				if (fTypeList == null)
				{
					fTypeList = new CodeDescriptionPairList();
					fTypeList.AddPair(ReceiptTypes.DirectDebit);
					fTypeList.AddPair(ReceiptTypes.eNettDirectDebit);
				}
				return fTypeList;
			}
		}

		#endregion
		#endregion
	}
}
