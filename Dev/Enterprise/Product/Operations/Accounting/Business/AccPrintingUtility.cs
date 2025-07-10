using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public class AccPrintingUtility : IDisposable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string RemittanceAdviceName = "Remittance Advice";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string PaymentVoucherName = "Payment Voucher";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string GeneralLedgerJournalrName = "General Ledger Journal";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string DocBuilderMatchingDocumentName = "DocBuilder Matching Document";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string DocBuilderReceiptDocumentName = "DocBuilder Receipt Document";

		public AccPrintingUtility(BusinessObjectFactory factory, Constants.DataContext dataContext)
		{
			this.Factory = factory;
			this.DataContext = dataContext;
		}

		public AccPrintingUtility(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public static bool CheckMenuItemExistsForChequeTemplate(string chequeTemplate, IDocumentSupportable payment, bool useLegacyDocument = false)
		{
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(payment);
			documentCommands.Load();
			BusinessObject[] menuItems = documentCommands.Find(GetTemplateMenuFilter(chequeTemplate, useLegacyDocument, false));
			return menuItems.Length == 1;
		}

		public static string GetMissingChequeTemplateMenuErrorMessage(string chequeTemplate)
		{
			return Res.GetString("1466a7a8-2bf2-4258-8ace-83631653e817", "Please make sure '{0}' Document menu with Empty menu path value exists for cheque template {0}. If not, please create one through the Document Menu Customization screen", chequeTemplate);
		}
		public DeliveryInstructionDestination PrintDocument(DocumentWrapper[] wrappers, string templateName, AllowedDeliveryOptions options)
		{
			if (wrappers != null && wrappers.Length == 1)
			{
				DocumentWrapper wrapper = wrappers[0];
				using (DocumentPack docPack = new DocumentPack(GetCorrespondingDocumentCommand(templateName, ((IBODocDataProvider)wrapper).ParentBusinessObject as IDocumentSupportable)))
				{
					AddDocumentToPack(templateName, wrapper, docPack);
#if DEBUG
					if (Globals.IsTest)
					{
						Test_TemplateNames.Add(templateName);
					}
#endif
					return RunPrintTask(docPack, null, null, options);
				}
			}
			else
			{
				throw new DeveloperNotificationException("The wrapper for this Business object is not properly created.");
			}
		}

		DocumentCommand GetCorrespondingDocumentCommand(string menuName, IDocumentSupportable parent)
		{
			DocumentCommand result = null;

			if (parent != null)
			{
				DocumentCommandCollection documentCommands = new DocumentCommandCollection(parent);
				documentCommands.Load();
				if (documentCommands.Count > 0)
				{
					ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
					BusinessObject[] menuItems = documentCommands.Find(filter);

					if (menuItems.Length > 0)
					{
						result = (DocumentCommand)menuItems[0];
					}
					else
					{
						result = documentCommands[0];
					}
				}
			}

			return result;
		}

		public DeliveryInstructionDestination PrintDocument(IDocumentSupportable transaction, string menuName, AllowedDeliveryOptions options)
		{
			return PrintDocument(transaction, menuName, options, ZGuid.Empty, false);
		}

		public DeliveryInstructionDestination PrintDocument(IDocumentSupportable transaction, string menuName, AllowedDeliveryOptions options, ZGuid printQueuePK, bool allowMultipleCopies)
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			DocumentCommand docCommand;
			if (menuName == RemittanceAdviceName)
			{
				if (DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.Value)
				{
					docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName, ZString.Empty, ZString.Empty);
				}
				else
				{
					docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName, Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments, ZString.Empty);
				}
			}
			else if (menuName == PaymentVoucherName)
			{
				if (DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.Value)
				{
					docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName, ZString.Empty, ZString.Empty);
				}
				else
				{
					docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName, Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments, ZString.Empty);
				}
			}
			else if (menuName == GeneralLedgerJournalrName)
			{
				docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName, true, true);
			}
			else
			{
				docCommand = DocumentCommand.GetDocumentCommand(tempFactory, transaction, menuName);
			}

			if (docCommand != null)
			{
				return PrintDocument(transaction, docCommand, options, printQueuePK, allowMultipleCopies);
			}
			else
			{
				throw new ReportException(string.Format("Menu used for this document has been deleted from the system. Document not printed. MenuName-{0}", menuName));
			}
		}

		public DeliveryInstructionDestination PrintDocument(IDocumentSupportable transaction, DocumentCommand docCommand, AllowedDeliveryOptions options, ZGuid printQueuePK, bool allowMultipleCopies)
		{
			if (docCommand != null)
			{
				docCommand.Parent = transaction;
				docCommand.AllowMultipleCopies = allowMultipleCopies;

#if DEBUG
				Test_NumberOfCopiesIsReadOnly = !docCommand.AllowMultipleCopies;
				if (Globals.IsTest)
				{
					Test_TemplateNames.Add(docCommand.SU_MenuName);
				}
#endif

				docCommand.SU_DraftOption = DraftOptionsList.Codes.Final;
				if (printQueuePK.IsValid)
				{
					return RunPrintSet(docCommand, AllowedDeliveryOptions.All, printQueuePK);
				}
				else
				{
					return RunPrintSet(docCommand, options, ZGuid.Empty);
				}
			}
			else
			{
				throw new ReportException(string.Format("Document Command was not specified. Document not printed."));
			}
		}

		public DeliveryInstructionDestination PrintDocuments(DocumentWrapper[] wrappers, string templateName, AllowedDeliveryOptions options)
		{
			if (wrappers != null && wrappers.Length > 0)
			{
				DocumentPack docPack = new DocumentPack(GetCorrespondingDocumentCommand(templateName, ((IBODocDataProvider)wrappers[0]).ParentBusinessObject as IDocumentSupportable));

				foreach (DocumentWrapper wrapper in wrappers)
				{
					AddDocumentToPack(templateName, wrapper, docPack);
				}
#if DEBUG
				if (Globals.IsTest)
				{
					Test_TemplateNames.Add(templateName);
				}
#endif
				return RunPrintTask(docPack, null, null, options);
			}
			else
			{
				throw new DeveloperNotificationException("The wrapper for this Business object is not properly created.");
			}
		}

		public DeliveryInstructionDestination PrintDocuments(IDocumentSupportable payment, string[] templateNames, AllowedDeliveryOptions options, bool allowMultipleCopies)
		{
			if (payment != null)
			{
				BusinessObjectFactory tempFactory = new BusinessObjectFactory();
				DocumentCommand command = DocumentCommand.GetDocumentCommand(tempFactory, payment, templateNames[0]);
				DocumentPack docPack = new DocumentPack(command, payment, null, null);
				docPack.RemoveAll();
				foreach (string templateName in templateNames)
				{
					DocumentCommand docCommand = DocumentCommand.GetDocumentCommand(tempFactory, payment, templateName);
					if (docCommand != null)
					{
						docCommand.Parent = payment;
						docCommand.AllowMultipleCopies = allowMultipleCopies;
#if DEBUG
						Test_NumberOfCopiesIsReadOnly = !docCommand.AllowMultipleCopies;
#endif
						docCommand.SU_DraftOption = DraftOptionsList.Codes.Final;
						AddDocumentToPack(docCommand, payment, docPack);
					}
				}
#if DEBUG
				DeliveryInstructions instruction = new DeliveryInstructions(new DocumentPack());
				instruction.Destination = DeliveryInstructionDestination.DummyDestinationForTesting;

				if (Globals.IsTest)
				{
					return RunPrintTask(docPack, command, instruction, options);
				}
#endif
				{
					return RunPrintTask(docPack, command, null, options);
				}
			}
			else
			{
				throw new DeveloperNotificationException("The Business object is not properly created.");
			}
		}

		public void AddPaymentDocumentToPack(ZString menuName, IDocumentSupportable payment, bool useLegacyDocument, bool findInSystemDefinedDocumentOnly)
		{
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(payment);
			documentCommands.Load();
			BusinessObject[] paymentCommands = documentCommands.Find(GetTemplateMenuFilter(menuName, useLegacyDocument, findInSystemDefinedDocumentOnly));
			if (paymentCommands.Length != 1)
			{
				throw new InvalidOperationException("Unable to find Document command for " + menuName + ".");
			}
			AddDocumentCommandToDocPack(payment, (DocumentCommand)paymentCommands[0], menuName);
		}

		static ZQuery GetTemplateMenuFilter(ZString menuName, bool useLegacyDocument, bool findInSystemDefinedDocumentOnly)
		{
			var filter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, useLegacyDocument ? Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments : String.Empty);
			if (!useLegacyDocument && findInSystemDefinedDocumentOnly)
			{
				filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			}
			return filter;
		}

