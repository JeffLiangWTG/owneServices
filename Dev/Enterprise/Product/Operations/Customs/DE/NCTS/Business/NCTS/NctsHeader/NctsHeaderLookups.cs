using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent) : base(parent)
		{
		}

		protected new NctsHeader Parent => (NctsHeader)base.Parent;

		public override CodeDescriptionPairList EventFlagList
		{
			get
			{
				var hasEventLogY = Parent.EventFlagYesLog != null;
				return Factory.GetCachedValue("EventFlagList_" + hasEventLogY, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(EU.NCTS.Business.EventFlagList.Codes.Yes, EU.NCTS.Business.EventFlagList.Descriptions.Yes);
					if (hasEventLogY)
					{
						result.AddPair(EU.NCTS.Business.EventFlagList.Codes.Cancelled, EU.NCTS.Business.EventFlagList.Descriptions.Cancelled);
					}
					else
					{
						result.AddPair(EU.NCTS.Business.EventFlagList.Codes.No, EU.NCTS.Business.EventFlagList.Descriptions.No);
					}

					return result;
				});
			}
		}

		public override CodeDescriptionPairList NctsMessageStatusList
		{
			get =>
				Factory.GetCachedValue($"DE.NctsMessageStatusList_{Parent.BH_HeaderType}",
					() =>
					{
						var list = new CodeDescriptionPairList(base.NctsMessageStatusList);
						if (Parent.IsDepartureMovement)
						{
							list.AddPairIfNotExist(EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent,
								EU.NCTS.Business.NctsMessageStatusList.Descriptions.DepartureDeclarationNotSent);
						}
						else
						{
							list.AddPairIfNotExist(EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationNotSent,
								EU.NCTS.Business.NctsMessageStatusList.Descriptions.ArrivalNotificationNotSent);
						}
						return list;
					});
		}
	}
}
