using System;

namespace CargoWise.EntityFramework
{
	internal class CollectionChildrenChangesSubscription : IDisposable
	{
		public CollectionChildrenChangesSubscription(BusinessObjectCollection collection, Action action, string[] fieldsToSubscribe)
		{
			this.collection = collection;
			this.action = action;
			this.fieldsToSubscribe = fieldsToSubscribe;
			Subscribe();
		}

		readonly BusinessObjectCollection collection;
		readonly Action action;
		readonly string[] fieldsToSubscribe;

		void Subscribe()
		{
			this.collection.CountChanged += CollectionCountChanged;

			foreach (BusinessObject element in this.collection)
			{
				SubscribeToChildChanges(element);
			}
		}

		void Unsubscribe()
		{
			this.collection.CountChanged -= CollectionCountChanged;

			foreach (BusinessObject element in this.collection)
			{
				UnsubscribeFromChildChanges(element);
			}
		}

		void CollectionCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (sender != null && action != null)
			{
				if (e.ItemAdded)
				{
					SubscribeToChildChanges(e.BizObject);
				}

				if (e.ItemRemoved)
				{
					UnsubscribeFromChildChanges(e.BizObject);
				}

				this.action();
			}
		}

		void SubscribeToChildChanges(BusinessObject child)
		{
			if (child != null && action != null)
			{
				foreach (var f in fieldsToSubscribe)
				{
					var propertyInfo = child.ZPropertyInfoHash[f];
					if (propertyInfo != null)
					{
						propertyInfo.ValueChanged += ChildValueChanged;
					}
				}
			}
		}

		void UnsubscribeFromChildChanges(BusinessObject child)
		{
			if (child != null)
			{
				foreach (var f in fieldsToSubscribe)
				{
					var propertyInfo = child.ZPropertyInfoHash[f];
					if (propertyInfo != null)
					{
						propertyInfo.ValueChanged -= ChildValueChanged;
					}
				}
			}
		}

		void ChildValueChanged(object sender, EventArgs e)
		{
			this.action();
		}

		public void Dispose()
		{
			Unsubscribe();
		}
	}
}
