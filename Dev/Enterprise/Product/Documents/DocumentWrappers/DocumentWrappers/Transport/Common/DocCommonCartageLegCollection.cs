using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonCartageLegCollection : DocumentWrapperCollection<DocCommonCartageLeg>
	{
		public DocCommonCartageLegCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCommonCartageLegCollection(CommonBookedCtgMove move)
			: base(move.Factory)
		{
			foreach (CommonCartageLeg leg in move.CartageLegs)
			{
				Add(DocCommonCartageLeg.New(leg, Factory));
			}
		}

		public DocCommonCartageLegCollection(CommonWorkSheet workSheet)
			: base(workSheet.Factory)
		{
			foreach (CommonCartageLeg leg in workSheet.CartageLegs)
			{
				Add(DocCommonCartageLeg.New(leg, Factory));
			}
		}

		public DocCommonCartageLegCollection(CommonCartage cartage)
			: base(cartage.Factory)
		{
			foreach (CommonCartageLeg leg in cartage.CartageLegs)
			{
				Add(DocCommonCartageLeg.New(leg, Factory));
			}
		}

		public void SortOnPlannedPickupTime()
		{
			this.Sort(new CartageLegPlannedPickupTimeSorter());
		}

		class CartageLegPlannedPickupTimeSorter : System.Collections.IComparer
		{
			public int Compare(object a, object b)
			{
				DocCommonCartageLeg legA = (DocCommonCartageLeg)a;
				DocCommonCartageLeg legB = (DocCommonCartageLeg)b;
				ZDateTime timeA = legA.PickupTimeIn.IsEmpty ? legA.PlannedPickupTime : legA.PickupTimeIn;
				ZDateTime timeB = legB.PickupTimeIn.IsEmpty ? legB.PlannedPickupTime : legB.PickupTimeIn;

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
						result = legA.CartageNumber.CompareTo(legB.CartageNumber);
					}
					return result;
				}
			}
		}
	}
}
