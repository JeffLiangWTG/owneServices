namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLineExportPreviousDocumentValidation : ExportPreviousDocumentValidation
	{
		public InvoiceLineExportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckPreviousDocumentCount();
		}

		protected override void CheckCSI_PackQty()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_PackQty(Parent);
		}

		protected override void CheckCSI_PackType()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_PackType(Parent);
		}

		protected override void CheckCSI_Quantity()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_Quantity(Parent);
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_UnitOfQuantity(Parent);
		}

		void CheckPreviousDocumentCount()
		{
			var parent = Parent;
			if (parent.Parent is JobComInvoiceLine invoiceLine && invoiceLine.PreviousDocuments.Count is var previousDocumentsCount && previousDocumentsCount > 9 && parent.IsParentInvoiceOrLineOrCusEntry)
			{
				if (parent.Declaration?.IsTransitionPeriodAES30 ?? false)
				{
					Parent.AddRowMessageError(Res.GetString("9A3446C4-ED35-4851-ADAE-5F3B8D1DCBAD", "During the transition period, which is active now, the maximum number of previous documents allowed is 9."));
				}
				else if (previousDocumentsCount > 99)
				{
					Parent.AddRowMessageError(Res.GetString("C68222CE-BBEA-41EA-B154-08B701128462", "During post transition period, the maximum number of previous documents allowed is 99 per entry line"));
				}
			}
		}
	}
}
