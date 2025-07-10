using System;
using System.Collections;
using System.ComponentModel;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B2JobComInvoiceHeaderCollection : InvoiceHeaderActiveCollection
	{
		public B2JobComInvoiceHeaderCollection(JobComInvoiceGroupHeader groupHeader)
			: base(groupHeader, true)
		{
			this.groupHeader = groupHeader;
		}
		readonly JobComInvoiceGroupHeader groupHeader;

		protected new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var invoice = (JobComInvoiceHeader)newElement;
			if (invoice != null && groupHeader != null)
			{
				using (invoice.GetValidationSuspender())
				{
					invoice.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
				}
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var declaration = groupHeader.JobDeclaration;
				return declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X);
			}
		}

		public override void Delete(BaseJobComInvoiceHeader businessObject)
		{
			if (!suspendDeleteAsAccountedFromAsClaimed && businessObject is JobComInvoiceHeader asClaimedHeader && asClaimedHeader.IsB2AsClaimedForSeededHeader && asClaimedHeader.CorrespondingAsAccountedForInvoice != null)
			{
				asClaimedHeader.CorrespondingAsAccountedForInvoice.Delete();
			}
			else
			{
				base.Delete(businessObject);
			}
		}

		public IDisposable SuspendDeleteAsAccountedFromAsClaimed()
		{
			return new DeleteAsAccountedFromAsClaimed(this);
		}

		class DeleteAsAccountedFromAsClaimed : IDisposable
		{
			public DeleteAsAccountedFromAsClaimed(B2JobComInvoiceHeaderCollection asClaimedInvoices)
			{
				this.asClaimedInvoices = asClaimedInvoices;
				asClaimedInvoices.suspendDeleteAsAccountedFromAsClaimed = true;
			}

			readonly B2JobComInvoiceHeaderCollection asClaimedInvoices;

			public void Dispose()
			{
				asClaimedInvoices.suspendDeleteAsAccountedFromAsClaimed = false;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool suspendDeleteAsAccountedFromAsClaimed = false;

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == JobComInvoiceHeader.Schema.JZ_InvoiceNumber)
			{
				comparer = GetComparer(property, direction);
			}
			else
			{
				comparer = base.GetSortComparerForProperty(property, direction);
			}
			return comparer;
		}

		B2InvoiceHeaderComparer GetComparer(PropertyDescriptor property, ListSortDirection direction)
		{
			if (comparer == null || comparer.Direction != direction)
			{
				comparer = new B2InvoiceHeaderComparer(property, direction);
			}
			return comparer;
		}
		B2InvoiceHeaderComparer comparer;
	}
}
