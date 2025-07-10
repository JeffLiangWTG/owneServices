using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE
{
	internal interface IExceptionBuffer
	{
		void AddToExceptionReport(string message, ContainerDatesFlatFileDataRow row);
	}

	internal class ContainerDatesExceptionBuffer : IExceptionBuffer
	{
		public void AddToExceptionReport(string message, ContainerDatesFlatFileDataRow row)
		{
			LinesBuffer.Add(row.ContainerNumber + "," +
				row.CLEReference + "," +
				row.DeliveryDateString + "," +
				row.DeHireDateString + "," +
				message
			);
		}

		public void CreateExceptionReport(INotifications notify)
		{
			CreateExceptionReportCore(notify);
		}

		protected virtual
		void CreateExceptionReportCore(INotifications notify)
		{
			if (LinesBuffer.Count > 0)
			{
				string pathToFile = Path.Combine(Env.TempPath, "ContainerUploadAudit.csv");
				try
				{
					using (StreamWriter writer = new StreamWriter(pathToFile))
					{
						writer.WriteLine("Container,Job,Delivered,De-Hire");
						foreach (string current in LinesBuffer)
						{
							writer.WriteLine(current);
						}
					}

					LinesBuffer.Clear();

					if (File.Exists(pathToFile) && new FileInfo(pathToFile).Length > 0)
					{
						EmailDef email = new EmailDef();
						AttachmentDef attachment = new AttachmentDef(pathToFile);
						email.Attachments.Add(attachment);
						email.Subject = EmailSubject;
						Env.OutgoingMailManager.CreateAndSave(email, CLEDataRegistry.Instance.ContainerUploadEmailNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(CLEDataRegistry.Instance.ContainerUploadEmailNotificationGroup));
						notify.Notify(new InfoNotification(ExceptionCreatedMessage));
					}
				}
				finally
				{
					if (File.Exists(pathToFile))
					{
						File.Delete(pathToFile);
					}
				}
			}
			else
			{
				notify.Notify(new InfoNotification(NoExceptionFound));
			}
		}

		readonly List<string> LinesBuffer = new List<string>();

		internal const string EmailSubject = "Container De-Hire Upload Audit";
		internal const string ExceptionCreatedMessage = "An Exception report file has been sent to the email notification group.";
		internal const string NoExceptionFound = "No Exception Report has been generated.";
	}
}
