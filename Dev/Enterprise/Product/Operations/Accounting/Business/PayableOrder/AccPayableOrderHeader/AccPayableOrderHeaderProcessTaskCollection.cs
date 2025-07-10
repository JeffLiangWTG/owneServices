using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public AccPayableOrderHeaderProcessTaskCollection(AccPayableOrderHeader order) : base(order) { }

		public new AccPayableOrderHeaderProcessTask this[int index]
		{
			get { return (AccPayableOrderHeaderProcessTask)Elements[index]; }
		}

		public new AccPayableOrderHeaderProcessTask AddNew()
		{
			return (AccPayableOrderHeaderProcessTask)base.AddNew();
		}

		public new AccPayableOrderHeader Parent
		{
			get { return (AccPayableOrderHeader)base.Parent; }
		}
	}
}


