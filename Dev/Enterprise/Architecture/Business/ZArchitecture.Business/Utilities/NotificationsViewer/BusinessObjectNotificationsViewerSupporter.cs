using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business
{
	public class BusinessObjectNotificationsViewerSupporter : NonPersistentBusinessObject
	{
		public BusinessObjectNotificationsViewerSupporter(IBusinessObjectCollectionNotificationsViewerProvider provider)
			: base(new BusinessObjectFactory())
		{
			this.BusinessObjectCollection = provider;

			LoadHumanReadableColumnCaptionsAndWidth();
		}

		#region Methods

		void LoadHumanReadableColumnCaptionsAndWidth()
		{
			fHumanReadableColumnFieldNames = new List<ZString>();
			fHumanReadableColumnCaptionsAndWidth = new List<(ResourceStringData, ZInt)>();
			foreach (var humanReadableColumn in BusinessObjectCollection.HumanReadableColumns)
			{
				fHumanReadableColumnFieldNames.Add(humanReadableColumn.FieldName);
				fHumanReadableColumnCaptionsAndWidth.Add((humanReadableColumn.Caption, humanReadableColumn.ColumnWidth));
			}
		}
		List<ZString> fHumanReadableColumnFieldNames;
		List<(ResourceStringData, ZInt)> fHumanReadableColumnCaptionsAndWidth;

		#endregion

		#region Properties

		public Action<string, int> LoadingNotificationsProcessAction { get; set; }

		public IBusinessObjectCollectionNotificationsViewerProvider BusinessObjectCollection { get; }

		public List<ZString> HumanReadableColumnFieldNames => fHumanReadableColumnFieldNames ?? (fHumanReadableColumnFieldNames = new List<ZString>());

		public List<(ResourceStringData Caption, ZInt ColumnWidth)> HumanReadableColumnCaptionsAndWidth => fHumanReadableColumnCaptionsAndWidth ?? (fHumanReadableColumnCaptionsAndWidth = new List<(ResourceStringData, ZInt)>());

		#endregion

		#region NotificationsCollection

		[ChildEditable(false)]
		public BusinessObjectNotificationsViewerLineCollection NotificationsCollection
		{
			get
			{
				if (fNotificationCollection == null)
				{
					fNotificationCollection = new BusinessObjectNotificationsViewerLineCollection(this);
					fNotificationCollection.Load();
				}
				return fNotificationCollection;
			}
		}
		BusinessObjectNotificationsViewerLineCollection fNotificationCollection;

		#endregion
	}
}