#if DEBUG
		public void AddPaymentDocumentToPack(DocumentWrapper[] wrappers, string templateName, IDocumentSupportable payment, Constants.DataContext dataContext)
		{
			if (wrappers != null && wrappers.Length > 0)
			{
				var stmMenuItem = GetCorrespondingDocumentCommand(templateName, payment);
				PrintTask task = GetTaskForPayment(stmMenuItem, dataContext.ToString());

				DocumentPack newDocumentPack = new DocumentPack(stmMenuItem);
				newDocumentPack.DocumentSupporter = payment.DocumentSupporter;

				foreach (DocumentWrapper wrapper in wrappers)
				{
					AddDocumentToPack(templateName, wrapper, newDocumentPack, dataContext);
				}
				task.Add(newDocumentPack);
			}
			else
			{
				throw new DeveloperNotificationException("The wrapper for this Business object is not properly created.");
			}
		}
#endif

		public DeliveryInstructionDestination PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions options)
		{
			return RunPrintTask(PaymentDocumentPacks, options, ZGuid.Empty);
		}

		public DeliveryInstructionDestination AutoPrintDocumentPacksForPaymentCollection(ZGuid printQueuePK)
		{
			return RunPrintTask(PaymentDocumentPacks, AllowedDeliveryOptions.All, printQueuePK);
		}

		#region Implementation

		protected BusinessObjectFactory Factory;
		protected Constants.DataContext DataContext;

		public Dictionary<string, PrintTask> PaymentDocumentPacks
		{
			get
			{
				if (fPaymentDocumentPacks == null)
				{
					fPaymentDocumentPacks = new Dictionary<string, PrintTask>();
				}
				return fPaymentDocumentPacks;
			}
		}
		Dictionary<string, PrintTask> fPaymentDocumentPacks;

