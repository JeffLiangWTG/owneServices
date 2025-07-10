using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public sealed class AESMessageSendingActionParent : CusEntryHeaderMessageSendingActionParent<AESMessageSendingAction>
	{
		public AESMessageSendingActionParent(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(AESMessageSendingActionCollection);

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new AESNotificationCollector(base.JobDeclaration).GetMessageErrors();
		}

		internal class AESNotificationCollector : Customs.Business.CustomsNotificationCollector
		{
			public AESNotificationCollector(BusinessObject topBuzObj)
				: base(topBuzObj, true, false, PropertyDescriptionType.HumanReadableName)
			{
			}

			protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
			{
				return base.ShouldIncludeNotificationsFromObject(businessObject)
					&& !(businessObject is Integration.Customs.IEExitControl.ICusExitHeader);
			}
		}
	}
}
