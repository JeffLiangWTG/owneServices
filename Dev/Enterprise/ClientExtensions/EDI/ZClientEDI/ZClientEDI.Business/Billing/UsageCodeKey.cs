using System;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Usage natural key.
	/// The category code and usage code uniquely identify the kind of billing usage.
	/// E.g. Category "STL", Code "SHP" is an STL shipment billing transaction.
	/// In the billing database, Category and Code are columns CH_Category and CH_PriceItemCode.
	/// In ClientChargeableUsage table, the column are U1_Code and U1_SubCode (the column names came before the Category and Code concept).
	/// On a pricelist, it's L7_Category and L7_Code.
	/// 
	/// Notes:
	/// 1. ClientMapping - the interface name can further identify the usage, since each interface
	/// can have it's own price.
	/// In the billing DB the category/code is "CMP"/"CMP" and the interface name is in Ref1
	/// In ClientChargeableUsage table, U1_Code is "CMP", and U1_SubCode is the interface name.
	/// On a pricelist, L7_Category and L7_Code is "CMP", L7_Ref4 is the interface name.
	/// </summary>
	public sealed class UsageCodeKey : Tuple<string, string>
	{
		public const int DisplayTextMaxLength = AutoClientLicencePriceItem.Schema.L7_CodeMaxLength + 1 + AutoClientLicencePriceItem.Schema.L7_CategoryMaxLength;

		/// <summary>
		/// Constructor. If given code is empty, then the code is set to the category.
		/// This is for when a category contains just one kind of usage so the category
		/// is sufficient to uniquely identify it.
		/// </summary>
		public UsageCodeKey(string category, string code) : base(category, !string.IsNullOrEmpty(code) ? code : category)
		{
		}

		#region case-insensitive methods

		public override bool Equals(object obj) => (obj is UsageCodeKey key) && EqualsIgnoringCase(key);

		public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode($"[{Category}]_[{Code}]");

		public bool EqualsIgnoringCase(UsageCodeKey other)
			=> string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase) &&
			   string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);

		#endregion

		const char DatabaseTextSeparator = '-';

		public static string ToCombinedText(string category, string code, char separator)
		{
			if (string.IsNullOrEmpty(category) && string.IsNullOrEmpty(code))
			{
				return string.Empty;
			}
			else
			{
				return category + separator + code;
			}
		}

		public static UsageCodeKey FromCombinedText(string categoryAndCode, char separator)
		{
			string category;
			string code;
			if (!string.IsNullOrEmpty(categoryAndCode))
			{
				var separatorIndex = categoryAndCode.IndexOf(separator);
				category = separatorIndex >= 0 ? categoryAndCode.Substring(0, separatorIndex) : categoryAndCode;
				code = separatorIndex >= 0 ? categoryAndCode.Substring(separatorIndex + 1) : string.Empty;
			}
			else
			{
				category = string.Empty;
				code = string.Empty;
			}
			return new UsageCodeKey(category, code);
		}

		/// <summary>
		/// Category and code stored in a single database text column.
		/// </summary>
		public string ToDatabaseText() => ToCombinedText(Category, Code, DatabaseTextSeparator);
		public static UsageCodeKey FromDatabaseText(string categoryAndCode) => FromCombinedText(categoryAndCode, DatabaseTextSeparator);

		public string Category => Item1;
		public string Code => Item2;
	}
}
