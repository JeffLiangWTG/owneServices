using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CA.Business
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceLine == null ? null : new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		public override ZString MergedLineNo
		{
			get
			{
				var entryLine = JobComInvoiceLine.B3EntryLine;
				return (entryLine == null) ? Res.GetString("3EAD5635-62D0-47F0-9FE0-E4C15EA9E2A9", "Not Merged") : entryLine.CL_LineNumber.ToString();
			}
		}

		public override ZShort MergedNumericLineNo
		{
			get
			{
				var entryLine = JobComInvoiceLine.B3EntryLine;
				return (entryLine == null) ? ZShort.Zero : entryLine.CL_LineNumber;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)CusEntryLineInternal; }
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public ZInt InvoicePageNo
		{
			get { return JobComInvoiceLine.CA_PageNumber; }
		}

		public ZInt InvoicePageRelativeLineNumber
		{
			get { return JobComInvoiceLine.CA_PageRelativeLineNumber; }
		}

		#region Tariff

		public ZString FormattedTariff
		{
			get { return JobComInvoiceLine.JI_FormattedTariff; }
		}

		public ZString TariffCode
		{
			get { return JobComInvoiceLine.CA_99TariffCode; }
		}

		#endregion

		#region Quantities

		public ZString InvoiceQuantityFormatted
		{
			get
			{
				var result = ZString.Empty;
				if (InvoiceQuantity > 0)
				{
					result = (InvoiceQuantity.ToString(QuantityFormat) + " " + InvoiceUQ).TrimEnd();
				}
				else if (CustomsQuantity > 0)
				{
					result = (CustomsQuantityFormatted + " " + this.CustomsUnitQty).TrimEnd();
				}
				return result;
			}
		}

		public ZString CustomsQuantityFormatted
		{
			get { return CustomsQuantity.ToString(QuantityFormat); }
		}

		public ZString CustomsQuantity2Formatted
		{
			get { return JobComInvoiceLine.JI_CustomsSecondQuantity.ToString(QuantityFormat); }
		}

		public ZString CustomsUnitQty2
		{
			get { return JobComInvoiceLine.JI_CustomsSecondUnitQty; }
		}

		public ZString CustomsQuantity3Formatted
		{
			get { return JobComInvoiceLine.JI_CustomsThirdQuantity.ToString(QuantityFormat); }
		}

		public ZString CustomsUnitQty3
		{
			get { return JobComInvoiceLine.JI_CustomsThirdUnitQty; }
		}

		#endregion

		#region Countries

		public ZString EffectiveCountryAndStateOfOrigin
		{
			get { return JobComInvoiceLine.EffectiveCountryAndStateOfOrigin; }
		}

		public ZString CountryAndStateOfOrigin
		{
			get { return JobComInvoiceLine.CountryAndStateOfOrigin; }
		}

		public ZString CountryAndStateOfExport
		{
			get { return JobComInvoiceLine.CountryAndStateOfExport; }
		}

		#endregion

		#region Amounts

		public ZDecimal ValueForCurrencyConversion
		{
			get { return JobComInvoiceLine.CA_CVforCurrConv; }
		}

		public ZDecimal ValueForDuty
		{
			get { return JobComInvoiceLine.CA_CustomsValue; }
		}

		public ZDecimal ValueForTax
		{
			get { return ((IDutyAndTaxData)JobComInvoiceLine).NormalValueForTax; }
		}

		public DocDutyOrTax SIMADuty
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.SIMADuty); }
		}

		public DocDutyOrTax CustomsDuties
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty); }
		}

		public DocDutyOrTax CustomsDuty1
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 0); }
		}

		public DocDutyOrTax CustomsDuty2
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 1); }
		}

		public DocDutyOrTax CustomsDuty3
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 2); }
		}

		public DocDutyOrTax ExciseTax
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.ExciseTax); }
		}

		public DocDutyOrTax GST
		{
			get { return IsInwardWarehouseEntry ? null : DocDutyOrTax.New(Factory, JobComInvoiceLine, DutyAndTaxTypes.Codes.GST); }
		}

		public ZDecimal TotalDutiesAndTaxes
		{
			get { return IsInwardWarehouseEntry ? decimal.Zero : JobComInvoiceLine.DutiesAndTaxes.Sum(a => a.C1_Amount); }
		}

		#endregion

		#region References & Other text fields

		public ZString AuthorityNumber
		{
			get { return JobComInvoiceLine.CA_AuthorityNumber; }
		}

		public ZString TRSNumber
		{
			get { return JobComInvoiceLine.CA_TRSNumber; }
		}

		public ZString EffectiveTreatmentCode
		{
			get { return JobComInvoiceLine.EffectiveTreatmentCode; }
		}

		public ZString TreatmentCode
		{
			get { return JobComInvoiceLine.CA_TreatmentCode; }
		}

		public ZString EffectiveValueForDutyCode
		{
			get { return JobComInvoiceLine.EffectiveValueForDutyCode; }
		}

		public ZString AddData
		{
			get
			{
				var result = new ZStringBuilder();
				if (!JobComInvoiceLine.CA_99TariffCode.IsEmpty)
				{
					result.Append("TC=" + JobComInvoiceLine.CA_99TariffCode);
				}

				if (JobComInvoiceLine.JI_CustomsSecondQuantity > 0)
				{
					result.Append("QTY2=" + JobComInvoiceLine.JI_CustomsSecondQuantity + JobComInvoiceLine.JI_CustomsSecondUnitQty);
				}

				if (JobComInvoiceLine.JI_CustomsThirdQuantity > 0)
				{
					result.Append("QTY3=" + JobComInvoiceLine.JI_CustomsThirdQuantity + JobComInvoiceLine.JI_CustomsThirdUnitQty);
				}

				if (!JobComInvoiceLine.CA_AuthorityNumber.IsEmpty)
				{
					result.Append("S/Auth=" + JobComInvoiceLine.CA_AuthorityNumber);
				}

				if (!JobComInvoiceLine.CA_TRSNumber.IsEmpty)
				{
					result.Append("TRS=" + JobComInvoiceLine.CA_TRSNumber);
				}

				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		#endregion

		#endregion

		#region Implementation

		JobComInvoiceLine JobComInvoiceLine
		{
			get { return (JobComInvoiceLine)WrappedObject; }
		}

		const string QuantityFormat = "#,###.###";

		bool IsInwardWarehouseEntry
		{
			get { return (JobComInvoiceLine.Declaration?.IsInwardWarehouseEntry).GetValueOrDefault(false); }
		}

		#endregion

	}
}
