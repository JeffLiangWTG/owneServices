using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	partial class Bill : Customs.Business.Bill
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		[ChildEditable(true)]
		public new BillContainerCollection Containers => (BillContainerCollection)base.Containers;

		public new BillLookups Lookups => (BillLookups)base.Lookups;

		public new BillValidation Validation => (BillValidation)base.Validation;

		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new Bill GetBillOfType(ZString billType) => (Bill)base.GetBillOfType(billType);

		public new Bill ParentBill => (Bill)base.ParentBill;

		public new ChildBillCollection<Bill, JobDeclaration> ChildBills => (ChildBillCollection<Bill, JobDeclaration>)base.ChildBills;

		public new Bill Clone() => (Bill)base.Clone();

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		protected override CusDecHouseBillLookups GetNewLookups()
		{
			return new BillLookups(this);
		}

		protected override CusDecHouseBillValidation GetNewValidation()
		{
			return new BillValidation(this);
		}

		protected override BaseBillContainerCollection GetContainerCollection()
		{
			return new BillContainerCollection(this);
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewHouseBillLevelInvoiceCollection()
		{
			return new InvoiceHeaderActiveCollection(this);
		}

		protected override IChildBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewChildBills()
		{
			return new ChildBillCollection<Bill, JobDeclaration>(this, Declaration);
		}

		#endregion

		#endregion
	}
}
