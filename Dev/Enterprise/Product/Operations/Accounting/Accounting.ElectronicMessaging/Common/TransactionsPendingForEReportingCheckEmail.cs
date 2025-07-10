using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class TransactionsPendingForEReportingCheckEmail : AccountingEmailDef
	{
		public TransactionsPendingForEReportingCheckEmail(string code, Guid recipientGuid, IEnumerable<(ZGuid transactionPK, ZString transactionType, ZString transactionNum)> transactions)
		{
			Argument.NotNullOrEmpty(code, nameof(code));
			Argument.NotNull(transactions, nameof(transactions));

			ContentType = EmailContentTypes.HTML;
			Subject = GetSubjectCore(code);
			Body = GetBodyCore(transactions);
			RecipientGuid = recipientGuid;
		}

		protected override GuidRegistryItem Recipient => null;

		protected override string GetBody()
		{
			return Body;
		}

		protected override string GetSubject()
		{
			return Subject;
		}

		protected override Guid GetRecipient()
		{
			return RecipientGuid;
		}

		protected override void SendCore()
		{
			Env.OutgoingMailManager.CreateAndSave(this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup));
		}

		protected override void CreateCore(ITransactionParticipant factory)
		{
			Env.OutgoingMailManager.Create(factory, this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup));
		}

		string GetSubjectCore(string code)
		{
			return $"Transactions pending for E-Reporting notification [{code}]";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		string GetBodyCore(IEnumerable<(ZGuid transactionPK, ZString transactionType, ZString transactionNum)> transactions)
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine(@"<html><body>");
			messageBuilder.AppendLine(@"<p>Following transactions are pending for E-Reporting:</p>");

			var sortingDictionary = new Dictionary<string, int>
			{
				{ TransactionTypes.Invoice, 0 },
				{ TransactionTypes.CreditNote, 1 },
				{ TransactionTypes.AdjustmentNote, 2 },
			};

			var sortedTransactions = transactions.OrderBy(x => sortingDictionary[x.transactionType]).ThenBy(x => x.transactionNum).ToList();

			foreach (var (transactionPK, transactionType, transactionNum) in sortedTransactions)
			{
				var transactionLink = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(EInvoicingControllerIdDecider.GetControllerIDRelatedToTransaction(LedgerTypes.AccountsReceivable, transactionType), transactionPK.ToGuid());
				messageBuilder.AppendLine($@"<p><a href='{transactionLink}'>Transaction AR {transactionType} {transactionNum}</a></p>");
			}
			messageBuilder.AppendLine(@"</body></html>");

			return messageBuilder.ToString();
		}

		Guid RecipientGuid { get; }
	}
}
