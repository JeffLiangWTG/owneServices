using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public class ANZRecord
	{
		public ANZRecord(string line, FlatFileFormat format)
		{
			FlatFileDataRow row = format.ConvertToRow(line);
			if (row.FieldCount < MinFieldCount)
			{
				throw new FormatException("Each line of ANZ file is expected to have at least " + MinFieldCount.ToString() + " fields.");
			}

			IsNotSupported = row.GetField(0) != "3";
			if (!IsNotSupported)
			{
				Amount = -row.GetFieldAsZDecimal(3, 2);
				IsDebitLine = Amount >= 0;

				Type = GetMappedType(row.GetField(6).Trim());

				Reference = GetXmlFriendlyString(row.GetField(9));
			}
		}

		public readonly ZString Type;
		public readonly ZString Reference;
		public readonly ZDecimal Amount;

		public readonly bool IsNotSupported;

		#region Implementation

		const int MinFieldCount = 10;
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
					result = ReceiptTypes.EFT;
					break;

				case "DC":
					result = IsDebitLine ? ReceiptTypes.DirectDebit : ReceiptTypes.DirectCredit;
					break;

				default:
					result = IsDebitLine ? ReceiptTypes.Cheque : TransactionTypes.ReceiptBatch;
					break;
			}

			return result;
		}

		#endregion

		#endregion
	}
}