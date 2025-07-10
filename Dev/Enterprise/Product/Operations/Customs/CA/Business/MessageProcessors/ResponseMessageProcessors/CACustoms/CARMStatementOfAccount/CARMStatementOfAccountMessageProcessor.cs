using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMStatementOfAccountMessageProcessor : CustomsMessageProcessor
	{
		public CARMStatementOfAccountMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.CARMStatementOfAccount, MessageTypeList.Descriptions.CARMStatementOfAccount)
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
			if (message is CARMStatementOfAccountMessage soaMessage)
			{
				using (DisposableEnvironment.ForBranch(soaMessage.EM_GB.ToGuid()))
				{
					errorMessage = ProcessCARMSOAMessage(soaMessage);
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

		string GetStatementType(ZcarmsoaFileType fileType)
		{
			var result = ZString.Empty;
			switch (fileType)
			{
				case ZcarmsoaFileType.Le:
					result = CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry;
					break;
				case ZcarmsoaFileType.Pt:
					result = CARMStatementOfAccountStatementTypeList.ShortCodes.ProgramType;
					break;
				case ZcarmsoaFileType.Pa:
					result = CARMStatementOfAccountStatementTypeList.ShortCodes.ProgramAccount;
					break;
			}

			return result;
		}

		ZString ProcessCARMSOAMessage(CARMStatementOfAccountMessage soaMessage)
		{
			var wrapper = new CARMStatementOfAcccountMessageWrapper(soaMessage);
			var statementHeader = GetOrCreateStatementHeader(soaMessage.Factory, wrapper, out var errorMessage);
			if (errorMessage.IsEmpty)
			{
				var importerBusinessNumber = wrapper.ImporterBusinessNumber;
				soaMessage.EM_LinkedObject = statementHeader;

				var cusStatementLineGroup = StatementMessageProcessorHelper.CreateLineGroupIfNeed(statementHeader, importerBusinessNumber);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PreviousStatementBalance, wrapper.PreviousStatementBalance);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CorrectionsToPreviousStatementBalance, wrapper.CorrectionsToPreviousStatementBalance);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PaymentsReceivedAfterPreviousSoA, wrapper.PaymentsReceivedAfterPreviousSOA);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.Disbursements, wrapper.Disburesements);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.InterestAndPenaltiesSumTotal, wrapper.InterestAndPenaltiesSumTotal);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCharges, wrapper.CurrentPeriodCharges);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCredits, wrapper.CurrentPeriodCredit);
				cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentStatementBalance, wrapper.CurrentStatementBalance);

				var cusStatementLineGroupDIST = StatementMessageProcessorHelper.CreateLineGroupIfNeed(statementHeader, importerBusinessNumber + "_DIST");
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Duties, wrapper.Duties);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, wrapper.Excise);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, wrapper.ExciseDuties);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, wrapper.SIMA);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, wrapper.GST);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, wrapper.HST);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, wrapper.PST);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Interest, wrapper.Interest);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Penalties, wrapper.Penalties);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Payments, wrapper.Payments);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Others, wrapper.Others);
				cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Totals, wrapper.Totals);

				statementHeader.StatementLines.RemoveAndDeleteAll();
				foreach (var programAccount in wrapper.ProgramAccount)
				{
					foreach (var daySummary in programAccount.DaySummary)
					{
						var statementLine = statementHeader.StatementLines.AddNew();
						StatementMessageProcessorHelper.SetValue(statementLine, CusStatementLineSchema.B3_ImporterCustomsID, programAccount.Acccount);
						statementLine.B3_DueDate = daySummary.ReleaseDate;
						statementLine.B3_ScheduledProcessDate = daySummary.AccountingDate.Date;

						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Duties, CARMDailyNoticeChargeTypeList.Codes.Duties, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Excise, CARMDailyNoticeChargeTypeList.Codes.ExciseTax, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.ExciseDuties, CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.SIMA, CARMDailyNoticeChargeTypeList.Codes.SIMA, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.GST, CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.HST, CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.PST, CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Interest, CARMDailyNoticeChargeTypeList.Codes.Interest, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Penalties, CARMDailyNoticeChargeTypeList.Codes.Penalties, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Payments, CARMDailyNoticeChargeTypeList.Codes.Payments, ZString.Empty);
						StatementMessageProcessorHelper.CreateStatementCharges(statementLine, daySummary.Others, CARMDailyNoticeChargeTypeList.Codes.Others, ZString.Empty);
					}
				}

				SendAcknowledgementReport(null, GetEmailAndSetOnMessage(soaMessage, wrapper));
			}

			return errorMessage;
		}

		CusStatementHeader GetOrCreateStatementHeader(BusinessObjectFactory factory, CARMStatementOfAcccountMessageWrapper wrapper, out ZString errorMessage)
		{
			CusStatementHeader result = null;
			(var statementNumber, errorMessage) = GetStatementNumber(wrapper.FileName);
			if (errorMessage.IsEmpty)
			{
				var statementType = GetStatementType(wrapper.FileType);
				var importerBusinessNumber = wrapper.ImporterBusinessNumber;
				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
				query.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
				query.OrderBy = CusStatementHeaderSchema.Constants.B2_SystemCreateTimeUtc + " DESC";

				result = factory.LoadTop1<CusStatementHeader>(query);

				if (result == null)
				{
					result = factory.New<CusStatementHeader>();
					result.B2_IsMonthlyStatement = true;
					result.B2_OH_Importer = StatementMessageProcessorHelper.FindImporter(factory, importerBusinessNumber, statementType);
					result.B2_EntryFilerCode = TransactionBatchExtension.GetAccountSecurityCode(result.Importer, null);

					StatementMessageProcessorHelper.SetValue(result, CusStatementHeaderSchema.B2_StatementType, statementType);
					StatementMessageProcessorHelper.SetValue(result, CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
					StatementMessageProcessorHelper.SetValue(result, CusStatementHeaderSchema.B2_ImporterCustomsID, importerBusinessNumber);
				}
				result.B2_DueDate = wrapper.DueDate;
				result.B2_StatementAmount = wrapper.StatementAmount;
				result.B2_PeriodStartDate = wrapper.PeriodStartDate.Date;
				result.B2_PeriodEndDate = wrapper.PeriodEndDate.Date;
				result.B2_PrintDate = wrapper.StatementDate;

				result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.EnglishMessageToRecipient, wrapper.MessageEN);
				result.CreateOrUpdateCustomNote(StatementMessageProcessorHelper.FrenchMessageToRecipient, wrapper.MessageFR);
				StatementMessageProcessorHelper.LocateDailyStatementHeadersIfNeed(result, statementType, importerBusinessNumber, wrapper.PeriodStartDate, wrapper.PeriodEndDate);
			}
			return result;
		}

		EmailDef GetEmailAndSetOnMessage(CARMStatementOfAccountMessage soaMessage, CARMStatementOfAcccountMessageWrapper wrapper)
		{
			var emailBuilder = new EmailDefBuilder(GetSubject(wrapper), soaMessage.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacement(EmailDefBuilder.GetJobLink(soaMessage, wrapper.MessageSubTypeDescription));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " " + StatementMessageProcessorHelper.MessageSender.TrimEnd());
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetMessageInterpretation(wrapper));
			soaMessage.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		string GetSubject(CARMStatementOfAcccountMessageWrapper wrapper)
		{
			return Res.GetString("B332C43D-8E3E-47F8-B83E-980DBAEB922D", "{0} for {1}", wrapper.MessageSubTypeDescription, wrapper.StatementDate.ToShortDateString());
		}

		string GetMessageInterpretation(CARMStatementOfAcccountMessageWrapper wrapper)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
			result.WriteRow(GetHeaderSection(wrapper));
			StatementMessageProcessorHelper.AddMessageInfosIfRequired(result, wrapper.MessageEN, wrapper.MessageFR);
			result.WriteRow(GetSummarySection(wrapper));

			foreach (var programAccount in wrapper.ProgramAccount)
			{
				TableInterpretation.AddTableInterpretationIfRequired(result, programAccount.DaySummary, programAccount);
				result.WriteRow("<hr />");
			}

			return result.ToHtml();
		}

		string GetHeaderSection(CARMStatementOfAcccountMessageWrapper wrapper)
		{
			var headerTable = new FieldValueTableInterpretation(false);
			headerTable.Add(Res.GetString("4D9AA102-BD8D-49D5-81A2-D81C43396492", "File Sequence"), wrapper.FileSeq);
			headerTable.Add(Res.GetString("5AE9070B-7F1A-4905-BC0A-24FFB72C2235", "File Type"), wrapper.FileType);
			headerTable.Add(Res.GetString("418EE55C-B188-41DA-8680-64CF50C03C1A", "File Name"), wrapper.FileName);
			headerTable.Add(Res.GetString("A1B035D4-CD66-4135-8EAB-F1996B5F92F8", "Message Type"), wrapper.MessageSubTypeDescription);
			headerTable.Add(Res.GetString("0BAE9B1D-DEC8-4561-855B-20569B7B185E", "Period Start Date"), InterpretationHelper.FormatDate(wrapper.PeriodStartDate));
			headerTable.Add(Res.GetString("04248597-DC4F-4E94-8F97-FE5FDBD10716", "Period End Date"), InterpretationHelper.FormatDate(wrapper.PeriodEndDate));
			headerTable.Add(Res.GetString("AA0FE7C7-07C6-4DF3-AE6D-0AB0943FD526", "Statement Date"), InterpretationHelper.FormatDate(wrapper.StatementDate));
			headerTable.Add(Res.GetString("DB67145C-3B95-4F2F-A648-93534FB92D60", "Due Date"), InterpretationHelper.FormatDate(wrapper.DueDate));
			headerTable.Add(Res.GetString("8ED6DD10-3430-4D6F-A72B-2C1A96A12737", "Amount"), InterpretationHelper.FormatAmount(wrapper.StatementAmount));
			headerTable.Add(Res.GetString("29E90457-9BDA-481E-B403-F9C76A06B1C8", "Importer Business Number"), wrapper.ImporterBusinessNumber);
			return headerTable.ToHtml();
		}

		string GetSummarySection(CARMStatementOfAcccountMessageWrapper wrapper)
		{
			var caption = new HtmlTableCreator(
				new[] {
					Res.GetString("D3798552-ADBD-4798-9480-F7CF092679D9", "Summary"),
				},
				TableInterpretation.Attributes.FullWidth
				)
			{ EnableHTMLEncoding = false };

			var summaryTable = new FieldValueTableInterpretation(false);
			summaryTable.Add(Res.GetString("980FD919-8F86-4385-BB6A-27A6DFE0B743", "Previous Statement Balance"), wrapper.PreviousStatementBalance);
			summaryTable.Add(Res.GetString("9C0925B3-5945-42EC-A0F5-43F1A56A32E7", "Corrections To Previous Statement Balance"), wrapper.CorrectionsToPreviousStatementBalance);
			summaryTable.Add(Res.GetString("FA24B624-5DF7-4720-958D-F5AA81FA3543", "Payments Received After Previous SOA"), wrapper.PaymentsReceivedAfterPreviousSOA);
			summaryTable.Add(Res.GetString("376B13BA-ABEB-432C-9423-1BF7F255D463", "Disbursements"), wrapper.Disburesements);
			caption.WriteRow(summaryTable.ToHtml());

			summaryTable = new FieldValueTableInterpretation(false);
			summaryTable.Add(Res.GetString("D44F9152-A693-4232-B1E9-9B2D4C0DEB50", "Interest And Penalties Sum Total"), wrapper.InterestAndPenaltiesSumTotal);
			summaryTable.Add(Res.GetString("CBFEA87A-52D9-409A-8728-F0213E104006", "Current Period Charges"), wrapper.CurrentPeriodCharges);
			summaryTable.Add(Res.GetString("0FDFD9E9-736C-4101-8973-FA9F037BCF88", "Current Period Credit"), wrapper.CurrentPeriodCredit);
			summaryTable.Add(Res.GetString("5BA9A6C8-6CD4-4A54-AD82-8864997D92B7", "Current Statement Balance"), wrapper.CurrentStatementBalance);
			caption.WriteRow(summaryTable.ToHtml());

			TableInterpretation.AddTableInterpretationIfRequired(caption, wrapper);
			var result = caption.ToHtml();
			return result;
		}

		(ZString, ZString) GetStatementNumber(ZString fileName)
		{
			var statementNumber = ZString.Empty;
			var errorMessage = ZString.Empty;
			var regex = CusStatementHeader.IsCARMStatementOfAccountRegex;
			if (regex.IsMatch(fileName))
			{
				var datetimeString = new ZString(regex.Split(fileName).Last()).SubstringSafe(0, 14);
				var businessNumber = regex.Matches(fileName)[0];
				statementNumber = businessNumber.ToString().Replace("SOA-", "") + datetimeString;
			}
			else
			{
				errorMessage = $"The file_name of message does not match the rule, it should start with 'SOA-BN9-'. file_name: {fileName}.";
			}

			return (statementNumber.ToUpper(), errorMessage);
		}
	}
}
