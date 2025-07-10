using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Registry.Business
{
	public partial class BillOfLadingNumberCustomisationElementView : NonPersistentBusinessObjectCollection<BillOfLadingNumberCustomisationElement>
	{
		public BillOfLadingNumberCustomisationElementView(BillOfLadingNumberCustomisationElementCollection innerCollection)
			: base(innerCollection.Factory)
		{
			this.innerCollection = innerCollection;
			this.innerCollection.CountChanged += new CollectionCountChangedEventHandler(innerCollection_CountChanged);
		}

		public void SetCategories(NumberCustomisationElementCategories categoriesToShow)
		{
			categories = categoriesToShow;

			RemoveAllButLeaveRelationshipsIntact();

			foreach (BillOfLadingNumberCustomisationElement element in innerCollection)
			{
				if (element.Matches(categories))
				{
					Add(element);
				}
			}
		}

		internal ZBool HasIncludedElementWithCheckDigit
		{
			get
			{
				foreach (BillOfLadingNumberCustomisationElement element in Elements)
				{
					if (element.Include && element.CheckDigit)
					{
						return true;
					}
				}
				return false;
			}
		}

		#region Events

		void innerCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			BillOfLadingNumberCustomisationElement element = (BillOfLadingNumberCustomisationElement)e.BizObject;

			if (e.ItemAdded)
			{
				if (element.Matches(categories))
				{
					Add(element);
				}
			}
			else if (e.ItemRemoved)
			{
				Remove(element);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillOfLadingNumberCustomisationElement(innerCollection.ParentCustomisation, new NullElementStrategy());
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == BillOfLadingNumberCustomisationElement.Schema.Order)
			{
				return new OrderComparer(direction != ListSortDirection.Ascending);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}

		#region OrderComparer

		class OrderComparer : IComparer, IComparer<BillOfLadingNumberCustomisationElement>
		{
			public OrderComparer(bool inverse)
			{
				this.inverse = inverse;
			}

			int CompareCore(byte x, byte y)
			{
				if (x == y)
				{
					return 0;
				}

				if (x == 0)
				{
					return 1;
				}

				if (y == 0)
				{
					return -1;
				}

				return x - y;
			}

			public int Compare(BillOfLadingNumberCustomisationElement x, BillOfLadingNumberCustomisationElement y)
			{
				int diff = CompareCore(x.Order, y.Order);
				return inverse ? -diff : diff;
			}

			#region IComparer Members

			int IComparer.Compare(object x, object y)
			{
				return Compare((BillOfLadingNumberCustomisationElement)x, (BillOfLadingNumberCustomisationElement)y);
			}

			#endregion

			readonly bool inverse;
		}

		#endregion

		#endregion

		NumberCustomisationElementCategories categories;
		readonly BillOfLadingNumberCustomisationElementCollection innerCollection;
	}
}
