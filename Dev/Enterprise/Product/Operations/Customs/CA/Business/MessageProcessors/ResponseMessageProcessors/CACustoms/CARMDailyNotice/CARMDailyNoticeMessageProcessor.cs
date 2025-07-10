using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMDailyNoticeMessageProcessor : CustomsMessageProcessor, IErrorNotification
	{
		public CARMDailyNoticeMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.CARMDailyNotice, MessageTypeList.Descriptions.CARMDailyNotice)
		{
		}

		ZGuid notificationGroup => StatementMessageProcessorHelper.AcknowledgementEmailGroup;

		protected override void SetMessageTypes(Enterprise.Messaging.Business.EDIMessage message)
		{
		}

		protected override ZGuid AcknowledgementEmailGroup => notificationGroup.IsEmpty ? CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.Value : notificationGroup;

		protected override ZString AcknowledgementEmailMode => notificationGroup.IsEmpty ? CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.Value : Core.Constants.EmailTo.NominatedGroup;

		protected override ZGuid ImpedimentEmailGroup => ZGuid.Empty;

		protected override ZString ImpedimentEmailMode => ZString.Empty;

		protected override ZGuid ErrorEmailGroup => ZGuid.Empty;

		protected override ZString ErrorEmailMode => ZString.Empty;

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage message)
		{
			var errorMessage = ZString.Empty;
			if (message is CARMDailyNoticeMessage dnMessage)
			{
				var messageSubType = dnMessage.EM_MessageSubType;
				if (messageSubType.IsEmpty)
				{
					messageSubType = dnMessage.EM_MessageText.ToUpper() switch
					{
						var text when text.Contains("ZCARMDNOTICEAH") => CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter,
						var text when text.Contains("ZCARMDNOTICECB") => CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker,
						_ => ZString.Empty
					};

					dnMessage.EM_MessageSubType = messageSubType;
				}

				using (DisposableEnvironment.ForBranch(dnMessage.EM_GB.ToGuid()))
				{
					if (messageSubType == CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter || messageSubType == CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker)
					{
						errorMessage = ProcessCARMDNoticeMessage(dnMessage);
					}
					else
					{
						throw new CriticalMessageProcessorException("Incorrect message version", "Daily Notice message has incorrect message subtype : " + dnMessage.EM_MessageSubType, message, this);
					}
				}
			}

			if (errorMessage.IsEmpty)
			{
				return EDIMessageStatusList.Codes.Received;
			}
			else
			{
				Logger.LogError(errorMessage);
				return EDIMessageStatusList.Codes.Failed;
			}
		}

		string IErrorNotification.MessageProcessorName => "CA CARM Daily Notice message processor.";

		void IErrorNotification.LogError(string errorMessage)
		{
			Logger.LogError(errorMessage);
		}

		void IErrorNotification.SendError(EmailDef email)
		{
		}

		void IErrorNotification.SendErrorToPostMaster(EmailDef email)
		{
			Env.OutgoingCustomsMailManager.CreateAndSave(email, Env.Registry.PostMasterGroup, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
		}

		ZString ProcessCARMDNoticeMessage(CARMDailyNoticeMessage dnMessage)
		{
			var wrapper = new CARMDailyNoticeMessageWrapper(dnMessage);
			var (statementNumber, errorMessage) = GetStatementNumber(wrapper.FileName);
			if (errorMessage.IsEmpty)
			{
				var statementType = wrapper.StatementType;
				var importerBusinessNumber = wrapper.ImporterBusinessNumber;
				var statementHeader = StatementMessageProcessorHelper.GetOrCreateCusStatementHeader(dnMessage.Factory, statementNumber, statementType, importerBusinessNumber, wrapper.Importer?.PK ?? ZGuid.Empty, false);
				dnMessage.EM_LinkedObject = statementHeader;

				statementHeader.B2_PrintDate = wrapper.StatementDate;
				statementHeader.B2_StatementAmount = wrapper.StatementAmount;
				statementHeader.B2_PaidAmount = wrapper.PaidAmount;
				statementHeader.B2_RefundAmount = wrapper.RefundAmount;
				var paymentDueDate = wrapper.DueDate;
				if (paymentDueDate.HasValue)
				{
					statementHeader.B2_DueDate = paymentDueDate.Value;
				}

				statementHeader.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, wrapper.MessageEN);
				statementHeader.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.FrenchMessageToRecipient, wrapper.MessageFR);

				var statementLines = statementHeader.StatementLines;
				if(wrapper.FileSeq == "E01")
				{
					statementLines.RemoveAndDeleteAll();
				}

				var minAccountingDate = ZDateTime.Empty;
				foreach (var lineGroup in wrapper.LineGroups)
				{
					foreach (var line in lineGroup.LineIteams)
					{
						var transactionNumber = line.TransactionNumber;
						var docType = line.DocType;
						if (statementHeader.B2_EntryFilerCode.IsEmpty)
						{
							statementHeader.B2_EntryFilerCode = transactionNumber.Left(5);
						}
						var statementLine = statementLines.GetStatementLineFor(transactionNumber, docType, ZString.Empty) ?? statementHeader.StatementLines.AddNew();
						var accountingDate = new ZDate(line.AccountingDate);
						statementLine.B3_ScheduledProcessDate = accountingDate;
						if (accountingDate < minAccountingDate || minAccountingDate.IsEmpty)
						{
							minAccountingDate = accountingDate;
						}

						SetValue(statementLine, CusStatementLineSchema.B3_EntryType, docType);
						SetValue(statementLine, CusStatementLineSchema.B3_AssociatedEntry, line.ReleatedDocumentNumber);
						SetValue(statementLine, CusStatementLineSchema.B3_EntryNum, transactionNumber);
						SetValue(statementLine, CusStatementLineSchema.B3_ImporterCustomsID, lineGroup.ImporterBusinessNumber);
						var releaseDate = line.ReleaseDate;
						if (!releaseDate.IsEmpty)
						{
							statementLine.B3_EntryDate = releaseDate;
						}

						IK84ReportAttachee k84ReportAttachee = null;
						var declaration = line.ReleatedDeclaration;
						if (declaration != null)
						{
							SetValue(statementLine, CusStatementLineSchema.B3_BrokerReference, line.JobNumber);
							if (declaration.IsB2Adjustments)
							{
								k84ReportAttachee = declaration;
							}
							else
							{
								var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration);
								if (entryHeader != null)
								{
									k84ReportAttachee = entryHeader;
								}
							}
							StatementMessageProcessorHelper.UpdateDates(statementHeader.B2_PrintDate, statementLine.B3_ScheduledProcessDate, true, k84ReportAttachee);
						}

						var paymentDueDateForLine = line.PaymentDueDate;
						if (paymentDueDateForLine.HasValue)
						{
							statementLine.B3_DueDate = paymentDueDateForLine.Value.Date;
						}

						var paymentParty = wrapper.IsBroker ? PaymentPartyCodeDescriptionList.Codes.Broker : PaymentPartyCodeDescriptionList.Codes.Importer;
						CreateStatementCharges(statementLine, line.Duties, CARMDailyNoticeChargeTypeList.Codes.Duties, paymentParty);
						CreateStatementCharges(statementLine, line.ExciseTax, CARMDailyNoticeChargeTypeList.Codes.ExciseTax, paymentParty);
						CreateStatementCharges(statementLine, line.ExciseDuties, CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, paymentParty);
						CreateStatementCharges(statementLine, line.SIMA, CARMDailyNoticeChargeTypeList.Codes.SIMA, paymentParty);
						CreateStatementCharges(statementLine, line.GST, CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, paymentParty);
						CreateStatementCharges(statementLine, line.HST, CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, paymentParty);
						CreateStatementCharges(statementLine, line.PST, CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, paymentParty);
						CreateStatementCharges(statementLine, line.Interest, CARMDailyNoticeChargeTypeList.Codes.Interest, paymentParty);
						CreateStatementCharges(statementLine, line.Penalties, CARMDailyNoticeChargeTypeList.Codes.Penalties, paymentParty);
						CreateStatementCharges(statementLine, line.Payments, CARMDailyNoticeChargeTypeList.Codes.Payments, paymentParty);
						CreateStatementCharges(statementLine, line.Others, CARMDailyNoticeChargeTypeList.Codes.Others, paymentParty);

						SetCARMExtentionProperties(statementLine, line);
					}

					var cusStatementLineGroup = StatementMessageProcessorHelper.CreateLineGroupIfNeed(statementHeader, lineGroup.ImporterBusinessNumber, lineGroup.RefundAmount, lineGroup.PaidAmount);

					cusStatementLineGroup?.UpdateImporter();
				}

				if (minAccountingDate.IsValid && (statementHeader.B2_ProcessDate.IsEmpty || minAccountingDate < statementHeader.B2_ProcessDate))
				{
					statementHeader.B2_ProcessDate = minAccountingDate;
				}
				SendAcknowledgementReport(null, GetEmailAndSetOnMessage(dnMessage, wrapper));
			}

			return errorMessage;
		}

		void SetCARMExtentionProperties(CusStatementLine statementLine, CARMDailyNoticeMessageLineWrapper line)
		{
			statementLine.CARMTransactionDescription = line.TransactionDescription;
			statementLine.CARMCADVersion = line.CADVERSION;
			statementLine.CARMSubmittedBy = line.SubmittedBy;
			statementLine.CARMStatus = line.AccountingStatus.HasValue ? (ZString)line.AccountingStatus.Value.ToString() : ZString.Empty;
			statementLine.CARMPort = line.Port;
			statementLine.CARMTotal = line.Totals;
		}

		void CreateStatementCharges(CusStatementLine statementLine, ZDecimal chargeAmount, ZString chargeType, ZString paymentParty)
		{
			if (!chargeAmount.IsEmpty)
			{
				var charge = statementLine.Charges.AddNew();
				charge.B4_ChargeType = chargeType;
				charge.B4_ChargeAmount = chargeAmount;
				charge.B4_PaymentParty = paymentParty;
			}
		}

		(ZString, ZString) GetStatementNumber(ZString fileName)
		{
			var statementNumber = ZString.Empty;
			var errorMessage = ZString.Empty;
			var regex = CusStatementHeader.IsCARMDailyNoticeRegex;
			if (regex.IsMatch(fileName))
			{
				var datetimeString = regex.Split(fileName).Last().Substring(2, 6);
				var businessNumber = regex.Matches(fileName)[0];
				statementNumber = businessNumber + datetimeString;
			}
			else
			{
				errorMessage = $"The file_name of message does not match the rule, it should start with 'DN-BN9(BN15)-'. file_name: {fileName}.";
			}

			return (statementNumber.ToUpper(), errorMessage);
		}

		void SetValue(BusinessObject obj, SchemaStringColumn schemaColumn, ZString value)
		{
			StatementMessageProcessorHelper.SetValue(obj, schemaColumn, value);
		}

		EmailDef GetEmailAndSetOnMessage(CARMDailyNoticeMessage dnMessage, CARMDailyNoticeMessageWrapper wrapper)
		{
			var emailBuilder = new EmailDefBuilder(GetSubject(wrapper), dnMessage.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacement(EmailDefBuilder.GetJobLink(dnMessage, wrapper.MessageSubTypeDescription));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " " + StatementMessageProcessorHelper.MessageSender.TrimEnd());
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetMessageInterpretation(wrapper));
			dnMessage.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		string GetSubject(CARMDailyNoticeMessageWrapper wrapper)
		{
			return Res.GetString("C8C529A4-E07B-4A92-9107-E08084E8B8CF", "{0} for {1}", wrapper.MessageSubTypeDescription, wrapper.StatementDate.ToShortDateString());
		}

		string GetMessageInterpretation(CARMDailyNoticeMessageWrapper wrapper)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder) { EnableHTMLEncoding = false };
			result.WriteRow(GetHeaderSection(wrapper));
			StatementMessageProcessorHelper.AddMessageInfosIfRequired(result, wrapper.MessageEN, wrapper.MessageFR);

			foreach (var lineGroupWrapper in wrapper.LineGroups)
			{
				result.WriteRow(GetImporterHeader(lineGroupWrapper));
				TableInterpretation.AddTableInterpretationIfRequired(result, lineGroupWrapper.LineIteams, lineGroupWrapper);
				result.WriteRow("<hr />");
			}
			TableInterpretation.AddTableInterpretationIfRequired(result, wrapper);

			return result.ToHtml();
		}

		string GetHeaderSection(CARMDailyNoticeMessageWrapper wrapper)
		{
			var dailyAccountingTable = new FieldValueTableInterpretation(false);
			dailyAccountingTable.Add(Res.GetString("53E94088-10CE-43FE-8F5F-58209B3DC347", "Legal Name"), wrapper.LegalName);
			dailyAccountingTable.Add(Res.GetString("5C88C674-7F22-4933-A41C-90E681F42235", "BN"), wrapper.BN9);
			dailyAccountingTable.Add(Res.GetString("B77DDBB1-15ED-4ACE-863E-684CD0908F4A", "RM Account Number"), wrapper.RMNumber);
			dailyAccountingTable.Add(Res.GetString("38BF10C6-D7E6-4BD8-9313-3164193DFDEF", "Statement Type"), wrapper.MessageSubTypeDescription);
			dailyAccountingTable.Add(Res.GetString("E6B85CE5-97FF-4C19-BCA8-4EC3E1E7A74D", "Statement Date"), InterpretationHelper.FormatDate(wrapper.StatementDate));
			dailyAccountingTable.Add(Res.GetString("C0F25FD5-5A18-478F-A0C7-278E1A5F63DE", "Payments"), InterpretationHelper.FormatAmount(wrapper.PaidAmount));
			dailyAccountingTable.Add(Res.GetString("AC9D31EC-F499-4B15-8926-9097378385E4", "Disbursements"), InterpretationHelper.FormatAmount(wrapper.RefundAmount));
			dailyAccountingTable.Add(Res.GetString("FB56CC73-D28E-45A1-AD3A-631C71EB18B7", "File Sequence"), wrapper.FileSeq);
			return dailyAccountingTable.ToHtml();
		}

		string GetImporterHeader(CARMDailyNoticeMessageLineGroupWrapper lineGroupWrapper)
		{
			var caption = new HtmlTableCreator(
				new[] {
					Res.GetString("D6DA366C-1540-4185-92E0-491A7E24CC3D", "Importer Details"),
					Res.GetString("A4C6E0DD-B7C7-4899-AC24-CBA0AB48554A", "Daily Notice")
				},
				TableInterpretation.Attributes.FullWidth
				)
			{ EnableHTMLEncoding = false };

			var importer = lineGroupWrapper.Importer;
			var importerDetailsTable = new HtmlTableCreator((IEnumerable<string>)null, TableInterpretation.Attributes.FullWidth);
			importerDetailsTable.WriteRow(new string[] { Res.GetString("FB623226-2E6D-45AE-BC07-F12F3C01D20F", "Importer Code"), importer?.OH_Code ?? ZString.Empty });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("81CE1B39-495E-4C67-ADF6-A4FA7F8CC848", "Importer Name"), importer == null ? lineGroupWrapper.LegalName : new ZString(lineGroupWrapper.LegalName + string.Format(CultureInfo.CurrentCulture, "\r\n({0})", importer.OH_FullNameTruncated)) });
			importerDetailsTable.WriteRow(new string[] { Res.GetString("D5481FA4-9872-439F-A328-7BFC6F0CD0D8", "Importer BN"), lineGroupWrapper.ImporterBusinessNumber });

			var tableAttribute = TableInterpretation.Attributes.FullWidth;
			tableAttribute.Add("height", "100%");
			var dailynoticeTable = new HtmlTableCreator((IEnumerable<string>)null, tableAttribute);
			dailynoticeTable.WriteRow(new string[] { Res.GetString("C7EAFEAB-AE36-4D0E-AAC8-3142980FCDCB", "Total Payments Received"), InterpretationHelper.FormatAmount(lineGroupWrapper.PaidAmount) });
			dailynoticeTable.WriteRow(new string[] { Res.GetString("39C65020-5065-428E-A17A-90153E2BAE80", "Refund"), InterpretationHelper.FormatAmount(lineGroupWrapper.RefundAmount) });

			caption.WriteRow(new string[] { importerDetailsTable.ToHtml(), dailynoticeTable.ToHtml() });
			var result = caption.ToHtml();
			return result;
		}
	}
}
