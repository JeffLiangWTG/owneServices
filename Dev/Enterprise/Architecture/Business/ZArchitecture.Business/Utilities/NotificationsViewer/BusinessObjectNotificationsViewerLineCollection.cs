using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class BusinessObjectNotificationsViewerLineCollection : NonPersistentBusinessObjectCollection<BusinessObjectNotificationsViewerLine>
	{
		public BusinessObjectNotificationsViewerLineCollection(BusinessObjectNotificationsViewerSupporter supporter)
			: base(supporter.Factory)
		{
			this.supporter = supporter;
		}
		readonly BusinessObjectNotificationsViewerSupporter supporter;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			RemoveAndDeleteAll();

			var businessObjectLoaded = 0;
			var numberOfBusinessObjects = supporter.BusinessObjectCollection.Count;
			foreach (BusinessObject bizObj in supporter.BusinessObjectCollection)
			{
				businessObjectLoaded++;
				LoadNotificationsFromBizObj(bizObj);
				UpdateProgressStatusAndPercent(Res.GetString("6e7a5bca-8534-4263-9021-0779a86ae618", "Loading notifications from {0}", bizObj.ToString()), (int)(businessObjectLoaded * 100F / numberOfBusinessObjects));
			}

			UpdateProgressStatusAndPercent(Res.GetString("977513ad-0a3e-4bbf-949d-34de81acc6c6", "Loading notifications completed"), 100);
		}

		public void LoadNotificationsFromBizObj(BusinessObject bizObj)
		{
			var humanReadableColumnFieldNamesCount = supporter.HumanReadableColumnFieldNames.Count;
			var humanReadableColumn1FieldName = humanReadableColumnFieldNamesCount > 0 ? supporter.HumanReadableColumnFieldNames[0] : ZString.Empty;
			var humanReadableColumn2FieldName = humanReadableColumnFieldNamesCount > 1 ? supporter.HumanReadableColumnFieldNames[1] : ZString.Empty;

			void SetHumanReadableHumanReadableColumn(ZString humanReadableColumn, ZPropertyInfo humanReadableColumnPropertyInfo)
			{
				if (!humanReadableColumn.IsEmpty)
				{
					humanReadableColumnPropertyInfo.Value = (ZString)(bizObj[humanReadableColumn].ToString());
				}
			}

			void PopulateNotificationsFromBizObj()
			{
				if (bizObj != null && !bizObj.IsDeleted && bizObj.NotificationsIncludingChildren.Any())
				{
					var notificationsCollector = new ZNotificationCollector(bizObj, true, false);
					var notificationsIncludingChildren = notificationsCollector.GetUniqueNotifications();
					foreach (var notification in notificationsIncludingChildren)
					{
						var notificationLine = new BusinessObjectNotificationsViewerLine(bizObj);
						notificationLine.NotificationType = ZNotificationsExtensions.NotificationTypeName(notification.Type, PluralState.NonPlural);
						notificationLine.NotificationMessage = notification.Message;
						SetHumanReadableHumanReadableColumn(humanReadableColumn1FieldName, notificationLine.HumanReadableColumn1Info);
						SetHumanReadableHumanReadableColumn(humanReadableColumn2FieldName, notificationLine.HumanReadableColumn2Info);
						this.Add(notificationLine);
					}
				}
			}

			PopulateNotificationsFromBizObj();
		}

		void UpdateProgressStatusAndPercent(string statusMessage, int percent)
		{
			if (supporter.LoadingNotificationsProcessAction != null)
			{
				supporter.LoadingNotificationsProcessAction(statusMessage, percent);
			}
		}
	}
}
