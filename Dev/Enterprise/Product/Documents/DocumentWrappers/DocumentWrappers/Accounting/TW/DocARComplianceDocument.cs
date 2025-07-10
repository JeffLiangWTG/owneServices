using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentWrappers.TW
{
	[CodeAlive("Used in Document")]
	public class DocARComplianceDocument : DocumentWrappers.DocARComplianceDocument
	{
		DocARComplianceDocument(ARComplianceDocumentHeader arComplianceDocumentHeader, BusinessObjectFactory factory)
		: base(arComplianceDocumentHeader, factory)
		{
		}

		public new static DocARComplianceDocument New(ARComplianceDocumentHeader arComplianceDocumentHeader, BusinessObjectFactory factory)
		{
			if (arComplianceDocumentHeader == null)
			{
				return null;
			}
			return new DocARComplianceDocument(arComplianceDocumentHeader, factory);
		}

		public override ZString InvoiceDateString => Header.ADH_DocumentDate.AddYears(-1911).ToString("yyy年MM月dd日", CultureInfo.InvariantCulture);

		public ZString ComplianceBookValidityPeriodString
		{
			get
			{
				var result = ZString.Empty;
				var complianceDocumentDate = Header.ADH_DocumentDate;
				if (!complianceDocumentDate.IsEmpty)
				{
					var complianceDocumentMonth = complianceDocumentDate.Month;
					ZInt startMonth;
					ZInt endMonth;
					if ((complianceDocumentMonth & 1) == 1)
					{
						startMonth = complianceDocumentMonth;
						endMonth = complianceDocumentMonth + 1;
					}
					else
					{
						startMonth = complianceDocumentMonth - 1;
						endMonth = complianceDocumentMonth;
					}
					var year = complianceDocumentDate.Year - 1911;
					result = String.Format(CultureInfo.InvariantCulture, (NoResString)"{0:D3}年 {1:D2}-{2:D2}月", year, startMonth, endMonth);
				}
				return result;
			}
		}

		public ZString InvoiceDateAsUniversalString => Header.ADH_DocumentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		public override ZString InternalReference => Header.ADH_DocumentDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + base.InternalReference;

		const int MaxChargeLineCount = 7;

		public DocARComplianceDocumentLineCollection ElectronicCreditNoteChargeLines => base.GetChargeLines();

		protected override DocARComplianceDocumentLineCollection GetChargeLines()
		{
			var result = new DocARComplianceDocumentLineCollection(Factory);

			if (Header != null)
			{
				var documentLines = Header.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().OrderBy(x => x.ADL_Sequence).ToList();
				var count = documentLines.Count <= MaxChargeLineCount ? MaxChargeLineCount : MaxChargeLineCount - 1;
				foreach (var line in documentLines.Take(count))
				{
					var docLine = DocARComplianceDocumentLine.New(line, Factory);
					result.Add(docLine);
				}

				if (documentLines.Count > MaxChargeLineCount)
				{
					var newFactory = new BusinessObjectFactory();
					var line = newFactory.New<AccComplianceDocumentLine>();
					line.ADL_Description = (NoResString)"其他費用";
					foreach (var otherLine in documentLines.Skip(MaxChargeLineCount - 1))
					{
						line.TransactionLines.AddRange(otherLine.TransactionLines);
					}

					var lastDocLine = DocARComplianceDocumentLine.New(line, Factory);
					result.Add(lastDocLine);
				}

				if (result.Count < MaxChargeLineCount)
				{
					var addCount = MaxChargeLineCount - result.Count;
					result.AddRange(GetEmptyLinesForComplianceDocumentLineCollection(addCount));
				}
			}

			return result;
		}

		#region ElectronicGUI

		const int MaxRemarkLineCount = 10;

		public DocARComplianceDocumentLineCollection ElectronicGUIChargeLinesFirstPart => GetLineCollectionForElectronicGUI(base.GetChargeLines().Take(MaxRemarkLineCount));

		public DocARComplianceDocumentLineCollection ElectronicGUIChargeLinesSecondPart => GetLineCollectionForElectronicGUI(base.GetChargeLines().Skip(MaxRemarkLineCount));

		DocARComplianceDocumentLineCollection GetLineCollectionForElectronicGUI(IEnumerable<BusinessObject> lineCollection)
		{
			var result = new DocARComplianceDocumentLineCollection(Factory);
			result.AddRange(lineCollection);
			return result;
		}

		#endregion

		public DocARComplianceDocumentLineCollection ElectronicGUIChargeLinesForPaper => base.GetChargeLines();

		DocARComplianceDocumentLine[] GetEmptyLinesForComplianceDocumentLineCollection(ZInt addCount)
		{
			var result = new List<DocARComplianceDocumentLine>();
			var newFactory = new BusinessObjectFactory();
			for (int i = 0; i < addCount; i++)
			{
				var newLine = newFactory.New<AccComplianceDocumentLine>();
				result.Add(DocARComplianceDocumentLine.New(newLine, Factory));
			}
			return result.ToArray();
		}

		public ZString ChargeLinesDescription
		{
			get
			{
				if (chargeLinesDescription.IsEmpty)
				{
					foreach (DocARComplianceDocumentLine line in GetChargeLines())
					{
						chargeLinesDescription += line.Description + System.Environment.NewLine;
					}
				}
				return chargeLinesDescription;
			}
		}
		ZString chargeLinesDescription;

		public ZString ChargeLinesAmount
		{
			get
			{
				if (chargeLinesAmount.IsEmpty)
				{
					foreach (DocARComplianceDocumentLine line in GetChargeLines())
					{
						if (line.LocalAmount > 0)
						{
							chargeLinesAmount += string.Format(CultureInfo.InvariantCulture, "{0}{1}", line.LocalAmount.ToStringTrimZeros("N0"), System.Environment.NewLine);
						}
					}
				}
				return chargeLinesAmount;
			}
		}
		ZString chargeLinesAmount;

		public ZString SumAmountStr => string.Format(CultureInfo.InvariantCulture, "{0}", Header.Amount.ToStringTrimZeros());

		public ZString CompanyPhoneAndFaxNumStr
		{
			get
			{
				if (!CompanyPhoneNum.IsEmpty && !CompanyFaxNum.IsEmpty)
				{
					return string.Format(CultureInfo.InvariantCulture, "TEL:{0}/FAX:{1}", CompanyPhoneNum, CompanyFaxNum);
				}
				else if (!CompanyPhoneNum.IsEmpty)
				{
					return string.Format(CultureInfo.InvariantCulture, "TEL:{0}", CompanyPhoneNum);
				}
				else if (!CompanyFaxNum.IsEmpty)
				{
					return string.Format(CultureInfo.InvariantCulture, "FAX:{0}", CompanyFaxNum);
				}
				return ZString.Empty;
			}
		}

		public ZBool Taxable => Header.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().Any(x => x.TaxRate != null && (x.TaxRate.AT_Code == "VAT" || x.TaxRate.AT_Code == "CAPVAT"));

		public ZBool ZeroRated => Header.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().Any(x => x.TaxRate != null && x.TaxRate.AT_Code == "FREEVAT");

		public ZBool TaxExempt => Header.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().Any(x => x.TaxRate != null && x.TaxRate.AT_Code == "EXEMPT");

		public ZString BarCode
		{
			get
			{
				if (Header.ADH_Ledger == LedgerTypes.AccountsReceivable && Header.ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE
					&& Header.ComplianceBook != null && !Header.ComplianceBook.XD_ExpiryDate.IsEmpty)
				{
					var builder = new ZStringBuilder();
					var reportingPeriod = FormattableString.Invariant($"{Header.ComplianceBook.XD_ExpiryDate.Year - 1911:D3}{Header.ComplianceBook.XD_ExpiryDate.Month:D2}");
					builder.Append("*");
					builder.Append(reportingPeriod);
					builder.Append(Header.ADH_DocumentNumber);
					if (DebtorCategory == OrgConstants.Category.NaturalPersonIndividual)
					{
						builder.Append(Header.ADH_BarCode);
					}
					builder.Append("*");

					return builder.ToString();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		static byte[] ConvertHexToByte(string hexString)
		{
			var array = new byte[hexString.Length / 2];
			var num = 0;
			for (var i = 0; i < hexString.Length; i += 2)
			{
				var value = Convert.ToInt32(hexString.Substring(i, 2), 16);
				array[num] = BitConverter.GetBytes(value)[0];
				num++;
			}
			return array;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		static string AESEncrypt(string plainText, string aesKey)
		{
			var bytes = Encoding.Default.GetBytes(plainText);
			// SYSLIB0022: Replaced RigndaelManaged with Aes
			using (var aes = Aes.Create())
			{
				aes.KeySize = 128;
				aes.Key = ConvertHexToByte(aesKey);
				aes.BlockSize = 128;
				aes.IV = Convert.FromBase64String("Dt8lyToo17X/XkXaQvihuA=="); // AES IV

				using (var transform = aes.CreateEncryptor())
				using (var memoryStream = new MemoryStream())
				using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
				{
					cryptoStream.Write(bytes, 0, bytes.Length);
					cryptoStream.FlushFinalBlock();
					var inArray = memoryStream.ToArray();
					return Convert.ToBase64String(inArray);
				}
			}
		}

		(ZString qrLeftCode, ZString qrRightCode) GenerateQRCode()
		{
			var key = AccountingMasterFilesRegistry.Instance.AESEncryptionKey.Value;
			if (string.IsNullOrEmpty(key) || Header.ADH_ComplianceSubType != TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE || Header.ADH_Ledger != LedgerTypes.AccountsReceivable || CompanyVATRegNum.IsEmpty || Header.TransactionHeaders.OfType<AccTransactionHeader>().Any(x => x.AH_TransactionType != TransactionTypes.Invoice))
			{
				return (string.Empty, string.Empty);
			}

			var qrLeftBuilder = new StringBuilder();

			var invoiceDate = FormattableString.Invariant($"{Header.ADH_DocumentDate.Year - 1911:D3}{Header.ADH_DocumentDate.Month:D2}{Header.ADH_DocumentDate.Day:D2}");
			var randomNumber = DebtorCategory == OrgConstants.Category.NaturalPersonIndividual ? Header.ADH_BarCode : new ZString("    ");
			var debtorRegistrationNumber = DebtorVATRegNum.IsEmpty ? new ZString("00000000") : DebtorVATRegNum;
			var encrypterResult = AESEncrypt(Header.ADH_DocumentNumber + randomNumber, key);

			qrLeftBuilder.Append(Header.ADH_DocumentNumber);
			qrLeftBuilder.Append(invoiceDate);
			qrLeftBuilder.Append(randomNumber);
			qrLeftBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:X8}", Header.Amount.ToZInt());
			qrLeftBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:X8}", Header.TotalAmount.ToZInt());
			qrLeftBuilder.Append(debtorRegistrationNumber);
			qrLeftBuilder.Append(CompanyVATRegNum);
			qrLeftBuilder.Append(encrypterResult);

			qrLeftBuilder.Append(":**********");
			var lineCount = Header.ComplianceDocumentLines.Count.ToString(CultureInfo.InvariantCulture);
			qrLeftBuilder.Append(":" + lineCount);
			qrLeftBuilder.Append(":" + lineCount);
			qrLeftBuilder.Append(":1");
			qrLeftBuilder.Append(":" + Header.ComplianceDocumentLines[0].ADL_Description.Replace(":", ""));
			qrLeftBuilder.Append(":1");
			qrLeftBuilder.Append(":" + Header.ComplianceDocumentLines[0].Amount.ToStringTrimZeros());
			qrLeftBuilder.Append(":");

			var qrRightBuilder = new ZStringBuilder();
			for (int i = 1; i < Header.ComplianceDocumentLines.Count; i++)
			{
				qrRightBuilder.Append(":" + Header.ComplianceDocumentLines[i].ADL_Description.Replace(":", ""));
				qrRightBuilder.Append(":1");
				qrRightBuilder.Append(":" + Header.ComplianceDocumentLines[i].Amount.ToStringTrimZeros());
			}

			return (qrLeftBuilder.ToString(), ZString.Format("**{0}", qrRightBuilder.ToString().TrimStart(':')));
		}

		(string qrCodeLeft, string qrCodeRight) QRCode
		{
			get
			{
				if (qrCode == default((string, string)))
				{
					qrCode = GenerateQRCode();
				}

				return qrCode;
			}
		}

		(string qrCodeLeft, string qrCodeRight) qrCode;

		public ZString QRCodeLeft => QRCode.qrCodeLeft;

		public ZString QRCodeRight => QRCode.qrCodeRight;

		public ZString DebtorVATRegNumForCRD
		{
			get
			{
				if (!string.IsNullOrEmpty(Header.OrgHeaderCategory) && Header.OrgHeaderCategory != OrgConstants.Category.NaturalPersonIndividual)
				{
					return Header.VATRegistrationNum;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZDateTime OriginalGUIDocumentDate
		{
			get
			{
				if (Header.INVComplianceDocumentHeaderForCRD != null)
				{
					return Header.INVComplianceDocumentHeaderForCRD.ADH_DocumentDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}
	}
}
