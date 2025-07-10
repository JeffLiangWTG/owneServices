using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseBillOfLadingCollection : NonPersistentBusinessObjectCollection<DocBaseBillOfLading>
	{
		public DocBaseBillOfLadingCollection(BaseJobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}
		readonly BaseJobDeclaration declaration;

		public void LoadBillOfLadings()
		{
			foreach (Bill bill in declaration.LowestBills)
			{
				if (!bill.CU_BillNum.IsEmpty)
				{
					AddNewBill(bill);
				}
			}
		}

		protected virtual void AddNewBill(Bill bill)
		{
			Add(new DocBaseBillOfLading(bill));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("CreateNonPersistentBusinessObject is not supported by DocBaseBillOfLadingCollection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
