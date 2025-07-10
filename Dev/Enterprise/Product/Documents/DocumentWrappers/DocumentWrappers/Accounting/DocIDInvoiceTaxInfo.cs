using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocIDInvoiceTaxInfo : DocBaseWrapper
	{
		protected DocIDInvoiceTaxInfo(IDInvoiceTaxInfo taxInfo, BusinessObjectFactory factory)
			: base(taxInfo, factory)
		{
		}

		public static DocIDInvoiceTaxInfo New(IDInvoiceTaxInfo taxInfo, BusinessObjectFactory factory)
		{
			if (taxInfo == null)
			{
				return null;
			}
			return new DocIDInvoiceTaxInfo(taxInfo, factory);
		}

		public IDInvoiceTaxInfo TaxInfo
		{
			get
			{
				return WrappedObject as IDInvoiceTaxInfo;
			}
		}

		public ZString TaxRate
		{
			get { return TaxInfo.TaxRate; }
		}

		public ZDecimal TotalLocalExTaxAmount
		{
			get { return TaxInfo.TotalLocalExTaxAmount; }
		}

		public ZDecimal TotalLocalTaxAmount
		{
			get { return TaxInfo.TotalLocalTaxAmount; }
		}
	}

	public class IDInvoiceTaxInfo
	{
		public ZString TaxRate { get; set; }
		public ZDecimal TotalLocalExTaxAmount { get; set; }
		public ZDecimal TotalLocalTaxAmount { get; set; }
	}
}
