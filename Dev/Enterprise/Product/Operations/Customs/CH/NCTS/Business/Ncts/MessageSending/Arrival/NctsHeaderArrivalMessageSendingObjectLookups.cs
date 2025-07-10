using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using AllMessageTypeList = Enterprise.Customs.CH.Business.PassarMessageTypeList;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderArrivalMessageSendingObjectLookups : NctsHeaderCommonMessageSendingObjectLookups
{
	public NctsHeaderArrivalMessageSendingObjectLookups(NctsHeaderArrivalMessageSendingObject parent) : base(parent)
	{
	}

	new NctsHeaderArrivalMessageSendingObject Parent => (NctsHeaderArrivalMessageSendingObject)base.Parent;

	public new CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var nctsArrivalMovementHeader = Parent?.NctsHeader?.ArrivalMovementHeader;

			if (nctsArrivalMovementHeader?.IsClosedRelease ?? false)
			{
				return new CodeDescriptionPairList();
			}

			var multipleMRNIndicator = nctsArrivalMovementHeader?.MultipleMRNIndicator ?? ZBool.False;

			if (multipleMRNIndicator)
			{
				return Factory?.GetCachedValue("CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectLookups.MessageTypeList|" + multipleMRNIndicator,
					() => new CodeDescriptionPairList() { new CodeDescriptionPair(AllMessageTypeList.Codes.NT007, AllMessageTypeList.Descriptions.NT007) });
			}
			else
			{
				return Factory?.GetCachedValue("CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectLookups.MessageTypeList|" + multipleMRNIndicator,
					() => new CodeDescriptionPairList() { new CodeDescriptionPair(AllMessageTypeList.Codes.NT044, AllMessageTypeList.Descriptions.NT044) });
			}
		}
	}
}
