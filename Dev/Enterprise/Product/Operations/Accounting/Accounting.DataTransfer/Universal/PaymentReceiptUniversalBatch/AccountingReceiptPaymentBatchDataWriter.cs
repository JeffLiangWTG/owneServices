using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Export.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public abstract class AccountingReceiptPaymentBatchDataWriter : TopLevelDataObjectWriter<BusinessObject, UniversalTransactionBatch>
	{
		public AccountingReceiptPaymentBatchDataWriter(IDataWritingManager manager) : base(manager) { }

		protected override IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return sourceBO.GetUniversalDataContextManager();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is not a frontend facing string")]
		protected override void PopulateDataObject(BusinessObject sourceBO, UniversalTransactionBatch dataObject)
		{
			var validLedgers = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			if (sourceBO is ReceiptPaymentBase receiptPaymentBase && validLedgers.Contains(receiptPaymentBase.AH_Ledger.ToString()))
			{
				var accountingStrategy = IsOrgProxyOnlySelected()
										? AllFieldsAllowedAccountingTransactionWriterStrategy.Instance
										: DefaultAccountingTransactionWriterStrategy.Instance;
				var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(
					parentStrategy: writeManager.WriterStrategy,
					accountingStrategy: accountingStrategy,
					context: "UniversalTransactionBatch (ReceiptPaymentBatch)"
				);
				dataObject.SetWriterStrategy(writerStrategy);

				var transaction = GetUniversalTransaction(receiptPaymentBase, writerStrategy);
				dataObject.TransactionCollection.Add(transaction);

				var matchLinksRelatedWithSource = receiptPaymentBase.Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptPaymentBase.PK));
				var matchLinks = new List<TransactionMatchLink>();

				foreach (var matchLinkRelatedWithSource in matchLinksRelatedWithSource)
				{
					var matchLinksInSameMatchGroup = receiptPaymentBase.Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLinkRelatedWithSource.AP_MatchGroupNum)).ToList();
					matchLinksInSameMatchGroup = matchLinksInSameMatchGroup.Where(x => x.MatchingTransaction.AH_GC == matchLinkRelatedWithSource.MatchingTransaction.AH_GC).ToList();

					var matchShouldListInXml = matchLinksInSameMatchGroup.Where(x => x.AP_AH != receiptPaymentBase.PK);
					matchLinks.AddRange(matchShouldListInXml);
				}

				if (matchLinks.Count > 0)
				{
					transaction.MatchLineCollection = new List<MatchLine>();
				}

				foreach (TransactionMatchLink matchLink in matchLinks)
				{
					var matchTransaction = matchLink.MatchingTransaction;

					var matchline = new MatchLine();
					matchline.LinkedTransactionIDCollection = new List<LinkedTransactionID>();

					var linkedTransactionID = new LinkedTransactionID();

					linkedTransactionID.Type = GetLinkedTransactionType(matchTransaction.AH_TransactionType);
					var key = linkedTransactionID.Key = GetLinkedTransactionKey(matchTransaction.AH_Ledger, matchTransaction.AH_TransactionType, matchTransaction.AH_TransactionNum);
					matchline.LinkedTransactionIDCollection.Add(linkedTransactionID);
					matchline.OSPaidAmount = matchLink.OSAmount;
					matchline.MatchDate = new ZDate(matchLink.AP_MatchDate);
					matchline.MatchGroupNumber = matchLink.AP_MatchGroupNum;

					var matchTransactionInfo = dataObject.TransactionCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Any(y => y.Key == key));

					if (matchTransactionInfo == null)
					{
						matchTransactionInfo = GetUniversalTransaction(matchTransaction, writerStrategy);
						dataObject.TransactionCollection.Add(matchTransactionInfo);
					}

					matchline.OrganizationAddress = matchTransactionInfo.OrganizationAddress;

					transaction.MatchLineCollection.Add(matchline);
				}
			}
		}

		string GetLinkedTransactionType(ZString transactionType)
		{
			var result = ZString.Empty;

			switch (transactionType)
			{
				case TransactionTypes.Payment:
					result = "AccountingPayment";
					break;
				case TransactionTypes.Receipt:
					result = "AccountingReceipt";
					break;
				case TransactionTypes.Journal:
					result = "AccountingJournal";
					break;
				case TransactionTypes.Invoice:
					result = "AccountingInvoice";
					break;
				case TransactionTypes.CreditNote:
					result = "AccountingCreditNote";
					break;
				case TransactionTypes.AdjustmentNote:
					result = "AccountingAdjustmentNote";
					break;
				case TransactionTypes.Contra:
					result = "AccountingContra";
					break;
				case TransactionTypes.Transfer:
					result = "AccountingTransfer";
					break;
				case TransactionTypes.Overpayment:
					result = "AccountingOverpayment";
					break;
				case TransactionTypes.Discount:
					result = "AccountingDiscount";
					break;
				case TransactionTypes.ExchangeDifference:
					result = "AccountingExchangeDifferences";
					break;
				default:
					break;
			}

			return result;
		}

		string GetLinkedTransactionKey(ZString ledger, ZString transactionType, ZString transactionNum)
		{
			return string.Join(" ", ledger, transactionType, transactionNum);
		}

		string GetCheckBookCode(BusinessObject businessObject)
		{
			var result = ZString.Empty;

			if (businessObject is Payment payment && payment.AH_ReceiptType == ReceiptTypes.Cheque)
			{
				result = payment.ChequeBookBizO?.AK_Code ?? string.Empty;
			}

			return result;
		}

		UniversalTransaction GetUniversalTransaction(TransactionHeader transactionHeader, AccountingTransactionDataObjectWriterStrategy writerStrategy)
		{
			var dataAcess = new BatchExportDataAccess(((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOConnection, ((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOTransaction);
			var transactionExporter = new TransactionExporter(dataAcess);
			var transactionInfo = new UniversalTransaction(writerStrategy);
			transactionExporter.PopulateUniversalTransaction(writerStrategy, transactionHeader.Company.GC_Code.ToString(), transactionHeader.PK.ToGuid(), transactionInfo);

			var nameSpace = SchemaVersionManager.Current.Namespace;

			if (!nameSpace.IsValidUniversalXmlNamespace())
			{
				throw new XmlProcessingException(string.Format(CultureInfo.InvariantCulture, "Invalid namespace [{0}] - Please use a valid Universal Namespace.", nameSpace ?? "(null)"));
			}
			var dataSource = DataContextFactory.NewDataSource(nameSpace);
			dataSource.Type = GetLinkedTransactionType(transactionHeader.AH_TransactionType);
			dataSource.Key = GetLinkedTransactionKey(transactionHeader.AH_Ledger, transactionHeader.AH_TransactionType, transactionHeader.AH_TransactionNum);
			var context = DataContextFactory.New(nameSpace);
			var companyRow = dataAcess.LoadCompany(transactionHeader.Company.GC_Code);
			context.SetCompanyAndDataProviderDetails(companyRow);
			context.AddDataSource(dataSource);
			transactionInfo.DataContext = context;
			transactionInfo.CheckBookCode = GetCheckBookCode(transactionHeader);

			return transactionInfo;
		}
	}
}
