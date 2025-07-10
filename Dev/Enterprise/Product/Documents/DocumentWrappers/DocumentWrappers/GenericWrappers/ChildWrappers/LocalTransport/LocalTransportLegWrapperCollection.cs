using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LocalTransportLegWrapperCollection : GenericWrapperCollection<LocalTransportLegWrapper>
	{
		public LocalTransportLegWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LocalTransportLegWrapperCollection(FreightWrapper parentWrapper, BusinessObjectFactory factory, IEnumerable<CommonCartageLeg> legCollection)
			: base(factory)
		{
			foreach (CommonCartageLeg leg in legCollection)
			{
				if (!ContainsWrappedObject(leg.PK))
				{
					LocalTransportLegWrapper wrapper = new LocalTransportLegWrapper(leg, Factory);
					wrapper.ParentWrapper = parentWrapper;
					Add(wrapper);
				}
			}
		}

		#region SortOnPlannedPickupTime

		public enum SortBy
		{
			PlannedPickup,
			Sequence,
			DisplayOrder
		}

		public void Sort(SortBy sortBy)
		{
			switch (sortBy)
			{
				case SortBy.PlannedPickup:
					Sort(new PlannedPickupTimeSorter());
					break;
				case SortBy.Sequence:
					Sort(delegate(LocalTransportLegWrapper a, LocalTransportLegWrapper b)
					{ return a.Sequence.CompareTo(b.Sequence); });
					break;
				case SortBy.DisplayOrder:
					Sort(delegate(LocalTransportLegWrapper a, LocalTransportLegWrapper b)
					{
						var aMove = a.BookedMove;
						var bMove = b.BookedMove;
						var result = aMove != null && bMove != null ? aMove.DisplayOrder.CompareTo(bMove.DisplayOrder) : 0;
						return result != 0 ? result : a.DisplayOrder.CompareTo(b.DisplayOrder);
					});
					break;
			}
		}

		class PlannedPickupTimeSorter : IComparer
		{
			public int Compare(object a, object b)
			{
				var legA = (LocalTransportLegWrapper)a;
				var legB = (LocalTransportLegWrapper)b;
				var timeA = legA.PickupTimeIn.IsEmpty ? legA.PlannedPickupTime : legA.PickupTimeIn;
				var timeB = legB.PickupTimeIn.IsEmpty ? legB.PlannedPickupTime : legB.PickupTimeIn;

				if (timeA.IsEmpty && !timeB.IsEmpty)
				{
					return 1;
				}
				else if (!timeA.IsEmpty && timeB.IsEmpty)
				{
					return -1;
				}
				else
				{
					int result = timeA.CompareTo(timeB);
					if (result == 0)
					{
						result = legA.Cartage.JobNumber.CompareTo(legB.Cartage.JobNumber);
					}
					return result;
				}
			}
		}

		#endregion
	}
}
