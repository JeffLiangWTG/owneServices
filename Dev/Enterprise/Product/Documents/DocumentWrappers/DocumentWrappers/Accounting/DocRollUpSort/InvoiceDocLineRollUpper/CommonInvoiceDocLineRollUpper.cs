namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper
{
	class CommonInvoiceDocLineRollUpper : BaseInvoiceDocLineRollUpper
	{
		public CommonInvoiceDocLineRollUpper(DocARBaseInvoice docARBaseInvoice)
			: base(docARBaseInvoice)
		{
			DocARInvoiceCommon = (DocARInvoiceCommon)docARBaseInvoice;
		}

		DocARInvoiceCommon DocARInvoiceCommon { get; }

		public override void PrepareRollUpLineForGrouping(DocARInvoiceLineForRollUp rollUpLine, DocARInvoiceLineCollection rolledUpLines)
		{
			if (rolledUpLines != null && rolledUpLines.Count > 0)
			{
				if (DocARInvoiceCommon.InvoiceType == DocARBaseInvoice.InvoiceTypeConsol && !DocARInvoiceCommon.IsRollUpEntireConsol)
				{
					rollUpLine.FKToShipment = rolledUpLines[0].FKToShipment;
					rollUpLine.Shipment = rolledUpLines[0].Shipment;
				}
			}

			base.PrepareRollUpLineForGrouping(rollUpLine, rolledUpLines);
		}
	}
}
