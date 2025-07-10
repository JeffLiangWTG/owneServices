using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchProcessTaskCollection : ProcessTaskCollection
	{
		public AccCollectionBatchProcessTaskCollection(AccCollectionBatch batch) : base(batch) { }

		public new AccCollectionBatchProcessTask this[int index]
		{
			get { return (AccCollectionBatchProcessTask)Elements[index]; }
		}

		public new AccCollectionBatchProcessTask AddNew()
		{
			return (AccCollectionBatchProcessTask)base.AddNew();
		}

		public new AccCollectionBatch Parent
		{
			get { return (AccCollectionBatch)base.Parent; }
		}
	}
}


