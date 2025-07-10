namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusContainerInvoiceLinePivotValidation : Customs.Business.CusContainerInvoiceLinePivotValidation
	{
		public CusContainerInvoiceLinePivotValidation(CusContainerInvoiceLinePivot parent)
			: base(parent)
		{
		}

		protected override void CheckC2_NetWeight()
		{
			base.CheckC2_NetWeight();
			if (!Parent.C2_NetWeight.IsEmpty && IsQuarantineDeclaration)
			{
				var invoiceLine = Parent.InvoiceLine;
				var invoiceHeader = invoiceLine != null ? invoiceLine.InvoiceHeader : null;
				if (invoiceHeader != null && invoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					Parent.C2_NetWeightInfo.AddMessageError("IMA1 net weight can only be entered when produce type is dairy.");
				}
			}
		}

		protected override void CheckC2_GrossWeight()
		{
			base.CheckC2_GrossWeight();
			if (!Parent.C2_GrossWeight.IsEmpty && IsQuarantineDeclaration)
			{
				var invoiceLine = Parent.InvoiceLine;
				var invoiceHeader = invoiceLine != null ? invoiceLine.InvoiceHeader : null;
				if (invoiceHeader != null && invoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					Parent.C2_GrossWeightInfo.AddMessageError("IMA1 gross weight can only be entered when produce type is dairy.");
				}
			}
		}

		protected override bool IsCusContainerInvoiceLineValidationRequired
		{
			get { return IsQuarantineDeclaration; }
		}

		bool IsQuarantineDeclaration
		{
			get
			{
				var invoiceLine = Parent.InvoiceLine;
				var invoiceHeader = invoiceLine != null ? invoiceLine.InvoiceHeader : null;
				var declaration = invoiceHeader != null ? invoiceHeader.JobDeclaration : null;

				return declaration != null && declaration.IsQuarantine;
			}
		}

		protected new CusContainerInvoiceLinePivot Parent
		{
			get { return (CusContainerInvoiceLinePivot)base.Parent; }
		}
	}
}
