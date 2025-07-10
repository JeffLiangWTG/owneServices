//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.CA.Business.BatchProcessor;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;
	using Enterprise.ZArchitecture.Environment;
	using TransactionDetails = K84DailyReportDocumentWrapper.DailyAccounting.TransactionDetails;

	class K84ReportMessageProcessor : ImportResponseMessageProcessor
	{
		public K84ReportMessageProcessor(LoggingInformation logger)
			: base(logger, null, MessageTypeList.Codes.K84Report, MessageTypeList.Descriptions.K84Report)
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage ediMessage)
		{
			base.DoPreProcessingReturningStatus(ediMessage);
			var message = (K84Message)ediMessage;
			message.EM_MessageSubType = BatchProcessorUtilities.GetK84MessageSubType(message.Interchange.EI_HeaderText);
			messageSubType = message.EM_MessageSubType;
			SendAcknowledgementReport(null, GetK84ReportEmailAndSetOnMessage(message));
			return EDIMessage.Status.Received;
		}

		ZString messageSubType;

		static EmailDef GetK84ReportEmailAndSetOnMessage(K84Message message)
		{
			var emailBuilder = new EmailDefBuilder(GetSubject(message), message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacement(EmailDefBuilder.GetJobLink(message, message.EM_MessageSubTypeDescription));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " " + MessageSender.TrimEnd());
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetMessageInterpretation(message));
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		static string GetSubject(K84Message message)
		{
			var subject = Res.GetString("e0de3011-1b5b-49b1-b5bb-7eed55a5b9a1", "{0} for", message.EM_MessageSubTypeDescription) + " ";
			switch (message.EM_MessageSubType)
			{
				case K84ReportTypes.Codes.Daily:
					subject += new K84DailyReportDocumentWrapper(message).CurrentDate;
					break;
				case K84ReportTypes.Codes.Monthly:
					subject += new K84MonthlyReportDocumentWrapper(message).StatementDate;
					break;
				case K84ReportTypes.Codes.Overdue:
					subject += new OverdueReleaseNoticeDocumentWrapper(message).CurrentDate;
					break;
			}
			return subject;
		}

		static string GetMessageInterpretation(K84Message message)
		{
			switch (message.EM_MessageSubType)
			{
				case K84ReportTypes.Codes.Daily:
					return GetDailyMessageInterpretation(message);
				case K84ReportTypes.Codes.Monthly:
					return GetMonthlyMessageInterpretation(message);
				case K84ReportTypes.Codes.Overdue:
					return GetOverdueMessageInterpretation(message);
				default:
					return ZString.Empty;
			}
		}

		ZGuid notificationGroup
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				switch (messageSubType)
				{
					case K84ReportTypes.Codes.Daily:
					case K84ReportTypes.Codes.Monthly:
						result = CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.Value;
						break;
					case K84ReportTypes.Codes.Overdue:
						result = CACustomsDataRegistry.Instance.SendOverdueReportNotificationsToGroup.Value;
						break;
				}
				return result;
			}
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				ZGuid group = notificationGroup;
				return group.IsEmpty ? base.AcknowledgementEmailGroup : group;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				ZGuid group = notificationGroup;
				return group.IsEmpty ? base.AcknowledgementEmailMode.ToString() : Core.Constants.EmailTo.NominatedGroup;
			}
		}

		#region GetDailyMessageInterpretation

		static string GetDailyMessageInterpretation(K84Message message)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };
			var wrapper = new K84DailyReportDocumentWrapper(message);

			foreach (K84DailyReportDocumentWrapper.DailyAccounting dailyDetails in wrapper.PreviousDaysAccountings)
			{
				result.WriteRow(GetDailyAccountingHeader(dailyDetails, wrapper.AccountSecurityNumber));

				var transactionsByImporter = from TransactionDetails transaction in dailyDetails.Transactions
											 group transaction by transaction.ImporterCode into g
											 select g;

				foreach (var transactions in transactionsByImporter)
				{
					if (!transactions.Key.IsEmpty)
					{
						result.WriteRow(GetImporterHeader(transactions));
					}

					TableInterpretation.AddTableInterpretationIfRequired(result, transactions.Cast<BusinessObject>(), new TransactionDetails.ImporterTotal(transactions));
				}
			}

			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.DailyAccountingTotals);
			return result.ToHtml();
		}

		static string GetDailyAccountingHeader(K84DailyReportDocumentWrapper.DailyAccounting dailyTotal, ZString accountSecurityCode)
		{
			var dailyTotalTable = new FieldValueTableInterpretation(false);
			dailyTotalTable.Add(() => accountSecurityCode);
			dailyTotalTable.Add(() => dailyTotal.AccountingOffice);
			dailyTotalTable.Add(() => dailyTotal.AccountingDate);
			dailyTotalTable.Add(() => dailyTotal.StatementDate);
			return dailyTotalTable.ToHtml();
		}

		static string GetImporterHeader(IEnumerable<TransactionDetails> group)
		{
			var result = ZString.Empty;
			var first = group.First();
			var declaration = first.Declaration;
			if (declaration != null)
			{
				var table = new FieldValueTableInterpretation(false);
				table.Add(() => first.ImporterCode);
				table.Add(() => declaration.ImporterName);
				table.Add(() => first.IsImporterSecuritySetOnOrganization, TableInterpretation.GetBooleanAsString(first.IsImporterSecuritySetOnOrganization));
				table.Add(() => first.IsGSTDirectSetOnOrganization, TableInterpretation.GetBooleanAsString(first.IsGSTDirectSetOnOrganization));
				result = table.ToHtml();
			}
			return result;
		}

		#endregion

		#region GetMonthlyMessageInterpretation

		static ZString GetMonthlyMessageInterpretation(K84Message message)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };
			var wrapper = new K84MonthlyReportDocumentWrapper(message);
			result.WriteRow(GetMonthlyAccountingHeader(wrapper));
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.DailyAccountingTotals, wrapper.MonthlyUnAdjustedTotals);
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.TransactionCorrections, wrapper.TotalTransactionCorrections);
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.PeriodicInterimPaymentsByNotice);
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.PeriodicInterimPaymentsByTransaction);
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.LatePenaltiesAndInterestCharges, wrapper.TotalLateFilingAndInterestDetails);
			TableInterpretation.AddTableInterpretationIfRequired(result, (ITableInterpretation)wrapper.InterestRates);
			TableInterpretation.AddTableInterpretationIfRequired(result, (ITableInterpretation)wrapper.MonthlyAdjustedTotals);
			return result.ToHtml();
		}

		static string GetMonthlyAccountingHeader(K84MonthlyReportDocumentWrapper wrapper)
		{
			var dailyTotalTable = new FieldValueTableInterpretation(false);
			dailyTotalTable.Add(() => wrapper.AccountingOffice);
			dailyTotalTable.Add(() => wrapper.AccountSecurityNumber);
			dailyTotalTable.Add(() => wrapper.StatementDate);
			return dailyTotalTable.ToHtml();
		}

		#endregion

		#region GetOverdueMessageInterpretation

		static ZString GetOverdueMessageInterpretation(K84Message message)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };
			var wrapper = new OverdueReleaseNoticeDocumentWrapper(message);
			result.WriteRow(GetOverdueReportHeader(wrapper));
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.OverdueNotices, null, TableInterpretation.Attributes.AlignLeft);
			return result.ToHtml();
		}

		static string GetOverdueReportHeader(OverdueReleaseNoticeDocumentWrapper wrapper)
		{
			var dailyTotalTable = new FieldValueTableInterpretation(false);
			dailyTotalTable.Add(() => wrapper.AccountSecurityNumber);
			dailyTotalTable.Add(() => wrapper.CurrentDate);
			return dailyTotalTable.ToHtml();
		}

		#endregion
	}
}
