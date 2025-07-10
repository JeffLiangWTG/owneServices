using System;
using System.Collections;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	public class Retriever : BaseInterchangeRetriever
	{
		#region Implementation

		ZDateTime lastRetrieveFailureTime = ZDateTime.Empty;
#if DEBUG
		public void ResetLastRetrieveFailureTimeForTesting()
		{
			lastRetrieveFailureTime = ZDateTime.Empty;
		}
#endif

		string[] filenames;

		protected override void RetrieveInterchanges(int numberToRetreive, CancellationToken token)
		{
			bool exception = false;
			retrievedInterchanges = new ArrayList();
			string inputDirectory = CACustomsDataRegistry.Instance.MessageInputDirectory.Value;
			if (!string.IsNullOrEmpty(inputDirectory))
			{
				try
				{
					filenames = Directory.GetFiles(inputDirectory, "test*.edi");
				}
				catch (ArgumentException e)
				{
					ReportException(e, "The Test Message Input Directory registry entry contains invalid characters.");
					exception = true;
				}
				catch (DirectoryNotFoundException e)
				{
					ReportException(e, "The specified path for the Test Message Input Directory registry entry is invalid, such as being on an unmapped drive.");
					exception = true;
				}
				catch (UnauthorizedAccessException e)
				{
					ReportException(e, "The required permissions to retireve from the Test Message Input Directory path have not been configured." + System.Environment.NewLine +
						"The network administrator will be required to resolve either network/user permissions.");
					exception = true;
				}
				catch (PathTooLongException e)
				{
					ReportException(e, "The Test Message Input Directory registry entry path, file name, or both exceed the system-defined maximum length." + System.Environment.NewLine +
							"For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.");
					exception = true;
				}
				catch (IOException e)
				{
					ReportException(e, e.Message.Replace("\r\n", ""));
					exception = true;
				}
				if (!exception)
				{
					ProcessInterchanges(numberToRetreive);
				}
			}
		}

		protected void ReportException(Exception e, string reasonsText)
		{
			if (lastRetrieveFailureTime.IsEmpty || lastRetrieveFailureTime.AddMinutes(5) < ZDateTime.Now)
			{
				Logger.Log("The CAC batch processor has failed to retrieve files due to the following error : " + e.Message);
				ZString subject = "CAC batch processor is unable to retrieve files from directory.";
				ZString body = "The CAC batch processor has failed to retrieve files due to the following error : " + e.Message + System.Environment.NewLine +
					"The path that the CAC batch processor is trying to retrieve from is " + CACustomsDataRegistry.Instance.MessageInputDirectory.Value + "." + System.Environment.NewLine +
					"The machine name that the CAC batch processor is running on is : " + System.Environment.MachineName + System.Environment.NewLine + System.Environment.NewLine +
					"The problem may be caused by one of the following : " + System.Environment.NewLine +
					reasonsText + System.Environment.NewLine + System.Environment.NewLine +
					"Please fix the problem.  Once the problem is resolved the batch processor will resume processing correctly";

				Env.OutgoingCustomsMailManager.CreateAndSaveToCompanyNotificationGroup(subject, body);
				lastRetrieveFailureTime = ZDateTime.Now;
			}
		}

		protected void ProcessInterchanges(int numberToRetreive)
		{
			if (filenames != null)
			{
				try
				{
					int numberRetreived = 0;
					ZString processingPath = Path.Combine(CACustomsDataRegistry.Instance.MessageInputDirectory.Value, "PROCESSING");
					ZString processingFile;
					if (!Directory.Exists(processingPath))
					{
						Directory.CreateDirectory(processingPath);
					}
					foreach (string filename in filenames)
					{
						ZString contents = ZString.Empty;
						contents = File.ReadAllText(filename).Replace("\r\n", "");
						processingFile = Path.Combine(processingPath, Path.GetFileName(filename));
						if (File.Exists(processingFile))
						{
							File.Delete(processingFile);
						}

						File.Move(filename, processingFile);
						if (contents != ZString.Empty)
						{
							RetrievedInterchangeFromDisk newInterchange = new RetrievedInterchangeFromDisk();
							newInterchange.Contents = contents;
							newInterchange.Filename = processingFile;
							retrievedInterchanges.Add(newInterchange);
							numberRetreived++;
						}
						if (numberRetreived == numberToRetreive)
						{
							break;
						}
					}
				}
				catch (IOException e)
				{
					ReportException(e, e.Message.Replace("\r\n", ""));
				}
			}
		}

		protected override EDIInterchange CreateInterchange(BusinessObjectFactory factory, BaseRetrievedInterchange retrievedInterchange)
		{
			string interchangeText = retrievedInterchange.Contents;
			return EDIInterchange.CreateNewInterchangeFromString(factory, interchangeText, BatchProcessorUtilities.GetApplicationCode(interchangeText), true, false);
		}

		protected override void ClearProcessedInterchanges()
		{
			foreach (BaseRetrievedInterchange processedInterchange in processedInterchanges)
			{
				if (processedInterchange is RetrievedInterchangeFromDisk)
				{
					if (!((RetrievedInterchangeFromDisk)processedInterchange).Filename.IsEmpty)
					{
						try
						{
							File.Delete(((RetrievedInterchangeFromDisk)processedInterchange).Filename);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							Logger.Log("Unable to delete processed interchange: " + e.Message);
						}
					}
				}
			}
		}

		#endregion
	}
}