#if DEBUG
		public int GetCountOfPaymentDocumentPacks()
		{
			return PaymentDocumentPacks.Count;
		}
#endif

		protected void AddDocumentToPack(string templateName, DocumentWrapper wrapper, DocumentPack docPack)
		{
			AddDocumentToPack(templateName, wrapper, docPack, this.DataContext);
		}

		protected void AddDocumentToPack(string templateName, DocumentWrapper wrapper, DocumentPack docPack, Constants.DataContext dataContext)
		{
			ExcelTemplate template = ExcelTemplateRetriever.GetTemplate(templateName, dataContext, Factory);
			if (template != null)
			{
				ZString reportName = GetReportNameForTemplate(templateName);
				Report rpt = new Report(docPack, template, wrapper, reportName, null, DocumentDirection.ANY, false);
				docPack.Add(rpt);
#if DEBUG
				if (Globals.IsTest)
				{
					Test_TemplateNames.Add(templateName);
				}
#endif
			}
			else
			{
				throw new ReportException("Template(s) used for this document has been deleted from the system. Documents not printed.");
			}
		}

		protected void AddDocumentToPack(DocumentCommand docCommand, IDocumentSupportable payment, DocumentPack docPack)
		{
			docPack.AddReportsToPack(docCommand, null, payment, null);
		}

		protected void AddDocumentCommandToDocPack(IDocumentSupportable businessObject, DocumentCommand docCommandToAdd, string menuItemName)
		{
			if (PaymentDocumentPacks.ContainsKey(menuItemName))
			{
				((DocumentPrintSet)PaymentDocumentPacks[menuItemName]).Add(new DocumentPack(docCommandToAdd, businessObject, null, null));
			}
			else
			{
				DocumentPrintSet newDocPrintSet = new DocumentPrintSet(docCommandToAdd, null);
				PaymentDocumentPacks.Add(menuItemName, newDocPrintSet);
#if DEBUG
				if (Globals.IsTest)
				{
					Test_TemplateNames.Add(menuItemName);
				}
#endif
			}
		}

		PrintTask GetTaskForPayment(IStmMenuItem menuItem, string documentName)
		{
			if (PaymentDocumentPacks.ContainsKey(documentName))
			{
				return PaymentDocumentPacks[documentName];
			}
			else
			{
				using (PrintTask newPrintTask = new PrintTask(menuItem))
				{
					PaymentDocumentPacks.Add(documentName, newPrintTask);
					return newPrintTask;
				}
			}
		}

		protected virtual ZString GetReportNameForTemplate(string templateName)
		{
			return templateName;
		}

		protected virtual DeliveryInstructionDestination RunPrintTask(DocumentPack docPack, DocumentCommand docCommand, DeliveryInstructions instruction, AllowedDeliveryOptions options)
		{
			PrintTask printTask;
			if (docCommand != null)
			{
				printTask = new PrintTask(docCommand);
			}
			else
			{
				printTask = new PrintTask();
				printTask.DeliveryInstructionsDefaultPK = MenuPKForAPTransactionDeliveryInstructionHolder;
			}
			if (docPack.Count > 0)
			{
				printTask.Add(docPack);
				if (instruction != null)
				{
					return printTask.RunWithPartialInstructions(instruction.DeliveryOptions, instruction, Env.Security.None);
				}
				else
				{
					return printTask.Run(options, Env.Security.None);
				}
			}
			return DeliveryInstructionDestination.None;
		}

		DeliveryInstructionDestination RunPrintTask(Dictionary<string, PrintTask> docPacks, AllowedDeliveryOptions options, ZGuid printQueuePK)
		{
			DeliveryInstructionDestination result = DeliveryInstructionDestination.None;
			if (docPacks.Count > 0)
			{
				if (printQueuePK == ZGuid.Empty)
				{
					foreach (PrintTask task in docPacks.Values)
					{
						result = task.Run(options, Env.Security.None);
#if DEBUG
						Test_DeliveryInstructionsPassedForPrinting.Add(task[0].DeliveryInstructions);
#endif
					}
				}
				else if (printQueuePK.IsValid)
				{
					foreach (PrintTask task in docPacks.Values)
					{
						foreach (DocumentPack docPack in task.GetDocumentPacks())
						{
							DeliveryInstructions instructions = new DeliveryInstructions();
							instructions.Destination = DeliveryInstructionDestination.Print;
							instructions.PrinterDelivery.PrintQueuePK = printQueuePK;
							var deliveryContacts = new DocAutoDelivery().GetDeliveryContactsForDocPack(docPack.StmMenuCommand, docPack.DocumentSupporter, docPack.DocumentGroup, docPack.Parent != null ? docPack.Parent.ParentMenuCommand : null);
							if (deliveryContacts.Any())
							{
								instructions.Recipients.Add(deliveryContacts.First());
							}
							task.RunDocumentPack(instructions, docPack);
#if DEBUG
							Test_DeliveryInstructionsPassedForPrinting.Add(instructions);
#endif
						}
					}
				}
			}
			return result;
		}

		public ZGuid MenuPKForAPTransactionDeliveryInstructionHolder
		{
			get { return new ZGuid("4e5eb33f-5913-458b-9310-a405a140739b"); }
		}

		protected virtual DeliveryInstructionDestination RunPrintSet(DocumentCommand command, AllowedDeliveryOptions options, ZGuid printQueuePK)
		{
			DeliveryInstructionDestination result = DeliveryInstructionDestination.None;

			using (DocumentPrintSet printSet = new DocumentPrintSet(command, null))
			{
				if (printQueuePK.IsValid)
				{
					printSet.CustomNotifications = new NotificationBuffer();
					DeliveryInstructions instructions = new DeliveryInstructions();
					instructions.Destination = DeliveryInstructionDestination.Print;
					instructions.PrinterDelivery.PrintQueuePK = printQueuePK;
					var deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(printSet.ParentMenuCommand, printSet[0].DocumentSupporter);
					if (deliveryContacts.Any())
					{
						instructions.Recipients.Add(deliveryContacts.First());
					}
					printSet.Run(instructions);
#if DEBUG
					Test_DeliveryInstructionsPassedForPrinting.Add(instructions);
#endif
				}
				else
				{
					result = printSet.Run(options, Env.Security.None);
#if DEBUG
					Test_DeliveryInstructionsPassedForPrinting.Add(printSet[0].DeliveryInstructions);
#endif
				}

				var customNotifications = printSet.CustomNotifications as NotificationBuffer;

				if (customNotifications != null && (customNotifications.HasErrors || customNotifications.HasWarnings))
				{
					throw new ZCannotSaveException(customNotifications.AsString, Res.GetString("2097a2f7-3676-47b6-ae38-3e3b0de51f4d", "Report Error"));
				}
				return result;
			}
		}

		public void Dispose()
		{
			if (fPaymentDocumentPacks != null)
			{
				foreach (var pack in fPaymentDocumentPacks.Values)
				{
					pack.Dispose();
				}
			}
		}

		#endregion

#if DEBUG
		public List<DeliveryInstructions> Test_DeliveryInstructionsPassedForPrinting = new List<DeliveryInstructions>();
		public bool Test_NumberOfCopiesIsReadOnly;
		public List<string> Test_TemplateNames = new List<string>();

		public bool DeliveryInstructionsPassedForPrintingHasEmptyRecipient()
		{
			if (Test_DeliveryInstructionsPassedForPrinting.Any()
				&& Test_DeliveryInstructionsPassedForPrinting.First().Recipients.Count > 0
				&& !Test_DeliveryInstructionsPassedForPrinting.First().Recipients[0].OrgHeaderPK.IsEmpty)
			{
				return false;
			}
			return true;
		}

#endif
	}
}
