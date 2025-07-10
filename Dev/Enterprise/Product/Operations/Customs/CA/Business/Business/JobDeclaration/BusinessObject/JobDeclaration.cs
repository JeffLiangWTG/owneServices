using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using EntryChargeTypeList = Enterprise.Customs.CA.Registry.EntryChargeTypeList;
using IManualCancelSupport = Enterprise.Integration.Customs.CA.IManualCancelSupport;
using IManualReleaseNote = Enterprise.Integration.Customs.CA.IManualReleaseNote;
using IManualReleaseSupport = Enterprise.Integration.Customs.CA.IManualReleaseSupport;
using IManualSubmissionNote = Enterprise.Integration.Customs.CA.IManualSubmissionNote;
using IManualSubmissionSupport = Enterprise.Integration.Customs.CA.IManualSubmissionSupport;
using RefCusCodeListAttributes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes;
using TransportCommonShared = Enterprise.TransportCommon.Shared;
using static Enterprise.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CAJobDeclaration)]
	public partial class JobDeclaration : AutoJobDeclaration
		, IBackDoorSavingSupportableBizObj
		, IMessageManagerEventHandler
		, ILandedCostHeader
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IJobDeclaration
		, IDocAddresses
		, IAdditionalReferenceNumberSupporter
		, IInvoicesProvider
		, IManualReleaseSupport
		, IManualCancelSupport
		, IManualSubmissionSupport
		, IJobInvoicingPlugInAdditionalJobs
		, ICusEntryNumberParent
		, IK84ReportAttachee
		, IStmNoteParentWithSystemNote
		, IDISHostProvider
		, ICADIFHost
		, ILPCOCollectionParent
		, ICurrencyConverterDataProvider
		, IUniversalCopySelectivelySupportable
		, ICADeclarationProvider
		, ICreditControlledNotificationTextProvider
		, IBondDetailsDefault
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetupEventStuff();
			LockDownMessageTypeIfNeed();
		}

		public new class Schema : AutoJobDeclaration.Schema
		{
			public const string JE_CCNsAsAString = "JE_CCNsAsAString";
			public const string JE_PeriodMonth = "JE_PeriodMonth";
			public const string JE_PeriodYear = "JE_PeriodYear";
			public const string EstimatedPaymentDueDate = "EstimatedPaymentDueDate";
			public const string TotalCustomsValueInLocalCurrency = "TotalCustomsValueInLocalCurrency";
			public const string TotalNormalDuty = "TotalNormalDuty";
			public const string TotalSimaDuty = "TotalSimaDuty";
			public const string TotalGST = "TotalGST";
			public const string TotalExciseTax = "TotalExciseTax";
			public const string TotalDutyAndTax = "TotalDutyAndTax";
			public const string TotalAmountPayable = "TotalAmountPayable";
			public const string JE_Period = "JE_Period";
			public const string CA_OGDStatusDescription = "CA_OGDStatusDescription";
			public const string IsEnableACROSSValidation = "IsEnableACROSSValidation";
			public const string IsEnableB3Validation = "IsEnableB3Validation";
			public const string CalculatedFreightAmount = "CalculatedFreightAmount";
			public const string CA_DeclarationExceptionDescription = "CA_DeclarationExceptionDescription";
			public const string ExamLocationDescription = "ExamLocationDescription";
			public const string CA_B2SubmissionDateOverride = "CA_B2SubmissionDateOverride";
			public const string CA_B2AcceptedDateOverride = "CA_B2AcceptedDateOverride";
			public const string CA_B2Explanation = "CA_B2Explanation";
		}

		public static class Constants
		{
			public static class MessageNames
			{
				public const string Release = "Release";
				public const string IID = "IID";
				public const string G7 = "G7";
			}
		}

		public override void DefaultValueForFakeDeclaration()
		{
			base.DefaultValueForFakeDeclaration();
			DefaultServiceOption();
		}

		void DefaultServiceOption()
		{
			if (IsImport && !IsLVS)
			{
				if (CA_ServiceOption.IsEmpty)
				{
					CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				}
			}
			else if (!CA_ServiceOption.IsEmpty)
			{
				CA_ServiceOption = ZString.Empty;
			}
		}

		public override bool IsNonTransportDeclarationType => IsB2Adjustments;

		public override bool UseGenPivotForRelatedDeclarations => true;

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get { return new ZString[] { ApplicationCodeList.Codes.CAACI, ApplicationCodeList.Codes.CACustoms, ApplicationCodeList.Codes.CAEXP, ApplicationCodeList.Codes.CAIMP }; }
		}
		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (!IsLVS && !IsB2Adjustments && !HasEntryTransactions && !IsB3X)
			{
				base.DefaultMessageTypeFromSupplierOrImporter(source);
			}
		}

		public bool HasEntryTransactions => ActiveEntryHeaders.Cast<CusEntryHeader>().Any(s => s.HasTransactionsWithCustoms);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		protected override void ThrowAwayMergeCore()
		{
			base.ThrowAwayMergeCore();
			MarkApportionmentDirty();
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		#region SetupEventStuff

		void SetupEventStuff()
		{
			ServiceEventUtils.ExcludeEventsFromAdd(this, new Event[] { Events.CanadianCADLateSendingWarning });
		}

		#endregion

		void SetupNonPersistentProperties()
		{
			hasSetIsEnableACROSSValidation = false;
			hasSetIsEnableB3Validation = false;
			isEnableACROSSValidation = false;
			isEnableB3Validation = false;
		}

		protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			if (!IsLVS && InvoiceLines.Any() && !IsMergeDone)
			{
				MarkApportionmentDirty();
			}

			var result = base.DoMergeCore(notifier);
			if (result)
			{
				if (SupportsChcPivotBetweenInvoiceLineAndPacking || SupportsChzPivotBetweenInvoiceHeaderAndPacking)
				{
					Factory.AddFetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, JE_ClusterKey);
					Factory.AddFetchHint(CusDecHouseContainerPackSchema.CW_ClusterKey, JE_ClusterKey);
					foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
					{
						header.RefreshActualTotalPacksCount();
					}
				}

				CA_RequiresMerge = false;
				ValidateAfterMerge();

				if (JE_MessageType == JobMessageTypeList.Codes.Import || JE_MessageType == JobMessageTypeList.Codes.LowValueShipments)
				{
					CalculateEstimatedPaymentDueDate();
				}
				CalculatedFreightAmountInfo.RefreshBinding();
			}
			return result;
		}

		public void CalculateIM2Total()
		{
			var subTotal = this.CA_AnySightDepositAmount;
			var gstTotal = ZDecimal.Zero;

			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				subTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.CustomsDuty, line);
				subTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.SIMADuty, line);
				subTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.ExciseTax, line);
				gstTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.GST, line);
			}
			foreach (JobComInvoiceLine line in PreviousJob.InvoiceLines)
			{
				subTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.CustomsDuty, line);
				subTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.SIMADuty, line);
				subTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.ExciseTax, line);
				gstTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.GST, line);
			}
			this.CA_B2Total = 0 - (subTotal + (gstTotal < 0 ? gstTotal : 0));
		}

		public void CalculateB2Total()
		{
			var subTotal = this.CA_AnySightDepositAmount;
			var gstTotal = ZDecimal.Zero;

			var invoiceHeaders = Invoices.Cast<JobComInvoiceHeader>();
			foreach (JobComInvoiceLine accountLine in invoiceHeaders.SelectMany(header => header.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>()))
			{
				foreach (JobComInvoiceLine claimLine in accountLine.ReadOnlyAsClaimForFilteredInvoiceLines)
				{
					subTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.CustomsDuty, claimLine);
					subTotal -= claimLine.DutyAndTaxManager.SIMAPayableAmount;
					subTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.ExciseTax, claimLine);
					gstTotal -= CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.GST, claimLine);
				}
				subTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.CustomsDuty, accountLine);
				subTotal += accountLine.DutyAndTaxManager.SIMAPayableAmount;
				subTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.ExciseTax, accountLine);
				gstTotal += CaculateInvoiceAmountPerChargeType(DutyAndTaxTypes.Codes.GST, accountLine);
			}
			this.CA_B2Total = 0 - (subTotal + (gstTotal < 0 ? gstTotal : 0));
		}

		ZDecimal CaculateInvoiceAmountPerChargeType(ZString chargeType, JobComInvoiceLine line)
		{
			var amount = ZDecimal.Zero;
			var dutyAndTaxManager = line.DutyAndTaxManager;
			switch (chargeType)
			{
				case DutyAndTaxTypes.Codes.CustomsDuty:
					amount = dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty);
					break;
				case DutyAndTaxTypes.Codes.SIMADuty:
					amount = dutyAndTaxManager.SIMAPayableAmount;
					break;
				case DutyAndTaxTypes.Codes.ExciseTax:
					amount = dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax);
					break;
				case DutyAndTaxTypes.Codes.GST:
					amount = dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
					break;
			}
			return amount;
		}

		void ValidateAfterMerge()
		{
			if (IsImport)
			{
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					line.AddInfoValidation.ValidateCA_AuthorityNumber();
				}
			}
		}

		protected override ZString OtherReferenceNumber
		{
			get
			{
				return CargoControlNumbers.Count > 0 ? CargoControlNumbers[0].CY_CargoControlNumber : ZString.Empty;
			}
		}

		public ZDecimal AmountDueImporter
		{
			get { return CA_B2Total < 0m ? CA_B2Total : ZDecimal.Zero; }
		}

		public ZDecimal AmountDueCBSA
		{
			get { return CA_B2Total > 0m ? CA_B2Total : ZDecimal.Zero; }
		}

		#region Effective CCN

		internal IEnumerable<ReleaseStatus> PersistReleaseStatus
		{
			get
			{
				return this.ReleaseStatuses.Cast<ReleaseStatus>().Where(x => x.IsPersistent);
			}
		}

		[ReadOnlyMember(nameof(MultipleCCN))]
		[MaxLength(CargoControlNumber.Schema.CY_CargoControlNumberMaxLength)]
		public ZString EffectiveCCN
		{
			get
			{
				var effectiveCCN = ZString.Empty;
				var count = CargoControlNumbers.Count;
				if (count == 1)
				{
					effectiveCCN = CargoControlNumbers[0].CY_CargoControlNumber;
				}
				else if (count > 1)
				{
					effectiveCCN = Res.GetString("95BE5A43-5B69-4A18-B29A-F094256060BD", "Multiple CCNs – See Packing");
				}
				return effectiveCCN;
			}
			set
			{
				var oldValue = this.EffectiveCCN;
				if (value != oldValue)
				{
					var existing = PersistReleaseStatus.FirstOrDefault();
					if (value.IsEmpty)
					{
						if (existing != null)
						{
							ReleaseStatuses.RemoveAndDelete(existing);
						}
					}
					else
					{
						if (existing == null)
						{
							existing = ReleaseStatuses.AddNew();
						}
						existing.RL_CargoControlNumber = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateEffectiveCCN();
						Validation.ValidateEffectiveCCNPrefix();
					}
				}
				this.EffectiveCCNInfo.RefreshBinding();
				this.EffectiveCCNPrefixInfo.RefreshBinding();
				this.ReleaseStatuses.RefreshBinding();
			}
		}

		ZString effectiveCCNPrefix;
		[ReadOnlyMember(nameof(MultipleCCN))]
		[MaxLength(CargoControlNumber.Schema.CY_CargoControlNumberPrefixLength)]
		public ZString EffectiveCCNPrefix
		{
			get
			{
				effectiveCCNPrefix = ZString.Empty;
				if (CargoControlNumbers.Count == 1)
				{
					effectiveCCNPrefix = CargoControlNumbers[0].CY_CargoControlNumber.Left(CargoControlNumber.Schema.CY_CargoControlNumberPrefixLength);
				}
				return effectiveCCNPrefix;
			}
			set
			{
				this.effectiveCCNPrefix = value;
				this.EffectiveCCN = this.effectiveCCNPrefix + this.effectiveCCNSuffix;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEffectiveCCNPrefix();
				}

				this.EffectiveCCNPrefixInfo.RefreshBinding();
			}
		}

		bool MultipleCCNOrPrefixIsInValid => PersistReleaseStatus.Count() > 1 || this.EffectiveCCNPrefix.Length < 4;
		ZString effectiveCCNSuffix;
		[ReadOnlyMember(nameof(MultipleCCNOrPrefixIsInValid))]
		[MaxLength(CargoControlNumber.Schema.CY_CargoControlNumberSuffixMaxLength)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString EffectiveCCNSuffix
		{
			get
			{
				effectiveCCNSuffix = (CargoControlNumbers.Count > 1 || EffectiveCCN.IsEmpty) ?
					EffectiveCCN :
					EffectiveCCN.SubstringSafe(CargoControlNumber.Schema.CY_CargoControlNumberPrefixLength);
				return effectiveCCNSuffix;
			}
			set
			{
				this.effectiveCCNSuffix = value;

				this.EffectiveCCN = this.effectiveCCNPrefix + this.effectiveCCNSuffix;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEffectiveCCNSuffix();
				}
				this.EffectiveCCNPrefixInfo.RefreshBinding();
				this.EffectiveCCNSuffixInfo.RefreshBinding();

				if (this.IsAir)
				{
					this.JE_HouseBill = effectiveCCNSuffix;
				}
			}
		}

		public void DefaultHouseBillFromEffectiveCCN()
		{
			if (Shipment == null && PersistReleaseStatus.Count() == 1 && EffectiveCCN.Length > 4 && JE_HouseBill.IsEmpty)
			{
				JE_HouseBill = EffectiveCCN.Substring(4).TrimStart(' ', '-').TrimEnd();
			}
		}

		public ZPropertyInfo EffectiveCCNInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveCCN)); }
		}

		public ZPropertyInfo EffectiveCCNPrefixInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveCCNPrefix)); }
		}

		public ZPropertyInfo EffectiveCCNSuffixInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveCCNSuffix)); }
		}

		public bool MultipleCCN => PersistReleaseStatus.Count() > 1;
		#endregion

		public ZString CreationMethod
		{
			get
			{
				var result = JE_MessageType;
				if (IsIM2)
				{
					result = Res.GetString("D836D459-C8C0-4A82-892B-92CE4118D285", "Copy IM2");
				}
				else if (IsB2Adjustments)
				{
					result = Res.GetString("1F7546AA-D42A-4397-AB87-DE751FA91D78", "Manual B2");
				}
				else if (IsB3X)
				{
					result = Res.GetString("E5BE5949-BF6E-4E45-8538-D73D5749D153", "X Type Entry");
				}

				return result;
			}
		}

		protected override ZString OtherReferenceNumberCaption
		{
			get
			{
				return "CCN";
			}
		}

		public JobCADeclaration CADeclaration => this.LoadOrCreateAddInfoChild(ref caDeclaration);
		JobCADeclaration caDeclaration;

		public override void MakeNonPersistent()
		{
			base.MakeNonPersistent();
			CADeclaration.MakeNonPersistent();
		}

		#region Cus Entry Numbers

		#region TransactionNumber

		public TransactionNumber TransactionNumber
		{
			get
			{
				if (IsLVX)
				{
					var declaration = Invoices.FirstOrDefault()?.FirstAdditionalDeclaration as JobDeclaration;
					return declaration == null ? new TransactionNumber() : declaration.TransactionNumber;
				}
				else
				{
					if (transactionNumber == null || transactionNumber.IsDeleted)
					{
						transactionNumber = new TransactionNumber(this);
					}
					return transactionNumber;
				}
			}
		}

		TransactionNumber transactionNumber;

		void IJobDeclaration.UpdateTransactionNumber(ZString accountSecurityCode, ZString sequentialNumber)
		{
			TransactionNumber.AccountSecurityCode = accountSecurityCode;
			TransactionNumber.SequentialNumber = sequentialNumber;
		}

		public ZString FormattedTransactionNumber
		{
			get { return TransactionNumber.ToString(); }
		}

		#endregion

		#endregion

		#region New Properties & Methods

		#region HasUSPlaceOfExportInvoice

		public bool HasUSPlaceOfExportInvoice
		{
			get { return Invoices.Any(l => ((JobComInvoiceHeader)l).IsUSPlaceOfExport); }
		}

		#endregion

		void DefaultExportServiceProviderIfRequired()
		{
			if (IsG7ExportDeclaration && Forwarder == null && Supplier != null && DefaultServiceProvider != null && !IsCompanyOrgProxyTheExporter)
			{
				JE_OH_Forwarder = DefaultServiceProvider.PK;
			}
		}

		#region JE_CERSProofOfReportNumber

		[MaxLength(25)]
		public ZString JE_CERSProofOfReportNumber
		{
			get { return CERSProofOfReportNumber.CE_EntryNum; }
			set
			{
				CheckMaximumLength(JE_CERSProofOfReportNumberInfo, value);
				CERSProofOfReportNumber.CE_EntryNum = value;
				CERSProofOfReportNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				JE_CERSProofOfReportNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_CERSProofOfReportNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JE_CERSProofOfReportNumber)); }
		}

		public CusEntryNumber CERSProofOfReportNumber
		{
			get
			{
				if (cersProofOfReportNumber == null)
				{
					cersProofOfReportNumber = CusEntryNumber.LoadOrCreate(this, CanadaAdditionalReferenceNumberTypes.Codes.CTN, Core.Constants.CountryCodes.Canada);
					if (cersProofOfReportNumber.CE_Category != CusEntryNumber.Categories.AdditionalReferenceNumber)
					{
						cersProofOfReportNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
					}
				}
				return cersProofOfReportNumber;
			}
		}
		CusEntryNumber cersProofOfReportNumber;

		public ICusEntryNumber JE_CAEDProofOfReportCusEntryNumber
		{
			get
			{
				if (caedProofOfReportCusEntryNumber == null)
				{
					caedProofOfReportCusEntryNumber = CusEntryNumber.Load(this, CanadaAdditionalReferenceNumberTypes.Codes.CTN, Core.Constants.CountryCodes.Canada);
				}
				return caedProofOfReportCusEntryNumber;
			}
		}
		ICusEntryNumber caedProofOfReportCusEntryNumber;

		#endregion

		public ZString ReleaseOffice
		{
			get { return CA_ReleaseOffice.IsEmpty ? JE_CustomsOffice : CA_ReleaseOffice; }
		}

		public ZString B3Comments
		{
			get
			{
				var result = ZString.Empty;
				var b3CommentsNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description);
				if (b3CommentsNotes.Length > 0)
				{
					result = b3CommentsNotes[0].ST_NoteText;
				}
				return result;
			}
		}

		public ZString CADComments
		{
			get
			{
				var result = ZString.Empty;
				var cadCommentsNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description);
				if (cadCommentsNotes.Length > 0)
				{
					result = cadCommentsNotes[0].ST_NoteText;
				}
				return result;
			}
		}

		public ZString JE_CCNsAsAString
		{
			get
			{
				var stringBuilder = new ZStringBuilder(from CargoControlNumber cnn in CargoControlNumbers where !cnn.CY_CargoControlNumber.IsEmpty select cnn.CY_CargoControlNumber);
				return stringBuilder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZDateTime EffectiveDutyDate
		{
			get { return JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate : (CA_EstReleaseDate.IsValid ? CA_EstReleaseDate : ZDate.Today); }
		}

		protected override ZDate GetDateForDutyRateCore()
		{
			return EffectiveDutyDate.Date;
		}

		internal ZDateTime LastDayOfCurrentMonth
		{
			get
			{
				var curYear = ZDateTime.Now.Year;
				var curMonth = ZDateTime.Now.Month;
				return new ZDateTime(curYear, curMonth, DateTime.DaysInMonth(curYear, curMonth));
			}
		}

		internal ZDate K84CutOffDate
		{
			get
			{
				var nowInOtawa = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
				var result = K84DateFor(nowInOtawa);
				if (nowInOtawa > result)
				{
					result = K84DateFor(nowInOtawa.AddMonths(1));
				}
				return result.Date;
			}
		}

		internal ZDateTime K84DateFor(ZDateTime date)
		{
			return new ZDateTime(date.Year, date.Month, 24, 20, 0, 0).GetNearestWorkingdayBefore(CAHolidaysApplicableToThisDeclaration);
		}

		internal bool IsPaymentDueDateAfterCutOffDate(out ZDate k84CutOffDate)
		{
			k84CutOffDate = K84CutOffDate;
			var dueDateForWarning = EstimatedPaymentDueDateForWarning;
			return (dueDateForWarning.IsValid && dueDateForWarning > k84CutOffDate);
		}

		ZDateTime DueDateForLowValueShipment
		{
			get
			{
				var baseDate = GetBaseDateForCalculatingDueDate();
				return baseDate.IsValid ? K84DateFor(new ZDateTime(baseDate.Year, baseDate.Month, 1).AddMonths(1)).Date : ZDateTime.Empty;
			}
		}

		ZDateTime EstimatedPaymentDueDateForWarning
		{
			get
			{
				var result = ZDateTime.Empty;
				if (JE_EntryAuthorisationDate.IsValid)
				{
					if (IsLVS || IsLowValueNormalReleaseJob)
					{
						result = DueDateForLowValueShipment;
					}
					else
					{
						var effectiveBranch = this.Branch ?? GlbBranch.CurrentBranch;
						var threshold = CACustomsDataRegistry.Instance.EntryStatementDateThreshold.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
						if (threshold > 0)
						{
							result = JE_EntryAuthorisationDate.AddWorkingDaysFromNearestWorkingDay(threshold, CAHolidaysApplicableToThisDeclaration).Date;
						}
					}
				}
				return result;
			}
		}

		public ZDateTime EstimatedPaymentDueDate
		{
			get { return CA_EstimatedPaymentDueDate; }
		}

		public void CalculateEstimatedPaymentDueDate()
		{
			if (IsLVS)
			{
				CA_EstimatedPaymentDueDate = DueDateForLowValueShipment;
			}
			else
			{
				if (CA_CSAEntry)
				{
					CA_EstimatedPaymentDueDate = ZDate.Empty;
				}
				else
				{
					var baseDate = GetBaseDateForCalculatingDueDate();
					if (baseDate.IsValid)
					{
						var estimatedPaymentDueDateForCSAApporvedImporter = GetEstimatedPaymentDueDateForCSAApporvedImporter(baseDate);
						if (estimatedPaymentDueDateForCSAApporvedImporter.IsValid)
						{
							CA_EstimatedPaymentDueDate = estimatedPaymentDueDateForCSAApporvedImporter;
						}
						else
						{
							CA_EstimatedPaymentDueDate = baseDate.AddWorkingDaysFromNearestWorkingDay(5, CAHolidaysApplicableToThisDeclaration).Date;
						}
					}
					else
					{
						CA_EstimatedPaymentDueDate = ZDate.Empty;
					}
				}
			}
		}

		ZDateTime GetBaseDateForCalculatingDueDate()
		{
			return JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate :
							CA_EstReleaseDate.IsValid ? CA_EstReleaseDate :
							JE_DateOfFirstArrival.IsValid ? JE_DateOfFirstArrival :
							JE_DateAtFinalDestination.IsValid ? JE_DateAtFinalDestination :
							JE_DateOfArrival.IsValid ? JE_DateOfArrival : ZDateTime.Empty;
		}

		ZDateTime GetEstimatedPaymentDueDateForCSAApporvedImporter(ZDateTime baseDate)
		{
			var result = ZDateTime.Empty;
			var orgImpAddInfo = ImporterAddInfo;
			if (orgImpAddInfo != null && !orgImpAddInfo.ZO_AccountingTimeOption.IsEmpty)
			{
				if (orgImpAddInfo.ZO_AccountingTimeOption == CSARSFAccountingOptionList.Codes.Option1)
				{
					var nextMonthDate = baseDate.AddMonths(1);
					result = new ZDateTime(nextMonthDate.Year, nextMonthDate.Month, 18);
				}
				else if (orgImpAddInfo.ZO_AccountingTimeOption == CSARSFAccountingOptionList.Codes.Option2)
				{
					var dateAffterCalculation = baseDate;
					if (baseDate.Day > 18)
					{
						dateAffterCalculation = baseDate.AddMonths(1);
					}
					var year = dateAffterCalculation.Year;
					var month = dateAffterCalculation.Month;
					result = new ZDateTime(year, month, DateTime.DaysInMonth(year, month)).GetNearestWorkingdayBefore(CAHolidaysApplicableToThisDeclaration);
				}
			}
			return result;
		}

		internal void UpdateCA_AccountingAge()
		{
			using (DisposableEnvironment.ForBranch(JE_GB.IsValid ? JE_GB.ToGuid() : GlbBranch.CurrentBranch.PK.ToGuid()))
			{
				if (CA_CSAEntry || IsLVS || !JE_EntryAuthorisationDate.IsValid || IsB3Lodged || IsCADLodged || JE_EntryAuthorisationDate.Date >= ZDate.Today || IsImport && IsB3NotRequired || !CA_K84AccountingDate.IsEmpty)
				{
					CA_AccountingAge = ZInt.Zero;
				}
				else
				{
					var result = JE_EntryAuthorisationDate.GetCountOfWorkingDaysTo(ZDateTime.Today, CAHolidaysApplicableToThisDeclaration);
					if (result >= 0 && CA_AccountingAge != result)
					{
						CA_AccountingAge = result;
					}
				}
			}
		}

		public OrgHeader ExportLicenceProxy
		{
			get
			{
				if (exportLicenceProxy == null)
				{
					var branch = Branch ?? GlbBranch.CurrentBranch;
					if (branch.OrgProxy is OrgHeader branchOrgProxy)
					{
						var orgCustomCode = branchOrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.ExportLicenceNumber, Core.Constants.CountryCodes.Canada);
						if (orgCustomCode != null)
						{
							exportLicenceProxy = branchOrgProxy;
						}
					}

					if (exportLicenceProxy == null && branch.Company?.OrgProxy is OrgHeader companyOrgProxy)
					{
						var orgCustomCode = companyOrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.ExportLicenceNumber, Core.Constants.CountryCodes.Canada);
						if (orgCustomCode != null)
						{
							exportLicenceProxy = companyOrgProxy;
						}
					}
				}
				return exportLicenceProxy;
			}
		}
		OrgHeader exportLicenceProxy;

		public OrgHeader DefaultServiceProvider
		{
			get
			{
				var result = Factory.Load<OrgHeader>(CACustomsDataRegistry.Instance.DefaultServiceProviderOrganization.Value);
				return result ?? ExportLicenceProxy;
			}
		}

		public ZString OtherGenericTransportModeDescription
		{
			get
			{
				return IsOtherGeneric ? Lookups.TransportTypeList.GetDescriptionFromCode(JE_TransportMode) : string.Empty;
			}
		}

#if DEBUG
		public void ResetCachedValuesForTest()
		{
			exportLicenceProxy = null;
		}
