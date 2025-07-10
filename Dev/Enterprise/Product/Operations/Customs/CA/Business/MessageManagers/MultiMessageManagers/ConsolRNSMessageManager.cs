using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class ConsolRNSMessageManager : MultiMessageManager
	{
		public ConsolRNSMessageManager(IRNSRequestParent parent, IEnumerable<RNSRequestBO> requestBOs)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(requestBOs, "requestBOs");

			this.rnsRequestParent = parent;
			this.requestBOs = requestBOs;
		}
		readonly IRNSRequestParent rnsRequestParent;
		readonly IEnumerable<RNSRequestBO> requestBOs;

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

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var messageManagers = new List<SingleMessageManager>();
			foreach (var rnsRequest in requestBOs)
			{
				var singleManager = new RNSMessageManager(rnsRequest, rnsRequestParent.Notification, rnsRequestParent.IsStatusQuery, rnsRequest.GetMessageErrors().Select(x => x.Message));
				singleManager.SetShouldWaitUntilResponded(rnsRequestParent.ShouldWaitUntilResponded);
				messageManagers.Add(singleManager);
			}
			return messageManagers.ToArray();
		}
	}
}
