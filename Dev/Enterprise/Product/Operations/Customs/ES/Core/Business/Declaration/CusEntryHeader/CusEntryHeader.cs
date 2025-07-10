using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using Constants = CargoWise.EventReference.Constants;
using MessageStatusList = Enterprise.Customs.Common.EU.MessageStatusList;
using PackageType = Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType;

namespace Enterprise.Customs.ES.Business.Declaration;

[SystemDefinedValues]
[DependentBusinessObject(typeof(JobDeclaration), "CustomsEntryHeaders")]
[VisualizableDocumentsSupportable("CusEntryHeaderESVisualizableDocumentSupporter")]

public partial class CusEntryHeader : AutoCusEntryHeader, Integration.Customs.ES.ICusEntryHeader, ICusStorageDocPivotParent, IESMessageInfoProvider, IESResponseBOMessageStatus, IEDocsDelayedSaver, IPollingTransactionParent
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Schema
	public new partial class Schema : AutoCusEntryHeader.Schema
	{
		public const string FormattedCircuit = "FormattedCircuit";
		public const string FormattedCircuitCan = "FormattedCircuitCan";
		public const string FormattedClearanceResult = "FormattedClearanceResult";
		public const string FormattedEADPrint = "FormattedEADPrint";
		public const string TotalAmount = "TotalAmount";
		public const string T2CMovementReferenceNumber = "T2CMovementReferenceNumber";
		public const string IndirectExport = "IndirectExport";
		public const string CSVClearance = "CSVClearance";
		public const string CircuitCan = "CircuitCan";
	}
	#endregion

	#region GenAddOn
	public static class GenAddOnColumnConstants
	{
		public const string IndirectExportColumnName = "ES_IndirectExport";
	}
	#endregion

	#region New Properties

	public ZString FormattedCircuit
	{
		get
		{
			var code = MovementReferenceNumberEntryStatus;
			var description = (ZString)AddInfoLookups.CircuitCodeList.GetDescriptionFromCode(code);

			return !description.IsEmpty ? description : code;
		}
	}
	public ZPropertyInfo FormattedCircuitInfo => MovementReferenceNumberEntryStatusInfo;

	public ZString FormattedCircuitCan
	{
		get
		{
			var code = CircuitCan;
			var description = (ZString)AddInfoLookups.CircuitCodeList.GetDescriptionFromCode(code);

			return !description.IsEmpty ? description : code;
		}
	}
	public ZPropertyInfo FormattedCircuitCanInfo => CircuitCanInfo;

	public ZString FormattedClearanceResult
	{
		get
		{
			var code = ZG_ClearanceResult;
			var description = (ZString)AddInfoLookups.ClearanceResultCodeList.GetDescriptionFromCode(code);

			return !description.IsEmpty ? description : code;
		}
	}
	public ZPropertyInfo FormattedClearanceResultInfo => CSVClearanceInfo;

	public ZString FormattedEADPrint => ((CusEUEntryHeader)AddInfoChild).FormattedEADPrint;
	public ZPropertyInfo FormattedEADPrintInfo => EUH_EADPrintProcedureInfo;

	public ZInt TotalPackagesQty => Factory.GetValue(ref totalPackagesQty, () =>
	{
		return InvoiceLines.Sum(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Sum(p =>
			{
				var packageType = p.Package?.CW_PackType ?? ZString.Empty;
				return packageType != EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked
					&& packageType != PackageType.Frame ? p.CHC_NumberOfPacks : ZInt.Zero;
			}));
	});
	CachedProperty<ZInt> totalPackagesQty;

	public ZInt TotalPiecesQty => Factory.GetValue(ref totalPiecesQty, () =>
	{
		return InvoiceLines.Sum(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Sum(p => (p.Package?.CW_PackType ?? ZString.Empty) == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked ? p.CHC_NumberOfPacks : ZInt.Zero));
	});
	CachedProperty<ZInt> totalPiecesQty;

	public ZInt TotalPackagesQtyNonBulk => Factory.GetValue(ref totalPackagesQtyNonBulk, () =>
	{
		return InvoiceLines.Sum(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Sum(p =>
			{
				var packageType = p.Package?.CW_PackType ?? ZString.Empty;
				return !PackageHelper.PackTypeIsBulk(packageType, Factory)
						&& packageType != PackageType.Frame ? p.CHC_NumberOfPacks : ZInt.Zero;
			}));
	});
	CachedProperty<ZInt> totalPackagesQtyNonBulk;

	public ZInt TotalVehiclesQty => Factory.GetValue(ref totalVehiclesQty, () =>
	{
		return AllEntryLines.Cast<CusEntryLine>().Sum(x => x.VehiclesQty);
	});
	CachedProperty<ZInt> totalVehiclesQty;

	public ZInt TotalBulkTypePackages => Factory.GetValue(ref totalBulkTypePackages, () =>
	{
		return InvoiceLines.Sum(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Count(p => PackageHelper.PackTypeIsBulk((p.Package?.CW_PackType ?? ZString.Empty), Factory)));
	});
	CachedProperty<ZInt> totalBulkTypePackages;

	public override ZInt PackagesCount
	{
		get
		{
			var totalPackagesCount = ZInt.Zero;
			if (IsExsSubStyle)
			{
				totalPackagesCount += TotalPackagesQtyNonBulk + TotalBulkTypePackages + TotalVehiclesQty;
			}
			else
			{
				totalPackagesCount += TotalPackagesQty + TotalPiecesQty + TotalVehiclesQty;
			}
			return totalPackagesCount;
		}
	}

	public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;
	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);
	protected override Customs.Business.CommonGoodsItemsIntegration.ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore => new ESCommonGoodsItemsIntegrator(this);

	public ZDecimal TotalAmount
	{
		get => this.GetSystemDefinedValue<ZDecimal>(GenAddOnHelper.TotalAmount);
		set
		{
			var oldValue = TotalAmount;
			this.SetSystemDefinedValue(GenAddOnHelper.TotalAmount, value);
			TotalAmountInfo.RefreshBinding(oldValue);
		}
	}
	public ZPropertyInfo TotalAmountInfo => GetZPropertyInfo(Schema.TotalAmount);

	#endregion

	protected override ZDecimal GetTotalGrossWeightInKG()
	{
		var totalGross = base.GetTotalGrossWeightInKG();
		return IsUCC6 ? totalGross : (ZDecimal)MergedLines.Sum(l => l.TotalGrossWeightInKG);
	}

	public IEnumerable<ZString> SealCodes
	{
		get
		{
			var sealCodesList = new List<ZString>();
			MergedLines.ForEach(x => sealCodesList.AddRange(x.SealsFromContainers()));
			MergedLines.ForEach(x => sealCodesList.AddRange(x.SealsFromEquipments));
			return sealCodesList.Distinct().ToArray();
		}
	}

	public IEnumerable<ZString> SealCodesEquipments
	{
		get
		{
			var sealCodesList = new List<ZString>();
			MergedLines.ForEach(x => sealCodesList.AddRange(x.SealsFromEquipments));
			return sealCodesList.Distinct().ToArray();
		}
	}

	public override bool ShouldLogEntryStatus => true;

	protected override bool IsOriginAndDestinationRequiredInItinerary => IsExsSubStyle || IsExportUCC6;

	protected override bool CanRepeatCountriesInItinerary => IsExsSubStyle || IsExportUCC6;

	public bool IsExsSubStyle => EntryInstruction != null && EntryInstruction.CEI_SubStyle == ExsEntrySubStyleList.Codes.EXS;

	public bool IsH2Style => EntryInstruction != null && EntryInstruction.CEI_Style == IMPDeclarationTypeList.Codes.H2;

	public bool IsUCC6 => ZG_UCC6Version > UCC6VersionCodes.NoUCC6;

	public bool IsExportUCC6 => Declaration.IsExport && IsUCC6;

	public bool IsExportNoUCC6 => Declaration.IsExport && !IsUCC6;

	public bool IsImportUCC6 => Declaration.IsImport && IsUCC6;

	protected override string GetLoadPortForItinerary() => Declaration.JE_RL_NKOrigin.ToUpper();

	protected override string GetDischargePortForItinerary() => Declaration.JE_RL_NKFinalDestination.ToUpper();

	#region T2CMovementReferenceNumber
	public ZString T2CMovementReferenceNumber => T2CMovementReferenceEntryNumber != null ? T2CMovementReferenceEntryNumber.CE_EntryNum : ZString.Empty;

	public ZPropertyInfo T2CMovementReferenceNumberInfo => GetZPropertyInfo(Schema.T2CMovementReferenceNumber);

	protected CusEntryNumber T2CMovementReferenceEntryNumber => CusEntryNumber.Load(this, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, CountryCode);
	#endregion
	public ZDecimal InvoiceAmount => InvoiceLines.Sum(l => l.JI_LinePrice);

	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CurrencyList))]
	public ZString InvoiceAmountCurrency => RandomHeader.JZ_RX_NKInvoice_Currency;

	#region MRNReferenceNumber
	[ReadOnlyMember(nameof(MRNFieldsReadOnly))]
	[MaxLength(Common.AutoCusEntryNum.Schema.CE_EntryNumMaxLength)]
	[ResourceStringData("E282C17B-EA37-427D-8A0A-C183323FC0BB", Caption = "MRN")]
	public new ZString MovementReferenceNumber
	{
		get => base.MovementReferenceNumber;
		set
		{
			var oldValue = base.MovementReferenceNumber;
			if (!IsCopying && oldValue != value)
			{
				CheckMaximumLength(MovementReferenceNumberInfo, value);
				MovementReferenceNumberSetter(value, MovementReferenceNumberIssueDate, MovementReferenceNumberEntryStatus);
				MovementReferenceNumberInfo.RefreshBinding(oldValue);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateMovementReferenceNumber();
			}
		}
	}

	[ReadOnlyMember(nameof(MRNFieldsReadOnly))]
	public new ZDateTime MovementReferenceNumberIssueDate
	{
		get
		{
			if (issueDate.IsEmpty)
			{
				issueDate = base.MovementReferenceNumberIssueDate;
			}
			return issueDate;
		}
		set
		{
			var oldValue = issueDate;
			if (!IsCopying && oldValue != value)
			{
				issueDate = value;
				MovementReferenceNumberSetter(MovementReferenceNumber, issueDate, MovementReferenceNumberEntryStatus);
				MovementReferenceNumberIssueDateInfo.RefreshBinding(oldValue);
			}
		}
	}
	ZDateTime issueDate;

	public void SetMovementReferenceNumberEntryStatus(ZString entryStatus) => MovementReferenceNumberSetter(MovementReferenceNumber, MovementReferenceNumberIssueDate, entryStatus);

	#endregion

	#region CSVClearance
	[ReadOnly(true)]
	[MaxLength(AutoCusEntryNum.Schema.CE_EntryNumMaxLength)]
	public ZString CSVClearance => CSVClearanceFromEntryNumber;

	ZString CSVClearanceFromEntryNumber => LoadCusEntryNumber(CusEntryNumberTypes.Spain.ClearanceCSV)?.CE_EntryNum ?? ZString.Empty;

	public void SetCSVClearanceNum(ZString csvCode) => CusEntryNumberSetter(CusEntryNumberTypes.Spain.ClearanceCSV, csvCode);

	public ZPropertyInfo CSVClearanceInfo => GetZPropertyInfo(Schema.CSVClearance);
	#endregion

	#region CircuitCan
	[ReadOnly(true)]
	[MaxLength(AutoCusEntryNum.Schema.CE_EntryStatusMaxLength)]
	public ZString CircuitCan => CircuitCanFromCusEntryNumStatus;

	ZString CircuitCanFromCusEntryNumStatus => LoadCusEntryNumber(CusEntryNumberTypes.Spain.CircuitCan)?.CE_EntryStatus ?? ZString.Empty;

	public void SetCircuitCan(ZString circuitCan) => CusEntryNumberSetter(CusEntryNumberTypes.Spain.CircuitCan, MovementReferenceNumber, circuitCan);

	public ZPropertyInfo CircuitCanInfo => GetZPropertyInfo(Schema.CircuitCan);
	#endregion

	CusEntryNumber LoadCusEntryNumber(ZString entryNumType) => CusEntryNumber.Load(this, entryNumType, CountryCode);

	void CusEntryNumberSetter(ZString entryNumType, ZString entryNum, ZString? entryStatus = null)
	{
		var cusEntryNumber = CusEntryNumber.LoadOrCreate(this, entryNumType, CountryCode);
		cusEntryNumber.CE_EntryIsSystemGenerated = true;
		cusEntryNumber.CE_EntryNum = entryNum;
		if (entryStatus.HasValue)
		{
			cusEntryNumber.CE_EntryStatus = entryStatus.Value;
		}
	}

	[ReadOnly(true)]
	public new ZDecimal Duty => base.Duty;

	[ReadOnly(true)]
	public new ZDecimal VAT => base.VAT;

	[ReadOnly(true)]
	public override ZString CH_EntryStatus { get => base.CH_EntryStatus; set => base.CH_EntryStatus = value; }

	[ReadOnly(true)]
	public override ZString CH_Status { get => base.CH_Status; set => base.CH_Status = value; }

	[ReadOnly(true)]
	public override ZString CH_MessageType { get => base.CH_MessageType; set => base.CH_MessageType = value; }

	[ReadOnly(true)]
	public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

	[ReadOnly(true)]
	public override ZDateTime CH_EntrySubmittedDate { get => base.CH_EntrySubmittedDate; set => base.CH_EntrySubmittedDate = value; }

	[ReadOnly(true)]
	public override ZDateTime ZG_AcceptanceDate { get => base.ZG_AcceptanceDate; set => base.ZG_AcceptanceDate = value; }

	[ReadOnly(true)]
	public override ZString ZG_CSVClearance { get => base.ZG_CSVClearance; set => base.ZG_CSVClearance = value; }

	[ReadOnly(true)]
	public override ZString ZG_CSVImportCertificate { get => base.ZG_CSVImportCertificate; set => base.ZG_CSVImportCertificate = value; }

	[ReadOnly(true)]
	public override ZString ZG_ExportMRN { get => base.ZG_ExportMRN; set => base.ZG_ExportMRN = value; }

	[ReadOnly(true)]
	public override ZDateTime ZG_LimitPaymentDate { get => base.ZG_LimitPaymentDate; set => base.ZG_LimitPaymentDate = value; }

	[ReadOnly(true)]
	public override ZDateTime ZG_ATCLimitPaymentDate { get => base.ZG_ATCLimitPaymentDate; set => base.ZG_ATCLimitPaymentDate = value; }

	[ReadOnly(true)]
	public override ZString ZG_PaymentProofNumber { get => base.ZG_PaymentProofNumber; set => base.ZG_PaymentProofNumber = value; }

	[ReadOnly(true)]
	public override ZString ZG_ATCPaymentProofNumber { get => base.ZG_ATCPaymentProofNumber; set => base.ZG_ATCPaymentProofNumber = value; }

	[ReadOnly(true)]
	public override ZBool ZG_Parallel { get => base.ZG_Parallel; set => base.ZG_Parallel = value; }

	[ReadOnly(true)]
	public override ZDateTime ZG_LimitDateOfArrival { get => base.ZG_LimitDateOfArrival; set => base.ZG_LimitDateOfArrival = value; }

	[ReadOnly(true)]
	public override ZString ZG_CSVT2L { get => base.ZG_CSVT2L; set => base.ZG_CSVT2L = value; }

	[ReadOnly(true)]
	public override ZString ZG_CSVExitCertificate { get => base.ZG_CSVExitCertificate; set => base.ZG_CSVExitCertificate = value; }

	[ReadOnly(true)]
	[ResourceStringData("32EAC1F9-983B-4EEA-A2B9-BB969F10D052", Caption = "Indirect Export", MediumCaption = "Indirect Export", ShortCaption = "Ind.Export")]
	public ZBool IndirectExport
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.IndirectExportColumnName);
		set
		{
			var oldValue = IndirectExport;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.IndirectExportColumnName, value);
				IndirectExportInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo IndirectExportInfo => GetZPropertyInfo(Schema.IndirectExport);

	ZBool MRNFieldsReadOnly
	{
		get
		{
			var subStyle = EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			var entryStatus = CH_EntryStatus;

			ZBool retVal;
			if (subStyle == EntrySubStyleList.Codes.T2C && (entryStatus == ZString.Empty || entryStatus == EntryStatusCodes.Error))
			{
				retVal = false;
			}
			else
			{
				var statusIsClearOrPermanentClear = entryStatus == EntryStatusCodeList.Clear || entryStatus == EntryStatusCodeList.PermanentClear;
				var messageIsAwaitingResponse = CH_Status == MessageStatusList.Codes.AwaitingResponse;
				retVal = subStyle != EntrySubStyleList.Codes.T2L || statusIsClearOrPermanentClear || messageIsAwaitingResponse;
			}

			return retVal;
		}
	}

	protected override bool TaxFeePaymentCodeIsDeferredCore(ZString cF_MethodOfPayment)
	{
		return cF_MethodOfPayment == FeeMethodOfPayment.Deferred || cF_MethodOfPayment == FeeMethodOfPayment.NonBillableTax;
	}

	protected override bool CanAddDeferredFeeToTotalChargeValue => false;

	public override bool HasBeenLodgedAtCustoms => LodgedAtCustomsEntryStatus.Contains(CH_EntryStatus);

	internal ImmutableHashSet<string> LodgedAtCustomsEntryStatus = ImmutableHashSet.Create(
		EntryStatusCodes.Cleared,
		EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
		EntryStatusCodes.CustomsDeclarationAccepted,
		EntryStatusCodes.PreDeclarationAccepted,
		EntryStatusCodes.Cancelled);

	public override bool IsWaitingForResponse => CH_Status == MessageStatusList.Codes.AwaitingResponse;

	protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => !LockNumberOfEntryLines && CH_EntryStatus != EntryStatusCodes.IncompletePreDeclaration;

	protected override void OnFactorySaving()
	{
		FillInBGM();
		base.OnFactorySaving();
		LogStatusIfRequired();
	}

	void FillInBGM()
	{
		if (CH_BGMReference.IsEmpty || (!IsInDatabase && Declaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin))
		{
			var year = ZDate.Today.Year.ToString("0000");
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			CH_BGMReference = Environment.Env.NumberFountains.ESBGMReference(year, registrationKey.EnterpriseCode, registrationKey.ServerCode).GetNextFormatted(Factory);
		}
	}

	void LogStatusIfRequired()
	{
		if (!IsInDatabase || CH_EntryStatusInfo.HasChanges)
		{
			Declaration?.LogCSHForHVLVStandAloneDeclaration();
		}
	}

	protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocumentsToProcess()
	{
		if (EntryInstruction is CusEntryInstruction instruction)
		{
			foreach (var document in instruction.SupportingDocuments)
			{
				yield return document;
			}
		}

		var docsFromBase = base.GetSupportingDocumentsToProcess();
		foreach (var document in docsFromBase)
		{
			yield return document;
		}
	}

	protected override void OnFactorySavingBeforeTransactionCore()
	{
		base.OnFactorySavingBeforeTransactionCore();
		RemoveEDocPivotCollection();
	}

	void RemoveEDocPivotCollection()
	{
		if (!RequiresT2LAnnexes() && !RequiresAESAnnexes && !RequiresT2LPOUSAnnexes && !RequiresH1Annexes)
		{
			EDocPivotCollection.RemoveAndDeleteAll();
		}
	}

	Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

	void ICusStorageDocPivotTypeSupporter.ReloadCollection()
	{
		if (IsEDocPivotCollectionLoaded)
		{
			EDocPivotCollection.Reload(true);
		}
	}

	[ChildEditable(true)]
	public CusStorageDocPivotCollection EDocPivotCollection
	{
		get
		{
			if (eDocPivotCollection == null)
			{
				eDocPivotCollection = new CusStorageDocPivotCollection(this);
				eDocPivotCollection.Load();
				RegisterEditableChildObject(eDocPivotCollection);
			}

			return eDocPivotCollection;
		}
	}
	CusStorageDocPivotCollection eDocPivotCollection;

	public ZBool ShouldAnnexesBeReadOnly => ZG_RequestDispatch == YesNoList.Codes.Yes;

	public List<CusStorageDocPivot> GetAllSendableEDocPivots()
	{
		return EDocPivotCollection.Where(p =>
									p.MessageStatus == ZString.Empty
									|| p.MessageStatus == EDIMessageStatusList.Codes.Failed
									|| p.MessageStatus == EDIMessageStatusList.Codes.Error
									|| p.MessageStatus == EDIMessageStatusList.Codes.Rejected
									|| p.MessageStatus == MessageStatusList.Codes.FailedFromTransmission
								).ToList();
	}

	bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

	CusStorageDocPivotCollection ICusStorageDocPivotParent.EDocPivotCollection => EDocPivotCollection;

	IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections
	{
		get
		{
			var declaration = Declaration;

			if (declaration != null)
			{
				foreach (var eDocCollection in EDocsHelper.GetEDocCollections(declaration, declaration.Shipment))
				{
					yield return eDocCollection;
				}
			}
		}
	}

	public override void Delete()
	{
		EDocPivotCollection.RemoveAndDeleteAll();

		base.Delete();
	}

	public ZBool IsT2L => !IsDeleted && EntryInstruction != null && EntryInstruction.IsT2L;

	public ZBool IsT2C => !IsDeleted && EntryInstruction != null && EntryInstruction.IsT2C;

	public ZBool IsT2LorT2C => IsT2L || IsT2C;

	public ZBool IsT2LorT2CorEXS => IsT2L || IsT2C || IsExsSubStyle;

	public ZBool IsStyleEmpty => !IsDeleted && EntryInstruction != null && EntryInstruction.IsStyleEmpty;

	public ZBool IsAcceptedExport => !IsDeleted && Declaration.IsExport
									&& (CH_EntryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations || CH_EntryStatus == EntryStatusCodes.Cleared || CH_EntryStatus == EntryStatusCodes.CustomsDeclarationAccepted)
									&& EntryInstruction != null && EntryInstruction.IsSubStyleAOrBOrCOrZ;

	public ZBool RequiresH1Annexes => IsImportUCC6 && !IsH2Style && (EntryStatusIsCDAorCDP || EntryStatusIsCLJ || HasAnnexes);

	ZBool EntryStatusIsCDAorCDP => CH_EntryStatus == EntryStatusCodes.CustomsDeclarationAccepted || CH_EntryStatus == EntryStatusCodes.ClearedWithPendingDocuments;
	ZBool EntryStatusIsCLJ => CH_EntryStatus == EntryStatusCodes.ClearedWithPendingJustificationCertificatesDJP;

	public ZBool RequiresAESAnnexes => (Declaration.IsExport &&
		EntryInstruction != null &&
		!MovementReferenceNumber.IsEmpty &&
		EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ &&
		IsUCC6 &&
		(CSVClearance.IsEmpty || HasAnnexes));

	public ZBool RequiresT2LAnnexes() => ZG_POUSVersion == 0 && IsT2L && !(Declaration.IsExport
		&& InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Any(x => x.CSI_Code.Equals("9010")));

	public ZBool RequiresT2LPOUSAnnexes => EntryInstruction != null
											&& ZG_POUSVersion > 0
											&& (EntryInstruction.IsT2L || EntryInstruction.IsT2C)
											&& !MovementReferenceNumber.IsEmpty
											&& (CSVClearance.IsEmpty || HasAnnexes );

	public ZBool HasAnnexes => EDocPivotCollection != null && EDocPivotCollection.Count > 0;

	public ZBool HasAnnexesSentWithoutResponse() => HasAnnexes
		&& EDocPivotCollection.Cast<CusStorageDocPivot>().Any(x => x.IsSentWithoutResponse);

	ZBool HasAnnexesToSend() => HasAnnexes
		&& EDocPivotCollection.Cast<CusStorageDocPivot>().Any(x => !x.IsAccepted && !x.IsSentWithoutResponse);

	public ZBool HasSendableAnnexesForH1 => RequiresH1Annexes && HasAnnexesToSend();

	public ZBool HasSendableAnnexesForAES => RequiresAESAnnexes && HasAnnexesToSend();

	public ZBool HasSendableAnnexesForT2LPOUS => RequiresT2LPOUSAnnexes && HasAnnexesToSend();

	public ZBool HasMRNAndIssueDate => !MovementReferenceNumber.IsEmpty && !MovementReferenceNumberIssueDate.IsEmpty;

	public ZBool CanRequestEffectiveDepartureCertificate => (Declaration.IsExport &&
		CH_EntryStatus == EntryStatusCodes.EffectiveDeparture &&
		EntryInstruction != null &&
		EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

	ZBool CanSendIQUForGuaranteeWriteOff() => Declaration.IsImport &&
		!MovementReferenceNumber.IsEmpty &&
		!CSVClearance.IsEmpty &&
		EntryInstruction != null &&
		!EntryInstruction.IsH2 &&
		EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ &&
		!IsWaitingForResponse &&
		EntryHasGuaranteeAssociated();

	ZBool EntryHasGuaranteeAssociated()
	{
		var result = false;
		var guaranteesInDeclaration = Declaration.Guarantees.Where(x => !x.PW_BondNumber.IsEmpty && x.EntryInstructionID == EntryInstruction?.PK);

		foreach (var guarantee in guaranteesInDeclaration)
		{
			if (CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, guarantee.PW_BondNumber, CountryCode, EUGuaranteeTypeList.Codes.IMP).Length > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	protected override ZString SadBoxATextCore
	{
		get
		{
			var boxAStringEntryNumber = EntryNumber.Length == 18 ? EntryNumber.InsertSafe(17, " ").InsertSafe(11, " ").InsertSafe(10, " ").InsertSafe(2, " ") : EntryNumber;
			var boxAStringMRN = MovementReferenceNumber.Length == 18 ? MovementReferenceNumber.InsertSafe(17, " ").InsertSafe(11, " ").InsertSafe(10, " ").InsertSafe(2, " ") : MovementReferenceNumber;
			return !boxAStringEntryNumber.IsEmpty ? boxAStringEntryNumber : boxAStringMRN;
		}
	}

	protected override ZBool ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction => ZBool.False;

	protected override bool IsFailedFromTransmissionCore => CH_Status == MessageStatusList.Codes.FailedFromTransmission;

	protected override void AfterSetAsFailedFromTransmission(ZString previousCH_Status)
	{
		var lastT2IEDIMEssage = Messages.GetLastMessage(ZString.Empty, DeclarationMessageTypeList.Codes.T2lReceptionPous, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
		if(lastT2IEDIMEssage != null)
		{
			lastT2IEDIMEssage.EM_Status = MessageStatusList.Codes.FailedFromTransmission;
		}
	}

	ZString IESMessageBusinessObject.EntryReference => CH_BGMReference;

	GlbStaff IESMessageInfoProvider.Broker => Declaration?.CusAgent;
	ZString IESMessageInfoProvider.MRN => MovementReferenceNumber;
	ZString IESMessageInfoProvider.DocumentJobReference => $"{CH_BGMReference} for {Declaration.JobNumber}";

	ZGuid IESResponseBusinessObject.BranchPK => Branch.PK;

	EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

	ZString IESResponseBOMessageStatus.MessageStatus { set => CH_Status = value; }

	ZString IPollingTransactionParent.CertificateName => Declaration?.JE_CustomsProfile ?? ZString.Empty;

	ZBool IPollingTransactionParent.IsTest
	{
		get
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem()
							|| (bool)Declaration?.ZG_IsTrainingDeclaration);
		}
	}

	GlbStaff IPollingTransactionParent.Broker => Declaration?.CusAgent;

	OrgHeader IPollingTransactionParent.Declarant => IsExportUCC6
														? Declaration?.Representative?.Header ?? Declaration?.DeclarantOrgAddress?.Header ?? Declaration?.Supplier
														: Declaration?.Declarant?.Header;

	protected override void ReCalculateStatusDetails()
	{
		if (CH_Status != MessageStatusList.Codes.NotSent && CH_EntrySubmittedDate.IsEmpty)
		{
			CH_EntrySubmittedDate = ZDateTime.Now;
		}
	}

	public override ZString DefaultStatusDescription => ZString.Empty;

	public void ResetCancelledEntry()
	{
		CH_BGMReference = ZString.Empty;
		CH_EntryStatus = ZString.Empty;
		if (MovementReferenceCusEntryNumber != null)
		{
			MovementReferenceNumberIssueDate = ZDateTime.Empty;
			SetMovementReferenceNumberEntryStatus(ZString.Empty);
			MovementReferenceCusEntryNumber.Delete();
		}
		var clearanceCusEntryNumber = LoadCusEntryNumber(CusEntryNumberTypes.Spain.ClearanceCSV);
		clearanceCusEntryNumber?.Delete();
		var circuitCanCusEntryNumber = LoadCusEntryNumber(CusEntryNumberTypes.Spain.CircuitCan);
		circuitCanCusEntryNumber?.Delete();
		ZG_ExportMRN = ZString.Empty;
		CH_EntryReleaseDate = ZDateTime.Empty;
		ZG_LimitPaymentDate = ZDateTime.Empty;
		ZG_ATCLimitPaymentDate = ZDateTime.Empty;
		ZG_CSVImportCertificate = ZString.Empty;
		ZG_PaymentProofNumber = ZString.Empty;
		ZG_ATCPaymentProofNumber = ZString.Empty;
		ZG_DJPMRN = ZString.Empty;
		ZG_RequestDispatch = ZString.Empty;
		ZG_Parallel = false;
		ZG_LimitDateOfArrival = ZDateTime.Empty;
		EUH_EADPrintProcedure = ZString.Empty;
		ZG_CSVT2L = ZString.Empty;
		IndirectExport = false;
		ZG_ClearanceResult = ZString.Empty;

		foreach (var entryLine in MergedLines)
		{
			entryLine.Fees.RemoveAndDeleteAll();

			var documentsToRemove = entryLine.GetPreviouslySentSupportingDocuments();
			foreach (var docu in documentsToRemove)
			{
				docu.Delete();
			}
		}

		foreach (EDIMessage message in Messages)
		{
			message.EM_IsActive = false;
		}

		var mergeManager = (MergeManager)Declaration.MergeManager;
		var merger = (LineMerger)mergeManager.GetNewLineMerger();
		merger.DoMerge();
	}

	public void SetCSVClearance(ZString csvCodeFromUser)
	{
		if (!csvCodeFromUser.Equals(CSVClearance))
		{
			var oldValue = CSVClearance;
			SetCSVClearanceNum(csvCodeFromUser);
			if (!csvCodeFromUser.IsEmpty)
			{
				SetEntryStatusWhenCSVClearanceIsSet();
			}

			SetLogWhenSettingCSVCode((NoResString)"CSV Clearance Code", oldValue, CSVClearanceInfo.Value.ToString());
		}
	}

	void SetEntryStatusWhenCSVClearanceIsSet()
	{
		CH_EntryStatus = (string)EntryInstruction.CEI_SubStyle switch
		{
			EntrySubStyleList.Codes.B or EntrySubStyleList.Codes.C => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
			EntrySubStyleList.Codes.Z => IsImport ? EntryStatusCodes.Cleared : EntryStatusCodes.EffectiveDeparture,
			_ => EntryStatusCodes.Cleared,
		};
	}

	public void SetClearanceDate(ZDateTime clearanceDateFromUser)
	{
		if (!clearanceDateFromUser.Equals(CH_EntryReleaseDate))
		{
			var oldValue = CH_EntryReleaseDate;
			CH_EntryReleaseDate = clearanceDateFromUser;

			SetLogWhenSettingCSVCode((NoResString)"Clearance Date", oldValue.ToCustomsFormatDateStringyyyyMMddTHHmmss(), CH_EntryReleaseDate.ToCustomsFormatDateStringyyyyMMddTHHmmss());
		}
	}

	public void SetCSVImportCertificate(ZString csvImportCertificateCodeFromUser)
	{
		if (!csvImportCertificateCodeFromUser.Equals(ZG_CSVImportCertificate))
		{
			var oldValue = ZG_CSVImportCertificate;
			ZG_CSVImportCertificate = csvImportCertificateCodeFromUser;

			SetLogWhenSettingCSVCode((NoResString)"CSV Import Certificate Code", oldValue, ZG_CSVImportCertificateInfo.Value.ToString());
		}
	}

	public void SetT2LClearance(ZString csvT2LClearanceCodeFromUser)
	{
		if (!csvT2LClearanceCodeFromUser.Equals(ZG_CSVT2L))
		{
			var oldValue = ZG_CSVT2L;
			ZG_CSVT2L = csvT2LClearanceCodeFromUser;

			SetLogWhenSettingCSVCode((NoResString)"CSV T2L Code", oldValue, ZG_CSVT2LInfo.Value.ToString());
		}
	}

	public void SetCSVExitCertificate(ZString csvExitCertificateCodeFromUser)
	{
		if (!csvExitCertificateCodeFromUser.Equals(ZG_CSVExitCertificate))
		{
			var oldValue = ZG_CSVExitCertificate;
			ZG_CSVExitCertificate = csvExitCertificateCodeFromUser;

			SetLogWhenSettingCSVCode((NoResString)"CSV Exit Certificate Code", oldValue, ZG_CSVExitCertificateInfo.Value.ToString());
		}
	}

	void SetLogWhenSettingCSVCode(string logReasonCode, string oldValue, string newValue)
	{
		var logTypeCode = "CSV";
		var logReason = (NoResString)"Manually Added " + logReasonCode + (NoResString)" to Entry " + CH_BGMReference;
		var parameters = new KeyValuePair<string, string>[]
		{
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, oldValue),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, newValue.IsNullOrEmpty() ? (NoResString)"Empty" : newValue),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, logTypeCode),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, logReason)
		};
		Logs.AddNew(ZArchitecture.Business.AutoEvents.ChangeOfIdentifier, parameters);
	}

	public ZDecimal GetTotalAmountToDeclare()
	{
		var amount = ZDecimal.Zero;
		foreach (var line in MergedLines)
		{
			foreach (CusEntryLineFee fee in line.Fees)
			{
				amount += fee.CF_ChargeAmount;
			}
		}
		return amount;
	}

	public string GetUrlToLaunch() => !IsDeleted && EntryInstruction != null ? new UrlDecider(this).GetUrl() : ZString.Empty;

	public ValidationModes ValidationMode
	{
		get
		{
			if (!fValidationMode.HasValue)
			{
				SetDefaultValidationMode();
			}
			return fValidationMode.Value;
		}
		set => fValidationMode = value;
	}
	ValidationModes? fValidationMode;

	public void SetDefaultValidationMode() => fValidationMode = ValidationModes.None;

	public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

	public bool CheckSupportingDocumentsHaveProcedure()
	{
		var entryLineSupDocs = new List<SupportingDocument>();
		MergedLines.ForEach(x => entryLineSupDocs.AddRange(x.SupportingDocuments.Cast<SupportingDocument>()));
		var entryLineDocsHaveProcedure = entryLineSupDocs.Any(doc => !doc.CSI_Procedure.IsEmpty);
		var declarationDocsHaveProcedure = Declaration.SupportingDocuments.Cast<SupportingDocument>().Any(doc => !doc.CSI_Procedure.IsEmpty);
		return entryLineDocsHaveProcedure || declarationDocsHaveProcedure;
	}

	public bool CheckPreviousDocumentsExist()
	{
		return !MergedLines.Any(x => x.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly() == null);
	}

	public bool CheckAnyEntryLineHasSupportingDocumentsToSend() => MergedLines.Any(x => x.HasSupportingDocumentsToSend());

	public bool AreAllDescriptionLengthCorrectForImport()
	{
		const int GoodsDescriptionMinLengthForImport = 5;

		return MergedLines.All(x => x.RandomLine.JI_Description.Length >= GoodsDescriptionMinLengthForImport);
	}

	public bool HasNoInvoiceUndeclared()
	{
		var entryLines = MergedLines.ToList();

		var entryLineCLComplExportDocumentList = new List<SupportingDocument>();
		entryLines.ForEach(entryLine => entryLineCLComplExportDocumentList.AddRange(entryLine.GetPreviouslySentComplXExportAcceptedDocumentsEntryLine()));

		var readOnlyComplExportDocumentList = new List<ReadOnlySupportingDocument>();
		entryLines.ForEach(entryLine => readOnlyComplExportDocumentList.AddRange(entryLine.GetReadOnlyComplXExportAcceptedDocuments()));

		return readOnlyComplExportDocumentList.All(doc => (doc.GetSupportingDocument() as SupportingDocument).MatchesAnyPreviouslySentDocument(entryLineCLComplExportDocumentList.ToArray()));
	}

	public bool IsContainerised() => MergedLines.Any(x => x.Containers.Any());

	public bool Is9015SupportingDocumentNeeded()
	{
		var hasCorrectEntryInstruction = EntryInstruction != null && EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ;
		var directRepresentationTypes = new ZString[] { ESRepresentationTypeList.Codes._2Direct, ESRepresentationTypeList.Codes._5IndirectATC, EU.Business.RepresentationTypeList.Codes._2Direct };
		var hasDirectRepresentation = directRepresentationTypes.Contains(Declaration.JE_DeclarantType);
		var hasCorrectPaymentMethod = (((CusEntryLine)RandomEntryLine)?.RandomLine?.ZG_MethodOfPayment ?? ZString.Empty) == MethodOfPaymentList.Codes.R;

		return IsImport && hasCorrectEntryInstruction && hasDirectRepresentation && hasCorrectPaymentMethod && CheckAuthorisationAndGuaranteeForDeclarantExist();

		bool CheckAuthorisationAndGuaranteeForDeclarantExist()
		{
			var declarantPK = Declaration.Declarant?.OA_OH ?? ZGuid.Empty;
			var authorisationForDeclarant = Declaration.LoadAEOCusGuaranteeHeaderFromReference(declarantPK);

			return authorisationForDeclarant != null && CheckGuaranteeForDeclarantExists(declarantPK);
		}

		bool CheckGuaranteeForDeclarantExists(ZGuid holder_OH_PK)
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder_OH_PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, EUGuaranteeTypeList.Codes.IMP);
			var guaranteeHeadersNumberList = Declaration.Factory.Load<CusGuaranteeHeader>(query).Select(x => x.CPH_Number).ToArray();

			return guaranteeHeadersNumberList.Length > 0 && Declaration.Guarantees.Cast<ESGuarantee>().Any(x => guaranteeHeadersNumberList.Contains(x.PW_BondNumber) && x.EntryInstructionID == EntryInstruction.PK);
		}
	}

	public bool EntryHas9015SupportingDocuments()
	{
		var docType = SupportingDocumentType.VATReductionCode;

		return MergedLines.Any(line => line.SupportingDocuments.Any(doc => doc.CSI_Code == docType)
									|| line.Header.SupportingDocuments.Any(doc => doc.CSI_Code == docType));
	}

	public void Remove9015Documents()
	{
		RemoveSupportingDocuments(SupportingDocumentType.VATReductionCode, removeFromEntryInstruction: false);
	}

	public void RemoveSupportingDocuments(string docType, bool removeFromDeclaration = true, bool removeFromInvoices = true, bool removeFromInvoiceLines = true, bool removeFromEntryInstruction = true)
	{
		if (removeFromDeclaration)
		{
			RemoveFromCollection(Declaration.SupportingDocuments);
		}

		if (removeFromInvoices)
		{
			InvoiceHeaders.ForEach(invoice => RemoveFromCollection(invoice.SupportingDocuments));
		}

		if (removeFromInvoiceLines)
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invoiceLine => RemoveFromCollection(invoiceLine.SupportingDocuments));
		}

		if (removeFromEntryInstruction && EntryInstruction is not null)
		{
			RemoveFromCollection(EntryInstruction.SupportingDocuments);
		}

		void RemoveFromCollection(EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection collection)
		{
			var documentsToRemove = collection.Find(x => x.CSI_Code == docType).ToList();
			documentsToRemove.ForEach(collection.RemoveAndDelete);
		}
	}

	public SupportingDocument[] GetPreviouslySentSupportingDocuments()
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, TablePrefix);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.SupportingDocument);
		return (SupportingDocument[])Factory.Load(typeof(SupportingDocument), query);
	}

	public AdditionalInfo[] GetPreviouslySentAdditionalInfos()
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, TablePrefix);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.AdditionalInfo);
		return (AdditionalInfo[])Factory.Load(typeof(AdditionalInfo), query);
	}

	public ZString[] GetInboxRequestMessageTypesForAES()
	{
		var messageTypes = new List<ZString>();

		var isPCO = CH_EntryStatus == EntryStatusCodes.PendingForEuOffice;
		var isCLR = CH_EntryStatus == EntryStatusCodes.Cleared;

		var isPCOWithoutClearanceOrCCO = (isPCO && CSVClearance.IsEmpty) || CH_EntryStatus == EntryStatusCodes.ControlsAtEuOffice;
		if (CH_EntryStatus == EntryStatusCodes.CustomsDeclarationAccepted || isPCOWithoutClearanceOrCCO)
		{
			messageTypes.Add(DeclarationMessageTypeList.Codes.ExportClearanceCommunication);
			messageTypes.Add(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication);

			if (isPCOWithoutClearanceOrCCO)
			{
				messageTypes.Add(DeclarationMessageTypeList.Codes.ExportCceControlCommunication);
			}
		}
		else if (CH_EntryStatus == EntryStatusCodes.PreDeclarationAccepted || isCLR || (isPCO && !CSVClearance.IsEmpty))
		{
			messageTypes.Add(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication);

			if (isCLR)
			{
				messageTypes.Add(DeclarationMessageTypeList.Codes.ExportExitResultCommunication);
			}
		}

		return messageTypes.ToArray();
	}

	public IQUMessageInfo SendIQUForGuaranteeWriteOffIfPossible()
	{
		var result = new IQUMessageInfo()
		{
			MessageCanBeSent = false
		};

		if (CanSendIQUForGuaranteeWriteOff())
		{
			result.MessageCanBeSent = true;
			result.DeclarationWithBrokerError = Declaration.JE_DeclarationReference;
			var broker = Declaration.CusAgent;
			if (broker != null && !Declaration.JE_CustomsProfileInfo.HasMessageErrors())
			{
				var certificate = CertificateHelper.GetCertificate(broker, Declaration.JE_CustomsProfile);
				if (certificate != null)
				{
					result.DeclarationWithBrokerError = ZString.Empty;
					TrySendIQUMessageForGuaranteeWriteOff(broker, certificate, result);
				}
			}
		}
		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:ErrorReporterKey", Justification = "Baseline")]
	void TrySendIQUMessageForGuaranteeWriteOff(GlbStaff broker, GlbExternalPassword certificate, IQUMessageInfo messageInfo)
	{
		var messages = new List<ESEDIMessage>();
		try
		{
			var certificateObject = new CertificateObject(broker, certificate.GP_Name, certificate.GP_UserID);

			var builderManager = new ESMessageBuilderManager(DeclarationMessageTypeList.Codes.ImportQuery, DeclarationMessageSubTypeList.Codes.Guarantee, this, certificateObject);

			var messageBuildersData = new List<ESMessageSender.MessageBuilderData>();
			if (Declaration.DestinationStateIsCanaryIsland)
			{
				messageBuildersData.Add(ESMessageSender.GetImportQueryMessageBuilderForATC(this, builderManager));
			}
			else
			{
				messageBuildersData.Add(ESMessageSender.GetImportQueryMessageBuilderForAEAT(this, builderManager));
			}
			ESMessageSender.Send(messageBuildersData, messages);

			if (messages.Count > 0)
			{
				CH_Status = MessageStatusList.Codes.AwaitingResponse;
				messageInfo.MessageSentCorrectly = true;
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ErrorReporter.ReportOnce("CusGuaranteeHeader.TrySendIQUMessageForGuaranteeWriteOff", Res.GetString("B8308C44-9472-42D5-AE4F-21EEB7D4BC44", "Error when creating the Import Query message in entry") + " " + CH_BGMReference);
		}
	}

	protected override ZString PreviousStatus => (ZString)CH_EntryStatusInfo.OriginalValue;
	protected override ZString CurrentStatus => CH_EntryStatus;

	protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
	{
		var statusCodesForAutoBillingFromRegistry = ListOfStatusCodesForAutoBilling;

		bool result;
		if (statusCodesForAutoBillingFromRegistry.Count > 0)
		{
			result = !statusCodesForAutoBillingFromRegistry.Contains(originalStatus) && statusCodesForAutoBillingFromRegistry.Contains(newStatus);
		}
		else
		{
			var now = ZDateTime.Now;
			var dataGroupingCode = Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;
			result = !Customs.Universal.CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, originalStatus, dataGroupingCode, now)
				&& Customs.Universal.CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, newStatus, dataGroupingCode, now);
		}

		return result;
	}

	HashSet<ZString> ListOfStatusCodesForAutoBilling => Factory.GetValue(ref listOfStatusCodesForAutoBillingCached,
		getValueDelegate: () => GetListOfStatusCodesForAutoBillingFromRegistry(Declaration));
	CachedProperty<HashSet<ZString>> listOfStatusCodesForAutoBillingCached;

	static HashSet<ZString> GetListOfStatusCodesForAutoBillingFromRegistry(JobDeclaration declaration)
	{
		var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration?.Branch.GB_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
		return options.EUCustomsStatusCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).Distinct().ToHashSet();
	}

	public class IQUMessageInfo
	{
		public ZBool MessageSentCorrectly;
		public ZBool MessageCanBeSent;
		public ZString DeclarationWithBrokerError;
	}

	protected override void OnFactorySaved(bool saveSucceeded)
	{
		base.OnFactorySaved(saveSucceeded);
		if (saveSucceeded)
		{
			foreach (var docManagerInfo in eDocsManagersQueuedForSaving)
			{
				docManagerInfo.Save();
			}
		}
	}

	void IEDocsDelayedSaver.QueueForSaving(DocManagerInfo docManagerInfo)
	{
		eDocsManagersQueuedForSaving.Add(docManagerInfo);
	}

	readonly List<DocManagerInfo> eDocsManagersQueuedForSaving = new List<DocManagerInfo>();

	protected override ZString SupplierEoriOfMainOfficeCore => OrgHeaderExtension.GetIDCode(Supplier.Organisation);

	protected override ZString ImporterEoriOfMainOfficeCore => OrgHeaderExtension.GetIDCode(Importer.Organisation);

	protected override ZString RepresentativeOrDeclarantEoriOfMainOfficeCore => OrgHeaderExtension.GetIDCode(DeclarantOrganisation);

	protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore()
	{
		return IsExportNonEXSOrT2LOrT2C ? GetEntryLineDataDeclaredToReserveTSGoodsExport() : GetEntryLineDataDeclaredToReserveTSGoodsNotExport();
	}

	public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsNotExport()
	{
		var resultList = new List<DeclarationDataToReserveTSGoods>();

		var docAndLineList = new List<(PreviousDocument doc, CusEntryLine line)>();
		foreach (var line in MergedLines)
		{
			var docs = line.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
			if (docs != null && docs.Count > 0)
			{
				docAndLineList.Add((docs[0], line));
			}
		}

		var distinctDocAndLineListByDoc = docAndLineList.GroupBy(x => new { x.doc.CSI_ReferenceNumber, x.doc.CSI_LineNo });

		foreach (var distinctDocAndLine in distinctDocAndLineListByDoc)
		{
			var distinctDocAndLineArray = distinctDocAndLine.ToArray();
			var docToReturn = distinctDocAndLineArray.First().doc;
			var grossWeight = ZDecimal.Zero;
			var grossWeightForVINs = ZDecimal.Zero;
			var packagesAll = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

			foreach (var (_, line) in distinctDocAndLineArray)
			{
				var lineEffectiveGrossWeight = line.EffectiveGrossWeight.InKilogramsSafe;
				grossWeight += lineEffectiveGrossWeight;
				grossWeightForVINs += line.InvoiceLinesWithVehicles.Any() ? lineEffectiveGrossWeight : 0;
				packagesAll.AddRange(GetPackages(line, null));
			}

			resultList.Add(GetDeclarationDataToReserveTSGoodsForDoc(packagesAll, grossWeight, grossWeightForVINs, docToReturn));
		}

		return (resultList, ZString.Empty);
	}

	public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsExport()
	{
		var resultList = new List<DeclarationDataToReserveTSGoods>();

		if (ThereIsAtLeastOneDocumentInList(Declaration.SupportingDocuments.Cast<SupportingDocument>())
			|| ThereIsAtLeastOneDocumentInList(EntryInstruction.SupportingDocuments.Cast<SupportingDocument>())
			|| InvoiceHeaders.Any(x => ThereIsAtLeastOneDocumentInList(x.SupportingDocuments.Cast<SupportingDocument>())))
		{
			return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("68CB20E5-8263-42B7-9695-E9F03218DB86", "{0}: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.", CH_BGMReference));
		}

		var docAndLineList = new List<(SupportingDocument doc, JobComInvoiceLine line)>();
		var invoiceLines = MergedLines.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>());
		foreach (var line in invoiceLines)
		{
			var filterData = line.SupportingDocuments.Cast<SupportingDocument>()
				.Where(x => x.CSI_Code == SupportingDocumentHelper.SupportingDocumentCode1217);
			var docs = IEnumerableExtensions.DistinctBy(filterData, x => x.CSI_ReferenceNumber);
			if (!docs.IsNullOrEmpty())
			{
				var docCount = docs.Count();
				if (docCount > 1
					&& docs.Any(x => x.CSI_Quantity.IsEmpty
										|| x.CSI_UnitOfQuantity != Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram))
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("66FE0D29-DF9D-4090-A8BB-F77963ECD46C", "{0} / Invoice {1} / {2}: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.", CH_BGMReference, line.InvoiceHeader.JZ_InvoiceNumber, line.JI_LineNo));
				}

				var packTypeDistinctList = line.PackagesPivot.Select(p => p.Package.CW_PackType).Distinct();
				if (packTypeDistinctList.Count() > 1)
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("56BB8C8D-1A66-4216-96CA-78EC24E6A956", "{0}: For managed LAME storages, only one package type per Invoice Line must be entered. If more than one, please, create as many Invoice Lines as package types so the inventory can be managed properly. Please, correct data and send again.", CH_BGMReference));
				}

				var vehiclesCount = line.Vehicles.Count;
				var packType = packTypeDistinctList.FirstOrDefault();
				if (docCount > 1
					&& vehiclesCount == 0
					&& !PackageHelper.PackTypeIsBulk(packType, Factory)
					&& packType != PackageType.Frame
					&& docs.Any(x => x.CSI_PackQty == ZDecimal.Zero
							|| (packType != x.CSI_PackType)))
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("F84C3E9D-2067-4866-8309-A6190702C7BD", "{0} / Invoice {1} / {2}: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Pack Quantity for each certificate in column Pack Qty, setting Pack Type to the corresponding Type of packages, so the inventory can be managed properly. Please, set that data and send again.", CH_BGMReference, line.InvoiceHeader.JZ_InvoiceNumber, line.JI_LineNo));
				}

				var sumCSI_QuantityForAll1217 = docs.Sum(q => q.CSI_Quantity);
				if (docCount > 1
					&& sumCSI_QuantityForAll1217 > line.GrossWeightInKG
					&& docs.Any(x => !x.CSI_Quantity.IsEmpty && x.CSI_UnitOfQuantity == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram))
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("512A8A6B-68D1-49F1-BE80-FB02A608F29B", "{0} / Invoice {1} / {2}: The SUM of Gross Weight declared in the LAME Certificates ({3}) is greater than the Gross Weight declared in the Invoice Line ({4}). Please, correct those values and send again.", CH_BGMReference, line.InvoiceHeader.JZ_InvoiceNumber, line.JI_LineNo, sumCSI_QuantityForAll1217, line.GrossWeightInKG));
				}

				var sumCSI_PackQtyForAll1217 = docs.Sum(q => q.CSI_PackQty);
				var isFrame = docs.Any(q => q.CSI_PackType == PackageType.Frame);
				var sumPackagesCHC_NumberOfPacks = line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Sum(p => p.CHC_NumberOfPacks);
				if (docCount > 1 &&
					(!isFrame && sumCSI_PackQtyForAll1217 > sumPackagesCHC_NumberOfPacks && docs.Any(x => x.CSI_PackQty > ZDecimal.Zero)) ||
					(isFrame && sumCSI_PackQtyForAll1217 > vehiclesCount))
				{
					return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), Res.GetString("34B9DAD9-3352-43FF-BC4D-1B416FC29656", "{0} / Invoice {1} / {2}: The SUM of Packages declared in the LAME Certificates ({3}) is greater than the number of packages declared in the Invoice Line ({4}). Please, correct those values and send again.", CH_BGMReference, line.InvoiceHeader.JZ_InvoiceNumber, line.JI_LineNo, sumCSI_PackQtyForAll1217, isFrame ? vehiclesCount : sumPackagesCHC_NumberOfPacks));
				}

				docs.ForEach(x => docAndLineList.Add((x, line)));
			}
		}

		var distinctDocAndLineListByDoc = docAndLineList.GroupBy(x => x.doc.CSI_ReferenceNumber);

		foreach (var distinctDocAndLine in distinctDocAndLineListByDoc)
		{
			var distinctDocAndLineArray = distinctDocAndLine.ToArray();
			var docToReturn = distinctDocAndLineArray.First().doc;
			var grossWeight = ZDecimal.Zero;
			var grossWeightForVINs = ZDecimal.Zero;
			var packagesAll = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

			foreach (var (doc, line) in distinctDocAndLineArray)
			{
				var docQuantity = doc.CSI_Quantity;
				var quantityGrossWeight = docQuantity.IsEmpty ? line.EffectiveGrossWeight.InKilogramsSafe : docQuantity;
				grossWeight += quantityGrossWeight;
				grossWeightForVINs += line.Vehicles.Cast<CusVehicle>().Any() ? quantityGrossWeight : 0;
				packagesAll.AddRange(GetPackages(null, line));
			}

			resultList.Add(GetDeclarationDataToReserveTSGoodsForDoc(packagesAll, grossWeight, grossWeightForVINs, docToReturn));
		}

		return (resultList, ZString.Empty);

		ZBool ThereIsAtLeastOneDocumentInList(IEnumerable<SupportingDocument> list) => list.Any(x => x.CSI_Code == SupportingDocumentHelper.SupportingDocumentCode1217);
	}

	IEnumerable<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> GetPackages(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
	{
		var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

		if (entryLine != null)
		{
			entryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => packages.Add((PackageType.Frame, 1, vehicle.CVH_VehicleIdentificationNumber, false))));
		}
		else
		{
			invoiceLine.Vehicles.Cast<CusVehicle>().ForEach(vehicle => packages.Add((PackageType.Frame, 1, vehicle.CVH_VehicleIdentificationNumber, false)));
		}

		var packagesList = entryLine != null ? entryLine.PackagingDetails : invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>();

		var packagingDetails = packagesList.Where(p => p.Package.CW_PackType != PackageType.Frame).GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
		foreach (var pack in packagingDetails)
		{
			var packType = pack.Key.CW_PackType;
			var packqty = ZInt.Zero;
			var piecesqty = ZInt.Zero;
			foreach (var p in pack.ToArray())
			{
				packqty += p.CHC_NumberOfPacks;
			}
			packages.Add((packType, packqty, pack.Key.CW_MarksAndNos, PackageHelper.PackTypeIsBulk(packType, Factory)));
		}
		return packages;
	}

	DeclarationDataToReserveTSGoods GetDeclarationDataToReserveTSGoodsForDoc(List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> packagesAll, ZDecimal grossWeight, ZDecimal grossWeightForVINs, CusSupportingInfo docToReturn)
	{
		var packagesGroupBy = packagesAll.GroupBy(p => new { p.type, p.marksOrVin });

		var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();
		foreach (var pack in packagesGroupBy)
		{
			packages.Add((pack.Key.type, pack.ToArray().Sum(p => p.qty), pack.Key.marksOrVin, pack.First().isBulk));
		}

		return new DeclarationDataToReserveTSGoods()
		{
			Document = docToReturn,
			TotalGrossWeight = grossWeight,
			TotalGrossWeightForVINs = grossWeightForVINs,
			Packages = packages
		};
	}

	ZBool IsImportNonH2NonT2LOrT2C => IsImport && !IsH2Style && !IsT2L && !IsT2C;
	ZBool IsImportH2 => IsImport && IsH2Style && !IsT2L && !IsT2C;
	ZBool IsExportNonEXSOrT2LOrT2C => IsExport && !IsExsSubStyle && !IsT2L && !IsT2C;

	ZString TSTransactionInternalRefType
	{
		get
		{
			if (IsImportNonH2NonT2LOrT2C)
			{
				return CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
			}
			else if (IsExsSubStyle)
			{
				return CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration;
			}
			else if (IsImportH2)
			{
				return CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
			}
			else if (IsExportNonEXSOrT2LOrT2C)
			{
				return CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration;
			}
			else
			{
				return ZString.Empty;
			}
		}
	}

	ZString TSTransactionCommentPrefix => IsImportNonH2NonT2LOrT2C || IsImportH2 ? TSTransactionCommentPrefixForImport : IsExport ? TSTransactionCommentPrefixForExport : ZString.Empty;

	protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => IsImportNonH2NonT2LOrT2C || IsExsSubStyle || IsImportH2 || IsExportNonEXSOrT2LOrT2C;

	protected override ZBool IsLAMETemporaryStorage => IsExportNonEXSOrT2LOrT2C;

	protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => CH_BGMReference;

	protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => TSTransactionInternalRefType;

	protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions
	{
		get
		{
			if (IsImportNonH2NonT2LOrT2C || IsImportH2)
			{
				return [EntryStatusCodes.PreDeclarationAccepted];
			}
			else if (IsExportNonEXSOrT2LOrT2C)
			{
				return [EntryStatusCodes.PreDeclarationAccepted, EntryStatusCodes.GoodsStoppedAtDeparture, EntryStatusCodes.Invalidated, EntryStatusCodes.Cancelled];
			}
			else
			{
				return Array.Empty<ZString>();
			}
		}
	}

	protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions
	{
		get
		{
			if (IsImportNonH2NonT2LOrT2C || IsImportH2)
			{
				return [EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, EntryStatusCodes.Cleared];
			}
			else if (IsExsSubStyle)
			{
				return [EntryStatusCodes.Cleared];
			}
			else if (IsExportNonEXSOrT2LOrT2C)
			{
				return [EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, EntryStatusCodes.Cleared, EntryStatusCodes.EffectiveDeparture];
			}
			else
			{
				return Array.Empty<ZString>();
			}
		}
	}

	protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions
	{
		get
		{
			if (IsImportNonH2NonT2LOrT2C || IsImportH2 || IsExportNonEXSOrT2LOrT2C)
			{
				return [EntryStatusCodes.PreDeclarationAccepted, ZString.Empty];
			}
			else if (IsExsSubStyle)
			{
				return [ZString.Empty];
			}
			else
			{
				return Array.Empty<ZString>();
			}
		}
	}

	protected override ZString TemporaryStorageTransactionCommentPrefix => TSTransactionCommentPrefix;

	protected override ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => " / " + TSTransactionInternalRefType;

	protected override IReadOnlyList<ZString> PreviousDocumentCodeForDataToReserveTemporaryStorageGoods
	{
		get
		{
			if (IsImportNonH2NonT2LOrT2C)
			{
				return [PreviousDocumentHelper.PreviousDocumentCodeSUM];
			}
			else if (IsExsSubStyle)
			{
				return [PreviousDocumentHelper.PreviousDocumentCodeSUM, PreviousDocumentHelper.PreviousDocumentCodeXSUM, PreviousDocumentHelper.PreviousDocumentCodeN337];
			}
			else if (IsImportH2)
			{
				return [PreviousDocumentHelper.PreviousDocumentCode337];
			}
			else if (IsExportNonEXSOrT2LOrT2C)
			{
				return [SupportingDocumentHelper.SupportingDocumentCode1217];
			}
			else
			{
				return Array.Empty<ZString>();
			}
		}
	}

	protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => true;

	protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => DocumentHelper.GetDsdtMRNNumberFormat(dsdtMRN);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string TSTransactionCommentPrefixForImport = "IMPORT JOB:";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string TSTransactionCommentPrefixForExport = "EXPORT JOB:";

	protected override bool IsMrnEntryNumberTheOneWeWantToShow
	{
		get
		{
			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(Declaration?.Branch?.GB_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
			return options.EnableAccountingIntegration;
		}
	}

	public ZString GetMappedSubTypeForExportPreDeclaration()
	{
		var entrySubStyle = EntryInstruction.CEI_SubStyle;

		if (SubStyleForExportPreDeclaration.TryGetValue(entrySubStyle, out var value))
		{
			return value;
		}

		return entrySubStyle;
	}

	Dictionary<string, string> SubStyleForExportPreDeclaration => new Dictionary<string, string>
	{
		{ EntrySubStyleList.Codes.A, MappedSubStyleCodeD },
		{ EntrySubStyleList.Codes.B, MappedSubStyleCodeE },
		{ EntrySubStyleList.Codes.C, MappedSubStyleCodeF },
	};

	const string MappedSubStyleCodeD = "D";
	const string MappedSubStyleCodeE = "E";
	const string MappedSubStyleCodeF = "F";
}
