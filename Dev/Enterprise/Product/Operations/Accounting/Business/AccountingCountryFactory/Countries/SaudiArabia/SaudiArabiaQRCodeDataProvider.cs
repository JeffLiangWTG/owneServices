using System;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class SaudiArabiaQRCodeDataProvider : IQRCodeDataProvider
	{
		public string GetTransactionQRCodeString(InvoicingBase invoice)
		{
			var qrCodeData = string.Empty;
			var useLocalValue = false;

			if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				useLocalValue = true;
			}
			else
			{
				var eInvoicingTransactionPivot = invoice.GetMostRecentEInvoicingTransactionPivot();
				if (eInvoicingTransactionPivot == null || eInvoicingTransactionPivot.AIP_Status == EInvoicingPivotState.Discarded)
				{
					useLocalValue = true;
				}
			}

			if (useLocalValue)
			{
				var vatRegistrationNumber = GetVATRegistrationNumber(invoice);
				if (!vatRegistrationNumber.IsEmpty)
				{
					qrCodeData = EncodeQRCodeDataInTLV_UTF8_Base64(
						invoice?.Branch?.OrgProxy?.OH_FullName ?? ZString.Empty,
						vatRegistrationNumber,
						invoice.AH_InvoiceDate,
						invoice.AH_OSTotal,
						invoice.AH_OSTaxAmount
					);
				}
			}
			else
			{
				var transactionHeaderAuthorisationRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoice.Factory, invoice.PK, invoice.Company.Country.RN_Code);
				qrCodeData = transactionHeaderAuthorisationRecord?.AHF_VerificationUrl;
			}

			return qrCodeData;
		}

		ZString GetVATRegistrationNumber(InvoicingBase invoice) => invoice?.Branch?.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, CountryCodes.SaudiArabia) ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		ZString EncodeQRCodeDataInTLV_UTF8_Base64(ZString name, ZString num, ZDateTime invoiceDate, ZDecimal totalInvoiceAmount, ZDecimal vatTax)
		{
			byte[][] fields = {
				Encoding.UTF8.GetBytes(name),
				Encoding.UTF8.GetBytes(num),
				Encoding.UTF8.GetBytes(invoiceDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				Encoding.UTF8.GetBytes(new ZDecimal(Math.Abs(Math.Round(totalInvoiceAmount, 2))).ToString(2)),
				Encoding.UTF8.GetBytes(new ZDecimal(Math.Abs(Math.Round(vatTax, 2))).ToString(2))
			};
			byte[] field_labels = { 1, 2, 3, 4, 5 };

			byte[] data = Array.Empty<byte>();
			foreach (var (segment, label) in fields.Zip(field_labels, (x, y) => (x, y)))
			{
				data = data.Concat(new byte[] { label, (byte)segment.GetLength(0) }).ToArray();
				data = data.Concat(segment).ToArray();
			}

			return Convert.ToBase64String(data);
		}
	}
}
