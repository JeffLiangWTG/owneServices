using System;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public partial class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
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

		public InvoiceHeaderActiveCollection(JobDeclaration declaration, string customsGenPivotType)
			: base(declaration, typeof(JobComInvoiceHeader), customsGenPivotType)
		{
		}

		public override void Delete(BaseJobComInvoiceHeader businessObject)
		{
			var reCalculateSeq = businessObject.JZ_InvoiceDisplaySequence - 1;
			base.Delete(businessObject);
			if (declaration != null && declaration.IsImport)
			{
				PageNumberCalculator.RecalculatePageNumber(this, reCalculateSeq);
			}
		}

		public IDisposable SuspendRecalculatePageNumbers()
		{
			return new DisposableAction(() => recalculatePageNumbersSuspenderIndex++, () => recalculatePageNumbersSuspenderIndex--);
		}

		public bool IsRecalculatePageNumbersSuspended => recalculatePageNumbersSuspenderIndex > 0;

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		byte recalculatePageNumbersSuspenderIndex;

		public void RecalculateAllPageNumbers()
		{
			PageNumberCalculator.RecalculatePageNumber(this, 0);
		}
	}
}
