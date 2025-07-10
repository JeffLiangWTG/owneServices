using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMessageSendingNotificationCollector : Customs.Business.JobDeclarationMessageSendingNotificationCollector
	{
		public JobDeclarationMessageSendingNotificationCollector(JobDeclaration declaration, IEnumerable<CusEntryHeader> selectedEntries) : base(declaration, selectedEntries)
		{
		}

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			return !(businessObject is MasterFiles.Business.JobHeader) && base.ShouldIncludeNotificationsFromObject(businessObject);
		}
	}
}
