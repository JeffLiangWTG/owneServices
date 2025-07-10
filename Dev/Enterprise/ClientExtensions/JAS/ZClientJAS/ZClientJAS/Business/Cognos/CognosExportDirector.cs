using System;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosExportDirector
	{
		public CognosExportDirector(ICognosNotificationSubscriber notifications)
		{
			this.Notifications = notifications;
		}

		public virtual bool Export(ZDateTime exportStartDateTime, ZString exportFilePath)
		{
			bool result = false;

			EnsureValidParams(exportStartDateTime, exportFilePath);
			CognosPreExportCheck preExportCheck = GetNewCognosPreExportCheck();
			if (preExportCheck.EnsureCanExport())
			{
				using (new ProcessStartFinishNotifier(Notifications, "Exporting", true, false))
				{
					CognosFileExporter exporter = GetNewCognosFileExporter(exportStartDateTime, Notifications);
					result = exporter.ExportToFile(exportFilePath);
					if (result)
					{
						Notifications.CompleteProgress();
					}
				}
			}

			return result;
		}

		#region Implementation

		protected virtual CognosFileExporter GetNewCognosFileExporter(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications)
		{
			return new CognosFileExporter(exportStartDateTime, notifications);
		}

		void EnsureValidParams(ZDateTime exportStartDateTime, ZString exportFilePath)
		{
			if (exportStartDateTime.IsEmpty || exportFilePath.IsEmpty)
			{
				throw new ArgumentException("ExportStartDateTime and ExportFilePath cannot be empty");
			}
		}

		protected virtual CognosPreExportCheck GetNewCognosPreExportCheck()
		{
			return new CognosPreExportCheck(Notifications);
		}

		#endregion

		public readonly ICognosNotificationSubscriber Notifications;
	}
}
