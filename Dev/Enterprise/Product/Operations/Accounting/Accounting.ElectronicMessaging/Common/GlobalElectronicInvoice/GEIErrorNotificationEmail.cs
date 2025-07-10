using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIErrorNotificationEmail : EInvoicingTransactionErrorNotificationEmail
	{
		public GEIErrorNotificationEmail(EDIMessage ediMessage, IncorrectTransactionDetails incorrectTransaction) : base()
		{
			SubjectTransaction = Argument.NotNull(incorrectTransaction, nameof(incorrectTransaction));
			IncorrectTransactions = new[] { incorrectTransaction };
			EDIMessage = ediMessage;
		}

		public GEIErrorNotificationEmail(EDIMessage ediMessage, IncorrectTransactionDetails headerTransaction, IReadOnlyCollection<IncorrectTransactionDetails> relatedTransactions) : base()
		{
			// Constructor to support AccEInvoicingBatch, which has no module.
			// If a module for AccEInvoicingBatch is created, removing this will simplify code.
			SubjectTransaction = Argument.NotNull(headerTransaction, nameof(headerTransaction));
			if (relatedTransactions == null || relatedTransactions.Count == 0)
			{
				throw new ArgumentNullException(nameof(relatedTransactions), "relatedTransactions was null or empty collection");
			}
			IncorrectTransactions = new[] { headerTransaction }.Concat(relatedTransactions).ToArray();
			EDIMessage = ediMessage;
		}

		public GEIErrorNotificationEmail(EDIMessage ediMessage, IReadOnlyCollection<IncorrectTransactionDetails> incorrectTransactions) : base()
		{
			if (incorrectTransactions == null || incorrectTransactions.Count == 0)
			{
				throw new ArgumentNullException(nameof(incorrectTransactions), "incorrectTransactions was null or empty collection");
			}
			IncorrectTransactions = incorrectTransactions;
			EDIMessage = ediMessage;
		}

		readonly IncorrectTransactionDetails SubjectTransaction;
		readonly IReadOnlyCollection<IncorrectTransactionDetails> IncorrectTransactions;
		readonly EDIMessage EDIMessage;

		protected override string GetSubject() => Res.GetString("D44D5DE7-4F2F-4EA3-9A6D-159771F81A44", "E-Reporting error notification for {0}{1}", GetSubjectTransactionDetail(), GetSubjectCompanyCode());

		string GetSubjectTransactionDetail()
			=> !SubjectTransaction.IsEmpty      ? SubjectTransaction.BizoName            + " " + SubjectTransaction.UniqueIdentifier
			 : IncorrectTransactions.Count == 1 ? IncorrectTransactions.First().BizoName + " " + IncorrectTransactions.First().UniqueIdentifier
			 : Res.GetString("0694DF8E-852D-4C29-9DBB-8017484B9B54", "{0} items", IncorrectTransactions.Count.ToString());

		string GetSubjectCompanyCode()
			=> (!EDIMessage?.Company?.GC_Code.IsEmpty ?? false) ? $" [{EDIMessage.Company.GC_Code}]" : string.Empty;
		protected override string GetBody()
			=> IncorrectTransactions.Count == 1 ? GetBodyForSingleTransaction() : GetBodyForManyTransactions();

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		string GetBodyForSingleTransaction()
		{
			var incorrectTransaction = IncorrectTransactions.Single();
			var transactionNameLower = incorrectTransaction.BizoName.ToLowerInvariant();

			var messageBuilder = new ZStringBuilder();
			messageBuilder.Append("<html><body>");

			messageBuilder.Append(Res.GetString("EA23C101-B309-44E9-BEC6-C72E3A750678", "The following {0} was not successfully submitted to E-Reporting authority:", transactionNameLower));
			messageBuilder.Append((NoResString)"<br/><br/>");
			messageBuilder.Append(GetAHrefOrPlainTextOf(incorrectTransaction, includeBizoName: false));
			messageBuilder.Append((NoResString)"<br/>");

			if (!string.IsNullOrEmpty(incorrectTransaction.ModuleName))
			{
				messageBuilder.Append(Res.GetString("456CD5F6-59D5-41AE-830E-9CD77AE8ABC1", "If you wish to re-submit the {0}, please reset the status of each {1} to Queued in the {2} module.", transactionNameLower, transactionNameLower, incorrectTransaction.ModuleName));
				messageBuilder.Append((NoResString)"<br/><br/>");
			}
			else
			{
				messageBuilder.Append(Res.GetString("DC892AC8-34D5-4193-8B64-05F380161E09", "If you wish to re-submit the {0}, please reset the status of each item below to Queued in the appropriate module.", transactionNameLower));
				messageBuilder.Append((NoResString)"<br/><br/>");
			}
			AppendErrorMessages(messageBuilder, incorrectTransaction, indent: false);
			messageBuilder.Append((NoResString)"</br>");

			AppendEDIMessageLink(messageBuilder);

			messageBuilder.Append((NoResString)"</body></html>");
			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		string GetBodyForManyTransactions()
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.Append("<html><body>");

			messageBuilder.Append(Res.GetString("4EE04903-7107-434F-905F-038D0A3A6802", "The following items were not successfully submitted to E-Reporting authority:"));
			messageBuilder.Append((NoResString)"<br/><br/>");
			foreach (var t in IncorrectTransactions)
			{
				messageBuilder.Append(GetAHrefOrPlainTextOf(t, includeBizoName: true));
				AppendErrorMessages(messageBuilder, t, indent: true);
			}
			messageBuilder.Append((NoResString)"</br>");

			messageBuilder.Append(Res.GetString("72BA3303-2215-4103-9DEF-DCADDDC629FE", "If you wish to re-submit the items, please reset the status of each item to Queued in the appropriate module."));
			messageBuilder.Append((NoResString)"<br/><br/>");

			AppendEDIMessageLink(messageBuilder);

			messageBuilder.Append((NoResString)"</body></html>");
			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		string GetAHrefOrPlainTextOf(IncorrectTransactionDetails incorrectTransaction, bool includeBizoName)
		{
			var linkToTransaction = GetLinkToTransaction(incorrectTransaction.LedgerType, incorrectTransaction.TransactionType, incorrectTransaction.TransactionPK, incorrectTransaction.ParentTableCode);
			var bizoNameString = includeBizoName ? incorrectTransaction.BizoName + " " : string.Empty;
			if (!string.IsNullOrEmpty(linkToTransaction))
			{
				return $"<a href='{linkToTransaction}'>{bizoNameString}{incorrectTransaction.UniqueIdentifier}</a><br/>";
			}
			else
			{
				return $"<span style='text-decoration: underline;'>{bizoNameString}{incorrectTransaction.UniqueIdentifier}</span><br/>";
			}
		}

		static void AppendErrorMessages(ZStringBuilder messageBuilder, IncorrectTransactionDetails incorrectTransaction, bool indent)
		{
			if ((incorrectTransaction.Errors ?? Enumerable.Empty<ZString>()).Any())
			{
				var indentSpan = indent ? "&nbsp;&nbsp;" : string.Empty;
				var indentForDiv = indent ? (NoResString)"margin-left:30px;" : string.Empty;

				messageBuilder.Append((NoResString)"<b>");
				messageBuilder.Append(indentSpan);
				messageBuilder.Append(Res.GetString("2253A26D-DE91-49EC-9B7D-3AEADCDEA43F", "Error Details:"));
				messageBuilder.Append((NoResString)"</b>");
				messageBuilder.AppendFormat((NoResString)"<div style='width:1200px; {0}'>", indentForDiv);
				messageBuilder.Append(incorrectTransaction.GetAllErrorsAsString((NoResString)"<br/>"));
				messageBuilder.Append((NoResString)"</div>");
			}
		}

		void AppendEDIMessageLink(ZStringBuilder messageBuilder)
		{
			if (EDIMessage != null)
			{
				messageBuilder.Append((NoResString)"<b>");
				messageBuilder.Append(Res.GetString("90DFC2A6-9204-4204-A69F-C1C86F27A537", "Related EDI Message:"));
				messageBuilder.AppendFormat("</b><a href='{0}'>{1}</a><br/>", GetLinkToEDIMessage(), EDIMessage.EM_MessageNum);
				messageBuilder.Append((NoResString)"<b>");
				messageBuilder.Append(Res.GetString("5B004BF1-816C-4A7D-B5B6-EF48B783D69B", "Company:"));
				messageBuilder.Append((NoResString)"</b>");
				messageBuilder.Append(Res.GetString("A97A10C7-1CA4-4D92-9A3C-04F6D2FEF32F", "{0} - {1}", EDIMessage.Company.GC_Code, EDIMessage.Company.GC_Name));
				messageBuilder.Append((NoResString)"<br/>");
			}
		}

		string GetLinkToEDIMessage()
		{
			var link = ZString.Empty;
			if (EDIMessage != null)
			{
				var controllerID = ControllerIDs.Messaging.EDIMessage;
				if (controllerID != null)
				{
					link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, EDIMessage.PK.ToGuid());
				}
			}
			return link;
		}
	}
}
