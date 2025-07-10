using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.Business.GSSI;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.UPE,
	ServiceTaskCode = GSSiServiceTask.Code,
	QueueName = "GSSi messages outbound",
	Table = EDIMessageSchema.Constants.TableName,
	Predicates = new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=GSS",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y"
	}
)]

[assembly: HostedService(GSSiServiceTask.Code, "GSSi Batch Message Processor", "CSP",
	typeof(GSSiServiceTask),
	MinimumPeriod = "1minute",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "15minutes"
	)]

namespace Enterprise.Client.UPE.ServiceTask
{
	public class GSSiServiceTask : UPEServiceTask
	{
		public const string Code = "ZU4";

		public GSSiServiceTask()
		{
		}

		public GSSiServiceTask(ILogger logger)
			: base(logger)
		{
		}

		public override void RunTask(CancellationToken token)
		{
			var branch = UPETools.Instance.UPECustomisationBranches(false).FirstOrDefault();
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					try
					{
						WaitFor<Object>.Run(TimeSpan.FromSeconds(UPEDataRegistry.Instance.SftpServerTimeout), () =>
						{
							var query = new ZQuery();
							query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "GSS");
							query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "TRX");
							query.AddToFilter(EDIMessageSchema.EM_Status, "QUE");
							query.MaximumRows = 1000;
							query.OrderBy = EDIMessageSchema.EM_MessageNum.Name;
							query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

							GSSMessage[] messages;
							do
							{
								token.ThrowIfCancellationRequested();

								var factory = new BusinessObjectFactory();
								messages = factory.Load<GSSMessage>(query);
								if (!messages.Any())
								{
									break;
								}

								using (var tempFile = CreateZipFile(messages))
								{
									Notify("Sending batch of " + messages.Length + " messages in one message set.");

									if (UploadMessage(tempFile.Filename, messages.First().EM_MessageNum, messages.Last().EM_MessageNum))
									{
										foreach (var msg in messages)
										{
											msg.EM_Status = "SNT";
											Notify("Marking message " + msg.EM_MessageNum + " as sent.");
										}
										Notify("Saving batch.");
										factory.Save();
										Notify("Batch saved successfully.");
									}
								}
							}
							while (messages.Length == query.MaximumRows);
							return null;
						});
					}
					catch (TimeoutException)
					{
						UPETools.Instance.SendTimeoutEmail(Code);
					}
				}
			}
		}

		static TempFile CreateZipFile(GSSMessage[] messages)
		{
			var tempFile = TempFile.New();

			FileStream fs = null;
			try
			{
				fs = File.Create(tempFile.Filename);
				{
					using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Create))
					{
						fs = null;
						foreach (var message in messages)
						{
							var zipEntry = zipArchive.CreateEntry(message.EM_MessageNum + ".txt", CompressionLevel.NoCompression);
							using (var writer = new StreamWriter(zipEntry.Open()))
							{
								writer.Write(message.EM_MessageText);
							}
						}
					}
				}
			}
			finally
			{
				fs?.Dispose();
			}
			return tempFile;
		}

		protected virtual bool UploadMessage(ZString fileName, ZString firstMessageNumber, ZString lastMessageNumber)
		{
			var destinationName = "SET" + firstMessageNumber + "_" + lastMessageNumber + ".ZIP";
			Notify("Starting upload of " + destinationName);
			var result = new GSSiUploader(Notifications, destinationName).UploadToFtpServerAndArchive(fileName);
			if (result)
			{
				Notify("Completed upload of " + destinationName + " successfully");
			}
			else
			{
				Notify("Failed upload of " + destinationName);
			}
			return result;
		}
	}
}
