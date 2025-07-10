using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderProcessTaskCollection : ProcessTaskCollection
	{
		public AccCollectionOrderProcessTaskCollection(AccCollectionOrder order) : base(order) { }

		public new AccCollectionOrderProcessTask this[int index]
		{
			get { return (AccCollectionOrderProcessTask)Elements[index]; }
		}

		public new AccCollectionOrderProcessTask AddNew()
		{
			return (AccCollectionOrderProcessTask)base.AddNew();
		}

		public new AccCollectionOrder Parent
		{
			get { return (AccCollectionOrder)base.Parent; }
		}
	}
}
