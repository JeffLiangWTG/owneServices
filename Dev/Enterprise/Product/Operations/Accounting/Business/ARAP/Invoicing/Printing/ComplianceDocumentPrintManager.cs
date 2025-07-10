using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public class ComplianceDocumentPrintManager
	{
		internal ComplianceDocumentPrintFailureInformation PrintFailureInfomation { get; private set; }

		public IEnumerable<PrintTask> GetComplianceDocumentPrintTasks(AccComplianceDocumentHeader[] selectedComplianceDocuments)
		{
			var result = new List<PrintTask>();
			PrintFailureInfomation = new ComplianceDocumentPrintFailureInformation();

			var printQueues = selectedComplianceDocuments.GroupBy(x => x.ComplianceBook.XD_SQ_DocumentPrintQueue);
			foreach (var printQueue in printQueues)
			{
				var docPacks = new List<DocumentPack>();
				var printOrderQueue = printQueue.OrderBy(x => x.ADH_DocumentNumber);
				foreach (var complianceDocument in printOrderQueue)
				{
					DocumentCommand stmMenuItem;
					try
					{
						stmMenuItem = GetComplianceDocumentCommand(complianceDocument);
					}
					catch (ComplianceSequenceRelatedException ex)
					{
						PrintFailureInfomation.PrintTaskErrorMessage = ex.UserFriendlyMessage;
						PrintFailureInfomation.FailureCount++;
						continue;
					}

					docPacks.Add(new DocumentPack(stmMenuItem, complianceDocument, null, null));
				}

				if (docPacks.Count > 0)
				{
					var task = new PrintTask();
					task.AddRange(docPacks);
					yield return task;
				}
			}
		}

		DocumentCommand GetComplianceDocumentCommand(AccComplianceDocumentHeader complianceDocument)
		{
			ZQuery commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, complianceDocument.GetComplianceBookMenuName());
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(complianceDocument);
			documentCommands.Load();
			BusinessObject[] commands = documentCommands.Find(commandFilter);
			if (!commands.Any())
			{
				throw new ComplianceSequenceHasNoDocumentMenuDefinedException();
			}
			return (DocumentCommand)commands[0];
		}

		public void UpdateComplianceDocumentPrintCountAsPrinted(PrintTask printTask, IEnumerable<ZGuid> complianceDocuments)
		{
			if (complianceDocuments.Any())
			{
				BusinessObjectFactory factoryForUpdateOfPrintedFlag = new BusinessObjectFactory();
				var complianceDocumentsInNewFactory = factoryForUpdateOfPrintedFlag.Load<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, complianceDocuments));
				complianceDocumentsInNewFactory.ForEach((x) =>
				{
					if (printTask.IsDocValidForPrinting(x.PK))
					{
						x.ADH_PrintCount++;
					}
				});
				factoryForUpdateOfPrintedFlag.Save();
			}
		}
	}

	public class ComplianceDocumentPrintFailureInformation
	{
		public ZString PrintTaskErrorMessage { get; set; }

		public ZInt FailureCount { get; set; }

		public void Initialize()
		{
			PrintTaskErrorMessage = ZString.Empty;
			FailureCount = 0;
		}
	}
}
