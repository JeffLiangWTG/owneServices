using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB
{
	public class DaimlerDataConverter : WCBDataConverter
	{
		public DaimlerDataConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		#region MapImport

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			fileLines.RemoveAt(0);  //Remove Header

			Xsd.InvoiceHeader invoiceHeader = valueObject as Xsd.InvoiceHeader;

			foreach (FlatFileDataRow fileLine in fileLines)
			{
				invoiceHeader.InvoiceNumber = fileLine[Constants.DaimlerInvoiceNumber];
				Xsd.InvoiceLine invoiceLine = new Xsd.InvoiceLine();
				ConvertDaimlerInvoiceLine(fileLine, invoiceLine);

				invoiceHeader.InvoiceLines.Add(invoiceLine);
				invoiceHeader.InvoiceAmount.Value += invoiceLine.LinePrice.Value;
			}
		}

		#endregion

		#region Convert Invoice Lines

		static void ConvertDaimlerInvoiceLine(FlatFileDataRow fileLine, Xsd.InvoiceLine invoiceLine)
		{
			invoiceLine.OrderNumber = ZString.Empty;
			invoiceLine.ProductNumber = fileLine[Constants.PartNumber];
			invoiceLine.LineClassification.OriginOfGoods = fileLine[Constants.Origin];
			if (invoiceLine.LineClassification.OriginOfGoods.StartsWith("0"))
			{
				invoiceLine.LineClassification.OriginOfGoods = ZString.Empty;
			}

			ZDecimal shippedQuantity = fileLine.GetFieldAsZDecimal(Constants.DaimlerQty);
			invoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(shippedQuantity, ZString.Empty);

			ZDecimal unitPrice = fileLine.GetFieldAsZDecimal(Constants.DaimlerUnitPrice);
			ZDecimal linePrice = Utilities.Round((unitPrice), 2);
			invoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrencyCode(linePrice, ZString.Empty);
		}

		#endregion
	}
}
