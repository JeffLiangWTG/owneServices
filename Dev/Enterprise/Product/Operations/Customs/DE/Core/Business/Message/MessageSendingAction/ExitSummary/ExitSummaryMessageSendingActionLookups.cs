using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class ExitSummaryMessageSendingActionLookups : ZLookups
	{
		public ExitSummaryMessageSendingActionLookups(ExitSummaryMessageSendingAction action) : base(action)
		{
		}

		new ExitSummaryMessageSendingAction Parent => (ExitSummaryMessageSendingAction)base.Parent;

		public CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var isStatus301 = Parent.Status == UniversalReferenceConstants.CusExitDetailStatus._301;
				return Factory.GetCachedValue("Enterprise.Customs.DE.Business.MessageTypeList_" + isStatus301, () =>
				{
					var result = new CodeDescriptionPairList();
					if (isStatus301)
					{
						result.AddPair(ExitSummaryMessageTypeList.Codes.Presentation, ExitSummaryMessageTypeList.Descriptions.Presentation);
					}
					else
					{
						result = ExitSummaryMessageTypeList.ArrivalMessageTypes;
					}
					return result;
				});
			}
		}
	}
}
