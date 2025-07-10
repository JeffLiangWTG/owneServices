using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class WNZDDRFileGenerator : DDRFileGenerator
	{
		public WNZDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected const string Separator = ",";
		protected const string HeaderRecordType = "A";
		protected const string DetailRecordType = "D";

		#region Header

		protected override void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
			string originBank = "03";
			string originBranch = row.BankAccount.AB_BSB.Replace("-", "").SubstringSafe(2, 4);
			string description = "REMITTANCE";

			StringBuilder builder = new StringBuilder();
			builder.Append(HeaderRecordType + Separator);
			builder.Append("1" + Separator);
			builder.Append(originBank + Separator);
			builder.Append(originBranch + Separator);
			builder.Append(Separator);
			builder.Append(Separator);
			builder.Append(description + Separator);
			builder.Append(Env.Time.CurrentLocalDate.ToString(DateFormat) + Separator);
			Writer.WriteLine(builder.ToString());
		}

		#endregion

		#region Detail

		protected override void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			ZString payeeBSB = GetPayeeBankBSB(row).Replace("-", "");
			ZString payeeAccountNoIncludingSuffix = GetPayeeBankAccountNumber(row);

			ZString payeeSuffix = payeeAccountNoIncludingSuffix.SubstringSafe(payeeAccountNoIncludingSuffix.Length - 3);
			ZString payeeAccountNo = payeeAccountNoIncludingSuffix.SubstringSafe(0, payeeAccountNoIncludingSuffix.Length - 3);

			string mTSSource = "DC";
			string payeeParticulars = "REMITTANCE";
			ZString remitterBSB = Header.BankAccount.AB_BSB.Replace("-", "");
			ZString remitterAccountNumberIncludingSuffix = Header.BankAccount.AB_AccountNum;

			ZString remitterSuffix = remitterAccountNumberIncludingSuffix.SubstringSafe(remitterAccountNumberIncludingSuffix.Length - 3);
			ZString remitterBankAccount = remitterAccountNumberIncludingSuffix.SubstringSafe(0, remitterAccountNumberIncludingSuffix.Length - 3);
			string remitterName = FitToWidth(row.BankAccount.AB_BankAccountName, 20);
			ZDecimal totalAmount = IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount;

			StringBuilder builder = new StringBuilder();

			builder.Append(DetailRecordType + Separator);
			builder.Append(recordCount + Separator);
			builder.Append(payeeBSB.SubstringSafe(0, 2) + Separator);
			builder.Append(payeeBSB.SubstringSafe(2, 4) + Separator);
			builder.Append(payeeAccountNo + Separator);
			builder.Append(FitToWidth(payeeSuffix, 4).Trim() + Separator);
			builder.Append("50" + Separator);
			builder.Append(mTSSource + Separator);
			builder.Append(GetFormattedAmountStringCore(totalAmount.ToString(2), 15, ' ').Trim() + Separator);
			builder.Append(FitToWidth(GetAccountTitle(row), 20).Trim() + Separator);
			builder.Append(FitToWidth(payeeParticulars, 12).Trim() + Separator);
			builder.Append(FitToWidth(row.AH_ChequeOrReference, 12).Trim() + Separator);
			builder.Append(Separator);
			builder.Append(remitterBSB.SubstringSafe(0, 2) + Separator);
			builder.Append(remitterBSB.SubstringSafe(2, 4) + Separator);
			builder.Append(remitterBankAccount + Separator);
			builder.Append(FitToWidth(remitterSuffix, 4).Trim() + Separator);
			builder.Append((remitterName.Length >= 20 ? remitterName : remitterName.Trim()) + Separator);
			Writer.WriteLine(builder.ToString());
		}

		#endregion

		#region Trailer

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			// does nothing
		}

		#endregion
	}
}
