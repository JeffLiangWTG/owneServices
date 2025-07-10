using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogCollection : BusinessObjectCollection<BusinessObject>
	{
		public StmALogCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public StmALogCollection(BusinessObjectFactory factory, ZQuery sQLFilter)
			: base(factory, sQLFilter)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public new StmALog this[int index]
		{
			get { return (StmALog)Elements[index]; }
		}

		public virtual new StmALog AddNew()
		{
			return (StmALog)base.AddNew();
		}

		public void CancelAll()
		{
			foreach (StmALog log in this)
			{
				if (!log.SL_IsCancelled)
				{
					log.Cancel();
				}
			}
		}
	}

	public class StmALogCollectionWithMaster : StmALogCollection
	{
		public StmALogCollectionWithMaster(IStmALogParent master)
			: base(((BusinessObject)master).Factory)
		{
			Master = master;
		}

		public IStmALogParent Master { get; private set; }
	}

	internal class StmALogCollectionWithRelatedElements : StmALogCollectionWithMaster, IBusinessObjectCollectionWithRelatedElements
	{
		public StmALogCollectionWithRelatedElements(IStmALogParent master)
			: base(master)
		{
		}

		#region IBusinessObjectCollectionWithRelatedElements Members

		public void RemoveAllRelatedElements()
		{
			using (SuspendListChanged())
			{
				foreach (StmALog log in Elements.ToArray())
				{
					if (Contains(log) && log.SL_Parent != ((BusinessObject)Master).PK)
					{
						Remove(log);
					}
				}
			}
		}

		#endregion
	}
}
