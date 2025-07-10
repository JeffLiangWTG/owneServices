using System;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectCollectionFetchStrategy : IBusinessObjectCollectionFetchStrategy
	{
		public BusinessObjectCollectionFetchStrategy(IBusinessObjectCollection collection)
		{
			this.Collection = collection;
		}
		protected IBusinessObjectCollection Collection;

		#region Fetch for Validate

		public void FetchForValidate()
		{
			if (!HasRunFetchForValidate)
			{
				FetchForValidateCore();
				HasRunFetchForValidate = true;
			}
		}
		bool HasRunFetchForValidate;

		protected virtual void FetchForValidateCore()
		{
		}

		#endregion

		#region Fetch for View

		public void FetchForView(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			FetchForViewCore(businessObjects, columns);

			if (AdditionalFetchForView != null)
			{
				AdditionalFetchForView(this, new FetchForViewEventArgs(businessObjects, columns));
			}
		}

		protected virtual void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
		}

		public event EventHandler<FetchForViewEventArgs> AdditionalFetchForView;

		#endregion

		#region Fetch for Bind

		public void FetchForBind()
		{
			FetchForBindCore();
		}

		protected virtual void FetchForBindCore()
		{
		}

		#endregion
	}
}
