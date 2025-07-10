using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: this(declaration, true)
		{
		}

		public InvoiceHeaderActiveCollection(JobDeclaration declaration, bool allowNew)
			: base(declaration, allowNew)
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

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)declaration; }
		}

		public bool HasAnElementWithValidCharges()
		{
			foreach (JobComInvoiceHeader invoice in this)
			{
				if (invoice.Charges.HasAnElementWithValidCharges())
				{
					return true;
				}
			}
			return false;
		}

		public void NotifyEffectiveDutyDateDirty()
		{
			foreach (JobComInvoiceHeader invoice in this)
			{
				invoice.NotifyEffectiveDutyDateDirty();
			}
		}

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)newElement;

			if (JobDeclaration != null)
			{
				if (JobDeclaration.IsDrawback)
				{
					invoiceHeader.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackHeaderAssesmentMethod;
					invoiceHeader.AddInfo.ZA_EDN_Hidden = JobDeclaration.AddInfo.ZA_EDN_Hidden;
				}
				else if (JobDeclaration.IsExWarehouse)
				{
					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				}
			}

			JobComInvoiceHeader previousRow = Count > 0 ? this[Count - 1] : null;
			if (previousRow != null)
			{
				invoiceHeader.AddInfo.ZA_ValuationBasis_Hidden = previousRow.AddInfo.ZA_ValuationBasis_Hidden;
			}
		}
	}
}