#endif

		[ReadOnlyMember(nameof(JE_PaymentMethod_ReadOnly))]
		public override ZString JE_PaymentMethod
		{
			get { return base.JE_PaymentMethod; }
			set { base.JE_PaymentMethod = value; }
		}

		bool JE_PaymentMethod_ReadOnly
		{
			get { return !IsB3X && !CACustomsDataRegistry.Instance.AllowPaymentPartyOverride.Value; }
		}

		#region Document related properties

		public ZString PreviousCCN
		{
			get
			{
				var previousCCN = CusEntryNumber.Load(this, CanadaAdditionalReferenceNumberTypes.Codes.PCN, CountryCode);
				return previousCCN != null ? previousCCN.CE_EntryNum : ZString.Empty;
			}
		}

		public ReleaseStatusDocumentWrapper ReleaseStatusWrapper
		{
			get
			{
				if (releaseStatusWrapper == null)
				{
					if (ReleaseEntryHeader is CusEntryHeader entryHeader)
					{
						var ediMessage = EDIReleaseMessage.GetLastReleaseStatusMessage(entryHeader.Messages, RNSMessagingBO.ReleaseSubTypesToIgnore);
						if (ediMessage != null)
						{
							releaseStatusWrapper = new ReleaseStatusDocumentWrapper(ediMessage);
						}
					}
					if (releaseStatusWrapper == null)
					{
						releaseStatusWrapper = new ReleaseStatusDocumentWrapper();
					}
				}
				return releaseStatusWrapper;
			}
		}
		ReleaseStatusDocumentWrapper releaseStatusWrapper;

		IReleaseStatusWrapper IJobDeclaration.ReleaseStatusWrapper => this.ReleaseStatusWrapper;

		public GlbStaff DeclarantOnEntryDocsBrokerOnB3
		{
			get
			{
				if (declarantOnEntryDocsBrokerOnB3 == null)
				{
					var effectiveBranch = this.Branch ?? GlbBranch.CurrentBranch;
					ZGuid declarantPK = CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
					declarantOnEntryDocsBrokerOnB3 = declarantPK.IsValid ? Factory.Load<GlbStaff>(declarantPK) : null;
				}
				return declarantOnEntryDocsBrokerOnB3;
			}
		}
		GlbStaff declarantOnEntryDocsBrokerOnB3;

		public GlbStaff DeclarantOnEntryDocsBrokerOnB2
		{
			get
			{
				if (declarantOnEntryDocsBrokerOnB2 == null)
				{
					var effectiveBranch = this.Branch ?? GlbBranch.CurrentBranch;
					ZGuid declarantPK = CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB2.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
					declarantOnEntryDocsBrokerOnB2 = declarantPK.IsValid ? Factory.Load<GlbStaff>(declarantPK) : null;
				}
				return declarantOnEntryDocsBrokerOnB2;
			}
		}
		GlbStaff declarantOnEntryDocsBrokerOnB2;

		public ZBool IsPrintBrokerSignatureImage
		{
			get
			{
				return Factory.GetValue(ref cachedIsPrintBrokerSignatureImage, () =>
				{
					var effectiveBranch = this.Branch ?? GlbBranch.CurrentBranch;
					return CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
				});
			}
		}
		CachedProperty<ZBool> cachedIsPrintBrokerSignatureImage;

		public ZBool IsDefaultExciseDutyQuantityToFirstCustomsQuantity
		{
			get
			{
				return Factory.GetValue(ref cachedIsDefaultExciseDutyQuantityToFirstCustomsQuantity, () =>
				{
					var effectiveBranch = this.Branch ?? GlbBranch.CurrentBranch;
					return CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
				});
			}
		}
		CachedProperty<ZBool> cachedIsDefaultExciseDutyQuantityToFirstCustomsQuantity;

		#endregion

		public bool AVSEventRequired { get; set; }
		public bool InSendingMessageProcess { private get; set; }
		public bool RefreshValidationBeforeSendMessage { get; set; } = true;

		public ZString CA_OGDStatusDescription
		{
			get { return AddInfoLookups.OGDStatusCodes.GetDescriptionFromCode(CA_OGDStatus); }
		}

		public ZPropertyInfo CA_OGDStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CA_OGDStatusDescription); }
		}

		public ZDateTime B3AcceptedDate
		{
			get
			{
				return B3EntryHeader?.CH_EntryReleaseDate ?? ZDateTime.Empty;
			}
		}

		public ZDateTime TimeAtPortOfDischarge
		{
			get { return GetEffectivePortOfArrival()?.LocationDateTime ?? ZDateTime.Empty; }
		}

		#region Booleans

		public bool IsCompanyOrgProxyTheExporter
		{
			get { return Supplier != null && DefaultServiceProvider != null && DefaultServiceProvider.PK == Supplier.PK; }
		}

		public bool IsDataLoadingModule
		{
			get { return IsExport && !IsG7ExportDeclaration; }
		}

		public bool IsLVS
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.LowValueShipments || JE_MessageType == JobMessageTypeList.Codes.LVSForConsolidation; }
		}

		public bool ShowShipmentRelatedFields => !(SuppressShipmentRelatedFields && IsImport);

		public bool ShowShipmentRelatedFieldsOrSea => ShowShipmentRelatedFields || IsSea;

		public bool IsConsolidatedLVS
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.LowValueShipments; }
		}

		public bool IsLVSTotalConsolidation
		{
			get { return IsConsolidatedLVS && JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation; }
		}

		public bool IsIM2
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ImportCopyforB2; }
		}

		public bool IsB3X
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.XTypeEntry; }
		}

		public ZBool IsCSAApprovedImporter
		{
			get
			{
				return ImporterAddInfo?.IsCSAApprovedImporter ?? ZBool.False;
			}
		}

		public bool IsLowValueNormalReleaseJob
		{
			get
			{
				if (isLowValueNormalReleaseJobCached == null)
				{
					isLowValueNormalReleaseJobCached = new CachedProperty<bool>(Factory, () =>
					{
						var result = false;
						if (Invoices.Count > 0 && InvoiceLines.Count > 0)
						{
							if (B3EntryHeader != null)
							{
								var valuationDate = Invoices.Count > 0 ? Invoices[0].EffectiveValuationDate.Date : ZDate.Today;
								if (!ApportionmentDirty)
								{
									var taxOrFee = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Canada, Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, valuationDate);
									result = taxOrFee != null && TotalCustomsValueInLocalCurrency <= taxOrFee.ZZF_Value;
								}
							}
						}
						return result;
					});
				}
				return isLowValueNormalReleaseJobCached.Value;
			}
		}
		CachedProperty<bool> isLowValueNormalReleaseJobCached;

		public bool IsB2Adjustments
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.B2Adjustments; }
		}

		public bool IsB2OrIM2OrB3X => IsB2Adjustments || IsIM2 || IsB3X;

		public bool IsLVX
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.LVSForConsolidation; }
		}

		public bool IsB3NotRequired
		{
			get { return JE_MessageSubType == B3EntryTypeList.Codes.NoB3; }
		}

		public bool IsB3CADNotMessageAllowedToSend
		{
			get
			{
				return JE_MessageSubType.IsEmpty || JE_MessageSubType == B3EntryTypeList.Codes.NoB3 || JE_MessageSubType == B3EntryTypeList.Codes.ConfirmingSight
					|| JE_MessageSubType == B3EntryTypeList.Codes.CashC || JE_MessageSubType == B3EntryTypeList.Codes.CashD || JE_MessageSubType == B3EntryTypeList.Codes.Supplementary
					|| JE_MessageSubType == B3EntryTypeList.Codes.Postal || JE_MessageSubType == B3EntryTypeList.Codes.Voluntary;
			}
		}

		public bool IsImporterPaysFlagged
		{
			get
			{
				return JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Importer ||
					(JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Default && IsImporterOrganizationDirect);
			}
		}

		internal ZBool IsImporterOrganizationDirect
		{
			get
			{
				var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
				if (importerAddInfo == null || IsLVSTotalConsolidation)
				{
					return false;
				}
				return IsLowValueNormalReleaseJob || IsConsolidatedLVS ? importerAddInfo.ZO_IsLVSImporterDirectPayment : importerAddInfo.ZO_IsImporterDirectPayment;
			}
		}

		public bool IsMisc
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Misc; }
		}

		public bool IsG7ExportDeclaration
		{
			get { return CustomsEntryHeaders.Count > 0 && !CustomsEntryHeaders[0].CH_MessageType.IsEmpty ? CustomsEntryHeaders[0].IsG7ExportDeclaration : IsExport && CACustomsDataRegistry.Instance.SendG7ExportMessages.Value; }
		}

		public ZBool IsFixedTransportInstallation
		{
			get { return JE_TransportMode == TransportTypeList.Codes.FixedTransportInstallations; }
		}

		public ZBool IsInlandWaterwayTransport
		{
			get { return JE_TransportMode == TransportTypeList.Codes.InlandWaterwayTransport; }
		}

		public ZBool IsNoCarrier
		{
			get { return JE_TransportMode == TransportTypeList.Codes.NoCarrier; }
		}

		public ZBool IsOtherGeneric
		{
			get { return this.TransportModeGeneric == TransportTypeGenericList.Codes.Other; }
		}

		public bool IsPARS
		{
			get { return CA_ServiceOption == ServiceOptions.Codes.PARS || CA_ServiceOption == ServiceOptions.Codes.PARSOGD; }
		}

		public bool IsAQ
		{
			get { return CA_AssesmentOption == AssessmentOptions.Codes.AppraisalQualityData; }
		}

		public bool IsOGD
		{
			get { return IsImport && !IsLVS && (CA_ServiceOption == ServiceOptions.Codes.PARSOGD || CA_ServiceOption == ServiceOptions.Codes.RMDOGD); }
		}

		public bool IsIID
		{
			get { return IsImport && !IsLVS && CA_ServiceOption == ACROSSServiceOptions.Codes.IID; }
		}

		public bool IsCSA
		{
			get { return IsImport && !IsLVS && CA_ServiceOption == ACROSSServiceOptions.Codes.CSA; }
		}

		[ReadOnlyMember(nameof(JE_MessageTypeReadOnly))]
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				bool hasChanges = base.JE_MessageType != value;
				base.JE_MessageType = value;
				if (hasChanges && !IsCopying)
				{
					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						line.RefreshDutiesAndTaxes();
					}

					var isImport = IsImport;
					if (isImport)
					{
						DefaultJI_OA_ConsigneeAddressFromInvoice();
						BondDetailsDefaulter.Default(this, CA_BondType);
					}
					else
					{
						CA_ServiceOption = ZString.Empty;
						if (cargoControlNumbers != null)
						{
							cargoControlNumbers.RemoveAndDeleteAll();
						}
						if (IsB2Adjustments || IsB3X)
						{
							CreateAsClaimedInvoiceGroupHeader();
						}
					}
					ReSetB3LateSendingWarningEvent();
				}
				DeletePackagePivotIfRequired();
				DefaultServiceOption();
				SetupNonPersistentProperties();
				DefaultJE_ApplicationCode();
			}
		}

		bool JE_MessageTypeReadOnly { get; set; }

		void LockDownMessageTypeIfNeed()
		{
			JE_MessageTypeReadOnly = IsIM2;
		}

		public override bool SupportsParentPackage => IsIID;

		public override event EventHandler ParentPackageColumnSupportedChanged
		{
			add
			{
				JE_MessageTypeInfo.ValueChanged += value;
				CA_ServiceOptionInfo.ValueChanged += value;
			}
			remove
			{
				JE_MessageTypeInfo.ValueChanged -= value;
				CA_ServiceOptionInfo.ValueChanged -= value;
			}
		}

		public ZBool IsArrivalDatePast => JE_DateOfArrival.IsValid && ZDate.Today > JE_DateOfArrival.Date;

		public bool IsWarehouseEntry
		{
			get
			{
				return (IsCADEnabled ? (JE_MessageSubType == CADEntryTypeList.Codes.Warehouse101 || JE_MessageSubType == CADEntryTypeList.Codes.Warehouse102)
					: JE_MessageSubType == B3EntryTypeList.Codes.Warehouse10)
					|| IsOtherWarehouseEntry;
			}
		}

		public bool IsInwardWarehouseEntry
		{
			get { return IsCADEnabled ? CADEntryTypeList.IsInwardWarehouseEntryType(JE_MessageSubType) : B3EntryTypeList.IsInwardWarehouseEntryType(JE_MessageSubType); }
		}

		public bool IsExWarehouseEntry
		{
			get { return IsCADEnabled ? CADEntryTypeList.IsExWarehouseEntryType(JE_MessageSubType) : B3EntryTypeList.IsExWarehouseEntryType(JE_MessageSubType); }
		}

		public bool IsWarehouseOrSupplementaryEntry
		{
			get
			{
				return JE_MessageSubType == B3EntryTypeList.Codes.Supplementary ||
						IsOtherWarehouseEntry;
			}
		}

		public bool IsOtherWarehouseEntry
		{
			get
			{
				if (IsCADEnabled)
				{
					return JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse131 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse132 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse201 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse211 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse212 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse213 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse214 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse215 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse216 ||
					JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods301 ||
					JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods302;
				}
				return JE_MessageSubType == B3EntryTypeList.Codes.ReWarehouse13 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse20 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse21 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse22 ||
					JE_MessageSubType == B3EntryTypeList.Codes.TransferOfGoods30;
			}
		}

		public bool IsExWarehouseAndDutyPayable
		{
			get { return JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse20 || JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse201; }
		}

		public bool IsPaperOnlyEntry
		{
			get
			{
				return JE_MessageSubType == B3EntryTypeList.Codes.ConfirmingSight ||
					JE_MessageSubType == B3EntryTypeList.Codes.CashC ||
					JE_MessageSubType == B3EntryTypeList.Codes.CashD ||
					JE_MessageSubType == B3EntryTypeList.Codes.Supplementary ||
					JE_MessageSubType == B3EntryTypeList.Codes.Postal ||
					JE_MessageSubType == B3EntryTypeList.Codes.Voluntary;
			}
		}

		public bool IsElectronicEntry
		{
			get
			{
				if (IsCADEnabled)
				{
					return JE_MessageSubType == CADEntryTypeList.Codes.Confirming ||
					JE_MessageSubType == CADEntryTypeList.Codes.Warehouse101 ||
					JE_MessageSubType == CADEntryTypeList.Codes.Warehouse102 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse131 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse132 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse201 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse211 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse212 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse213 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse214 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse215 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse216 ||
					JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse22 ||
					JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods301 ||
					JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods302;
				}
				return JE_MessageSubType == B3EntryTypeList.Codes.Confirming ||
					JE_MessageSubType == B3EntryTypeList.Codes.Warehouse10 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ReWarehouse13 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse20 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse21 ||
					JE_MessageSubType == B3EntryTypeList.Codes.ExWarehouse22 ||
					JE_MessageSubType == B3EntryTypeList.Codes.TransferOfGoods30;
			}
		}

		public bool IsWarehouseNotInType
		{
			get
			{
				return JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse131
				|| JE_MessageSubType == CADEntryTypeList.Codes.ReWarehouse132
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse201
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse211
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse212
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse213
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse214
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse215
				|| JE_MessageSubType == CADEntryTypeList.Codes.ExWarehouse216
				|| JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods301
				|| JE_MessageSubType == CADEntryTypeList.Codes.TransferOfGoods302;
			}
		}

		public bool IsTypeF
		{
			get
			{
				return JE_MessageSubType == B3EntryTypeList.Codes.LowValueShipments;
			}
		}

		bool EntryIsLodged(string entryType)
		{
			var result = false;
			var b3EntryHeader = B3EntryHeader;
			if (b3EntryHeader != null && b3EntryHeader.CH_MessageType == entryType)
			{
				result = b3EntryHeader.IsClearedB3CorCAD;
			}
			return result;
		}

		public bool IsB3Lodged
		{
			get
			{
				return EntryIsLodged(MessageTypeList.Codes.B3CUSDEC);
			}
		}

		public bool IsCADLodged
		{
			get
			{
				return EntryIsLodged(MessageTypeList.Codes.CommercialAccountingDeclaration);
			}
		}

		public bool IsB3ValidationRequired
		{
			get
			{
				return DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC);
			}
		}

		public OrgImpAddInfo ImporterAddInfo
		{
			get { return Importer != null ? OrgImpAddInfo.Get(Importer) : null; }
		}

		public OrgImpAddInfo ImporterOfRecordAddInfo
		{
			get { return ImporterOfRecordAddress.HasRealOrganisation ? OrgImpAddInfo.Get(ImporterOfRecordAddress.Organisation) : null; }
		}

		public OrgHeader ImporterOfRecord => ImporterOfRecordAddress.HasRealOrganisation ? ImporterOfRecordAddress.Organisation : null;

		public OrgHeader EffectiveImporter => ImporterOfRecord ?? Importer;

		public OrgImpAddInfo EffectiveImporterAddInfo => ImporterOfRecordAddInfo ?? ImporterAddInfo;

		public ZString EffectiveCFIAFeePaymentMethod => EffectiveImporterAddInfo?.ZO_EffectiveCFIAFeePaymentMethod ?? new ZString(CFIAPaymentMethods.Codes.Other);

		public GlbBranch EffectiveBranch => this.Branch ?? GlbBranch.GetCurrentBranch(Factory);

		public bool IsImporterDirectPayment
		{
			get
			{
				var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
				return IsImporterDirectPaymentCore(JE_PaymentMethod, importerAddInfo, (x) => x.ZO_IsLVSImporterDirectPayment, (y) => y.ZO_IsImporterDirectPayment);
			}
		}

		public bool IsImporterDirectPaymentAutoRated
		{
			get
			{
				var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
				return IsImporterDirectPaymentAutoRatedCore(importerAddInfo);
			}
		}

		public bool IsImporterDirectPaymentAutoRatedCore(OrgImpAddInfo importerAddInfo)
		{
			return IsImporterDirectPaymentCore(JE_PaymentMethod, importerAddInfo,
				(x) => x.ZO_IsLVSImporterDirectPayment && !x.ZO_IsLVSImporterAutoDutyDirectAmts, (y) => y.ZO_IsImporterDirectPayment && !y.ZO_IsHighImporterAutoDutyDirectAmts);
		}

		bool IsImporterDirectPaymentCore(ZString paymentMethod, OrgImpAddInfo importerAddInfo, Func<OrgImpAddInfo, bool> isLVSImpDirectPayment, Func<OrgImpAddInfo, bool> isHVSImpDirectPayment)
		{
			bool IsImporterOrganizationDirectCore()
			{
				if (importerAddInfo == null || IsLVSTotalConsolidation)
				{
					return false;
				}
				return IsLowValueNormalReleaseJob || IsConsolidatedLVS ? isLVSImpDirectPayment(importerAddInfo) : isHVSImpDirectPayment(importerAddInfo);
			}

			return paymentMethod == Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer ||
				(paymentMethod == Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default && IsImporterOrganizationDirectCore());
		}

		public bool IsGSTDirectPayment
		{
			get
			{
				var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
				return !IsLVSTotalConsolidation && IsGSTDirectPaymentCore(importerAddInfo);
			}
		}

		internal bool IsGSTDirectPaymentCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo != null && (IsCADEnabled ? !importerAddInfo.ZO_CADIsBrokerToPay : (bool)importerAddInfo.ZO_IsGSTDirectPayment);
		}

		public bool IsGSTDirectAutoRated
		{
			get
			{
				var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
				return IsGSTDirectAutoRatedCore(importerAddInfo);
			}
		}

		internal static bool IsGSTDirectAutoRatedCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo != null && importerAddInfo.ZO_IsGSTDirectAutoRated;
		}

		public ZBool ForceManualInputOfTransactionNumber
		{
			get { return CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		public ZBool DisplaySequentialOfTransactionNumberSeparately
		{
			get { return CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		public bool IsImporterAccountSecurityCodeUsed
		{
			get
			{
				if (isImporterAccountSecurityCodeUsedCache == null)
				{
					isImporterAccountSecurityCodeUsedCache = new CachedProperty<bool>(Factory, delegate
					{
						var importerAddInfo = ImporterOfRecordAddInfo ?? ImporterAddInfo;
						return importerAddInfo != null && !importerAddInfo.ZO_AccountSecurityNumber.IsEmpty && importerAddInfo.ZO_AccountSecurityNumber == TransactionNumber.AccountSecurityCode;
					});
				}
				return isImporterAccountSecurityCodeUsedCache.Value;
			}
		}
		CachedProperty<bool> isImporterAccountSecurityCodeUsedCache;

		internal bool ShouldCalculateDutiesOnMerge { get; private set; }

#if DEBUG
		public void ResetShouldCalculateDutiesOnMergeForTest()
		{
			ShouldCalculateDutiesOnMerge = false;
		}
#endif

		public bool IsAnyInvoiceHasFutureDirectShipmentDate
		{
			get
			{
				ZDateTime timeInOttawa = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
				foreach (var invoice in Invoices)
				{
					if (invoice.JZ_ValuationDateOverride.IsValid && invoice.JZ_ValuationDateOverride.Date > timeInOttawa.Date)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ZBool IsSimplifiedLVSMode { get; set; }

		#endregion

		#region CarrierCode

		#region ExportCarrierCode

		public ZString ExportCarrierCode
		{
			get
			{
				var exportCarrierCode = CA_TransportDocumentNumber.SubstringSafe(0, 4);
				return !exportCarrierCode.IsEmpty && exportCarrierCode != "77YY" ? exportCarrierCode : GetCarrierCodeFromShippingLineOrForwarder();
			}
		}

		ZString GetCarrierCodeFromShippingLineOrForwarder()
		{
			var result = ZString.Empty;
			if (ShippingLine != null)
			{
				var orgCustomCode = ShippingLine.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
				if (orgCustomCode != null)
				{
					result = orgCustomCode.OK_CustomsRegNo;
				}
			}

			if (result.IsEmpty && Forwarder != null)
			{
				var orgCustomCode = Forwarder.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
				if (orgCustomCode != null)
				{
					result = orgCustomCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		public OrgHeader G7ExportCarrier
		{
			get { return ShippingLine ?? Forwarder; }
		}

		public OrgHeader ExportingCarrier
		{
			get { return IsG7ExportDeclaration ? G7ExportCarrier : null; }
		}

		#endregion

		#endregion

		#region JE_MessageStatusDescription

		public override ZString JE_MessageStatusDescription
		{
			get
			{
				if (IsLVX)
				{
					var declaration = LVXInvoiceHeader.FirstAdditionalDeclaration;
					return declaration == null ? ZString.Empty : declaration.JE_MessageStatusDescription;
				}
				else
				{
					return Lookups.MessageStatusList.GetDescriptionFromCode(JE_MessageStatus) ?? "";
				}
			}
		}

		#endregion

		#region Validator

		public CADeclarationValidator DeclarationValidator
		{
			get { return declarationValidator ?? (declarationValidator = new CADeclarationValidator(this)); }
		}
		CADeclarationValidator declarationValidator;

		#endregion

		#region Casual Import

		public ZBool IsExistingEffectiveCasualImport
		{
			get
			{
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					if (line.InvoiceHeader != null && line.InvoiceHeader.IsAttachedToPersistentDeclaration && line.IsEffectiveCasualImport)
					{
						return true;
					}
				}

				return false;
			}
		}

		public JobComInvoiceHeader LastInvoiceHeader
		{
			get
			{
				return Invoices.OfType<JobComInvoiceHeader>().LastOrDefault();
			}
		}

		#endregion

		#region LVX

		public JobComInvoiceHeader LVXInvoiceHeader
		{
			get
			{
				return IsLVX ? Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault() ?? Invoices.AddNew() : null;
			}
		}

		#endregion

		#region Scheduled B3 Auto-Sending Date

		public ZDateTime ScheduledB3AutoSendingDate
		{
			get
			{
				return CA_B3AutoSend && CACustomsDataRegistry.Instance.ActivateAutoB3Sending.Value ? CalculatedB3SendingDate : ZDateTime.Empty;
			}
		}

		public ZDateTime CalculatedB3SendingDate
		{
			get
			{
				if (!IsMergeInProgress)
				{
					var strategy = JobDeclarationB3SendingStrategy.GetJobDeclarationB3SendingStrategy(this);
					return strategy == null ? ZDateTime.Empty : strategy.ScheduledB3AutoSendingDate;
				}
				return ZDateTime.Empty;
			}
		}

		public ZDateTime ScheduledB3SendingDate
		{
			get
			{
				ZDateTime result;
				if (HasScheduledB3Message)
				{
					var scheduledB3AutoSendingDate = ScheduledB3AutoSendingDate;
					var scheduledB3MessageTime = ScheduledB3MessageTime;

					if (scheduledB3AutoSendingDate.IsValid)
					{
						result = scheduledB3MessageTime <= scheduledB3AutoSendingDate ? ScheduledB3MessageTime : scheduledB3AutoSendingDate;
					}
					else
					{
						result = scheduledB3MessageTime;
					}
				}
				else
				{
					result = ScheduledB3AutoSendingDate;
				}

				return result;
			}
		}

		#endregion

		#region Enable ACROSS/B3 Validation

		public ZBool IsEnableACROSSValidation
		{
			get
			{
				if (!hasSetIsEnableACROSSValidation)
				{
					hasSetIsEnableACROSSValidation = true;
					if (JE_MessageType != JobMessageTypeList.Codes.Export)
					{
						isEnableACROSSValidation = (CACustomsDataRegistry.Instance.AlwaysEnableACROSSMessageValidation.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) || JE_EntryAuthorisationDate.IsEmpty) && !IsIM2;
					}
				}
				return isEnableACROSSValidation;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(IsEnableACROSSValidationInfo, ref isEnableACROSSValidation, value);
				}
			}
		}
		protected ZBool isEnableACROSSValidation;
		ZBool hasSetIsEnableACROSSValidation;

		public ZPropertyInfo IsEnableACROSSValidationInfo
		{
			get { return this.GetZPropertyInfo(Schema.IsEnableACROSSValidation); }
		}

		public ZBool IsEnableB3Validation
		{
			get
			{
				if (!hasSetIsEnableB3Validation)
				{
					hasSetIsEnableB3Validation = true;
					if (JE_MessageType != JobMessageTypeList.Codes.Export)
					{
						isEnableB3Validation = CACustomsDataRegistry.Instance.AlwaysEnableB3MessageValidation.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) || !JE_EntryAuthorisationDate.IsEmpty;
					}
				}
				return isEnableB3Validation;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(IsEnableB3ValidationInfo, ref isEnableB3Validation, value);
				}
			}
		}
		protected ZBool isEnableB3Validation;
		ZBool hasSetIsEnableB3Validation;

		public ZPropertyInfo IsEnableB3ValidationInfo
		{
			get { return this.GetZPropertyInfo(Schema.IsEnableB3Validation); }
		}

		#endregion

		#region Freight Amount

		public ZDecimal CalculatedFreightAmount
		{
			get
			{
				if (!IsMergeInProgress)
				{
					return Utilities.Round(TotalCustomsValueInLocalCurrency * EffectiveFreightPercentage / 100, 0);
				}
				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo CalculatedFreightAmountInfo
		{
			get { return this.GetZPropertyInfo(Schema.CalculatedFreightAmount); }
		}

		public ZDecimal EffectiveFreightPercentage
		{
			get
			{
				var result = ZDecimal.Zero;
				if (!JE_TransportMode.IsEmpty)
				{
					var freightPercentage = OrgImpAddInfo.Get(Importer)?.FreightPercentages?.GetFirstElementHaving(JE_TransportMode);
					if (freightPercentage != null)
					{
						result = freightPercentage.DefaultFreightPercentage;
					}
					else
					{
						var registryFreightPercentage = CACustomsDataRegistry.Instance.DefaultFreightPercentages.Value?.Cast<DefaultFreightPercentage>()
							?.FirstOrDefault(o => o != null && o.ModeofTransport == JE_TransportMode);
						if (registryFreightPercentage != null)
						{
							result = registryFreightPercentage.FreightPercentage;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region New Collections

		#region Permits

		[ChildEditable(true)]
		public DeclarationExportPermitCollection Permits
		{
			get
			{
				if (permits == null)
				{
					permits = new DeclarationExportPermitCollection(this);
					permits.Load();
					RegisterEditableChildObject(permits);
				}
				return permits;
			}
		}

		DeclarationExportPermitCollection permits;

		#endregion

		#region Cargo Control Numbers

		[ChildEditable(true)]
		public CargoControlNumberCollection CargoControlNumbers
		{
			get
			{
				if (cargoControlNumbers == null)
				{
					cargoControlNumbers = new CargoControlNumberCollection(this);
					cargoControlNumbers.Load();
					RegisterEditableChildObject(cargoControlNumbers);
				}
				return cargoControlNumbers;
			}
		}
		CargoControlNumberCollection cargoControlNumbers;

		[ChildEditable(false)]
		public CargoControlNumberCollection OriginalJobCargoControlNumbers
		{
			get
			{
				return IsB2OrIM2OrB3X && OriginalDeclaration != null ? OriginalDeclaration.CargoControlNumbers : CargoControlNumbers;
			}
		}

		#endregion

		#region Release Statuses

		[ChildEditable(true)]
		public ReleaseStatusCollection ReleaseStatuses
		{
			get
			{
				if (releaseStatuses == null)
				{
					releaseStatuses = new ReleaseStatusCollection(this);
					RegisterEditableChildObject(releaseStatuses);
				}

				return releaseStatuses;
			}
		}
		ReleaseStatusCollection releaseStatuses;

		public ReleaseStatusToPrintCollection ReleaseStatusesToPrint
		{
			get
			{
				if (releaseStatusesToPrint == null)
				{
					releaseStatusesToPrint = new ReleaseStatusToPrintCollection(this);
					ReleaseStatuses.CountChanged += ReleaseStatuses_CountChanged;
				}
				return releaseStatusesToPrint;
			}
		}
		ReleaseStatusToPrintCollection releaseStatusesToPrint;

		void ReleaseStatuses_CountChanged(object sender, EventArgs e)
		{
			releaseStatusesToPrint = null;
		}

		internal void FireOnGetReleaseStatusesToPrint(CancelEventArgs eventArgs)
		{
			if (OnGetReleaseStatusesToPrint != null)
			{
				OnGetReleaseStatusesToPrint(this, eventArgs);
			}
		}

		public event EventHandler<CancelEventArgs> OnGetReleaseStatusesToPrint;

		#endregion

		#region CusCALPCOCollection

		[ChildEditable(true)]
		public CusCALPCOCollection LPCOs
		{
			get
			{
				if (lpcos == null)
				{
					lpcos = new CusCALPCOCollection(this);
					lpcos.Load();
					RegisterEditableChildObject(lpcos);
				}
				return lpcos;
			}
		}
		CusCALPCOCollection lpcos;

		[ChildEditable]
		public LPCOViewCollection LPCOViews
		{
			get
			{
				if (lpcosViews == null)
				{
					lpcosViews = new LPCOViewCollection(LPCOs, null, null);
					RegisterEditableChildObject(lpcosViews);
				}
				return lpcosViews;
			}
		}
		LPCOViewCollection lpcosViews;

		#endregion

		#region PGAs

		public bool DoesRequireImporterHasPGAContact
		{
			get
			{
				return IsIID &&
					(HasInvoiceLinesWithHCPGA
					|| HasInvoiceLinesWithCFIAPGA
					|| HasInvoiceLinesWithTCPGA
					|| HasInvoiceLinesWithPHACPGA);
			}
		}

		public bool ClassificationsHasPGARequirements
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (invoice.HasInvoiceLinesWithPGARequirements)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool DoesRequireExporterHasPGAContact
		{
			get { return IsIID && HasInvoiceLinesWithWENIndOnECCCPGA; }
		}

		public bool DoesRequireImporterHasCFIAAccountNumber
		{
			get
			{
				return IsIID && HasInvoiceLinesWithCFIAPGA;
			}
		}

		public bool IsCFIAAccountNumberRequired
		{
			get
			{
				return Factory.GetValue(ref isCFIAAccountNumberRequiredCached, () =>
				{
					var result = false;
					if (EffectiveCFIAFeePaymentMethod == CFIAPaymentMethods.Codes.Broker)
					{
						var branch = EffectiveBranch;
						var branchOrgProxy = branch?.OrgProxy;
						if (branchOrgProxy == null || branchOrgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.CFIAAccountNumber, Core.Constants.CountryCodes.Canada).Length == 0)
						{
							var companyOrgProxy = branch?.Company?.OrgProxy;
							if (companyOrgProxy == null || companyOrgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.CFIAAccountNumber, Core.Constants.CountryCodes.Canada).Length == 0)
							{
								result = true;
							}
						}
					}
					return result;
				});
			}
		}
		CachedProperty<bool> isCFIAAccountNumberRequiredCached;

		public bool HasInvoiceLinesWithCFIAPGA
		{
			get
			{
				if (!hasInvoiceLinesWithCFIAPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithCFIAPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithCNSCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithCNSCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithCNSCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithDFOPGA
		{
			get
			{
				if (!hasInvoiceLinesWithDFOPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithDFOPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithECCCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithECCCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithECCCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithGACPGA
		{
			get
			{
				if (!hasInvoiceLinesWithGACPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithGACPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithHCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithHCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithHCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithNRCanPGA
		{
			get
			{
				if (!hasInvoiceLinesWithNRCanPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithNRCanPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithPHACPGA
		{
			get
			{
				if (!hasInvoiceLinesWithPHACPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithPHACPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithTCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithTCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithTCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithWENIndOnECCCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithWENIndOnECCCPGA.HasValue)
				{
					calculateInvoiceLinesWithWENIndOnECCCPGA();
				}
				return hasInvoiceLinesWithWENIndOnECCCPGA.Value;
			}
		}

		void calculateInvoiceLinesWithWENIndOnECCCPGA()
		{
			hasInvoiceLinesWithWENIndOnECCCPGA = false;

			foreach (JobComInvoiceHeader invoice in this.Invoices)
			{
				hasInvoiceLinesWithWENIndOnECCCPGA |= invoice.HasInvoiceLinesWithWENIndOnECCCPGA;
			}
		}

		void CalculateInvoiceLinesWithPGAs()
		{
			hasInvoiceLinesWithCFIAPGA = false;
			hasInvoiceLinesWithCNSCPGA = false;
			hasInvoiceLinesWithDFOPGA = false;
			hasInvoiceLinesWithECCCPGA = false;
			hasInvoiceLinesWithGACPGA = false;
			hasInvoiceLinesWithHCPGA = false;
			hasInvoiceLinesWithNRCanPGA = false;
			hasInvoiceLinesWithPHACPGA = false;
			hasInvoiceLinesWithTCPGA = false;

			foreach (JobComInvoiceHeader invoice in this.Invoices)
			{
				hasInvoiceLinesWithCFIAPGA |= invoice.HasInvoiceLinesWithCFIAPGA;
				hasInvoiceLinesWithCNSCPGA |= invoice.HasInvoiceLinesWithCNSCPGA;
				hasInvoiceLinesWithDFOPGA |= invoice.HasInvoiceLinesWithDFOPGA;
				hasInvoiceLinesWithECCCPGA |= invoice.HasInvoiceLinesWithECCCPGA;
				hasInvoiceLinesWithGACPGA |= invoice.HasInvoiceLinesWithGACPGA;
				hasInvoiceLinesWithHCPGA |= invoice.HasInvoiceLinesWithHCPGA;
				hasInvoiceLinesWithNRCanPGA |= invoice.HasInvoiceLinesWithNRCanPGA;
				hasInvoiceLinesWithPHACPGA |= invoice.HasInvoiceLinesWithPHACPGA;
				hasInvoiceLinesWithTCPGA |= invoice.HasInvoiceLinesWithTCPGA;
			}
		}

		public void RefreshInvoiceLinesWithPGAs()
		{
			hasInvoiceLinesWithCFIAPGA = null;
			hasInvoiceLinesWithCNSCPGA = null;
			hasInvoiceLinesWithDFOPGA = null;
			hasInvoiceLinesWithECCCPGA = null;
			hasInvoiceLinesWithGACPGA = null;
			hasInvoiceLinesWithHCPGA = null;
			hasInvoiceLinesWithNRCanPGA = null;
			hasInvoiceLinesWithPHACPGA = null;
			hasInvoiceLinesWithTCPGA = null;
		}

		bool? hasInvoiceLinesWithCFIAPGA;
		bool? hasInvoiceLinesWithHCPGA;
		bool? hasInvoiceLinesWithPHACPGA;
		bool? hasInvoiceLinesWithTCPGA;
		bool? hasInvoiceLinesWithECCCPGA;
		bool? hasInvoiceLinesWithNRCanPGA;
		bool? hasInvoiceLinesWithDFOPGA;
		bool? hasInvoiceLinesWithCNSCPGA;
		bool? hasInvoiceLinesWithGACPGA;
		bool? hasInvoiceLinesWithWENIndOnECCCPGA;

		#endregion

		#endregion

		#region Doc Addresses

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.CustomsContainerTerminalOperatorAddress || docAddress.DocAddressType == DocAddressType.CustomsDepotAddress)
			{
				DefaultPortOfClearanceAndSubLocationCode();
			}
		}

		#region Supplier Documentary Address

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			MarkAsNeedingValidation();
		}

		protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
		{
			base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
			supplierDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += SupplierDocumentaryAddressE2_OA_AddressChanged;
		}

		void SupplierDocumentaryAddressE2_OA_AddressChanged(object sender, EventArgs e)
		{
			var newDocAddress = (JobDocAddress)sender;
			var newDocAddressOrganisationPK = newDocAddress.OrganisationPK;
			var newDocAddressE2_OA_Address = newDocAddress.E2_OA_Address;

			if (JE_OH_Supplier != newDocAddressOrganisationPK)
			{
				JE_OH_Supplier = newDocAddressOrganisationPK;
			}

			if (currentSupplierDocumentaryAddressE2_OA_Address == null)
			{
				currentSupplierDocumentaryAddressE2_OA_Address = (ZBool)newDocAddress.E2_AddressOverrideInfo.OriginalValue ? ZGuid.Empty : (ZGuid)newDocAddress.E2_OA_AddressInfo.OriginalValue;
			}
			if (!newDocAddress.E2_AddressOverride && !currentSupplierDocumentaryAddressE2_OA_Address.Value.IsEmpty)
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (!invoice.SupplierDocumentaryAddress.E2_AddressOverride && invoice.SupplierDocumentaryAddress.E2_OA_Address == currentSupplierDocumentaryAddressE2_OA_Address)
					{
						invoice.SupplierDocumentaryAddress.OrganisationPK = newDocAddressOrganisationPK;
						invoice.SupplierDocumentaryAddress.E2_OA_Address = newDocAddressE2_OA_Address;
					}
				}
			}
			currentSupplierDocumentaryAddressE2_OA_Address = newDocAddressE2_OA_Address;
		}
		ZGuid? currentSupplierDocumentaryAddressE2_OA_Address;

		protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid je_oh_supplier)
		{
		}

		#endregion

		#region Importer Documentary Address

		public override JobDocAddress ImporterDocumentaryAddress
		{
			get
			{
				if (fImporterDocumentaryAddress == null || fImporterDocumentaryAddress.IsDeleted || shouldUpdateImporterDocumentaryAddress)
				{
					fImporterDocumentaryAddress = base.ImporterDocumentaryAddress;
					if (fImporterDocumentaryAddress != null)
					{
						fImporterDocumentaryAddress.DefaultAddressType = IsIID ? AddressType.CST : AddressType.NoDefault;
					}
				}
				return fImporterDocumentaryAddress;
			}
		}
		JobDocAddress fImporterDocumentaryAddress;

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			MarkAsNeedingValidation();
		}

		protected override JobDocAddressRequirement AddImporterDocAddressRequirement()
		{
			var result = base.AddImporterDocAddressRequirement();
			result.DefaultAddressType = IsIID ? AddressType.CST : AddressType.NoDefault;
			return result;
		}

		#endregion

		#region Vendor Documentary Address

		public JobDocAddress VendorDocAddress
		{
			get
			{
				return FindOrCreateJobDocAddress(ref fVendorDocAddress, () =>
				{
					var docAddress = DocAddresses.FindOrCreateWithRequirement(VendorDocAddressRequirement);
					docAddress.DocAddressChanged += delegate
					{ MarkAsNeedingValidation(); };
					return docAddress;
				});
			}
		}
		JobDocAddress fVendorDocAddress;

		JobDocAddressRequirement VendorDocAddressRequirement
		{
			get
			{
				if (fVendorDocAddressRequirement == null)
				{
					fVendorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SellingParty, ContactType.Administration);
					DocAddressManager.AddRequirement(fVendorDocAddressRequirement);
				}
				return fVendorDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fVendorDocAddressRequirement;

		#endregion

		#region SetCodeFromDepotOrCTODocAddress

		void DefaultPortOfClearanceAndSubLocationCode()
		{
			if (!IsCopying)
			{
				if (IsImportIncludingB2)
				{
					var onlyIsEmpty = IsInDatabase;
					if (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.Value)
					{
						UNLOCODefaulter.DefaultCustomsCode(() => JE_CustomsOfficeInfo, CACustomsCodeType.Office, onlyIsEmpty);
						UNLOCODefaulter.DefaultCustomsCode(() => JE_LocationOfGoodsInfo, CACustomsCodeType.SubLocation, onlyIsEmpty);
					}
					else
					{
						CustomsPortOfClearanceDefaulter.DefaultCustomsCode(onlyIsEmpty);
						SubLocationDefaulter.DefaultCustomsCode(onlyIsEmpty);
					}

					if (JE_CustomsOffice.IsEmpty && EffectiveBranch != null && EffectiveBranch.Company != null)
					{
						JE_CustomsOffice = CACustomsDataRegistry.Instance.DefaultPortOfClearance.GetFallBackValueAtAllLevels(EffectiveBranch.Company.PK.ToGuid(), EffectiveBranch.PK.ToGuid(), Guid.Empty);
					}
				}
				else
				{
					JE_CustomsOffice = ZString.Empty;
					JE_LocationOfGoods = ZString.Empty;
				}
			}
		}

		#endregion

		#region Commercial Invoice Originator

		public JobDocAddress CommercialInvoiceOriginator => FindOrCreateJobDocAddress(ref fCommercialInvoiceOriginator, () => DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CommercialInvoiceOriginator));
		JobDocAddress fCommercialInvoiceOriginator;

		#endregion

		#region Manufacturer

		public new JobDocAddress Manufacturer => FindOrCreateJobDocAddress(ref fManufacturer, () => DocAddresses.FindOrCreateWithRequirement(ManufacturerRequirement));
		JobDocAddress fManufacturer;

		JobDocAddressRequirement ManufacturerRequirement
		{
			get
			{
				if (fManufacturerRequirement == null)
				{
					fManufacturerRequirement = new JobDocAddressRequirement(DocAddressType.Manufacturer, AddressType.OFC, ContactType.NoContactType, true, 0);
					DocAddressManager.AddRequirement(fManufacturerRequirement);
				}
				return fManufacturerRequirement;
			}
		}
		JobDocAddressRequirement fManufacturerRequirement;

		#endregion

		#region AdditionalDeliveryAddress

		public JobDocAddress AdditionalDeliveryAddress
		{
			get { return FindOrCreateJobDocAddress(ref fAdditionalDeliveryAddress, () => DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AdditionalDeliveryAddress)); }
		}
		JobDocAddress fAdditionalDeliveryAddress;

		JobDocAddressRequirement AdditionalDeliveryAddressRequirement
		{
			get
			{
				if (fAdditionalDeliveryAddressRequirement == null)
				{
					fAdditionalDeliveryAddressRequirement = new JobDocAddressRequirement(DocAddressType.AdditionalDeliveryAddress, AddressType.OFC, ContactType.NoContactType, true, 0);
					DocAddressManager.AddRequirement(fAdditionalDeliveryAddressRequirement);
				}
				return fAdditionalDeliveryAddressRequirement;
			}
		}
		JobDocAddressRequirement fAdditionalDeliveryAddressRequirement;

		#endregion

		#region AdditionalConsignee

		public JobDocAddress AdditionalConsignee => FindOrCreateJobDocAddress(ref fAdditionalConsignee, () => DocAddresses.FindOrCreateWithRequirement(AdditionalConsigneeRequirement));
		JobDocAddress fAdditionalConsignee;

		JobDocAddressRequirement AdditionalConsigneeRequirement
		{
			get
			{
				if (fAdditionalConsigneeRequirement == null)
				{
					fAdditionalConsigneeRequirement = new JobDocAddressRequirement(DocAddressType.AdditionalConsignee, AddressType.OFC, ContactType.Consignee, true, 0);
					DocAddressManager.AddRequirement(fAdditionalConsigneeRequirement);
				}
				return fAdditionalConsigneeRequirement;
			}
		}
		JobDocAddressRequirement fAdditionalConsigneeRequirement;

		#endregion

		#region OGDProcessInspectionLPCO

		public JobDocAddress OGDProcessInspectionLPCO => FindOrCreateJobDocAddress(ref fOGDProcessInspectionLPCO, () => DocAddresses.FindOrCreateWithRequirement(OGDProcessInspectionLPCORequirement));
		JobDocAddress fOGDProcessInspectionLPCO;

		JobDocAddressRequirement OGDProcessInspectionLPCORequirement
		{
			get
			{
				if (fOGDProcessInspectionLPCORequirement == null)
				{
					fOGDProcessInspectionLPCORequirement = new JobDocAddressRequirement(DocAddressType.OGDProcessInspectionLPCO, AddressType.OFC, ContactType.NoContactType, true, 0);
					DocAddressManager.AddRequirement(fOGDProcessInspectionLPCORequirement);
				}
				return fOGDProcessInspectionLPCORequirement;
			}
		}
		JobDocAddressRequirement fOGDProcessInspectionLPCORequirement;

		#endregion

		#region CFIAPaymentParty

		public JobDocAddress CFIAPaymentParty => FindOrCreateJobDocAddress(ref fCFIAPaymentParty, () => DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CFIAPaymentParty));
		JobDocAddress fCFIAPaymentParty;

		#endregion

		#region ImporterOfRecordAddress

		public JobDocAddress ImporterOfRecordAddress
		{
			get
			{
				if (fImporterOfRecordAddress == null || fImporterOfRecordAddress.IsDeleted || shouldUpdateImporterOfRecord)
				{
					fImporterOfRecordAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterOfRecord);
					if (fImporterOfRecordAddress != null)
					{
						fImporterOfRecordAddress.DefaultAddressType = IsIID ? AddressType.CST : AddressType.NoDefault;
						fImporterOfRecordAddress.OrgHeaderAfterChange += ImporterOfRecordAddress_OrgHeaderAfterChange;
					}
					shouldUpdateImporterOfRecord = false;
				}
				return fImporterOfRecordAddress;
			}
		}
		JobDocAddress fImporterOfRecordAddress;

		void ImporterOfRecordAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			if (IsImportIncludingB2)
			{
				TransactionNumber.SetAccountSecurityNo();
				BondDetailsDefaulter.Default(this, CA_BondType);
			}
		}

		#endregion

		JobDocAddress FindOrCreateJobDocAddress(ref JobDocAddress docAddress, Func<JobDocAddress> createDocAddress)
		{
			if (docAddress == null || docAddress.IsDeleted)
			{
				docAddress = createDocAddress();
			}
			return docAddress;
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.SellingParty:
					return VendorDocAddressRequirement;
				case DocAddressType.Manufacturer:
					return ManufacturerRequirement;
				case DocAddressType.AdditionalDeliveryAddress:
					return AdditionalDeliveryAddressRequirement;
				case DocAddressType.AdditionalConsignee:
					return AdditionalConsigneeRequirement;
				case DocAddressType.OGDProcessInspectionLPCO:
					return OGDProcessInspectionLPCORequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			ZValidation result;
			switch (JE_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					result = new ExportJobDocAddressValidation(addressToValidate, this);
					break;
				case JobMessageTypeList.Codes.Import:
					result = new ImportJobDocAddressValidation(addressToValidate, this);
					break;
				default:
					result = base.PiggyBackedDocAddressValidation(addressToValidate);
					break;
			}
			return result;
		}

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				var result = new List<DocAddressType>(base.SupportedAddressTypesCore);
				if (IsImport)
				{
					result.Add(DocAddressType.CommercialInvoiceOriginator);
					result.Add(DocAddressType.Manufacturer);
					result.Add(DocAddressType.AdditionalDeliveryAddress);
					result.Add(DocAddressType.AdditionalConsignee);
					result.Add(DocAddressType.OGDProcessInspectionLPCO);
					result.Add(DocAddressType.CFIAPaymentParty);
					result.Add(DocAddressType.ImporterOfRecord);
				}
				else if (IsExport)
				{
					result.Add(DocAddressType.SellingParty);
				}
				else if (IsB2Adjustments || IsB3X)
				{
					result.Remove(DocAddressType.SupplierDocumentaryAddress);
					result.Remove(DocAddressType.SupplierPickupDeliveryAddress);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Public Overrides

		#region Methods

		void DefaultJI_OA_ConsigneeAddressFromInvoice()
		{
			foreach (var invoice in Invoices.Cast<JobComInvoiceHeader>())
			{
				var invoiceFinalConsigneeAddressPK = invoice?.FinalConsigneeAddress?.RealAddress?.PK ?? ZGuid.Empty;
				if (invoiceFinalConsigneeAddressPK.IsValid)
				{
					foreach (var invoiceLine in invoice.InvoiceLines.Cast<JobComInvoiceLine>())
					{
						invoiceLine.DefaultJI_OA_ConsigneeAddress(ZGuid.Empty, invoiceFinalConsigneeAddressPK);
					}
				}
			}
		}

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public override object GetService(Type serviceType)
		{
			return serviceType == typeof(ICustomsCharges) ? new JobDeclarationCustomsCharges(this) : base.GetService(serviceType);
		}

		protected override JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new CAJobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected override void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration newDeclaration, CloneType cloneType)
		{
			base.CleanUpNewDeclarationAfterCloneCore(newDeclaration, cloneType);
			var declaration = (JobDeclaration)newDeclaration;
			declaration.CA_TransportDocumentNumber = ZString.Empty;
			declaration.UpdateCA_B3AutoSend();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!InSendingMessageProcess && HasChanges && IsInDatabase && IsB2OrIM2OrB3X && fMessageInitiator != null)
			{
				var factory = new BusinessObjectFactory();
				var declarationInDB = factory.Load<JobDeclaration>(PK);
				if (declarationInDB != null && (declarationInDB.CA_B2SubmissionDate.IsValid || declarationInDB.CA_ConfirmedDate.IsValid || declarationInDB.CA_B2AcceptedDate.IsValid))
				{
					var caption = Res.GetString("A759083B-58D2-4876-895F-1604475D2391", "Save {0} declaration", JE_MessageType);
					var message = Res.GetString("294D6DE5-268A-45BE-91CC-2CC9D5D630BA", @"This {0} has been already submitted for Review – hence no changes are allowed to the data that is printed on the {0}.
 If the current changes are of the administrative nature, something that would NOT cause the reprint of the {0} to produce different results, please proceed with saving, otherwise please do not save these changes", JE_MessageType);
					MessageInitiator.WarnUserAboutSomething(message, caption);
				}
			}
			else if (!IsIM2)
			{
				if (ReleaseEntryHeader is CusEntryHeader entryHeader)
				{
					foreach (CargoControlNumber ccn in CargoControlNumbers)
					{
						if (ccn.HasChanges || !entryHeader.IsInDatabase)
						{
							CheckForForwardedManifestByCCN(ccn.CY_CargoControlNumber, entryHeader);
						}
					}
				}

				if (AVSEventRequired)
				{
					AddServiceRequestedEvent();
					RecalculateConsolidatedAVSStatus();
				}
			}
			AddSubmitAndDecideEventIfNeeded();
		}

		void CheckForForwardedManifestByCCN(ZString ccn, CusEntryHeader entryHeader)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ACIHouseBill);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, ACIForwarderReceivedMessageTypes.Codes.ManifestForward);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			foreach (EDIMessage match in Factory.Load<EDIMessage>(query))
			{
				var ccnList = new List<ZString>(from CargoControlNumber cnn in CargoControlNumbers where !cnn.CY_CargoControlNumber.IsEmpty select cnn.CY_CargoControlNumber);
				if (!ccnList.Contains(match.CargoControlNumber))
				{
					match.EM_LinkedObject = null;
				}
			}

			query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ACIHouseBill);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, ACIForwarderReceivedMessageTypes.Codes.ManifestForward);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAACI);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, ccn.Replace(" ", ""));
			query.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Queued);
			query.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);
			query.IncludeBlob(EDIMessageSchema.EM_MessageText);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Like, string.Format(CultureInfo.CurrentCulture, "%RFF+AFM:%:{0}'%", SecondaryNotifyPartyTypeList.Codes.CustomsBroker));
			foreach (EDIMessage match in Factory.Load<EDIMessage>(query))
			{
				if (match.EM_LinkedObject == null || match.EM_LinkedObject.PK != entryHeader.PK)
				{
					match.EM_LinkedObject = entryHeader;
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!IsIM2 && saveSucceeded)
			{
				AVSEventRequired = false;
				InSendingMessageProcess = false;
				RefreshValidationBeforeSendMessage = false;
			}
		}

		protected override bool RequiresOrderNumbersOnDocsCore()
		{
			return !IsLVS && !IsB2OrIM2OrB3X && base.RequiresOrderNumbersOnDocsCore();
		}

		protected override bool RequiresOrderTrackLinkCore()
		{
			return !IsLVS && !IsB2OrIM2OrB3X && base.RequiresOrderTrackLinkCore();
		}

		#region B3LateSendingWarningEvent

		public void CancelAllSystemB3LateSendingWarningEvent()
		{
			foreach (var log in Logs.Find(l => l.SL_SE_NKEvent == Events.CanadianCADLateSendingWarningCode
				&& l.SL_GS_NKUser == User.ServiceUserCode && !l.IsCancelled))
			{
				log.Cancel();
			}
		}

		void ReSetB3LateSendingWarningEvent()
		{
			var b3SendingStrategy = JobDeclarationB3SendingStrategy.GetJobDeclarationB3SendingStrategy(this);
			var shouldLogEvent = b3SendingStrategy?.ShouldAddB3LateSendingWarningEvent ?? ZBool.False;
			if (shouldLogEvent)
			{
				var sentLog = GetLatestCanadianB3LateWarningSentEvent();
				if (sentLog == null || sentLog.SL_PostedTimeUtc < ZDateTime.UtcNow.AddHours(-24))
				{
					var scheduleDate = b3SendingStrategy?.B3LateSendingWarningScheduleDate ?? ZDateTime.Empty;
					if (scheduleDate > ZDateTime.Today.AddMonths(-2))
					{
						using (DisposableEnvironment.ForBranch(RegistryBranchPK))
						{
							var log = Logs.CreateRecreateOrUpdateEventLog(Events.CanadianCADLateSendingWarning, EstimateActual.Estimate, scheduleDate.ToDateTimeOffset(Branch.HomePort), B3LateSendingWarningLogSubscriber.B3WarningComment);
							if (log != null && !log.IsInDatabase)
							{
								using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
								{
									log.SL_GS_NKUser = User.ServiceUserCode;
								}
							}
						}
					}
				}
			}
			else
			{
				CancelAllSystemB3LateSendingWarningEvent();
			}
		}

		StmALog GetLatestCanadianB3LateWarningSentEvent()
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Table, JobDeclaration.Schema.TableName);
			query.AddToFilter(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CanadianCADLateWarningSentCode);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";

			return Factory.LoadTop1<StmALog>(query);
		}

		#endregion

		#region AVS

		void AddServiceRequestedEvent()
		{
			Logs.AddNew(AutoEvents.ServiceRequested, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.AVSQuery));
		}

		public void RecalculateConsolidatedAVSStatus()
		{
			var avsStatus = AVSStatusList.Codes.Unknown;
			var invoiceLines = InvoiceLines.Cast<JobComInvoiceLine>();
			if (!invoiceLines.Any() || invoiceLines.All(l => l.CA_OGDStatus == AVSStatusList.Codes.Blank))
			{
				avsStatus = AVSStatusList.Codes.Blank;
			}
			else
			{
				foreach (var status in aVSStatusInSequence)
				{
					if (invoiceLines.Any(l => l.CA_OGDStatus == status))
					{
						avsStatus = status;
						break;
					}
				}
			}

			if (avsStatus == AVSStatusList.Codes.Unknown)
			{
				if (invoiceLines.All(l => l.CA_OGDStatus == AVSStatusList.Codes.Blank || l.CA_OGDStatus.StartsWith("OK", StringComparison.Ordinal)))
				{
					avsStatus = AVSStatusList.Codes.WillBeApproved;
				}
			}

			CA_OGDStatus = avsStatus;
		}

		readonly string[] aVSStatusInSequence = new string[]
		{
			AVSStatusList.Codes.Rejected,
			AVSStatusList.Codes.NotImport,
			AVSStatusList.Codes.InspectionRequired,
			AVSStatusList.Codes.ReviewRequired,
			AVSStatusList.Codes.NotValidated
		};

		#endregion

		void AddSubmitAndDecideEventIfNeeded()
		{
			if (IsInDatabase)
			{
				var addInfo = GetAddInfo();
				if (addInfo.HasChangesSinceLastSaving(CAAddInfoSchema.CA_B2SubmissionDate))
				{
					if (CA_B2SubmissionDate.IsEmpty)
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=SUBMITTED CANCELLED|TYP=B2", ((ZDateTime)addInfo.GetOriginalValue(CAAddInfoSchema.CA_B2SubmissionDate)).ToOffset());
					}
					else
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=SUBMITTED|TYP=B2", CA_B2SubmissionDate.ToOffset());
					}
				}

				if (addInfo.HasChangesSinceLastSaving(CAAddInfoSchema.CA_B2AcceptedDate))
				{
					if (CA_B2AcceptedDate.IsEmpty)
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=DECIDED CANCELLED|TYP=B2", ((ZDateTime)addInfo.GetOriginalValue(CAAddInfoSchema.CA_B2AcceptedDate)).ToOffset());
					}
					else
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=DECIDED|TYP=B2", CA_B2AcceptedDate.ToOffset());
					}
				}

				if (addInfo.HasChangesSinceLastSaving(CAAddInfoSchema.CA_ConfirmedDate))
				{
					if (CA_ConfirmedDate.IsEmpty)
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=CONFIRMED CANCELLED|TYP=B2", ((ZDateTime)addInfo.GetOriginalValue(CAAddInfoSchema.CA_ConfirmedDate)).ToOffset());
					}
					else
					{
						Logs.AddNew(AutoEvents.StatusUpdated, "|RES=CONFIRMED|TYP=B2", CA_ConfirmedDate.ToOffset());
					}
				}
			}
		}

		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			if (transportMode == Core.Constants.TransportModes.Road)
			{
				switch (shipmentPackingMode)
				{
					case Core.Constants.ContainerModes.FCL:
					case Core.Constants.ContainerModes.LCL:
					case Core.Constants.ContainerModes.BuyersConsol:
						result = Core.Constants.ContainerModes.Containerised;
						break;
					case Core.Constants.ContainerModes.FTL:
					case Core.Constants.ContainerModes.LTL:
						result = Core.Constants.ContainerModes.BreakBulk;
						break;
				}
			}
			else if (transportMode == Core.Constants.TransportModes.Rail)
			{
				switch (shipmentPackingMode)
				{
					case Core.Constants.ContainerModes.FCL:
					case Core.Constants.ContainerModes.LCL:
					case Core.Constants.ContainerModes.BuyersConsol:
						result = Core.Constants.ContainerModes.Containerised;
						break;
				}
			}
			else
			{
				result = base.GetContainerModeForDeclarationCore(transportMode, shipmentPackingMode);
			}

			return result;
		}

		protected override ZString GetContainerModeForImportDeclaration(ZString shipmentPackingMode)
		{
			return (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.Bulk ||
				shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.Liquid ||
				shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.BreakBulk ||
				shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff) ? shipmentPackingMode : base.GetContainerModeForImportDeclaration(shipmentPackingMode);
		}

		#endregion

		#region Properties

		#region Booleans

		public override ZBool IsImport
		{
			get { return base.IsImport || IsLVS || IsIM2; }
		}

		public ZBool IsImportIncludingB2
		{
			get { return IsImport || IsB2Adjustments || IsB3X; }
		}

		public override bool AlwaysUseClassificationDescription
		{
			get
			{
				return base.AlwaysUseClassificationDescription
					|| CA_MergeBy == B3MergeByList.Codes.ClassificationLookupUsingClassificationDescriptionAlways;
			}
		}

		public override bool PrefixPartDescriptionWithPartNumber
		{
			get
			{
				return base.PrefixPartDescriptionWithPartNumber
					|| CA_MergeBy == B3MergeByList.Codes.NotMergeUsingProductNumberInDescription
					|| CA_MergeBy == B3MergeByList.Codes.ProductNumberUsingProductNumberInDescription;
			}
		}

		public ZBool SuppressShipmentRelatedFields
		{
			get
			{
				if (suppressShipmentRelatedFieldsHasBeenSet)
				{
					return suppressShipmentRelatedFields;
				}
				else if (!IsStandAlone)
				{
					return false;
				}

				return CACustomsDataRegistry.Instance.SuppressShipmentRelatedFields.Value;
			}
			set
			{
				suppressShipmentRelatedFields = value;
				suppressShipmentRelatedFieldsHasBeenSet = true;
				SuppressShipmentRelatedFieldsInfo.RefreshBinding();
			}
		}
		ZBool suppressShipmentRelatedFields;
		ZBool suppressShipmentRelatedFieldsHasBeenSet;

		public ZPropertyInfo SuppressShipmentRelatedFieldsInfo => GetZPropertyInfo(nameof(SuppressShipmentRelatedFields));

		#endregion

		#region CA_UseImporterAccountSecurityNumber

		public override ZBool CA_UseImporterAccountSecurityNumber
		{
			get { return base.CA_UseImporterAccountSecurityNumber; }
			set
			{
				ZBool oldValue = CA_UseImporterAccountSecurityNumber;
				base.CA_UseImporterAccountSecurityNumber = value;
				if (CA_UseImporterAccountSecurityNumber != oldValue && IsImportIncludingB2)
				{
					TransactionNumber.SetAccountSecurityNo();
				}
			}
		}

		#endregion

		#region CA_OriginalTransactionNo

		[ReadOnlyMember(nameof(CA_OriginalTransactionNo_ReadOnly))]
		public override ZString CA_OriginalTransactionNo
		{
			get { return IsBlanketB2 ? (ZString)"VAR" : base.CA_OriginalTransactionNo; }
			set
			{
				var oldValue = base.CA_OriginalTransactionNo;
				base.CA_OriginalTransactionNo = value;
				if (oldValue != value)
				{
					DefaultB2FromB3IfNeeded();
					originalDeclaration = null;
				}
			}
		}

		bool CA_OriginalTransactionNo_ReadOnly
		{
			get { return IsBlanketB2; }
		}

		#endregion

		#region DeclarationNumber

		public override ZString DeclarationNumber
		{
			get { return IsImportIncludingB2 ? TransactionNumber : base.DeclarationNumber; }
		}

		#endregion

		#region JE_DateOfArrival

		public override ZDateTime JE_DateOfArrival
		{
			get { return base.JE_DateOfArrival; }
			set
			{
				var oldValue = base.JE_DateOfArrival;
				base.JE_DateOfArrival = value;
				if (!IsCopying && JE_DateOfArrival != oldValue)
				{
					CalculateEstimatedPaymentDueDate();
				}
			}
		}

		protected override void UpdateDateOfFirstArrivalIfPossible()
		{
			if (IsPARS && !IsDataSyncFromShipment && JE_DateOfArrival.IsValid && IsFirstArrivalDateAndPortUsed)
			{
				if (JE_RL_NKPortOfFirstArrival == JE_RL_NKPortOfArrival && !JE_RL_NKPortOfArrival.IsEmpty && !JE_RL_NKPortOfFirstArrival.IsEmpty && JE_DateOfFirstArrival != JE_DateOfArrival)
				{
					JE_DateOfFirstArrival = JE_DateOfArrival;
				}
			}
		}

		#endregion

		#region JE_DateAtFinalDestination

		public override ZDateTime JE_DateAtFinalDestination
		{
			get { return base.JE_DateAtFinalDestination; }
			set
			{
				var oldValue = JE_DateAtFinalDestination;
				base.JE_DateAtFinalDestination = value;
				if (!IsCopying && JE_DateAtFinalDestination != oldValue)
				{
					CalculateEstimatedPaymentDueDate();
				}
			}
		}

		#endregion

		#region JE_MasterBill

		public override ZString JE_MasterBill
		{
			get { return base.JE_MasterBill; }
			set
			{
				var oldValue = base.JE_MasterBill;
				base.JE_MasterBill = value;
				if (JE_MasterBill != oldValue)
				{
					UpdateTransportDocumentNumberIfApplicable(true);
				}
			}
		}

		#endregion

		#region JE_MergeBy

		[ReadOnlyMember(nameof(JE_MergeBy_ReadOnly))]
		public override ZString JE_MergeBy
		{
			get => base.JE_MergeBy;
			set => base.JE_MergeBy = value;
		}

		bool JE_MergeBy_ReadOnly
		{
			get { return IsImport; }
		}

		#endregion

		#region JE_HouseBill

		public override ZString JE_HouseBill
		{
			get { return base.JE_HouseBill; }
			set
			{
				var oldValue = base.JE_HouseBill;
				base.JE_HouseBill = value;
				if (JE_HouseBill != oldValue)
				{
					UpdateTransportDocumentNumberIfApplicable(false);
				}
			}
		}

		#endregion

		#region JE_TransportMode

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				var oldValue = base.JE_TransportMode;
				base.JE_TransportMode = value;
				if (JE_TransportMode != oldValue)
				{
					UpdateTransportDocumentNumberIfApplicable(false);
					JE_ContainerMode = ZString.Empty;
					if (IsImport && !IsCopying)
					{
						DefaultPortOfClearanceAndSubLocationCode();
					}
				}
			}
		}

		#endregion

		#region JE_GB

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				var oldValue = base.JE_GB;
				base.JE_GB = value;
				if (!IsCopying && oldValue != JE_GB)
				{
					DefaultPortOfClearanceAndSubLocationCode();
					SetupNonPersistentProperties();
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				bool hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public ZString CA_DeclarationExceptionDescription
		{
			get { return AddInfoLookups.CAExceptionCodeList.GetDescriptionFromCode(CA_DeclarationException); }
		}

		public ZPropertyInfo CA_DeclarationExceptionDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CA_DeclarationExceptionDescription); }
		}

		#region JE_MessageType

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);
			UpdateTransportDocumentNumberIfApplicable(false);
			UpdateMergeBy();
			UpdateMessageSubType();
			UpdateAssesmentOption();
			UpdateTransportMode();
			UpdateEntryAuthorisationDate();
			DefaultPortOfClearanceAndSubLocationCode();
			DefaultExportServiceProviderIfRequired();
			UpdateCA_B3AutoSend();
			UpdatePlaceOfReport();
			UpdateAVSEventRequired();
			DefaulTotalNoOfPacksPackTypeIfRequired();
			UnsetCA_CSAEntry();

			Packages.MarkAsNeedingValidation();

			var importerAddInfo = ImporterAddInfo;
			if (IsImportIncludingB2
				&& importerAddInfo != null
				&& importerAddInfo.HasAccountSecurityNumber
				&& TransactionNumber.CanChange)
			{
				TransactionNumber.SetAccountSecurityNo();
			}
		}

		void UpdateTransportDocumentNumberIfApplicable(bool overrideEvenIfEmpty)
		{
			if (!IsCopying && IsExport && IsAir && !ShouldSynchroniseWithShipment() && (overrideEvenIfEmpty || CA_TransportDocumentNumber.IsEmpty))
			{
				SetBillTransportDocumentNumber(JE_MasterBill.IsEmpty ? JE_HouseBill : JE_MasterBill);
			}
		}

		internal void SetBillTransportDocumentNumber(ZString bill)
		{
			CA_TransportDocumentNumber = FormatAirwayBillIfApplicable(bill);
		}

		internal ZString FormatAirwayBillIfApplicable(ZString value)
		{
			var builder = new ZStringBuilder();
			if (IsAir && value.Length > 3)
			{
				var isAlreadyFormatted = value[3] == '-';
				if (!isAlreadyFormatted)
				{
					builder.Append(value.Substring(0, 3));
					builder.Append(value.Substring(3));
				}
			}
			else
			{
				builder.Append(value);
			}
			return builder.ToStringWithDelimiterBetweenAppends("-");
		}

		internal void UpdateMergeBy()
		{
			if (IsImport)
			{
				JE_MergeBy = IsConsolidatedLVS ? JobMessageTypeList.Codes.LowValueShipments : OrgConstants.MergeInvoiceLines.NotMerge;
				CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
			}
			else
			{
				JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				CA_MergeBy = ZString.Empty;
			}
		}

		void UpdateMessageSubType()
		{
			JE_MessageSubType = IsLVS ? LowValueShipmentsTypes.Codes.TotalConsolidation
									: IsImport ? B3EntryTypeList.Codes.Confirming : string.Empty;
		}

		void UpdateAssesmentOption()
		{
			CA_AssesmentOption = IsImport ? AssessmentOptions.Codes.AppraisalQualityData : string.Empty;
		}

		void UpdateTransportMode()
		{
			if (Shipment == null || JE_OverrideFreightDefaults)
			{
				JE_TransportMode = IsLVS ? TransportTypeList.Codes.Road : string.Empty;
			}
		}

		void UpdateEntryAuthorisationDate()
		{
			JE_EntryAuthorisationDate = IsLVS ? LastDayOfCurrentMonth : ZDateTime.Empty;
		}

		void UpdateCA_B3AutoSend()
		{
			if ((JE_MessageType == JobMessageTypeList.Codes.Import || JE_MessageType == JobMessageTypeList.Codes.LowValueShipments)
			&& CACustomsDataRegistry.Instance.ActivateAutoB3Sending.Value)
			{
				CA_B3AutoSend = true;
			}
			else
			{
				CA_B3AutoSend = false;
			}
		}

		void UpdatePlaceOfReport()
		{
			if (!IsExport)
			{
				CA_PlaceOfReport = ZString.Empty;
			}
		}

		void UnsetCA_CSAEntry()
		{
			CA_CSAEntry = false;
		}

		void UpdateAVSEventRequired()
		{
			AVSEventRequired = JE_MessageType == JobMessageTypeList.Codes.Import
				&& InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsRegulatedByCFIA || l.IsRegulatedByIIDCFIA);
			CA_OGDStatus = AVSStatusList.Codes.Blank;
		}

		[ResourceStringData("Enterprise.Customs.CA.Business.JobDeclaration|JE_TotalWeightUnit", Caption = "Total Weight UQ", ShortCaption = "Weight")]
		public override ZString JE_TotalWeightUnit { get => base.JE_TotalWeightUnit; set => base.JE_TotalWeightUnit = value; }

		[ResourceStringData("Enterprise.Customs.CA.Business.JobDeclaration|JE_TotalNoOfPacks", Caption = "No. Packages", ShortCaption = "Packages", FullDescription = "Enter the total number of outer packages for this shipment, as distinct from the number of units. The total number of outer packages is what the goods are packed into and the type of packaging in which the goods are contained or wrapped. Example: Drums, Barrels, Pallets etc.")]
		public override ZInt JE_TotalNoOfPacks { get => base.JE_TotalNoOfPacks; set => base.JE_TotalNoOfPacks = value; }

		void DefaulTotalNoOfPacksPackTypeIfRequired()
		{
			if (IsLVS)
			{
				JE_TotalNoOfPacksPackType = ZString.Empty;
			}
		}

		public bool NeedValidateMessageType { get; set; }

		#endregion

		#region JE_MessageSubType

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageSubTypeList))]
		public override ZString JE_MessageSubType
		{
			get
			{
				return CADEntryTypeList.ConvertToLongCode(base.JE_MessageSubType);
			}
			set
			{
				var oldValue = JE_MessageSubType;
				base.JE_MessageSubType = CADEntryTypeList.ConvertToShortCode(value);
				if (!IsCopying && oldValue != JE_MessageSubType)
				{
					if (IsLVS)
					{
						var shouldResetBuyer = JE_MessageSubType != LowValueShipmentsTypes.Codes.TotalConsolidation;
						var shouldResetLineCalculationMethod = oldValue == LowValueShipmentsTypes.Codes.ConsolidationByImporter;

						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							if (shouldResetBuyer)
							{
								invoice.JZ_OH_Buyer = Invoices.IsAdditionalInvoice(invoice) ? JE_OH_Importer : ZGuid.Empty;
							}
							if (shouldResetLineCalculationMethod)
							{
								invoice.ResetLineCalculationMethod();
							}

							invoice.JZ_OH_BuyerInfo.RefreshBinding();

							foreach (JobComInvoiceLine invoiceLine in invoice.InvoiceLines)
							{
								invoiceLine.CA_AuthorityNumberInfo.RefreshBinding();
							}
						}

						if (IsLVSTotalConsolidation)
						{
							CA_AllowOIC = false;
						}
					}
					ReSetB3LateSendingWarningEvent();
					UpdateCA_AccountingAge();
				}
			}
		}

		#endregion

		#region JE_RL_NKPortOfLoading

		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set
			{
				var oldValue = JE_RL_NKPortOfLoading;
				base.JE_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfLoading)
				{
					UpdateCanadianExportPortsIfApplicable();
				}
			}
		}

		void UpdateCanadianExportPortsIfApplicable()
		{
			if (IsExport)
			{
				if (CA_PortOfExit.IsEmpty)
				{
					CA_PortOfExit = GetPortOfficeFromUnLoco(PortOfLoading);
				}

				if (CA_PlaceOfReport.IsEmpty)
				{
					CA_PlaceOfReport = GetPortOfficeFromUnLoco(PortOfLoading);
				}
			}
		}

		public ZString GetPortOfficeFromUnLoco(RefUNLOCO unloco)
		{
			return Factory.GetCachedValue($"GetPortOfficeFromUnLoco|{unloco?.PK ?? ZGuid.Empty}", () =>
				{
					if (unloco != null && unloco.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Canada && unloco.CountryStates != null)
					{
						var unlocoPort = unloco.RL_PortName.Replace("St ", "St. ").Replace("Saint-", "St. ").Replace("Saint ", "St. ");
						var unlocoProvince = unloco.CountryStates.RW_Code;
						var offices = AddInfoLookups.CBSAOffices;

						if (!offices.IsLoaded)
						{
							offices.Load();
						}

						var officeCode = offices.Where(x => x.ZZD_Description == unlocoPort && x.GetAttribute(RefCusCodeListAttributes.Province) == unlocoProvince).FirstOrDefault();

						if (officeCode != null)
						{
							return officeCode.ZZD_Code.SubstringSafe(0, UniversalReferenceConstants.CBSAOfficeCodeMaxLength);
						}
					}

					return ZString.Empty;
				});
		}

		#endregion

		#region JE_RL_NKPortOfArrival

		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set
			{
				bool hasChanges = base.JE_RL_NKPortOfArrival != value;
				base.JE_RL_NKPortOfArrival = value;
				if (IsImport && !IsCopying && hasChanges)
				{
					DefaultPortOfClearanceAndSubLocationCode();
				}
				if (JE_RL_NKPortOfFirstArrival.IsEmpty)
				{
					JE_RL_NKPortOfFirstArrival = value;
				}
			}
		}

		#endregion

		public override bool UseImporterAddress => true;

		#region JE_OH_Importer
		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				var oldValue = JE_OH_Importer;
				base.JE_OH_Importer = value;
				var newValue = JE_OH_Importer;
				if (!IsCopying && newValue != oldValue)
				{
					if (IsImportIncludingB2 && !ImporterOfRecordAddress.HasRealOrganisation)
					{
						TransactionNumber.SetAccountSecurityNo();
					}
					if (IsLVS && JE_MessageSubType != LowValueShipmentsTypes.Codes.TotalConsolidation)
					{
						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							if (Invoices.IsAdditionalInvoice(invoice))
							{
								invoice.JZ_OH_Buyer = newValue;
							}
						}
					}
					if (IsImport)
					{
						BondDetailsDefaulter.Default(this, CA_BondType);
					}
				}
			}
		}

		protected override ZBool PreventBuyerSupplierRelationshipsCore
		{
			get { return IsConsolidatedLVS || IsB2Adjustments || IsB3X; }
		}

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (IsLVS)
			{
				if (!fIsImportingData)
				{
					DefaultImporterDocAddresses(newValue);
					if (Importer != null)
					{
						RefreshAllInvoiceLinesPartSyncManagers();
					}
				}
				DefaultValuesFromLocalPartyWhenEntered(LocalParty);
			}
			else if (IsB2Adjustments && IsCSAApprovedImporter)
			{
				JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				CA_B2Type = B2TypeList.Codes.Specific;
				JE_PaymentMethod = ZString.Empty;
			}
			else if (IsB3X && !IsCSAApprovedImporter)
			{
				JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			}
			else
			{
				base.JE_OH_ImporterChanged(oldValue, newValue);
			}

			UnsetCA_CSAEntry();
			DefaultValueforDutyCodeFromSupplierImporterLink();
		}

		public OrgHeader LVSImporter
		{
			get
			{
				if (LVXInvoiceHeader is JobComInvoiceHeader invoice && !invoice.JZ_OH_Buyer.IsEmpty)
				{
					return invoice.Buyer;
				}
				return Importer;
			}
		}

		public OrgHeader LVSSupplier
		{
			get
			{
				if (LVXInvoiceHeader is JobComInvoiceHeader invoice && !invoice.JZ_OH_Supplier.IsEmpty)
				{
					return invoice.Supplier;
				}
				return Supplier;
			}
		}

		#region B2/B3X/IM2 fields
		[ReadOnlyMember(nameof(B3XFieldsReadOnly))]
		public override ZString CA_B2Type { get => base.CA_B2Type; set => base.CA_B2Type = value; }

		protected bool B3XFieldsReadOnly
		{
			get { return IsB3X; }
		}

		#region CA_B2SubmissionDate

		[ResourceStringData("CAAddInfo|CA_B2SubmissionDate", Caption = "B2 Submission Date")]
		[ReadOnlyMember(nameof(CA_B2SubmissionDate_ReadOnly))]
		public override ZDateTime CA_B2SubmissionDate
		{
			get { return base.CA_B2SubmissionDate; }
			set { base.CA_B2SubmissionDate = value; }
		}

		bool CA_B2SubmissionDate_ReadOnly
		{
			get { return B3XFieldsReadOnly && !CA_B2SubmissionDateOverride; }
		}

		public ZBool CA_B2SubmissionDateOverride
		{
			get
			{
				if (b2SubmissionDateOverride)
				{
					return b2SubmissionDateOverride;
				}
				else
				{
					if (b2SubmissionDateOverrideCache == null)
					{
						b2SubmissionDateOverrideCache = new CachedProperty<ZBool>(Factory, delegate
						{
							if (IsB3X)
							{
								var submitedDate = this.CA_B2SubmissionDate;
								var lastSendMessage = this.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.XTypeEntry).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
								return !submitedDate.IsEmpty && (lastSendMessage?.EM_SystemCreateTimeUtc.Date ?? ZDate.Empty) != submitedDate.Date;
							}
							else
							{
								return false;
							}
						});
					}
					return b2SubmissionDateOverrideCache.Value;
				}
			}
			set
			{
				var hasChanges = CA_B2SubmissionDateOverride != value;
				SetNonPersistentPropertyValue(CA_B2SubmissionDateOverrideInfo, ref b2SubmissionDateOverride, value);
				if (hasChanges && !value)
				{
					var lastSendMessage = this.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.XTypeEntry).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
					this.CA_B2SubmissionDate = lastSendMessage?.EM_SystemCreateTimeUtc.Date ?? ZDate.Empty;
				}
				b2SubmissionDateOverrideCache = null;
			}
		}
		ZBool b2SubmissionDateOverride;
		CachedProperty<ZBool> b2SubmissionDateOverrideCache;

		public ZPropertyInfo CA_B2SubmissionDateOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.CA_B2SubmissionDateOverride); }
		}
		#endregion

		#region CA_B2AcceptedDate

		[ReadOnlyMember(nameof(CA_B2AcceptedDate_ReadOnly))]
		public override ZDateTime CA_B2AcceptedDate
		{
			get => base.CA_B2AcceptedDate;
			set => base.CA_B2AcceptedDate = value;
		}

		bool CA_B2AcceptedDate_ReadOnly
		{
			get { return B3XFieldsReadOnly && !CA_B2AcceptedDateOverride; }
		}

		public ZBool CA_B2AcceptedDateOverride
		{
			get
			{
				if (b2AcceptedDateOverride)
				{
					return b2AcceptedDateOverride;
				}
				else
				{
					if (b2AcceptedDateOverrideCache == null)
					{
						b2AcceptedDateOverrideCache = new CachedProperty<ZBool>(Factory, delegate
						{
							if (IsB3X)
							{
								var acceptedDate = this.CA_B2AcceptedDate;
								var lastRecievedMessage = this.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => !x.IsTransmitMessage
								&& x.EM_MessageType == MessageTypeList.Codes.XTypeEntry
								&& x.EM_MessageSubType == B3EntryStatusList.Codes.Accepted).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
								return !acceptedDate.IsEmpty && (lastRecievedMessage?.EM_SystemCreateTimeUtc.Date ?? ZDate.Empty) != acceptedDate.Date;
							}
							else
							{
								return false;
							}
						});
					}
					return b2AcceptedDateOverrideCache.Value;
				}
			}
			set
			{
				var hasChanges = CA_B2AcceptedDateOverride != value;
				SetNonPersistentPropertyValue(CA_B2AcceptedDateOverrideInfo, ref b2AcceptedDateOverride, value);
				if (hasChanges && !value)
				{
					var lastRecievedMessage = this.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(x => !x.IsTransmitMessage
								 && x.EM_MessageType == MessageTypeList.Codes.XTypeEntry
								&& x.EM_MessageSubType == B3EntryStatusList.Codes.Accepted).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
					this.CA_B2AcceptedDate = lastRecievedMessage?.EM_SystemCreateTimeUtc.Date ?? ZDate.Empty;
				}
				b2AcceptedDateOverrideCache = null;
			}
		}
		ZBool b2AcceptedDateOverride;
		CachedProperty<ZBool> b2AcceptedDateOverrideCache;

		public ZPropertyInfo CA_B2AcceptedDateOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.CA_B2AcceptedDateOverride); }
		}
		#endregion

		public bool IsBlanketB2
		{
			get { return (IsB2Adjustments || IsB3X) && CA_B2Type == B2TypeList.Codes.Blanket; }
		}

		#endregion

		protected override void RefreshAllInvoiceLinesPartSyncManagers()
		{
			if (!IsB2Adjustments && !IsB3X)
			{
				base.RefreshAllInvoiceLinesPartSyncManagers();
			}
		}

		#region Implementation

		void DefaultValueforDutyCodeFromSupplierImporterLink()
		{
			if (Importer != null && !fIsImportingData && SupplierImporterLink != null && SupplierImporterLink.OL_ValuationBasis.IsValid)
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (invoice.CA_ValueForDutyCode.IsEmpty)
					{
						invoice.CA_ValueForDutyCode = SupplierImporterLink.OL_ValuationBasis.Left(2);
						invoice.CA_ValueForDutyCodeInfo.RefreshBinding();
					}
				}
			}
		}

		protected override ZAddress GetNewJE_OA_ImporterAddress_ZAddress()
		{
			ZAddress result = base.GetNewJE_OA_ImporterAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#endregion

		public override bool UseSupplierAddress => true;

		#region JE_OH_Supplier

		protected override void MarkInvoiceLinesAsNeedingValidationOnJE_OH_SupplierChanged()
		{
			InvoiceLines.MarkAsNeedingValidation();
		}

		[ReadOnlyMember(nameof(JE_OH_Supplier_ReadOnly))]
		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set => base.JE_OH_Supplier = value;
		}

		bool JE_OH_Supplier_ReadOnly
		{
			get { return IsLVS; }
		}

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_SupplierChanged(oldValue, newValue);
			DefaultExportServiceProviderIfRequired();
			DefaultValueforDutyCodeFromSupplierImporterLink();
		}

		protected override ZAddress GetNewJE_OA_SupplierAddress_ZAddress()
		{
			ZAddress result = base.GetNewJE_OA_SupplierAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#region JE_EntryAuthorisationDate

		[ReadOnlyMember(nameof(JE_EntryAuthorisationDateReadOnly))]
		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return base.JE_EntryAuthorisationDate; }
			set
			{
				var hasChanged = base.JE_EntryAuthorisationDate != value;
				base.JE_EntryAuthorisationDate = value;
				if (hasChanged && !IsCopying)
				{
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
					if (!IsIM2)
					{
						if (ReleaseEntryHeader is CusEntryHeader entryHeader &&
							!((IEDIReleaseMessageAttachee)entryHeader).SettingReleaseDateWithStatusUpdate)
						{
							if (value.IsEmpty)
							{
								CancelCustomsClearedEvent();
								StatusLogManager.CancelCustomsClearedEvent(entryHeader.Logs);
								if (!JE_MessageTypeInfo.HasChanges && entryHeader.CH_EntryStatus != EDIReleaseImportEntryStatusList.Codes.Cancelled)
								{
									ErrorReporter.ReportOnce("Actual release date should not be cleared once it was released.");
								}
							}
							else
							{
								if (entryHeader.ShouldLogCustomsClearedToDeclarationOrShipment)
								{
									LogCustomsClearedIfNeeded();
								}

								StatusLogManager.AddCustomsClearedEvent(entryHeader.Logs, "MANUAL", value.ToOffset());
							}
						}
						SetupNonPersistentProperties();
						CalculateEstimatedPaymentDueDate();
						ReSetB3LateSendingWarningEvent();
					}
					if (!IsLVX)
					{
						SetDeclarationException();
					}
					UpdateCA_AccountingAge();
				}
			}
		}
		bool JE_EntryAuthorisationDateReadOnly => IsImport && !IsLVS && !IsCSA;

		[MaxLength(2)]
		public ZInt JE_PeriodMonth
		{
			get { return JE_EntryAuthorisationDate.IsValid ? (ZInt)JE_EntryAuthorisationDate.Month : ZInt.Zero; }
			set
			{
				var year = JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.Year : ZDateTime.Today.Year;
				JE_EntryAuthorisationDate = new ZDateTime(year, Math.Max(Math.Min(value, 12), 1), 1);
			}
		}

		public ZPropertyInfo JE_PeriodMonthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_PeriodMonth, x => JE_EntryAuthorisationDateInfo); }
		}

		[MaxLength(4)]
		public ZInt JE_PeriodYear
		{
			get { return JE_EntryAuthorisationDate.IsValid ? (ZInt)JE_EntryAuthorisationDate.Year : ZInt.Zero; }
			set
			{
				var month = JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.Month : ZDateTime.Today.Month;
				var year = Math.Max(Math.Min(value, ZDateTime.MaxSmallDateTimeValue.Year - 1), ZDateTime.MinSmallDateTimeValue.Year);
				JE_EntryAuthorisationDate = new ZDateTime(year, month, 1);
			}
		}

		public ZPropertyInfo JE_PeriodYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_PeriodYear, x => JE_EntryAuthorisationDateInfo); }
		}

		public ZString JE_Period
		{
			get
			{
				return JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.ToString("MM/yyyy", CultureInfo.CurrentCulture) : string.Empty;
			}
		}
		#endregion

		#region JE_CarrierCode

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.JE_CarrierCodeList))]
		public override ZString JE_CarrierCode
		{
			get { return base.JE_CarrierCode; }
			set
			{
				var oldValue = JE_CarrierCode;
				base.JE_CarrierCode = value;
				if (!IsCopying && oldValue != JE_CarrierCode)
				{
					var fCACarrier = new ZZRefCarrierCombined.Loader(Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, value);
					CA_CarrierName = fCACarrier?.ZZ4_Description ?? ZString.Empty;
					DefaultTranportMode(fCACarrier);
				}
				if (this.EffectiveCCN.IsEmpty && JE_MessageType != JobMessageTypeList.Codes.Export)
				{
					this.EffectiveCCNPrefix = value;
				}
			}
		}

		void DefaultTranportMode(ZZRefCarrierCombined fCACarrier)
		{
			if (JE_TransportMode.IsEmpty)
			{
				if (fCACarrier != null && !fCACarrier.ZZ4_Code.IsEmpty)
				{
					var transport = fCACarrier.TransportModePairList.FirstOrDefault(x => x.Value);
					if (transport != null)
					{
						JE_TransportMode = transport.Description;
					}
					else
					{
						var attr = fCACarrier.Attributes.FirstOrDefault(x => x.ZZG_Name == RefTransportModeList.Codes.AIR
						|| x.ZZG_Name == RefTransportModeList.Codes.RAI
						|| x.ZZG_Name == RefTransportModeList.Codes.ROA
						|| x.ZZG_Name == RefTransportModeList.Codes.SEA);
						if (attr != null)
						{
							JE_TransportMode = attr.ZZG_Name.Left(3);
						}
					}
				}
			}
		}

		#endregion

		#region CA_MergeBy

		public override ZString CA_MergeBy
		{
			get { return (IsLVS || !(JE_MessageType == JobMessageTypeList.Codes.Import && IsCADEnabled)) ? base.CA_MergeBy : B3MergeByList.Codes.NotMerge; }
			set
			{
				var oldValue = CA_MergeBy;
				base.CA_MergeBy = value;
				if (!IsCopying && oldValue != CA_MergeBy)
				{
					MarkApportionmentDirty();
				}
			}
		}

		#endregion

		#region CA_OGDCFIA

		public override ZBool CA_OGDCFIA
		{
			get { return base.CA_OGDCFIA; }
			set
			{
				var oldValue = CA_OGDCFIA;
				base.CA_OGDCFIA = value;
				if (!IsCopying && oldValue != CA_OGDCFIA)
				{
					InvoiceLines.RunCFIAValidation();

					if (!IsIID && CA_OGDCFIA)
					{
						var cusCode = EffectiveImporter?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, Core.Constants.CountryCodes.Canada);
						if (cusCode != null)
						{
							foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
							{
								invoiceLine.AddDefaultCFIARegistrationNumber(cusCode);
							}
						}
					}
				}
			}
		}

		#endregion

		#region CA_OGDIC

		public override ZBool CA_OGDIC
		{
			get { return base.CA_OGDIC; }
			set
			{
				var oldValue = CA_OGDIC;
				base.CA_OGDIC = value;
				if (!IsCopying && oldValue != CA_OGDIC)
				{
					InvoiceLines.RunSITTValidation();
				}
			}
		}

		#endregion

		#region CA_OGDNR

		public override ZBool CA_OGDNR
		{
			get { return base.CA_OGDNR; }
			set
			{
				var oldValue = CA_OGDNR;
				base.CA_OGDNR = value;
				if (!IsCopying && oldValue != CA_OGDNR)
				{
					InvoiceLines.RunNRCANValidation();
				}
			}
		}

		#endregion

		#region CA_OGDTC

		public override ZBool CA_OGDTC
		{
			get { return base.CA_OGDTC; }
			set
			{
				var oldValue = CA_OGDTC;
				base.CA_OGDTC = value;
				if (!IsCopying && oldValue != CA_OGDTC)
				{
					InvoiceLines.RunTiresValidation();
				}
			}
		}

		#endregion

		#region CA_CSAEntry

		public override ZBool CA_CSAEntry
		{
			get => base.CA_CSAEntry;
			set
			{
				var hasChanged = base.CA_CSAEntry != value;
				base.CA_CSAEntry = value;
				if (hasChanged && !IsCopying)
				{
					if (value)
					{
						ReSetB3LateSendingWarningEvent();
					}
					UpdateCA_AccountingAge();
				}
			}
		}

		#endregion

		#region JE_LocationOfGoods

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SubLocationCodes))]
		[MaxLength(4)]
		public override ZString JE_LocationOfGoods
		{
			get { return base.JE_LocationOfGoods; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_LocationOfGoods))
				{
					var oldValue = JE_LocationOfGoods;
					base.JE_LocationOfGoods = value;
					if (!IsCopying && oldValue != JE_LocationOfGoods)
					{
						if (CA_ExamLocationCode.IsEmpty)
						{
							CA_ExamLocationCode = value;
						}
						CA_SubLocationName = Lookups.SubLocationCodes.GetDescriptionFromCode(JE_LocationOfGoods);
					}
				}
			}
		}

		public override ZString CA_SubLocationName
		{
			get => base.CA_SubLocationName;
			set
			{
				var oldValue = CA_SubLocationName;
				base.CA_SubLocationName = value;
				if (oldValue != CA_SubLocationName)
				{
					if (CA_ExamLocationName.IsEmpty && CA_ExamLocationCode.IsEmpty)
					{
						CA_ExamLocationName = value.SubstringSafe(0, CAAddInfoSchema.CA_ExamLocationName.MaxLength);
						ExamLocationDescriptionInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region CA_ExamLocationCode

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExamLocationCodes))]
		[MaxLength(4)]
		public override ZString CA_ExamLocationCode
		{
			get { return base.CA_ExamLocationCode; }
			set
			{
				var oldValue = CA_ExamLocationCode;
				base.CA_ExamLocationCode = value;
				ExamLocationDescription = ZString.Empty;
				if (!IsCopying && oldValue != CA_ExamLocationCode)
				{
					ExamLocationDescriptionInfo.RefreshBinding();
				}
			}
		}
		#endregion

		[MaxLength(AutoCAAddInfo.Schema.CA_ExamLocationNameMaxLength)]
		[ReadOnlyMember(nameof(ExamLocationDescription_ReadOnly))]
		public ZString ExamLocationDescription
		{
			get
			{
				if (examLocationName == null)
				{
					examLocationName = new CachedProperty<ZString>(Factory, delegate
					{
						return CA_ExamLocationCode.IsEmpty ? base.CA_ExamLocationName : new ZString(Lookups.ExamLocationCodes.GetDescriptionFromCode(CA_ExamLocationCode));
					});
				}
				return examLocationName.Value;
			}
			set
			{
				CA_ExamLocationName = value;
				Validation.ValidateExamLocationDescription();
				ExamLocationDescriptionInfo.RefreshBinding();
			}
		}
		CachedProperty<ZString> examLocationName;

		public ZPropertyInfo ExamLocationDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ExamLocationDescription); }
		}

		bool ExamLocationDescription_ReadOnly => !CA_ExamLocationCode.IsEmpty;

		#region CA_ServiceOption

		public override ZString CA_ServiceOption
		{
			get { return base.CA_ServiceOption; }
			set
			{
				var oldValue = base.CA_ServiceOption;
				var oldIsIID = IsIID;
				var oldIsOGD = IsOGD;
				base.CA_ServiceOption = value;
				if (CA_ServiceOption != oldValue)
				{
					UpdateDateOfFirstArrivalIfPossible();
					if (Shipment != null && ShouldSynchroniseWithShipment())
					{
						ShipmentSynchroniser.Synchronise(true);
					}
				}

				if (IsIID)
				{
					CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
					CA_OGDCFIA = false;
					CA_OGDIC = false;
					CA_OGDNR = false;
					CA_OGDTC = false;
					if (JE_TotalNoOfPacksPackType == ACROSSPackageTypes.Codes.PACKAGE)
					{
						JE_TotalNoOfPacksPackType = IIDUnitOfCountCodeList.Codes.Pack;
					}
				}
				else if (IsCSA)
				{
					CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
				}
				else if (JE_TotalNoOfPacksPackType == IIDUnitOfCountCodeList.Codes.Pack)
				{
					JE_TotalNoOfPacksPackType = ACROSSPackageTypes.Codes.PACKAGE;
				}

				if ((IsImport && oldValue == ACROSSServiceOptions.Codes.IID && value != ACROSSServiceOptions.Codes.IID) || !IsImport)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						invoiceLine.ClearPGAIndicators();
					}
				}

				if (oldIsIID != IsIID || oldIsOGD != IsOGD)
				{
					DeletePackagePivotIfRequired();

					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						invoiceLine.SetAVSStatusIfNeeded();
					}

					if (oldIsIID != IsIID)
					{
						shouldUpdateImporterDocumentaryAddress = true;
						shouldUpdateImporterOfRecord = true;
						ImporterDocumentaryAddress.SetDefaultAddressFromOrg();
						ImporterOfRecordAddress.SetDefaultAddressFromOrg();
					}
				}
			}
		}

		bool shouldUpdateImporterOfRecord;
		bool shouldUpdateImporterDocumentaryAddress;

		void DeletePackagePivotIfRequired()
		{
			if (!SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				var invoicePackagePivotQuery = new ZQuery(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, PK);
				Factory.Load<InvoiceHeaderPackagePivot>(invoicePackagePivotQuery).DeleteAll();
			}

			if (!SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				var invoiceLinePackagePivotQuery = new ZQuery(CusHouseContPackInvoiceLinePivotSchema.CHC_JE, PK);
				Factory.Load<InvoiceLinePackagePivot>(invoiceLinePackagePivotQuery).DeleteAll();
			}
		}

		#endregion

		#region CA_K84AccountingDate

		[ReadOnlyMember(nameof(CA_K84AccountingDate_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_K84AccountingDate", Caption = "Accounting Date", ShortCaption = "Acc. Date", MediumCaption = "Accounting Date", FullDescription = "The date the Entry accepted by CBSA.")]
		public override ZDateTime CA_K84AccountingDate
		{
			get
			{
				return IsBlanketB2 ? ZDateTime.Empty : base.CA_K84AccountingDate;
			}
			set
			{
				var hasChanged = base.CA_K84AccountingDate != value;
				base.CA_K84AccountingDate = value;

				if (IsConsolidatedLVS)
				{
					foreach (var invoice in Invoices)
					{
						var declaration = (JobDeclaration)invoice.JobDeclaration;
						if (declaration.IsLVX)
						{
							using (declaration.SetterSuspender.SuspendSetting(JobDeclaration.Schema.CA_K84AccountingDate))
							{
								declaration.CA_K84AccountingDate = value;
							}
						}
					}
				}

				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.CA_K84AccountingDate))
				{
					if (!value.IsEmpty)
					{
						if (hasChanged && !IsCopying)
						{
							AddCustomsReadyToPayEvent(value.ToOffset());
						}

						if (B3EntryHeader is CusEntryHeader entryHeader)
						{
							entryHeader.CancelAndDeactivateDeferredB3Message();
						}
					}

					if (hasChanged && !IsCopying)
					{
						SetDeclarationException();

						ReSetB3LateSendingWarningEvent();
					}
				}

				if (hasChanged && !IsCopying)
				{
					UpdateCA_AccountingAge();
				}
			}
		}

		public void SetDeclarationException()
		{
			DeclarationExceptionCodeCalculator.SetDeclarationException(this, this.Factory);
			CA_DeclarationExceptionDescriptionInfo.RefreshBinding();
		}

		DeclarationExceptionCodeCalculator DeclarationExceptionCodeCalculator
		{
			get { return new DeclarationExceptionCodeCalculator(CompanyPK.ToGuid()); }
		}

		void AddCustomsReadyToPayEvent(ZDateTimeOffset dateTime)
		{
			StatusLogManager.AddCustomsReadyToPayEvent(LogsOfDeclarationOrShipment, dateTime);

			if (IsConsolidatedLVS)
			{
				foreach (var invoice in Invoices)
				{
					var declaration = (JobDeclaration)invoice.JobDeclaration;
					if (declaration.IsLVX)
					{
						StatusLogManager.AddCustomsReadyToPayEvent(declaration.Logs, dateTime);
					}
				}
			}
		}

		void CancelCustomsReadyToPayEvent()
		{
			StatusLogManager.CancelCustomsReadyToPayEvent(LogsOfDeclarationOrShipment);

			if (IsConsolidatedLVS)
			{
				foreach (var invoice in Invoices)
				{
					var declaration = (JobDeclaration)invoice.JobDeclaration;
					if (declaration.IsLVX)
					{
						StatusLogManager.CancelCustomsReadyToPayEvent(declaration.Logs);
					}
				}
			}
		}

		bool CA_K84AccountingDate_ReadOnly
		{
			get { return IsLVX; }
		}

		#endregion

		#region CA_OriginalAccountingDate

		public override ZDateTime CA_OriginalAccountingDate
		{
			get { return !base.CA_OriginalAccountingDate.IsValid ? CA_K84AccountingDate : base.CA_OriginalAccountingDate; }
			set { base.CA_OriginalAccountingDate = value; }
		}

		#endregion

		#region CA_AllowOIC

		public override ZBool CA_AllowOIC
		{
			get { return base.CA_AllowOIC; }
			set
			{
				var oldValue = CA_AllowOIC;
				base.CA_AllowOIC = value;
				if (!IsCopying && oldValue != CA_AllowOIC)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						invoiceLine.CA_AuthorityNumberInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region K84 Data

		#region CA_K84StatementDate

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		[ResourceStringData("CAAddInfo|CA_K84StatementDate", Caption = "Statement Date", ShortCaption = "Stat. Date", MediumCaption = "Statement Date", FullDescription = "The date of the monthly statement on which this job appears.")]
		public override ZDateTime CA_K84StatementDate
		{
			get
			{
				if (IsLVX)
				{
					var declaration = LVXInvoiceHeader.FirstAdditionalDeclaration;
					return declaration == null ? ZDateTime.Empty : declaration.CA_K84StatementDate;
				}
				else
				{
					return base.CA_K84StatementDate;
				}
			}
			set { base.CA_K84StatementDate = value; }
		}

		#endregion

		#region CA_ReleaseOffice

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		[ResourceStringData("CAAddInfo|CA_ReleaseOffice", Caption = "Release Office")]
		public override ZString CA_ReleaseOffice
		{
			get { return base.CA_ReleaseOffice; }
			set { base.CA_ReleaseOffice = value; }
		}

		#endregion

		[ResourceStringData("CAAddInfo|CA_TotalDutyAmount", Caption = "Duty")]
		public ZDecimal CA_TotalDutyAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.TotalDutyAmount);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalSIMAAmount", Caption = "SIMA")]
		public ZDecimal CA_TotalSIMAAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.TotalSIMAAmount);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalExciseTaxAmount", Caption = "Excise Tax")]
		public ZDecimal CA_TotalExciseTaxAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(CARMDailyNoticeChargeTypeList.Codes.ExciseTax);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalGSTDirectAmount", Caption = "GST Direct")]
		public ZDecimal CA_TotalGSTDirectAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.TotalGSTDirectAmount);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalGSTAmount", Caption = "GST")]
		public ZDecimal CA_TotalGSTAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.TotalGSTAmount);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalDutyAndTaxAmount", Caption = "Total Duty and Tax")]
		public ZDecimal CA_TotalDutyAndTaxAmount
		{
			get
			{
				return CA_TotalDutyAmount + CA_TotalSIMAAmount + CA_TotalExciseTaxAmount + CA_TotalGSTDirectAmount + CA_TotalGSTAmount;
			}
		}

		[ResourceStringData("CAAddInfo|CA_K84LateFilingPenalty", Caption = "Others(Penalty)")]
		public ZDecimal CA_K84LateFilingPenalty
		{
			get { return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.K84LateFilingPenalty); }
		}

		[ResourceStringData("CAAddInfo|CA_ARLOthers", Caption = "Others")]
		public ZDecimal CA_ARLOthersAmount
		{
			get
			{
				return GetK84ChargeAmountByChargeType(EntryChargeTypeList.Codes.Others);
			}
		}

		[ResourceStringData("CAAddInfo|CA_TotalIncludingPenalty", Caption = "Total")]
		public ZDecimal CA_TotalIncludingPenalty
		{
			get
			{
				return CA_TotalDutyAndTaxAmount + CA_K84LateFilingPenalty + CA_ARLOthersAmount;
			}
		}

		ZDecimal GetK84ChargeAmountByChargeType(ZString chargeType)
		{
			var result = ZDecimal.Zero;
			if (K84DataSource != null && K84DataSource.Length > 0)
			{
				var bn9 = K84DataSource[0].B2_AccountNo;
				result = K84DataSource.Where(x => x.B2_AccountNo == bn9 && x.B3_EntryType != OtherTransactionEntryType.UnderReview).Sum(x => x.Charges.GetAmountFor(chargeType));
			}
			return result;
		}

		CusStatementLine[] K84DataSource
		{
			get
			{
				var transactionNum = FormattedTransactionNumber;
				if (k84DataSource == null && !JE_DeclarationReference.IsEmpty && !transactionNum.IsEmpty)
				{
					var lines = new List<CusStatementLine>();
					var arlDataQuery = new ZDBOnlyQuery(typeof(CusStatementLine));
					arlDataQuery.AddToFilter(CusStatementLineSchema.B3_BrokerReference, JE_DeclarationReference);
					arlDataQuery.AddToFilter(CusStatementLineSchema.B3_EntryNum, transactionNum);
					var arlDataCusStatementHeaderFilter = new ZDBOnlySubQuery(typeof(CusStatementHeader), CusStatementLineSchema.B3_B2);
					arlDataCusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_EntryFilerCode, TransactionNumber.AccountSecurityCode);
					arlDataCusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, SQLComparisonOperator.NotEqual, ZString.Empty);
					arlDataCusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_StatementType, SQLComparisonOperator.NotEqual, CusStatementHeaderTypes.Codes.RSF);
					arlDataCusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
					arlDataQuery.AddSubQuery(arlDataCusStatementHeaderFilter, JoinCondition.And);

					var arlLines = Factory.Load<CusStatementLine>(arlDataQuery);

					foreach (var dnLine in arlLines)
					{
						if (!lines.Any(x => CusStatementLine.IsDuplicatedStatementLine(x, dnLine)))
						{
							lines.Add(dnLine);
						}
					}

					k84DataSource = lines.OrderBy(x => x.B2_ProcessDate).ToArray();
				}
				return k84DataSource;
			}
		}

