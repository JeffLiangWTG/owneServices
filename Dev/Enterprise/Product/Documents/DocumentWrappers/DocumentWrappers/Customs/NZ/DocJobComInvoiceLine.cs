
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(invoiceLine, factory)
		{
			InvoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine InvoiceLine;

		public static DocJobComInvoiceLine New(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			if (invoiceLine == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceLine(invoiceLine, factory);
			}
		}

		public ZBool RequiresPermitCodes
		{
			get
			{
				bool requiresPermits = NZCTariffsPermitsApplyTo.Load(Factory, Tariff, true, false) != null;
				return requiresPermits && InvoiceLine.PermitCodes.Count == 0;
			}
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Enterprise.Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Enterprise.Customs.Business.CusEntryLine entryLineToWrap)
		{
			return FormalEntry.DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		public FormalEntry.DocCusEntryLine EntryLine
		{
			get { return (FormalEntry.DocCusEntryLine)CusEntryLineInternal; }
		}

		protected override DocCountry CountryOfOriginCore
		{
			get { return DocCountry.New(InvoiceLine.EffectiveCountryOfOriginRefCountry, Factory); }
		}

		protected override ZString CountryOfOriginCodeCore
		{
			get { return InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin; }
		}
	}
}
