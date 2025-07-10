//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHHouseContainerPivotLookups
//
//    This class should be used for overriding collections in AutoCusCAeMHHouseContainerPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotLookups : AutoCusCAeMHHouseContainerPivotLookups
	{
		public CusCAeMHHouseContainerPivotLookups(AutoCusCAeMHHouseContainerPivot parent) : base(parent)
		{
		}

		protected new CusCAeMHHouseContainerPivot Parent
		{
			get { return (CusCAeMHHouseContainerPivot)base.Parent; }
		}

		public ActiveBusinessObjectCollection<CusCAeMHContainer> Containers
		{
			get
			{
				var house = Parent.HouseBill;
				var master = house != null ? house.MasterBill : null;
				return master != null ? master.Containers : EmptyContainerCollection;
			}
		}

		ActiveBusinessObjectCollection<CusCAeMHContainer> EmptyContainerCollection
		{
			get
			{
				if (fEmptyContainerCollection == null)
				{
					var query = new ZQuery();
					query.IsNoResultQuery = true;
					fEmptyContainerCollection = new ActiveBusinessObjectCollection<CusCAeMHContainer>(Factory, query);
				}
				return fEmptyContainerCollection;
			}
		}
		ActiveBusinessObjectCollection<CusCAeMHContainer> fEmptyContainerCollection;
	}
}
