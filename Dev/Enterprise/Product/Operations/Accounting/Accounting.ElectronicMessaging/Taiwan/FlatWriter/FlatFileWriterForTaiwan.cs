using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class FlatFileWriterForTaiwan
	{
		public void WriteFlatFileToStream(ComplianceDocumentHeaderDetail[] complianceDocumentHeaderDetails, Stream stream, INotifications notifications)
		{
			using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
			{
				foreach (var headerDetail in complianceDocumentHeaderDetails)
				{
					writer.WriteLine(GetHeaderDetailString(headerDetail, notifications));

					foreach (var lineDetail in headerDetail.ComplianceDocumentLineDetails)
					{
						writer.WriteLine(GetLineDetailString(headerDetail, lineDetail));
					}
				}
				writer.Write(complianceDocumentHeaderDetails.Length);
			}
		}

		#region Header Detail

		bool IsB2BCategory(OrgHeaderDetail orgHeaderDetail) => orgHeaderDetail.CountryCode == CountryCodes.Taiwan && orgHeaderDetail.Category == OrgConstants.Category.Business;
		bool IsB2CCategory(OrgHeaderDetail orgHeaderDetail) => orgHeaderDetail.Category == OrgConstants.Category.NaturalPersonIndividual
			|| (orgHeaderDetail.CountryCode != CountryCodes.Taiwan && orgHeaderDetail.Category == OrgConstants.Category.Business);

		ZString GetHeaderDetailString(ComplianceDocumentHeaderDetail headerDetail, INotifications notifications)
		{
			var headerDetailString = new ZStringBuilder();
			headerDetailString.Append("M");
			headerDetailString.Append(GetBillType(headerDetail, notifications));
			headerDetailString.Append(GetDocumentNumber(headerDetail, TransactionTypes.Invoice));
			headerDetailString.Append(GetDocumentDate(headerDetail, TransactionTypes.Invoice));
			headerDetailString.Append(GetDocumentDate(headerDetail, TransactionTypes.CreditNote));
			headerDetailString.Append(GetVoidDate(headerDetail));
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(headerDetail.SystemVATRegistrationNum);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(IsB2BCategory(headerDetail.OrgHeaderDetail) ? headerDetail.OrgHeaderDetail.VATRegistrationNum : ZString.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(headerDetail.OrgHeaderDetail.CompanyName);
			headerDetailString.Append(GetAmount(headerDetail));
			var taxType = GetTaxType(headerDetail);
			headerDetailString.Append(taxType);
			headerDetailString.Append(GetTaxRate(headerDetail));
			headerDetailString.Append(GetTaxAmount(headerDetail));
			headerDetailString.Append(Math.Abs(headerDetail.TotalAmount.Normalize()).ToString(CultureInfo.InvariantCulture));
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(GetDescription(headerDetail, notifications));
			headerDetailString.Append(GetInternalReference(headerDetail));
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(GetCustomsClearanceMark(headerDetail, taxType));
			headerDetailString.Append(string.Empty);
			headerDetailString.Append(IsB2BCategory(headerDetail.OrgHeaderDetail) ? ZString.Empty : headerDetail.BarCode);

			var mciRegistrationNum = GetMCIRegistrationNum(headerDetail.OrgHeaderDetail);
			var pigRegistrationNum = GetPIGRegistrationNum(headerDetail.OrgHeaderDetail);
			headerDetailString.Append(mciRegistrationNum.IsEmpty ? string.Empty : CarrierType);
			headerDetailString.Append(mciRegistrationNum);
			headerDetailString.Append(pigRegistrationNum);
			headerDetailString.Append(!mciRegistrationNum.IsEmpty || !pigRegistrationNum.IsEmpty ? "N" : "Y");

			if (headerDetail.IsSpecialVoiding)
			{
				headerDetailString.Append(headerDetail.VoidingReason);
				headerDetailString.Append(headerDetail.ApprovalNumber);
			}

			return GetStringReplaceLineBreak(headerDetailString.ToStringWithDelimiterBetweenAppends("|"));
		}

		const string CarrierType = "3J0002";

		ZString GetBillType(ComplianceDocumentHeaderDetail headerDetail, INotifications notifications)
		{
			switch (headerDetail.TransactionType)
			{
				case TransactionTypes.Invoice:
					return headerDetail.VoidDate.HasValue ? "C" : "O";
				case TransactionTypes.CreditNote:
					return headerDetail.VoidDate.HasValue ? "D" : "A2";
				default:
					notifications.AddError(Res.GetString("65069256-0d1a-4d12-bf07-2a72fa66cdbf", "Failed to upload compliance document because {0} has Transaction Type {1}", headerDetail.DocumentNumber, headerDetail.TransactionType));
					return "";
			}
		}

		ZString GetDocumentNumber(ComplianceDocumentHeaderDetail headerDetail, string transactionType)
		{
			if (headerDetail.TransactionType == transactionType)
			{
				return headerDetail.DocumentNumber;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetDocumentDate(ComplianceDocumentHeaderDetail headerDetail, string transactionType)
		{
			if (headerDetail.TransactionType == transactionType)
			{
				if (transactionType == TransactionTypes.Invoice)
				{
					return headerDetail.DocumentDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
				}
				else if (transactionType == TransactionTypes.CreditNote)
				{
					return headerDetail.DocumentDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				}
			}

			return ZString.Empty;
		}

		ZString GetVoidDate(ComplianceDocumentHeaderDetail headerDetail)
		{
			if (headerDetail.VoidDate.HasValue)
			{
				return headerDetail.VoidDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetAmount(ComplianceDocumentHeaderDetail headerDetail)
		{
			if (headerDetail.TransactionType == TransactionTypes.Invoice && IsB2CCategory(headerDetail.OrgHeaderDetail))
			{
				return Math.Abs(headerDetail.TotalAmount.Normalize()).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				return Math.Abs(headerDetail.Amount.Normalize()).ToString(CultureInfo.InvariantCulture);
			}
		}

		ZString GetTaxAmount(ComplianceDocumentHeaderDetail headerDetail)
		{
			if (headerDetail.TransactionType == TransactionTypes.Invoice && IsB2CCategory(headerDetail.OrgHeaderDetail))
			{
				return "0";
			}
			else
			{
				return Math.Abs(headerDetail.TaxAmount.Normalize()).ToString(CultureInfo.InvariantCulture);
			}
		}

		const string VATTaxType = "1";
		const string FreeTaxType = "2";
		const string ExemptTaxType = "3";
		ZString GetTaxType(ComplianceDocumentHeaderDetail headerDetail)
		{
			if (headerDetail.ComplianceDocumentLineDetails.Any(x => x.TaxCode == "VAT"))
			{
				return VATTaxType;
			}
			else if (headerDetail.ComplianceDocumentLineDetails.Any(x => x.TaxCode == "FREEVAT"))
			{
				return FreeTaxType;
			}
			else if (headerDetail.ComplianceDocumentLineDetails.Any(x => x.TaxCode == "EXEMPT"))
			{
				return ExemptTaxType;
			}
			else
			{
				return "";
			}
		}

		ZString GetTaxRate(ComplianceDocumentHeaderDetail headerDetail)
		{
			return new ZDecimal(headerDetail.ComplianceDocumentLineDetails.First().Rate / 100).ToStringTrimZeros();
		}

		ZString GetDescription(ComplianceDocumentHeaderDetail headerDetail, INotifications notifications)
		{
			if (headerDetail.Description.Contains('|'))
			{
				notifications.AddError(Res.GetString("7a6504de-603b-47d1-8496-b4a52ce90496", "Failed to upload compliance document because {0} has description which contains '|'", headerDetail.DocumentNumber));
			}
			return headerDetail.Description;
		}

		ZString GetInternalReference(ComplianceDocumentHeaderDetail headerDetail)
		{
			return FormattableString.Invariant($"{headerDetail.DocumentDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}{headerDetail.InternalReference}"); // upload content should be English
		}

		ZString GetCustomsClearanceMark(ComplianceDocumentHeaderDetail headerDetail, ZString taxType)
		{
			if (taxType == FreeTaxType && headerDetail.TransactionType == TransactionTypes.Invoice)
			{
				return "2";
			}
			else
			{
				return "";
			}
		}

		ZString GetMCIRegistrationNum(OrgHeaderDetail orgHeaderDetail)
		{
			if (IsB2CCategory(orgHeaderDetail))
			{
				return orgHeaderDetail.MCIRegistrationNum;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetPIGRegistrationNum(OrgHeaderDetail orgHeaderDetail)
		{
			if (IsB2CCategory(orgHeaderDetail))
			{
				return orgHeaderDetail.PIGRegistrationNum;
			}
			else
			{
				return ZString.Empty;
			}
		}

		#endregion

		#region Line Detail

		ZString GetLineDetailString(ComplianceDocumentHeaderDetail headerDetail, ComplianceDocumentLineDetail lineDetail)
		{
			var lineDetailString = new ZStringBuilder();
			lineDetailString.Append("D");
			lineDetailString.Append(lineDetail.LineDescription);
			lineDetailString.Append("1");
			lineDetailString.Append(GetAmount(headerDetail, lineDetail));
			lineDetailString.Append(GetAmount(headerDetail, lineDetail));
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append("0");
			lineDetailString.Append("0");
			lineDetailString.Append(GetInternalReference(headerDetail));
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(GetDocumentNumber(headerDetail, TransactionTypes.CreditNote));
			lineDetailString.Append(GetOriginalDocumentDate(headerDetail));
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);
			lineDetailString.Append(string.Empty);

			return GetStringReplaceLineBreak(lineDetailString.ToStringWithDelimiterBetweenAppends("|"));
		}

		ZString GetAmount(ComplianceDocumentHeaderDetail headerDetail, ComplianceDocumentLineDetail lineDetail)
		{
			if (IsB2CCategory(headerDetail.OrgHeaderDetail))
			{
				return Math.Abs(lineDetail.Amount.Normalize() + lineDetail.TaxAmount.Normalize()).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				return Math.Abs(lineDetail.Amount.Normalize()).ToString(CultureInfo.InvariantCulture);
			}
		}

		ZString GetOriginalDocumentDate(ComplianceDocumentHeaderDetail headerDetail)
		{
			if (headerDetail.TransactionType == TransactionTypes.CreditNote && headerDetail.OriginalDocumentDate.HasValue)
			{
				return headerDetail.OriginalDocumentDate.Value.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
			}

			return ZString.Empty;
		}

		#endregion

		ZString GetStringReplaceLineBreak(string str)
		{
			return str.Replace("\r\n", " ").Replace("\n", " ");
		}

#if DEBUG

		public ZString GetHeaderDetailString_ForTestOnly(ComplianceDocumentHeaderDetail headerDetail, INotifications notifications)
		{
			return GetHeaderDetailString(headerDetail, notifications);
		}

		public ZString GetLineDetailString_ForTestOnly(ComplianceDocumentHeaderDetail headerDetail, ComplianceDocumentLineDetail lineDetail)
		{
			return GetLineDetailString(headerDetail, lineDetail);
		}

		public ZString GetInternalReference_ForTestOnly(ComplianceDocumentHeaderDetail headerDetail)
		{
			return GetInternalReference(headerDetail);
		}

#endif
	}
}
