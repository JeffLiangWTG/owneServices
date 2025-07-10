using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Business
{
	public class DataImporterBusinessObject : NonPersistentBusinessObject, IObsoleteValidation, INotifications, INotificationSubscriberQueryUser
	{
		public DataImporterBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZString ImportOperationDescription;

		public void OnBeforeImport()
		{
			ResetProgressText();
			AppendProgressText(ImportOperationDescription.IsEmpty ? "" : (ImportOperationDescription + "\r\n\r\n"));
			RecordsAdded = 0;
			RecordsUpdated = 0;
			DelayShowNotificationInfos.Clear();
		}

		public void OnAfterImport(bool fatalErrorOccurred)
		{
			ShowDelayedNotifications();

			if (fatalErrorOccurred)
			{
				AppendProgressText(ProgressMessageForFatalError);
				RecordsAdded = 0;
				RecordsUpdated = 0;
			}
			else
			{
				AppendProgressText(ProgressMessageForSuccessfulImport);
			}
		}

		void ShowDelayedNotifications()
		{
			foreach (INotification info in DelayShowNotificationInfos)
			{
				if (!string.IsNullOrEmpty(info.Message) || info is NewlineNotification)
				{
					AppendProgressText(info.Message.Replace("\r\n", "\n").Replace("\n", "\r\n") + "\r\n");
				}
			}
			DelayShowNotificationInfos.Clear();
		}

		protected virtual ZString ProgressMessageForFatalError
		{
			get { return "\r\n" + Res.GetString("69e2ae39-ffed-4ebe-a6be-92349d0ea049", "No changes were made due to the above errors. Please fix the errors and try again.") + "\r\n"; }
		}

		protected virtual ZString ProgressMessageForSuccessfulImport
		{
			get { return "\r\n" + Res.GetString("3ea35e04-57ae-41ff-b40d-a05478e16312", "Data Import completed. See notifications above") + "\r\n"; }
		}

		#region ProgressText

		public class TextAppendedEventArgs : EventArgs
		{
			public TextAppendedEventArgs(string newText)
			{
				NewText = newText;
			}

			public string NewText
			{
				get;
				private set;
			}
		}

		public event EventHandler<TextAppendedEventArgs> OnAppendProgressText;

		void AppendProgressText(string appendingText)
		{
			if (OnAppendProgressText != null)
			{
				OnAppendProgressText(this, new TextAppendedEventArgs(appendingText));
			}
		}

		public event EventHandler OnResetProgressText;

		void ResetProgressText()
		{
			if (OnResetProgressText != null)
			{
				OnResetProgressText(this, EventArgs.Empty);
			}
		}

		#endregion

		#region RecordsAdded

		[ReadOnly(true)]
		public ZInt RecordsAdded
		{
			get { return recordsAdded; }
			set { SetNonPersistentPropertyValue(RecordsAddedInfo, ref recordsAdded, value); }
		}
		ZInt recordsAdded;

		public ZPropertyInfo RecordsAddedInfo
		{
			get { return GetZPropertyInfo(nameof(RecordsAdded)); }
		}

		#endregion

		#region RecordsUpdated

		[ReadOnly(true)]
		public ZInt RecordsUpdated
		{
			get { return recordsUpdated; }
			set { SetNonPersistentPropertyValue(RecordsUpdatedInfo, ref recordsUpdated, value); }
		}
		ZInt recordsUpdated;

		public ZPropertyInfo RecordsUpdatedInfo
		{
			get { return GetZPropertyInfo(nameof(RecordsUpdated)); }
		}

		#endregion

		#region INotifications Members

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
		}

		void INotifications.Add(INotification notification)
		{
			INotificationWithMessageForAfterSave afterSaveNotification = notification as INotificationWithMessageForAfterSave;
			if (afterSaveNotification != null)
			{
				if (!(notification is BusinessObjectCreatedOrUpdatedNotification && ((BusinessObjectCreatedOrUpdatedNotification)notification).UpdateRecordCountOnlyWithoutMessage))
				{
					DelayShowNotificationInfos.Add(notification);
				}
				if (notification is BusinessObjectCreatedOrUpdatedNotification)
				{
					if (((BusinessObjectCreatedOrUpdatedNotification)notification).WasInDatabase)
					{
						RecordsUpdated++;
					}
					else
					{
						RecordsAdded++;
					}
				}
			}
			else
			{
				ShowNotification(notification);
			}
		}
		readonly ArrayList DelayShowNotificationInfos = new ArrayList();

		void ShowNotification(INotification notification)
		{
			if (!string.IsNullOrEmpty(notification.Message) || notification is NewlineNotification)
			{
				string message = notification.Message.Replace("\r\n", "\n").Replace("\n", "\r\n");
				AppendProgressText(message + "\r\n");
			}
		}

		#endregion
	}
}