#if DEBUG
		internal
#endif
		CusStatementLine[] k84DataSource;

		#endregion

		#region RNS&DN&Notice Collections

		public ActiveCusStatementLineCollection DNHistoryLines
		{
			get
			{
				if (activeCusStatementLineCollection == null)
				{
					activeCusStatementLineCollection = new ActiveCusStatementLineCollection(Factory);
					var lines = new List<CusStatementLine>();
					var jobnumbers = (from b2 in B2s.Cast<JobDeclaration>() select b2.JobNumber).Append(JobNumber);
					activeCusStatementLineCollection.AdditionalFilter = new ZQuery(CusStatementLineSchema.B3_BrokerReference, jobnumbers);
					foreach (var dnLine in activeCusStatementLineCollection)
					{
						if (dnLine.B2_AccountNo.IsEmpty && !lines.Any(x => CusStatementLine.IsDuplicatedStatementLine(x, dnLine)))
						{
							lines.Add(dnLine);
						}
					}
					activeCusStatementLineCollection = new ActiveCusStatementLineCollection(Factory, new ZQuery(CusStatementLineSchema.PK, lines.Select(x => x.PK)));
				}
				return activeCusStatementLineCollection;
			}
		}
		ActiveCusStatementLineCollection activeCusStatementLineCollection;

		public EDIMessageCollection RnsHistoryMessages
		{
			get
			{
				if (rnsHistoryMessages == null && ReleaseEntryHeader is CusEntryHeader entryHeader)
				{
					var rnsHistoryQuery = new ZQuery();
					rnsHistoryQuery.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.EDIRelease);
					rnsHistoryQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					var errorTypes = new[]
					{
						EDIReleaseImportEntryStatusList.Codes.Error,
						EDIReleaseImportEntryStatusList.Codes.MessageContentRejected,
						EDIReleaseImportEntryStatusList.Codes.SyntaxError
					};
					rnsHistoryQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, errorTypes);
					rnsHistoryMessages = new EDIMessageCollection(entryHeader, rnsHistoryQuery);
					rnsHistoryMessages.Load();
				}
				return rnsHistoryMessages ?? new EDIMessageCollection(this);
			}
		}
		EDIMessageCollection rnsHistoryMessages;

		public EDIMessageCollection NoticesMessages
		{
			get
			{
				if (noticesMessages == null && ReleaseEntryHeader is CusEntryHeader entryHeader)
				{
					noticesMessages = new EDIMessageCollection(entryHeader, new ZQuery(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.ACIHouseBill));
					noticesMessages.Load();
				}
				return noticesMessages ?? new EDIMessageCollection(this);
			}
		}
		EDIMessageCollection noticesMessages;

		public NoticesMessageCollection NoticesMessagesForDisplay
		{
			get
			{
				if (noticesMessagesForDisplay == null)
				{
					noticesMessagesForDisplay = new NoticesMessageCollection(Factory);
					var entryHeaderPK = this.ReleaseEntryHeader?.PK ?? ZGuid.Invalid;
					var shipmentPK = Shipment?.PK ?? ZGuid.Invalid;
					var query = new ZDBOnlyQuery(typeof(EDIMessage));
					query.AddToFilter(EDIMessageQueryHelper.GetEDIMessageGenPivotQuery(new[] { entryHeaderPK, PK, shipmentPK }, new ZString[] { UniversalEventMessageTypes.Codes.D4Notices }));
					query.AddToFilter(NoticesMessages.CompleteFilter, JoinCondition.Or);
					foreach (EDIMessage message in Factory.Load<EDIMessage>(query))
					{
						var statusCodes = (message as UniversalEventMessage)?.StatusCodes;
						if (statusCodes != null && statusCodes.Any())
						{
							foreach (var statusCode in statusCodes)
							{
								var noticesMessage = new NoticesMessage(message, ZString.Format("{0} - {1}", statusCode, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, statusCode)));
								noticesMessagesForDisplay.Add(noticesMessage);
							}
						}
						else
						{
							noticesMessagesForDisplay.Add(new NoticesMessage(message, ZString.Empty));
						}
					}
					noticesMessagesForDisplay.SetReadOnlyIncludingChildren(true);
				}
				return noticesMessagesForDisplay;
			}
		}
		NoticesMessageCollection noticesMessagesForDisplay;

		public NoticesMessage LatestNoticeMessage
		{
			get
			{
				var latestNotices = NoticesMessagesForDisplay.Cast<NoticesMessage>().Where(m => m.RNSProcessingDate.IsValid).CollectMaxBy(m => m.RNSProcessingDate);
				if (latestNotices.Any(m => m.Message is UniversalEventMessage))
				{
					return latestNotices.Where(m => m.Message is UniversalEventMessage)
						.CollectMaxBy(m => m.Message.EDIFACTInterchangeNumber)
						.CollectMaxBy(m => m.Message.EDIFACTMessageNumber).FirstOrDefault();
				}
				else
				{
					return latestNotices.OrderBy(m => m.Message.EM_InterchangeNumber).LastOrDefault();
				}
			}
		}

		#endregion

		#region CA_PortOfExit

		[MaxLength(AutoCAAddInfo.Schema.CA_PortOfExitMaxLength)]
		public override ZString CA_PortOfExit
		{
			get => base.CA_PortOfExit;
			set => base.CA_PortOfExit = value.IsEmpty ? value : value.PadLeft(4, '0');
		}

		#endregion

		#region CA_PlaceOfReport

		[MaxLength(AutoCAAddInfo.Schema.CA_PlaceOfReportMaxLength)]
		public override ZString CA_PlaceOfReport
		{
			get => base.CA_PlaceOfReport;
			set
			{
				if (value.Length <= AutoCAAddInfo.Schema.CA_PlaceOfReportMaxLength)
				{
					base.CA_PlaceOfReport = value.IsEmpty ? value : value.PadLeft(4, '0');
				}
			}
		}

		#endregion

		#region JE_CustomsOffice

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CBSAOffices))]
		[ReadOnlyMember(nameof(JE_CustomsOffice_ReadOnly))]
		[RelatedBusinessObject(nameof(PortOfClearance))]
		[ResourceStringData("1D9FB2FE-21CF-4742-A6ED-D90D7A8A8A27", Caption = "Customs Port of Clearance", ShortCaption = "Port", FullDescription = "Port of Clearance")]
		public override ZString JE_CustomsOffice
		{
			get { return IsBlanketB2 ? (ZString)"VAR" : base.JE_CustomsOffice; }
			set
			{
				var newValue = value.IsEmpty ? value : value.SubstringSafe(0, 4).PadLeft(4, '0');
				bool hasChanges = base.JE_CustomsOffice != newValue;
				base.JE_CustomsOffice = newValue;
				if (IsImport && !IsDataSyncFromShipment && !IsCopying && hasChanges)
				{
					UNLOCODefaulter.DefaultUNLOCOCode(value, IsInDatabase);
				}
				if (hasChanges)
				{
					fCAHolidaysApplicableToThisDeclaration = null;
					if (!IsCopying)
					{
						CalculateEstimatedPaymentDueDate();
						UpdateCA_AccountingAge();
					}
				}
				if (JE_MessageType == JobMessageTypeList.Codes.Import && !IsOtherWarehouseEntry && !PortOfClearanceRelatedUSPortOfExit.IsEmpty)
				{
					foreach (JobComInvoiceHeader invoice in Invoices)
					{
						if (invoice.CA_USPortOfExit.IsEmpty && invoice.CA_RN_NKExport == Core.Constants.CountryCodes.UnitedStates)
						{
							invoice.CA_USPortOfExit = PortOfClearanceRelatedUSPortOfExit;
							invoice.CA_USPortOfExitInfo.RefreshBinding();
						}
					}
				}
			}
		}

		bool JE_CustomsOffice_ReadOnly
		{
			get { return IsBlanketB2; }
		}

		public ZZRefCusCodeListCombined PortOfClearance
		{
			get { return GetOfficeCode(JE_CustomsOffice); }
		}

		ZZRefCusCodeListCombined GetOfficeCode(string portOfClearance)
		{
			return Factory.GetCachedValue("PortOfClearance" + "|" + Core.Constants.CountryCodes.Canada + "|" + portOfClearance, () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, portOfClearance, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			});
		}

		public ZString PortOfClearanceRelatedUSPortOfExit
		{
			get { return PortOfClearance?.GetAttribute(RefCusCodeListAttributes.USPortOfExit) ?? ZString.Empty; }
		}

		public ZString GetDescriptionFromPortOfficeCode(ZString officeCode)
		{
			return Factory.GetCachedValue($"GetDescriptionFromPortOfficeCode|{officeCode}", () =>
			{
				var office = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, officeCode, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return office?.ZZD_Description ?? ZString.Empty;
			});
		}

		#endregion

		#region CA_UnladingOffice

		public override ZString CA_UnladingOffice
		{
			get { return base.CA_UnladingOffice; }
			set { base.CA_UnladingOffice = value.IsEmpty ? value : value.PadLeft(4, '0'); }
		}

		#endregion

		#region CA_NetWeight

		[MeasureUnit(Schema.CA_NetWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CA_NetWeight
		{
			get { return base.CA_NetWeight; }
			set
			{
				if (CA_NetWeightUQ.IsEmpty)
				{
					CA_NetWeightUQ = UnitOfWeightList.Codes.Kilogram;
				}

				base.CA_NetWeight = value;
			}
		}

		#endregion

		#region LVSCloseDate

		public override ZDateTime CA_LVSCloseDate
		{
			get { return base.CA_LVSCloseDate; }
			set
			{
				var oldValue = CA_LVSCloseDate;
				base.CA_LVSCloseDate = value;
				var newValue = CA_LVSCloseDate;

				if (!IsCopying && newValue != oldValue)
				{
					if (!newValue.IsEmpty)
					{
						AddCustomsReadyToPayEvent(value.ToOffset());
					}
					else
					{
						CancelCustomsReadyToPayEvent();
					}
				}

				Invoices.RefreshBinding();
			}
		}

		#endregion

		#region CA_OGDStatus

		public override ZString CA_OGDStatus
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Import ? base.CA_OGDStatus : ZString.Empty; }
			set
			{
				var oldValue = CA_OGDStatus;
				base.CA_OGDStatus = value;
				var newValue = CA_OGDStatus;
				if (!IsCopying && oldValue != newValue && newValue != AVSStatusList.Codes.Blank && newValue != AVSStatusList.Codes.NotValidated)
				{
					Logs.AddNew(AutoEvents.ServiceCompleted, newValue, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.AVSQuery));
				}
			}
		}

		#endregion

		#region JE_DateOfFirstArrival

		[ResourceStringData("117e6550-56c0-4591-8268-baeb8fd38a4b", ShortCaption = "ETA", Caption = "Arrival Date at First Port of Arrival", FullDescription = "The Date of Importation at the First Port of Arrival.")]
		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return base.JE_DateOfFirstArrival; }
			set
			{
				var oldValue = JE_DateOfFirstArrival;
				var newValue = value;
				if (!IsCopying && oldValue != newValue && !JE_EntryAuthorisationDate.IsValid)
				{
					var timeAtPort = this.TimeAtPortOfDischarge;
					if (JE_TransportMode == TransportTypeList.Codes.Road && timeAtPort.IsValid && value < timeAtPort.AddHours(3) && value > timeAtPort.AddHours(-1))
					{
						newValue = timeAtPort.AddHours(3);
					}
					else if (JE_TransportMode == TransportTypeList.Codes.Air && timeAtPort.IsValid && value < timeAtPort.AddHours(5) && value > timeAtPort.AddHours(-1))
					{
						newValue = timeAtPort.AddHours(5);
					}
				}
				base.JE_DateOfFirstArrival = newValue;
				if (!IsCopying && oldValue != newValue)
				{
					CalculateEstimatedPaymentDueDate();
					SetDeclarationException();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CA_EstReleaseDate

		public override ZDateTime CA_EstReleaseDate
		{
			get { return base.CA_EstReleaseDate; }
			set
			{
				var oldValue = CA_EstReleaseDate;
				base.CA_EstReleaseDate = value;
				if (!IsCopying && CA_EstReleaseDate != oldValue)
				{
					CalculateEstimatedPaymentDueDate();
				}
			}
		}

		#endregion

		#region JE_EntrySubmittedDate

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		[ResourceStringData("5a2b11bd-e527-4e4e-b343-8b506896dd73", Caption = "Submitted Date")]
		public override ZDateTime JE_EntrySubmittedDate
		{
			get { return base.JE_EntrySubmittedDate; }
			set { base.JE_EntrySubmittedDate = value; }
		}

		#endregion

		#region B3EntrySubmittedDate

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		[ResourceStringData("12D1BA11-6716-4571-B07B-57186C34B901", Caption = "B3 Submission Date")]
		public ZDateTime B3EntrySubmittedDate
		{
			get { return B3EntryHeader?.CH_EntrySubmittedDate ?? ZDateTime.Empty; }
			set
			{
				if (B3EntryHeader is CusEntryHeader entryHeader)
				{
					entryHeader.CH_EntrySubmittedDate = value;
				}
			}
		}

		#endregion

		#region

		[ReadOnly(true)]
		public ZShort CADVersionID
		{
			get
			{
				return B3EntryHeader?.CH_VersionID ?? 0;
			}
		}

		#endregion

		#region RelEntrySubmittedDate

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		[ResourceStringData("FAFEEDBE-1476-4DC7-88B6-23888D1E892A", Caption = "Release Submission Date")]
		public ZDateTime RelEntrySubmittedDate
		{
			get { return ReleaseEntryHeader?.CH_EntrySubmittedDate ?? ZDateTime.Empty; }
			set
			{
				if (ReleaseEntryHeader is CusEntryHeader entryHeader)
				{
					entryHeader.CH_EntrySubmittedDate = value;
				}
			}
		}

		#endregion

		#region For B3 Date Calculations - federal and provincial holidays

		public IEnumerable<ZDateTime> CAHolidaysApplicableToThisDeclaration
		{
			get
			{
				if (fCAHolidaysApplicableToThisDeclaration == null)
				{
					var provinceOfClearance = PortOfClearance?.GetAttribute(RefCusCodeListAttributes.Province) ?? ZString.Empty;
					fCAHolidaysApplicableToThisDeclaration = this.GetHardCodedCAHolidays(provinceOfClearance);
				}
				return fCAHolidaysApplicableToThisDeclaration;
			}
		}
		IEnumerable<ZDateTime> fCAHolidaysApplicableToThisDeclaration;

		#endregion

		#region CA_AssesmentOption

		[ReadOnlyMember(nameof(CA_AssessmentOptions_ReadOnly))]
		public override ZString CA_AssesmentOption
		{
			get => base.CA_AssesmentOption;
			set => base.CA_AssesmentOption = value;
		}

		bool CA_AssessmentOptions_ReadOnly
		{
			get { return IsIID || IsCSA; }
		}

		#endregion

		public bool AlwaysReturnTrue
		{
			get { return true; }
		}

		#region Product Audit Type

		public static class CAProductAuditType
		{
			public const string B3High = "B3High";
			public const string B3Low = "B3Low";
			public const string ACROSSHigh = "ACROSSHigh";
			public const string ACROSSLow = "ACROSSLow";
		}

		public ZString ProductAuditType
		{
			get
			{
				if (productAuditType == null)
				{
					productAuditType = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						if (IsB2Adjustments || IsB3X)
						{
							result = CAProductAuditType.B3High;
						}
						else if (!JE_EntryAuthorisationDate.IsValid)
						{
							if (IsLowValueNormalReleaseJob)
							{
								result = CAProductAuditType.ACROSSLow;
							}
							else
							{
								result = CAProductAuditType.ACROSSHigh;
							}
						}
						else if (IsLVS)
						{
							result = CAProductAuditType.B3Low;
						}
						else
						{
							if (IsLowValueNormalReleaseJob)
							{
								result = CAProductAuditType.B3Low;
							}
							else
							{
								result = CAProductAuditType.B3High;
							}
						}
						return result;
					});
				}
				return productAuditType.Value;
			}
		}
		CachedProperty<ZString> productAuditType;

		#endregion

		#region CA_B2Explanation

		public StmNote B2Explanation
		{
			get
			{
				if (fB2Explanation == null || fB2Explanation.IsDeleted)
				{
					fB2Explanation = Notes.FindByDescription(B2ExplanationNote).FirstOrDefault();
				}

				return fB2Explanation;
			}
		}
		StmNote fB2Explanation;

		const string B2ExplanationNote = "B2 Explanation Note";

		public ZString CA_B2Explanation
		{
			get { return B2Explanation?.ST_NoteDataAsText ?? ZString.Empty; }
			set
			{
				var hasChanges = CA_B2Explanation != value;
				if (hasChanges)
				{
					if (fB2Explanation == null)
					{
						fB2Explanation = Notes.AddNew(true, B2ExplanationNote, ZString.Empty);
						RegisterEditableChildObject(fB2Explanation);
					}
					fB2Explanation.ST_NoteDataAsText = value;
				}
				CA_B2ExplanationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_B2ExplanationInfo
		{
			get { return GetZPropertyInfo(Schema.CA_B2Explanation); }
		}

		#endregion

		#endregion

		public override bool SupportAdditionalInvoices
		{
			get
			{
				return IsConsolidatedLVS;
			}
		}

		public bool HasAB3AcceptedOrWaiting
		{
			get
			{
				var entryHead = B3EntryHeader;
				return IsInDatabase && entryHead != null && (entryHead.IsClearedB3CorCAD || entryHead.CH_Status == MessageStatusList.Codes.AwaitingOriginal);
			}
		}

		#endregion

		#region Protected Overrides

		#region Methods

		protected override bool IsIntegrationWithAccountingSupported
		{
			get { return true; }
		}

		protected override ZString GetMessageTypeForDocumentFilter()
		{
			ZString result;
			var messageType = JE_MessageType;
			if (messageType == JobMessageTypeList.Codes.LowValueShipments
				|| messageType == JobMessageTypeList.Codes.B2Adjustments
				|| messageType == JobMessageTypeList.Codes.LVSForConsolidation
				|| messageType == JobMessageTypeList.Codes.ImportCopyforB2
				|| messageType == JobMessageTypeList.Codes.XTypeEntry)
			{
				result = messageType;
			}
			else
			{
				result = base.GetMessageTypeForDocumentFilter();
			}
			return result;
		}

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			var caDeclaration = (JobDeclaration)declaration;

			using (caDeclaration.GetValidationSuspender())
			using (caDeclaration.SuspendSettingHasChanges())
			using (caDeclaration.GetAddInfo().GetValidationSuspender())
			using (caDeclaration.GetAddInfo().SuspendSettingHasChanges())
			using (caDeclaration.SuspendMarkApportionmentDirty())
			{
				base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);

				caDeclaration.CA_ReleaseOffice = "";
				caDeclaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
				caDeclaration.CA_OGDStatus = ZString.Empty;
				declaration.JE_TotalWeight = ZDecimal.Zero;
				declaration.JE_TotalVolume = ZDecimal.Zero;
				declaration.JE_TotalVolumeUnit = ZString.Empty;
				declaration.JE_TotalNoOfPacks = ZInt.Zero;
				caDeclaration.CA_AccountingAge = ZInt.Zero;
				caDeclaration.CA_DeclarationException = ZString.Empty;

				if (declaration.IsImport)
				{
					caDeclaration.CA_NetWeight = ZDecimal.Zero;
					caDeclaration.CA_NetWeightUQ = ZString.Empty;
					caDeclaration.CA_AmendReasonCode = ZString.Empty;
					BondDetailsDefaulter.Default(caDeclaration, caDeclaration.CA_BondType);
				}

				foreach (JobComInvoiceGroupHeader groupInvoice in declaration.AllGroupHeaders)
				{
					ResetValuesOnGroupInvoiceForTemplateCopy(groupInvoice);
				}

				foreach (JobComInvoiceHeader invoice in declaration.Invoices)
				{
					ResetValuesOnInvoiceForTemplateCopy(invoice);
				}

				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					ResetValuesOnInvoiceLineForTemplateCopy(invoiceLine);
				}
			}
		}

		void ResetValuesOnGroupInvoiceForTemplateCopy(JobComInvoiceGroupHeader groupInvoice)
		{
			using (groupInvoice.GetValidationSuspender())
			using (groupInvoice.SuspendSettingHasChanges())
			{
				groupInvoice.JZ_InvoiceDate = ZDateTime.Empty;
				groupInvoice.JZ_InvoiceAmount = ZDecimal.Zero;
				groupInvoice.JZ_Weight = ZDecimal.Zero;
				groupInvoice.JZ_WeightUQ = ZString.Empty;
				groupInvoice.JZ_Volume = ZDecimal.Zero;
				groupInvoice.JZ_VolumeUQ = ZString.Empty;
				groupInvoice.JZ_NoOfPacks = ZDecimal.Zero;

				foreach (JobComInvCharge charge in groupInvoice.Charges)
				{
					charge.J7_Amount = ZDecimal.Zero;
					charge.HasChanges = false;
				}
			}
			groupInvoice.HasChanges = false;
		}

		void ResetValuesOnInvoiceForTemplateCopy(JobComInvoiceHeader invoice)
		{
			using (invoice.GetValidationSuspender())
			using (invoice.SuspendSettingHasChanges())
			using (invoice.GetAddInfo().SuspendSettingHasChanges())
			using (invoice.GetAddInfo().GetValidationSuspender())
			{
				invoice.JZ_InvoiceDate = ZDateTime.Empty;
				invoice.JZ_InvoiceAmount = ZDecimal.Zero;
				invoice.JZ_Weight = ZDecimal.Zero;
				invoice.JZ_WeightUQ = ZString.Empty;
				invoice.JZ_Volume = ZDecimal.Zero;
				invoice.JZ_VolumeUQ = ZString.Empty;
				invoice.JZ_NoOfPacks = ZDecimal.Zero;
				invoice.JZ_NetWeight = ZDecimal.Zero;
				invoice.JZ_ValuationDateOverride = ZDateTime.Empty;
				invoice.CA_OtherReference = ZString.Empty;
				invoice.CA_ReadyForConsolidation = ZBool.False;

				foreach (JobComInvCharge charge in invoice.Charges)
				{
					charge.J7_Amount = ZDecimal.Zero;
					charge.HasChanges = false;
				}
			}
			invoice.HasChanges = false;
		}

		void ResetValuesOnInvoiceLineForTemplateCopy(JobComInvoiceLine invoiceLine)
		{
			using (invoiceLine.GetValidationSuspender())
			using (invoiceLine.SuspendSettingHasChanges())
			using (invoiceLine.GetAddInfo().SuspendSettingHasChanges())
			using (invoiceLine.GetAddInfo().GetValidationSuspender())
			{
				invoiceLine.JI_Weight = ZDecimal.Zero;
				invoiceLine.JI_WeightUQ = ZString.Empty;
				invoiceLine.JI_Volume = ZDecimal.Zero;
				invoiceLine.JI_VolumeUQ = ZString.Empty;
				invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
				invoiceLine.JI_InvoiceUQ = ZString.Empty;
				invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
				invoiceLine.JI_LinePrice = ZDecimal.Zero;
				invoiceLine.JI_NetWeight = ZDecimal.Zero;
				invoiceLine.JI_NetWeightUQ = ZString.Empty;
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_CustomsSecondQuantity = ZDecimal.Zero;
				invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
				invoiceLine.CA_CVforCurrConvOvr = false;
				invoiceLine.CA_CustomsValueOvr = false;
				invoiceLine.CA_CustomsValue = ZDecimal.Zero;
				invoiceLine.SetAVSStatusIfNeeded();

				foreach (JobComInvCharge charge in invoiceLine.Charges)
				{
					charge.J7_Amount = ZDecimal.Zero;
					charge.HasChanges = false;
				}
			}
			invoiceLine.HasChanges = false;
		}

		protected override TransportCommonShared.DtbBookingDirection[] GetSupportedDirectionsCore()
		{
			return IsB2OrIM2OrB3X ? Array.Empty<TransportCommonShared.DtbBookingDirection>() : base.GetSupportedDirectionsCore();
		}

		protected override ZBool SupportViewRelatedCommunicationsCore => base.SupportViewRelatedCommunicationsCore && !IsB2OrIM2OrB3X;

		protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore
		{
			get { return IsIID; }
		}

		protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore
		{
			get { return IsIID; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (GetAddInfo().SuspendSettingHasChanges())
			using (GetAddInfo().GetValidationSuspender())
			{
				if (IsExport)
				{
					CA_PlaceOfReport = CACustomsDataRegistry.Instance.DefaultPlaceOfReport.Value;
					var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode);
					CA_RX_DeclaredCurr = localCurrency != null ? localCurrency.PK : ZGuid.Empty;
				}
				if (IsImport)
				{
					JE_CustomsOffice = CACustomsDataRegistry.Instance.DefaultPortOfClearance.GetFallBackValueAtAllLevels(EffectiveBranch.Company.PK.ToGuid(), EffectiveBranch.PK.ToGuid(), Guid.Empty);
				}
				if (IsB3X)
				{
					JE_PaymentMethod = ZString.Empty;
				}

				UpdateMergeBy();
			}
		}

		protected override bool ShouldRefreshExRatesOnApportionment(ICurrencyProvider currencyProvider, ZString localCurrency)
		{
			return true;
		}

		public override ZGuid CA_RX_DeclaredCurr
		{
			get { return base.CA_RX_DeclaredCurr; }
			set
			{
				base.CA_RX_DeclaredCurr = value;

				if (IsExport)
				{
					foreach (JobComInvoiceHeader invoice in Invoices)
					{
						invoice.ResetInvoiceCurrencyIfNeeded();
					}
				}
			}
		}

		protected override void DefaultJE_MergeByFromLocalParty(OrgHeader localParty)
		{
			if (IsImport)
			{
				if (!IsConsolidatedLVS)
				{
					var mergeBy = GetEffectiveMergeCustomsInvoiceLinesBy(localParty);
					if (!mergeBy.IsEmpty && AddInfoLookups.CAMergeByList.ContainsCode(mergeBy))
					{
						CA_MergeBy = mergeBy;
					}
				}
			}
			else
			{
				base.DefaultJE_MergeByFromLocalParty(LocalParty);
			}
		}

		protected override void MarkApportionmentDirtyCore()
		{
			if (IsLVX && ApportionmentDirty && Invoices.Count > 0)
			{
				foreach (JobDeclaration consolidatedLVS in LVXInvoiceHeader.AdditionalDeclarations)
				{
					consolidatedLVS.MarkApportionmentDirty();
				}
			}
		}

		protected override void OnApportioned()
		{
			base.OnApportioned();

			if (IsImportIncludingB2)
			{
				AddFetchsHintForPopulateDutiesAndTaxesIfNeeded();
				if (IsLVS)
				{
					ApportionedForLVS();
				}
				else
				{
					ClearCasualImportDummyData(InvoiceLines);

					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						if (line.InvoiceHeader != null && line.InvoiceHeader.IsAttachedToPersistentDeclaration)
						{
							line.DutyAndTaxManager.PopulateDutiesAndTaxes();
						}
					}

					ShouldCalculateDutiesOnMerge = true;
				}
			}
		}

		void ApportionedForLVS()
		{
			var hasAutoDummyHSCodeCasualImportLine = InvoiceLines.Cast<JobComInvoiceLine>().Where(line => line.CA_IsAutoDummyHSCodeCasualImportLine).IsCountMoreThan(0);

			var supporter = new InvoicesOverrideDeclarationSupporter(this);

			try
			{
				foreach (JobComInvoiceHeader header in Invoices)
				{
					ClearCasualImportDummyData(header.JobComInvoiceLines);
					if (header.IsAttachedToPersistentDeclaration)
					{
						foreach (JobComInvoiceLine line in header.JobComInvoiceLines)
						{
							line.DutyAndTaxManager.PopulateDutiesAndTaxes();
						}
					}
					ShouldCalculateDutiesOnMerge = true;
				}
			}
			finally
			{
				supporter.Dispose();
			}

			if (!hasAutoDummyHSCodeCasualImportLine)
			{
				hasAutoDummyHSCodeCasualImportLine = InvoiceLines.Cast<JobComInvoiceLine>().Where(line => line.CA_IsAutoDummyHSCodeCasualImportLine).IsCountMoreThan(0);
			}

			foreach (JobComInvoiceHeader header in Invoices)
			{
				var relatedDeclaration = IsLVX ? header.FirstAdditionalDeclaration : header.JobDeclaration;
				if (relatedDeclaration != null && relatedDeclaration != this)
				{
					if (hasAutoDummyHSCodeCasualImportLine || relatedDeclaration.ApportionmentDirty)
					{
						relatedDeclaration.CA_RequiresMerge = true;
					}
					relatedDeclaration.ApportionmentDirty = false;
				}
			}
		}

		#region AddFetchsHintForPopulateDutiesAndTaxesIfNeeded

		public void AddFetchsHintForPopulateDutiesAndTaxesIfNeeded()
		{
			if (!hasFetchHintsForPopulateDutiesAndTaxes)
			{
				hasFetchHintsForPopulateDutiesAndTaxes = true;

				Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JE, PK);
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, invoiceLine.PK);
				}

				var effectiveDutyDate = EffectiveDutyDate;
				var classificationNumbers = new List<ZString>();
				var tariffCodes = new List<ZString>();
				var classHeaders = new List<CACClassHeader>();

				AddFetchHintsForCACClass(classificationNumbers, tariffCodes);
				AddFetchHintsForCACRateHeaderAndCACTaxRefNumHeader(effectiveDutyDate, classificationNumbers, classHeaders);
				AddFetchHintsForCACRateAndLine(effectiveDutyDate, classHeaders, tariffCodes);
			}
		}
		bool hasFetchHintsForPopulateDutiesAndTaxes;

		void AddFetchHintsForCACClass(List<ZString> classificationNumbers, List<ZString> tariffCodes)
		{
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				var classificationNumber = invoiceLine.JI_Tariff;
				if (!classificationNumber.IsEmpty)
				{
					if (!classificationNumbers.Contains(classificationNumber))
					{
						classificationNumbers.Add(classificationNumber);
						Factory.AddFetchHint(CACClassSchema.CT_Tariff, classificationNumber);
						Factory.AddFetchHint(CACClassHeaderSchema.ZA_ClassificationNumber, classificationNumber);
					}
				}
				var tariffCode = invoiceLine.CA_99TariffCode;
				if (!tariffCode.IsEmpty)
				{
					if (!tariffCodes.Contains(tariffCode))
					{
						tariffCodes.Add(tariffCode);
						Factory.AddFetchHint(CACClassSchema.CT_Tariff, tariffCode);
						Factory.AddFetchHint(CACTariffHeaderSchema.ZF_TariffCode, tariffCode);
					}
				}
			}
		}

		void AddFetchHintsForCACRateHeaderAndCACTaxRefNumHeader(ZDateTime effectiveDutyDate, List<ZString> classificationNumbers, List<CACClassHeader> classHeaders)
		{
			foreach (var classificationNumber in classificationNumbers)
			{
				var classHeader = CACClassHeader.Load(Factory, effectiveDutyDate, classificationNumber);
				if (classHeader != null)
				{
					classHeaders.Add(classHeader);
					Factory.AddFetchHint(CACRateHeaderSchema.ZB_ZA_ClassHeader, classHeader.PK);
					Factory.AddFetchHint(CACTaxRefNumHeaderSchema.ZD_ZA_ClassNumber, classHeader.PK);
				}
			}
		}

		void AddFetchHintsForCACRateAndLine(ZDateTime effectiveDutyDate, List<CACClassHeader> classHeaders, List<ZString> tariffCodes)
		{
			var classRateHeaders = new List<CACRateHeader>();
			foreach (var classHeader in classHeaders)
			{
				var classRateHeader = CACRateHeader.Load(classHeader, effectiveDutyDate, CACRateHeader.RateType.ClassificationRate);
				if (classRateHeader != null && !classRateHeader.ZB_FreeInd)
				{
					classRateHeaders.Add(classRateHeader);
					Factory.AddFetchHint(CACRateSchema.ZC_ParentID, classRateHeader.PK);
				}
				classRateHeader = CACRateHeader.Load(classHeader, effectiveDutyDate, CACRateHeader.RateType.ExciseDutyRate);
				if (classRateHeader != null)
				{
					classRateHeaders.Add(classRateHeader);
					Factory.AddFetchHint(CACRateSchema.ZC_ParentID, classRateHeader.PK);
				}
			}

			foreach (var classRateHeader in classRateHeaders)
			{
				foreach (var treatmentCode in InvoiceLines.Cast<JobComInvoiceLine>().Where(l => l.JI_Tariff == classRateHeader.ClassHeader.ZA_ClassificationNumber).Select(l => l.EffectiveTreatmentCode).Distinct())
				{
					if (!treatmentCode.IsEmpty)
					{
						var classRate = CACRate.Load(classRateHeader, treatmentCode);
						if (classRate != null)
						{
							Factory.AddFetchHint(CACRateLineSchema.ZR_ZC_Rate, classRate.PK);
						}
					}
				}
			}

			AddFetchHintsForCACReateAndLineWithTariffCodes(effectiveDutyDate, tariffCodes);
		}

		void AddFetchHintsForCACReateAndLineWithTariffCodes(ZDateTime effectiveDutyDate, List<ZString> tariffCodes)
		{
			var tariffHeaders = new List<CACTariffHeader>();
			foreach (var tariffCode in tariffCodes)
			{
				var tariffHeader = CACTariffHeader.Load(Factory, effectiveDutyDate, tariffCode);
				if (tariffHeader != null && !tariffHeader.ZF_FreeInd
					&& tariffHeader.ZF_RateEffectiveDate.Date <= effectiveDutyDate.Date && effectiveDutyDate.Date <= tariffHeader.ZF_RateExpiryDate.Date)
				{
					tariffHeaders.Add(tariffHeader);
					Factory.AddFetchHint(CACRateSchema.ZC_ParentID, tariffHeader.PK);
				}
			}

			foreach (var tariffHeader in tariffHeaders)
			{
				foreach (var treatmentCode in InvoiceLines.Cast<JobComInvoiceLine>().Where(l => l.CA_99TariffCode == tariffHeader.ZF_TariffCode).Select(l => l.EffectiveTreatmentCode).Distinct())
				{
					if (!treatmentCode.IsEmpty)
					{
						var classRate = CACRate.Load(tariffHeader, treatmentCode);
						if (classRate != null)
						{
							Factory.AddFetchHint(CACRateLineSchema.ZR_ZC_Rate, classRate.PK);
						}
					}
				}
			}
		}

		#endregion

		public void RunMergeForLVSIfRequired()
		{
			if (CA_RequiresMerge)
			{
				ApportionmentDirty = true;
				DoMerge();
			}
		}

		void ClearCasualImportDummyData(BusinessObjectCollection invoiceLines)
		{
			AutoDummyHSCodeCasualImportLinesDic.Clear();
			RemoveAutoDummyHSCodeCasualImportLines(invoiceLines);
		}

		protected void RemoveAutoDummyHSCodeCasualImportLines(BusinessObjectCollection collection)
		{
			if (collection != null)
			{
				var invoiceLinesToRemove = collection.OfType<JobComInvoiceLine>().Where(line => line.CA_IsAutoDummyHSCodeCasualImportLine).ToList();

				foreach (var line in invoiceLinesToRemove)
				{
					if (!AutoDummyHSCodeCasualImportLinesDic.ContainsKey(line.JI_Tariff))
					{
						AutoDummyHSCodeCasualImportLinesDic.Add(line.JI_Tariff, line.GetAddInfo());
					}
				}

				using (SuspendMarkApportionmentDirty())
				{
					invoiceLinesToRemove.ForEach(line =>
					{
						if (line.InvoiceHeader != null && line.InvoiceHeader.IsAttachedToPersistentDeclaration)
						{
							var invoiceLines = line.InvoiceHeader.JobComInvoiceLines;
							using (invoiceLines.JobDeclaration.SuspendMarkApportionmentDirty())
							{
								invoiceLines.RemoveAndDelete(line);
							}
						}
					});
				}
			}
		}

		Dictionary<string, AddInfoJobComInvoiceLine> AutoDummyHSCodeCasualImportLinesDic
		{
			get
			{
				if (autoDummyHSCodeCasualImportLinesDic == null)
				{
					autoDummyHSCodeCasualImportLinesDic = new Dictionary<string, AddInfoJobComInvoiceLine>();
				}
				return autoDummyHSCodeCasualImportLinesDic;
			}
		}

		Dictionary<string, AddInfoJobComInvoiceLine> autoDummyHSCodeCasualImportLinesDic;

		#region DeriveDeclarationStatus

		protected override void DeriveExportDeclarationStatus()
		{
			if (ActiveEntryHeaders.Count > 0)
			{
				JE_MessageStatus = ActiveEntryHeaders[0].CH_Status;
				JE_EntryStatus = ActiveEntryHeaders[0].CH_EntryStatus;
			}
			else
			{
				JE_MessageStatus = JE_EntryStatus = ZString.Empty;
			}
		}

		protected override void DeriveImportDeclarationStatus()
		{
			var header = GetEntryHeaderOfLastSentMessage();
			JE_MessageStatus = header != null ? header.CH_Status : ZString.Empty;
			JE_EntryStatus = ReleaseEntryHeader?.CH_EntryStatus ?? ZString.Empty;
		}

		public CusEntryHeader GetEntryHeaderOfLastSentMessage()
		{
			CusEntryHeader result = null;
			Enterprise.Messaging.Business.EDIMessage lastMessage = null;
			foreach (CusEntryHeader header in ActiveEntryHeaders)
			{
				var message = header.Messages.LastOutgoingMessage;
				if (message != null && (lastMessage == null || message.EM_SystemCreateTimeUtc > lastMessage.EM_SystemCreateTimeUtc))
				{
					lastMessage = message;
					result = header;
				}
			}
			return result;
		}

		public CusEntryHeader GetEntryHeaderFor(string entryType)
		{
			if (!hasAddedEntryDetailsFetchHints)
			{
				hasAddedEntryDetailsFetchHints = true;
				var clusterKey = JE_ClusterKey;
				Factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, clusterKey);
				Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_ClusterKey, clusterKey);
				Factory.AddFetchHint(CusEntryLineSchema.CL_ClusterKey, clusterKey);
				Factory.AddFetchHint(CusEntryLineFeeSchema.CF_ClusterKey, clusterKey);
			}
			return (from CusEntryHeader header in CustomsEntryHeaders
					where header.CH_MessageType == entryType
					select header).FirstOrDefault();
		}
		bool hasAddedEntryDetailsFetchHints;

		public CusEntryHeader ReleaseEntryHeader
		{
			get
			{
				if (releaseEntryHeader == null || releaseEntryHeader.IsDeleted)
				{
					releaseEntryHeader = GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
				}
				return releaseEntryHeader;
			}
		}
		CusEntryHeader releaseEntryHeader;

		public CusEntryHeader G7ExportEntryHeader
		{
			get
			{
				if (g7ExportEntryHeader == null || g7ExportEntryHeader.IsDeleted)
				{
					g7ExportEntryHeader = GetEntryHeaderFor(MessageTypeList.Codes.G7Export);
				}
				return g7ExportEntryHeader;
			}
		}
		CusEntryHeader g7ExportEntryHeader;

		public CusEntryHeader B3EntryHeader
		{
			get
			{
				if (b3EntryHeader == null || b3EntryHeader.IsDeleted)
				{
					b3EntryHeader = GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC) ?? GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration);
				}
				return b3EntryHeader;
			}
		}
		CusEntryHeader b3EntryHeader;

		public bool IsCADEnabled
		{
			get
			{
				if (B3EntryHeader is CusEntryHeader b3EntryHeader)
				{
					return b3EntryHeader.IsCAD;
				}
				else
				{
					return UniversalReferenceConstants.IsCarmR2;
				}
			}
		}

		#endregion

		public override ZString JE_EntryStatusDescription
		{
			get
			{
				var result = base.JE_EntryStatusDescription;
				if (ReleaseEntryHeader is CusEntryHeader entryHeader && entryHeader.CH_EntryStatus.IsEmpty)
				{
					result = entryHeader.CH_Status.IsEmpty ? string.Empty : Res.GetString("71e14d73-8dca-4105-b3a8-01813acc5383", "Release Requested/In Progress");
				}

				return result;
			}
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override string GetDefaultMessageType(bool import)
		{
			var result = import ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
			if (!Lookups.MessageTypeList.ContainsCode(result))
			{
				result = JobMessageTypeList.Codes.Misc;
			}
			return result;
		}

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;

				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;

				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;

				case TransportTypeList.Codes.Road:
					return TransportTypeGenericList.Codes.Road;

				case TransportTypeList.Codes.Rail:
					return TransportTypeGenericList.Codes.Rail;

				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.NoCarrier:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		protected override Directions GetJobDirection()
		{
			return IsLVX || IsB2Adjustments || IsB3X ? Directions.Import : base.GetJobDirection();
		}

		[ResourceStringData("f082fb66-67b4-4470-8c0a-0c3d92c559c4", Caption = "Rail Car No.", IsApplicableMember = nameof(IsRail))]
		[ResourceStringData("6485305a-4a90-4d3a-8d0c-64b459121189", Caption = "Transport Ref.", IsApplicableMember = nameof(IsTransportReference))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		public bool IsTransportReference => IsInlandWaterwayTransport || IsFixedTransportInstallation || IsPost || IsNoCarrier;

		#endregion

		#region Properties

		protected override string DefaultTotalNoOfPacksPackType => IsIID ? IIDUnitOfCountCodeList.Codes.Pack : ACROSSPackageTypes.Codes.PACKAGE;

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		protected override ZBool SupportScreeningPresentationCore
		{
			get { return !IsB2OrIM2OrB3X; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return true; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.Canada; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		protected override ZDecimal TotalCustomsValueInLocalCurrencyCore
		{
			get
			{
				return B3EntryHeader?.CustomsValue ?? ZDecimal.Zero;
			}
		}

		protected override bool DoesCustomsEntryStatusAllowCancellation
		{
			get
			{
				var releaseEntryHeaderAllowCancellation = !(ReleaseEntryHeader is CusEntryHeader entryHeader) || MessageStatusList.IsMessageStatusAllowCancellation(entryHeader.CH_Status);
				var b3EntryHeaderAllowCancellation = !(B3EntryHeader is CusEntryHeader entryHeader2) || MessageStatusList.IsMessageStatusAllowCancellation(entryHeader2.CH_Status);

				return string.IsNullOrEmpty(JE_MessageStatus) || MessageStatusList.IsMessageStatusAllowCancellation(JE_MessageStatus) ||
					(IsImport && releaseEntryHeaderAllowCancellation && b3EntryHeaderAllowCancellation);
			}
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("CA"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool IsPackingInformationRelevantCore
		{
			get { return !IsExport && !IsB2Adjustments && !IsB3X && base.IsPackingInformationRelevantCore; }
		}

		protected override ZBool ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return !IsLVS && base.ShouldPromptToSaveBuyerSupplierRelationship; }
		}
		#endregion

		#endregion

		#region Defaulters

		CustomsCodesDefaulter CustomsPortOfClearanceDefaulter
		{
			get
			{
				return customsPortOfClearanceDefaulter ?? (customsPortOfClearanceDefaulter =
					new CustomsCodesDefaulter(Factory, JE_CustomsOfficeInfo, GetEffectiveAddresses, GetTransports, () => JE_TransportMode, OrgCusCode.CACodeTypes.CustomsOfficeCode, true));
			}
		}
		CustomsCodesDefaulter customsPortOfClearanceDefaulter;

		internal UNLOCODefaulter UNLOCODefaulter
		{
			get
			{
				return unlocoDefaulter ?? (unlocoDefaulter = new UNLOCODefaulter(Factory, GetEffectivePortOfArrivalInfo, () => JE_TransportMode));
			}
		}
		UNLOCODefaulter unlocoDefaulter;

		CustomsCodesDefaulter SubLocationDefaulter
		{
			get
			{
				return subLocationDefaulter ?? (subLocationDefaulter =
					new CustomsCodesDefaulter(Factory, JE_LocationOfGoodsInfo, GetEffectiveAddresses, GetTransports, () => JE_TransportMode, OrgCusCode.CodeTypes.ControlledPremisesID, false));
			}
		}
		CustomsCodesDefaulter subLocationDefaulter;

		IEnumerable<OrgAddress> GetEffectiveAddresses()
		{
			if (ContainerTerminalOperatorDocAddress != null && ContainerTerminalOperatorDocAddress.IsValidAddress)
			{
				yield return ContainerTerminalOperatorDocAddress.Address;
			}
			if (DepotDocAddress != null && DepotDocAddress.IsValidAddress)
			{
				yield return DepotDocAddress.Address;
			}
		}

		IEnumerable<Transport> GetTransports()
		{
			return TransportsIncludingRelated.Cast<Transport>();
		}

		bool ShouldUsePortOfArrivalForCustomsCodesDefaulting
		{
			get { return JE_TransportMode == Core.Constants.TransportModes.Sea || JE_TransportMode == Core.Constants.TransportModes.Air; }
		}

		ZPropertyInfo GetEffectivePortOfArrivalInfo()
		{
			return ShouldUsePortOfArrivalForCustomsCodesDefaulting ? JE_RL_NKPortOfArrivalInfo : null;
		}

		RefUNLOCO GetEffectivePortOfArrival()
		{
			return (ShouldUsePortOfArrivalForCustomsCodesDefaulting ? PortOfArrival : null) ?? Branch?.HomePort;
		}

		protected override RoutingCollection GetNewTransportsIncludingRelated()
		{
			var result = base.GetNewTransportsIncludingRelated();
			result.CountChanged += TransportsIncludingRelated_CountChanged;
			foreach (Transport routing in result)
			{
				routing.JW_RL_NKDiscPortInfo.ValueChanged -= RoutingDischargePortChanged;
				routing.JW_RL_NKDiscPortInfo.ValueChanged += RoutingDischargePortChanged;
			}
			return result;
		}

		void TransportsIncludingRelated_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var routing = (Transport)e.BizObject;
			if (routing != null)
			{
				routing.JW_RL_NKDiscPortInfo.ValueChanged -= RoutingDischargePortChanged;
				if (e.ItemAdded)
				{
					routing.JW_RL_NKDiscPortInfo.ValueChanged += RoutingDischargePortChanged;
				}
				RoutingDischargePortChanged(sender, null);
			}
		}

		void RoutingDischargePortChanged(object sender, EventArgs e)
		{
			DefaultPortOfClearanceAndSubLocationCode();
		}

		#endregion

		#region IMessageManageableBizObj Members

		#region GetMessageManagerForAmendmentDetection

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return GetMessageManagerForAmendmentDetection();
		}

		protected
#if DEBUG
 virtual
#endif
 JobDeclarationMessageManager GetMessageManagerForAmendmentDetection()
		{
			var actions = new CAMessageSendingActionCollection(this, MessageSendingMessageType.Amendment);
			return new JobDeclarationMessageManager(this, actions);
		}

		#endregion

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return IsExport && ActiveEntryHeaders.Count > 0 && ActiveEntryHeaders[0].Messages.GetLastClearReceivedMessage(MessageTypeList.Codes.DataLoadingModule) != null; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return !MergeManager.RequiresMerge || DoMerge() ? ContinueWithDetection.Yes : ContinueWithDetection.No;
		}

		#endregion

		#region IBackDoorSavingSupportableBizObj Members

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return IsExport; }
		}

		#endregion

		#region Implementation of IMessageManagerEventHandler

		void IMessageManagerEventHandler.OnMessageQueuedForSending()
		{
			LogCustomsCommencedIfNeeded();
		}

		#endregion

		#region Line To Print

		protected override void Lines_CountChanged(object sender, EventArgs e)
		{
			base.Lines_CountChanged(sender, e);
			Invoices.CountChanged -= Lines_CountChanged;
		}

		protected override LineToPrintCollection GetNewLineToPrintCollection()
		{
			Invoices.CountChanged += Lines_CountChanged;
			return new JobComInvoiceHeaderToPrintCollection(this);
		}

		public override bool SupportSelectingLinesToPrint
		{
			get { return true; }
		}

		#endregion

		#region B2 Document

		public JobComInvoiceGroupHeader B2AsAccountedForInvoiceGroupHeader => (JobComInvoiceGroupHeader)TopGroupInvoice;

		public JobComInvoiceGroupHeader B2AsClaimedForInvoiceGroupHeader => TopGroupInvoice.JobComInvoiceGroupHeaders.Cast<JobComInvoiceGroupHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == JobComInvoiceGroupHeader.AsClaimed);

		public JobComInvoiceGroupHeader CreateAsClaimedInvoiceGroupHeader()
		{
			var b2AsClaimedForInvoiceGroupHeader = this.B2AsClaimedForInvoiceGroupHeader;
			if (b2AsClaimedForInvoiceGroupHeader == null)
			{
				b2AsClaimedForInvoiceGroupHeader = (JobComInvoiceGroupHeader)this.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				b2AsClaimedForInvoiceGroupHeader.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AsClaimed;
			}
			return b2AsClaimedForInvoiceGroupHeader;
		}

		[ChildEditable(true)]
		public B2JobComInvoiceHeaderCollection B2AsAccountedForInvoices
		{
			get
			{
				if (fAsAccountedForFilteredInvoices == null)
				{
					fAsAccountedForFilteredInvoices = new B2JobComInvoiceHeaderCollection(B2AsAccountedForInvoiceGroupHeader);
					RegisterEditableChildObject(fAsAccountedForFilteredInvoices);

					foreach (var invoice in fAsAccountedForFilteredInvoices.Cast<JobComInvoiceHeader>())
					{
						invoice.EnableSynchroniser();
					}
				}
				return fAsAccountedForFilteredInvoices;
			}
		}
		B2JobComInvoiceHeaderCollection fAsAccountedForFilteredInvoices;

		[BusinessObjectTestExclude]
		[ChildEditable(true)]
		public B2JobComInvoiceHeaderCollection B2AsClaimedForInvoices
		{
			get
			{
				if (fAsClaimedForFilteredInvoices == null)
				{
					if (IsB2Adjustments || IsB3X)
					{
						var b2AsClaimedForInvoiceGroupHeader = B2AsClaimedForInvoiceGroupHeader
							?? CreateAsClaimedInvoiceGroupHeader();

						fAsClaimedForFilteredInvoices = new B2JobComInvoiceHeaderCollection(b2AsClaimedForInvoiceGroupHeader);
						RegisterEditableChildObject(fAsClaimedForFilteredInvoices);
					}
				}
				return fAsClaimedForFilteredInvoices;
			}
		}
		B2JobComInvoiceHeaderCollection fAsClaimedForFilteredInvoices;

		public CodeDescriptionPairList B2SortedInvoiceList(bool isAccounted)
		{
			var sortedInvoiceList = new CodeDescriptionPairList();
			var list = (isAccounted ? B2AsAccountedForInvoices : B2AsClaimedForInvoices).ToList();
			list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.JZ_InvoiceDisplaySequence, y.JZ_InvoiceDisplaySequence));
			sortedInvoiceList.AddRange(list);
			return sortedInvoiceList;
		}

		public override ZString IncoTerm
		{
			get { return IsB2Adjustments || IsB3X ? new ZString(Core.Constants.IncoTerms.FreeOnBoard) : base.IncoTerm; }
		}

		void DefaultB2FromB3IfNeeded()
		{
			if (IsB2Adjustments || IsB3X)
			{
				var b3HeaderAndAccountingDate = OriginalLodgedB3MessageWrapperAndAccountingDate;
				if (b3HeaderAndAccountingDate != null)
				{
					var b3Header = b3HeaderAndAccountingDate.Item1;
					var importer = b3Header.Importer?.Organisation;
					if (importer != null && !IsB3X)
					{
						JE_OH_Importer = importer.PK;
					}
					JE_CustomsOffice = b3Header.CBSAOffice;
					CA_OriginalAccountingDate = b3HeaderAndAccountingDate.Item2;
					JE_EntryAuthorisationDate = b3Header.ReleaseDate;
				}
			}
		}

		public ZString CA_MailToMiscFields
		{
			get { return ZString.Empty; }
		}

