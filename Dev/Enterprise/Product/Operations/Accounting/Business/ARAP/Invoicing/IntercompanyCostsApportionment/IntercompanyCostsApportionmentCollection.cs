using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IntercompanyCostsApportionmentCollection : NonPersistentBusinessObjectCollection<IntercompanyCostsApportionment>	{
		readonly IntercompanyCostsApportionmentInvoiceLine invoiceLine;

		public IntercompanyCostsApportionmentCollection(BusinessObjectFactory factory, IntercompanyCostsApportionmentInvoiceLine invoiceLine)
			: base(factory)
		{
			this.invoiceLine = invoiceLine;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IntercompanyCostsApportionment(Factory, invoiceLine);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var apportionment = (IntercompanyCostsApportionment)child;
			apportionment.TaxBranch = invoiceLine.TaxBranch;
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return ShouldAllowRemove;
			}
		}
		protected bool ShouldAllowRemove { get; set; }

		protected override bool AllowNewCore
		{
			get
			{
				return ShouldAllowNew;
			}
		}
		protected bool ShouldAllowNew { get; set; }

		public class ApportionmentsAllowNewAndRemoveSuspender : IDisposable
		{
			public ApportionmentsAllowNewAndRemoveSuspender(IntercompanyCostsApportionmentCollection apportionments)
			{
				this.apportionments = apportionments;
				apportionments.ShouldAllowNew = true;
				apportionments.ShouldAllowRemove = true;
			}

			readonly IntercompanyCostsApportionmentCollection apportionments;

			void IDisposable.Dispose()
			{
				apportionments.ShouldAllowRemove = false;
				apportionments.ShouldAllowNew = false;
				apportionments.RefreshBinding();
			}
		}

		public ApportionmentsAllowNewAndRemoveSuspender GetApportionmentsAllowNewAndRemoveSuspender()
		{
			return new ApportionmentsAllowNewAndRemoveSuspender(this);
		}
	}
}
