using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Integration
{
	public class JobProfitLossCollection : NonPersistentBusinessObjectCollection<NonPersistentBusinessObject>
	{
		public JobProfitLossCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new IJobProfitLoss this[int index]
		{
			get { return (IJobProfitLoss)Elements[index]; }
		}

		public new IJobProfitLoss AddNew()
		{
			throw new NotSupportedException("Cannot add elements to this collection.");
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}
	}
}
