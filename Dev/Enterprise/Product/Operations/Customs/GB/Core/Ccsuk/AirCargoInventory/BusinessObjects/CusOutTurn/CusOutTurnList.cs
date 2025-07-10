using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusOutTurnList : List<CusOutTurn>
	{
		public int TotalDelivered
		{
			get { return (from CusOutTurn outTurn in this where outTurn.IsDelivered select (int)outTurn.C5_PackagesOutturned).Sum(); }
		}
	}
}
