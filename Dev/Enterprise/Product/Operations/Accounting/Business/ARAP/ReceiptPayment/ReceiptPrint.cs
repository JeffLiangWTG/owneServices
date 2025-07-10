using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	/// <summary>
	/// Print handler for Receipt Matching Report
	/// </summary>
	public class ReceiptPrint : AccPrintingUtility
	{
		public ReceiptPrint()
			: base(new BusinessObjectFactory(), Core.Constants.DataContext.TransactionHeader)
		{
		}

		public void PrintReceiptMatchingReport(TransactionHeader transactionToPrint, ZString reportName)
		{
			if (reportName == AccountingUtils.AccountingDocumentTitles.ReceiptJournal && DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.Value)
			{
				var docCommand = DocumentCommand.GetDocumentCommand(Factory, transactionToPrint, DocBuilderReceiptDocumentName, onlySystemDefined: true, isDocBuilder: true);
				PrintDocument(transactionToPrint, docCommand, AllowedDeliveryOptions.All, ZGuid.Empty, false);
			}
			else if (reportName == AccountingUtils.AccountingDocumentTitles.ReceiptMatching && DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.Value)
			{
				var docCommand = DocumentCommand.GetDocumentCommand(Factory, transactionToPrint, DocBuilderMatchingDocumentName, onlySystemDefined: true, isDocBuilder: true);
				PrintDocument(transactionToPrint, docCommand, AllowedDeliveryOptions.All, ZGuid.Empty, false);
			}
			else
			{
				Print(reportName, transactionToPrint.PK.ToGuid());
			}
		}

		void Print(ZString reportName, Guid pK)
		{
			this.ReportName = reportName;
			TransactionHeader transaction = Factory.Load<TransactionHeader>(pK);
			if (transaction != null)
			{
				DocumentWrapper[] wrappers = transaction.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TransactionHeader, null);
				PrintDocument(wrappers, (NoResString)"Receipt Matching", AllowedDeliveryOptions.All);
			}
		}

		public void Print(ZString reportName, Guid[] pKs)
		{
			this.ReportName = reportName;

			var menuName = reportName;
			var useDocBuilder = false;

			if (reportName == AccountingUtils.AccountingDocumentTitles.ReceiptJournal && Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderReceiptDocument.Value)
			{
				menuName = AccPrintingUtility.DocBuilderReceiptDocumentName;
				useDocBuilder = true;
			}
			else if (reportName == AccountingUtils.AccountingDocumentTitles.ReceiptMatching && Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderMatchingDocument.Value)
			{
				menuName = AccPrintingUtility.DocBuilderMatchingDocumentName;
				useDocBuilder = true;
			}

			if (useDocBuilder)
			{
				foreach (Guid pk in pKs)
				{
					TransactionHeader transaction = Factory.Load<TransactionHeader>(pk);
					if (transaction != null)
					{
						AddDocumentCommandToDocPack(transaction, GetExactDocumentCommand(transaction, menuName));
					}
				}

				PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
			}
			else
			{
				var wrappers = new List<DocumentWrapper>();
				foreach (Guid pK in pKs)
				{
					TransactionHeader transaction = Factory.Load<TransactionHeader>(pK);
					if (transaction != null)
					{
						wrappers.AddRange(transaction.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TransactionHeader, null));
					}
				}

				PrintDocuments(wrappers.ToArray(), AccountingUtils.AccountingDocumentTitles.ReceiptMatching, AllowedDeliveryOptions.All);
			}
		}

		DocumentCommand GetExactDocumentCommand(TransactionHeader transaction, ZString menuName, bool useLegacyDocuments = false)
		{
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(transaction);
			documentCommands.Load();
			ZQuery filter = new ZQuery(Enterprise.ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, menuName);
			filter.AddToFilter(Enterprise.ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, useLegacyDocuments ? Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments : String.Empty);
			BusinessObject[] foundCommands = documentCommands.Find(filter);

			if (foundCommands.Length != 1)
			{
				throw new ApplicationException("Unable to find Document command for " + menuName + ".");
			}

			return (DocumentCommand)foundCommands[0];
		}

		void AddDocumentCommandToDocPack(TransactionHeader transaction, DocumentCommand documentCommand, bool groupByOrg = true)
		{
			PrintTask task;
			if (PaymentDocumentPacks.TryGetValue(documentCommand.SU_MenuName, out task))
			{
				var printSet = (DocumentPrintSet)task;
				var existingPack = groupByOrg ? printSet.GetDocumentPacks().FirstOrDefault(x => ((TransactionHeader)x.DocumentSupporter.BusinessObject).AH_OH == transaction.AH_OH) : null;

				if (existingPack != null)
				{
					existingPack.AddReportsToPack(documentCommand, null, transaction, null);
				}
				else
				{
					printSet.Add(new DocumentPack(documentCommand, transaction, null, null));
				}
			}
			else
			{
				DocumentPrintSet newPrintSet = new DocumentPrintSet(documentCommand, null);
				PaymentDocumentPacks.Add(documentCommand.SU_MenuName, newPrintSet);
#if DEBUG
				if (Globals.IsTest)
				{
					Test_TemplateNames.Add(documentCommand.SU_MenuName);
				}
#endif
			}
		}

		protected ZString ReportName;
		protected override ZString GetReportNameForTemplate(string templateName)
		{
			return ReportName;
		}
	}
}