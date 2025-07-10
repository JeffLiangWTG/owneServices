using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB
{
	public class FreightlinerDataConverter : WCBDataConverter
	{
		public FreightlinerDataConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			fileLines.RemoveAt(0);  //Remove Header

			ZString invoiceNumber = fileLines[0][Constants.InvoiceNumber];
			Xsd.InvoiceHeader invoiceHeader = valueObject as Xsd.InvoiceHeader;

			foreach (FlatFileDataRow fileLine in fileLines)
			{
				invoiceHeader.InvoiceNumber = invoiceNumber;
				Xsd.InvoiceLine invoiceLine = new Xsd.InvoiceLine();
				ConvertFreightlinerInvoiceLine(fileLine, invoiceLine);
				invoiceHeader.InvoiceLines.Add(invoiceLine);
				invoiceHeader.InvoiceAmount.Value += invoiceLine.LinePrice.Value;
			}
		}

		protected static void ConvertFreightlinerInvoiceLine(FlatFileDataRow fileLine, Xsd.InvoiceLine invoiceLine)
		{
			invoiceLine.OrderNumber = fileLine[Constants.OrderNumber];
			invoiceLine.ProductNumber = fileLine[Constants.PartNumber];
			invoiceLine.LineClassification.OriginOfGoods = fileLine[Constants.Origin];

			ZDecimal shippedQuantity = fileLine.GetFieldAsZDecimal(Constants.Quantity);
			invoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(shippedQuantity, ZString.Empty);

			ZDecimal unitPrice = fileLine.GetFieldAsZDecimal(Constants.UnitPrice);
			ZDecimal linePrice = Utilities.Round((shippedQuantity * unitPrice), 2);
			invoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrencyCode(linePrice, ZString.Empty);
		}
	}
}
