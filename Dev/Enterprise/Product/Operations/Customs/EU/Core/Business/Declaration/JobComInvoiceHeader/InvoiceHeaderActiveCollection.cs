using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
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

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public bool AtLeastOneInvoiceHasASupplier
		{
			get
			{
				foreach (JobComInvoiceHeader inv in this)
				{
					if (!inv.JZ_OH_Supplier.IsEmpty)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AtLeastOneInvoiceHasABuyer
		{
			get
			{
				foreach (JobComInvoiceHeader inv in this)
				{
					if (!inv.JZ_OH_Buyer.IsEmpty)
					{
						return true;
					}
				}
				return false;
			}
		}

		#region SetDefaultsForNewElement

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
		}

		#endregion
	}
}
