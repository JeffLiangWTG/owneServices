using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers
{
	public class DocInvoiceHasChargeCodeCollection : GenericWrapperCollection<DocInvoiceHasChargeCode>
	{
		public DocInvoiceHasChargeCodeCollection(DocARInvoiceLineCollection linesForInvoice, BusinessObjectFactory factory)
			: base(factory)
		{
			this.linesForInvoice = linesForInvoice;
		}

		readonly DocARInvoiceLineCollection linesForInvoice;

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return DocInvoiceHasChargeCode.New((InvoiceHasChargeCode)objectToWrap, Factory);
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (linesForInvoice != null && linesForInvoice.OfType<DocARInvoiceLine>().Any(x => x.ChargeCode.Code == index))
			{
				return DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode(index, ZBool.True), Factory);
			}
			return DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode(index, ZBool.False), Factory);
		}
	}
}

