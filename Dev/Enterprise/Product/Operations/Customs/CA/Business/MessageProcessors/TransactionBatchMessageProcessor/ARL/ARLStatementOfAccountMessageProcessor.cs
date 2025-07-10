using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using DailySummaryTotal = Enterprise.Customs.CA.Business.ARLStatementOfAccountDocumentWrapper.DailySummaryTotal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class ARLStatementOfAccountMessageProcessor : ARLMessageProcessor
	{
		public ARLStatementOfAccountMessageProcessor(Enterprise.Messaging.Business.EDIMessage message, TransactionBatch transactionBatch, IXmlSessionTracker logger)
			: base(message, logger, ARLMessageTypes.Codes.StatementOfAccount)
		{
			wrapper = new ARLStatementOfAccountDocumentWrapper(transactionBatch, message.Factory);
		}

		readonly ARLStatementOfAccountDocumentWrapper wrapper;

		Dictionary<DailySummaryTotal, CusStatementLineGroup> dailySummaryTotalsMapping;

		protected override bool ProcessCore()
		{
			dailySummaryTotalsMapping = new Dictionary<DailySummaryTotal, CusStatementLineGroup>();
			var header = GetOrCreateStatementHeader();

			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = header.TableName;

			var legacyDataForDelete = new List<BusinessObject>();
			legacyDataForDelete.AddRange(header.LineGroupCollection.Cast<CusStatementLineGroup>());
			legacyDataForDelete.AddRange(header.StatementLines.Cast<CusStatementLine>());
			var processedLineGroups = new List<CusStatementLineGroup>();
			var lineGroups = wrapper.DailySummaryTotals.GroupBy(c => c.ImporterBusinessNumber);

			foreach (var lineGroup in lineGroups)
			{
				var dailySummaryTotals = lineGroup.ToList();
				var result = CreateLineGroupIfNeed(header, lineGroup.Key, dailySummaryTotals);
				legacyDataForDelete.Remove(result);
				if (!processedLineGroups.Contains(result))
				{
					processedLineGroups.Add(result);
				}
				dailySummaryTotals.ForEach(x => dailySummaryTotalsMapping.Add(x, result));
			}

			foreach (var dailyTotal in wrapper.DailySummaryTotals)
			{
				foreach (var transaction in dailyTotal.OtherTransactions)
				{
					var result = CreateOrUpdateLine(header, dailyTotal, TransactionBatchConstants.TransactionCategoryCodes.Other, transaction);
					legacyDataForDelete.Remove(result);
				}

				foreach (var transaction in dailyTotal.UnderReviewTransactions)
				{
					var result = CreateOrUpdateLine(header, dailyTotal, TransactionBatchConstants.TransactionCategoryCodes.UnderReview, transaction);
					legacyDataForDelete.Remove(result);
				}
			}

			message.EM_MessageSubType = messageType;
			message.SetSystemDefinedValue(Enterprise.Customs.CA.Business.EDIMessage.Schema.K84StatementDate, wrapper.StatementDate.ToZDateTime());
			message.SetSystemDefinedValue(Enterprise.Customs.CA.Business.EDIMessage.Schema.K84AccountingDate, wrapper.DueDate.ToZDateTime());
			message.EM_MessageOwner = ZString.Empty;

			foreach (var data in legacyDataForDelete)
			{
				logger.Log(Integration.LogType.Information, string.Format(CultureInfo.InvariantCulture, "Delete unused {0}.", data.HumanReadableName));
				data.Delete();
			}

			processedLineGroups.ForEach(x => x.UpdateImporter());
			return true;
		}

		protected override IKeysResult GetKeysCore() => KeysResult.ForceSequentialOrdering((nameof(ARLStatementOfAccountMessageProcessor), nameof(ARLStatementOfAccountMessageProcessor)));

		#region Static Methods

		CusStatementHeader GetOrCreateStatementHeader()
		{
			var statementType = GetStatementType();
			var importerBusinessNumber = GetImporterBusinessNumber();
			var statementNumber = GetStatementNumber(importerBusinessNumber);

			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
			query.AddToFilter(CusStatementHeaderSchema.B2_EntryFilerCode, wrapper.AccountSecurityCode);
			query.AddToFilter(CusStatementHeaderSchema.B2_PrintDate, wrapper.StatementDate);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
			query.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			query.OrderBy = CusStatementHeaderSchema.Constants.B2_SystemCreateTimeUtc + " DESC";

			var result = factory.LoadTop1<CusStatementHeader>(query);

			if (result == null)
			{
				result = factory.New<CusStatementHeader>();

				result.B2_IsMonthlyStatement = true;
				result.B2_OH_Importer = StatementMessageProcessorHelper.FindImporter(factory, importerBusinessNumber, statementType);

				SetValue(result, CusStatementHeaderSchema.B2_StatementType, statementType);
				SetValue(result, CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
				SetValue(result, CusStatementHeaderSchema.B2_ImporterCustomsID, importerBusinessNumber);
				SetValue(result, CusStatementHeaderSchema.B2_EntryFilerCode, wrapper.AccountSecurityCode);
				result.B2_PrintDate = wrapper.StatementDate;
			}

			result.B2_DueDate = wrapper.DueDate;
			result.B2_StatementAmount = wrapper.GrandTotal;

			result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, wrapper.MessageEN);
			result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.FrenchMessageToRecipient, wrapper.MessageFR);

			StatementMessageProcessorHelper.LocateDailyStatementHeadersIfNeed(result, statementType, importerBusinessNumber, wrapper.BillingDateFrom, wrapper.BillingDateTo);

			return result;
		}

		ZString GetImporterBusinessNumber()
		{
			var originalAccountNumber = wrapper.RMAccountNumber.ReplaceIgnoringCase(TransactionBatchConstants.ARLMessageProcessorConstants.RM, ZString.Empty);

			var accountNumber = originalAccountNumber.IsEmpty
				? string.Empty
				: string.Concat(TransactionBatchConstants.ARLMessageProcessorConstants.RM, originalAccountNumber);

			return new ZString(string.Concat(wrapper.ImporterBusinessNumber, accountNumber))
				.Left(ImporterBusinessNumberMaxLength)
				.ToUpper();
		}

		ZString GetStatementNumber(ZString importerBusinessNumber)
		{
			var statementNumber = new ZStringBuilder();
			statementNumber.Append(importerBusinessNumber);
			statementNumber.Append(wrapper.BillingDateFrom.Month.ToString("00", CultureInfo.InvariantCulture));
			statementNumber.Append(wrapper.BillingDateFrom.Year.ToString("0000", CultureInfo.InvariantCulture));

			if (!CACustomsDataRegistry.ShouldConsolidateSOA)
			{
				statementNumber.Append(TransactionBatchConstants.ARLMessageProcessorConstants.Dash);
				statementNumber.Append(wrapper.NumberOfSupportingDocuments?.ToString("000", CultureInfo.InvariantCulture) ?? "000");
			}

			return new ZString(statementNumber.ToString())
				.SubstringSafe(0, CusStatementHeaderSchema.B2_StatementNumber.MaxLength)
				.ToUpper();
		}

		ZString GetStatementType()
		{
			return factory.GetCachedValue<CusStatementHeaderTypes>().ContainsCode(wrapper.StatementType)
				? wrapper.StatementType
				: (ZString)CusStatementHeaderTypes.Codes.Unknown;
		}

		CusStatementLineGroup CreateLineGroupIfNeed(CusStatementHeader header, ZString importerBusinessNumber, IEnumerable<DailySummaryTotal> dailySummaryTotals)
		{
			CusStatementLineGroup lineGroup = null;

			if (!importerBusinessNumber.IsEmpty)
			{
				lineGroup = header.LineGroupCollection.FindOrCreate(importerBusinessNumber);

				UpdateFinancialDetails(lineGroup, dailySummaryTotals);
			}

			return lineGroup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdateFinancialDetails(CusStatementLineGroup lineGroup, IEnumerable<DailySummaryTotal> dailySummaryTotals)
		{
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.ArrearsInterest, dailySummaryTotals.Sum(c => c.ArrearsInterest));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InterestAmount, dailySummaryTotals.Sum(c => c.InterestAmount));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InstalmentLastAmount, dailySummaryTotals.Sum(c => c.InstalmentLastAmount));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InstalmentCurrentAmount, dailySummaryTotals.Sum(c => c.InstalmentCurrentAmount));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.OtherCharges, dailySummaryTotals.Sum(c => c.OtherCharges));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.Refund, dailySummaryTotals.Sum(c => c.Refund));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.PreviousMonthlyStatementTotal, dailySummaryTotals.Sum(c => c.PreviousMonthlyStatementTotal));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.PaymentReceivedSinceLastMonthlyStatement, dailySummaryTotals.Sum(c => c.PaymentReceivedSinceLastMonthlyStatement));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalCredits, dailySummaryTotals.Sum(c => c.TotalCredits));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPayableForBroker, dailySummaryTotals.Sum(c => c.TotalPayableForBroker));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPayableForImporter, dailySummaryTotals.Sum(c => c.TotalPayableForImporter));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TransactionTotal, dailySummaryTotals.Sum(c => c.Total));
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.UnpaidBalanceForward, dailySummaryTotals.Sum(c => c.UnpaidBalanceForward));
		}

		CusStatementLine CreateOrUpdateLine(CusStatementHeader header, DailySummaryTotal dailyTotal, string transactionCategory, DailySummaryTotal.OtherTransactionDetails transaction)
		{
			var entryNumber = transaction.DocumentNumber;
			var entryType = transaction.DocumentType;
			var entryStatus = transaction.Status;

			var line = header.StatementLines.GetStatementLineFor(entryNumber, entryType, entryStatus) ?? header.StatementLines.AddNew();

			line.B3_CustomsFeesTotal = transaction.Total;
			SetValue(line, CusStatementLineSchema.B3_Status, transactionCategory);
			SetValue(line, CusStatementLineSchema.B3_EntryStatus, entryStatus);
			SetValue(line, CusStatementLineSchema.B3_ImporterCustomsID, dailyTotal.ImporterBusinessNumber);
			SetValue(line, CusStatementLineSchema.B3_EntryType, entryType);
			SetValue(line, CusStatementLineSchema.B3_EntryNum, transaction.DocumentNumber);
			SetValue(line, CusStatementLineSchema.B3_EIIndicator, transaction.PaymentCategory);
			SetValue(line, CusStatementLineSchema.B3_AssociatedEntry, transaction.B3RelatedDocumentNumber);
			SetValue(line, CusStatementLineSchema.B3_CreditNote, transaction.CheckNumberOrPaymentRef);

			line.B3_ScheduledProcessDate = transaction.AccountingDate;
			line.B3_DueDate = transaction.PaymentDueDate;
			line.B3_CreditNoteDate = dailyTotal.CheckIssueDate;

			return line;
		}

		#endregion

		#region GetMessageInterpretation

		protected override string GetMessageInterpretation(BusinessObjectFactory factoryForEmail)
		{
			var tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };

			var dataTarget = wrapper.DataTargetKey;
			if (!dataTarget.IsEmpty)
			{
				var documentName = string.Format(CultureInfo.InvariantCulture, "Document Number: {0}", dataTarget);
				tableCreator.WriteRowWithFormatting(new CellWithFormatting(documentName, TableInterpretation.Attributes.AlignLeft, true));
			}

			tableCreator.WriteRow(GetHeaderSection(wrapper));
			StatementMessageProcessorHelper.AddMessageInfosIfRequired(tableCreator, wrapper.MessageEN, wrapper.MessageFR);

			foreach (DailySummaryTotal dailySummary in wrapper.DailySummaryTotals)
			{
				CusStatementLineGroup lineGroup = null;
				dailySummaryTotalsMapping?.TryGetValue(dailySummary, out lineGroup);
				tableCreator.WriteRow(GetImporterHeader(dailySummary, lineGroup?.Importer));
				TableInterpretation.AddTableInterpretationIfRequired(tableCreator, dailySummary.Transactions, dailySummary.GetImporterTotal());
				TableInterpretation.AddTableInterpretationIfRequired(tableCreator, dailySummary.OtherTransactions, dailySummary.GetOtherTransactionTotal());
				TableInterpretation.AddTableInterpretationIfRequired(tableCreator, dailySummary.UnderReviewTransactions, dailySummary.GetUnderReviewTransactionTotal());
				tableCreator.WriteRow("<hr />");
			}

			TableInterpretation.AddTableInterpretationIfRequired(tableCreator, wrapper.GetImporterGrandTotal());
			TableInterpretation.AddTableInterpretationIfRequired(tableCreator, wrapper.DailySummaryTotals, wrapper.GetImporterSummaryTotal());

			return tableCreator.ToHtml();
		}

		string GetHeaderSection(ARLStatementOfAccountDocumentWrapper header)
		{
			var dailyAccountingTable = new FieldValueTableInterpretation(false);
			dailyAccountingTable.Add(Res.GetString("b7ecd136-9118-43c7-a3aa-a6723ffc0278", "Legal Name"), header.ImporterLegalName);
			dailyAccountingTable.Add(Res.GetString("46fb25d1-5bac-4977-8319-61662e8700f0", "BN"), header.ImporterBusinessNumber);
			dailyAccountingTable.Add(Res.GetString("13d77767-7020-451f-953e-45e98f31f8fe", "RM Account Number"), header.RMAccountNumber);
			dailyAccountingTable.Add(Res.GetString("11b5625a-f6fb-4b3e-bb5f-858ac00a34a3", "Statement Type"), header.StatementTypeDescription);
			dailyAccountingTable.Add(Res.GetString("642ff1dd-1d4f-4f03-aa72-f67b407ab1cb", "Account Security Code"), header.AccountSecurityCode);
			dailyAccountingTable.Add(Res.GetString("6e595be5-a780-4450-9fef-036a9e7ab674", "Statement Date"), InterpretationHelper.FormatDate(header.StatementDate));
			dailyAccountingTable.Add(Res.GetString("421e6915-ffdc-4f20-b2ab-f5a870afb445", "Due Date"), InterpretationHelper.FormatDate(header.DueDate));
			dailyAccountingTable.Add(Res.GetString("266e90fd-c071-4e39-893c-7488b1ef0cbd", "Billing Period From"), InterpretationHelper.FormatDate(header.BillingDateFrom));
			dailyAccountingTable.Add(Res.GetString("3fe8d495-0dbb-471f-95ea-b3f3d5b38a79", "Billing Period To"), InterpretationHelper.FormatDate(header.BillingDateTo));
			dailyAccountingTable.Add(Res.GetString("a6d9d3a7-c584-45f8-bb4a-ebfe21b8191e", "Grand Total"), header.GrandTotal);
			dailyAccountingTable.Add(Res.GetString("88c9ed92-28e4-4410-a975-f7b6e4435742", "File Sequence"), header.NumberOfSupportingDocuments);
			return dailyAccountingTable.ToHtml();
		}

		string GetImporterHeader(DailySummaryTotal dailySummary, OrgHeader importer)
		{
			var caption = new HtmlTableCreator(new[] { Res.GetString("99e8a1ac-3ab4-464b-bb93-d25f0d5c360c", "Daily Summary Totals") }, TableInterpretation.Attributes.FullWidth);
			return caption.ToHtml() + GetImporterDetails(dailySummary, importer);
		}

		string GetImporterDetails(DailySummaryTotal dailySummary, OrgHeader importer)
		{
			var importerDetailsTable = new FieldValueTableInterpretation(false);
			importerDetailsTable.Add(Res.GetString("46c43224-70b9-4b38-81ec-9f5a2328ab33", "Importer Code"), importer?.OH_Code ?? dailySummary.ImporterCode);
			importerDetailsTable.Add(Res.GetString("6dd5a481-3e4f-499c-bdbe-ac03eef143e1", "Importer BN"), dailySummary.ImporterBusinessNumber);
			importerDetailsTable.Add(Res.GetString("389f81a8-5e7d-4e71-92c0-0f805af5d72e", "Importer Name"), importer == null ? dailySummary.ImporterLegalName : new ZString(dailySummary.ImporterLegalNameFromMessage + string.Format(CultureInfo.CurrentCulture, "\r\n({0})", importer.OH_FullNameTruncated)));
			importerDetailsTable.Add(Res.GetString("e62ac25b-5c61-4eaa-8425-107dc0245f01", "Check Issue Date"), InterpretationHelper.FormatDate(dailySummary.CheckIssueDate));

			return importerDetailsTable.ToHtml();
		}

		#endregion

		#region SendEmail

		protected override string GetSubject()
		{
			return Res.GetString("cc404e6c-48f8-4534-827e-06fa12d70754", "{0} for {1}", ARLMessageTypes.Descriptions.StatementOfAccount, wrapper.StatementDate.ToShortDateString());
		}

		ZGuid notificationGroup
		{
			get { return CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.Value; }
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

		#endregion
	}
}
