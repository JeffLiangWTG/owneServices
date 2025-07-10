using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Accounting.KR
{
	public class DocElectronicInvoiceLine : DocumentWrapper
	{
		protected DocElectronicInvoiceLine(TaxInvoiceTradeLineItem line, BusinessObjectFactory factoryToWrap) : base(line, factoryToWrap)
		{
			Line = line;
		}

		public static DocElectronicInvoiceLine New(TaxInvoiceTradeLineItem line, BusinessObjectFactory factoryToWrap)
		{
			if (line != null)
			{
				return new DocElectronicInvoiceLine(line, factoryToWrap);
			}
			return null;
		}

		#region Properties

		public ZString DescriptionText => Line.DescriptionText;

		public ZString InvoiceAmount => Line.InvoiceAmount;

		public ZString CalculatedAmount => Line.CalculatedAmount;

		public ZString NameText => Line.NameText;

		public ZString PurchaseExpiryDateTime => Line.PurchaseExpiryDateTime;

		TaxInvoiceTradeLineItem Line { get; }

		#endregion
	}
}
