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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using DailyAccounting = Enterprise.Customs.CA.Business.ARLDailyNoticeDocumentWrapper.DailyAccounting;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class ARLDailyNoticeMessageProcessor : ARLMessageProcessor
	{
		public ARLDailyNoticeMessageProcessor(Enterprise.Messaging.Business.EDIMessage message, TransactionBatch transactionBatch, IXmlSessionTracker logger)
			: base(message, logger, ARLMessageTypes.Codes.DailyNotice)
		{
			wrapper = new ARLDailyNoticeDocumentWrapper(transactionBatch, message.Factory);
		}

		readonly ARLDailyNoticeDocumentWrapper wrapper;

		protected override IKeysResult GetKeysCore() => KeysResult.ForceSequentialOrdering((nameof(ARLDailyNoticeDocumentWrapper), nameof(ARLDailyNoticeDocumentWrapper)));

		#region Process
		Dictionary<DailyAccounting, CusStatementLineGroup> dailyAccountingsMapping;

		protected override bool ProcessCore()
		{
			dailyAccountingsMapping = new Dictionary<DailyAccounting, CusStatementLineGroup>();
			var processedLineGroups = new List<CusStatementLineGroup>();
			var accountSecurityCode = wrapper.GetAccountSecurityCode(logger);

			if (!accountSecurityCode.IsEmpty)
			{
				var header = GetOrCreateStatementHeader();

				message.EM_LinkUniqueID = header.PK;
				message.EM_LinkTable = header.TableName;

				var legacyDataForDelete = new List<BusinessObject>();
				legacyDataForDelete.AddRange(header.LineGroupCollection.Cast<CusStatementLineGroup>());
				legacyDataForDelete.AddRange(header.StatementLines.Cast<CusStatementLine>());
				var lineGroups = wrapper.PreviousDaysAccountings.GroupBy(c => c.ImporterBusinessNumber);

				foreach (var lineGroup in lineGroups)
				{
					var dailyAccountings = lineGroup.ToList();
					var result = StatementMessageProcessorHelper.CreateLineGroupIfNeed(header, lineGroup.Key, dailyAccountings.Sum(c => c.Refund), dailyAccountings.Sum(c => c.PaymentsReceived));
					legacyDataForDelete.Remove(result);
					if (!processedLineGroups.Contains(result))
					{
						processedLineGroups.Add(result);
					}
					dailyAccountings.ForEach(x => dailyAccountingsMapping.Add(x, result));
				}

				foreach (var dailyTotal in wrapper.PreviousDaysAccountings)
				{
					foreach (var transaction in dailyTotal.Transactions)
					{
						ProcessTransactionDetails(header, legacyDataForDelete, dailyTotal, TransactionBatchConstants.TransactionCategoryCodes.Normal, transaction, true);
						var declaration = transaction.Declaration ?? ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(message.Factory, transaction.DocumentNumber, JobMessageTypeList.Codes.Import);
						if (declaration != null)
						{
							declaration.Logs.AddNew(Events.MessageStatusChange, "DN Received");
						}
					}

					foreach (var transaction in dailyTotal.OtherTransactions)
					{
						ProcessTransactionDetails(header, legacyDataForDelete, dailyTotal, TransactionBatchConstants.TransactionCategoryCodes.Other, transaction, true);
					}

					foreach (var transaction in dailyTotal.UnderReviewTransactions)
					{
						ProcessTransactionDetails(header, legacyDataForDelete, dailyTotal, TransactionBatchConstants.TransactionCategoryCodes.UnderReview, transaction, false);
					}
				}

				foreach (var data in legacyDataForDelete)
				{
					logger.Log(Integration.LogType.Information, string.Format(CultureInfo.InvariantCulture, "Delete unused {0}.", data.HumanReadableName));
					data.Delete();
				}
			}

			message.EM_MessageSubType = messageType;
			message.SetSystemDefinedValue(Enterprise.Customs.CA.Business.EDIMessage.Schema.K84StatementDate, wrapper.StatementDate.ToZDateTime());
			message.SetSystemDefinedValue(Enterprise.Customs.CA.Business.EDIMessage.Schema.K84AccountingDate, wrapper.AccountingDate.ToZDateTime());
			message.EM_MessageOwner = string.Empty;

			processedLineGroups.ForEach(x => x.UpdateImporter());
			return true;
		}

		void ProcessTransactionDetails(CusStatementHeader header, List<BusinessObject> legacyDataForDelete, DailyAccounting dailyTotal, string transactionCategory, DailyAccounting.BaseTransactionDetails transaction, bool isNotUnderReviewTransaction)
		{
			var normalTransaction = transaction as DailyAccounting.TransactionDetails;
			var accountingDate = (normalTransaction?.AccountingDate).GetValueOrDefault();
			if (accountingDate.IsEmpty)
			{
				accountingDate = wrapper.AccountingDate;
			}

			var k84ReportAttachee = GetK84ReportAttachee(transaction.DocumentNumber);
			StatementMessageProcessorHelper.UpdateDates(wrapper.StatementDate, accountingDate, isNotUnderReviewTransaction, k84ReportAttachee);

			if (!transaction.IsImporterSecurityClientTotal)
			{
				var isOtherTransaction = transaction is DailyAccounting.OtherTransactionDetails;
				var documentType = transaction.DocumentType;

				var statementLine = CreateOrUpdateLine(header, dailyTotal, transactionCategory, transaction);

				var paymentPartyDic = transaction != null ? EntryChargeTypeList.GetPaymentParty(transaction.B3Field6Identifier) : null;
				var chargeAmounts = transaction?.GetRequiredChargeAmounts();

				if (paymentPartyDic != null && chargeAmounts != null)
				{
					foreach (var chargeAmount in chargeAmounts)
					{
						var chargeType = chargeAmount.ChargeType;
						var amount = chargeAmount.Amount;

						var paymentParty = ZString.Empty;

						if (normalTransaction != null)
						{
							paymentPartyDic.TryGetValue(chargeType, out paymentParty);
						}

						statementLine.Charges.UpdateLineChargeFor(chargeType, amount, paymentParty, isOtherTransaction && chargeType == TransactionBatchConstants.TransactionCategoryCodes.Other);
					}
				}

				if (normalTransaction != null)
				{
					var calculatedTotal = normalTransaction.CustomsDuties + normalTransaction.SIMA + normalTransaction.ExciseTax + normalTransaction.GST + normalTransaction.Others;
					var actualTotal = transaction.Total;

					if (calculatedTotal != actualTotal)
					{
						logger.LogBoth(Integration.LogType.Warning, Res.GetString("54fbe9c6-3d3c-4ccd-8e5b-0079da73be85", "ARL Total Amount not equal to calculated total, ARL for {0}, Total Amount = {1}, calculated total = {2}", accountingDate.ToString("yyyyMMMdd", CultureInfo.CurrentCulture), transaction.Total, calculatedTotal));
					}

					if (documentType == ARLDocumentTypeList.Codes.LA)
					{
						statementLine.B3_CustomsFeesTotal += actualTotal;
					}
					else
					{
						statementLine.B3_CustomsFeesTotal = actualTotal;
					}
				}

				if (isOtherTransaction)
				{
					statementLine.B3_CustomsFeesTotal += transaction.Total;
				}

				legacyDataForDelete.Remove(statementLine);
			}
		}

		IK84ReportAttachee GetK84ReportAttachee(ZString transactionNumber)
		{
			var k84Attachee = (IK84ReportAttachee)ImportLinkedObjectManager.GetCusEntryHeaderByTransactionNumber(message.Factory, transactionNumber, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			if (k84Attachee == null)
			{
				var b2Dec = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(message.Factory, transactionNumber, JobMessageTypeList.Codes.B2Adjustments);
				k84Attachee = b2Dec;
			}
			return k84Attachee;
		}

		#region Implement

		CusStatementHeader GetOrCreateStatementHeader()
		{
			var statementType = GetStatementType();

			var importerBusinessNumber = GetImporterBusinessNumber();
			var statementNumber = GetStatementNumber(importerBusinessNumber);
			var importerPK = StatementMessageProcessorHelper.FindImporter(factory, importerBusinessNumber, statementType);
			var result = StatementMessageProcessorHelper.GetOrCreateCusStatementHeader(factory, statementNumber, statementType, importerBusinessNumber, importerPK, false);

			result.B2_PrintDate = wrapper.StatementDate;
			result.B2_ProcessDate = wrapper.AccountingDate;
			result.B2_StatementAmount = wrapper.TotalTotal;
			result.B2_PaidAmount = wrapper.PaymentsReceived;
			result.B2_RefundAmount = wrapper.Refund;

			SetValue(result, CusStatementHeaderSchema.B2_EntryFilerCode, wrapper.AccountSecurityCode);

			result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, wrapper.MessageEN);
			result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.FrenchMessageToRecipient, wrapper.MessageFR);

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
			var numberOfSupportingDocuments = wrapper.NumberOfSupportingDocuments?.ToString("000", CultureInfo.InvariantCulture) ?? "000";

			return ZString.Join(new ZString[]
			{
				importerBusinessNumber,
				wrapper.AccountingDate.ToString("yyMMdd", CultureInfo.InvariantCulture),
				TransactionBatchConstants.ARLMessageProcessorConstants.Dash,
				numberOfSupportingDocuments
			}).SubstringSafe(0, CusStatementHeaderSchema.B2_StatementNumber.MaxLength)
			.ToUpper();
		}

		ZString GetStatementType()
		{
			return factory.GetCachedValue<CusStatementHeaderTypes>().ContainsCode(wrapper.StatementType)
				? wrapper.StatementType
				: (ZString)CusStatementHeaderTypes.Codes.Unknown;
		}

		CusStatementLine CreateOrUpdateLine(CusStatementHeader header, DailyAccounting dailyTotal, string transactionCategory, DailyAccounting.BaseTransactionDetails transaction)
		{
			var entryNumber = transaction.DocumentNumber;
			var entryType = transaction.DocumentType;

			var line = header.StatementLines.GetStatementLineFor(entryNumber, entryType, ZString.Empty) ?? header.StatementLines.AddNew();

			var normalTransaction = transaction as DailyAccounting.TransactionDetails;
			var otherTransaction = transaction as DailyAccounting.OtherTransactionDetails;

			SetValue(line, CusStatementLineSchema.B3_AssociatedEntry, transaction.B3RelatedDocumentNumber);
			SetValue(line, CusStatementLineSchema.B3_EntryType, entryType);
			SetValue(line, CusStatementLineSchema.B3_EntryProcessPort, normalTransaction != null ? normalTransaction.Port : ZString.Empty);
			SetValue(line, CusStatementLineSchema.B3_EntryNum, entryNumber);
			SetValue(line, CusStatementLineSchema.B3_ImporterCustomsID, dailyTotal.ImporterBusinessNumber);
			SetValue(line, CusStatementLineSchema.B3_Status, transactionCategory);
			SetValue(line, CusStatementLineSchema.B3_BrokerReference, transaction.Declaration?.JE_DeclarationReference ?? ZString.Empty);

			line.B3_EntryDate = normalTransaction != null ? normalTransaction.ReleaseDate : transaction.DocumentDate;
			line.B3_ScheduledProcessDate = normalTransaction != null ? normalTransaction.AccountingDate : ZDate.Empty;
			line.B3_DueDate = otherTransaction != null ? otherTransaction.PaymentDueDate : ZDate.Empty;

			return line;
		}

		#endregion

		#endregion

		#region GetMessageInterpretation

		protected override string GetMessageInterpretation(BusinessObjectFactory factoryForEmail)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };

			result.WriteRow(GetHeaderSection(wrapper));
			StatementMessageProcessorHelper.AddMessageInfosIfRequired(result, wrapper.MessageEN, wrapper.MessageFR);

			foreach (DailyAccounting importerDetails in wrapper.PreviousDaysAccountings)
			{
				CusStatementLineGroup lineGroup = null;
				dailyAccountingsMapping?.TryGetValue(importerDetails, out lineGroup);
				result.WriteRow(GetImporterHeader(importerDetails, lineGroup?.Importer));
				TableInterpretation.AddTableInterpretationIfRequired(result, importerDetails.Transactions, importerDetails.GetImporterTotal());
				TableInterpretation.AddTableInterpretationIfRequired(result, importerDetails.OtherTransactions, importerDetails.GetOtherTransactionTotal());
				TableInterpretation.AddTableInterpretationIfRequired(result, importerDetails.UnderReviewTransactions, importerDetails.GetUnderReviewTransactionTotal());
				result.WriteRow("<hr />");
			}

			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper.GetImporterGrandTotal());

			return result.ToHtml();
		}

		static string GetHeaderSection(ARLDailyNoticeDocumentWrapper header)
		{
			var dailyAccountingTable = new FieldValueTableInterpretation(false);
			dailyAccountingTable.Add(Res.GetString("50115B13-CA8D-4A35-B0F7-EF2D653B97F9", "Legal Name"), header.OrganizationLegalName);
			dailyAccountingTable.Add(Res.GetString("46fb25d1-5bac-4977-8319-61662e8700f0", "BN"), header.ImporterBusinessNumber);
			dailyAccountingTable.Add(Res.GetString("13d77767-7020-451f-953e-45e98f31f8fe", "RM Account Number"), header.RMAccountNumber);
			dailyAccountingTable.Add(Res.GetString("11b5625a-f6fb-4b3e-bb5f-858ac00a34a3", "Statement Type"), header.StatementTypeDescription);
			dailyAccountingTable.Add(Res.GetString("642ff1dd-1d4f-4f03-aa72-f67b407ab1cb", "Account Security Code"), header.AccountSecurityCode);
			dailyAccountingTable.Add(Res.GetString("6e595be5-a780-4450-9fef-036a9e7ab674", "Statement Date"), InterpretationHelper.FormatDate(header.StatementDate));
			dailyAccountingTable.Add(Res.GetString("3f8ab5c8-9541-4968-a0ce-3d2c4aef3825", "Accounting Date"), InterpretationHelper.FormatDate(header.AccountingDate));
			dailyAccountingTable.Add(Res.GetString("af41946f-3a8e-49c5-a74f-e8e7412669d1", "Payments Received"), InterpretationHelper.FormatAmount(header.PaymentsReceived));
			dailyAccountingTable.Add(Res.GetString("17f199d8-b208-4d3f-8adb-295a6a85433e", "Refund"), InterpretationHelper.FormatAmount(header.Refund));
			dailyAccountingTable.Add(Res.GetString("48c2529e-ba04-4afa-8a52-20f37f80277f", "File Sequence"), header.NumberOfSupportingDocuments);
			return dailyAccountingTable.ToHtml();
		}

		static string GetImporterHeader(DailyAccounting importerDetails, OrgHeader importer)
		{
			var caption = new HtmlTableCreator(
				new[] {
					Res.GetString("837e234e-de96-4e85-9acc-60c0ece634e8", "Importer Details"),
					Res.GetString("5BCC9917-40D5-4A89-A479-6FDF80060A00", "Daily Notice")
				},
				TableInterpretation.Attributes.FullWidth
				)
			{ EnableHTMLEncoding = false };

			var importerDetailsTable = new HtmlTableCreator((IEnumerable<string>)null, TableInterpretation.Attributes.FullWidth);
			importerDetailsTable.WriteRow(new string[] { Res.GetString("8d19b130-643b-450f-9d02-0bb1e4f80f02", "Importer Code"), importer?.OH_Code ?? importerDetails.ImporterCode });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("2895bdbe-0f90-4e65-9173-4306c9c0b6ae", "Importer Name"), importer == null ? importerDetails.ImporterLegalName : new ZString(importerDetails.ImporterLegalNameFromMessage + string.Format(CultureInfo.CurrentCulture, "\r\n({0})", importer.OH_FullNameTruncated)) });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("8a499d81-299a-449b-a8e3-60901ba6703b", "Importer BN"), importerDetails.ImporterBusinessNumber });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("e791f501-80f2-4733-9b7a-7d9777b2e2c9", "Importer Direct"), TableInterpretation.GetBooleanAsString(importerDetails.IsImporterDirectPayment) });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("6473290b-fb22-4f2d-85b7-c91b65812a76", "GST Direct"), TableInterpretation.GetBooleanAsString(importerDetails.IsGSTDirectPayment) });

			var tableAttribute = TableInterpretation.Attributes.FullWidth;
			tableAttribute.Add("height", "100%");
			var dailynoticeTable = new HtmlTableCreator((IEnumerable<string>)null, tableAttribute);
			dailynoticeTable.WriteRow(new string[] { Res.GetString("4920b741-b5b9-4f38-8285-64820d5bf784", "Total Payments Received"), InterpretationHelper.FormatAmount(importerDetails.PaymentsReceived) });
			dailynoticeTable.WriteRow(new string[] { Res.GetString("51fbfc17-d9fd-4877-a6b8-67a9f88d6f4d", "Refund"), InterpretationHelper.FormatAmount(importerDetails.Refund) });

			caption.WriteRow(new string[] { importerDetailsTable.ToHtml(), dailynoticeTable.ToHtml() });
			var result = caption.ToHtml();
			return result;
		}

		#endregion

		#region SendEmail

		protected override string GetSubject()
		{
			return Res.GetString("cc404e6c-48f8-4534-827e-06fa12d70754", "{0} for {1}", ARLMessageTypes.Descriptions.DailyNotice, wrapper.AccountingDate.ToShortDateString());
		}

		ZGuid notificationGroup
		{
			get { return StatementMessageProcessorHelper.AcknowledgementEmailGroup; }
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

	public static class OtherTransactionEntryType
	{
		public const string Other = "OT";
		public const string UnderReview = "UR";
	}
}
