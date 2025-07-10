using System;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectCollectionFetchStrategy : IFetchStrategy
	{
		void FetchForValidate();
		void FetchForView(BusinessObject[] businessObjects, TableColumn[] columns);

		event EventHandler<FetchForViewEventArgs> AdditionalFetchForView;
	}

	public class FetchForViewEventArgs : EventArgs
	{
		public FetchForViewEventArgs(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			BusinessObjects = businessObjects;
			TableColumns = columns;
		}

		public BusinessObject[] BusinessObjects { get; set; }
		public TableColumn[] TableColumns { get; set; }
	}
}
