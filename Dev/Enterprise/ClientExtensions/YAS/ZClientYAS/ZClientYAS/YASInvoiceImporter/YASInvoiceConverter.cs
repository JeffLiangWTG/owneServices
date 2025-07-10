using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.YAS.YASInvoiceImporter
{
	public class YASInvoiceConverter : FlatFileConverter
	{
		public YASInvoiceConverter(INotifications subscribeNotification, BusinessObjectFactory factory) : base(subscribeNotification, factory)
		{
		}

		protected override void MapImport(Enterprise.DataTransfer.Xml.IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			base.MapImport(valueObject, fileLines);

			Xsd.InvoiceHeaderCollection xsdInvoiceHeaders = (Xsd.InvoiceHeaderCollection)valueObject;

			fileLines = SortInvoiceCollection(fileLines);

			ZString lastInvoiceNumber = "";
			Xsd.InvoiceHeader currentHeader = null;

			foreach (FlatFileDataRow row in fileLines)
			{
				ZString currentInvoiceNumber = row.GetField(InvoiceConstants.FixedFieldPosition.InvoiceNo);
				if (lastInvoiceNumber != currentInvoiceNumber)
				{
					currentHeader = xsdInvoiceHeaders.AddNew();
					currentHeader.InvoiceNumber = currentInvoiceNumber;
					lastInvoiceNumber = currentInvoiceNumber;
				}

				ImportInvoiceLine(currentHeader, row);
			}
		}

		FlatFileDataRowCollection SortInvoiceCollection(FlatFileDataRowCollection unsortedCollection)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();

			ArrayList listOfRows = new ArrayList(unsortedCollection);
			listOfRows.Sort(new RowComparer());

			foreach (FlatFileDataRow row in listOfRows)
			{
				result.Add(row);
			}

			return result;
		}

		void ImportInvoiceLine(Xsd.InvoiceHeader header, FlatFileDataRow row)
		{
			Xsd.InvoiceLine line = header.InvoiceLines.AddNew();

			line.ProductNumber = row.GetField(InvoiceConstants.FixedFieldPosition.PartNo);
			line.ProductDescription = row.GetField(InvoiceConstants.FixedFieldPosition.PartDescription);
			ZDecimal invoiceQty = row.GetFieldAsZDecimal(InvoiceConstants.FixedFieldPosition.Qty, 2);
			ZDecimal unitPrice = row.GetFieldAsZDecimal(InvoiceConstants.FixedFieldPosition.Value) / 100;
			line.InvoiceQty.Value = invoiceQty;
			line.LinePrice.Value = invoiceQty * unitPrice;
			header.InvoiceAmount.Value += line.LinePrice.Value;
		}

		#region IComparer Members

		protected class RowComparer : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				return Compare((FlatFileDataRow)x, (FlatFileDataRow)y);
			}

			public int Compare(FlatFileDataRow rowX, FlatFileDataRow rowY)
			{
				ZString rowXInvoice = rowX.GetField(InvoiceConstants.FixedFieldPosition.InvoiceNo);
				ZString rowYInvoice = rowY.GetField(InvoiceConstants.FixedFieldPosition.InvoiceNo);
				return String.Compare(rowXInvoice, rowYInvoice);
			}
		}

		#endregion

	}
}
