using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export
{
	class DeclarationInvoiceExportFlatFileConverter : FlatFileConverter
	{
		public DeclarationInvoiceExportFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		#region Override

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			var dataRows = new FlatFileDataRowCollection();
			var xsdDec = valueObject as Xsd.ConsolAndShipment;

			if(xsdDec != null)
			{
				CreateAndAddBodyRecord(dataRows, xsdDec);
			}
			return dataRows;
		}

		#endregion

		#region Implementation

		void CreateAndAddBodyRecord(FlatFileDataRowCollection dataRows, Xsd.ConsolAndShipment xsdDec)
		{
			if (xsdDec != null)
			{
				var preparedDataRows = new List<DeclarationInvoiceExportFlatFileDataRowBody>();
				foreach (Xsd.InvoiceHeader invHeader in xsdDec.Shipment.Invoices)
				{
					foreach (Xsd.InvoiceLine invLine in invHeader.InvoiceLines)
					{
						var row = new DeclarationInvoiceExportFlatFileDataRowBody();
						row.IndicateBodyRecord = 1;
						row.JobNumber = xsdDec.Shipment.ShipmentDetails.AgentReference;
						row.InvoiceNumberForLine = invHeader.InvoiceNumber;
						row.IndentPONumber = invLine.OrderNumber;
						row.MaterialProductCode = invLine.ProductNumber;
						row.UnitPrice = invLine.LinePrice.Value;
						row.Quantity = invLine.InvoiceQty.Value.ToZInt();
						row.PopulateFields();

						var matchedDataRow = preparedDataRows.Find(
							delegate(DeclarationInvoiceExportFlatFileDataRowBody d)
							{
								return d.JobNumber == row.JobNumber && d.InvoiceNumberForLine == row.InvoiceNumberForLine && d.IndentPONumber == row.IndentPONumber
									&& d.MaterialProductCode == row.MaterialProductCode && d.UnitPrice == row.UnitPrice;
							}
						);

						if (matchedDataRow == null)
						{
							preparedDataRows.Add(row);
						}
						else
						{
							matchedDataRow.Quantity += row.Quantity;
						}
					}
				}

				foreach (DeclarationInvoiceExportFlatFileDataRowBody each in preparedDataRows)
				{
					dataRows.Add(each);
				}
			}
		}

		#endregion

	}
}
