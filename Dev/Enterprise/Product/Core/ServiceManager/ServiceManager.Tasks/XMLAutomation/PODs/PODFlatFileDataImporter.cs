using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class PODFlatFileDataImporter : FlatFileImporter
	{
		protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
		{
			PODDataLoader dataLoader = new PODDataLoader();

			NotificationBuffer buffer = notifications as NotificationBuffer;

			SetInfoNotification(buffer, "====================== POD File Import ======================");

			dataLoader.ImportPODData(dataFile.FullName);

			SendEmailToNotificationGroup(dataFile.FullName, dataLoader);

			OutputFinalTotals(dataLoader, buffer);

			SetInfoNotification(buffer, Res.GetString("a117cb95-5933-4c23-a20b-5ab6063a0e58", "Import Result is sent to the Nominated Notification Group"));
			SetInfoNotification(buffer, "=============================================================");
		}

		void SetInfoNotification(NotificationBuffer buffer, ZString message)
		{
			if (buffer != null)
			{
				InfoNotification info = new InfoNotification(message);
				buffer.Notify(info);
			}
		}

		void OutputFinalTotals(PODDataLoader dataLoader, NotificationBuffer buffer)
		{
			int count = dataLoader.Log.Count;
			ZString mesg = dataLoader.Log[count - 1];

			if (mesg.StartsWith("\r\nT O T A L :"))
			{
				SetInfoNotification(buffer, mesg.ExcludeChars("\r\n"));
			}
		}

		bool IsEmailToBeSentOnSuccess
		{
			get { return !SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value; }
		}

		#region SendEmailToNotificationGroup

		void SendEmailToNotificationGroup(ZString fileName, PODDataLoader dataLoader)
		{
			EmailDef email = new EmailDef();

			email.Attachments.Add(new AttachmentDef(fileName));

			if (!dataLoader.FileHeaderIsValid)
			{
				using (TempFile templateFile = TempFile.New())
				{
					dataLoader.WriteCSVTemplate(templateFile.Filename);
					email.Body = Res.GetString("4e651bf0-bb21-47a9-ab0c-92c8c1554454", "File Header information is incorrect. The import of POD data requires a specific .CSV format file. \r\nPlease check the format in the template file attached.");
					email.Body += System.Environment.NewLine + System.Environment.NewLine + GetLogMessage(dataLoader);
					email.Attachments.Add(new AttachmentDef("Template File.csv", templateFile.Filename));
				}
			}
			else
			{
				email.Body = GetLogMessage(dataLoader);
			}

			bool importFailed = !dataLoader.FileHeaderIsValid || dataLoader.HasErrors;
			email.Subject = Res.GetString("3997be54-9a29-4afc-904c-ecc05ab87c4f", "POD CSV Import {0} - {1}", importFailed ? Res.GetString("cdef02d7-b0f9-417e-8fbd-992fd863e674", "Failed") : Res.GetString("3288cc39-7c98-4059-91ed-f69e8619b7f1", "Succeeded"), Path.GetFileName(fileName));

			if (importFailed || IsEmailToBeSentOnSuccess)
			{
				Guid notificationGroupPK = NotificationDataRegistry.Instance.PODImportNotificationGroup.Value;
				Env.OutgoingMailManager.CreateAndSave(email, notificationGroupPK, GroupSourceLocator.GetFromRegistryItem(NotificationDataRegistry.Instance.PODImportNotificationGroup));
			}
		}

		#endregion

		#region GetLogMessage

		internal static ZString GetLogMessage(PODDataLoader dataLoader)
		{
			StringBuilder builder = new StringBuilder();

			foreach (ZString logMsg in dataLoader.Log)
			{
				builder.AppendLine(logMsg);
			}

			return builder.ToString();
		}

		#endregion
	}
}
