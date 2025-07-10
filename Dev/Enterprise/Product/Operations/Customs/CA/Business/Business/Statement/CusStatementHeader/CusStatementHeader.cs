using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[SingleObjectAroundARow]
	[UniversalDataContext(DataContextType.CAAccountsReceivableLedger)]
	[CodeProperty(Schema.B2_StatementNumber), DescriptionProperty(Schema.B2_StatementNumber)]
	public class CusStatementHeader : BaseCusStatementHeader
		, Integration.Customs.CA.ICusStatementHeader
		, IEDocsProvider
		, IWorkflowProviderTemplateCriteria
	{
		public CusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DefaultPeriodYearAndPeriodMonth();
		}

		public new class Schema : AutoCusStatementHeader.Schema
		{
			public const string B2_TotalCustomsDuties = "B2_TotalCustomsDuties";
			public const string B2_TotalSIMA = "B2_TotalSIMA";
			public const string B2_TotalExciseTax = "B2_TotalExciseTax";
			public const string B2_TotalGST = "B2_TotalGST";
			public const string B2_TotalOthers = "B2_TotalOthers";
			public const string B2_TotalInterests = "B2_TotalInterests";
		}

		public ZString ImporterName
		{
			get
			{
				if (importerName == null)
				{
					importerName = new CachedProperty<ZString>(Factory, () =>
					{
						return Importer?.OH_FullNameTruncated ?? ZString.Empty;
					});
				}
				return importerName.Value;
			}
		}
		CachedProperty<ZString> importerName;

		[ReadOnly(true)]
		public ZString Period
		{
			get
			{
				var result = ZString.Empty;
				var periodEnd = B2_PeriodEndDate;
				if (IsCSARSF && periodEnd.IsValid && B2_PeriodStartDate.IsValid && IsValidEndDateForRSF(periodEnd))
				{
					result = periodEnd.ToString("MM/yyyy");
				}
				return result;
			}
		}

		[MaxLength(2)]
		[ReadOnlyMember(nameof(ImporterHasNoAccountingTimeOption))]
		public ZInt PeriodMonth
		{
			get
			{
				return periodMonth;
			}
			set
			{
				var oldValue = periodMonth;
				if (oldValue != value)
				{
					periodMonth = value;
					PeriodMonthInfo.RefreshBinding(oldValue);

					ValidatePeriodMonthAndPeriodYearAndCalculatePeriodEndDate();
				}
			}
		}
		ZInt periodMonth = 0;

		public ZPropertyInfo PeriodMonthInfo => GetZPropertyInfo(nameof(PeriodMonth));

		[MaxLength(4)]
		[ReadOnlyMember(nameof(ImporterHasNoAccountingTimeOption))]
		public ZInt PeriodYear
		{
			get
			{
				return periodYear;
			}
			set
			{
				var oldValue = periodYear;
				if (oldValue != value)
				{
					periodYear = value;
					PeriodYearInfo.RefreshBinding(oldValue);

					ValidatePeriodMonthAndPeriodYearAndCalculatePeriodEndDate();
				}
			}
		}
		ZInt periodYear = 0;

		public ZPropertyInfo PeriodYearInfo => GetZPropertyInfo(nameof(PeriodYear));

		OrgImpAddInfo ImporterAddInfo
		{
			get
			{
				if (importerAddInfo == null)
				{
					importerAddInfo = new CachedProperty<OrgImpAddInfo>(Factory, () =>
					{
						var importer = Importer;
						return importer != null ? OrgImpAddInfo.Get(importer) : null;
					});
				}
				return importerAddInfo.Value;
			}
		}
		CachedProperty<OrgImpAddInfo> importerAddInfo;

		void ValidatePeriodMonthAndPeriodYearAndCalculatePeriodEndDate()
		{
			void CalculatePeriodEndDate()
			{
				if (ImporterAddInfo is OrgImpAddInfo impAddInfo)
				{
					if (impAddInfo.ZO_AccountingTimeOption == CSARSFAccountingOptionList.Codes.Option1)
					{
						B2_PeriodEndDate = new ZDate(periodYear, periodMonth, DateTime.DaysInMonth(periodYear, periodMonth));
					}
					else if (impAddInfo.ZO_AccountingTimeOption == CSARSFAccountingOptionList.Codes.Option2)
					{
						B2_PeriodEndDate = new ZDate(periodYear, periodMonth, 18);
					}
				}
			}

			if (!ImporterHasNoAccountingTimeOption && Validation is CSARevenueSummaryFormValidation validation)
			{
				validation.ValidatePeriodMonth();
				validation.ValidatePeriodYear();

				if (!suspendCalculatePeriodEndDate && !PeriodMonthInfo.HasErrors() && !PeriodYearInfo.HasErrors())
				{
					CalculatePeriodEndDate();
				}
			}
		}

		bool suspendCalculatePeriodEndDate;

		public bool ImporterHasNoAccountingTimeOption
		{
			get
			{
				return !(IsCSARSF && ImporterAddInfo is OrgImpAddInfo impAddInfo && impAddInfo.ZO_IsCSAApprovedImporter && !impAddInfo.ZO_AccountingTimeOption.IsEmpty);
			}
		}

		#region Properties

		[ReadOnlyMember(nameof(IsCSARSF))]
		public override ZString B2_Status { get => base.B2_Status; set => base.B2_Status = value; }

		[ReadOnlyMember(nameof(IsCSARSF))]
		public override ZDateTime B2_PrintDate { get => base.B2_PrintDate; set => base.B2_PrintDate = value; }

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.StatementTypes))]
		public override ZString B2_StatementType { get => base.B2_StatementType; set => base.B2_StatementType = value; }

		public ZString ConvertedStatementType
		{
			get
			{
				var result = ZString.Empty;
				if (IsCARMSOA)
				{
					result = CARMStatementOfAccountStatementTypeList.GetLongCode(B2_StatementType);
				}
				return result;
			}
		}

		public override ZDate B2_PeriodEndDate
		{
			get => base.B2_PeriodEndDate;
			set
			{
				var oldValue = base.B2_PeriodEndDate;
				base.B2_PeriodEndDate = value;
				if (IsCSARSF && oldValue != base.B2_PeriodEndDate)
				{
					DefaultPeriodStartDate();
					DefaultStatementNumber();
					DefaultPeriodYearAndPeriodMonth();
				}
			}
		}

		void DefaultPeriodStartDate()
		{
			B2_PeriodStartDate = ZDate.Empty;
			var date = B2_PeriodEndDate;
			if (IsCSARSF && date.IsValid && IsValidEndDateForRSF(date))
			{
				var day = date.Day;
				var endDay = DateTime.DaysInMonth(date.Year, date.Month);
				if (day == 18)
				{
					B2_PeriodStartDate = new ZDate(date.Year, date.Month, 19).AddMonths(-1);
				}
				if (day == endDay)
				{
					B2_PeriodStartDate = new ZDate(date.Year, date.Month, 1);
				}
			}
		}

		void DefaultPeriodYearAndPeriodMonth()
		{
			var date = B2_PeriodEndDate;
			if (IsCSARSF && date.IsValid && IsValidEndDateForRSF(date))
			{
				suspendCalculatePeriodEndDate = true;
				PeriodMonth = date.Month;
				PeriodYear = date.Year;
				suspendCalculatePeriodEndDate = false;
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public override ZDate B2_PeriodStartDate => base.B2_PeriodStartDate;

		public bool IsCSARSF => B2_StatementType == CusStatementHeaderTypes.Codes.RSF;

		public bool IsValidEndDateForRSF(ZDate date)
		{
			var endDay = DateTime.DaysInMonth(date.Year, date.Month);
			var month = date.Month;
			var day = date.Day;
			if (month > 0 && month < 13 && (day == 18 || day == endDay))
			{
				return true;
			}
			return false;
		}

		public override ZBool B2_IsMonthlyStatement
		{
			get => base.B2_IsMonthlyStatement;
			set
			{
				if (value != base.B2_IsMonthlyStatement)
				{
					base.B2_IsMonthlyStatement = value;
					ReloadWorkflowItems();
				}
			}
		}

		public override ZGuid B2_OH_Importer
		{
			get => base.B2_OH_Importer;
			set
			{
				var oldValue = base.B2_OH_Importer;
				base.B2_OH_Importer = value;
				if (IsCSARSF)
				{
					B2_ImporterCustomsID = Importer?.GetCABusinessNumber() ?? ZString.Empty;
					DefaultStatementNumber();
					ValidatePeriodMonthAndPeriodYearAndCalculatePeriodEndDate();
				}
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public override ZString B2_ImporterCustomsID { get => base.B2_ImporterCustomsID; set => base.B2_ImporterCustomsID = value; }

		void DefaultStatementNumber()
		{
			if (IsCSARSF && Importer != null && !B2_ImporterCustomsID.IsEmpty && IsValidEndDateForRSF(B2_PeriodEndDate))
			{
				B2_StatementNumber = string.Format("{0}{1}", B2_ImporterCustomsID, B2_PeriodEndDate.ToString("yyyyMM"));
			}
			else
			{
				B2_StatementNumber = ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public override ZString B2_StatementNumber { get => base.B2_StatementNumber; set => base.B2_StatementNumber = value; }

		public ZString ARLMessageTypeCode
		{
			get
			{
				return IsCSARSF ? ARLMessageTypes.Codes.RevenueSummaryForm : (B2_IsMonthlyStatement ? ARLMessageTypes.Codes.StatementOfAccount : ARLMessageTypes.Codes.DailyNotice);
			}
		}

		public ZString ARLMessageType
		{
			get
			{
				return Factory.GetCachedValue<ARLMessageTypes>().GetDescriptionFromCode(ARLMessageTypeCode);
			}
		}

		public ZPropertyInfo ARLMessageTypeInfo => GetZPropertyInfo(nameof(ARLMessageType));

		public CusStatementHeader ParentStatement => !B2_IsMonthlyStatement && B2_B2_PeriodicStatement.IsValid ? Factory.Load<CusStatementHeader>(B2_B2_PeriodicStatement) : null;

		public bool IsCARMDailyNotice
		{
			get
			{
				return IsCARMDailyNoticeRegex.IsMatch(B2_StatementNumber) && UniversalReferenceConstants.IsCarmR2;
			}
		}

		public readonly static Regex IsCARMDailyNoticeRegex = new Regex(@"(?i)^DN-[0-9]{9}(RM[0-9]{4})?-");

		public bool IsCARMSOA
		{
			get
			{
				return CARMStatementOfAccountStatementTypeList.GetLongCode(B2_StatementType).Length == 2 && UniversalReferenceConstants.IsCarmR2;
			}
		}

		public readonly static Regex IsCARMStatementOfAccountRegex = new Regex(@"(?i)^SOA-[0-9]{9}(RM[0-9]{4})?-");

		ZDecimal CalculateTotalAmount(Func<ZDecimal?> fvalueGetter)
		{
			if (!fvalueGetter().HasValue)
			{
				fTotalCustomsDuties = 0m;
				fTotalSIMA = 0m;
				fTotalExciseTax = 0m;
				fTotalExciseDuties = 0m;
				fTotalGST = 0m;
				fTotalOthers = 0m;
				fTotalInterests = 0m;

				if (IsCARMDailyNotice)
				{
					foreach (var line in StatementLines.Cast<CusStatementLine>())
					{
						fTotalCustomsDuties += line.B4_CARMDNChargeAmount_Duties;
						fTotalSIMA += line.B4_CARMDNChargeAmount_SIMA;
						fTotalExciseTax += line.B4_CARMDNChargeAmount_ExciseTax;
						fTotalExciseDuties += line.B4_CARMDNChargeAmount_ExciseDuties;
						fTotalGST += line.B4_CARMDNChargeAmount_GSTAndHSTAndPST;
						fTotalInterests += line.B4_CARMDNChargeAmount_Interests;
						fTotalOthers += line.B4_CARMDNChargeAmount_Others;
					}
				}
				else
				{
					foreach (var line in StatementLines.Cast<CusStatementLine>().Where(x => x.IsNormalLine))
					{
						fTotalCustomsDuties += line.B4_ChargeAmountDTY;
						fTotalSIMA += line.B4_ChargeAmountSIM;
						fTotalExciseTax += line.B4_ChargeAmountEXS;
						fTotalGST += line.B4_ChargeAmountGSTOrGSD;
						fTotalOthers += line.B4_ChargeAmountOTH;
					}
				}
			}

			return fvalueGetter().Value;
		}

		#region Total Properties

		public ZDecimal B2_TotalCustomsDuties => CalculateTotalAmount(() => fTotalCustomsDuties);
		ZDecimal? fTotalCustomsDuties;

		public ZDecimal B2_TotalSIMA => CalculateTotalAmount(() => fTotalSIMA);
		ZDecimal? fTotalSIMA;

		public ZDecimal B2_TotalExciseTax => CalculateTotalAmount(() => fTotalExciseTax);
		ZDecimal? fTotalExciseTax;

		public ZDecimal B2_TotalExciseDuties => CalculateTotalAmount(() => fTotalExciseDuties);
		ZDecimal? fTotalExciseDuties;

		public ZDecimal B2_TotalGST => CalculateTotalAmount(() => fTotalGST);
		ZDecimal? fTotalGST;

		public ZDecimal B2_TotalOthers => CalculateTotalAmount(() => fTotalOthers);
		ZDecimal? fTotalOthers;

		public ZDecimal B2_TotalInterests => CalculateTotalAmount(() => fTotalInterests);
		ZDecimal? fTotalInterests;

		#region CSA Revenue Summary Form

		[ReadOnlyMember(nameof(IsCSARSF))]
		public ZDecimal DebitsTotal
		{
			get
			{
				return IsCSARSF ? Debits.TotalAmount.Round(2) : ZDecimal.Zero;
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public ZDecimal CreditsTotal
		{
			get
			{
				return IsCSARSF ? Credits.TotalAmount.Round(2) : ZDecimal.Zero;
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public ZDecimal InterimPaymentsTotal
		{
			get
			{
				return IsCSARSF ? InterimPayments.TotalAmount.Round(2) : ZDecimal.Zero;
			}
		}

		[ReadOnlyMember(nameof(IsCSARSF))]
		public ZDecimal CustomsAssessmentsTotal
		{
			get
			{
				if (customsAssessmentsTotal == null && IsCSARSF)
				{
					customsAssessmentsTotal = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return decimal.Round(CustomsAssessments.Cast<CSARSFAssessment>().Sum(x => x.Amount), 2);
					});
				}
				return customsAssessmentsTotal.Value;
			}
		}
		CachedProperty<ZDecimal> customsAssessmentsTotal;

		#endregion

		#endregion

		#endregion

		#region Collections

		[ChildEditable(true)]
		public DailyNoticeCusStatementHeaderCollection DailyStatementHeaders
		{
			get
			{
				if (dailyStatementHeaders == null)
				{
					dailyStatementHeaders = new DailyNoticeCusStatementHeaderCollection(this);
					dailyStatementHeaders.Load(new ZQuery { IsNoResultQuery = !B2_IsMonthlyStatement });

					RegisterEditableChildObject(dailyStatementHeaders);
				}

				return dailyStatementHeaders;
			}
		}
		DailyNoticeCusStatementHeaderCollection dailyStatementHeaders;

		[ChildEditable(true)]
		public CusStatementLineCollection StatementLines
		{
			get
			{
				if (fStatementLines == null)
				{
					fStatementLines = new CusStatementLineCollection(this);
					fStatementLines.Load();
					RegisterEditableChildObject(fStatementLines);
				}
				return fStatementLines;
			}
		}
		CusStatementLineCollection fStatementLines;

		[ChildEditable(true)]
		public CusStatementLineGroupCollection LineGroupCollection
		{
			get
			{
				if (lineGroupCollection == null)
				{
					lineGroupCollection = new CusStatementLineGroupCollection(this);
					lineGroupCollection.Load();
					RegisterEditableChildObject(lineGroupCollection);
				}

				return lineGroupCollection;
			}
		}
		CusStatementLineGroupCollection lineGroupCollection;

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					if (IsCSARSF)
					{
						messages = new RSFMessageCollection(this);
					}
					else
					{
						messages = new EDIMessageCollection(this);
					}
					messages.Load();
					RegisterEditableChildObject(messages);
				}

				return messages;
			}
		}
		EDIMessageCollection messages;

		public void CreateOrUpdateCustomNote(ZString description, ZString noteText)
		{
			var note = Notes.FindByDescription(description).FirstOrDefault();

			if (noteText.IsEmpty)
			{
				note?.Delete();
			}
			else
			{
				note = note ?? Notes.AddNew();

				note.ST_IsCustomDescription = true;
				note.ST_Description = description;
				note.ST_NoteDataAsText = noteText;
				note.IsReadOnlyAfterAdd = true;
			}
		}

		#region CSA Revenue Summary Form Collection

		public CusStatementLine DebitLine
		{
			get
			{
				if (debitLine == null && IsCSARSF)
				{
					debitLine = this.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Debit);
					RegisterEditableChildObject(debitLine);
				}
				return debitLine;
			}
		}
		CusStatementLine debitLine;

		[ChildEditable(true)]
		public CSARSFPaymentCollection Debits
		{
			get
			{
				if (debits == null && IsCSARSF)
				{
					debits = new CSARSFPaymentCollection(this, CSARSFPaymentTypes.Codes.Debit);
					RegisterEditableChildObject(debits);
				}
				return debits;
			}
		}
		CSARSFPaymentCollection debits;

		public CusStatementLine CreditLine
		{
			get
			{
				if (creditLine == null && IsCSARSF)
				{
					creditLine = this.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Credit);
					RegisterEditableChildObject(creditLine);
				}
				return creditLine;
			}
		}
		CusStatementLine creditLine;

		[ChildEditable(true)]
		public CSARSFPaymentCollection Credits
		{
			get
			{
				if (credits == null && IsCSARSF)
				{
					credits = new CSARSFPaymentCollection(this, CSARSFPaymentTypes.Codes.Credit);
					RegisterEditableChildObject(credits);
				}
				return credits;
			}
		}
		CSARSFPaymentCollection credits;

		public CusStatementLine InterimLine
		{
			get
			{
				if (interimLine == null && IsCSARSF)
				{
					interimLine = this.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Interim);
					RegisterEditableChildObject(interimLine);
				}
				return interimLine;
			}
		}
		CusStatementLine interimLine;

		[ChildEditable(true)]
		public CSARSFPaymentCollection InterimPayments
		{
			get
			{
				if (interimPayments == null && IsCSARSF)
				{
					interimPayments = new CSARSFPaymentCollection(this, CSARSFPaymentTypes.Codes.Interim);
					RegisterEditableChildObject(interimPayments);
				}
				return interimPayments;
			}
		}
		CSARSFPaymentCollection interimPayments;

		[ChildEditable(true)]
		public CSARSFCustomsAssessmentCollection CustomsAssessments
		{
			get
			{
				if (customsAssessments == null && IsCSARSF)
				{
					customsAssessments = new CSARSFCustomsAssessmentCollection(this);
					RegisterEditableChildObject(customsAssessments);
				}
				return customsAssessments;
			}
		}
		CSARSFCustomsAssessmentCollection customsAssessments;

		[ChildEditable(true)]
		public CSARSFTransactionCollection CSARSFTransactions
		{
			get
			{
				if (csaRSFTransactions == null && IsCSARSF)
				{
					csaRSFTransactions = new CSARSFTransactionCollection(this);
					RegisterEditableChildObject(csaRSFTransactions);
				}
				return csaRSFTransactions;
			}
		}
		CSARSFTransactionCollection csaRSFTransactions;

		public void ResetCSFRSFData()
		{
			if (IsCSARSF)
			{
				ResetAmountInPayments(Debits);
				ResetAmountInPayments(Credits);
				ResetAmountInPayments(InterimPayments);
				var linesToDelete = StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == JobMessageTypeList.Codes.Import || x.B3_EntryType == JobMessageTypeList.Codes.XTypeEntry).ToArray();
				foreach (var line in linesToDelete)
				{
					line.Delete();
				}
				fStatementLines = null;
				CSARSFTransactions?.RemoveAndDeleteAll();
			}
			Factory.Save();
		}

		void ResetAmountInPayments(CSARSFPaymentCollection payments)
		{
			foreach (CSARSFPayment payment in payments)
			{
				if (payment.IsCalculatedAutomatically())
				{
					payment.Amount = ZDecimal.Zero;
				}
			}
		}

		#endregion

		#endregion

		#region Override

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public void RefreshLineGroupStatementLines()
		{
			foreach (CusStatementLineGroup lineGroup in LineGroupCollection)
			{
				lineGroup.RefreshStatementLines();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				StatementLines.RemoveAndDeleteAll();
				LineGroupCollection.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public new CusStatementHeaderLookups Lookups => (CusStatementHeaderLookups)base.Lookups;

		protected override Customs.Business.CusStatementHeaderLookups GetNewLookups()
		{
			return new CusStatementHeaderLookups(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableNameCore => Res.GetString("E9522A15-9D04-46D1-882A-29EB096F5CC2"
			, "{0} - {1}"
			, ARLMessageType
			, B2_StatementNumber.IsEmpty ? B2_ImporterCustomsID : B2_StatementNumber);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore => Res.GetString("125E60C9-4304-4C97-AB40-AAA458216DBD"
			, "{0} Statement - {1}"
			, ARLMessageTypeCode
			, B2_StatementNumber.IsEmpty ? B2_ImporterCustomsID : B2_StatementNumber);

		protected override CusStatementHeaderValidation GetNewValidation()
		{
			return IsCSARSF ? new CSARevenueSummaryFormValidation(this) : base.GetNewValidation();
		}

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new CusStatementHeaderDocumentSupporter(this);

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CusStatementHeader);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IWorkflowProviderTemplateCriteria

		ZGuid IWorkflowProviderTemplateCriteria.CompanyPK => B2_GC;

		#endregion

		#region IWorkflowProvider

		protected override ZString GetWorkflowTypeCore()
		{
			return B2_IsMonthlyStatement
				? StatementProcessTask.StatementWorkflow.Code
				: DailyNoticeWorkflowDescriptor.Constants.Code;
		}

		protected override ProcessTaskCollection GetWorkflowItemsCore()
		{
			return B2_IsMonthlyStatement
				? base.GetWorkflowItemsCore()
				: new DailyNoticeStatementProcessTaskCollection(this);
		}

		protected override string WorkflowItemCollectionKey => B2_IsMonthlyStatement ? base.WorkflowItemCollectionKey : nameof(CusStatementHeader);

		#endregion

		#region CARM SOA

		const string CARMSOASummaryDISTCode = "_DIST";

		CusStatementLineGroup CARMSOASummary
		{
			get
			{
				if (fCARMSOASummary == null && IsCARMSOA)
				{
					fCARMSOASummary = LineGroupCollection.OfType<CusStatementLineGroup>().FirstOrDefault(x => x.B10_ImporterCustomsID == B2_ImporterCustomsID);
				}
				return fCARMSOASummary;
			}
		}
		CusStatementLineGroup fCARMSOASummary;

		public ZDecimal PreviousStatementBalance => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PreviousStatementBalance);

		public ZDecimal CorrectionsLastBalance => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CorrectionsToPreviousStatementBalance);

		public ZDecimal PaymentsAfterLastSOA => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PaymentsReceivedAfterPreviousSoA);

		public ZDecimal Disbursements => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.Disbursements);

		public ZDecimal InterestSum => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.InterestAndPenaltiesSumTotal);

		public ZDecimal CurrentPeriodCharges => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCharges);

		public ZDecimal CurrentPeriodCredits => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCredits);

		public ZDecimal Total => GetAmountFromCARMSOASummary(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentStatementBalance);

		ZDecimal GetAmountFromCARMSOASummary(string type)
		{
			var result = ZDecimal.Zero;
			if (CARMSOASummary != null)
			{
				result = CARMSOASummary.FinancialDetailCollection.Find(type)?.B11_Amount ?? ZDecimal.Zero;
			}
			return result;
		}

		CusStatementLineGroup CARMSOASummaryDIST
		{
			get
			{
				if (fCARMSOASummaryDIST == null && IsCARMSOA)
				{
					fCARMSOASummaryDIST = LineGroupCollection.OfType<CusStatementLineGroup>().FirstOrDefault(x => x.B10_ImporterCustomsID == B2_ImporterCustomsID + CARMSOASummaryDISTCode);
				}
				return fCARMSOASummaryDIST;
			}
		}
		CusStatementLineGroup fCARMSOASummaryDIST;

		public ZDecimal Duties => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.Duties);

		public ZDecimal Excise => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.ExciseTax);

		public ZDecimal ExciseDuties => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties);

		public ZDecimal SIMA => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.SIMA);

		public ZDecimal GST => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax);

		public ZDecimal HST => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax);

		public ZDecimal PST => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax);

		public ZDecimal Interest => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.Interest);

		public ZDecimal Penalties => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.Penalties);

		public ZDecimal Payments => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.Payments);

		public ZDecimal Others => GetAmountFromCARMSOASummaryDIST(CARMDailyNoticeChargeTypeList.Codes.Others);

		ZDecimal GetAmountFromCARMSOASummaryDIST(string type)
		{
			var result = ZDecimal.Zero;
			if (CARMSOASummaryDIST != null)
			{
				result = CARMSOASummaryDIST.FinancialDetailCollection.Find(type)?.B11_Amount ?? ZDecimal.Zero;
			}
			return result;
		}

		#endregion
	}
}
