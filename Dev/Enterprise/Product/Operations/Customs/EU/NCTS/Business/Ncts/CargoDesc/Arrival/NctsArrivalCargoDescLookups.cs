using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalCargoDescLookups : NctsCommonCargoDescLookups
	{
		public NctsArrivalCargoDescLookups(NctsArrivalCargoDesc parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList UnloadedStates
		{
			get
			{
				var parent = Parent as NctsArrivalCargoDesc;
				var isNew = parent.BY_UnloadedStateInfo.OriginalValue.ToString() == NctsUnloadedStateListForHouseConsignment.Codes.NEW;
				return Factory.GetCachedValue(nameof(UnloadedStates) + isNew, () =>
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
	}
}
