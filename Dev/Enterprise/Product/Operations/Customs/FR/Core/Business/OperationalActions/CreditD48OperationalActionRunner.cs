using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessagesWrappers.COD;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;
using Enterprise.Customs.FR.Messaging.MessageBuilders.COD;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using TransactionTypes = Enterprise.Customs.FR.Messaging.MessageBuilders.TransactionTypes;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditD48OperationalActionRunner
	{
		public CreditD48OperationalActionRunner(IOperationalActionSectionLog log, IEnumerable<JobDeclaration> declarations)
		{
			logger = new OperationalActionSectionLogWrapper(log);
			targets = declarations;
		}

		public void CreditD48(FrDeclarationCreditD48Applicator applicator)
		{
			var currencyConverter = CurrencyConverter.New(applicator.Factory);
			logger.LogFormat(LogType.Information, (NoResString)"Searching for Supporting Documents with Code = {0} and Reference = {1}", applicator.D48DocumentCode, applicator.ReferenceNumber);

			var entryHeaders = targets.SelectMany(x => x.ActiveEntryHeaders).Cast<CusEntryHeader>();
			foreach (var entryHeader in entryHeaders)
			{
				var groupedSupportingDocs = entryHeader.MergedLines
						.SelectMany(x => x.SupportingDocumentsToCustoms.Select(y => new { EntryLine = x, SupportingDoc = y })).Where(x =>
						{
							var docWrapper = new SupportingDocumentWrapper(x.SupportingDoc as SupportingDocument, x.EntryLine);
							var header = x.EntryLine.Header;
							return header.IsVALOrBAEOrComplete && docWrapper.D48Amount > 0m && docWrapper.RefNumber.EqualsIgnoringCase(applicator.ReferenceNumber) && docWrapper.Code.Equals(applicator.D48DocumentCode);
						})
						.GroupBy(x => new { EntryLinePK = x.EntryLine.PK, EntryLine = x.EntryLine });

				int entryLineCount = groupedSupportingDocs.Count();

				if (entryLineCount > 0)
				{
					logger.LogFormat(LogType.Information, (NoResString)"{0} entry lines in entry header {1} found that are eligible for crediting transactions for D48", entryLineCount, entryHeader.CH_BGMReference);

					var articles = new List<IArticle>();
					foreach (var entryLineGroup in groupedSupportingDocs)
					{
						var total = Money.Empty;
						var entryLine = entryLineGroup.Key.EntryLine;
						foreach (var doc in entryLineGroup)
						{
							total = currencyConverter.Add(total, new Money(doc.SupportingDoc.CSI_Value, doc.SupportingDoc.Currency ?? entryLine.InvoiceCurrency));
						}

						var declaration = entryHeader.Declaration;
						var guarantee = declaration.CustomsGuarantee;

						if (guarantee != null)
						{
							try
							{
								if (guarantee.Mutex.Lock())
								{
									var permitRecord = new Customs.Business.PermitRecord
									{
										PermitHeader = guarantee,
										Value = total.Amount,
										Quantity = 0,
										Procedure = CusEntryLine.Schema.D48
									};

									var permitComment = string.Format(CultureInfo.InvariantCulture, (NoResString)"D48 for doc {0}/{1} for entry line {2}", applicator.D48DocumentCode, applicator.ReferenceNumber, entryLine.CL_LineNumber);
									guarantee.AddTransaction(Customs.Business.PermitHelper.GetPermitReferenceForEntry(entryHeader), permitComment, "", permitRecord.Procedure, permitRecord.Value, permitRecord.Quantity, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, Customs.Business.PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader));

									entryLine.Header.Declaration.Cast<JobDeclaration>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Where(x => x.CSI_Code == applicator.D48DocumentCode && x.CSI_ReferenceNumber.EqualsIgnoringCase(applicator.ReferenceNumber) && x.CSI_Quantity3 > 0).ToList().ForEach(x => x.CSI_Quantity3 = 0);
									entryLine.Header.InvoiceHeaders.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Where(x => x.CSI_Code == applicator.D48DocumentCode && x.CSI_ReferenceNumber.EqualsIgnoringCase(applicator.ReferenceNumber) && x.CSI_Quantity3 > 0).ToList().ForEach(x => x.CSI_Quantity3 = 0);
									entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Where(x => x.CSI_Code == applicator.D48DocumentCode && x.CSI_ReferenceNumber.EqualsIgnoringCase(applicator.ReferenceNumber) && x.CSI_Quantity3 > 0).ToList().ForEach(x => x.CSI_Quantity3 = 0);

									logger.LogFormat(LogType.Information, (NoResString)"Declaration {0}, entry {1}, entry line {2}, credit {3} to guarantee {4}"
										, declaration.JE_DeclarationReference, entryHeader.CH_BGMReference, entryLine.CL_LineNumber, total, guarantee.CPH_Number);
								}
							}
							finally
							{
								var mutex = guarantee.Mutex;
								if (mutex.HasLock)
								{
									mutex.Unlock();
								}
							}
						}

						var docAapurer = new DocAapurerWrapper(applicator);
						var article = new MessagesWrappers.COD.ArticleWrapper(entryLine, new List<IDocAapurer>() { docAapurer });
						articles.Add(article);
					}

					var message = CreateCODMessage(entryHeader, new CODWrapper(entryHeader, "1", articles, null));
					logger.LogFormat(LogType.Information, (NoResString)"Create COD message {0} in entry header {1} successfully", message.EM_MessageNum, entryHeader.CH_BGMReference);
				}
				else
				{
					logger.LogFormat(LogType.Information, (NoResString)"There were no entry lines found in entry header {0}.", entryHeader.CH_BGMReference);
				}
			}
		}

		EDIMessage CreateCODMessage(CusEntryHeader entryHeader, ICOD codWrapper)
		{
			var messageText = new CODSendMessageBuilder(codWrapper, new EU.Business.ErrorCollector(), TransactionTypes.Original).GetMessage();

			var factory = entryHeader.Factory;
			var ediMessage = factory.New<CODSendMessage>();
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.MessageNumberStrategy = new FRMessageNumberStrategy(factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			ediMessage.EM_LinkedObject = entryHeader;

			entryHeader.Logs.AddNew(Events.CustomsGuaranteeUpdated, string.Format("D48 | {0} Released", entryHeader.Declaration.CustomsGuarantee?.CPH_Number ?? ZString.Empty), ZDateTimeOffset.Now);
			factory.Save();

			return ediMessage;
		}

		readonly ICommonLogger logger;
		readonly IEnumerable<JobDeclaration> targets;
	}
}
