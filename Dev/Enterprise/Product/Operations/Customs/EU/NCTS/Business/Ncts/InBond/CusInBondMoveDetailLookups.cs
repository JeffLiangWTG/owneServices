using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondMoveDetailLookups : Customs.Business.CusInBondMoveDetailLookups
	{
		public CusInBondMoveDetailLookups(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}

		public CodeDescriptionPairList UnloadedStatesList
		{
			get
			{
				var parent = Parent;
				var isNew = parent.B9_UnloadedStateInfo.OriginalValue.ToString() == ZString.Empty
					|| parent.B9_UnloadedStateInfo.OriginalValue.ToString() == NctsUnloadedStateListForHouseConsignment.Codes.NEW;

				return Factory.GetCachedValue(nameof(UnloadedStatesList) + isNew, () =>
				{
					var list = new NctsUnloadedStateListForHouseConsignment();
					if (!isNew)
					{
						list.RemoveCode(NctsUnloadedStateListForHouseConsignment.Codes.NEW);
					}
					return list;
				});
			}
		}

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);
	}
}
