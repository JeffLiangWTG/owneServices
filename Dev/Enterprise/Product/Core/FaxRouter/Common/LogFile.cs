using System;
using System.Globalization;
using System.IO;
#if NETFRAMEWORK
#pragma warning disable CW1086 // Do not use System.Web.Mail
using System.Web.Mail;
#pragma warning restore CW1086
#else
using System.Net.Mail;
#endif
using Enterprise.ZArchitecture.Core;

namespace Enterprise.FaxRouter
{
	public static class LogFile
	{
		public static void AddLog(string @event, string eventText)
		{
			AddLog(@event, eventText, "");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static void AddLog(string @event, string eventText, string filenameOverride)
		{
			CreateTempDirIfNotExist();
			string logRecordDate = DateTime.Now.ToString("s");
			for (int i = 0; i < 4; i++)
			{
				try
				{
					string file = string.IsNullOrEmpty(filenameOverride) ? LogFilename : Path.Combine(Constants.FAX_GATEWAY_TEMP_FILE_DIRECTORY, filenameOverride);
					using (StreamWriter log = File.AppendText(file))
					{
						log.Write(logRecordDate);
						log.Write("\t");
						log.Write(@event);
						log.Write("\t");
						log.Write(eventText);
						log.WriteLine();
						break;
					}
				}
				catch (IOException ex)
				{
					if (ex.Message.Substring(0, 34) != "The process cannot access the file" || ex.Message.Substring(ex.Message.Length - 44, 44) != "because it is being used by another process.")
					{
						throw;
					}
				}
				System.Threading.Thread.Sleep(5000);
			}
		}

		static void CreateTempDirIfNotExist()
		{
			if (!Directory.Exists(Constants.FAX_GATEWAY_TEMP_FILE_DIRECTORY))
			{
				Directory.CreateDirectory(Constants.FAX_GATEWAY_TEMP_FILE_DIRECTORY);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1086:DoNotUseSystemWebMail", Justification = "Baseline")]
		public static void EmailAdmin(string subject, string message)
		{
#if NETFRAMEWORK
			SmtpMail.SmtpServer = Constants.EDI_ACK_SMTP_SERVER;
			SmtpMail.Send(Constants.FAX_GATEWAY_EMAIL, Constants.FAX_ADMINISTRATOR_EMAIL, subject, message);
#else
			using (var client = new SmtpClient(Constants.EDI_ACK_SMTP_SERVER))
			{
				var mailMessage = new MailMessage
				{
					From = new MailAddress(Constants.FAX_GATEWAY_EMAIL),
					Subject = subject,
					Body = message
				};

				mailMessage.To.Add(Constants.FAX_ADMINISTRATOR_EMAIL);
				client.Send(mailMessage);
			}
#endif
		}

		static string LogFilename
		{
			get
			{
				DateTime now = DateTime.Now;
				if (now.Date != fCurrentDate.Date)
				{
					fCurrentDate = now.Date;
					fLogFilename = GetNewLogFilename();
				}
				return fLogFilename;
			}
		}

		static string GetNewLogFilename()
		{
			string result;
			string lastWeekFile = null;
			string nameWithoutExtention = Path.GetFileNameWithoutExtension(Constants.FAX_GATEWAY_LOG);
			string[] files = Directory.GetFiles(Constants.FAX_GATEWAY_TEMP_FILE_DIRECTORY, nameWithoutExtention + '*');
			foreach (string file in files)
			{
				DateTime date;
				string dateString = Path.GetFileNameWithoutExtension(file).Substring(nameWithoutExtention.Length);
				if (DateTime.TryParseExact(dateString, LogFileDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
				{
					TimeSpan diff = (fCurrentDate - date);
					if (diff.Days < 70)
					{
						if (diff.Days < 7)
						{
							lastWeekFile = file;
						}
					}
					else
					{
						TempFile.Delete(file);
					}
				}
			}
			if (lastWeekFile != null)
			{
				result = lastWeekFile;
			}
			else
			{
				string newFileName = nameWithoutExtention + fCurrentDate.ToString(LogFileDateFormat) + ".txt";
				result = Path.Combine(Constants.FAX_GATEWAY_TEMP_FILE_DIRECTORY, newFileName);
			}
			return result;
		}

		static string fLogFilename;
		static DateTime fCurrentDate = new DateTime(1900, 1, 1);
		const string LogFileDateFormat = "yyyy_MM_dd";
	}
}
