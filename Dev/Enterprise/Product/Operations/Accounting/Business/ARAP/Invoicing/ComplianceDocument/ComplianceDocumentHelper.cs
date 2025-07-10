using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration.Accounting;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ComplianceDocumentHelper : IVoidComplianceDocumentHeader
	{
		#region IVoidComplianceDocumentHeader

		public ComplianceDocumentHelper()
		{ }

		public void CreateVoidedComplianceDocumentHeader(BusinessObjectFactory factory, ZGuid sequenceBook, ZString subType, ZString documentNumber)
		{
			var arComplianceDocumentHeader = factory.New<ARComplianceDocumentHeader>();
			arComplianceDocumentHeader.ADH_XD_ComplianceBook = sequenceBook;
			arComplianceDocumentHeader.ADH_DocumentNumber = documentNumber;
			arComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
			arComplianceDocumentHeader.ADH_DocumentDate = ZDateTime.Now;
			arComplianceDocumentHeader.ADH_ReportingPeriod = ZDateTime.Now.Year * 100 + ZDateTime.Now.Month;
			arComplianceDocumentHeader.ADH_ComplianceSubType = subType;
			arComplianceDocumentHeader.ADH_DocumentType = "VAT";
			arComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			arComplianceDocumentHeader.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			arComplianceDocumentHeader.ADH_Description = (NoResString)"Sequence number voided via compliance invoice book";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			arComplianceDocumentHeader.Logs.AddNew(Events.EditedARecord, "Compliance Document Voided");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		#endregion

		#region public static methods

		public static SecurityCheckpoint GetCreateComplianceDocumentsSecurity(string ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					return Env.Security.CreatePayablesComplianceDocuments;
				case LedgerTypes.AccountsReceivable:
					return Env.Security.CreateReceivablesComplianceDocuments;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Not supportable for ledger {0}", ledger));
			}
		}

		public static ZString[] AllocateComplianceDocuments(BusinessObjectFactory factory, AccComplianceDocumentHeader[] complianceDocuments)
		{
			var invalidSequenceCodes = new HashSet<ZString>();

			foreach (var complianceDocument in complianceDocuments.Where(x => x.IsAdded).OrderBy(x => x.ADH_DocumentDate))
			{
				if (!complianceDocument.ADH_XD_ComplianceBook.IsValid)
				{
					complianceDocument.SetComplianceSequenceBook();
				}

				if (complianceDocument.ComplianceBook != null)
				{
					if (invalidSequenceCodes.Contains(complianceDocument.ComplianceBook.XD_Code))
					{
						continue;
					}

					if (AccountingMasterFilesRegistry.Instance.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Value && CheckSyncOfDocumentNumberAndDocumentDate(factory, complianceDocument))
					{
						invalidSequenceCodes.Add(complianceDocument.ComplianceBook.XD_Code);
					}
					else
					{
						complianceDocument.SetComplianceDocumentNumber();
					}
				}
			}

			return invalidSequenceCodes.ToArray();
		}

		public static ComplianceDocumentPrintFailureInformation PrintComplianceDocument(AccComplianceDocumentHeader[] complianceDocuments)
		{
			var printManager = new ComplianceDocumentPrintManager();

			var printTasks = printManager.GetComplianceDocumentPrintTasks(complianceDocuments);
			foreach (var task in printTasks)
			{
				using (task)
				{
					var complianceDocumentPKs = task.GetDocumentPacks().Select(x => x.BizObject).Cast<AccComplianceDocumentHeader>().Select(y => y.PK);
					var printQueuePK = ((AccComplianceDocumentHeader)task.GetFirstDocumentPack().BizObject).ComplianceBook.XD_SQ_DocumentPrintQueue;
					if (printQueuePK.IsValid)
					{
						var instructions = new DeliveryInstructions() { Destination = DeliveryInstructionDestination.Print };
						instructions.PrinterDelivery.PrintQueuePK = printQueuePK;
						task.Run(instructions);

						printManager.UpdateComplianceDocumentPrintCountAsPrinted(task, complianceDocumentPKs);
					}
					else
					{
						var dest = task.Run(Env.Security.None);
						if (dest != DeliveryInstructionDestination.None &&
							dest != DeliveryInstructionDestination.Preview &&
							dest != DeliveryInstructionDestination.UserCancelled)
						{
							printManager.UpdateComplianceDocumentPrintCountAsPrinted(task, complianceDocumentPKs);
						}
					}
				}
			}

			return printManager.PrintFailureInfomation;
		}

		public static ZString GetCompanyProxyVATNumber() => GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("VAT", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? string.Empty;

		public static ZString GetCompanyProxyGTXNumber() => GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("GTX", GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? string.Empty;

		public static ZString GetFormatCode(ZString subType, ZString ledger) => ledger == LedgerTypes.AccountsPayable ? GetFormatCodeForPayable(subType) : GetFormatCodeForReceivable(subType);

		public static ZString PrintingCaption => Res.GetString("5ceec3b2-e16c-4401-9c55-b4254eebdfef", "Print Compliance Document");

		public static ZString GetNotSyncErrorMessage(ZString[] complianceBooks)
		{
			return Res.GetString("A83B10F5-EFF8-4B64-B5E8-9207301C2E56", "Document number cannot be allocated for Invoice Book {0} as number sequence and date sequence will not synchronize if allocated.", ZString.Join(",", complianceBooks));
		}

		public static void PromptAndPrintComplianceDocument(
			Func<string, bool> confirmToPrintDocument,
			Action<string, bool> showPrintingResult,
			IEnumerable<AccComplianceDocumentHeader> complianceDocuments,
			string message,
			bool isSkipPrompt = false)
		{
			StringBuilder stringBuilder = new StringBuilder(message);
			stringBuilder.Append(message.IsNullOrEmpty() ? string.Empty : " ");
			stringBuilder.Append(Res.GetString("822b9e17-8171-44f8-9a94-d05bf6ce9b95", "Do you want to print Compliance Document?"));

			if (isSkipPrompt || confirmToPrintDocument(stringBuilder.ToString()))
			{
				string msg = ValidateAndPrintComplianceDocument(new BusinessObjectFactory(), complianceDocuments, out bool isError);
				if (!string.IsNullOrEmpty(msg))
				{
					showPrintingResult(msg, isError);
				}
			}
		}

		public static string JoinAndAttachLineNumber(List<string> messages)
		{
			if (messages == null)
			{
				return null;
			}
			if (messages.Count == 0)
			{
				return string.Empty;
			}
			if (messages.Count == 1)
			{
				return messages[0];
			}

			string result = "";
			for (var i = 0; i < messages.Count ; i++)
			{
				var lineNumber = i + 1;
				result += $"{lineNumber}. {messages[i]}" + System.Environment.NewLine;
			}
			result = result.TrimEnd();
			return result;
		}

		#endregion

		#region private static methods

		static ZBool CheckSyncOfDocumentNumberAndDocumentDate(BusinessObjectFactory factory, AccComplianceDocumentHeader complianceDocument)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_XD_ComplianceBook, complianceDocument.ADH_XD_ComplianceBook));
			query.AddToFilter(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentDate, SQLComparisonOperator.GreaterThanOrEqualTo, complianceDocument.ADH_DocumentDate.AddDays(1).Date));
			query.AddToFilter(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, SQLComparisonOperator.NotEqual, ZString.Empty));
			return factory.Exists(typeof(AccComplianceDocumentHeader), query);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ZString GetFormatCodeForPayable(ZString subType)
		{
			switch (subType)
			{
				case "TXI":
				case "TXP":
					return "21";
				case "TDC":
				case "TDP":
				case "TDI":
					return "22";
				case "TCR":
				case "TCE":
					return "23";
				case "TCD":
					return "24";
				case "TXE":
				case "TXC":
					return "25";
				case "TSX":
					return "26";
				case "TSD":
					return "27";
				case "TXS":
					return "28";
				default:
					return "  ";
			}
		}

		static ZString GetFormatCodeForReceivable(ZString subType)
		{
			switch (subType)
			{
				case "TXI":
				case "TXP":
					return "31";
				case "TDC":
				case "TDP":
				case "TDI":
					return "32";
				case "TCR":
				case "TCE":
					return "33";
				case "TCD":
					return "34";
				case "TXE":
				case "TXC":
					return "35";
				default:
					return "  ";
			}
		}

		#region Print Messages

		#region Message Headers

		static string PrintAllFailed => Res.GetString("E7AB320B-6F1D-45D1-A176-14C2643F5C79", "No compliance document printed due to one of the following reasons:");
		static string PrintSomeFailed => Res.GetString("9A81738A-B19B-4A73-9AB0-99FAB25A08DC", "Some of the compliance document records selected cannot be printed due to one of the following reasons:");
		static string PrintException => Res.GetString("C0E90883-FDB4-41A7-A8B3-2F8246451746", "The compliance document cannot be printed.");

		#endregion

		#region Error Reasons

		static string Printed => Res.GetString("BA47344F-FAC0-488B-B232-7B640BAB005E", "The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.");
		static string NoMatchingComplianceInvoiceBook => Res.GetString("22C6E3FC-AB33-49CC-8C50-918784D40328", "The compliance document cannot be printed as no matching compliance invoice book is found.");
		static string AllNoMatchingComplianceInvoiceBook => Res.GetString("91759A1B-8A47-4427-AE14-1DEA1745B2E6", @"The selected compliance document(s) cannot be printed as no matching compliance invoice book is found.");
		static string PrintCountExceeded => Res.GetString("B0C56A67-9680-4362-A94C-C436E81794C9", "The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in {0}{1}{2}.", AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.Category.Replace(RegistryItemSet.Delimiter, " > "), " > ", AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.Caption);
		static string Voided => Res.GetString("04AD94A6-30B4-4A47-A5B7-D876F8B02F4C", "The compliance document has already been voided.");
		static string TheOnlyOneVoided => Res.GetString("8CCCEE10-FE99-43D2-BB95-DF05F049A4C7", @"The selected compliance document has been voided and cannot be printed.");
		static string AllVoided => Res.GetString("2E80F6FD-4EED-4340-B484-C5AD3BF933A9", @"These selected compliance documents have been voided and cannot be printed.");
		static string OrganizationProblem => Res.GetString("73D77E6D-02E7-4BD0-8162-4AA007F2DA81", "The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.");
		static string AllhaveOrganizationProblem => Res.GetString("BD8497DE-4A6A-451B-AAC4-432A1F2DEDE0", @"Document(s) cannot be printed for a TXE compliance document as the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.");
		static string TXENotAllowedByRegistry => Res.GetString("D585C883-FBDC-4B76-90C9-ACDF5D2E0CA6", "Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.");
		static string PrintTaskError => Res.GetString("5C8975D2-EA85-4A17-BD6B-8F0009FBC546", "Some compliance document have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing:");
		static string Suggestion => Res.GetString("75F98EDA-0687-4D1F-B57B-45A018D3C747", @"Please review compliance invoice books, registry settings and selection before printing.");

		#endregion

		static List<string> FixedPossibleReasons => new List<string>() {
				Printed,
				NoMatchingComplianceInvoiceBook,
				PrintCountExceeded,
				Voided,
				OrganizationProblem,
				TXENotAllowedByRegistry
		};

		#endregion

		static string ValidateAndPrintComplianceDocument(BusinessObjectFactory factory, IEnumerable<AccComplianceDocumentHeader> complianceDocuments, out bool isError)
		{
			isError = true;
			var errorMessage = ZString.Empty;

			if (!Env.Security.PrintReceivablesComplianceDocuments.IsAllowed)
			{
				Env.Security.PrintReceivablesComplianceDocuments.ShowError();
				return errorMessage;
			}

			var validComplianceDocumentsForPrinting = GetValidComplianceDocumentsForPrinting(complianceDocuments, out errorMessage);
			if (!errorMessage.IsEmpty)
			{
				return errorMessage;
			}

			if (validComplianceDocumentsForPrinting.IsCountMoreThan(0))
			{
				var possiblereasonsForNotPrinting = FixedPossibleReasons;

				var complianceDocumentsToAllocate = factory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, validComplianceDocumentsForPrinting.ToArray()));
				var complianceDocumentsToPrint = GetValidComplianceDocumentsForPrintingAfterAllocate(factory, complianceDocumentsToAllocate, out ZString errorMessageForAllocated, out ZString errorMessageForNotSync);
				if (complianceDocuments.IsCountEqualTo(1))
				{
					if (!errorMessageForAllocated.IsEmpty) { return errorMessageForAllocated; }
					if (!errorMessageForNotSync.IsEmpty) { return errorMessageForNotSync; }
				}

				if (!errorMessageForNotSync.IsEmpty)
				{
					possiblereasonsForNotPrinting.Add(errorMessageForNotSync);
				}
				var possiblereasonsForNotPrintingMessage = JoinAndAttachLineNumber(possiblereasonsForNotPrinting);
				var errorMessageForAllCanNotPrinted = $"{PrintAllFailed}\r\n{possiblereasonsForNotPrintingMessage}\r\n\r\n{Suggestion}";
				var errorMessageForSomeCanNotPrinted = $"{PrintSomeFailed}\r\n{possiblereasonsForNotPrintingMessage}\r\n\r\n{Suggestion}";

				var printFailureCount = 0;
				if (complianceDocumentsToPrint.IsCountMoreThan(0))
				{
					var complianceDocumentPrintFailureInformation = PrintComplianceDocument(complianceDocumentsToPrint.ToArray());
					printFailureCount = complianceDocumentPrintFailureInformation.FailureCount;
					if (!complianceDocumentPrintFailureInformation.PrintTaskErrorMessage.IsEmpty && !AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.Value)
					{
						errorMessage = $"{PrintTaskError} {complianceDocumentPrintFailureInformation.PrintTaskErrorMessage}";
						isError = false;
						return errorMessage;
					}
				}
				else
				{
					return errorMessageForAllCanNotPrinted;
				}

				if (complianceDocumentsToPrint.Count() < complianceDocuments.Count() && complianceDocuments.IsCountMoreThan(1))
				{
					if (printFailureCount == complianceDocumentsToPrint.Count())
					{
						errorMessage = errorMessageForAllCanNotPrinted;
					}
					else
					{
						isError = false;
						errorMessage = errorMessageForSomeCanNotPrinted;
					}
				}
			}
			else
			{
				errorMessage = string.Join(System.Environment.NewLine,
					PrintAllFailed,
					JoinAndAttachLineNumber(FixedPossibleReasons),
					Suggestion);
			}
			return errorMessage;
		}

		static IEnumerable<AccComplianceDocumentHeader> GetValidComplianceDocumentsForPrintingAfterAllocate(BusinessObjectFactory factory, AccComplianceDocumentHeader[] complianceDocumentsToAllocate, out ZString errorMessage, out ZString errorMessageForNotSync)
		{
			errorMessage = ZString.Empty;
			errorMessageForNotSync = ZString.Empty;
			var errorMessages = new List<string>();
			ZString[] notSyncSequenceBooks;
			try
			{
				if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
				{
					Func<AccComplianceDocumentHeader, bool> subTypeCodeIsTXEPredicate = n => n is ARComplianceDocumentHeader header && header.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;

					if (complianceDocumentsToAllocate.Any(subTypeCodeIsTXEPredicate))
					{
						complianceDocumentsToAllocate = complianceDocumentsToAllocate.Where(n => !subTypeCodeIsTXEPredicate(n)).ToArray();
						errorMessages.Add(TXENotAllowedByRegistry);
					}
				}

				if (complianceDocumentsToAllocate.Any())
				{
					var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(complianceDocumentsToAllocate.FirstOrDefault().Company.Country.Code);
					var complianceDocumentNumberProvider = (countryFactory as IInstanceProvider<IComplianceDocumentNumberProvider>)?.Get();
					var allocateComplianceDocumentNumberErrorMessage = complianceDocumentsToAllocate
						.Select(x => x is ARComplianceDocumentHeader header
							? complianceDocumentNumberProvider?.AllocateComplianceDocumentNumberErrorMessage(new ARComplianceDocumentHeader[] { header })
							: string.Empty)
						.FirstOrDefault(x => !string.IsNullOrEmpty(x));
					if (!string.IsNullOrEmpty(allocateComplianceDocumentNumberErrorMessage))
					{
						errorMessages.Add(allocateComplianceDocumentNumberErrorMessage);
					}
				}

				notSyncSequenceBooks = AllocateComplianceDocuments(factory, complianceDocumentsToAllocate);
				factory.Save();
			}
			catch (ComplianceSequenceRelatedException ex) when
			(ex is FailedToFindComplianceSequenceException
			 || ex is AllocationComplianceSequenceFullException
			 || ex is AllocationComplianceSequenceBusyException
			)
			{
				errorMessages.Add(PrintException + System.Environment.NewLine +  ex.UserFriendlyMessage);
				errorMessage = JoinAndAttachLineNumber(errorMessages);
				return Array.Empty<ARComplianceDocumentHeader>();
			}

			if (notSyncSequenceBooks.Length > 0)
			{
				errorMessageForNotSync = GetNotSyncErrorMessage(notSyncSequenceBooks);
			}
			else if (complianceDocumentsToAllocate.Any(x => !x.IsAllocated && x.ComplianceBook == null))
			{
				errorMessages.Add(NoMatchingComplianceInvoiceBook);
			}

			errorMessage = JoinAndAttachLineNumber(errorMessages);

			return complianceDocumentsToAllocate.Where(x => x.IsAllocated);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static List<ZGuid> GetValidComplianceDocumentsForPrinting(IEnumerable<AccComplianceDocumentHeader> selectedDocumentHeaders, out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			var validComplianceDocuments = new List<ZGuid>();

			var complianceDocumentRePrintRestriction = AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.Value;

			if (selectedDocumentHeaders.All(x => x.IsVoided))
			{
				if (selectedDocumentHeaders.IsCountEqualTo(1))
				{
					errorMessage = TheOnlyOneVoided;
				}
				else
				{
					errorMessage = AllVoided;
				}

				return validComplianceDocuments;
			}

			if (selectedDocumentHeaders.All(x => x.ShouldPreventPrintDocument))
			{
				errorMessage = AllhaveOrganizationProblem;

				return validComplianceDocuments;
			}

			if (selectedDocumentHeaders.All(x => x.IsAllocated && x.ADH_XD_ComplianceBook.IsEmpty))
			{
				errorMessage = AllNoMatchingComplianceInvoiceBook;

				return validComplianceDocuments;
			}

			foreach (var complianceDocument in selectedDocumentHeaders)
			{
				if (complianceDocument.IsVoided || complianceDocument.ShouldPreventPrintDocument || (complianceDocument.IsAllocated && complianceDocument.ADH_XD_ComplianceBook.IsEmpty))
				{
					continue;
				}
				else if (!complianceDocument.IsAllocated)
				{
					validComplianceDocuments.Add(complianceDocument.PK);
				}
				else if (complianceDocument.ADH_PrintCount > 0 && (complianceDocument.ComplianceBook.XD_RollupBehaviourWhenMaxExceeded == ComplianceRollupBehaviourType.SinglePageSummarize || complianceDocument.ComplianceBook.XD_RollupBehaviourWhenMaxExceeded == ComplianceRollupBehaviourType.SinglePageReferAttached))
				{
					if (selectedDocumentHeaders.IsCountEqualTo(1))
					{
						errorMessage = Printed;
					}
				}
				else
				{
					var complianceBookDocumentMenuItemName = complianceDocument.ComplianceBook.MenuItem != null ? (ZString)((CargoWise.Integration.ICodeDescription)complianceDocument.ComplianceBook.MenuItem).Code : ZString.Empty;

					var rePrintRestrictedComplianceDocuments = new List<ComplianceDocumentRePrintRestriction>();

					for (int i = 0; i < complianceDocumentRePrintRestriction.Count; i++)
					{
						if (complianceDocumentRePrintRestriction[i].ComplianceDocumentMenu == complianceBookDocumentMenuItemName.ToUpper() &&
					complianceDocumentRePrintRestriction[i].OrganizationCategory == complianceDocument.OrgHeaderCategory &&
					complianceDocument.ADH_PrintCount > complianceDocumentRePrintRestriction[i].NumberOfReprintAllowed)
						{
							rePrintRestrictedComplianceDocuments.Add(complianceDocumentRePrintRestriction[i]);
						}
					}

					if (rePrintRestrictedComplianceDocuments.IsCountMoreThan(0))
					{
						if (selectedDocumentHeaders.IsCountEqualTo(1))
						{
							errorMessage = PrintCountExceeded;
						}
					}
					else
					{
						validComplianceDocuments.Add(complianceDocument.PK);
					}
				}
			}

			return validComplianceDocuments;
		}

		#endregion
	}
}
