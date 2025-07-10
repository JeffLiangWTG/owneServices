using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class GovtTaxInvoicePrinter
	{
		public GovtTaxInvoicePrinter()
		{ }

		public GovtTaxInvoicePrinter(ZString invoicePrintingOptionCode)
		{
			this.InvoicePrintingOptionCode = invoicePrintingOptionCode;
		}

		public void PrintGovtTaxInvoices(BusinessObject[] invoicesToPrint)
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var transactionsReloaded = ReloadTransactionsInNewFactory(invoicesToPrint, factory);
				var transactionEligibleForPrinting = InvoicePrintHelper.GetEligibleForPrintingTransactions(transactionsReloaded);

				if (transactionEligibleForPrinting.Any())
				{
					var complianceSequencePrinterMapping = new Dictionary<ZGuid, ZGuid>();
					try
					{
						// check if one printer is used for more than one sequence.
						foreach (var header in transactionEligibleForPrinting)
						{
							var sequence = header.ComplianceSequence;
							var printerPK = header.ComplianceSequencePrinterPK;
							if (printerPK.IsValid)
							{
								if (!complianceSequencePrinterMapping.ContainsKey(printerPK))
								{
									complianceSequencePrinterMapping.Add(printerPK, sequence.PK);
								}
								else if (complianceSequencePrinterMapping[printerPK] != sequence.PK)
								{
									throw new OnePrinterCannotHandleMultipleComplianceSequenceTypesException();
								}
							}
						}
					}
					catch (ComplianceSequenceRelatedException ex)
					{
						if (ex is OnePrinterCannotHandleMultipleComplianceSequenceTypesException)
						{
							throw;
						}
					}

					AllocateComplianceSequenceNumberAndPrint(InvoicePrintingOptionCode, transactionsReloaded, factory);
				}
			}
			catch (ComplianceSequenceRelatedException e)
			{
				Globals.Message.ShowError(e.UserFriendlyMessage);
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowError(Res.GetString("927e1611-7f2d-4154-9f0d-60160eb7ce14", "While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this module to try again."));
			}
			catch (ZCannotSaveException e)
			{
				Globals.Message.ShowError(Res.GetString("92A995A5-39AA-4ACC-B97F-006C38563A96", @"An error has occurred during the rendering of the selected document.
Please review the template and/or network connection before printing this document again.") + System.Environment.NewLine + System.Environment.NewLine + e.Message);
			}
		}

#if DEBUG
		protected virtual
#endif
		void AllocateComplianceSequenceNumberAndPrint(ZString invoicePrintingOptionCode, TransactionHeader[] transactionsReloaded, BusinessObjectFactory factory)
		{
			if (transactionsReloaded.Length > 0)
			{
				var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(invoicePrintingOptionCode, transactionsReloaded, factory);
				allocator.AskReprintInvoiceOption += delegate()
				{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionReprintPrintedInvoice, Res.GetString("f23ed8b6-c66e-4e9c-8b3a-d8ddd8af411b", "Reprint Invoice"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
				allocator.AskReassignNumberOption += delegate()
				{
					return AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value != AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual
						&& Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionAssignNewSequenceNumber, Res.GetString("739ab52b-d2cc-4103-a70a-9bcb2d18d387", "Reassign sequence number"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
				};
				allocator.AskUseCurrentBrancheBooksOption += delegate()
				{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionUseCurrentBranchesBooks, Res.GetString("b6a09469-c1a1-40b8-b5dd-77dd09a04f9b", "Allocate sequence number"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
				allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate()
				{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionUseCurrentBrancheDepartmentsBooks, Res.GetString("1032c498-705a-4ab3-ba70-18db39f711ac", "Allocate sequence number"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
				var participants = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, factory);
				BusinessObjectFactory.SaveTogether(participants);

				if (allocator.PartialAllocationOccured)
				{
					Globals.Message.Show(Res.GetString("db67c241-aabc-47c8-9b52-05cf2ba591e0", @"Some transactions have skipped assigning a Government Compliance Number because existing Compliance Invoice Book setups are missing or do not have enough remaining numbers available.")
						, (NoResString)"Allocate Sequence Number", MessageBoxButtons.OK, MessageBoxIcon.Warning); // This is user message
				}

				if (!allocator.LastGovernmentPrintTaskErrorMessage.IsEmpty)
				{
					if (allocator.LastGovernmentPrintTaskErrorMessage != ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceHasNoDocumentMenuDefinedExceptionMessage || !AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.Value)
					{
						Globals.Message.Show(Res.GetString("6919e844-b756-4940-b202-575c1b2ff14f", @"Some transactions have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing: ") + allocator.LastGovernmentPrintTaskErrorMessage,
							(NoResString)"Printing Government Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning); // This is user message
					}
				}

				// print those government invoices that do not have printer setup in the compliance sequence plus all the standard invoices
				if (allocator.PrintManager != null)
				{
					var task = allocator.PrintManager.PrintTaskForInvoicesWithoutPrinterSetup;
					if (task != null)
					{
						var dest = task.Run(AllowedDeliveryOptions.All, Env.Security.None);
						if (dest != DeliveryInstructionDestination.None &&
							dest != DeliveryInstructionDestination.Preview &&
							dest != DeliveryInstructionDestination.UserCancelled)
						{
							allocator.PrintManager.UpdateTransactionsWithoutPrinterQueueAsPrinted();
						}
					}

#if DEBUG
					printeTaskForTesting = task;
#endif
				}
			}
		}

#if DEBUG
		public PrintTask printeTaskForTesting;
#endif

		TransactionHeader[] ReloadTransactionsInNewFactory(BusinessObject[] transactions, BusinessObjectFactory newFactory)
		{
			List<ZGuid> pKList = new List<ZGuid>();

			foreach (var transaction in transactions)
			{
				if (transaction is TransactionHeader)
				{
					pKList.Add(transaction.PK);
				}
			}
			return newFactory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, pKList.ToArray()));
		}

		ZString InvoicePrintingOptionCode { get; set; }
	}
}
