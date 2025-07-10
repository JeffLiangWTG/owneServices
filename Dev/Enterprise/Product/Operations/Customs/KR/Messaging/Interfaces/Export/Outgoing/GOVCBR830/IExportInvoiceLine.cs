using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportInvoiceLine : IInvoiceLine
	{
		[ID()]
		new ZString InvoiceLineNo { get; }
		[DataItemID("C201")]
		ZDecimal QtyOrWeight { get; }
		[DataItemID("C202")]
		ZString QtyOrWeightUnit { get; }
		[DataItemID("C203")]
		ZDecimal UnitPrice { get; }
		[DataItemID("C204")]
		ZDecimal Amount { get; }
		[DataItemID("C102")]
		ZString Ingredient { get; }
		[DataItemID("C101")]
		ZString DetailDescription { get; }
		[DataItemID("C103")]
		ZString LotNumber { get; }
		IEnumerable<IExportGAApprovalDocument> GAApprovalDocuments { get; }
		IEnumerable<IExportVehicleNo> VehicleNumbers { get; }
	}
}
