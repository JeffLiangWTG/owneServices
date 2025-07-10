using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public class WestpacRecord
	{
		public WestpacRecord(string line, FlatFileFormat format)
		{
			FlatFileDataRow row = format.ConvertToRow(line);
			if (row.FieldCount < MinFieldCount)
			{
				throw new FormatException("Each line of Westpac file is expected to have at least " + MinFieldCount.ToString() + " fields.");
			}

			IsNotSupported = row.GetField(31) == "<<<+-+>>>";
			if (!IsNotSupported)
			{
				IsDebitLine = !row.GetField(37).IsEmpty;
				Amount = IsDebitLine ? Math.Abs(row.GetFieldAsZDecimal(37, 2)) : -Math.Abs(row.GetFieldAsZDecimal(38, 2));

				Type = GetMappedType(row.GetField(32).Trim());

				Reference = GetXmlFriendlyString(row.GetField(35));
			}
		}

		public readonly ZString Type;
		public readonly ZString Reference;
		public readonly ZDecimal Amount;

		public readonly bool IsNotSupported;

		#region Implementation

		const int MinFieldCount = 39;
		readonly bool IsDebitLine;

		#region GetXmlFriendlyString

		ZString GetXmlFriendlyString(ZString field)
		{
			return field.Trim().Replace("&", "&amp;").Replace("<", "&lt;");
		}

		#endregion

		#region GetMappedType

		ZString GetMappedType(string type)
		{
			ZString result = "";

			switch (type)
			{
				case "AP":
				case "PS":
					result = IsDebitLine ? ReceiptTypes.EFT : TransactionTypes.ReceiptBatch;
					break;

				case "CR":
					result = TransactionTypes.ReceiptBatch;
					break;

				case "DC":
					result = IsDebitLine ? ReceiptTypes.DirectDebit : ReceiptTypes.DirectCredit;
					break;

				case "":
					result = IsDebitLine ? ReceiptTypes.Cheque : TransactionTypes.ReceiptBatch;
					break;
			}

			return result;
		}

		#endregion

		#endregion
	}
}
