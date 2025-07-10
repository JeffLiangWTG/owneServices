using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Documents.DocDataObjects;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

[SystemDefinedValues]
[VisualizableDocumentsSupportable(nameof(CusEntryHeaderITVisualizableDocumentSupporter))]
public partial class CusEntryHeader : AutoCusEntryHeader
	, Integration.Customs.IT.ICusEntryHeader
	, ISingleWindowRequestDataProvider
	, ICustomsEntryApplicationReference
	, ISendableCustomsEntry
	, ICustomsLinkedObjectAdapterProvider
	, ICustomsEntryTransmissionFailable
	, ICustomsProfileDataProvider
	, IMovementReferenceNumberProvider
	, ITopLevelBusinessObjectProvider
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
	{
		public const string CH_FreightAdjustment = "CH_FreightAdjustment";
		public const string CH_IncoTerm = "CH_IncoTerm";
		public const string ReleaseCode = "EntryNumbersProvider.ReleaseInfo.CE_EntryNum";
		public const string CustomsChannelDescription = "CustomsChannelDescription";
		public const int CustomsChannelMaxLength = 2;
		public const string CustomsChannel = "CustomsChannel";
		public const string InvoiceAmount = "InvoiceAmount";
		public const int InvoiceAmountCurrencyMaxLength = 3;
		public const string InvoiceAmountCurrency = "InvoiceAmountCurrency";
	}

	public static class GenAddOnColumnConstants
	{
		public const string CustomsChannelColumnName = "IT_CustomsChannel";
		public const string InvoiceAmountColumnName = "IT_InvoiceAmount";
		public const string InvoiceAmountCurrencyColumnName = "IT_InvoiceAmountCurrency";
	}

	protected override ZString EntryNumberType
	{
		get
		{
			if (entryNumberType == null)
			{
				entryNumberType = new CachedProperty<ZString>(Factory, () => GetEntryNumberType());
			}
			return entryNumberType.Value;
		}
	}
	CachedProperty<ZString> entryNumberType;

	ZString GetEntryNumberType()
	{
		var declaration = Declaration;
		if (declaration is null)
		{
			return ZString.Empty;
		}

		if (declaration.IsImport && declaration.IsInterface)
		{
			return CusEntryNumberConstants.EntryTypes.Import;
		}

		return CusEntryNumberConstants.EntryTypes.Mrn;
	}

	public override ZString DefaultStatusDescription => ZString.Empty;

	[ReadOnly(true)]
	public override ZString CH_Status { get => base.CH_Status; set => base.CH_Status = value; }

	[ReadOnly(true)]
	public override ZString CH_EntryStatus { get => base.CH_EntryStatus; set => base.CH_EntryStatus = value; }

	[ReadOnly(true)]
	public override ZString CH_WarehouseTransactionStatus { get => base.CH_WarehouseTransactionStatus; set => base.CH_WarehouseTransactionStatus = value; }

	[ReadOnly(true)]
	public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

	[ReadOnly(true)]
	public override ZDateTime CH_EntrySubmittedDate { get => base.CH_EntrySubmittedDate; set => base.CH_EntrySubmittedDate = value; }

	[ReadOnly(true)]
	public override ZString CH_MessageType { get => base.CH_MessageType; set => base.CH_MessageType = value; }

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryHeader|CH_FreightAdjustment", Caption = "Freight Adjustment")]
	public ZDecimal CH_FreightAdjustment
	{
		get => AddInfo.ZG_FreightAdjustment;
		set
		{
			if (CH_FreightAdjustment != value)
			{
				AddInfo.ZG_FreightAdjustment = value;
				CH_FreightAdjustmentInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo CH_FreightAdjustmentInfo => GetWrappedZPropertyInfo(Schema.CH_FreightAdjustment, _ => AddInfo.ZG_FreightAdjustmentInfo);

	[ReadOnly(true)]
	[MaxLength(Schema.CustomsChannelMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CustomsChannelCodeList))]
	public ZString CustomsChannel
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.CustomsChannelColumnName);
		set
		{
			var oldValue = CustomsChannel;
			if (oldValue != value)
			{
				CheckMaximumLength(CustomsChannelInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.CustomsChannelColumnName, value);
				CustomsChannelInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo CustomsChannelInfo => GetZPropertyInfo(Schema.CustomsChannel);

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryHeader|InvoiceAmount", Caption = "Invoice Amount")]
	public ZDecimal InvoiceAmount
	{
		get => this.GetSystemDefinedValue<ZDecimal>(GenAddOnColumnConstants.InvoiceAmountColumnName);
		set
		{
			var oldValue = InvoiceAmount;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.InvoiceAmountColumnName, value);
				InvoiceAmountInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo InvoiceAmountInfo => GetZPropertyInfo(Schema.InvoiceAmount);

	[ReadOnly(true)]
	[MaxLength(Schema.InvoiceAmountCurrencyMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryHeader|InvoiceAmountCurrency", Caption = "Invoice Amount Currency")]
	public ZString InvoiceAmountCurrency
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.InvoiceAmountCurrencyColumnName);
		set
		{
			var oldValue = InvoiceAmountCurrency;
			if (oldValue != value)
			{
				CheckMaximumLength(InvoiceAmountCurrencyInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.InvoiceAmountCurrencyColumnName, value);
				InvoiceAmountCurrencyInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo InvoiceAmountCurrencyInfo => GetZPropertyInfo(Schema.InvoiceAmountCurrency);

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryHeader|CH_IncoTerm", Caption = "Incoterm")]
	public ZString CH_IncoTerm
	{
		get => AddInfo.ZG_IncoTerm;
		set
		{
			if (CH_IncoTerm != value)
			{
				AddInfo.ZG_IncoTerm = value;
				CH_IncoTermInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo CH_IncoTermInfo => GetWrappedZPropertyInfo(Schema.CH_IncoTerm, _ => AddInfo.ZG_IncoTermInfo);

	protected override void ResetCachedValues()
	{
		base.ResetCachedValues();

		CH_FreightAdjustment = ZDecimal.Zero;
		InvoiceAmount = ZDecimal.Zero;
		InvoiceAmountCurrency = ZString.Empty;
		CH_IncoTerm = ZString.Empty;
	}

	public AllGroupedPreviousDocumentCollection AllGroupedPreviousDocuments => allGroupedPreviousDocuments ?? (allGroupedPreviousDocuments = AllGroupedPreviousDocumentCollection.LoadNew(this));
	AllGroupedPreviousDocumentCollection allGroupedPreviousDocuments;

	internal void ResetAllGroupedPreviousDocuments()
	{
		allGroupedPreviousDocuments = null;
		foreach (CusEntryLine entryLine in MergedLines)
		{
			entryLine.ResetGroupedPreviousDocuments();
		}
	}

	public ZBool IsEntryStatusRegisteredOrNbRejected => CH_EntryStatus == ITEntryStatusList.Codes.Registered || CH_EntryStatus == ITEntryStatusList.Codes.NbRejected;

	public ZBool IsEntryLockedForEditing => Factory.GetCached(ref isEntryLockedForEditingCachedProperty, GetIsEntryLockedForEditing);
	CachedProperty<bool> isEntryLockedForEditingCachedProperty;

	bool GetIsEntryLockedForEditing()
	{
		return IsWaitingForResponse || IsInLockedEntryStatus() || IsEntryAcknowledgedAndWaitingForIrisp();

		bool IsInLockedEntryStatus()
		{
			return Lookups.CH_EntryStatusList.ContainsCode(CH_EntryStatus)
				&& (!IsInAmendingStatus || CH_Status.In(messageStatusesLockEntryInAmending))
				&& !IsInDepositStatus;
		}

		bool IsEntryAcknowledgedAndWaitingForIrisp() => CH_Status == ITMessageStatusList.Codes.AcknowledgedOriginal && CH_EntryStatus.IsEmpty;
	}

	public bool IsInAmendableStatus => Factory.GetCached(ref isInAmendableStatusCachedProperty, () => new CusEntryHeaderAmendmentWrapper(this).IsInAmendableStatus);
	CachedProperty<bool> isInAmendableStatusCachedProperty;

	public ZBool IsInAmendingStatus => Factory.GetCached(ref isInAmendingStatusCachedProperty, () => new CusEntryHeaderAmendmentWrapper(this).IsAmending);
	CachedProperty<bool> isInAmendingStatusCachedProperty;

	public override bool IsWaitingForResponse => Common.Shared.MessageStatusList.IsAwaiting(CH_Status);

	public ZBool StatusAllowsSending => CH_Status == ITMessageStatusList.Codes.NotSent
			|| CH_Status == ITMessageStatusList.Codes.ErrorOriginal
			|| CH_Status == Common.EU.MessageStatusList.Codes.FailedFromTransmission
			|| (CH_Status == ITMessageStatusList.Codes.AcknowledgedOriginal && CH_EntryStatus.In(sendableEntryStatuses))
			|| (IsInAmendingStatus && !IsWaitingForResponse);

	public ZString CustomsChannelDescription => Lookups.CustomsChannelCodeList.GetDescriptionFromCode(CustomsChannel);

	protected override ZString SadBoxATextCore
	{
		get
		{
			var office = Declaration?.JE_CustomsOffice ?? ZString.Empty;
			var formattedOffice = ZString.Empty;
			var formattedMRN = ZString.Empty;
			var result = new ZStringBuilder();

			if (!office.IsEmpty && EffectiveValuationDate.IsValid)
			{
				var presentationOffice = ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, Declaration.JE_CustomsOffice, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, EffectiveValuationDate);
				if (presentationOffice != null)
				{
					var name = presentationOffice.ZZD_Description.Left(SADConstants.CustomsFieldMaxLength.EntryCustomsOffice.Name);
					formattedOffice = FormattableString.Invariant($"{office} - {name}");
				}
			}

			if (HasMrn)
			{
				formattedMRN = FormattableString.Invariant($"{CusEntryNumberConstants.EntryTypes.Mrn}: {MovementReferenceNumber}");
			}

			result.AppendIfNotEmpty(formattedOffice);
			result.AppendIfNotEmpty(formattedMRN);

			return result.ToStringWithNewLineBetweenAppends();
		}
	}

	public IEnumerable<CusEntryLine> MergedLinesWithSendableGroupedPreviousDocuments => MergedLines.Cast<CusEntryLine>().Where(x => x.GroupedPreviousDocuments.IsSendableInNbMessage);

	protected override ZBool ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction => ZBool.False;

	protected override ZDecimal TotalCustomsQuantityCore => IsInProcessOfMerging ? ZDecimal.Zero : base.TotalCustomsQuantityCore;

	public override ZInt PackagesCount => IsInProcessOfMerging ? ZInt.Zero : base.PackagesCount;

	protected override bool IsFailedFromTransmissionCore => CH_Status == ITMessageStatusList.Codes.FailedFromTransmission;

	protected override ZString SupplierEoriOfMainOfficeCore => Declaration?.SupplierTraderId ?? ZString.Empty;

	protected override ZString ImporterEoriOfMainOfficeCore => Declaration?.ImporterTraderId ?? ZString.Empty;

	public override bool ShouldLogEntryStatus => true;

	protected override bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivot => true;

	public ZString LocationOfGoods
	{
		get
		{
			if (locationOfGoodsCache == null)
			{
				locationOfGoodsCache = new CachedProperty<ZString>(Factory, () =>
				{
					if (Declaration == null || EntryInstruction == null)
					{
						return ZString.Empty;
					}
					return new LocationOfGoodsCalculator().Calculate(this);
				});
			}
			return locationOfGoodsCache.Value;
		}
	}

	CachedProperty<ZString> locationOfGoodsCache;

	public ZString RegistrationNumber => EntryNumbersProvider.RegistrationInfo?.CE_EntryNum ?? ZString.Empty;

	public CusEntryHeaderEntryNumbersProvider EntryNumbersProvider => entryNumbersProvider ?? (entryNumbersProvider = new CusEntryHeaderEntryNumbersProvider(this));
	CusEntryHeaderEntryNumbersProvider entryNumbersProvider;

	protected override void AfterSetAsFailedFromTransmission(ZString previousCH_EntryStatus)
	{
		var logText = FormattableString.Invariant($"Entry set to {ITMessageStatusList.Codes.FailedFromTransmission}, original status: {previousCH_EntryStatus}");
		Logs.AddNew(Events.CustomsStatusOverride, logText, ZDateTimeOffset.Now);
	}

	public bool EntryStatusHasHigherPriorty(ZString entryStatus)
	{
		var result = Declaration.IsExport ? SadCustomsStatusInformationProvider.CompareExportCustomsStatusOrder(entryStatus, CH_EntryStatus) : SadCustomsStatusInformationProvider.CompareImportCustomsStatusOrder(entryStatus, CH_EntryStatus);
		return result < 0;
	}

	public void DoManualReleaseAndSetAsChanged(EntryManualReleaseHandler manualRelease)
	{
		var manualReleaseDate = manualRelease.ReleaseDate;
		EntryNumbersProvider.InsertOrUpdateManualReleaseCode(manualRelease.ReleaseCode, manualReleaseDate);
		CH_EntryReleaseDate = manualReleaseDate;
		CH_EntryStatus = Declaration.IsImport ? ITEntryStatusList.Codes.ImportCleared : ITEntryStatusList.Codes.ExportCleared;
		HasChanges = true;
	}

	public void SetAsAmending(ZString? movementReferenceNumber = null, ZInt? totalEntryLines = null)
	{
		var previousEntryStatus = CH_EntryStatus;
		var previousStatus = CH_Status;

		CH_EntryStatus = ITEntryStatusList.Codes.Amending;
		CH_Status = ZString.Empty;

		if (movementReferenceNumber is ZString value && !value.IsEmpty)
		{
			MovementReferenceNumberSetter(value);
			MovementReferenceNumberInfo.RefreshBinding();
		}

		if (totalEntryLines is ZInt totalEntryLinesInt && !totalEntryLinesInt.IsEmpty)
		{
			ZG_SentEntryLinesCount = totalEntryLinesInt;
		}

		var logText = FormattableString.Invariant($"Entry set to AMG, original status: [{previousEntryStatus} | {previousStatus}]");
		Logs.AddNew(Events.CustomsStatusOverride, logText, ZDateTimeOffset.Now);
	}

	public void ResetCancelledEntry()
	{
		CH_Status = ZString.Empty;
		CH_EntryStatus = ZString.Empty;
		CH_EntrySubmittedDate = ZDateTime.Empty;
		CH_EntryReleaseDate = ZDateTime.Empty;
		EntryPayInfos.RemoveAndDeleteAll();

		CusEntryNumber?.Delete();
		MovementReferenceCusEntryNumber?.Delete();
		EntryNumbersProvider.RegistrationInfo?.Delete();
		EntryNumbersProvider.ReleaseInfo?.Delete();
		EntryNumbersProvider.Ivisto?.Delete();
	}

	#region ISingleWindowRequestDataProvider members

	ZString ISingleWindowRequestDataProvider.ApplicationReference => Declaration?.GetApplicationReference() ?? ZString.Empty;

	ZDate ISingleWindowRequestDataProvider.IssueDate => EntryNumbersProvider.RegistrationInfoWrapper.IssueDate;

	ZString ISingleWindowRequestDataProvider.RegisterIncludingSeries => EntryNumbersProvider.RegistrationInfoWrapper.RegisterIncludingSeries;

	ZString ISingleWindowRequestDataProvider.RegistrationNumberWithoutCin => EntryNumbersProvider.RegistrationInfoWrapper.RegistrationNumberWithoutCin;

	ZString ISingleWindowRequestDataProvider.CustomsOffice => Declaration?.JE_CustomsOffice ?? ZString.Empty;

	ZGuid ISingleWindowRequestDataProvider.PK => PK;

	ZString ISingleWindowRequestDataProvider.TableName => CusEntryHeader.Schema.TableName;

	#endregion

	#region ICustomsEntryApplicationReference Members

	ZString ICustomsEntryApplicationReference.Node => Declaration?.Node ?? ZString.Empty;

	ZString ICustomsEntryApplicationReference.Subscriber => Declaration?.JE_GS_NKCusAgent ?? ZString.Empty;

	ZString ICustomsEntryApplicationReference.CustomsOffice => Declaration?.JE_CustomsOffice ?? ZString.Empty;

	ZString ICustomsEntryApplicationReference.CustomsProfile => CustomsProfile;

	#endregion

	#region ISendableCustomsEntry

	void ISendableCustomsEntry.ConsumeGuarantee(BusinessObjectFactory factory, ITEDIMessage message)
	{
	}

	void ISendableCustomsEntry.PreProcessBeforeSending()
	{
	}

	void ISendableCustomsEntry.MarkAsSent(IMessageType sentMessage)
	{
		if (!StatusAllowsSending)
		{
			Declaration?.Logs.LogCustomsStatusOverride(CH_BGMReference, CH_Status, CH_EntryStatus);
		}

		CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		foreach (var entryLine in MergedLinesWithSendableGroupedPreviousDocuments)
		{
			entryLine.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Sent;
		}

		if (sentMessage?.IsCancellation ?? ZBool.False)
		{
			CH_EntryStatus = ITEntryStatusList.Codes.Canceling;
		}
	}

	ZString ISendableCustomsEntry.CustomsProfile => CustomsProfile;

	#endregion

	#region ICustomsLinkedObjectAdapterProvider Members

	ISadCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(this);

	ISingleWindowCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(this);

	IXmlCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter() => new CusEntryHeaderCustomsLinkedObjectAdapter(this);

	#endregion

	#region ITopLevelBusinessObjectProvider Members

	BusinessObject ITopLevelBusinessObjectProvider.TopLevelBusinessObject
	{
		get
		{
			var declaration = Declaration;
			if (declaration is null)
			{
				return null;
			}

			return declaration.Shipment as BusinessObject ?? declaration;
		}
	}

	#endregion

	public ZBool IsNbRejected => CH_EntryStatus == ITEntryStatusList.Codes.NbRejected;
	public ZBool IsNbNeRejected => IsNbRejected;

	public ZBool IsInDepositStatus => CH_EntryStatus == ITEntryStatusList.Codes.Deposited;

	public ZBool HasCertificateOfOriginMessage => GetCertificateOfOriginMessage() != null;

	public bool IsCancellationAcceptedBySystem => CH_EntryStatus == ITEntryStatusList.Codes.Canceled && CH_Status == ITMessageStatusList.Codes.AcceptedBySystem;

	public ITEDIMessage GetCertificateOfOriginMessage() => Messages.GetLastMessageByType(EDIMessageTypeList.Codes.CertificateOfOrigin);

	public ZString NatureOfTransaction => RandomHeader?.JZ_ValuationCode ?? ZString.Empty;

	public ZString TransportChargesMethodOfPayment => RandomHeader?.ZG_TransportChargesMethodOfPayment ?? ZString.Empty;

	public ZString CountryOfDestination => Declaration?.CountryOfDestinationCode ?? ZString.Empty;

	public ZString CountryOfExport => Declaration?.JE_GoodsOrigin ?? ZString.Empty;

	public ZBool HasMrn => Factory.GetCached(ref hasMrnCached, () => !MovementReferenceNumber.IsEmpty);
	CachedProperty<bool> hasMrnCached;

	protected override DocumentSupporter CreateNewDocumentSupporter()
	{
		return new CusEntryHeaderDocumentSupporter(this);
	}

	protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocumentsToProcess()
	{
		if (Declaration?.IsUCC6AndIsExport ?? false)
		{
			return GetEntryInstructionLevelSupportingDocuments();
		}

		return base.GetSupportingDocumentsToProcess();
	}

	protected override ZString RepresentativeOrDeclarantEoriOfMainOfficeCore => GetRepresentativeOrDeclarantEoriOfMainOffice();

	ZString GetRepresentativeOrDeclarantEoriOfMainOffice()
	{
		if (!Declaration?.IsUCC6 ?? false)
		{
			return base.RepresentativeOrDeclarantEoriOfMainOfficeCore;
		}

		var representativeEuIdentificationNumber = RepresentativeOrganisation?.GetEuIdentificationNumber() ?? ZString.Empty;

		return representativeEuIdentificationNumber.IsEmpty
			? (DeclarantOrganisation?.GetEuIdentificationNumber() ?? ZString.Empty)
			: representativeEuIdentificationNumber;
	}

	IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetEntryInstructionLevelSupportingDocuments()
	{
		if (EntryInstruction == null)
		{
			yield break;
		}

		foreach (var entryInstructionSupportingDocument in EntryInstruction.SupportingDocuments)
		{
			yield return entryInstructionSupportingDocument;
		}
	}

	public ZString CustomsProfile => Declaration?.JE_CustomsProfile ?? ZString.Empty;

	public ZBool BondedWarehouseProcessingRequired => WarehouseTransactionStatusList.IsPendingInwardOrOutward(CH_WarehouseTransactionStatus) && !SkipBondedWarehouseProcessing;

	bool SkipBondedWarehouseProcessing => skipBondedWarehouseEntryStatusToSkipPredicate.TryGetValue(CH_EntryStatus, out var checkStatusFunc) && checkStatusFunc(CH_Status);

	readonly ImmutableArray<ZString> sendableEntryStatuses = new ZString[]
	{
		ITEntryStatusList.Codes.NbRejected,
		ITEntryStatusList.Codes.Deposited,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> messageStatusesLockEntryInAmending = new ZString[]
	{
		ITMessageStatusList.Codes.AcknowledgedOriginal,
		ITMessageStatusList.Codes.AcceptedBySystem,
	}.ToImmutableArray();

	readonly Dictionary<string, Func<string, bool>> skipBondedWarehouseEntryStatusToSkipPredicate = new()
	{
		{ string.Empty, (s) => s.In(string.Empty, ITMessageStatusList.Codes.AcknowledgedOriginal) },
		{ ITEntryStatusList.Codes.Registered, (s) => s == ITMessageStatusList.Codes.AcknowledgedOriginal },
		{ ITEntryStatusList.Codes.UnderControl, (s) => true },
		{ ITEntryStatusList.Codes.Canceling, (s) => s.In(ITMessageStatusList.Codes.AcknowledgedOriginal, ITMessageStatusList.Codes.AcceptedBySystem) },
		{ ITEntryStatusList.Codes.Amending, (s) => s.In(string.Empty, ITMessageStatusList.Codes.AcknowledgedOriginal, ITMessageStatusList.Codes.AcceptedBySystem) },
	};
}
