using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT
{
	public class PurchaseAndSalesVATWriter : VATDataFileWriter
	{
		public PurchaseAndSalesVATWriter(AccComplianceReport report) : base(report)
		{
		}

		int SequenceNumber { get; set; }

		internal override ComplianceDocumentHeaderDetails[] GetDocumentHeaderDetails()
		{
			SequenceNumber = 1;

			var collector = new ComplianceReportDocumentDataCollector(Report);
			var documentHeaderDetails = new List<ComplianceDocumentHeaderDetails>(collector.ComplianceDocumentHeader);
			documentHeaderDetails.AddRange(GetDocumentHeaderForComplianceSequence(collector));

			return (from documentHeaderDetail in documentHeaderDetails
					let formatCode = ComplianceDocumentHelper.GetFormatCode(documentHeaderDetail.ComplianceSubType, documentHeaderDetail.Ledger)
					orderby formatCode, documentHeaderDetail.DocumentNumber
					select documentHeaderDetail).ToArray();
		}

		ComplianceDocumentHeaderDetails[] GetDocumentHeaderForComplianceSequence(ComplianceReportDocumentDataCollector collector)
		{
			var result = new List<ComplianceDocumentHeaderDetails>();

			var sequenceDetails = collector.GetUnusedComplianceSequenceDetails();
			foreach (var sequence in sequenceDetails)
			{
				var headerDetail = new ComplianceDocumentHeaderDetails();
				headerDetail.Ledger = LedgerTypes.AccountsReceivable;
				headerDetail.ComplianceSubType = sequence.ComplianceSubType;
				headerDetail.ReportingPeriod = AccountingPeriodCalculator.GetPeriodFromDate(sequence.ExpiryDate);
				headerDetail.DocumentNumber = sequence.Prefix + sequence.NextNumber.ToStringTrimZeros(0).PadLeft(sequence.MaximumNumberDigits, '0');
				headerDetail.VATRegistrationNum = sequence.EndNumber.ToStringTrimZeros(0).PadLeft(sequence.MaximumNumberDigits, '0');

				headerDetail.ExTaxAmount = ZDecimal.Zero;
				headerDetail.TaxAmount = ZDecimal.Zero;

				result.Add(headerDetail);
			}

			return result.ToArray();
		}

		internal override string BuildDocumentData(ComplianceDocumentHeaderDetails headerDetail)
		{
			var result = new StringBuilder();
			var taxType = GetTaxType(headerDetail);
			result.Append(ComplianceDocumentHelper.GetFormatCode(headerDetail.ComplianceSubType, headerDetail.Ledger));
			result.Append(ProxyGTXRegistrationNumber);
			result.Append(SequenceNumber++.ToString(CultureInfo.InvariantCulture).PadLeft(7, '0'));
			result.Append(GetPeriodYear(headerDetail.ReportingPeriod));
			result.Append(GetPeriodMonth(headerDetail.ReportingPeriod));
			result.Append(GetDebtorRegistrationNumber(headerDetail));
			result.Append(GetCreditorRegistrationNumber(headerDetail.Ledger, headerDetail.VATRegistrationNum));
			result.Append(headerDetail.DocumentNumber);
			result.Append(GetExTaxAmount(headerDetail));
			result.Append(taxType);
			result.Append(GetTaxAmount(headerDetail));
			result.Append(GetDeductibleCode(headerDetail.Ledger, headerDetail.SupportingReason, taxType));
			result.Append(new ZString(' ', 5));
			result.Append(new ZString(' ', 1));
			result.Append(GetNote(taxType));
			result.Append(GetCustomRelated(headerDetail.Ledger, headerDetail.CustomRelated, taxType));
			return result.ToString();
		}

		#region Build Data

		ZString GetTaxType(ComplianceDocumentHeaderDetails headerDetail)
		{
			if (headerDetail.DocumentStatus == ComplianceDocumentStatus.Voided)
			{
				return "F";
			}

			if (headerDetail.LineRateCode == null || headerDetail.LineRateCode.Length == 0)
			{
				return "D";
			}

			if (headerDetail.LineRateCode.All(x => x == "FREEVAT"))
			{
				return "2";
			}
			else if (IsInvoiceWithNaturalPersonIndividualOrg(headerDetail))
			{
				return "1";
			}
			else
			{
				if (headerDetail.LineRateCode.All(x => x == "VAT" || x == "CAPVAT"))
				{
					return "1";
				}
				else if (headerDetail.LineRateCode.All(x => x == "EXEMPT"))
				{
					return "3";
				}
			}

			return new ZString(" ");
		}

		ZString GetDebtorRegistrationNumber(ComplianceDocumentHeaderDetails headerDetail)
		{
			return headerDetail.DocumentStatus == ComplianceDocumentStatus.Voided ? new ZString(' ', 8) : GetVATRegistrationNumber(headerDetail.Ledger == LedgerTypes.AccountsReceivable, headerDetail.VATRegistrationNum);
		}

		ZString GetCreditorRegistrationNumber(ZString ledger, ZString vatRegistrationNum)
		{
			return GetVATRegistrationNumber(ledger == LedgerTypes.AccountsPayable, vatRegistrationNum);
		}

		ZString GetTaxAmount(ComplianceDocumentHeaderDetails detail)
		{
			var taxAmount = ((detail.Ledger == LedgerTypes.AccountsReceivable && IsInvoiceWithNaturalPersonIndividualOrg(detail)) || IsAPWithDuplicateComplianceSubType(detail)) ? ZDecimal.Zero : detail.TaxAmount.Normalize();
			return Math.Abs(taxAmount).ToString(CultureInfo.InvariantCulture).PadLeft(10, '0');
		}

		internal override ZString GetExTaxAmount(ComplianceDocumentHeaderDetails detail)
		{
			if ((detail.Ledger == LedgerTypes.AccountsReceivable && IsInvoiceWithNaturalPersonIndividualOrg(detail)) || IsAPWithDuplicateComplianceSubType(detail))
			{
				ZDecimal totalAmount = detail.ExTaxAmount + detail.TaxAmount;
				return Math.Abs(totalAmount.Normalize()).ToString(CultureInfo.InvariantCulture).PadLeft(12, '0');
			}

			return base.GetExTaxAmount(detail);
		}

		ZString GetDeductibleCode(ZString ledger, ZString supportingReason, ZString taxType)
		{
			if (!supportingReason.IsEmpty && taxType != "F" && ledger == LedgerTypes.AccountsPayable)
			{
				return supportingReason.Substring(0, 1);
			}
			else
			{
				return new ZString(" ");
			}
		}

		ZString GetNote(ZString taxType)
		{
			if (taxType == "D")
			{
				return new ZString("A");
			}
			else
			{
				return new ZString(" ");
			}
		}

		ZString GetCustomRelated(ZString ledger, ZBool customRelated, ZString taxType)
		{
			if (ledger == LedgerTypes.AccountsReceivable && taxType == "2")
			{
				return customRelated ? "2" : "1";
			}
			else
			{
				return new ZString(" ");
			}
		}

		#endregion

		bool IsInvoiceWithNaturalPersonIndividualOrg(ComplianceDocumentHeaderDetails detail)
		{
			return (detail.OrgHeaderCategory == OrgConstants.Category.NaturalPersonIndividual
					|| (detail.OrgHeaderCountryCode != CountryCodes.Taiwan && detail.OrgHeaderCategory == OrgConstants.Category.Business))
				&& detail.TransactionType == TransactionTypes.Invoice;
		}

		bool IsAPWithDuplicateComplianceSubType(ComplianceDocumentHeaderDetails detail)
		{
			var duplicateComplianceSubTypeList = new List<string>
			{
				TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI,
				TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC,
				TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP
			};

			return detail.Ledger == LedgerTypes.AccountsPayable && duplicateComplianceSubTypeList.Contains(detail.ComplianceSubType);
		}
	}
}
