using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.Customs.GB.CDS.Messaging.MessageManagers;
using Enterprise.MasterFiles.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Organisation
{
	public class OrgHeaderCheckerSendingManager
	{
		public OrgHeaderCheckerSendingManager(NonPersistentOrgHeaderChecker orgHeaderChecker)
		{
			this.orgHeaderChecker = Argument.NotNull(orgHeaderChecker, nameof(orgHeaderChecker));
		}

		public MessageSendingNotificationCollection SendingNotifications { get; } = [];

		public void Send()
		{
			SendingNotifications.Clear();
			var factory = orgHeaderChecker.Factory;
			using var transactionManager = ((ITransactionStarter)factory).BeginTransactionWithManager();
			foreach (var code in orgHeaderChecker.CodesForQuery)
			{
				using var universalEvent = GetUniversalEventForOrgCusCode(code);
				if (universalEvent != null)
				{
					var message = factory.New<CDSDISQueryMessage>();
					message.EM_ApplicationReference = code.PK.ToString();

					if (CDSQuerySendingHelper.Deliver(factory, universalEvent, orgHeaderChecker.OrgHeader, Constants.EDIInterchange.GBCustoms, SendingNotifications, message))
					{
						code.MarkOrgCusCodeVerifiedOrUnVerified(false, string.Empty, string.Empty);
					}
					else
					{
						break;
					}
				}
			}
			factory.Save();
			transactionManager.CommitTransaction();
		}

		public UniversalEvent GetUniversalEventForOrgCusCode(OrgCusCode orgCusCode)
		{
			var builder = new OrgHeaderCheckerUniversalEventBuilder(orgHeaderChecker.OrgHeader, orgCusCode);
			return builder.BuildUniversalEvent();
		}

		readonly NonPersistentOrgHeaderChecker orgHeaderChecker;
	}
}
