using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIEmailNotificationCreator : EInvoicingEmailNotificationCreator
	{
		public GEIEmailNotificationCreator(EDIMessage ediMessage, InvoicingBase transaction, IEnumerable<ZString> errors, ILogger logger)
			: base(logger)
		{
			Argument.NotNull(transaction, nameof(transaction));

			EDIMessage = ediMessage;
			IncorrectTransactions = new[] { IncorrectTransactionDetails.FromBizo(transaction, errors) };
			Company = transaction.Company;
		}

		public GEIEmailNotificationCreator(EDIMessage ediMessage, AccComplianceDocumentHeader complianceDocumentHeader, IEnumerable<ZString> errors, ILogger logger)
			: base(logger)
		{
			Argument.NotNull(complianceDocumentHeader, nameof(complianceDocumentHeader));

			EDIMessage = ediMessage;
			IncorrectTransactions = new[] { IncorrectTransactionDetails.FromBizo(complianceDocumentHeader, errors) };
			Company = complianceDocumentHeader.Company;
		}

		public GEIEmailNotificationCreator(EDIMessage ediMessage, AccEInvoicingBatch batch, IEnumerable<ZString> errors, IEnumerable<AccTransactionHeader> relatedTransactions, ILogger logger)
			: base(logger)
		{
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(relatedTransactions, nameof(relatedTransactions));

			EDIMessage = ediMessage;
			IncorrectTransactionHeader = IncorrectTransactionDetails.FromBizo(batch, errors);
			IncorrectTransactions = relatedTransactions.Select(t => IncorrectTransactionDetails.FromBizo(t, Enumerable.Empty<ZString>())).ToArray();
			Company = batch.Company;
		}

		public GEIEmailNotificationCreator(EDIMessage ediMessage, IncorrectTransactionDetails incorrectSubjectTransaction, IReadOnlyCollection<IncorrectTransactionDetails> incorrectTransactionDetails, GlbCompany company, ILogger logger)
			: this(ediMessage, incorrectTransactionDetails, company, logger)
		{
			IncorrectTransactionHeader = Argument.NotNull(incorrectSubjectTransaction, nameof(incorrectSubjectTransaction));
		}

		public GEIEmailNotificationCreator(EDIMessage ediMessage, IReadOnlyCollection<IncorrectTransactionDetails> incorrectTransactionDetails, GlbCompany company, ILogger logger)
			: base(logger)
		{
			if (incorrectTransactionDetails == null || incorrectTransactionDetails.Count == 0)
			{
				throw new ArgumentNullException(nameof(incorrectTransactionDetails), "incorrectTransactionDetails was null or empty collection");
			}

			EDIMessage = ediMessage;
			IncorrectTransactions = incorrectTransactionDetails;
			Company = company;
		}

		readonly EDIMessage EDIMessage;
		readonly GlbCompany Company;
		readonly IncorrectTransactionDetails IncorrectTransactionHeader;
		readonly IReadOnlyCollection<IncorrectTransactionDetails> IncorrectTransactions;

		public override void SendEmail()
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Company.PK).AddToFilter(GlbBranchSchema.GB_IsActive, true));

			if (branch == null)
			{
				if (!IncorrectTransactionHeader.IsEmpty)
				{
					ServiceLogger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Email Notification was not sent for {0} '{1}' of company '{2}' due to missing company branch.", IncorrectTransactionHeader.BizoName, IncorrectTransactionHeader.UniqueIdentifier, Company.CompanyName));
				}
				else if (IncorrectTransactions.Count == 1)
				{
					ServiceLogger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Email Notification was not sent for {0} '{1}' of company '{2}' due to missing company branch.", IncorrectTransactions.First().BizoName, IncorrectTransactions.First().UniqueIdentifier, Company.CompanyName));
				}
				else
				{
					ServiceLogger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Email Notification was not sent for {0:N0} items of company '{1}' due to missing company branch.", IncorrectTransactions.Count, Company.CompanyName));
				}
				return;
			}

			using (SwitchCompanyContextTemporarilyIfRequires(branch.PK.ToGuid()))
			{
				var email = CreateEmailer();
				var result = email.Send();
				if (result == EmailSendResult.Successful)
				{
					ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Email Notification was sent successfully for {0}.", branch.CompanyName));
				}
				else
				{
					LogDefaultUnsuccessfulEmailSentResult(email, branch.CompanyName);
				}

				ServiceLogger.Log(LogType.Debug, "E-Reporting Email Notification task completed.");
			}
		}

		GEIErrorNotificationEmail CreateEmailer()
			=> IncorrectTransactionHeader.IsEmpty
				? new GEIErrorNotificationEmail(EDIMessage, IncorrectTransactions)
				: new GEIErrorNotificationEmail(EDIMessage, IncorrectTransactionHeader, IncorrectTransactions);
	}
}
