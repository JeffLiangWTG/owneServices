using System;
using System.IO;
using System.Threading;

using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT
{
	public class TNTReturnExitStatus
	{
		#region Constructor(s)

		public TNTReturnExitStatus()
			: this(new NotificationBuffer()) { }

		public TNTReturnExitStatus(bool populateFileNameInMilliSeconds)
			: this(new NotificationBuffer(), populateFileNameInMilliSeconds) { }

		public TNTReturnExitStatus(INotifications notificationSubscriber)
			: this(notificationSubscriber, false)
		{
		}

		public TNTReturnExitStatus(INotifications notificationSubscriber, bool populateFileNameInMilliSeconds)
		{
			if (notificationSubscriber != null)
			{
				this.notificationBuffer = new NotificationBuffer(notificationSubscriber);
			}
			PopulateFileNameInMilliSeconds = populateFileNameInMilliSeconds;
		}

		#endregion

		internal
		bool PopulateFileNameInMilliSeconds;

		NotificationBuffer notificationBuffer;

		public NotificationBuffer NotificationBuffer
		{
			get { return notificationBuffer ?? (notificationBuffer = new NotificationBuffer()); }
		}

		public ZString SendEDNReply(ZString branch, ZString hbill, ZString org, ZString dest, ZString eDN, ZString status, ZString cleared)
		{
			string returnData = FixedPadRight(hbill, 15)
				+ FixedPadRight(org, 5)
				+ FixedPadRight(dest, 5)
				+ FixedPadRight(eDN, 9)
				+ FixedPadRight(status, 5)
				+ cleared;
			string message = string.Empty;
			try
			{
				CreateResponseFile(hbill, branch, returnData);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				message = exception.Message;
				NotificationBuffer.Notify(new ErrorNotification(ErrorType.IOError, "Error while sending the EDN Reply:" + System.Environment.NewLine + exception.Message));
			}
			return message;
		}

		string FixedPadRight(ZString value, int length)
		{
			return value.Left(length).PadRight(length);
		}

		void CreateResponseFile(ZString hBill, ZString branchCode, string replyData)
		{
			int retry = 3;

			while (true)
			{
				try
				{
					using (var file = new FileStream(GetPathToFile(hBill, branchCode), FileMode.Append, FileAccess.Write, FileShare.None))
					using (StreamWriter writer = new StreamWriter(file))
					{
						NotificationBuffer.Notify(new InfoNotification(ZString.Format("Attempt to write record with bill no {0} in file {1} ......", hBill, file.Name)));
						writer.WriteLine(replyData);
						NotificationBuffer.Notify(new InfoNotification(ZString.Format("Record with bill no {0} is written into file {1}", hBill, file.Name)));
						break;
					}
				}
				catch (Exception ex)
				{
					int errorCode = ex.HResult & ((1 << 16) - 1);

					if (!(errorCode == 5 || errorCode == 32 || errorCode == 33) || --retry == 0)
					{
						throw;
					}

					NotificationBuffer.Notify(new InfoNotification(String.Join(System.Environment.NewLine, "Error writing to file: ", ex.Message, System.Environment.NewLine, "Retrying......")));
					Thread.Sleep(100);
				}
			}
		}

		protected virtual ZString GetPathToFile(ZString hBill, ZString branchCode)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
			ZString currentTimeString = (PopulateFileNameInMilliSeconds) ? currentDateTime.ToString("HHmmssfff") : currentDateTime.ToString("HHmmss");

			string fileName = "TIES" + currentDateString + currentTimeString + "E." + (branchCode.IsEmpty ? "SYD" : branchCode.Left(3).ToString()) + QuantumFile.Extension;
			string dirPath = GetReplyDirectory();
			return Path.Combine(dirPath, fileName);
		}

		string GetReplyDirectory()
		{
			StringRegistryItem replyDirectory = GetReplyDirectoryCore();
			if (string.IsNullOrEmpty(replyDirectory.Value))
			{
				throw new NotImplementedException(string.Format("{0} has not been set.", replyDirectory.Caption));
			}

			DirectoryInfo replyDirectoryInfo = new DirectoryInfo(replyDirectory.Value);

			if (!replyDirectoryInfo.Exists)
			{
				replyDirectoryInfo.Create();
			}

			return replyDirectoryInfo.FullName;
		}

		protected virtual StringRegistryItem GetReplyDirectoryCore()
		{
			return TNTDataRegistry.Instance.TNTReplyDirectoryRaw;
		}
	}

	#region SetUp & TearDown
	#endregion

}
