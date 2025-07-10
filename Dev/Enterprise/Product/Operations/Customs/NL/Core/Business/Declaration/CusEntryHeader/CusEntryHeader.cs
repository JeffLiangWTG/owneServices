using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration;

[SystemDefinedValues]
[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.CustomsEntryHeaders))]
public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader, Integration.Customs.NL.ICusEntryHeader, ICusEntryNumberValidationDeciderOfType, ILRNGenerator
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
	{
		public const string CH_ExitDate = nameof(CusEntryHeader.CH_ExitDate);
		public const string CusEntryNumberIssueDate = nameof(CusEntryHeader.CusEntryNumberIssueDate);
		public const string CH_PhaseStatusDescription = nameof(CusEntryHeader.CH_PhaseStatusDescription);
		public const string DMSFallbackIsActive = nameof(CusEntryHeader.DMSFallbackIsActive);
		public const string FallbackEntryNumberIssueDate = nameof(FallbackEntryNumberIssueDate);
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	[ChildEditable(true)]
	public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

	protected override Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this);

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType() => typeof(CusEntryNumberValidation);

	public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	[ReadOnly(true)]
	public override ZString CH_BGMReference
	{
		get => base.CH_BGMReference;
		set => base.CH_BGMReference = value;
	}

	protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

	public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
	{
		return new CusEntryLineCollection<CusEntryLine>(this);
	}

	protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		GenerateAndSetLocalReferenceNumberIfNeeded();
	}

	public ZString DeclarantEoriOfMainOfficeRegistrationNumber => RepresentativeOrDeclarantEoriOfMainOffice.SubstringSafe(2);

	public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

	public bool EntryLinesHasCosts => AllEntryLines.Cast<CusEntryLine>().Any(entryLine => entryLine.Fees.Count > 0);

	bool IsCurrentLocalReferenceNumberGeneratedForCurrentDeclarantEORI => Regex.IsMatch(CH_BGMReference, string.Format("^{0}{1}{2}$", "\\d{2}", DeclarantEoriOfMainOfficeRegistrationNumber, "\\d{" + (20 - DeclarantEoriOfMainOfficeRegistrationNumber.Length).ToString() + "}"));

	void GenerateAndSetLocalReferenceNumberIfNeeded()
	{
		if (Declaration != null && !HasBeenLodgedAtCustoms && !IsWaitingForResponse && !DeclarantEoriOfMainOfficeRegistrationNumber.IsEmpty && (CH_BGMReference.IsEmpty || !IsCurrentLocalReferenceNumberGeneratedForCurrentDeclarantEORI))
		{
			CH_BGMReference = LRNGeneratorHelper.GenerateLocalReferenceNumber(this);
		}
	}

	[ResourceStringData("AA30FE42-4E40-4F34-90FC-2D1732137948", Caption = "Exit Date")]
	[ReadOnly(true)]
	public ZDateTime CH_ExitDate
	{
		get => AddInfo.ZG_ExitDate;
		set => AddInfo.ZG_ExitDate = value;
	}

	public ZPropertyInfo CH_ExitDateInfo => GetWrappedZPropertyInfo(Schema.CH_ExitDate, x => AddInfo.ZG_ExitDateInfo);

	[ReadOnlyMember(nameof(EntryStatusReadonly))]
	public override ZString CH_EntryStatus => base.CH_EntryStatus;

	bool EntryStatusReadonly => !(DMSFallbackIsActive && ValidStatusCombinationsAfterFallbackIsSent.Any(c => c.entryHeaderStatus == CH_Status && c.entryStatus == CH_EntryStatus));

	List<(ZString entryHeaderStatus, ZString entryStatus)> ValidStatusCombinationsAfterFallbackIsSent =>
	[
		(StatusNew.Accepted, ZString.Empty),
		(StatusNew.Accepted, EntryStatusNew.NotReceiveResponseBeforeEndOfFallback),
		(StatusNew.Error, ZString.Empty),
		(StatusNew.SentToCustoms, ZString.Empty),
		(StatusNew.Accepted, EntryStatusNew.PhysicalInspection),
	];

	[ResourceStringData("E9649F8E-26FA-41B6-9373-5159DE8F42C6", Caption = "Phase Status", ShortCaption = "Ph. Status")]
	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.EntryPhaseStatusList))]
	[ReadOnly(true)]
	public override ZString CH_PhaseStatus => base.CH_PhaseStatus;

	[ResourceStringData("ABA07B16-EA3C-4BDB-A57F-261CE6E690F8", Caption = "Phase Status Description", ShortCaption = "Ph. Status Desc.")]
	public ZString CH_PhaseStatusDescription => Lookups.EntryPhaseStatusList.GetDescriptionFromCode(CH_PhaseStatus);

	public ZPropertyInfo CH_PhaseStatusDescriptionInfo => GetZPropertyInfo(Schema.CH_PhaseStatusDescription);

	[ResourceStringData("0C0990DE-6F63-4D12-B503-269519D7BDE2", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Status")]
	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.MessageStatusList))]
	[ReadOnlyMember(nameof(CH_StatusReadonly))]
	public override ZString CH_Status { get => base.CH_Status; set => base.CH_Status = value; }

	bool CH_StatusReadonly => !(DMSFallbackIsActive && IsManualFallback && ValidMessageStatusCombinationsAfterFallbackIsSent.Any(c => c.entryHeaderStatus == CH_Status && c.entryStatus == CH_EntryStatus));
	List<(ZString entryHeaderStatus, ZString entryStatus)> ValidMessageStatusCombinationsAfterFallbackIsSent =>
	[
		(StatusNew.Error, ZString.Empty),
		(StatusNew.SentToCustoms, ZString.Empty),
	];

	bool IsManualFallback => NLCustomsRegistry.Instance.FallbackEmail.Value == EmailFallbackList.Codes.MNL;

	[ReadOnly(true)]
	public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

	[ResourceStringData("71B3F2B6-15A0-4BE9-AC66-8DF4E8FBE2F6", Caption = "Acceptance Date", MediumCaption = "Accept. Date", ShortCaption = "Acc. Date")]
	public ZDateTime CusEntryNumberIssueDate => CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

	public ZPropertyInfo CusEntryNumberIssueDateInfo => GetZPropertyInfo(Schema.CusEntryNumberIssueDate);

	[ReadOnly(true)]
	[ResourceStringData("A0F2B1D6-3C8B-4E5A-9F7B-1D3E4F5A6B7C", Caption = "MRN")]
	public override ZString EntryNumber => base.EntryNumber;

	public override bool ShouldLogEntryStatus => true;

	protected override bool ShouldLogStatus => true;

	protected override bool ShouldLogPhaseStatus => true;

	public override ZString DefaultStatusDescription => ZString.Empty;

	protected override bool IsMrnEntryNumberTheOneWeWantToShow => true;

	#region Fallback

	[ReadOnlyMember(nameof(DMSFallbackIsActiveReadonly))]
	[ResourceStringData("CFE7EF0E-96D3-4784-AF01-81EF85B5757D", Caption = "Fallback?")]
	public ZBool DMSFallbackIsActive
	{
		get => CH_PhaseStatus.EqualsIgnoringCase(CustomsEntryPhaseStatusList.Codes.FBK);
		set
		{
			CH_PhaseStatus = value ? CustomsEntryPhaseStatusList.Codes.FBK : CustomsEntryPhaseStatusList.Codes._515;
			AddOrRemoveAdditionalInfosIfNeeded(value);
			ChangeFBKMessageAfterFallbackIfNeeded(value);
			DMSFallbackIsActiveInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo DMSFallbackIsActiveInfo => GetZPropertyInfo(nameof(DMSFallbackIsActive));

	bool DMSFallbackIsActiveReadonly => !DMSFallbackIsActive && !(IsExport && DuringDMSExportFallback);

	public ZDateTime FallbackEntryNumberIssueDate
	{
		get { return FallbackEntryNumber?.CE_IssueDate ?? ZDateTime.Empty; }
		set
		{
			if (FallbackEntryNumberIssueDate != value)
			{
				if (FallbackEntryNumber == null)
				{
					CreateFallbackEntryNumber();
				}
				FallbackEntryNumber.CE_IssueDate = value;
				FallbackEntryNumberIssueDateInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo FallbackEntryNumberIssueDateInfo => GetZPropertyInfo(Schema.FallbackEntryNumberIssueDate);

	CusEntryNumber FallbackEntryNumber => Factory.GetValue(ref fallbackEntryNumberCache, () => CusEntryNumber.Load(this, CusEntryNumberTypes.EU.Fallback, CountryCode));
	CachedProperty<CusEntryNumber> fallbackEntryNumberCache;

	CusEntryNumber CreateFallbackEntryNumber()
	{
		var fallbackEntryNumber = CusEntryNumber.New(this, CusEntryNumberTypes.EU.Fallback, CountryCode);
		fallbackEntryNumber.CE_EntryIsSystemGenerated = true;
		fallbackEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		return fallbackEntryNumber;
	}

	public ZBool MessageHasBeenSentBeforeFallback => Factory.GetValue(ref isEntrySentBeforeFallback, () => Messages.Cast<NLEDIMessage>()
						.Any(x => x.EM_ReceiveTransmit == Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit
						&& x.EM_SystemCreateTimeUtc < DMSExportFallbackConfiguration.Start.ToUniversalBranchTime()));
	CachedProperty<ZBool> isEntrySentBeforeFallback;

	void ChangeFBKMessageAfterFallbackIfNeeded(ZBool fallbackIsActive)
	{
		if (!fallbackIsActive && !DuringDMSExportFallback)
		{
			var fallbackMessage = Messages.Cast<NLEDIMessage>().Where(x => x.EM_MessageSubType == ExportSendMessageTypes.Codes.FBK).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
			if (fallbackMessage != null)
			{
				fallbackMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				fallbackMessage.EM_MessageSubType = ExportSendMessageTypes.Codes.DEC;
			}
		}
	}

	void AddOrRemoveAdditionalInfosIfNeeded(ZBool fallbackIsActive)
	{
		if (EntryInstruction is CusEntryInstruction entryInstruction)
		{
			var target = entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalInformation && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.Codes.NP500);
			if (fallbackIsActive)
			{
				if (target == null && !MessageHasBeenSentBeforeFallback)
				{
					var additionalDocument = entryInstruction.AdditionalInfos.AddNew();
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.NP500;
				}
			}
			else
			{
				if (target != null)
				{
					entryInstruction.AdditionalInfos.RemoveAndDelete(target);
				}
			}
		}
	}

	ZBool DuringDMSExportFallback
	{
		get
		{
			var result = DMSExportFallbackConfiguration.Start.IsInThePast();
			if (result)
			{
				var end = DMSExportFallbackConfiguration.End;
				result = end.IsEmpty || end.IsInTheFuture();
			}
			return result;
		}
	}

	FallbackConfiguration DMSExportFallbackConfiguration => NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.Value;

	#endregion

	public bool HasRelatedExitControl => Declaration.ExitReports.Any(x => x.Consignment?.CXC_MovementReference.ToString() == EntryNumber);

	public EDIMessageCollection CustomsMessageRemarks
	{
		get
		{
			var result = new EDIMessageCollection(this);
			foreach (NLEDIMessage message in Messages)
			{
				if (!message.StatementType.IsEmpty)
				{
					result.Add(message);
				}
			}
			return result;
		}
	}

	public ZDateTime GetMostRecentStatusChangeEventTime(Event statusChangeEvent, ZString status)
	{
		return Logs.MostRecentLogByEventTime(statusChangeEvent, status)?.EventTime ?? ZDateTime.Empty;
	}

	public ZString RetrievePreviousValueFromLog(Event statusChangeEvent, ZDateTime comparedStatusChangeEventTime)
	{
		var query = new ZQuery(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThan, comparedStatusChangeEventTime);
		return Logs.MostRecentLogByEventTime(statusChangeEvent, query)?.Reference ?? ZString.Empty;
	}

	public INumberFountainProxy LrnNumberFountain => Env.NumberFountains.EULocalReferenceNumber(Declaration.Company.PK.ToGuid());
}
