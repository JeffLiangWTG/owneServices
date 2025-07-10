//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHHouseContainerPivotValidation
//
//    This class should be used for overriding validation in AutoCusCAeMHHouseContainerPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotValidation : AutoCusCAeMHHouseContainerPivotValidation
	{
		public CusCAeMHHouseContainerPivotValidation(AutoCusCAeMHHouseContainerPivot parent) : base(parent)
		{
		}

		protected new CusCAeMHHouseContainerPivot Parent
		{
			get { return (CusCAeMHHouseContainerPivot)base.Parent; }
		}

		protected override void CheckBPA_BQ_Container()
		{
			base.CheckBPA_BQ_Container();
			var houseBill = Parent.HouseBill;
			if (!Parent.BPA_BQ_Container.IsEmpty && houseBill != null && houseBill.Pivots.Count(x => x.BPA_BQ_Container == Parent.BPA_BQ_Container) > 1)
			{
				Parent.BPA_BQ_ContainerInfo.AddError(Res.GetString("21C98648-E929-48FA-AF6B-C56E5F2D55D2", "This container is already linked to the house bill."));
			}
		}
	}
}
