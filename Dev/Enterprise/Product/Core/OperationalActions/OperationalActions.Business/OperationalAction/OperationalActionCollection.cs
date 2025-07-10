using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionCollection : MenuEditableBusinessObjectCollection<OperationalAction>
	{
		public OperationalActionCollection(BusinessObjectFactory factory, OperationalActionContext context)
			: base(factory)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			this.context = context;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(new OperationalActionZQuery(context.Supporter.BusinessContext));
			return result;
		}

		protected override void SetUpChild(OperationalAction child)
		{
			base.SetUpChild(child);
			child.Context = context;
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == StmMenuItemSchema.Constants.SU_MenuPath)
			{
				return new ActionComparer(direction == ListSortDirection.Descending);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}

		readonly OperationalActionContext context;
	}
}
