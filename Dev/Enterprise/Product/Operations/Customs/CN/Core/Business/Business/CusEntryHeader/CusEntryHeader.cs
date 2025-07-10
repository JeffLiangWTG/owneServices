using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryHeader : Customs.Business.CusEntryHeader
	{
		public new partial class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string PreEntryNumber = "PreEntryNumber";
			public const string DeclarationUnifiedNumber = "DeclarationUnifiedNumber";
			public const string CustomsMessagesRemarks = "CustomsMessagesRemarks";
			public const string CIQNumber = "CIQNumber";
			public const string CIQStatus = "CIQStatus";
			public const string CIQStatusDescription = "CIQStatusDescription";
			public const string CEI_PackageUQ = "CEI_PackageUQ";
			public const string XC_PackageUQDescription = "XC_PackageUQDescription";
			public const string CountOfLines = "CountOfLines";
			public const string InvoiceNumbers = "InvoiceNumbers";
			public const string NumberOfContainers = "NumberOfContainers";
			public const string ArchiveDate = "ArchiveDate";
			public const string ArchiveUser = "ArchiveUser";
			public const string ACDANumber = "ACDANumber";
		}

		#region Override Properties

		public override ZInt PackagesCount => this.CEI_Packages;

		protected override ZString HumanReadableNameCore => this.CH_BGMReference.IsEmpty ?
			new ZString(string.Format(CultureInfo.CurrentCulture, Res.GetString("A1424FDD-BFAE-4B48-BBB1-6073F511ADBF", "Entry {0}"), base.HumanReadableNameCore)) :
			new ZString(string.Format(CultureInfo.CurrentCulture, Res.GetString("1A0D27E5-BDB6-46B6-B8D2-212CA8D097E2", "Entry {0}"), CH_BGMReference));

		public override bool ShouldLogEntryStatus => true;

		public override ZString ClearanceEventReference => CH_EntryStatus;

		public override bool HasBeenLodgedAtCustoms => !DeclarationUnifiedNumber.IsEmpty || !EntryNumber.IsEmpty;

		public override bool IsWaitingForResponse => JobMessageStatusList.IsAwaiting(CH_Status);

		public override bool IsFormalEntry => CH_MessageType == EntryTypeList.Codes.CustomsEntry || CH_MessageType == EntryTypeList.Codes.RecordListing;

		public override bool HasBeenWithdrawn => false;

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => !HasBeenLodgedAtCustoms;

		#endregion

		#region Entry Numbers

		#region Pre Entry Number

		CusEntryNumber CusPreEntryNumber => GetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|PreEntryNumber", Caption = "Pre Entry Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString PreEntryNumber
		{
			get => CusPreEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				SetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, value, CusPreEntryNumber?.CE_IssueDate ?? ZDateTime.Empty);
				PreEntryNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PreEntryNumberInfo => GetZPropertyInfo(Schema.PreEntryNumber);

		public ZDateTime PreEntryNumberIssueDate => CusPreEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

		public void SetPreEntryNumber(ZString number, ZDateTime issueDate)
		{
			SetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, number, issueDate);
		}

		#endregion

		#region Declaration Unified Number

		public CusEntryNumber CusDeclarationUnifiedNumber => GetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|CusDeclarationUnifiedNumber", Caption = "Declaration Unified Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString DeclarationUnifiedNumber
		{
			get => CusDeclarationUnifiedNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				SetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, value, CusDeclarationUnifiedNumber?.CE_IssueDate ?? ZDateTime.Empty);
				DeclarationUnifiedNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeclarationUnifiedNumberInfo => GetZPropertyInfo(Schema.DeclarationUnifiedNumber);

		#endregion

		#region CIQNumber

		public void SetCIQNumber(ZString ciqNumber, ZDateTime issueDate)
		{
			SetEntryNumber(CusEntryNumberTypes.China.CIQNumber, ciqNumber, issueDate);
		}

		public void SetCIQStatus(ZString status)
		{
			var cusCIQNumber = CusCIQNumber;
			if (cusCIQNumber != null)
			{
				cusCIQNumber.CE_EntryStatus = status;
			}
		}

		CusEntryNumber CusCIQNumber => GetEntryNumber(CusEntryNumberTypes.China.CIQNumber);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|CusCIQNumber", Caption = "CIQ Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString CIQNumber
		{
			get => CusCIQNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				SetEntryNumber(CusEntryNumberTypes.China.CIQNumber, value, CusCIQNumber?.CE_IssueDate ?? ZDateTime.Empty);
				CIQNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CIQNumberInfo => GetZPropertyInfo(Schema.CIQNumber);

		public ZString CIQStatus => CusCIQNumber?.CE_EntryStatus ?? ZString.Empty;

		public ZString CIQStatusDescription => Lookups.CH_EntryStatusList.GetDescriptionFromCode(CIQStatus);

		public ZDateTime CIQIssueDate => CusCIQNumber?.CE_IssueDate ?? ZDateTime.Empty;

		#endregion

		CusEntryNumber GetEntryNumber(string entryNumberType) => CusEntryNumber.Load(this, entryNumberType, CountryCode);

		public bool IsEntryNumberSystemGernerated(string numberType) => GetEntryNumber(numberType)?.CE_EntryIsSystemGenerated ?? false;

		public void ManuallySetEntryNumber(ZString entryType, ZString entryNumber, ZDateTime? issueDate = null)
		{
			var cusEntryNumber = CusEntryNumber.Load(this, entryType, CountryCode);
			if (!issueDate.HasValue)
			{
				if (cusEntryNumber != null && cusEntryNumber.CE_EntryNum == entryNumber)
				{
					issueDate = cusEntryNumber.CE_IssueDate;
				}
				else
				{
					issueDate = ZDateTime.Empty;
				}
			}
			if (!cusEntryNumber?.CE_EntryIsSystemGenerated ?? true)
			{
				cusEntryNumber = CusEntryNumber.LoadOrCreate(this, entryType, CountryCode);
				cusEntryNumber.CE_EntryIsSystemGenerated = false;
				cusEntryNumber.CE_EntryNum = entryNumber;
				cusEntryNumber.CE_IssueDate = issueDate.Value;
			}

			switch (entryType)
			{
				case CusEntryNumberTypes.Standard.MovementReferenceNumber:
					OnMovementReferenceNumberChanged(entryNumber, issueDate.Value);
					break;
				case CusEntryNumberTypes.China.PreEntryNumber:
					PreEntryNumberInfo.RefreshBinding();
					break;
				case CusEntryNumberTypes.China.DeclarationUnifiedNumber:
					DeclarationUnifiedNumberInfo.RefreshBinding();
					break;
				case CusEntryNumberTypes.China.CIQNumber:
					CIQNumberInfo.RefreshBinding();
					break;
			}
		}

		public void SetMovementReferenceNumber(ZString movementReferenceNumber, ZDateTime movementReferenceNumberIssueDate)
		{
			MovementReferenceNumberSetter(movementReferenceNumber, movementReferenceNumberIssueDate);
			OnMovementReferenceNumberChanged(movementReferenceNumber, movementReferenceNumberIssueDate);
		}

		void OnMovementReferenceNumberChanged(ZString movementReferenceNumber, ZDateTime movementReferenceNumberIssueDate)
		{
			MovementReferenceNumberInfo.RefreshBinding();

			if (EntryInstruction != null)
			{
				EntryInstruction.CEI_DateForDuty = movementReferenceNumberIssueDate;

				if (!IsChildEntry && EntryInstruction.ChildInstruction != null)
				{
					EntryInstruction.ChildInstruction.CEI_RelatedMRN = movementReferenceNumber;
				}
			}
		}

		#endregion

		#region Override Methods

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PopulateCNCustomsLocalReferenceNumberIfRequired();
			AddEntryNumberChangedLogIfNeeded(CusPreEntryNumber);
			AddEntryNumberChangedLogIfNeeded(CusDeclarationUnifiedNumber);
			AddEntryNumberChangedLogIfNeeded(MovementReferenceCusEntryNumber);
			AddEntryNumberChangedLogIfNeeded(CusCIQNumber);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && localReferenceNumberAssignedFromNumberFountain)
			{
				CH_BGMReference = ZString.Empty;
			}
			localReferenceNumberAssignedFromNumberFountain = false;
		}

		protected override bool ShouldLogCustomsClearedEvent()
		{
			return IsCustomsClearedEventSupported && IsEntryStatusChangedToClearSinceLoading;
		}

		#endregion

		#region RecordArchivedLog

		public ZString ArchiveUser => RecordArchivedLog?.User?.FullName ?? ZString.Empty;

		public ZDateTime ArchiveDate => RecordArchivedLog?.SL_EventTime ?? ZDateTime.Empty;

		StmALog RecordArchivedLog => Logs?.Find(x => x.SL_SE_NKEvent == Events.RecordArchivedCode && !x.SL_IsCancelled).FirstOrDefault();

		public void AddRecordArchivedLog()
		{
			if (RecordArchivedLog != null)
			{
				if (RecordArchivedLog.IsInDatabase)
				{
					RecordArchivedLog.Cancel();
				}
				else
				{
					RecordArchivedLog.Delete();
				}
			}

			Logs.AddNew(Events.RecordArchived, ZString.Empty);
		}

		#endregion

		#region EntryNumberChangedLog

		void AddEntryNumberChangedLogIfNeeded(CusEntryNumber number)
		{
			if (number != null && !number.IsDeleted && (number.CE_EntryNumInfo.HasChanges || !number.IsInDatabase) && !(number.CE_EntryNum.IsEmpty && (number.IsInDatabase ? number.CE_EntryNumInfo.OriginalValue : ZString.Empty).IsEmpty))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(
					Events.EditedARecord,
					ZString.Format("{0}: From <{1}> to <{2}>", number.CE_EntryType, number.IsInDatabase ? number.CE_EntryNumInfo.OriginalValue : ZString.Empty, number.CE_EntryNum)
				);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		ZBool localReferenceNumberAssignedFromNumberFountain;

		void PopulateCNCustomsLocalReferenceNumberIfRequired()
		{
			if (CH_BGMReference.IsEmpty)
			{
				localReferenceNumberAssignedFromNumberFountain = true;
				CH_BGMReference = GetNewDeclarationReference();
			}
		}

		ZString GetNewDeclarationReference()
		{
			ZString result = ZString.Empty;
			if (Declaration != null)
			{
				ZString fountain = Env.NumberFountains.GetCNCustomsLocalReferenceNumber(CH_MessageType).GetNextFormatted(Factory);
				result = ZString.Format(
					"{0}{1}{2}",
					CH_MessageType,
					Declaration.DateOfValuation.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					fountain.PadLeft(7, '0').Right(7)
				);
			}
			return result;
		}

		#endregion

		#region New Properties

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|XC_Packages", Caption = "No. Packs")]
		public ZInt CEI_Packages => EntryInstruction?.CEI_Packages ?? ZInt.Zero;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|XC_PackageUQ", Caption = "Pack Type")]
		public ZString CEI_PackageUQ => EntryInstruction?.CEI_PackageUQ ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|XC_PackageUQDescription", Caption = "Package Type Description")]
		public ZString XC_PackageUQDescription => EntryInstruction?.XC_PackageUQDescription ?? ZString.Empty;

		public int MaximumEntryLinesAllowed => 50;

		public ZBool IsCustomsEntry => CH_MessageType == EntryTypeList.Codes.CustomsEntry;

		public ZBool IsRecordListing => CH_MessageType == EntryTypeList.Codes.RecordListing;

		public ZBool IsEntering => !Declaration.WillGenerateBothEntries && Declaration.IsImport
														|| Declaration.WillGenerateBothEntries && IsCustomsEntry && Declaration.IsImport
														|| Declaration.WillGenerateBothEntries && IsRecordListing && Declaration.IsExport;

		public ZBool IsExiting => !Declaration.WillGenerateBothEntries && Declaration.IsExport
														|| Declaration.WillGenerateBothEntries && IsCustomsEntry && Declaration.IsExport
														|| Declaration.WillGenerateBothEntries && IsRecordListing && Declaration.IsImport;

		public ZBool IsChildEntry => EntryInstruction?.IsChild ?? false;

		public ZInt CountOfLines => MergedLines.Count;

		public ZPropertyInfo CountOfLinesInfo => GetZPropertyInfo(Schema.CountOfLines);

		public ZString InvoiceNumbers => ZString.Join(",", InvoiceHeaders.Select(invoice => invoice.JZ_InvoiceNumber).ToArray());

		public ZPropertyInfo InvoiceNumbersInfo => GetZPropertyInfo(Schema.InvoiceNumbers);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|TotalExciseAmount", Caption = "Total Excise")]
		public ZDecimal TotalExciseAmount => MergedLines.Cast<CusEntryLine>().Sum(line => line.ExciseAmount);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|TotalGSTVATAmount", Caption = "Total VAT")]
		public ZDecimal TotalGSTVATAmount => MergedLines.Cast<CusEntryLine>().Sum(line => line.GSTVATAmount);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|TotalAntiDumpingAmount", Caption = "Total Anti-Dumping", ShortCaption = "Total ADD")]
		public ZDecimal TotalAntiDumpingAmount => MergedLines.Cast<CusEntryLine>().Sum(line => line.AntiDumpingAmount);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|TotalCountervailingAmount", Caption = "Total Countervailing", ShortCaption = "Total CVD")]
		public ZDecimal TotalCountervailingAmount => MergedLines.Cast<CusEntryLine>().Sum(line => line.CountervailingAmount);

		public ZDecimal TotalWeight => InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => line.JI_Weight);

		public ZInt DaysOfDelayedDeclaration
		{
			get
			{
				var result = 0;

				var declarantDate = DeclarantDate;
				var deadline = Declaration.DeclarationDeadline;
				if (declarantDate.IsValid && deadline.IsValid)
				{
					var days = (declarantDate.Date - deadline.Date).Days;
					if (days > 0)
					{
						result = days;
					}
				}

				return result;
			}
		}

		public ZDecimal FeeForDelayedDeclaration
		{
			get
			{
				var result = 0m;

				var declarantDate = DeclarantDate.Date;
				if (DaysOfDelayedDeclaration > 0 && declarantDate <= Declaration.JE_DateOfArrival.AddMonths(3).Date)
				{
					result = CalculateDelayedFee(declarantDate);
				}

				return result;
			}
		}

		public decimal CalculateDelayedFee(ZDate declarantDate)
		{
			var result = 0m;

			var days = (declarantDate - WorkingDayHelper.GetNearestWorkingdayAfter(Declaration.DeclarationDeadline).Date).Days + 1;
			if (days > 0)
			{
				var delayDeclarationTaxOrFee = DelayDeclarationTaxOrFee;
				if (delayDeclarationTaxOrFee != null)
				{
					result = Math.Floor(MergedLines.Cast<CusEntryLine>().Sum(x => x.CL_CustomsValue) * delayDeclarationTaxOrFee.ZZF_Value * days);
					if (result < delayDeclarationTaxOrFee.ZZF_Minimum)
					{
						result = 0;
					}
				}
			}

			return result;
		}

		public RefCusTaxOrFee DelayDeclarationTaxOrFee => new RefCusTaxOrFee.Loader(Declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.China, Constants.UniversalReferenceConstants.RefCusTaxOrFee.DDF, ZDate.Today);

		public ZInt RemainingDaysForDeclaration
		{
			get
			{
				var result = 0;
				if (DeclarantDate.IsEmpty && !Declaration.DeclarationDeadline.IsEmpty)
				{
					result = Declaration.DeclarationDeadline >= ZDateTime.Today ? (Declaration.DeclarationDeadline - ZDateTime.Today).Days + 1
																				: (Declaration.DeclarationDeadline - ZDateTime.Today).Days;
				}
				return result;
			}
		}

		#region LastAudited

		StmALog LastAuditedLog => Logs.MostRecentLogByEventTime(Events.RecordAudited);

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|LastAuditedDate", Caption = "Last Audited Date")]
		public ZDateTime LastAuditedDate => LastAuditedLog?.SL_EventTime ?? ZDateTime.Empty;

		public ZPropertyInfo LastAuditedDateInfo => GetZPropertyInfo(nameof(LastAuditedDate));

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryHeader|LastAuditedUser", Caption = "Last Audited User")]
		public ZString LastAuditedUser => LastAuditedLog?.User.GS_FullName ?? ZString.Empty;

		public ZPropertyInfo LastAuditedUserInfo => GetZPropertyInfo(nameof(LastAuditedUser));

		public ZString ACDANumber => EntryInstruction?.Attachments.Cast<EntryInstructionAttachment>().Where(x => x.AttachmentType == CSDDocTypeList.Codes._10000001).Select(x => x.AttachmentNumber)?.FirstOrDefault(x => !x.IsEmpty) ?? ZString.Empty;

		#endregion

		[DocumentFieldExcludeFromMap]
		public ZString HtmlFormatEntryData
		{
			get
			{
				if (fHtmlFormatEntryData.IsEmpty)
				{
					fHtmlFormatEntryData = MessageTextInterpretator.GetInterpretionForCustomsEntry(this);
				}
				return fHtmlFormatEntryData;
			}
		}
		ZString fHtmlFormatEntryData;

		internal void MarkAsNeedingReloadHtmlFormatEntryData() => fHtmlFormatEntryData = ZString.Empty;

		public ZBool ReadyForCompleteDeclaration => (Declaration.IsTwoStepDeclaration && CH_Status == JobMessageStatusList.Codes.ClearedPreliminaryDeclaration);

		#endregion

		#region CustomsMessagingRemarks

		public ZString CustomsMessagesRemarks => EntryInstruction?.CustomsMessageRemarks ?? ZString.Empty;

		public ZPropertyInfo CustomsMessagesRemarksInfo => GetZPropertyInfo(Schema.CustomsMessagesRemarks);

		#endregion

		#region IDocumentSupport Members

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		#endregion

		#region EntryHeaderContainers

		public EntryHeaderContainerCollection EntryHeaderContainers
		{
			get
			{
				if (fEntryHeaderContainers == null)
				{
					fEntryHeaderContainers = new EntryHeaderContainerCollection(this);
					fEntryHeaderContainers.Load();
				}
				return fEntryHeaderContainers;
			}
		}

		EntryHeaderContainerCollection fEntryHeaderContainers;

		public ZInt NumberOfContainers => Containers?.Length ?? ZInt.Zero;

		public ZPropertyInfo NumberOfContainersInfo => GetZPropertyInfo(Schema.NumberOfContainers);

		#endregion

		#region Supporting Documents

		public IEnumerable<CusSupportingDocument> CusSupportingDocuments
		{
			get
			{
				if (cachedCusSupportingDocuments == null)
				{
					cachedCusSupportingDocuments = new CachedProperty<IEnumerable<CusSupportingDocument>>(Factory, () =>
					{
						var isEntering = IsEntering;
						var isExiting = IsExiting;

						return InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusSupportingDocuments.Cast<CusSupportingDocument>()).Where(x => (isEntering && x.IsImport) || (isExiting && x.IsExport));
					});
				}
				return cachedCusSupportingDocuments.Value;
			}
		}
		CachedProperty<IEnumerable<CusSupportingDocument>> cachedCusSupportingDocuments;

		public EntryHeaderSupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					fSupportingDocuments = new EntryHeaderSupportingDocumentCollection(this);
					fSupportingDocuments.Load();
				}
				return fSupportingDocuments;
			}
		}
		EntryHeaderSupportingDocumentCollection fSupportingDocuments;

		#endregion

		#region EntryHeaderRequiredDocuments

		public EntryHeaderRequiredDocumentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new EntryHeaderRequiredDocumentCollection(this);
					fRequiredDocuments.Load();
				}
				return fRequiredDocuments;
			}
		}
		EntryHeaderRequiredDocumentCollection fRequiredDocuments;

		internal void ClearCachedMergedData()
		{
			fSupportingDocuments = null;
			fEntryHeaderContainers = null;
			fRequiredDocuments = null;

			MergedLines.Cast<CusEntryLine>().ForEach(l => l.ClearCachedMergedData());
		}

		#endregion

		public ZString GetDeclarationType()
		{
			ZString result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				var clearanceMode = declaration.JE_ClearanceMode;
				if (!CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.Value || clearanceMode.IsEmpty || clearanceMode == ClearanceModeList.Codes.Integrated)
				{
					result = DeclarationTypeList.Codes.IntegratedDeclaration;
				}
				else if (clearanceMode == ClearanceModeList.Codes.TwoStep)
				{
					if (JobMessageStatusList.AvailableForCompleteDeclaration(CH_Status))
					{
						result = DeclarationTypeList.Codes.CompleteDeclaration;
					}
					else
					{
						result = DeclarationTypeList.Codes.PreliminaryDeclaration;
					}
				}
				else if (clearanceMode == ClearanceModeList.Codes.TwoStepManual)
				{
					result = DeclarationTypeList.Codes.ManualDeclaration;
				}
				else if (clearanceMode == ClearanceModeList.Codes.TwoStepAuto)
				{
					result = DeclarationTypeList.Codes.AutoDeclaration;
				}
			}
			return result;
		}
	}
}
