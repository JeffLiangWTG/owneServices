using System;
using CargoWise.Common;

namespace Enterprise.DataConverters.Accounting
{
	public abstract class AccountingConverterBase
	{
		public const string Account = "Account";
		public const string Reference = "Reference";
		public const string InvoiceDate = "InvoiceDate";
		public const string DueDate = "DueDate";
		public const string Currency = "Currency";
		public const string ForeignAmount = "ForeignAmount";
		public const string LocalAmount = "LocalAmount";
		public const string Branch = "Branch";
		public const string Department = "Department";

		protected bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 9
				&& string.Equals(line.FieldValues[0], Account, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[1], Reference, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[2], InvoiceDate, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[3], DueDate, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[4], Currency, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[5], ForeignAmount, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[6], LocalAmount, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[7], Branch, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(line.FieldValues[8], Department, StringComparison.OrdinalIgnoreCase);
		}

		protected string InvalidHeaderMessage()
		{
			return string.Format("File Header information is incorrect. The file should contain 9 columns in CSV format: \n\n{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} and {8}.\n\nPlease make sure that the column names are spelled correctly.", Account, Reference, InvoiceDate, DueDate, Currency, ForeignAmount, LocalAmount, Branch, Department);
		}
	}
}
