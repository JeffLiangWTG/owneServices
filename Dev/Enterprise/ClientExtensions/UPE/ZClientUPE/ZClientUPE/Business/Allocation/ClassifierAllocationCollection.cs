
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class ClassifierAllocationCollection : NonPersistentBusinessObjectCollection<ClassifierAllocation>	{
		public ClassifierAllocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public ClassifierAllocationCollection GetIncludedForAllocation()
		{
			ClassifierAllocationCollection result = new ClassifierAllocationCollection(Factory);
			foreach (ClassifierAllocation classifierAllocation in this)
			{
				if (classifierAllocation.IncludeForAllocation)
				{
					result.Add(classifierAllocation);
				}
			}
			return result;
		}

		public ClassifierAllocationCollection GetExcludedForAllocation()
		{
			ClassifierAllocationCollection result = new ClassifierAllocationCollection(Factory);
			foreach (ClassifierAllocation classifierAllocation in this)
			{
				if (!classifierAllocation.IncludeForAllocation)
				{
					result.Add(classifierAllocation);
				}
			}
			return result;
		}

		public ClassifierAllocation ClassifierWithLowestAllocation
		{
			get
			{
				ClassifierAllocation result = null;
				int numberAllocated = int.MaxValue;
				foreach (ClassifierAllocation classifierAllocation in this)
				{
					if (classifierAllocation.NumberAllocated < numberAllocated)
					{
						result = classifierAllocation;
						numberAllocated = classifierAllocation.NumberAllocated;
					}
				}
				return result;
			}
		}

		public ClassifierAllocation ClassifierWithHighestAllocation
		{
			get
			{
				ClassifierAllocation result = null;
				int numberAllocated = -1;
				foreach (ClassifierAllocation classifierAllocation in this)
				{
					if (classifierAllocation.NumberAllocated > numberAllocated)
					{
						result = classifierAllocation;
						numberAllocated = classifierAllocation.NumberAllocated;
					}
				}
				return result;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClassifierAllocation(Factory);
		}
	}
}
