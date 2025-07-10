using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class RNSMultiMessageManager : MultiMessageManager
	{
		public RNSMultiMessageManager(IRNSRequestParent parent)
		{
			Argument.NotNull(parent, "parent");

			this.rnsRequestParent = parent;
		}

		readonly IRNSRequestParent rnsRequestParent;

		public IRNSRequestParent RNSRequestParent
		{
			get
			{
				return this.rnsRequestParent;
			}
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return this.rnsRequestParent; }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var messageManagers = new List<SingleMessageManager>();

			if (rnsRequestParent != null)
			{
				foreach (IRNSRequest rnsRequest in rnsRequestParent.GetRNSRequestCollections())
				{
					var singleManager = new RNSMessageManager(rnsRequest, rnsRequestParent.Notification, rnsRequestParent.IsStatusQuery);
					singleManager.SetShouldWaitUntilResponded(rnsRequestParent.ShouldWaitUntilResponded);
					messageManagers.Add(singleManager);
				}
			}

			return messageManagers.ToArray();
		}
	}
}
