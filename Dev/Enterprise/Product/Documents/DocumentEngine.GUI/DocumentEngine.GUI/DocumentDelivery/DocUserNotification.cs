using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public sealed class DocUserNotification : IProgressNotificationUI, IDisposable
	{
		public DocUserNotification(DeliveryInstructions deliveryInstructions, int totalPacks)
		{
			this.TotalDocPacks = totalPacks;
			AddEventHandlers(deliveryInstructions);
		}

		public DocUserNotification(PrintTaskSettings taskSettings)
		{
			this.TaskSettings = taskSettings;
			this.TotalDocPacks = taskSettings.PrintTask.Count;
			foreach (DeliveryInstructions deliveryInstructions in taskSettings.DocPacksDeliveryInstructions)
			{
				AddEventHandlers(deliveryInstructions);
			}
			taskSettings.StartDocProcessing += new EventHandler(Instructions_StartDocProcessing);
			taskSettings.EndDocProcessing += new EventHandler(Instructions_EndDocProcessing);

			taskSettings.StartDocumentGeneration += StartDocumentGeneration;
			taskSettings.EndDocumentGeneration += EndDocumentGeneration;
		}

		readonly List<DeliveryInstructions> instructionsWithEventHandlers = new List<DeliveryInstructions>(7);

		void AddEventHandlers(DeliveryInstructions deliveryInstructions)
		{
			instructionsWithEventHandlers.Add(deliveryInstructions);

			deliveryInstructions.StartDocProcessing += new EventHandler(Instructions_StartDocProcessing);
			deliveryInstructions.DocPackStarted += new DeliveryInstructions.DocPrinted(Instructions_DocPackStarted);
			deliveryInstructions.DocPackPrinted += new DeliveryInstructions.DocPrinted(Instructions_DocPackPrinted);
			deliveryInstructions.DocPrintedForContact += new DeliveryInstructions.PrintedForContact(Instructions_DocPrintedForContact);
			deliveryInstructions.DocPrintedWithinPack += new DeliveryInstructions.DocWithinPack(Instructions_DocPrintedWithinPack);
			deliveryInstructions.EndDocProcessing += new EventHandler(Instructions_EndDocProcessing);
		}

		void RemoveEventHandlers(DeliveryInstructions deliveryInstructions)
		{
			deliveryInstructions.StartDocProcessing -= new EventHandler(Instructions_StartDocProcessing);
			deliveryInstructions.DocPackStarted -= new DeliveryInstructions.DocPrinted(Instructions_DocPackStarted);
			deliveryInstructions.DocPackPrinted -= new DeliveryInstructions.DocPrinted(Instructions_DocPackPrinted);
			deliveryInstructions.DocPrintedForContact -= new DeliveryInstructions.PrintedForContact(Instructions_DocPrintedForContact);
			deliveryInstructions.DocPrintedWithinPack -= new DeliveryInstructions.DocWithinPack(Instructions_DocPrintedWithinPack);
			deliveryInstructions.EndDocProcessing -= new EventHandler(Instructions_EndDocProcessing);
		}

		readonly PrintTaskSettings TaskSettings;

		#region Progress Form

		ProgressFormManager Progress
		{
			get
			{
				if (fProgress == null || fProgress.IsDisposed)
				{
					fProgress = new ProgressFormManager();
					fProgress.IsCancelButtonVisible = false;
					fProgress.IsProgressBarVisible = true;
				}

				return fProgress;
			}
		}

		ProgressFormManager fProgress;

		#endregion

		#region Start Document Processing

		void Instructions_StartDocProcessing(object sender, EventArgs e)
		{
			string status = Res.GetString("965240c4-c263-4d32-9296-e2c87e6443a0", "Started processing {0} documents.", TotalDocPacks);
			Progress.UpdateStatus(status, 0);
			Progress.Start();
		}

		#endregion

		#region Document Pack Started

		readonly int TotalDocPacks;
		int CurrentDocPack;

		void Instructions_DocPackStarted(int docNumber)
		{
			CurrentDocPack = docNumber;
			string status = Res.GetString("b78279f4-ef1d-43aa-8cf3-d4a1f083660b", "Please wait while documents are processed.");
			Progress.UpdateStatus(status, GetPercentComplete(0, 1));
		}

		#endregion

		#region Document within Pack Printed

		int GetPercentComplete(int docsFinished, int totalDocsInPack)
		{
			int result = 0;
			Decimal currentDocumentPack = Convert.ToDecimal(CurrentDocPack);
			if (TotalDocPacks == CurrentDocPack && CurrentDocPack > 0)
			{
				currentDocumentPack = Convert.ToDecimal(CurrentDocPack - 1);
			}
			result = (int)(((currentDocumentPack + Convert.ToDecimal(docsFinished) / Convert.ToDecimal(totalDocsInPack)) / Convert.ToDecimal(TotalDocPacks)) * 100);
			if (result > 100)
			{
				string errorMessage = (NoResString)"Progress.PercentComplete is greater than 100!";
				errorMessage += System.Environment.NewLine + "PercentComplete = " + result.ToString();
				errorMessage += System.Environment.NewLine + "CurrentDocPack = " + CurrentDocPack.ToString();
				errorMessage += System.Environment.NewLine + "DocNumber = " + docsFinished.ToString();
				errorMessage += System.Environment.NewLine + "TotalDocsInPack = " + totalDocsInPack.ToString();
				errorMessage += System.Environment.NewLine + "TotalDocPacks = " + TotalDocPacks.ToString();

				ErrorReporter.ReportOnce(errorMessage);
				result = 100;
			}
			return result;
		}

		void Instructions_DocPrintedWithinPack(int docNum, int totalDocs)
		{
			Progress.UpdateStatus(Progress.Status, GetPercentComplete(docNum, totalDocs));
		}

		#endregion

		#region Document Pack Printed

		void Instructions_DocPackPrinted(int docNumber)
		{
			CurrentDocPack = docNumber;
			string status = Res.GetString("b78279f4-ef1d-43aa-8cf3-d4a1f083660b", "Please wait while documents are processed.");
			Progress.UpdateStatus(status, GetPercentComplete(0, 1));
		}

		#endregion

		#region Document Processing Complete

		void Instructions_EndDocProcessing(object sender, EventArgs e)
		{
			string status = Res.GetString("a8c3dfaf-699c-43d9-8b98-e31bd159bf75", "Finished processing {0} documents.", TotalDocPacks);
			Progress.UpdateStatus(status, 100);
			Progress.Dispose();
		}

		#endregion

		#region Document Generation Start

		void StartDocumentGeneration(object sender, EventArgs e)
		{
			var status = Res.GetString("50F738B4-0FD7-469E-8D49-8F9CCB4FC906", "Started generating documents.");
			Progress.UpdateStatus(status, 0);
			Progress.Start();
		}

		#endregion

		#region Document Generation End

		void EndDocumentGeneration(object sender, EventArgs e)
		{
			string status = Res.GetString("C0E28723-E20E-4F93-9026-CE138FA99336", "Finished generating documents.");
			Progress.UpdateStatus(status, 100);
			Progress.Dispose();
		}

		#endregion

		#region Document Printed for Contact

		void Instructions_DocPrintedForContact(int contactNumber, int totalContacts)
		{
			string status = Res.GetString("b78279f4-ef1d-43aa-8cf3-d4a1f083660b", "Please wait while documents are processed.");
			decimal contactNum = Convert.ToDecimal(contactNumber);
			decimal total = Convert.ToDecimal(totalContacts);
			int percentComplete = GetPercentComplete(((int)(contactNum / total)), TotalDocPacks);
			Progress.UpdateStatus(status, percentComplete);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (fProgress != null && !fProgress.IsDisposed)
			{
				fProgress.Dispose();
			}

			if (TaskSettings != null)
			{
				TaskSettings.StartDocProcessing -= new EventHandler(Instructions_StartDocProcessing);
				TaskSettings.EndDocProcessing -= new EventHandler(Instructions_EndDocProcessing);
				TaskSettings.StartDocumentGeneration -= StartDocumentGeneration;
				TaskSettings.EndDocumentGeneration -= EndDocumentGeneration;
			}

			foreach (DeliveryInstructions deliveryInstructions in instructionsWithEventHandlers)
			{
				RemoveEventHandlers(deliveryInstructions);
			}
		}

		#endregion

		#region DEBUG Test Points
#if DEBUG

		internal int GetPercentCompleteForTesting(int docsFinished, int totalDocsInPack)
		{
			return GetPercentComplete(docsFinished, totalDocsInPack);
		}

#endif
		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.DocumentEngine.GUI.Testing
{
	using Enterprise.DocumentEngine.Testing;

	class DocUserNotificationForTesting : IProgressNotificationUI
	{
		internal DocUserNotificationForTesting(DeliveryInstructions instructions)
		{
			deliveryInstructions = instructions as MockDeliveryInstructions;
			AddEventHandlers(instructions);
		}
		readonly MockDeliveryInstructions deliveryInstructions;

		internal DocUserNotificationForTesting(PrintTaskSettings taskSettings)
		{
			this.TaskSettings = taskSettings;
			foreach (DeliveryInstructions deliveryInstructions in taskSettings.DocPacksDeliveryInstructions)
			{
				AddEventHandlers(deliveryInstructions);
			}
			taskSettings.StartDocProcessing += Instructions_StartDocProcessingForTesting;
			taskSettings.EndDocProcessing += Instructions_EndDocProcessingForTesting;

			taskSettings.StartDocumentGeneration += Settings_StartDocumentGeneration;
			taskSettings.EndDocumentGeneration += Settings_EndDocumentGeneration;
		}

		readonly List<DeliveryInstructions> instructionsWithEventHandlers = new List<DeliveryInstructions>(7);
		readonly PrintTaskSettings TaskSettings;

		void AddEventHandlers(DeliveryInstructions instructions)
		{
			instructionsWithEventHandlers.Add(instructions);
			instructions.StartDocProcessing += Instructions_StartDocProcessingForTesting;
			instructions.DocPackStarted += Instructions_DocPackStartedForTesting;
			instructions.DocPackPrinted += Instructions_DocPackPrintedForTesting;
			instructions.DocPrintedForContact += Instructions_DocPrintedForContactForTesting;
			instructions.DocPrintedWithinPack += Instructions_DocPrintedWithinPackForTesting;
			instructions.EndDocProcessing += Instructions_EndDocProcessingForTesting;
		}

		void RemoveEventHandlers(DeliveryInstructions instructions)
		{
			instructions.StartDocProcessing -= Instructions_StartDocProcessingForTesting;
			instructions.DocPackStarted -= Instructions_DocPackStartedForTesting;
			instructions.DocPackPrinted -= Instructions_DocPackPrintedForTesting;
			instructions.DocPrintedForContact -= Instructions_DocPrintedForContactForTesting;
			instructions.DocPrintedWithinPack -= Instructions_DocPrintedWithinPackForTesting;
			instructions.EndDocProcessing -= Instructions_EndDocProcessingForTesting;
		}

		void Settings_StartDocumentGeneration(object sender, EventArgs e)
		{
			TaskSettings.NotificationLogs += "Settings_StartDocumentGeneration\n";
		}

		void Settings_EndDocumentGeneration(object sender, EventArgs e)
		{
			TaskSettings.NotificationLogs += "Settings_EndDocumentGeneration\n";
		}

		void Instructions_StartDocProcessingForTesting(object sender, EventArgs e)
		{
			UpdateLastHandlerAndNotificationCount("StartDocProcessing");
		}

		void Instructions_DocPackStartedForTesting(int docNumber)
		{
			UpdateLastHandlerAndNotificationCount("DocPackStarted");
		}

		void Instructions_DocPackPrintedForTesting(int docNumber)
		{
			UpdateLastHandlerAndNotificationCount("DocPackPrinted");
		}

		void Instructions_DocPrintedForContactForTesting(int contactNumber, int totalContacts)
		{
			UpdateLastHandlerAndNotificationCount("DocPrintedForContact");
		}

		void Instructions_DocPrintedWithinPackForTesting(int docNumber, int totalDocsInPack)
		{
			UpdateLastHandlerAndNotificationCount("DocPrintedWithinPack");
		}

		void Instructions_EndDocProcessingForTesting(object sender, EventArgs e)
		{
			UpdateLastHandlerAndNotificationCount("EndDocProcessing");
		}

		void UpdateLastHandlerAndNotificationCount(ZString handler)
		{
			if (deliveryInstructions != null)
			{
				deliveryInstructions.NotificationCount++;
				deliveryInstructions.LastHandler = handler;
			}
		}

		void IDisposable.Dispose()
		{
			if (TaskSettings != null)
			{
				TaskSettings.StartDocProcessing -= new EventHandler(Instructions_StartDocProcessingForTesting);
				TaskSettings.EndDocProcessing -= new EventHandler(Instructions_EndDocProcessingForTesting);
				TaskSettings.StartDocumentGeneration -= Settings_StartDocumentGeneration;
				TaskSettings.EndDocumentGeneration -= Settings_EndDocumentGeneration;
			}

			foreach (DeliveryInstructions deliveryInstructions in instructionsWithEventHandlers)
			{
				RemoveEventHandlers(deliveryInstructions);
			}
		}
	}
}

#endif
#endregion