#if DEBUG
		internal
#endif
		Tuple<IB3Header, ZDateTime> OriginalLodgedB3MessageWrapperAndAccountingDate
		{
			get
			{
				return Factory.GetCachedValue(this.CA_OriginalTransactionNo, delegate
				{
					Tuple<IB3Header, ZDateTime> result = null;
					if (!this.CA_OriginalTransactionNo.IsEmpty)
					{
						var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
						subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, this.CA_OriginalTransactionNo);
						subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CATransactionNumber);
						subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
						var query = new ZDBOnlyQuery(typeof(JobDeclaration));
						query.AddSubQuery(subQuery, JoinCondition.And);
						var originalB3Declaration = Factory.LoadTop1<JobDeclaration>(query);
						if (originalB3Declaration != null)
						{
							var entryHeader = originalB3Declaration.B3EntryHeader;
							if (entryHeader != null)
							{
								var originalB3 = B3Message.GetLastSentAcceptedB3Message(entryHeader);
								if (originalB3 != null)
								{
									result = new Tuple<IB3Header, ZDateTime>(new B3AsLodgedDocumentWrapper(originalB3),
										originalB3Declaration.CA_K84AccountingDate);
								}

								if (result == null && !originalB3Declaration.IsElectronicEntry)
								{
									result = new Tuple<IB3Header, ZDateTime>(new B3ImportMessageWrapper(entryHeader),
										originalB3Declaration.CA_K84AccountingDate);
								}
							}
						}
					}
					return result;
				});
			}
		}

		public ZDateTime OriginalLodgedB3ReleaseDateForB3X
		{
			get
			{
				return Factory.GetCachedValue(this.CA_OriginalTransactionNo, delegate
				{
					var result = ZDateTime.Empty;

					if (!this.CA_OriginalTransactionNo.IsEmpty)
					{
						var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
						subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, this.CA_OriginalTransactionNo);
						subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CATransactionNumber);
						subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
						var query = new ZDBOnlyQuery(typeof(JobDeclaration));
						query.AddSubQuery(subQuery, JoinCondition.And);
						var originalB3Declaration = Factory.LoadTop1<JobDeclaration>(query);
						if (originalB3Declaration != null)
						{
							result = originalB3Declaration.JE_EntryAuthorisationDate;
						}
					}
					return result;
				});
			}
		}

		public void SeedingB2(ClassificationLineWrapperCollection collection)
		{
			var b3HeaderAndAccountingDate = OriginalLodgedB3MessageWrapperAndAccountingDate;

			if (b3HeaderAndAccountingDate != null)
			{
				var b3Header = b3HeaderAndAccountingDate.Item1;
				var suspendActions = new List<Tuple<Action, Action, Action>>();

				foreach (ClassificationLine1Wrapper wrapper in collection.Where(l => l.IsSelected))
				{
					var subHeader = b3Header.PositiveB3SubHeaders.FirstOrDefault(l => l.B3SubHeaderNumber == wrapper.B3SubHeaderNumber);
					if (subHeader != null)
					{
						var invoice = CreateOrGetB2Invoice(subHeader);
						var seededB2AsAccountForLine = invoice.CreateB2AsAccountForLineIfDoesNotExist(wrapper.ClassificationLine);

						if (seededB2AsAccountForLine != null)
						{
							seededB2AsAccountForLine.JI_JZ = invoice.PK;
							var asClaimedForLine = seededB2AsAccountForLine.CorrespondingAsClaimedForInvoiceLine;

							var savedLine = wrapper.ClassificationLine;
							suspendActions.Add(new Tuple<Action, Action, Action>(() => seededB2AsAccountForLine.GetDutyAndTaxFrom(savedLine),
								() => SpecifyQuantitiesForSpecificDuty(seededB2AsAccountForLine),
								() => asClaimedForLine?.OverwriteDutiesAndTaxesFromAsAccountedLine(seededB2AsAccountForLine)));
						}
					}
				}

				this.ResumeApportionment();

				foreach (var tuple in suspendActions)
				{
					tuple.Item1();
					tuple.Item2();
					tuple.Item3();
				}
			}
		}

		public void OverwriteAsClaimedData()
		{
			using (B2AsClaimedForInvoices.SuspendDeleteAsAccountedFromAsClaimed())
			{
				B2AsClaimedForInvoices.DeleteAll();
			}
			DefaultAsAccountedDataIntoAsClaimed();
		}

		public void DefaultAsAccountedDataIntoAsClaimed()
		{
			foreach (var b2AsAccountedForInvoice in B2AsAccountedForInvoices.Cast<JobComInvoiceHeader>())
			{
				b2AsAccountedForInvoice.ReEnableAndSynchronise();
				foreach (var asAccountedForInvoiceLine in b2AsAccountedForInvoice.JobComInvoiceLines.Cast<JobComInvoiceLine>())
				{
					asAccountedForInvoiceLine.ReEnableAndSynchronise();
				}
			}
		}

		void SpecifyQuantitiesForSpecificDuty(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.CA_IsAccountForLine)
			{
				if (invoiceLine.DutiesAndTaxes.Count(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty) > 1)
				{
					var specificDuty = invoiceLine.DutiesAndTaxes.FirstOrDefault(l => l.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty
						&& l.C1_RateType == RateTypes.Codes.Specific);
					if (specificDuty != null && specificDuty.C1_UnitOfMeasure == invoiceLine.JI_CustomsUnitQty && !specificDuty.C1_Amount.IsEmpty
						&& specificDuty.Quantity.IsEmpty)
					{
						if (invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
						{
							invoiceLine.JI_CustomsSecondUnitQty = invoiceLine.JI_CustomsUnitQty;
							invoiceLine.JI_CustomsSecondQuantity = invoiceLine.JI_CustomsQuantity;
						}
						else if (invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
						{
							invoiceLine.JI_CustomsThirdUnitQty = invoiceLine.JI_CustomsUnitQty;
							invoiceLine.JI_CustomsThirdQuantity = invoiceLine.JI_CustomsQuantity;
						}
					}
				}
			}
		}

		JobComInvoiceHeader CreateOrGetB2Invoice(IB3SubHeader subHeader)
		{
			var result = (from JobComInvoiceHeader invoice in this.B2AsAccountedForInvoices
						  where invoice.JZ_InvoiceNumber == subHeader.B3SubHeaderNumber.ToString()
						  select invoice).FirstOrDefault();
			if (result == null)
			{
				result = this.B2AsAccountedForInvoices.AddNew();
				result.CA_IsSeeded = true;
				result.CopyB3SubHeaderToInvoice(subHeader);
			}
			return result;
		}

		public JobComInvoiceHeader CreateOrGetB2AsClaimedInvoice(JobComInvoiceHeader asAccountedInvoice)
		{
			var result = (from JobComInvoiceHeader invoice in this.B2AsClaimedForInvoices
						  where invoice.JZ_InvoiceNumber == asAccountedInvoice.JZ_InvoiceNumber.ToString() && invoice.CA_IsSeeded
						  select invoice).FirstOrDefault();
			if (result == null)
			{
				result = this.B2AsClaimedForInvoices.AddNew();
				result.CA_IsSeeded = true;
				result.GetDetailsFrom(asAccountedInvoice);
			}
			return result;
		}

		public ClassificationLineWrapperCollection GetOriginalB3Lines()
		{
			var b3Header = OriginalLodgedB3MessageWrapperAndAccountingDate;
			return b3Header != null ? new ClassificationLineWrapperCollection(b3Header.Item1) : null;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this, true);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CACCN, typeof(CargoControlNumber));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.Permit, typeof(DeclarationExportPermit));
			return result;
		}

		#endregion

		#region B13A Documents
		public ZString InvoiceNumbers
		{
			get { return this.Invoices.Select(x => x.JZ_InvoiceNumber).ConcatenateWithPageDelimiter(",", 110, 360); }
		}

		#endregion

		#region auto-rating support

		protected override BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return IsB2OrIM2OrB3X ? new B2bDeclarationInvoicingSupporter(this) : new JobDeclarationInvoicingSupporter(this);
		}

		public class B2bDeclarationInvoicingSupporter : JobDeclarationInvoicingSupporter
		{
			public B2bDeclarationInvoicingSupporter(JobDeclaration parent)
				: base(parent)
			{
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return JobInvoicingConsumerTypes.PostClearanceBrokerage; }
			}
		}

		public class JobDeclarationInvoicingSupporter : BaseJobDeclarationInvoicingSupporter
		{
			public JobDeclarationInvoicingSupporter(JobDeclaration parent)
				: base(parent)
			{
				this.parent = parent;
			}
			protected readonly JobDeclaration parent;

			public override RefUNLOCO Destination
			{
				get
				{
					if (!CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.Value)
					{
						if (parent.IsLVX)
						{
							var province = parent.ImporterDeliveryAddress?.StateCode ?? ZString.Empty;
							if (!province.IsEmpty)
							{
								return parent.Factory.GetCachedValue(province, () => CalculateFixedPlaceOfSupplyBasedOnProvince(province));
							}
							else
							{
								var portOfClearance = parent.LVXInvoiceHeader.CA_PortOfClearance;
								return parent.Factory.GetCachedValue(portOfClearance, () => CalculateFixedPlaceOfSupply(portOfClearance));
							}
						}
						return base.Destination;
					}
					return CalculateLocation();
				}
			}
			public override ILocation FixedPlaceOfSupply
			{
				get
				{
					ILocation result = null;
					if (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value)
					{
						if (!CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.Value)
						{
							var portOfClearance = parent.JE_CustomsOffice;
							result = parent.Factory.GetCachedValue(portOfClearance, () => CalculateFixedPlaceOfSupply(portOfClearance));
						}
						else
						{
							result = CalculateLocation();
						}
					}
					return result;
				}
			}

			RefUNLOCO CalculateLocation()
			{
				RefUNLOCO result = null;
				if (parent.IsImport)
				{
					var casualImportProvinces = parent.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.CA_IsCasualImport && !x.CA_IsAutoDummyHSCodeCasualImportLine && !x.CA_CasualImportDestinationProvince.IsEmpty).Select(y => y.CA_CasualImportDestinationProvince);
					if (casualImportProvinces.Any() && casualImportProvinces.AllSame(x => x))
					{
						var casualProvince = casualImportProvinces.First();
						result = parent.Factory.GetCachedValue(casualProvince, () => CalculateFixedPlaceOfSupplyBasedOnProvince(casualProvince));
					}
					else if (!parent.JE_CustomsOffice.IsEmpty)
					{
						var portOfClearance = parent.JE_CustomsOffice;
						result = parent.Factory.GetCachedValue(portOfClearance, () => CalculateFixedPlaceOfSupply(portOfClearance));
					}
				}
				else
				{
					result = base.Destination;
				}
				return result;
			}

			public override RefUNLOCO Origin
			{
				get
				{
					if (!CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.Value)
					{
						return base.Origin;
					}
					return null;
				}
			}

			public override OrgHeader Consignee
			{
				get
				{
					return parent.IsLVSTotalConsolidation ? null : base.Consignee;
				}
			}

			public override OrgHeader OverriddenDefaultLocalClient
			{
				get
				{
					return parent.IsLVSTotalConsolidation && parent.Branch != null ? parent.Branch.OrgProxy : base.OverriddenDefaultLocalClient;
				}
			}

			protected override ZGuid OverridenDepartment
			{
				get
				{
					if (JobInvoicingTransportMode == TransportTypeList.Codes.FixedTransportInstallations
						|| JobInvoicingTransportMode == TransportTypeList.Codes.NoCarrier
						|| parent.IsB2Adjustments || parent.IsB3X)
					{
						return IsImport ? CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsImportOther : CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsOther;
					}

					return base.OverridenDepartment;
				}
			}

			RefUNLOCO CalculateFixedPlaceOfSupply(string portOfClearance)
			{
				RefUNLOCO result = null;

				var officeCode = parent.GetOfficeCode(portOfClearance);
				var provinceCode = officeCode?.GetAttribute(RefCusCodeListAttributes.Province) ?? ZString.Empty;

				result = CalculateFixedPlaceOfSupplyBasedOnProvince(provinceCode);

				return result;
			}

			RefUNLOCO CalculateFixedPlaceOfSupplyBasedOnProvince(ZString provinceCode)
			{
				RefUNLOCO result = null;
				if (!provinceCode.IsEmpty)
				{
					result = FindPortByTaxZone(provinceCode) ?? FindPortByCountryState(provinceCode);
				}

				return result;
			}

			RefUNLOCO FindPortByTaxZone(string provinceCode)
			{
				var sqlFilter = @" RL_Code =
(
	select top 1 RL_Code
	from dbo.refzoneheader
	join dbo.refzonepivot on f2_fz = fz_pk
	join dbo.refunloco on f2_parentid = rl_pk
	join dbo.refcountrystates on rl_rw = rw_pk
	where fz_zonetype = @ZoneType and RW_Code = @FixedPlaceOfSupplyState and RL_RN_NKCountryCode = @CurrentCompanyCountry and RL_IsActive = 1
	order by RL_Code
)";
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@FixedPlaceOfSupplyState", provinceCode, RefCountryStatesSchema.RW_Code));
				sqlParams.Add(ZSqlParameter.New("@ZoneType", RefZoneHeaderLookups.ZoneTypeCodes.Tax, RefZoneHeaderSchema.FZ_ZoneType));
				sqlParams.Add(ZSqlParameter.New("@CurrentCompanyCountry", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefUNLOCOSchema.RL_RN_NKCountryCode));

				var query = new ZDBOnlyQuery(typeof(RefUNLOCO));
				query.AddFilterAndZSQLParameterCollection(sqlFilter, sqlParams);
				query.IgnoreActiveFilter = true;

				return parent.Factory.Load<RefUNLOCO>(query).FirstOrDefault();
			}

			RefUNLOCO FindPortByCountryState(string provinceCode)
			{
				var sqlFilter = @" RL_Code =
(
	select top 1 RL_Code
	from dbo.refunloco join
	dbo.refcountrystates on rl_rw = rw_pk
	where rw_code = @FixedPlaceOfSupplyState and RL_RN_NKCountryCode = @CurrentCompanyCountry and RL_IsActive = 1
	order by RL_Code
)";
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@FixedPlaceOfSupplyState", provinceCode, RefCountryStatesSchema.RW_Code));
				sqlParams.Add(ZSqlParameter.New("@CurrentCompanyCountry", GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString(), RefUNLOCOSchema.RL_RN_NKCountryCode));

				var query = new ZDBOnlyQuery(typeof(RefUNLOCO));
				query.AddFilterAndZSQLParameterCollection(sqlFilter, sqlParams);
				query.IgnoreActiveFilter = true;

				return parent.Factory.Load<RefUNLOCO>(query).FirstOrDefault();
			}
		}

		protected override RatingAdaptersProvider GetRatingAdaptersProviderCore()
		{
			return IsConsolidatedLVS ? new LVSDeclarationRatingAdaptersProvider(this) : new JobDeclarationRatingAdaptersProvider(this);
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new JobDeclarationRatingAdapter<JobDeclaration>(this);
		}

		#endregion

		public override bool EnableAttachCommercialInvoice
		{
			get { return !IsIM2 && !IsLVS; }
		}

		public override bool EnableCopyCommercialInvoice
		{
			get
			{
				return !IsIM2 && !IsLVS;
			}
		}

		public override bool EnableImportInvoices
		{
			get { return !IsIM2; }
		}

		public ZString BrokerBusinessNumber
		{
			get
			{
				if (brokerBusinessNumberCached == null)
				{
					brokerBusinessNumberCached = new CachedProperty<ZString>(Factory, () =>
					{
						return IB3HeaderHelper.GetBrokerBusinessNumber(GetOrgProxyWithCABusinessNumber());
					});
				}
				return brokerBusinessNumberCached.Value;
			}
		}
		CachedProperty<ZString> brokerBusinessNumberCached;

		#region Document Override Properties and Methods

		protected override ZBool SupportValidateCustomsMessagingCore
		{
			get { return true; }
		}

		protected override ZString GetOwnersRefOverrideForDocumentsCore(OrgHeader debtor)
		{
			if (IsConsolidatedLVS)
			{
				return new ZStringBuilder((IsLVSTotalConsolidation ? Invoices.Where(x => debtor != null && x.Importer_Effective != null && x.Importer_Effective.DeliveryCustomsBillTo.PK == debtor.PK) : Invoices).Select(x => x.JZ_InvoiceNumber)).ToStringWithDelimiterBetweenAppends(",");
			}
			else if (IsB2Adjustments)
			{
				return B2DocumentsAndInvoiceOwnerRef;
			}
			else if (IsB3X)
			{
				return B3XDocumentsAndInvoiceOwnerRef;
			}
			else if (IsLVX)
			{
				return LVXDocumentsAndInvoiceOwnerRef;
			}
			return base.GetOwnersRefOverrideForDocumentsCore(debtor);
		}

		protected override ZString DeliveryAddressCore()
		{
			if (IsLVX && ImporterAddInfo != null && ImporterAddInfo.ZO_PrintDeliveryAddressForLVX)
			{
				return LVXInvoiceHeader.JobDeclaration.ImporterDeliveryAddress.AddressAsASingleLine;
			}
			return base.DeliveryAddressCore();
		}

		public ZString B2DocumentsAndInvoiceOwnerRef
		{
			get
			{
				return Res.GetString("204a1d95-d672-46c4-b9d2-dd92c812f6e0", "B2 Transaction # {0}", DeclarationNumber);
			}
		}

		public ZString B3XDocumentsAndInvoiceOwnerRef
		{
			get
			{
				return Res.GetString("CFB32D75-4F89-48BA-BC91-C2CAADEDB49F", "B3X Transaction # {0}", DeclarationNumber);
			}
		}

		public ZString LVXDocumentsAndInvoiceOwnerRef
		{
			get
			{
				return Res.GetString("004c72a8-2b24-4fb7-9d96-816fa3ded73a", "LVS ID: {0}", LVXInvoiceHeader.JZ_InvoiceNumber);
			}
		}

		protected override ZString OriginForDocumentsCore
		{
			get { return IsLVX ? ZString.Empty : base.OriginForDocumentsCore; }
		}

		protected override ZString FinalDestinationForDocumentsCore
		{
			get { return IsConsolidatedLVS || IsB2Adjustments || IsB3X || IsLVX ? ZString.Empty : base.FinalDestinationForDocumentsCore; }
		}

		protected override bool ShowInvoiceNumbersOnDocumentsCore
		{
			get { return !(IsConsolidatedLVS || IsB2Adjustments || IsB3X || IsLVX) && base.ShowInvoiceNumbersOnDocumentsCore; }
		}

		public ZString GoodsDescriptionForDocumentsAndInvoice(ZString baseValue)
		{
			if (IsConsolidatedLVS)
			{
				return new ZString(Res.GetString("f4366914-7cf3-4c86-9564-3162cd18ce4b", "Various low value shipments"));
			}
			else if (IsBlanketB2)
			{
				return Res.GetString("d59b060c-e711-4a63-a1c3-37cd34cf033c", "Blanket B2");
			}
			else if (IsB2Adjustments)
			{
				return Res.GetString("33fb48d8-e929-4f9f-b6ae-2730a93cb4cb", "B2 for Original Transaction # {0}", CA_OriginalTransactionNo);
			}
			else if (IsB3X)
			{
				return Res.GetString("88B02AB1-89BC-4FCC-9142-ADFEB8E35EE2", "B3X for Original Transaction # {0}", CA_OriginalTransactionNo);
			}
			else if (IsLVX)
			{
				return Res.GetString("7f7ef701-3508-4b44-8452-869e95e021d8", "Low Value Shipment");
			}

			return baseValue;
		}

		protected override ZString GoodsDescriptionForDocumentsCore
		{
			get
			{
				return GoodsDescriptionForDocumentsAndInvoice(base.GoodsDescriptionForDocumentsCore);
			}
		}

		public override bool AllowEntryLinesToBeLinkedToAnotherJob => IsConsolidatedLVS;

		#endregion

		#region IAdditionalReferenceNumberSupporter

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return AdditionalReferenceNumbers; }
		}

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return false; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
			if (additionalReferenceNumber.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
			{
				CreateOrUpdateCargoControlNumberOnPackingTab(additionalReferenceNumber);
				EffectiveCCNInfo.RefreshBinding();
			}
		}

		void CreateOrUpdateCargoControlNumberOnPackingTab(CusEntryNumber cusEntryNumber)
		{
			try
			{
				IsSettingEntryNum = true;
				var ccNumber = CargoControlNumbers.FirstOrDefault(ccn => ccn.CA_IsFromNumbersTab);
				if (ccNumber == null)
				{
					ccNumber = CargoControlNumbers.AddNew();
					ccNumber.CA_IsFromNumbersTab = true;
				}
				ccNumber.CY_CargoControlNumber = cusEntryNumber.CE_EntryNum.Left(ccNumber.CY_CargoControlNumberInfo.MaxLength);
			}
			finally
			{
				IsSettingEntryNum = false;
			}
		}

		public bool IsSettingEntryNum;

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
			if (type == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
			{
				ReleaseStatusValidation.ValidateCCNNumber(info, number, this);
			}
		}

		#endregion

		#region Accounting and Other Total Amounts

		protected override List<ZGuid> EntryChargeTypeCodesToMatchCore
		{
			get { return Factory.GetCachedValue<EntryChargeTypeList>().GetAllChargeCodePKsOf(Branch.Company.PK).ToList(); }
		}

		public ZDecimal TotalNormalDuty
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsB3X)
				{
					foreach (JobComInvoiceHeader invoice in B2AsClaimedForInvoices)
					{
						result += invoice.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_DutyAmount);
					}
					foreach (JobComInvoiceHeader invoice in B2AsAccountedForInvoices)
					{
						result -= invoice.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_DutyAmount);
					}
				}
				else
				{
					result = B3EntryHeader?.TotalDutyAmount ?? ZDecimal.Zero;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalNormalDutyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalNormalDuty); }
		}

		public ZDecimal TotalGST
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsB3X)
				{
					foreach (JobComInvoiceHeader invoice in B2AsClaimedForInvoices)
					{
						result += invoice.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_GSTAmount);
					}
					foreach (JobComInvoiceHeader invoice in B2AsAccountedForInvoices)
					{
						result -= invoice.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_GSTAmount);
					}
				}
				else
				{
					result = B3EntryHeader?.GSTAmount ?? ZDecimal.Zero;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalGSTInfo
		{
			get { return GetZPropertyInfo(Schema.TotalGST); }
		}

		public ZDecimal TotalSimaDuty
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsB3X)
				{
					foreach (JobComInvoiceHeader invoice in B2AsClaimedForInvoices)
					{
						result += invoice.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_SIMADutyAmount);
					}
					foreach (JobComInvoiceHeader invoice in B2AsAccountedForInvoices)
					{
						result -= invoice.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_SIMADutyAmount);
					}
				}
				else
				{
					result = B3EntryHeader?.TotalSimaDuty ?? ZDecimal.Zero;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalSimaDutyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalSimaDuty); }
		}

		public ZDecimal TotalExciseTax
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsB3X)
				{
					foreach (JobComInvoiceHeader invoice in B2AsClaimedForInvoices)
					{
						result += invoice.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_ExciseTaxesAmount);
					}
					foreach (JobComInvoiceHeader invoice in B2AsAccountedForInvoices)
					{
						result -= invoice.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_ExciseTaxesAmount);
					}
				}
				else
				{
					result = B3EntryHeader?.TotalExciseTax ?? ZDecimal.Zero;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalExciseTaxInfo
		{
			get { return GetZPropertyInfo(Schema.TotalExciseTax); }
		}

		public ZDecimal TotalDutyAndTax
		{
			get { return B3EntryHeader?.TotalDutyAndTax ?? ZDecimal.Zero; }
		}

		public ZPropertyInfo TotalDutyAndTaxInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDutyAndTax); }
		}

		public ZDecimal TotalAmountPayable
		{
			get { return B3EntryHeader?.TotalAmountPayable ?? ZDecimal.Zero; }
		}

		public ZPropertyInfo TotalAmountPayableInfo
		{
			get { return GetZPropertyInfo(Schema.TotalAmountPayable); }
		}

		#endregion

		#region CA B3 Deferred Sending Part

		public bool HasScheduledB3Message
		{
			get { return this.IsImport && ScheduledB3MessageTime.IsValid; }
		}

		public ZDateTime ScheduledB3MessageTime
		{
			get
			{
				var entryHeader = B3EntryHeader;
				var scheduledTime = entryHeader == null ? ZDateTime.Empty : entryHeader.DeferredB3MessageTime;
				return scheduledTime.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(scheduledTime.ToDateTime()) : ZDateTime.Empty;
			}
		}

		#endregion

		#region ConvertLVXToNormalDeclaration

		public void ConvertLVXToNormalDeclaration()
		{
			if (IsLVX)
			{
				var lvxInvoice = LVXInvoiceHeader;
				using (SuspendMessageTypeChangeProcess())
				{
					JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				JE_TransportMode = ZString.Empty;
				JE_CarrierCode = lvxInvoice.CA_CarrierCode;
				JE_OwnerRef = lvxInvoice.CA_OtherReference.Replace("\r\n", ",");
				if (JE_TransportMode.IsEmpty)
				{
					JE_TransportMode = Core.Constants.TransportModes.Road;
				}

				JE_EntryAuthorisationDate = ZDateTime.Empty;
				CA_AccountingAge = ZInt.Zero;
			}
		}

		#endregion

		#region StmNote

		void SetOrCreateNoteText(string predefinedNoteTypeDescription, ZString value)
		{
			var note = GetNoteCore(predefinedNoteTypeDescription);
			if (note != null)
			{
				note.ST_NoteText = value;
			}
			else
			{
				EffectiveNote.AddNew(false, predefinedNoteTypeDescription, value);
			}
		}

		ZString GetNoteText(string predefinedNoteTypeDescription)
		{
			var note = GetNoteCore(predefinedNoteTypeDescription);
			if (note != null)
			{
				return note.ST_NoteText;
			}
			else
			{
				return ZString.Empty;
			}
		}

		StmNote GetNoteCore(string predefinedNoteTypeDescription)
		{
			return EffectiveNote.FindByDescription(predefinedNoteTypeDescription).FirstOrDefault();
		}

		bool IStmNoteParentWithSystemNote.IsSystemNote(StmNote note)
		{
			return note != null
				&& (note.ST_Description == PredefinedNoteTypes.Instance.ManualRelease.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.ManualCancel.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.ManualSubmission.Description);
		}

		Notes EffectiveNote => notes ?? (notes = (IsPluggedIntoShipment && Shipment != null) ? Shipment.Notes : Notes);
		Notes notes;

		#endregion

		#region IManualSubmissionSupport

		ZString ManualSubmissionNoteType
		{
			get { return PredefinedNoteTypes.Instance.ManualSubmission.Description; }
		}

		public StmNote ManualSubmissionNote
		{
			get { return GetNoteCore(ManualSubmissionNoteType); }
		}

		ZString ManualSubmissionNoteText
		{
			get { return GetNoteText(ManualSubmissionNoteType); }
			set
			{
				SetOrCreateNoteText(ManualSubmissionNoteType, value);
			}
		}

		void IManualSubmissionSupport.ManualSubmission(IManualSubmissionNote[] submissionNotes)
		{
			var noteText = new ZStringBuilder();
			foreach (var note in submissionNotes)
			{
				var entryHeader = note.MessageType == MessageTypeList.Codes.B3CUSDEC ? B3EntryHeader : ReleaseEntryHeader;
				if (entryHeader != null)
				{
					if (note.IsDeleted)
					{
						entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
						entryHeader.CA_PortOfClearanceOverride = ZString.Empty;
					}
					else
					{
						entryHeader.CH_EntrySubmittedDate = note.ManualSubmissionDate;
						entryHeader.CA_PortOfClearanceOverride = note.PortOfClearanceOverride;

						if (!note.ManualSubmissionDate.IsEmpty)
						{
							noteText.AppendIfNotEmpty(note.NoteText);
						}
					}
				}
			}
			if (noteText.Length > 0)
			{
				ManualSubmissionNoteText = noteText.ToStringWithNewLineBetweenAppends();
			}
			else if (ManualSubmissionNote != null)
			{
				ManualSubmissionNote.Delete();
			}
		}

		ZString IManualSubmissionSupport.ManualSubmissionNoteText
		{
			get { return ManualSubmissionNoteText; }
		}

		ZString IManualSubmissionSupport.GetReasonForCannotManualSubmission(ZString messageType)
		{
			var result = ZString.Empty;
			var entryHeader = messageType == MessageTypeList.Codes.B3CUSDEC ? B3EntryHeader : ReleaseEntryHeader;
			if (entryHeader == null)
			{
				result = Res.GetString("3E8BFBA3-5E0E-4D26-9E41-C1817473EC09",
					"Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.");
			}
			else if (entryHeader.Messages.Any())
			{
				result = Res.GetString("525A40FF-2603-43CD-B3BD-DD5757ADED1A",
					"This job has already been submitted, you cannot mark it for Manual submission.");
			}
			return result;
		}

		#endregion

		#region IManualReleaseSupport

		ZString ReleaseNoteType
		{
			get { return PredefinedNoteTypes.Instance.ManualRelease.Description; }
		}

		public StmNote ManualReleaseNote
		{
			get { return GetNoteCore(ReleaseNoteType); }
		}

		public ZString ManualReleaseText
		{
			get { return GetNoteText(ReleaseNoteType); }
			set
			{
				SetOrCreateNoteText(ReleaseNoteType, value);
			}
		}

		void IManualReleaseSupport.DeleteManualRelease()
		{
			ManualReleaseNote.Delete();
			if (ReleaseEntryHeader is CusEntryHeader entryHeader)
			{
				entryHeader.CH_EntryStatus = ZString.Empty;
				entryHeader.CH_EntryReleaseDate = ZDate.Empty;
			}
			JE_EntryAuthorisationDate = ZDateTime.Empty;
			JE_EntryStatus = ZString.Empty;
			CA_AccountingAge = ZInt.Zero;
		}

		void IManualReleaseSupport.ManualRelease(IManualReleaseNote note)
		{
			ManualReleaseText = note.NoteText;
			if (ReleaseEntryHeader is CusEntryHeader entryHeader)
			{
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.ManualRelease;
				entryHeader.CH_EntryReleaseDate = note.ManualReleaseDate;
			}
			JE_EntryAuthorisationDate = note.ManualReleaseDate;
			JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.ManualRelease;
		}

		ZString IManualReleaseSupport.ManualReleaseNoteText
		{
			get { return ManualReleaseText; }
		}

		ZString IManualReleaseSupport.GetReasonForCannotManualRelease()
		{
			var result = ZString.Empty;
			if (ReleaseEntryHeader == null)
			{
				result = Res.GetString("3E8BFBA3-5E0E-4D26-9E41-C1817473EC09",
					"Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.");
			}
			else if (JE_EntryAuthorisationDate.IsValid && JE_EntryStatus != EDIReleaseImportEntryStatusList.Codes.ManualRelease)
			{
				result = Res.GetString("4121B9E5-B517-44C9-AD17-3BB40E886B5D", "You don't need to manually release for a released job.");
			}
			else if (B3AcceptedDate.IsValid)
			{
				result = Res.GetString("32D9A9E6-EE1B-41F4-99B5-CEA01822DA67", "Entry message for this shipment has already been accepted, it cannot be manually released again.");
			}
			else if ((JE_LocationOfGoods.IsEmpty || CA_SubLocationName.IsEmpty) && !ReleaseStatuses.Any() && JE_MessageSubType != (IsCADEnabled ? CADEntryTypeList.Codes.ExWarehouse201 : B3EntryTypeList.Codes.ExWarehouse20))
			{
				result = Res.GetString("A8B37C94-4FF8-4ADF-8875-0194CB1C5C92", "Cargo Control number and sub-location code or the text identifying cargo location are not specified.");
			}
			return result;
		}

		#endregion

		#region IManualCancelSupport

		ZString CancelNoteType
		{
			get { return PredefinedNoteTypes.Instance.ManualCancel.Description; }
		}

		public StmNote ManualCancelNote
		{
			get { return GetNoteCore(CancelNoteType); }
		}

		public ZString ManualCancelText
		{
			get { return GetNoteText(CancelNoteType); }
			set
			{
				SetOrCreateNoteText(CancelNoteType, value);
			}
		}

		void IManualCancelSupport.ManualCancel(IManualReleaseNote note)
		{
			ManualCancelText = note.NoteText;
			if (ReleaseEntryHeader is CusEntryHeader entryHeader)
			{
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			}
			JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			JE_EntryAuthorisationDate = ZDateTime.Empty;
		}

		ZString IManualCancelSupport.ManualCancelNoteText
		{
			get { return ManualCancelText; }
		}

		ZString IManualCancelSupport.GetReasonForCannotManualCancel()
		{
			var result = ZString.Empty;
			if (ReleaseEntryHeader == null)
			{
				result = Res.GetString("7124A320-541C-4B08-A252-0D5FB7C75756",
					"Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.");
			}
			return result;
		}

		#endregion

		#region IJobInvoicingPlugInAdditionalJobs Members

		IJobInvoicingPlugIn[] IJobInvoicingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor
		{
			get
			{
				var list = new List<IJobInvoicingPlugIn>();
				if (JE_MessageType == JobMessageTypeList.Codes.LowValueShipments && Invoices.Count > 0)
				{
					list.AddRange(from JobComInvoiceHeader invoice in this.Invoices where invoice.JobDeclaration.IsLVX select (IJobInvoicingPlugIn)invoice.JobDeclaration);
				}
				return list.ToArray();
			}
		}
		#endregion

		#region Accounting Integration
		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			base.OnIntegratedWithAccountingSuccessfully();
			CA_JobReadyForPost = false;
		}

		protected override bool IsJobReadyForPost
		{
			get { return CA_JobReadyForPost; }
		}
		#endregion

		#region Copy for B2

		public JobDeclaration OriginalDeclaration
		{
			get
			{
				if (originalDeclaration == null)
				{
					var genAddOnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					genAddOnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);
					genAddOnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
					genAddOnQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, this.CA_OriginalTransactionNo);
					var query = new ZDBOnlyQuery(typeof(JobDeclaration));
					query.AddSubQuery(JobDeclarationSchema.PK, CusEntryNumSchema.CE_ParentID, genAddOnQuery, JoinCondition.And);
					originalDeclaration = Factory.Load<JobDeclaration>(query).ToArray().OrderBy(declaration => declaration.CA_Version).FirstOrDefault();
				}
				return originalDeclaration;
			}
		}
		JobDeclaration originalDeclaration;

		public ZString PreviousTransactionNumber
		{
			get { return (PreviousJob != null && PreviousJob.IsIM2) ? PreviousJob.TransactionNumber : ZString.Empty; }
		}

		public ZString PreviousJobNumber
		{
			get { return (PreviousJob != null && PreviousJob.IsIM2) ? PreviousJob.JE_DeclarationReference : ZString.Empty; }
		}

		public JobDeclaration PreviousJob
		{
			get { return previousJob ?? (previousJob = Factory.Load<JobDeclaration>(CA_JE_PreviousJob)); }
		}
		JobDeclaration previousJob;

		public JobDeclaration GetNewCopyToPRECARMAdjustmentDeclaration()
		{
			var sourceDeclaration = this;
			var newCopyToPRECARMAdjustmentDeclaration = (JobDeclaration)sourceDeclaration.GetNewRelatedDeclaration(Factory, PRECARMAdjustmentDeclarationRelationshipType);
			newCopyToPRECARMAdjustmentDeclaration.Invoices.DeleteAll();
			((IBusinessObjectInternals)newCopyToPRECARMAdjustmentDeclaration).IsCopying = true;
			try
			{
				newCopyToPRECARMAdjustmentDeclaration.CA_OriginalTransactionNo = sourceDeclaration.TransactionNumber;
				newCopyToPRECARMAdjustmentDeclaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
				newCopyToPRECARMAdjustmentDeclaration.JE_EntryAuthorisationDate = sourceDeclaration.JE_EntryAuthorisationDate;

				var wrapper = new B3ImportMessageWrapper(B3EntryHeader, true);

				foreach (var b3SubHeader in wrapper.GetB3SubHeaders())
				{
					var newInvoiceHeader = newCopyToPRECARMAdjustmentDeclaration.Invoices.AddNew();
					newInvoiceHeader.CopyB3SubHeaderToInvoice(b3SubHeader);
					var header = b3SubHeader.InvoiceHeader;
					newInvoiceHeader.JZ_OH_Supplier = header.JZ_OH_Supplier;
					newInvoiceHeader.JZ_ValuationDateOverride = header.JZ_ValuationDateOverride;
					newInvoiceHeader.JZ_InvoiceAmount = b3SubHeader.Lines.Sum(x => x.LinePriceForBalanceCalc);
					newInvoiceHeader.JZ_RX_NKInvoice_Currency = header.JZ_RX_NKInvoice_Currency;
					newInvoiceHeader.CA_USPortOfExit = header.CA_USPortOfExit;
					newInvoiceHeader.CA_ValueForDutyCode = header.CA_ValueForDutyCode;

					foreach (var classificationLine1 in b3SubHeader.GetB3SubHeaderLines())
					{
						var newInvoiceLine = newInvoiceHeader.JobComInvoiceLines.AddNew();
						newInvoiceLine.CopyB3SubHeaderLineToInvoiceLine(classificationLine1);
						newInvoiceLine.CA_OriginalLineNo = ZString.Empty;
						if (classificationLine1 is CusEntryLine line)
						{
							newInvoiceLine.JI_LineNo = line.CL_LineNumber;
							newInvoiceLine.JI_CountryOfOrigin = line.CountryOfOrigin?.Code ?? ZString.Empty;
							newInvoiceLine.JI_StateOrRegionOfOrigin = line.RandomLine.JI_StateOrRegionOfOrigin;
							newInvoiceLine.JI_LinePrice = line.TotalLinePrice.Amount;
							newInvoiceLine.JI_CustomsQuantity = line.CustomsQuantity;
							newInvoiceLine.JI_CustomsUnitQty = line.CustomsUnitQty;
							newInvoiceLine.JI_InvoiceUQ = line.InvoiceUQ;
							newInvoiceLine.JI_InvoiceQuantity = line.InvoiceQuantity;
							newInvoiceLine.CA_TreatmentCode = line.RandomLine.CA_TreatmentCode;
							newInvoiceLine.CA_TRSNumber = classificationLine1.TRSNumber;
							newInvoiceLine.CA_CalculationMethod = line.RandomLine.CA_CalculationMethod;

							var dutyAndTaxesToCopy = line.InvoiceLines.OfType<JobComInvoiceLine>()
								.SelectMany(x => x.DutiesAndTaxes).GroupBy(x => x.C1_TaxType);
							foreach (var group in dutyAndTaxesToCopy)
							{
								var newDutyAndTax = newInvoiceLine.DutiesAndTaxes.AddNew();
								newDutyAndTax.C1_Override = true;
								newDutyAndTax.C1_TaxType = group.Key;
								newDutyAndTax.C1_Amount = group.Sum(x => x.C1_Amount);
								newDutyAndTax.C1_ExemptCode = group.FirstOrDefault(x => !x.C1_ExemptCode.IsEmpty)?.C1_ExemptCode ?? ZString.Empty;
								newDutyAndTax.C1_Code = group.FirstOrDefault(x => !x.C1_Code.IsEmpty)?.C1_Code ?? ZString.Empty;
								newDutyAndTax.C1_UnitOfMeasure = group.FirstOrDefault(x => !x.C1_UnitOfMeasure.IsEmpty)?.C1_UnitOfMeasure ?? ZString.Empty;
							}
						}
					}
				}
			}
			finally
			{
				((IBusinessObjectInternals)newCopyToPRECARMAdjustmentDeclaration).IsCopying = false;
			}

			return newCopyToPRECARMAdjustmentDeclaration;
		}
		public const string PRECARMAdjustmentDeclarationRelationshipType = "PRE";

		protected override string[] RelatedDeclarationTypes => new[] { string.Empty, PRECARMAdjustmentDeclarationRelationshipType };

		public bool HasPRECARMAdjustmentDeclaration()
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, RelatedDeclarations.Select(x => x.PK));
			query.AddToFilter(GenPivotSchema.XX_RelationType, PRECARMAdjustmentDeclarationRelationshipType);
			return Factory.Exists(typeof(GenPivot), query);
		}

		public JobDeclaration GetNextVersionNumberDeclaration()
		{
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CAAddInfoSchema.Constants.CA_OriginalTransactionNo);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, IsIM2 ? CA_OriginalTransactionNo : TransactionNumber.ToString());
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddSubQuery(JobDeclarationSchema.PK, GenAddOnColumnSchema.XA_ParentID, genAddOnQuery, JoinCondition.And);
			query.AddToFilter(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.Like, "%Version=%");
			query.IgnoreActiveFilter = true;
			var declarations = Factory.Load<JobDeclaration>(query);
			return declarations.Length > 0 ? declarations.OrderByDescending(declaration => declaration.CA_Version).FirstOrDefault() : null;
		}

		public JobDeclaration GetNewCopyToB2Declaration()
		{
			var sourceDeclaration = this;
			var newCopyToB2Declaration = (JobDeclaration)new CAJobDeclarationDeepCloneStrategy(sourceDeclaration, CloneType.DeepTemplateCopy, Factory).Clone();
			((IBusinessObjectInternals)newCopyToB2Declaration).IsCopying = true;
			try
			{
				var sourceIsImport = sourceDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import;
				newCopyToB2Declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				newCopyToB2Declaration.LockDownMessageTypeIfNeed();
				newCopyToB2Declaration.TransactionNumber.Delete();
				newCopyToB2Declaration.CA_JE_PreviousJob = sourceDeclaration.PK;

				if (sourceDeclaration.ImporterOfRecord is OrgHeader importerOfRecord)
				{
					newCopyToB2Declaration.JE_OH_Importer = importerOfRecord.PK;
					newCopyToB2Declaration.JE_OA_ImporterAddress = importerOfRecord.MainAddress.PK;
				}
				var transactionNo = ZString.Empty;
				var caVersion = 2;
				if (sourceIsImport)
				{
					newCopyToB2Declaration.CA_AmendmentTo = AmendmentToList.Codes.OriginalB3;
					transactionNo = sourceDeclaration.TransactionNumber;
				}
				else
				{
					newCopyToB2Declaration.CA_AmendmentTo = AmendmentToList.Codes.B2;
					if (sourceDeclaration.IsIM2)
					{
						transactionNo = sourceDeclaration.CA_OriginalTransactionNo;
						caVersion = sourceDeclaration.CA_Version + 1;
					}
				}

				var nextDec = GetNextVersionNumberDeclaration();
				newCopyToB2Declaration.CA_Version = (nextDec != null && nextDec.IsCancelled) ? (nextDec.CA_Version + 1) : caVersion;
				newCopyToB2Declaration.CA_OriginalTransactionNo = transactionNo;
				newCopyToB2Declaration.CA_B2SubmissionDate = ZDateTime.Empty;
				newCopyToB2Declaration.CA_K84AccountingDate = ZDateTime.Empty;
				newCopyToB2Declaration.CA_B2Type = B2TypeList.Codes.Specific;
				newCopyToB2Declaration.CA_ConfirmedDate = ZDateTime.Empty;
				newCopyToB2Declaration.CA_B2AcceptedDate = ZDateTime.Empty;
				newCopyToB2Declaration.JE_EntryAuthorisationDate = sourceDeclaration.JE_EntryAuthorisationDate;
				newCopyToB2Declaration.JE_GS_NKCusAgent = sourceDeclaration.JE_GS_NKCusAgent;
				newCopyToB2Declaration.CA_MergeBy = sourceDeclaration.CA_MergeBy;
				newCopyToB2Declaration.CA_DeclarationException = ZString.Empty;
				newCopyToB2Declaration.CA_OriginalAccountingDate = sourceDeclaration.CA_K84AccountingDate;
				newCopyToB2Declaration.CA_K84StatementDate = ZDateTime.Empty;
				newCopyToB2Declaration.CA_WoodPackagingInd = ZBool.False;
				newCopyToB2Declaration.CA_PermitApplication = ZBool.False;
				newCopyToB2Declaration.CA_InspectionArrangementsComplete = ZBool.False;
				newCopyToB2Declaration.CA_CSAEntry = ZBool.False;
				newCopyToB2Declaration.CA_ATDExCode = ZString.Empty;
				newCopyToB2Declaration.CA_AmendReasonCode = ZString.Empty;
				newCopyToB2Declaration.CA_OGDCFIA = ZBool.False;
				newCopyToB2Declaration.CA_OGDIC = ZBool.False;
				newCopyToB2Declaration.CA_OGDNR = ZBool.False;
				newCopyToB2Declaration.CA_OGDTC = ZBool.False;

				if (newCopyToB2Declaration.B3EntryHeader is CusEntryHeader entryHeader)
				{
					entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
				}

				foreach (var invoice in sourceDeclaration.InvoiceLines.OfType<JobComInvoiceLine>())
				{
					var newinvoice = newCopyToB2Declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.InvoiceHeader.JZ_InvoiceNumber == invoice.InvoiceHeader.JZ_InvoiceNumber
						&& x.JI_LineNo == invoice.JI_LineNo);
					if (newinvoice != null)
					{
						if (sourceIsImport)
						{
							newinvoice.CA_PreviousB3LineNo = ZInt.ParseSafe(invoice.JI_B3LineNumber, 0);
							newinvoice.CA_PreviousB3SubHeaderNo = invoice.CA_B3SubHeaderNumber;
						}
						else if (sourceDeclaration.IsIM2 && invoice.B3EntryLine != null)
						{
							var entryLine = invoice.B3EntryLine;
							newinvoice.CA_PreviousB3LineNo = ZInt.ParseSafe(entryLine.CA_B2LineNo, 0);
							newinvoice.CA_PreviousB3SubHeaderNo = entryLine.CA_B2SubHeader;
						}
						newinvoice.CA_PreviousLineNo = invoice.JI_LineNo;
					}
				}
				newCopyToB2Declaration.CargoControlNumbers.RemoveAndDeleteAll();
			}
			finally
			{
				((IBusinessObjectInternals)newCopyToB2Declaration).IsCopying = false;
			}
			return newCopyToB2Declaration;
		}

		#endregion

		#region B2TabDetailsOnStatustab
		public JobDeclarationCollection B2s
		{
			get
			{
				if (b2s == null)
				{
					b2s = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
					var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
					genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
					genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CAAddInfoSchema.Constants.CA_OriginalTransactionNo);
					genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, FormattedTransactionNumber);
					var query = new ZDBOnlyQuery(typeof(JobDeclaration));
					query.AddSubQuery(JobDeclarationSchema.PK, GenAddOnColumnSchema.XA_ParentID, genAddOnQuery, JoinCondition.And);
					b2s.Load(query);
				}
				return b2s;
			}
		}
		JobDeclarationCollection b2s;

		#endregion

		#region ICusEntryNumberErrorReporter
		bool ICusEntryNumberParent.CanBeChangedOrDeleted(CusEntryNumber entryNumber, out string errMsg)
		{
			errMsg = string.Empty;

			var shouldReportError =
				entryNumber.IsInDatabase &&
				!entryNumber.CE_EntryNumInfo.OriginalValue.IsEmpty &&
				entryNumber.CE_EntryTypeInfo.OriginalValue.ToString() == CusEntryNumber.EntryType.CATransactionNumber &&
				DeclarationMessagesHaveBeenSent(true) && !IsExport;

			if (shouldReportError)
			{
				errMsg = Res.GetString("BD19B0B7-576D-4B4C-BCB0-11D96561868B", "This entry number {0} {1} is already lodged at customs. Attempted to change it to {2} {3}.", entryNumber.CE_EntryTypeInfo.OriginalValue, entryNumber.CE_EntryNumInfo.OriginalValue,
				entryNumber.CE_EntryType, entryNumber.CE_EntryNum);
			}

			return string.IsNullOrEmpty(errMsg);
		}

		void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
		{
			entryNumberChangedCallStack = System.Environment.StackTrace;
		}

		string ICusEntryNumberParent.EntryNumberChangedCallStack => entryNumberChangedCallStack;

		string entryNumberChangedCallStack;

		#endregion

		#region IK84ReportAttachee Members
		ZDateTime IK84ReportAttachee.StatementDate
		{
			get { return CA_K84StatementDate; }
			set { CA_K84StatementDate = value; }
		}

		ZDateTime IK84ReportAttachee.AccountingDate
		{
			get { return CA_K84AccountingDate; }
			set { CA_K84AccountingDate = value; }
		}

		ZDateTime IK84ReportAttachee.ConfirmedDate
		{
			get { return CA_ConfirmedDate; }
			set { CA_ConfirmedDate = value; }
		}

		ZDateTime IK84ReportAttachee.B2AcceptedDate
		{
			get { return CA_B2AcceptedDate; }
			set { CA_B2AcceptedDate = value; }
		}

		ZString IK84ReportAttachee.MessageType
		{
			get { return JE_MessageType; }
		}

		#endregion

		#region Bonded Warehouse

		protected override bool SupportsBondedWarehousingCore
		{
			get { return true; }
		}

		public override event EventHandler OnBondedWarehouseRelatedFieldChanged
		{
			add
			{
				base.OnBondedWarehouseRelatedFieldChanged += value;
				JE_MessageSubTypeInfo.ValueChanged -= value;
				JE_MessageSubTypeInfo.ValueChanged += value;
			}
			remove
			{
				base.OnBondedWarehouseRelatedFieldChanged -= value;
				JE_MessageSubTypeInfo.ValueChanged -= value;
			}
		}

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return IsWHSUniversalXMLActive && IsInwardWarehouseEntry; }
		}

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return IsWHSUniversalXMLActive && IsExWarehouseEntry; }
		}

		protected override bool ShouldUpdateOutwardLinesWithInventoryDetailsCore
		{
			get { return IsExWarehouseEntry; }
		}

		protected override bool IsInvoiceQuantityRequiredForBondedWarehouse
		{
			get { return true; }
		}

		protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		public static string InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN
		{
			get { return Res.GetString("55a8c96a-ca12-409c-8599-7f74b537c4d3", "An Invoice Line marked for Bonded Warehousing must have Previous Tran. # and PTLN specified; not all Invoice Lines marked for Bonded Warehousing have Previous Tran. # and PTLN specified."); }
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetWarehouseEntries()
		{
			var entryHeader = B3EntryHeader;
			if (entryHeader != null)
			{
				yield return entryHeader;
			}
		}

		protected override ZString GetWarehouseTransactionStatus()
		{
			return SingleWarehouseEntry?.CH_WarehouseTransactionStatus ?? ZString.Empty;
		}

		protected override bool IsValidWarehouseEntry(Customs.Business.CusEntryHeader entry)
		{
			return base.IsValidWarehouseEntry(entry) && (((CusEntryHeader)entry).IsB3CorCAD);
		}

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_ValueChanged(sender, e);
			if (IsWHSUniversalXMLActive)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region JobRequiredDocumentAddInfo

		public ZString DIFURNs
		{
			get
			{
				var addinfos = JobRequiredDocumentAddInfos;
				var count = addinfos.Take(2).Count();
				if (count > 1)
				{
					return MultipleValues;
				}
				else if (count == 1)
				{
					return addinfos.FirstOrDefault().EX_ReferenceNumber;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString DIFMessageStatus
		{
			get
			{
				var addinfos = CargoWise.Common.IEnumerableExtensions.DistinctBy(JobRequiredDocumentAddInfos, addinfo => addinfo.EX_Status).ToArray();
				var count = addinfos.Length;
				if (count > 1)
				{
					return MultipleValues;
				}
				else if (count == 1)
				{
					return Factory.GetCachedValue<Common.CA.DIF.StatusList>().GetDescriptionFromCode(addinfos[0].EX_Status);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		IEnumerable<JobRequiredDocumentAddInfo> JobRequiredDocumentAddInfos => DocsAndCartage.RequiredDocuments.Cast<JobRequiredDocument>()
					.SelectMany(document => document.AddInfos.Cast<JobRequiredDocumentAddInfo>())
					.Where(addinfo =>
					{
						return addinfo.EX_ApplicationCode == Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF
							&& addinfo.EX_GC_Company == GlbCompany.CurrentCompany.PK;
					});

		static readonly MultilingualString MultipleValues = ResString.GetMultilingualString("D13EC17C-10BB-4E3F-B2D4-1436A17B4871", "Multiple");

		#endregion

		protected override ZString WorkflowImportOrExport
		{
			get
			{
				return JE_MessageType == JobMessageTypeList.Codes.Import ? (ZString)ImportExportCodeList.Codes.ImportOnly : base.WorkflowImportOrExport;
			}
		}

		#region Related OrgHeader

		public OrgHeader GetOrgProxyWithCABusinessNumber()
		{
			var branchOrgProxy = EffectiveBranch.OrgProxy;

			if (branchOrgProxy.GetCABrokerBusinessNumber().IsEmpty)
			{
				return EffectiveBranch.Company?.OrgProxy;
			}

			return branchOrgProxy;
		}

		public OrgHeader GetLPCOHolderParty(ZString partyType)
		{
			switch (partyType)
			{
				case LPCOHolderPartyTypeCodes.Codes.Importer:
					{
						return Importer;
					}

				case LPCOHolderPartyTypeCodes.Codes.Supplier:
					{
						return Supplier;
					}

				case LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord:
					{
						return ImporterOfRecordAddress.HasRealOrganisation
						? ImporterOfRecordAddress.Organisation
						: null;
					}

				default:
					{
						return null;
					}
			}
		}

		#endregion

		#region IDISHost

		IDISHost IDISHostProvider.DISHost => this;

		ZBool IDISHost.ShowDISFeatures => IsImport && CA_ServiceOption == ACROSSServiceOptions.Codes.IID;

		ZGuid IDISHost.BranchPK => Branch?.PK ?? ZGuid.Empty;

		ZGuid IDISHost.CompanyPK => Branch?.GB_GC ?? ZGuid.Empty;

		IEnumerable<string> IDISHost.ApplicationCodes => new[] { Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF };

		IHaveRequiredDocuments IDISHost.RequiredDocumentsProvider => DocsAndCartage;

		IEnumerable<IeDoc> IDISHost.EDocs
		{
			get
			{
				var docManager = Shipment != null ? Shipment.DocManagerInfo : DocManagerInfo;
				return docManager.GetRelatedEDocsView();
			}
		}
		ZString IDISHost.JobNumber => JE_DeclarationReference;

		ZString IDISHost.HumanReadable => "DIF";

		ZBool IDISHost.DISEditable => Environment.Env.Security.CACustomsDIFEdit.IsAllowed;

		IControllerIDProvider IDISHost.ControllerIDProvider => this;

		ZString IDISHost.ImporterName => Importer?.OH_FullName ?? ZString.Empty;

		IEnumerable<ZString> IDISHost.ErrorMessages => Array.Empty<ZString>();

		BusinessObjectFactory IDISHost.Factory => Factory;

		ZGuid IDISHost.PK => PK;

		event EventHandler IDISHost.DISFeatureVisibilityChanged
		{
			add
			{
				JE_MessageTypeInfo.ValueChanged += value;
				CA_ServiceOptionInfo.ValueChanged += value;
			}

			remove
			{
				JE_MessageTypeInfo.ValueChanged -= value;
				CA_ServiceOptionInfo.ValueChanged -= value;
			}
		}

		bool IDISHost.NeedToDoPreFormAction() => !IsMergeDone || MergeManager.RequiresMerge;

		bool IDISHost.DoPreFormAction() => DoMerge();

		Shared.IDISReferenceNumberFountainStrategy IDISHost.DISReferenceNumberFountainStrategy => new DIFReferenceNumberFountainStrategy(Factory);

		#region ICADIFHost

		ICADIFDefaultValues ICADIFHost.ValueProvider => null;

		ZBool ICADIFHost.ShouldSendChangeMessageAsAmendment => IsArrivalDatePast;

		ZString ICADIFHost.ImporterBusinessNumber => EffectiveImporter?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada)?.OK_CustomsRegNo ?? ZString.Empty;

		OrgHeader ICADIFHost.BusinessNumberHolder => EffectiveImporter;

		ZDateTime ICADIFHost.DateOfArrival => JE_DateOfArrival;

		#endregion

		#endregion

		#region ICurrencyConverterDataProvider

		public new const int CurrencyConverterMaximumDaysToFallBack = 365;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => CurrencyConverterMaximumDaysToFallBack;

		#endregion

		#region IUniversalCopySelectivelySupportable Members

		bool IUniversalCopySelectivelySupportable.SupportsUniversalCopy => !IsB2OrIM2OrB3X;

		string IUniversalCopySelectivelySupportable.ReasonForNotSupportingUniversalCopy => Res.GetString("CDC2BF76-E685-4FB1-840E-903990877F9D", "Universal Copy not allowed for B2, IM2 and B3X jobs.");

		#endregion

		#region Override IJobDeclarationAutoSendingMessageSupporter Members

		protected override ZBool SupportEntryDeclarationMessageCore
		{
			get { return IsExport && CACustomsDataRegistry.Instance.ExportDeclarationActive.Value; }
		}

		protected override ZString GetReasonForNotSupportEntryDeclarationMessageCore()
		{
			var result = ZString.Empty;
			if (!IsExport)
			{
				result = SendEntryDeclarationTriggerOnlySupportedForExportJobMessage;
			}
			else if (!CACustomsDataRegistry.Instance.ExportDeclarationActive.Value)
			{
				result = ExportDeclarationIsNotActiveMessage;
			}

			return result;
		}
		internal const string SendEntryDeclarationTriggerOnlySupportedForExportJobMessage = "The Send Entry/Declaration Message trigger is only supported for export declarations.";
		internal const string ExportDeclarationIsNotActiveMessage = "The Send Entry/Declaration Message trigger is not supported when export declaration functionality is not activated.";

		protected override ZBool SupportReleaseMessageCore
		{
			get { return IsImport && !IsLVS; }
		}

		protected override ZString GetReasonForNotSupportReleaseMessageCore()
		{
			var result = ZString.Empty;
			if (!IsImport || IsLVS)
			{
				result = SendReleaseMessageOnlySupportForImportJobMessage;
			}

			return result;
		}
		internal const string SendReleaseMessageOnlySupportForImportJobMessage = "The Send Release Message trigger is only supported for import declarations and shipment type is not 'LVS' and 'LVX'.";

		protected override IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			if (IsExport && CACustomsDataRegistry.Instance.ExportDeclarationActive.Value)
			{
				return new AutoSendG7ExportMessageProcessor(this);
			}

			return null;
		}

		protected override IProcessor GetReleaseMessageProcessorCore()
		{
			if (IsImport && !IsLVS)
			{
				if (IsIID)
				{
					return new AutoSendIIDMessageProcessor(this);
				}
				else
				{
					return new AutoSendACROSSMessageProcessor(this);
				}
			}

			return null;
		}

		#endregion

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => CADeclaration;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobCADeclarationSchema.CAD_JE;

		#endregion

		#region IBondDetailsDefault Members

		BondDetailsDefaulter BondDetailsDefaulter
		{
			get { return fBondDetailsDefaulter ?? (fBondDetailsDefaulter = new BondDetailsDefaulter()); }
		}
		BondDetailsDefaulter fBondDetailsDefaulter;

		public override ZString CA_BondType
		{
			get { return base.CA_BondType; }
			set
			{
				var hasChanges = base.CA_BondType != value;
				base.CA_BondType = value;

				if (hasChanges && !IsCopying)
				{
					if (!CA_BondType.IsEmpty && IsImport)
					{
						BondDetailsDefaulter.DefaultWhenBondTypeChanges(this, CA_BondType);

						RefreshBondDetailFields();
					}
					else
					{
						CA_BondNo = ZString.Empty;
						CA_SuretyCode = ZString.Empty;
					}
				}
			}
		}

		bool IsContinuousBond
		{
			get { return CA_BondType == BondTypeList.Codes.ContinuousBond; }
		}

		void RefreshBondDetailFields()
		{
			CA_BondNoInfo.RefreshBinding();
		}

		bool CA_BondNo_ReadOnly
		{
			get { return IsContinuousBond; }
		}

		bool CA_SuretyCode_ReadOnly
		{
			get { return IsContinuousBond; }
		}

		[ReadOnlyMember(nameof(CA_BondNo_ReadOnly))]
		public override ZString CA_BondNo { get => base.CA_BondNo; set => base.CA_BondNo = value; }

		[ReadOnlyMember(nameof(CA_SuretyCode_ReadOnly))]
		public override ZString CA_SuretyCode { get => base.CA_SuretyCode; set => base.CA_SuretyCode = value; }

		OrgImpAddInfo IBondDetailsDefault.ImporterOfRecord
		{
			get { return ImporterOfRecordAddInfo; }
		}

		OrgImpAddInfo IBondDetailsDefault.ImportOrg
		{
			get { return ImporterAddInfo; }
		}

		ZDateTime IBondDetailsDefault.EffectiveDate
		{
			get { return EffectiveDutyDate; }
		}

		ZString IBondDetailsDefault.CA_BondType
		{
			get { return CA_BondType; }
			set { CA_BondType = value; }
		}

		ZString IBondDetailsDefault.CA_BondNo
		{
			set { CA_BondNo = value; }
		}

		ZString IBondDetailsDefault.CA_SuretyCode
		{
			set { CA_SuretyCode = value; }
		}

		#endregion

		ZBool ICADeclarationProvider.IsValidationEnabled => true;

		#region ICreditControlledNotificationTextProvider Members

		MultilingualString ICreditControlledNotificationTextProvider.NotificationHeaderText
		{
			get
			{
				if (IsLVX)
				{
					return ResString.GetMultilingualString("3E1A5B92-126E-4383-AA2F-72E42C97569C", "CLVS job cannot be added to F-Type job because:");
				}
				return null;
			}
		}

		MultilingualString ICreditControlledNotificationTextProvider.NotificationConfirmationText
		{
			get
			{
				if (IsLVX)
				{
					return ResString.GetMultilingualString("25794291-BC2E-4657-9B73-DDD590F39E6C", "Do you wish to override it and proceed?");
				}
				return null;
			}
		}

		#endregion

		#region Credit Restriction Message Caption

		public override ZString CreditRestrictionMessageCaption
		{
			get
			{
				return IsLVX ? (ZString)Res.GetString("372FD09F-C0D0-4B9D-8ABE-E67D98AA2C13", "Add CLVS to F-Type with Credit Restriction") : base.CreditRestrictionMessageCaption;
			}
		}

		#endregion

		#region SentCADMessageCount

		public ZInt GetSentCADMessageCountInThisVersion(ZString versionID)
		{
			var cadMessagesCount = 0;
			if (IsCADEnabled && B3EntryHeader is CusEntryHeader entryHeader)
			{
				cadMessagesCount = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.CommercialAccountingDeclaration && x.BatchNumber == versionID);
			}
			return cadMessagesCount;
		}

		public ZInt SentCADMessageCount
		{
			get
			{
				var cadMessagesCount = 0;
				if (IsCADEnabled && B3EntryHeader is CusEntryHeader entryHeader)
				{
					cadMessagesCount = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.CommercialAccountingDeclaration);
				}
				return cadMessagesCount;
			}
		}

		#endregion

		#region Customs Rule

		public override ZDecimal DisbursementAmount
		{
			get
			{
				var result = 0m;
				foreach (var line in FilteredInvoiceLines)
				{
					result += line.DutiesAndTaxes.Sum(x => x.C1_Amount);
				}
				return result;
			}
		}

		public override ZDecimal TotalDutyAmount => new ZDecimal(Invoices.Sum(x => x.JobComInvoiceLines.OfType<JobComInvoiceLine>().Sum(y => y.JI_Calc_DutyAmount)));

		#endregion

		#region Builtin or Interface

		protected override void DefaultJE_ApplicationCode()
		{
			if (JE_MessageType == JobMessageTypeList.Codes.Import || JE_MessageType == JobMessageTypeList.Codes.Export)
			{
				base.DefaultJE_ApplicationCode();
			}
			else
			{
				JE_ApplicationCode = ZString.Empty;
			}
		}

		protected override bool IsDeclarationIntegratedCore() => IsInterface;

		protected override bool ShowSubmitMenuItemCore() => IsInterface;

		#endregion

		public bool UseBaseMergeStrategyForTesting { get; set; }

		internal void ResetCachedValuesForTesting()
		{
			releaseStatusWrapper = null;
			releaseStatuses = null;
			releaseStatusesToPrint = null;
		}
	}
}
