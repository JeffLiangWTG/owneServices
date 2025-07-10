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
	public class ProductFlatFileImporter : FlatFileImporter
	{
		protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
		{
			bool updateProduct = SystemDataRegistry.Instance.UpdateProductsDuringAutomaticImport.Value;
			bool useLegacyCodes = SystemDataRegistry.Instance.UseLegacyCodesDuringAutomaticImport.Value;

			OrgSupplierPartDataLoad dataLoader = OrgSupplierPartDataLoad.New();

			NotificationBuffer buffer = notifications as NotificationBuffer;
			string padding = "======================";

			SetInfoNotification(buffer, Res.GetString("870a3135-c778-4500-acda-6829188f04a8", "{0} Product File Import {0}", padding));
			dataLoader.ImportProductData(dataFile.FullName, updateProduct, useLegacyCodes);

			SendEmailToNotificationGroup(dataFile.FullName, dataLoader);
			OutputFinalTotals(dataLoader, buffer);
			SetInfoNotification(buffer, Res.GetString("df8614f1-dbfe-41cf-a3ba-82872d1843a0", "Import Result is sent to the Nominated Notification Group"));
			SetInfoNotification(buffer, "=================================================================");
		}

		void SetInfoNotification(NotificationBuffer buffer, ZString message)
		{
			if (buffer != null)
			{
				InfoNotification info = new InfoNotification(message);
				buffer.Notify(info);
			}
		}

		void OutputFinalTotals(OrgSupplierPartDataLoad dataLoader, NotificationBuffer buffer)
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

		void SendEmailToNotificationGroup(ZString fileName, OrgSupplierPartDataLoad dataLoader)
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
				email.Subject = Res.GetString("33b40842-00e5-4be2-bd4a-2423e6aaffd5", "Product CSV Import Failed - {0}", Path.GetFileName(fileName));
			}
			else
			{
				email.Subject = Res.GetString("e79be432-f6b9-4a44-977d-e2f370d5ca6e", "Product CSV Import Succeeded - {0}", Path.GetFileName(fileName));
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
			get { return Res.GetString("03db7ea7-69c5-47fe-9a8f-042db0bed43e", "File Header information is incorrect. The import of product data requires a specific .CSV format file: \r\nPlease check the format in the template file attached."); }
		}

		#endregion

		#region GetLogMessage

		ZString GetLogMessage(OrgSupplierPartDataLoad dataLoader)
		{
			StringBuilder builder = new StringBuilder();

			foreach (ZString logMesg in dataLoader.Log)
			{
				builder.AppendLine(logMesg);
			}

			return builder.ToString();
		}

		#endregion
	}
}
