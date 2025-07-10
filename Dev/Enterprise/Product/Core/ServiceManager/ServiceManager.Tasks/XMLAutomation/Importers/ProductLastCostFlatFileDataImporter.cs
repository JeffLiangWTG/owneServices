using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class ProductLastCostFlatFileDataImporter : FlatFileImporter
	{
		protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
		{
			bool useLegacyCodes = SystemDataRegistry.Instance.UseLegacyCodesDuringAutomaticImport.Value;

			OrgSupplierPartLastCostDataLoad dataLoader = new OrgSupplierPartLastCostDataLoad();
			NotificationBuffer buffer = notifications as NotificationBuffer;
			string padding = "=================";

			SetInfoNotification(buffer, Res.GetString("51bfc893-c5ec-4b5e-97d8-586b4f274666", "{0} Product File Last Cost Import {0}", padding));
			dataLoader.ImportPartLastCostData(dataFile.FullName, useLegacyCodes);

			SendEmailToNotificationGroup(dataFile.FullName, dataLoader);
			OutputFinalTotals(dataLoader, buffer);
			SetInfoNotification(buffer, Res.GetString("bc8721e8-8c86-44bc-8ee3-694ffc9ec12d", "Import Result is sent to the Nominated Notification Group"));
			SetInfoNotification(buffer, "=================================================================");
		}

		#region implementation

		void SetInfoNotification(NotificationBuffer buffer, ZString message)
		{
			if (buffer != null)
			{
				InfoNotification info = new InfoNotification(message);
				buffer.Notify(info);
			}
		}

		void OutputFinalTotals(OrgSupplierPartLastCostDataLoad dataLoader, NotificationBuffer buffer)
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

		void SendEmailToNotificationGroup(ZString fileName, OrgSupplierPartLastCostDataLoad dataLoader)
		{
			bool isImportSucceeded = false;
			Guid notificationGroupPK = NotificationDataRegistry.Instance.ProductImportNotificationGroup.Value;

			EmailDef email = new EmailDef();

			AttachmentDef attachment = new AttachmentDef(fileName);
			email.Attachments.Add(attachment);

			if (!dataLoader.FileHeaderIsValid)
			{
				dataLoader.WriteCSVTemplate(TemplateFileFullPath);
				email.Body = InvalidHeaderMessage;
				AttachmentDef templateAttachment = new AttachmentDef(TemplateFileFullPath);
				email.Attachments.Add(templateAttachment);
			}
			else
			{
				email.Body = GetLogMessage(dataLoader);
			}

			if (!dataLoader.FileHeaderIsValid || dataLoader.RunCounters.RecsExcluded > 0)
			{
				email.Subject = Res.GetString("87f11f74-ab1a-4de5-bdd3-75712973a320", "Product Last Cost CSV Import Failed - {0}", Path.GetFileName(fileName));
			}
			else
			{
				email.Subject = Res.GetString("10846848-67c9-4d94-bd1b-a1bd45b79d6c", "Product Last Cost CSV Import Succeeded - {0}", Path.GetFileName(fileName));
				isImportSucceeded = true;
			}

			if ((!isImportSucceeded) || (isImportSucceeded && IsEmailToBeSentOnSuccess))
			{
				Env.OutgoingMailManager.CreateAndSave(email, notificationGroupPK, GroupSourceLocator.GetFromRegistryItem(NotificationDataRegistry.Instance.ProductImportNotificationGroup));
			}

			File.Delete(TemplateFileFullPath);
		}

		readonly ZString TemplateFileFullPath = Path.Combine(Env.TempPath, "Template File.csv");

		public static string InvalidHeaderMessage
		{
			get { return Res.GetString("36de93db-54d9-445f-832f-e248ee84a2ea", "File Header information is incorrect. The import of product last cost data requires a specific .CSV format file: \r\nPlease check the format in the template file attached."); }
		}

		#endregion

		#region GetLogMessage

		ZString GetLogMessage(OrgSupplierPartLastCostDataLoad dataLoader)
		{
			StringBuilder builder = new StringBuilder();

			foreach (ZString logMesg in dataLoader.Log)
			{
				builder.AppendLine(logMesg);
			}

			return builder.ToString();
		}

		#endregion

		#endregion
	}
}
