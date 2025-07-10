using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.HarleyDavidson
{
	public class HarleyDavidsonConverter : FlatFileConverter<Xsd.InvoiceHeader>
	{
		public HarleyDavidsonConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(Xsd.InvoiceHeader valueObject, FlatFileDataRowCollection fileLines)
		{
			valueObject.InvoiceNumber = "Invoice";
			valueObject.InvoiceAmount.CurrencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

			foreach (FlatFileDataRow row in fileLines)
			{
				Xsd.InvoiceLine line = valueObject.InvoiceLines.AddNew();
				line.ProductNumber = row[Constants.PartNumber];
				line.ProductDescription = row[Constants.Description];

				line.InvoiceQty.Value = row.GetFieldAsZDecimal(Constants.Quantity);
				line.LinePrice.Value = row.GetFieldAsZDecimal(Constants.UnitPrice, 2) * line.InvoiceQty.Value;
				line.LinePrice.CurrencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

				valueObject.InvoiceAmount.Value += line.LinePrice.Value;
			}
		}

		#region Implementation

		static class Constants
		{
			public const int PartNumber = 0;
			public const int Description = 1;
			public const int Quantity = 2;
			public const int UnitPrice = 3;
		}

		#endregion
	}
}
