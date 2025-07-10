using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IRNSRequestParent : IMessageManageableBizObj
	{
		IUserNotification Notification { get; set; }
		bool IsStatusQuery { get; set; }

		BusinessObject ParentBusinessObject { get; }
		IEnumerable<IRNSRequest> GetRNSRequestCollections();

		bool ShouldDefaultChooseToSent(SingleMessageManager messageManager);

		bool ShouldWaitUntilResponded { get; }
	}
}

