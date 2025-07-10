using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class DependentJobCollection<T> : JobCollection where T : BusinessObject
	{
		protected DependentJobCollection(T master, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Master = master;
		}

		internal T Master { get; private set; }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public new DependentJob<T> this[int index]
		{
			get { return (DependentJob<T>)Elements[index]; }
		}

		public void RefreshElementCachedValues()
		{
			foreach (DependentJob<T> element in this)
			{
				element.RefreshCachedValues();
			}
		}
	}
}
