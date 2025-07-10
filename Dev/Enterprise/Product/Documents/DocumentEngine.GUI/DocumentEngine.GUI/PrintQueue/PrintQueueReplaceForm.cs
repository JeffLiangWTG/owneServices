using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class PrintQueueReplaceForm : ZChildForm
	{
		protected new PrintQueueReplaceBizo BusinessEntity => (PrintQueueReplaceBizo)base.BusinessEntity;
		readonly BusinessObjectFactory Factory;

		readonly ZQuery isWarehouseOrArea = new ZQuery(StmDefaultPrinterSchema.SDP_SubjectTableCode, new[] { WhsWarehouseSchema.Constants.Prefix, WhsAreaSchema.Constants.Prefix });

		internal readonly Dictionary<string, (Type, SchemaColumn, ZQuery)> printQueueDependants;

		internal bool HasDependants
		{
			get
			{
				foreach (var pair in printQueueDependants)
				{
					var query = new ZQuery(pair.Value.Item2, BusinessEntity.PrintQueuePK).AddToFilter(pair.Value.Item3);
					if (Factory.Exists(pair.Value.Item1, query, false))
					{
						return true;
					}
				}
				return false;
			}
		}

		public PrintQueueReplaceForm(BusinessObjectFactory factory, PrintQueueReplaceBizo printQueue, bool deleteOldPrinter)
			: base(printQueue)
		{
			InitializeComponent();

			Factory = factory;
			this.deleteOldPrinter = deleteOldPrinter;
			printQueueDependants = new ()
			{
				{ StmDefaultPrinterSchema.Constants.TableName, (typeof(StmDefaultPrinter), StmDefaultPrinterSchema.SDP_SQ_Printer, isWarehouseOrArea) },
				{ AccChequeBookSchema.Constants.TableName, (typeof(AccChequeBook), AccChequeBookSchema.AK_SQ, null) },
				{ AccComplianceSequenceSchema.Constants.TableName, (typeof(AccComplianceSequence), AccComplianceSequenceSchema.XD_SQ_DocumentPrintQueue, null) },
				{ ProcessTaskNotificationSchema.Constants.TableName , (typeof(ProcessTaskNotification), ProcessTaskNotificationSchema.PQ_SQ, null) },
				{ StmPrintJobSchema.Constants.TableName , (typeof(StmPrintJob), StmPrintJobSchema.SP_SQ, null) },
				{ StmPrintJobQueueSchema.Constants.TableName , (typeof(StmPrintJobQueue), StmPrintJobQueueSchema.SPQ_SQ_PrintQueue, null) },
				{ StmScheduleTaskRecipientSchema.Constants.TableName , (typeof(StmScheduleTaskRecipient), StmScheduleTaskRecipientSchema.S6_SQ, null) },
				{ WhsLocationSchema.Constants.TableName , (ObjectFactory.GetType<IWhsLocation>(), WhsLocationViewSchema.WLV_SQ_DefaultPrintQueue, null) },
				{ WhsRFRegistrySchema.Constants.TableName , (ObjectFactory.GetType<IWhsRFRegistry>(), WhsRFRegistrySchema.WRR_SQ_Printer, null) }
			};
			label1.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("BBC9E3A7-1986-42C6-90F3-F186DC85FA53", "Replace all usages of '{0}' with another Print Queue", BusinessEntity.DisplayName);
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.ReplacePrintQueuePK.IsValid)
			{
				Globals.Message.Show(Res.GetString("B4118FB4-4905-4E42-8D3B-530913B3379A", "Please select a printer."));
				return;
			}

			if (!BusinessEntity.HasErrors)
			{
				var result = Globals.Message.ShowConfirmation(Res.GetString("B1878153-17ED-4A4C-BA9E-D5873DC4CCC4", "This action will replace all usages of '{0}' with '{1}'.\r\nDo you want to continue?", BusinessEntity.DisplayName, OtherPrintQueueFindBox.CodeBox.Text), Res.GetString("CB1563A3-B9B8-4E40-8B85-2C889F8C74E4", "WARNING REPLACING PRINT QUEUE"), Res.GetString("B79FDDA0-E62C-45A6-B453-758C764D7CE0", "REPLACE PRINT QUEUE"), System.Windows.Forms.MessageBoxIcon.Warning);
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					Factory.SaveInTransactionActions.Add(new PrintQueueReplacer(BusinessEntity));
					if (!deleteOldPrinter)
					{
						Factory.Save();
					}
					Globals.Message.Show(Res.GetString("9E5C1E7B-945D-4B31-8938-DE8871F66609", "{0} has been successfully replaced.", BusinessEntity.DisplayName));
					this.DialogResult = DialogResult.OK;
				}
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		readonly bool deleteOldPrinter;
	}
}
