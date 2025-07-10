using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionWithOverriddenBranchAndDepartmentAdaptor : WrapperCollectionAdaptor<TransactionWithOverriddenBranchAndDepartment>
	{
		public TransactionWithOverriddenBranchAndDepartmentAdaptor(InvoicingBase invoice)
			: base(invoice.Factory, invoice.PK)
		{
			this.invoice = invoice;
		}
		readonly InvoicingBase invoice;

		protected override BusinessObject[] LoadInnerObjects(ZGuid[] pks)
		{
			return new BusinessObject[] { invoice };
		}

		protected override TransactionWithOverriddenBranchAndDepartment Wrap(BusinessObject bizo)
		{
			return new TransactionWithOverriddenBranchAndDepartment((InvoicingBase)bizo);
		}

		public InvoicingBase Invoice
		{
			get
			{
				return invoice;
			}
		}

		public override Type InnerObjectType
		{
			get
			{
				return typeof(InvoicingBase);
			}
		}

		public void ApplyOrCancelChanges(bool apply)
		{
			if (apply)
			{
				invoice.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);
				WrappedObjects[0].AppendChange();
			}
			else
			{
				WrappedObjects[0].CancelChange();
			}

			WrappedObjects.HasChanges = false;
		}
	}
}
