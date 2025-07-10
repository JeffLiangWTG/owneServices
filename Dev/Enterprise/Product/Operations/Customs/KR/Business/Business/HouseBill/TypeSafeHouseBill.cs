using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	partial class Bill : AutoKRHouseBill
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusDecHouseBillLookups Lookups => (CusDecHouseBillLookups)base.Lookups;

		public new CusDecHouseBillValidation Validation => (CusDecHouseBillValidation)base.Validation;

		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new Bill GetBillOfType(ZString billType) => (Bill)base.GetBillOfType(billType);

		public new Bill ParentBill => (Bill)base.ParentBill;

		public new ChildBillCollection<Bill, JobDeclaration> ChildBills => (ChildBillCollection<Bill, JobDeclaration>)base.ChildBills;

		public new Bill Clone() => (Bill)base.Clone();

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups() => new CusDecHouseBillLookups(this);

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			Customs.Business.CusDecHouseBillValidation result = null;
			if (IsD87)
			{
				result = new D87BillValidation(this);
			}
			else
			{
				result = new CusDecHouseBillValidation(this);
			}
			return result;
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewHouseBillLevelInvoiceCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IChildBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewChildBills() => new ChildBillCollection<Bill, JobDeclaration>(this, Declaration);

		#endregion

		#endregion
	}
}
