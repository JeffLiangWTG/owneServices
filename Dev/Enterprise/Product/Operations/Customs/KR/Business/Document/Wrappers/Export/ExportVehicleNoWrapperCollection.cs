using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportVehicleNoWrapperCollection : NonPersistentBusinessObjectCollection<ExportVehicleNoWrapper>
	{
		public ExportVehicleNoWrapperCollection(IEnumerable<IExportEntryLine> exportEntryLines)
		{
			PopulateElements(exportEntryLines);
		}

		void PopulateElements(IEnumerable<IExportEntryLine> exportEntryLines)
		{
			foreach (var entryLine in exportEntryLines)
			{
				var orderInvoiceLine = entryLine.InvoiceLines.OrderBy(x => x.InvoiceLineNo);
				foreach (var invoiceline in orderInvoiceLine)
				{
					var orderVehicleNumbers = invoiceline.VehicleNumbers.Cast<IExportVehicleNo>().OrderBy(x => x.SequenceNo);
					foreach (var vehicleNo in orderVehicleNumbers)
					{
						var exportVehicleNo = new ExportVehicleNoWrapper(vehicleNo);
						exportVehicleNo.InvoiceLineNo = invoiceline.InvoiceLineNo;
						Add(exportVehicleNo);
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
