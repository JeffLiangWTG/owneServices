using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			return cusEntryLine == null ? null : new DocCusEntryLine(cusEntryLine, factoryToWrap);
		}

		#region Overrides

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		#endregion

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new DocJobComInvoiceLineCollection(Factory);

					foreach (var line in CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray())
					{
						invoiceLines.Add(DocJobComInvoiceLine.New(line, Factory));
					}
				}
				return invoiceLines;
			}
		}
		protected DocJobComInvoiceLineCollection invoiceLines;

		public DocCusEntryLineFeeCollection AdValoremFees
		{
			get
			{
				if (adValoremFees == null)
				{
					adValoremFees = new DocCusEntryLineFeeCollection(Factory);
					adValoremFees.AddRange(CusEntryLine.Fees.Where(x => !x.IsQuantityPerUnit).Select(x => DocCusEntryLineFee.New(x, Factory)).ToArray());
				}
				return adValoremFees;
			}
		}
		DocCusEntryLineFeeCollection adValoremFees;

		public DocCusEntryLineFeeCollection QuantityPerUnitFees
		{
			get
			{
				if (quantityPerUnitFees == null)
				{
					quantityPerUnitFees = new DocCusEntryLineFeeCollection(Factory);
					quantityPerUnitFees.AddRange(CusEntryLine.Fees.Where(x => x.IsQuantityPerUnit).Select(x => DocCusEntryLineFee.New(x, Factory)).ToArray());
				}
				return quantityPerUnitFees;
			}
		}
		DocCusEntryLineFeeCollection quantityPerUnitFees;

		#region ZDecimal

		public ZDecimal NetWeightInKG => CusEntryLine.EffectiveNetWeight.InKilogramsSafe;

		public ZDecimal ICMSRate => FirstInvoiceLine.JI_ICMSRate;

		public ZDecimal DutyDueAmount => CusEntryLine.DutyDueAmount.Round(2);

		public ZDecimal DutyAdValorem => FirstInvoiceLine.DutyVigentRateValue;

		public ZDecimal IPIDueAmount => CusEntryLine.IPIDueAmount.Round(2);

		public ZDecimal IPIAdValorem => FirstInvoiceLine.IPIVigentRateValue;

		public ZDecimal PISDueAmount => CusEntryLine.PISDueAmount.Round(2);

		public ZDecimal PisAdValorem => FirstInvoiceLine.PisVigentRateValue;

		public ZDecimal CofinsDueAmount => CusEntryLine.CofinsDueAmount.Round(2);

		public ZDecimal CofinsAdValorem => FirstInvoiceLine.CofinsVigentRateValue;

		public ZDecimal AntidumpingDueAmount => CusEntryLine.AntidumpingDueAmount;

		public ZDecimal DefaultAntidumpingAdValorem => FirstInvoiceLine.DefaultAntidumpingRateValue;

		public ZDecimal DutyAgreementRate => CusEntryLine.DutyAgreementRate;

		public ZDecimal DutyReducedRate => CusEntryLine.DutyReducedRate;

		public ZDecimal DutyReductionPercentage => CusEntryLine.DutyReductionPercentage;

		public ZDecimal IPIReducedRate => CusEntryLine.IPIReducedRate;

		public ZDecimal PISCofinsReducedRate => CusEntryLine.PISCofinsReducedRate;

		public ZDecimal PISCofinsReductionPercentage => CusEntryLine.PISCofinsReductionPercentage;

		#endregion

		#region Implementation

		CusEntryLine CusEntryLine => (CusEntryLine)WrappedObject;

		JobComInvoiceLine FirstInvoiceLine => CusEntryLine.FirstLine as JobComInvoiceLine;

		#endregion
	}
}
