using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobDocumentPrinter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public JobDocumentPrinter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		public ZBool PrintChargeSummary { get; set; }
		public ZBool PrintProfitRecognitionByDateSummary { get; set; }
		public ZBool PrintChargeDetail { get; set; }
		public ZBool PrintARInvoiceAnalysis { get; set; }
		public ZBool PrintAPInvoiceAnalysis { get; set; }
		public ZBool PrintJobRevenueJournalAnalysis { get; set; }

		public ZBool IsProfitLossDoc { get; set; }

		protected virtual string JobProfitDocumentMenuName
		{
			get { return (NoResString)"Job Profit Document"; }
		}

		#endregion

		public virtual void PrintJobProfitDocuments(BusinessObjectFactory factory, params JobDocumentPrintItem[] jobDocumentPrintItems)
		{
			if (jobDocumentPrintItems != null)
			{
				var docCommand = FindJobDocumentCommand(jobDocumentPrintItems.Length > 0 ? jobDocumentPrintItems[0] : null);
				using (var task = GetPrintTask(docCommand.PK))
				{
					var docPack = new DocumentPack(docCommand);
					foreach (JobDocumentPrintItem jobDocumentPrinter in jobDocumentPrintItems)
					{
						docPack.AddReportsToPack(docCommand, null, jobDocumentPrinter, null);
					}

					task.Add(docPack);
					task.RunWithPartialInstructions(AllowedDeliveryOptions.All, GetPrintDeliveryInstructions(docPack), Env.Security.None);
				}
			}
		}

		protected virtual PrintTask GetPrintTask(ZGuid docCommandPk)
		{
			PrintTask task = new PrintTask();
			task.DeliveryInstructionsDefaultPK = docCommandPk;
			return task;
		}

		DocumentPack GetDocumentPack()
		{
			return new DocumentPack(FindJobDocumentCommand());
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		DocumentCommand FindJobDocumentCommand(JobDocumentPrintItem jobDocumentPrintItem = null)
		{
			if (fDocumentCommand == null)
			{
				ZQuery commandFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, JobProfitDocumentMenuName);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsSystemDefined, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
				fDocumentCommand = LoadDocumentCommand(commandFilter, jobDocumentPrintItem);

				if (fDocumentCommand == null)
				{
					throw new ApplicationException("Unable to find " + JobProfitDocumentMenuName + " menu command");
				}
			}

			return fDocumentCommand;
		}

		protected virtual DocumentCommand LoadDocumentCommand(ZQuery commandFilter, JobDocumentPrintItem jobDocumentPrintItem = null)
		{
			return Factory.LoadTop1<DocumentCommand>(commandFilter);
		}

		DocumentCommand fDocumentCommand;

		DeliveryInstructions GetPrintDeliveryInstructions(DocumentPack docPack)
		{
			DeliveryInstructions instructions = new DeliveryInstructions(docPack);

			foreach (DocDeliveryContact contact in instructions.Recipients)
			{
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			}

			return instructions;
		}
	}
}
