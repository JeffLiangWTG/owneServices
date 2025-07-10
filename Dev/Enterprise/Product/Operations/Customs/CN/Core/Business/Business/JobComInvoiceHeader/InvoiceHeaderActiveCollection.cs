using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public InvoiceHeaderActiveCollection(Bill bill)
			: base(bill)
		{
		}

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);

		public JobDeclaration JobDeclaration => (JobDeclaration)base.declaration;

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var invoiceHeader = (JobComInvoiceHeader)newElement;
			if (invoiceHeader != null)
			{
				invoiceHeader.JZ_IncoTerm = invoiceHeader.IncoTermConverter.GetConvertedIncoTerm(declaration.JE_ShipmentIncoTerm, declaration.IsImport);

				var link = JobDeclaration.SupplierImporterLink;
				if (!link?.OL_RelatedParty.IsEmpty ?? false)
				{
					if (link.OL_RelatedParty == MasterFiles.Business.Customs.RelatedPartyList.Codes.Related)
					{
						invoiceHeader.JZ_SpecialRelationshipConfirm = ConfirmationTypeList.Codes.Yes;
					}
					else if (link.OL_RelatedParty == MasterFiles.Business.Customs.RelatedPartyList.Codes.Unrelated)
					{
						invoiceHeader.JZ_SpecialRelationshipConfirm = ConfirmationTypeList.Codes.No;
					}
				}
			}
		}

		public void SetDefaultsForInvoiceHeader(BaseJobComInvoiceHeader invoiceHeader)
		{
			SetDefaultsForNewElementCore(invoiceHeader);
		}
	}
}
