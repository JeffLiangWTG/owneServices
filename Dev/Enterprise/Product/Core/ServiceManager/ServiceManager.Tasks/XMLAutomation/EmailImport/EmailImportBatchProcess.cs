using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class EmailImportBatchProcessor : EmailReaderBatchProcess
	{
		protected EmailImportBatchProcessor(StringRegistryItem registryPath, NotificationBuffer buffer)
			: base(buffer)
		{
			RegistryPath = registryPath;
		}

		internal bool IsEnvironmentDataValidInternal() => IsEnvironmentDataValid();
		protected override bool IsEnvironmentDataValid()
		{
			bool result = base.IsEnvironmentDataValid();
			ZString warningMessage = ZString.Empty;
			if (DestinationDirectory.IsEmpty)
			{
				warningMessage = Res.GetString("839559f9-f9c4-4bfe-a9a6-4ee61a8f5302", "No {0} registry item set. Associated import not running.", RegistryPath.Name);
			}
			else if (!Directory.Exists(DestinationDirectory))
			{
				warningMessage = Res.GetString("3ad33359-1196-4695-9ef1-c5d9be3de846", "The directory {0} does not exist. Emails' attachments cannot be downloaded into.", DestinationDirectory);
			}
			if (!warningMessage.IsEmpty)
			{
				var warning = new WarningNotification(warningMessage);
				Buffer.Notify(warning);
				result = false;
			}
			return result;
		}

		protected override void ProcessMailItem(MailItem mail)
		{
			foreach (MailAttachment attachment in mail.MailAttachments)
			{
				ProcessAttachment(attachment);
			}
		}

		void ProcessAttachment(MailAttachment attachment)
		{
			if (Path.GetExtension(attachment.MA_FileName).ToLower() == ".xml")
			{
				string receivedFile = Path.Combine(DestinationDirectory, attachment.MA_FileName);
				using (var stream = new FileStream(receivedFile, FileMode.Create))
				{
					stream.Write(attachment.MA_Data, 0, attachment.MA_Data.Length);
				}
			}
		}

		ZString DestinationDirectory
		{
			get { return RegistryPath.Value; }
		}

		readonly StringRegistryItem RegistryPath;
	}
}
