using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.HEN.Nissan
{
	public class NissanInvoiceConverter : FlatFileConverter<InvoiceHeader>
	{
		public NissanInvoiceConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(InvoiceHeader valueObject, FlatFileDataRowCollection fileLines)
		{
			foreach (FlatFileDataRow dataRow in fileLines)
			{
				switch (dataRow[NissanConstants.LineTypePosition])
				{
					case NissanConstants.InvoiceHeaderLineType:
						MapInvoiceHeader(valueObject, dataRow);
						break;
					case NissanConstants.InvoiceLineLineType:
						MapInvoiceLine(valueObject, dataRow);
						break;
				}
			}

			if (!HasCorrectRows)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, NissanConstants.NissanErrorMessage));
			}
		}

		void MapInvoiceHeader(InvoiceHeader invoiceHeader, FlatFileDataRow dataRow)
		{
			invoiceHeader.InvoiceNumber = dataRow[NissanConstants.InvoiceHeader.InvoiceNo];
			invoiceHeader.InvoiceAmount.CurrencyCode = dataRow[NissanConstants.InvoiceHeader.Currency];
			invoiceHeader.InvoiceAmount.Value = ZDecimal.ParseSafe(dataRow[NissanConstants.InvoiceHeader.TotalAmount], ZDecimal.Zero);
			invoiceHeader.Weight.Value = ZDecimal.ParseSafe(dataRow[NissanConstants.InvoiceHeader.GrossWeight], ZDecimal.Zero);

			HasCorrectRows = true;
		}

		void MapInvoiceLine(InvoiceHeader invoiceHeader, FlatFileDataRow dataRow)
		{
			InvoiceLine line = invoiceHeader.InvoiceLines.AddNew();

			line.ProductNumber = dataRow[NissanConstants.InvoiceLine.ProductCode];
			line.InvoiceQty.Value = ZDecimal.ParseSafe(dataRow[NissanConstants.InvoiceLine.InvoiceQty], ZDecimal.Zero);
			line.LinePrice.Value = ZDecimal.ParseSafe(dataRow[NissanConstants.InvoiceLine.InvoicePrice], ZDecimal.Zero);
			line.Weight.Value = ZDecimal.ParseSafe(dataRow[NissanConstants.InvoiceLine.GrossWeight], ZDecimal.Zero);
			line.OrderNumber = dataRow[NissanConstants.InvoiceLine.OrderNo];

			RefCountry country = RefCountry.LoadFromCountryName(Factory, dataRow[NissanConstants.InvoiceLine.GoodsOrigin]);
			line.LineClassification.OriginOfGoods = country != null ? country.RN_Code : ZString.Empty;
		}

		bool HasCorrectRows;
	}
}
