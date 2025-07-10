using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture
{
	[Enterprise.Core.NonSerializedClass]
	public abstract class BusinessObjectAfterSaveNotification : NotificationSubscriberNotification, INotificationWithMessageForAfterSave
	{
		public BusinessObjectAfterSaveNotification(BusinessObject bizObj, NotificationSubscriberType notifyType) : base(notifyType, String.Empty)
		{
			businessEntity = bizObj;
			if (BusinessEntity.Factory != null)
			{
				BusinessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(AfterFactorySaved);
			}
		}

		void AfterFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				CachedDisplayMessage = Message;
				CachedMultiLineDisplayMessage = MultiLineDisplayMessage;
				FactoryHasSaved = true;
				if (businessEntity != null && businessEntity.Factory != null)
				{
					businessEntity.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(AfterFactorySaved);
				}
				businessEntity = null;
			}
		}

		bool FactoryHasSaved;

		public BusinessObject BusinessEntity
		{
			get
			{
				if (FactoryHasSaved)
				{
					ErrorReporter.ReportOnce("Attempted to access business object after save", "Attempted to access business object after save");
				}
				return businessEntity;
			}
		}

		internal BusinessObject businessEntity;
	}
}
