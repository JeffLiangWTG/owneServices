using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class PrintQueueForm : ZForm
	{
		public PrintQueueForm(StmPrintQueue businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected override void OnPostButtonClick(object sender, EventArgs e)
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				var bizo = new PrintQueueReplaceBizo(PrintQueue);

				using (var printReplace = new PrintQueueReplaceForm(PrintQueue.Factory, bizo, true))
				{
					if (printReplace.HasDependants)
					{
						needReplace = true;
						ZFormModaliser.ShowDialogWithoutDispose(printReplace);
					}

					if (printReplace.DialogResult == DialogResult.Cancel)
					{
						this.Close();
					}
				}
			}

			base.OnPostButtonClick(sender, e);
		}

		protected void NumericOnly_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (!Char.IsControl(e.KeyChar) && !Char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		bool needReplace;
		protected override void Save(ITransactionParticipant[] factories)
		{
			var allFactories = new List<ITransactionParticipant>();
			if (needReplace)
			{
				var replacer = BusinessEntity.Factory.SaveInTransactionActions.FirstOrDefault(x => x is PrintQueueReplacer);
				if (replacer != null)
				{
					allFactories.Add(replacer);
					BusinessEntity.Factory.SaveInTransactionActions.Remove(replacer);
				}
			}

			allFactories.AddRange(factories);
			base.Save(allFactories.ToArray());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		StmPrintQueue PrintQueue
		{
			get { return BusinessEntity as StmPrintQueue; }
		}

		protected override string GetMessageForCannotDeleteRecordInUseException(Exception ex)
		{
			ZStringBuilder builder = new ZStringBuilder();

			CheckForAccChequeBooksRecords(builder);
			CheckForAccComplianceSequenceRecords(builder);
			CheckForProcessTaskNotificationRecords(builder);
			CheckForStmDefaultPrinterRecords(builder);
			CheckForStmPrintJobRecords(builder);
			// Do not need to check for StmPrintJobQueue records - they are linked to StmPrintJob and are deleted by trigger.
			CheckForStmScheduleTaskRecipientRecords(builder);
			CheckForWhsLocationRecords(builder);
			CheckForWhsRFRegistryRecords(builder);

			if (builder.IsEmpty)
			{
				builder.AppendLine(ex.Message);
			}

			return builder.ToString();
		}

		void CheckForAccChequeBooksRecords(ZStringBuilder builder)
		{
			var cheques = PrintQueueUtils.GetReferencedRecords<AccChequeBook>(PrintQueue.Factory, PrintQueue[StmPrintQueue.Schema.PK], AccChequeBook.Schema.TableName, AccChequeBook.Schema.AK_SQ);

			var message = string.Join(System.Environment.NewLine, cheques.Select(c => c.AK_Desc));

			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("273FBAD0-F2FC-445B-8B5A-29A58277624D", @"This record is in use by one or more record(s) of the module Cheque Books with the following descriptions, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}

		void CheckForAccComplianceSequenceRecords(ZStringBuilder builder)
		{
			var compliances = PrintQueueUtils.GetReferencedRecords<AccComplianceSequence>(PrintQueue.Factory, PrintQueue[StmPrintQueue.Schema.PK], AccComplianceSequence.Schema.TableName, AccComplianceSequence.Schema.XD_SQ_DocumentPrintQueue);

			var message = string.Join(System.Environment.NewLine, compliances.Select(c => c.XD_Description));

			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("5FEC5377-3421-449F-96BB-0BC82ADE7899", @"This record is in use by one or more record(s) of the module Compliance Sequence with the following descriptions, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}

		void CheckForProcessTaskNotificationRecords(ZStringBuilder builder)
		{
			var messageList = new List<string>();
			var taskNotifications = PrintQueueUtils.GetReferencedRecords<ProcessTaskNotification>(PrintQueue.Factory, PrintQueue[StmPrintQueue.Schema.PK], ProcessTaskNotification.Schema.TableName, ProcessTaskNotification.Schema.PQ_SQ);

			foreach (var taskNotification in taskNotifications)
			{
				var trigger = taskNotification.Parent;
				var parentBizo = trigger?.GetJob();

				if (parentBizo != null)
				{
					messageList.Add(parentBizo.HumanReadableName);
				}
				else
				{
					messageList.Add(trigger.HumanReadableName);
				}
			}

			var message = string.Join(System.Environment.NewLine, messageList);
			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("4BCC33A6-D974-4AF7-A981-57FC208601AC", @"This record is in use by the task(s) in the record(s) with the following ID, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}

		void CheckForStmDefaultPrinterRecords(ZStringBuilder builder)
		{
			ObjectFactory.New<IWhsDefaultPrinterHelper>().CheckForStmDefaultPrinterRecords(builder, PrintQueue.PK, PrintQueue.Factory);
		}

		void CheckForStmPrintJobRecords(ZStringBuilder builder)
		{
			var jobs = PrintQueueUtils.GetReferencedRecords<StmPrintJob>(PrintQueue.Factory, PrintQueue[StmPrintQueue.Schema.PK], StmPrintJob.Schema.TableName, StmPrintJob.Schema.SP_SQ);
			var message = string.Join(System.Environment.NewLine, jobs.Select(j => j.SP_DocumentName));

			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("1349B5F3-EBE7-4075-9871-41B511D60E28", @"This record is in use by one or more record(s) of the module Print Job with the following document names, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}

		void CheckForStmScheduleTaskRecipientRecords(ZStringBuilder builder)
		{
			string message = string.Empty;
			var schedules = PrintQueueUtils.GetReferencedRecords<StmScheduleTaskRecipient>(PrintQueue.Factory, PrintQueue[StmPrintQueue.Schema.PK], StmScheduleTaskRecipient.Schema.TableName, StmScheduleTaskRecipient.Schema.S6_SQ);

			message = string.Join(
				System.Environment.NewLine,
				schedules.Select(sch =>
				{
					var scheduleTask = PrintQueueUtils.GetReferencedRecords<StmScheduleTask>(PrintQueue.Factory, sch[StmScheduleTaskRecipient.Schema.S6_S5], StmScheduleTask.Schema.TableName, StmScheduleTask.Schema.PK).FirstOrDefault();
					return scheduleTask?.S5_ScheduleDescription;
				})
			);

			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("DF7B47B4-75F4-4534-9A25-E53D838D669B", @"This record is in use by one or more record(s) of the module Schedule Report with the following descriptions, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}

		void CheckForWhsLocationRecords(ZStringBuilder builder)
		{
			ObjectFactory.New<IWhsLocationDocumentEngineHelper>().CheckForWhsLocationRecords(builder, PrintQueue.PK, PrintQueue.Factory);
		}

		void CheckForWhsRFRegistryRecords(ZStringBuilder builder)
		{
			ObjectFactory.New<IWhsRFRegistryDocumentEngineHelper>().CheckForWhsRFRegistryRecords(builder, PrintQueue.PK, PrintQueue.Factory);
		}

		void LoadXLSwithPrintersettingsButton_Click(object sender, System.EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				object printerDriverSettings;
				try
				{
					using (ExcelInterface xlInterface = new ExcelInterface())
					using (var stream = openFileDialog.OpenFile())
					{
						xlInterface.LoadExcelFile(stream);
						printerDriverSettings = xlInterface.GetPrinterDriverSettings();
					}
					if (printerDriverSettings == null)
					{
						Globals.Message.ShowWarning(Res.GetString("ebff20c7-e536-4e22-bdf1-32b555348c1a", "This XLS file does not have a printer set up."), Res.GetString("bf584d13-8d74-4a31-9de9-57fc7c3d020c", "Not a proper XLS"));
					}
					else
					{
						((StmPrintQueue)BusinessEntity).SQ_XLSTemplateForPrintSettings = new ZBlob(printerDriverSettings);
					}
					((StmPrintQueue)BusinessEntity).RefreshBinding();
				}
				catch (System.IO.IOException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void ClearPrintersettingsButton_Click(object sender, System.EventArgs e)
		{
			((StmPrintQueue)BusinessEntity).SQ_XLSTemplateForPrintSettings = ZBlob.Empty;
			((StmPrintQueue)BusinessEntity).RefreshBinding();
		}

		#region Dispose

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (openFileDialog != null)
				{
					openFileDialog.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
