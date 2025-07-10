using System.IO;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class YusenDDRFileGenerator : DDRFileGenerator
	{
		public YusenDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected const string separator = ",";

		protected override bool HasDescriptiveHeaderRecord
		{
			get { return false; }
		}

		protected override void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
		}

		protected override void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			string valueDate = Header.AH_ChequeOrReference.SubstringSafe(0, 8);
			ZDecimal totalAmount = IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount;
			string amount = totalAmount.ToString(string.Format("F{0}", row.TransactionCurrency.Decimals));
			string emptyField = DoubleQuotes("");

			ZStringBuilder builder = new ZStringBuilder();
			builder.Append(DoubleQuotes(""));												//  1. Serial Number				 (Optional)
			builder.Append(DoubleQuotes("GI"));				    							//  2. Payment Method				 (Mandatory)
			builder.Append(DoubleQuotes("MIS"));		        							//  3. Transaction Code				 (Mandatory)
			builder.Append(DoubleQuotes("T"));												//  4. Record Type					 (Mandatory)
			builder.Append(DoubleQuotes(valueDate, 8));										//  5. Value Date					 (Mandatory)
			builder.Append(DoubleQuotes(row.BankAccount.AB_AccountNum, 8));					//  6. Account No. with BTM			 (Mandatory)
			builder.Append(DoubleQuotes(row.TransactionCurrency.RX_Code, 3));				//  7. Currency						 (Optional)
			builder.Append(DoubleQuotes(amount, 15));										//  8. Amount						 (Mandatory)
			builder.Append(DoubleQuotes(GetAccountTitle(row), 35));							//  9. Beneficiary Name 1			 (Mandatory)
			builder.Append(DoubleQuotes(""));												// 10. Beneficiary Name 2			 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 11. Beneficiary Attention		 (Not Applicable)
			builder.Append(DoubleQuotes(""));					                			// 12. Contact						 (Conditional)
			builder.Append(DoubleQuotes(""));												// 13. Beneficiary Address 1		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 14. Beneficiary Address 2		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 15. Beneficiary Address 3		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 16. Beneficiary Country			 (Not Applicable)
			builder.Append(DoubleQuotes(GetPayeeBankAccountNumber(row), 34));				// 17. Beneficiary Account No.		 (Mandatory
			builder.Append(DoubleQuotes(""));												// 18. Beneficiary Bank Name		 (Not Applicable)
			builder.Append(DoubleQuotes(GetPayeeBankBSB(row).SubstringSafe(0, 4)));			// 19. Beneficiary Bank Code		 (Mandatory)
			builder.Append(DoubleQuotes(""));												// 20. Beneficiary Branch Name		 (Not Applicable)
			builder.Append(DoubleQuotes(GetPayeeBankBSB(row).SubstringSafe(4, 3)));			// 21. Beneficiary Branch Code		 (Mandatory)
			builder.Append(DoubleQuotes(""));												// 22. Beneficiary Bank Address 1	 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 23. Beneficiary Bank Address 2	 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 24. Beneficiary Bank Address 3	 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 25. Beneficiary Bank Country		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 26. Beneficiary Bank BIC			 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 27. Intermediary Bank Name		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 28. Intermediary Bank Branch Name (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 29. Intermediary Bank Address1    (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 30. Intermediary Bank Address2    (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 31. Intermediary Bank Address3    (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 32. Intermediary Bank Country     (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 33. Intermediary Bank BIC         (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 34. Charge Account No.			 (Not Applicable)
			builder.Append(DoubleQuotes(""));						 						// 35. Message to Beneficiary		 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 36. Message to Bank				 (Not Applicable)
			builder.Append(DoubleQuotes(row.AH_ChequeOrReference, 16));						// 37. Reference 					 (Conditional)
			builder.Append(DoubleQuotes(""));												// 38. Charge Code					 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 39. Contract No					 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 40. Delivery Mode				 (Not Applicable)
			builder.Append(DoubleQuotes(""));												// 41. Special Flag					 (Not Applicable)
			Writer.WriteLine(builder.ToStringWithDelimiterBetweenAppends(separator));

			AccTransactionMatchLink matchLink = row.Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, row.PK));

			if (matchLink != null)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionMatchLink), AccTransactionMatchLinkSchema.AP_AH);
				subQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLink.AP_MatchGroupNum);
				query.AddSubQuery(AccTransactionHeaderSchema.PK, AccTransactionMatchLinkSchema.AP_AH, subQuery, JoinCondition.And);

				TransactionHeader[] transactionHeaders = row.Factory.Load<TransactionHeader>(query);

				// export everything except the matchlink row for the payment itself
				foreach (TransactionHeader header in transactionHeaders)
				{
					if (header.PK != row.PK)
					{
						builder = new ZStringBuilder();
						builder.Append(DoubleQuotes(""));
						builder.Append(DoubleQuotes("GI"));
						builder.Append(DoubleQuotes("MIS"));
						builder.Append(DoubleQuotes("A"));

						IMatching matching = header as IMatching;
						TransactionMatchLink link = row.Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, header.PK));
						ZDecimal oSPaidAmount = header.AH_OSTotal * (-link.AP_Amount / (header.AH_InvoiceAmount + header.AH_GSTAmount));
						string oSPaidAmountString = oSPaidAmount.ToString(string.Format("F{0}", header.TransactionCurrency.Decimals));
						string invoiceType = oSPaidAmount >= 0 ? "PI" : "PC";

						ZString additionalDescription = string.Format("{0}/{1}/{2}/{3}/{4}/{5}",
											 valueDate,
											 invoiceType,
											 "CL",
											 header.AH_Desc,
											 header.AH_RX_NKTransactionCurrency,
											 oSPaidAmountString);

						builder.Append(DoubleQuotes(additionalDescription));
						Writer.WriteLine(builder.ToStringWithDelimiterBetweenAppends(separator));
					}
				}
			}
		}

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
		}

		static string DoubleQuotes(ZString fieldvalue)
		{
			return "\"" + fieldvalue + "\"";
		}

		static string DoubleQuotes(ZString fieldvalue, int maximumWidth)
		{
			if (fieldvalue.Length > maximumWidth)
			{
				fieldvalue = fieldvalue.SubstringSafe(0, maximumWidth);
			}

			return "\"" + fieldvalue + "\"";
		}
	}
}
