using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class B2JobComInvoiceLineViewCollection : JobComInvoiceLineViewCollection
	{
		public B2JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection, bool isAccountForLine)
			: base(invoice, completeCollection)
		{
			this.isAccountForLine = isAccountForLine;
			Rebuild();
		}

		readonly bool isAccountForLine;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var invoiceLine = (JobComInvoiceLine)element;
			return base.IsThisPartOfTheCollection(element) && invoiceLine != null && invoiceLine.CA_IsAccountForLine == isAccountForLine;
		}

		protected override bool ShouldWeAddBusinessObjectStraightToView(BusinessObject businessObject)
		{
			return true;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var invoiceLine = (JobComInvoiceLine)child;
			if (invoiceLine != null)
			{
				using (invoiceLine.GetValidationSuspender())
				{
					invoiceLine.CA_IsAccountForLine = isAccountForLine;
				}
			}
		}

		public new JobComInvoiceLine AddNew()
		{
			return base.AddNew();
		}

		public JobComInvoiceLine AddNew(JobComInvoiceLine asAccountForLine)
		{
			var result = AddNew();
			if (!isAccountForLine && asAccountForLine != null && asAccountForLine.CA_IsAccountForLine)
			{
				result.JI_ParentID = asAccountForLine.PK;
				result.JI_ParentTableCode = asAccountForLine.TablePrefix;
				result.CA_IsSeeded = true;
				asAccountForLine.ReadOnlyAsClaimForFilteredInvoiceLines.Rebuild();
			}
			return result;
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == JobComInvoiceLine.Schema.CA_OriginalLineNo)
			{
				comparer = new CAInvoiceLineComparer(direction == ListSortDirection.Descending, JobComInvoiceLine.Schema.CA_OriginalLineNo);
			}
			else
			{
				comparer = base.GetComparerForSort(property, direction);
			}
			return comparer;
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete is JobComInvoiceLine asClaimedLine && asClaimedLine.IsB2OrB3XAdjustments && !asClaimedLine.CA_IsAccountForLine && asClaimedLine.CorrespondingAsAccountedForInvoiceLine is JobComInvoiceLine correspondingAsAccountedForInvoiceLine)
			{
				correspondingAsAccountedForInvoiceLine.Delete();
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected override bool AllowNewCore
		{
			get
			{
				var result = base.AllowNewCore;
				if (!InvoiceHeader.IsDeleted && InvoiceHeader.IsB2AsAccountForSeededHeader)
				{
					result = false;
				}
				return result;
			}
		}
	}
}
