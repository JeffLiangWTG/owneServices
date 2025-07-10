using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), "CustomsEntryHeaders")]
	public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader
		, Integration.Customs.GB.ICusEntryHeader
		, ICanBeImportOrExport
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter
		, IMessageAttachee
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
		{
			public const string LRN = "LRN";
			public const int LRNMaxLength = GBCustomsDataRegistry.CDSEntryLocalReferenceNumberMaxLength;
			public const string IrcInventoryReturnCode = "CH_IrcInventoryReturnCode";
			public const string RouteOfEntry = "CH_RouteOfEntry";
			public const string ImportClearanceStatusICS = "CH_ImportClearanceStatusICS";
			public const string StyleOfEntrySOE = "CH_StyleOfEntrySOE";
			public const string IsCancelledWithCustoms = "IsCancelledWithCustoms";
			public const string CH_MasterUCR = "CH_MasterUCR";
			public const string CH_ExitActualOffice = "CH_ExitActualOffice";
			public const string CH_ExitDate = "CH_ExitDate";
		}

		public new CusEntryInstruction EntryInstruction => base.EntryInstruction as CusEntryInstruction;

		public new JobComInvoiceHeader RandomHeader
		{
			get { return base.RandomHeader as JobComInvoiceHeader; }
		}

		protected override bool SupportsPreviousDocumentsAtEntryHeaderLevelCore => Declaration?.IsUCCCompliant ?? false;

		protected override ZString[] GetMethodsOfPaymentThatCanInfluenceAutoRatingCore()
		{
			//] { "F", "D", "P", "Q"};
			var allCodes = Factory.GetCachedValue("EU.CusEntryHeader.GetMethodsOfPaymentThatCanInfluenceAutoRatingCore", delegate
			{
				var result = Factory.GetCachedListMatchAllAttributes(
						Declaration.GetDefaultDataGroupingCode(),
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment,
						new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, ZString.Empty) }
				);
				return result.GetAllCodesZString();
			});
			return (allCodes.Length == 0) // RefZZ data is buggered
					? base.GetMethodsOfPaymentThatCanInfluenceAutoRatingCore() // e.g. new ZString[] {""}
					: allCodes;
		}

		protected override IEnumerable<ZString> GetRateCodesCore(ZString rateType)
		{
			var codes = base.GetRateCodesCore(rateType).ToList();
			if (rateType.EqualsIgnoringCase(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat))
			{
				codes.Add(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
				codes.Add(EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland);
				codes.Add(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat);
			}
			return codes;
		}

		protected override bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivot => false;

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new ICusEntryLineCollection<CusEntryLine> MergedLines
		{
			get { return (ICusEntryLineCollection<CusEntryLine>)base.MergedLines; }
		}

		public new GBAddInfoCusEntryHeaderLookups AddInfoLookups => (GBAddInfoCusEntryHeaderLookups)base.AddInfoLookups;

		[ChildEditable(true)]
		public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

		protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

		protected override EU.Business.Declaration.AddInfoCusEntryHeader GetNewAddInfoCusEntryHeader() => new AddInfoCusEntryHeader(this.CH_AddInfoInfo);

		[ChildEditable(true)]
		public new GbEDIMessageCollection Messages => (GbEDIMessageCollection)base.Messages;

		protected override EDIMessageCollection GetNewMessageCollection() => new GbEDIMessageCollection(this);

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment => true;

		protected override ZString EntryNumberType => Declaration?.ApplicationExtender?.GetEntryNumberType(Declaration) ?? JobMessageTypeList.Codes.Import;

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			var result = false;
			var statusCodesForAutoBillingFromRegistry = Declaration?.ApplicationExtender.GetListOfStatusCodesForAutoBilling(Declaration);

			if (statusCodesForAutoBillingFromRegistry.Count > 0)
			{
				result = !statusCodesForAutoBillingFromRegistry.Contains(originalStatus) && statusCodesForAutoBillingFromRegistry.Contains(newStatus);
			}
			else
			{
				var dataGroupingCode = Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;
				result = (!CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, originalStatus, dataGroupingCode, ZDateTime.Now)
					&& CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, newStatus, dataGroupingCode, ZDateTime.Now));
			}

			return result;
		}

		protected override ZString PreviousStatus => Declaration?.ApplicationExtender?.GetOldEntryStatus(this) ?? ZString.Empty;
		protected override ZString CurrentStatus => Declaration?.ApplicationExtender?.GetEntryStatus(this) ?? ZString.Empty;

		protected override ZString SupplierEoriOfMainOfficeCore => Declaration?.JE_ApplicationCode.ToString() == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
			? Supplier?.GetEuIdentificationNumberForCDS() ?? ZString.Empty
			: base.SupplierEoriOfMainOfficeCore;

		protected override ZString ImporterEoriOfMainOfficeCore => Declaration?.JE_ApplicationCode.ToString() == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
			? Importer?.GetEuIdentificationNumberForCDS() ?? ZString.Empty
			: base.SupplierEoriOfMainOfficeCore;

		[List(nameof(Declaration) + "." + nameof(CusEntryHeader.Declaration.AddInfoLookups) + "." + nameof(JobDeclarationLookups.ImportClearanceStatusICSList))]
		public ZString CH_ImportClearanceStatusICS
		{
			get { return AddInfo.ZG_ImportClearanceStatusICS; }
			set
			{
				AddInfo.ZG_ImportClearanceStatusICS = value;
				if (Declaration != null && Declaration.ICSMeansClear(value) && IsImport && GBCustomsDataRegistry.Instance.ChiefUpdateJobsToClearWhenIcsCodeMeansClear.Value)
				{
					CH_EntryStatus = EntryStatusList.Codes.Clear;  // ="CLR";
				}
			}
		}
		public ZPropertyInfo CH_ImportClearanceStatusICSInfo => GetWrappedZPropertyInfo(Schema.ImportClearanceStatusICS, x => AddInfo.ZG_ImportClearanceStatusICSInfo);

		public ZString IcsStatusOriginalValue => (ZString)AddInfo.ZG_ImportClearanceStatusICSInfo.OriginalValue;

		[ResourceStringData("JobDeclaration.JE_GBIrcInventoryReturnCode", ShortCaption = "IRC", MediumCaption = "Inventory Match", Caption = "Inventory Return Code", FullDescription = "Inventory Return Code (IRC) for last E0 report processed.")]
		[List(nameof(Declaration) + "." + nameof(CusEntryHeader.Declaration.GbInventoryReturnCodeIRCList))]
		[ReadOnly(true)]
		public ZString CH_IrcInventoryReturnCode
		{
			get { return AddInfo.ZG_IrcInventoryReturnCode; }
			set { AddInfo.ZG_IrcInventoryReturnCode = value; }
		}

		public ZPropertyInfo CH_IrcInventoryReturnCodeInfo => GetWrappedZPropertyInfo(Schema.IrcInventoryReturnCode, x => AddInfo.ZG_IrcInventoryReturnCodeInfo);

		[List(nameof(Declaration) + "." + nameof(CusEntryHeader.Declaration.AddInfoLookups) + "." + nameof(JobDeclarationLookups.StyleOfEntrySOEList))]
		public ZString CH_StyleOfEntrySOE
		{
			get { return AddInfo.ZG_StyleOfEntrySOE; }
			set { AddInfo.ZG_StyleOfEntrySOE = value; }
		}
		public ZPropertyInfo CH_StyleOfEntrySOEInfo => GetWrappedZPropertyInfo(Schema.StyleOfEntrySOE, x => AddInfo.ZG_StyleOfEntrySOEInfo);

		public ZString CH_RouteOfEntry
		{
			get { return AddInfo.ZG_RouteOfEntry; }
			set { AddInfo.ZG_RouteOfEntry = value; }
		}

		public ZPropertyInfo CH_RouteOfEntryInfo => GetWrappedZPropertyInfo(Schema.RouteOfEntry, x => AddInfo.ZG_RouteOfEntryInfo);

		[ReadOnly(true)]
		[MaxLength(Schema.LRNMaxLength)]
		public ZString LRN
		{
			get { return LRNEntryNumber.CE_EntryNum; }
			set { LRNEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo LRNInfo { get { return GetWrappedZPropertyInfo(Schema.LRN, x => LRNEntryNumber.CE_EntryNumInfo); } }

		CusEntryNumber LRNEntryNumber => Factory.GetValue(ref lrnEntryNumber, () =>
		{
			var lrnEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode);
			if (lrnEntryNumberInternal == null)
			{
				lrnEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode);
				lrnEntryNumberInternal.CE_EntryIsSystemGenerated = true;
				lrnEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			}
			return lrnEntryNumberInternal;
		});
		CachedProperty<CusEntryNumber> lrnEntryNumber;

		public const string LRNReferencePlaceHolderXmlFriendly = "LRNREFERENCEPLACEHOLDERF6AB79A7845D49FA82919554DED4F991";

		public static ZBool IsUniqueEntryLocalReferenceNumber(BusinessObjectFactory factory, ZString lrn)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_EntryNum, lrn);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			return !factory.ExistsInDatabase(CusEntryNumber.Schema.TableName, query);
		}

		public new ZDecimal Duty
		{
			get
			{
				var dutyCodeSet = DutyCodeSet;
				return SumLineConfirmedFees(fee => dutyCodeSet.Contains(fee.CF_ChargeType));
			}
		}

		public new ZDecimal VAT
		{
			get
			{
				var taxCodeSet = TaxCodeSet;
				return SumLineConfirmedFees(fee => taxCodeSet.Contains(fee.CF_ChargeType));
			}
		}

		public new ZDecimal DutyImmediate
		{
			get
			{
				var dutyCodeSet = DutyCodeSet;
				return SumLineConfirmedFees(fee => dutyCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal DutyDeferred
		{
			get
			{
				var dutyCodeSet = DutyCodeSet;
				return SumLineConfirmedFees(fee => dutyCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal VATImmediate
		{
			get
			{
				var taxCodeSet = TaxCodeSet;
				return SumLineConfirmedFees(fee => taxCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal VATDeferred
		{
			get
			{
				var taxCodeSet = TaxCodeSet;
				return SumLineConfirmedFees(fee => taxCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal AllOtherFeesImmediate
		{
			get
			{
				var excludedFeeCodesForAllOtherFees = ExcludedFeeCodesForAllOtherFees;
				return SumLineConfirmedFees(fee => !excludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal AllOtherFeesDeferred
		{
			get
			{
				var excludedFeeCodesForAllOtherFees = ExcludedFeeCodesForAllOtherFees;
				return SumLineConfirmedFees(fee => !excludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));
			}
		}

		public new ZDecimal AllOtherFees
		{
			get
			{
				var excludedFeeCodesForAllOtherFees = ExcludedFeeCodesForAllOtherFees;
				return SumLineConfirmedFees(fee => !excludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType));
			}
		}

		public new ZDecimal DutyTotalUnion
		{
			get
			{
				var dutyTotalUnionList = DutyTotalUnionList();
				return SumLineConfirmedFees(fee => dutyTotalUnionList.Contains(fee.CF_ChargeType));
			}
		}

		public ZDateTime DateOfLegalAcceptance => DateTimeFromEarliestCustomsEntryStatusEvent(ThreeCharFunctionCode.Codes.ACC);

		public ZDateTime ClearanceDateFromCLE => DateTimeFromEarliestCustomsEntryStatusEvent(ThreeCharFunctionCode.Codes.CLE);

		ZDateTime DateTimeFromEarliestCustomsEntryStatusEvent(string requiredReference)
		{
			var result = ZDateTime.Empty;
			var zQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
			zQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomsEntryStatusCode);
			zQuery.AddToFilter(StmALogSchema.SL_Reference, requiredReference);
			zQuery.OrderBy = AutoStmALog.Schema.SL_EventTime;
			var stmALog = Factory.LoadTop1<StmALog>(zQuery);
			if (stmALog != null)
			{
				result = stmALog.SL_EventTime;
			}

			return result;
		}

		#region   MaritimeUcnsThatAreHeld
		[ChildEditable(true)]
		public CusAddInfoCollection<MaritimeUcnThatIsHeld> MaritimeUcnsThatAreHeld
		{
			get { return maritimeUcnsThatAreHeld ?? (maritimeUcnsThatAreHeld = GetMaritimeUcnsThatAreHeld()); }
		}
		CusAddInfoCollection<MaritimeUcnThatIsHeld> maritimeUcnsThatAreHeld;

		CusAddInfoCollection<MaritimeUcnThatIsHeld> GetMaritimeUcnsThatAreHeld()
		{
			var result = new CusAddInfoCollection<MaritimeUcnThatIsHeld>(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}
		#endregion

		[ReadOnly(true)]
		public ZString ZG_AmendmentReasonCode
		{
			get => AddInfo.ZG_AmendmentReasonCode;
			set => AddInfo.ZG_AmendmentReasonCode = value;
		}

		public ZPropertyInfo ZG_AmendmentReasonCodeInfo => GetWrappedZPropertyInfo(nameof(ZG_AmendmentReasonCode), x => AddInfo.ZG_AmendmentReasonCodeInfo);

		[ReadOnly(true)]
		public override ZString CH_CustomsMessageRemarks { get => base.CH_CustomsMessageRemarks; set => base.CH_CustomsMessageRemarks = value; }

		public override ZGuid CH_JE
		{
			get => base.CH_JE;
			set
			{
				bool hasChanged = CH_JE != value;
				base.CH_JE = value;
				if (hasChanged && Declaration != null)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
					FECChallenges.MarkAsNeedingValidation();
				}
			}
		}

		// Below is for CCSUK....
		public override ZInt PackagesCount
		{
			get
			{
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					return entryInstruction.CEI_PackageCount;
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		ZBool ICanBeImportOrExport.IsExport
		{
			get { return Declaration != null && Declaration.IsExport; }
		}

		ZBool ICanBeImportOrExport.IsImport
		{
			get { return Declaration != null && Declaration.IsImport; }
		}

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => Declaration?.CountryCode;

		string ICanBeImportOrExport.DataGroupingCode => Declaration?.GetDefaultDataGroupingCode();

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, typeof(CusAddInfo<MaritimeUcnThatIsHeld>));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.FEC, typeof(FECChallenge));
			return result;
		}

		#endregion

		protected override bool TaxFeePaymentCodeIsDeferredCore(ZString cF_MethodOfPayment)
		{
			return cF_MethodOfPayment == Declaration?.ApplicationExtender.DeferredFeeMethodOfPaymentCode;
		}

		protected override ISet<ZString> DutyCodeSetCore()
		{
			var dutyCodeSet = base.DutyCodeSetCore();
			dutyCodeSet.Add(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty);
			return dutyCodeSet;
		}

		protected override ISet<ZString> TaxCodeSetCore()
		{
			var taxCodeSet = base.TaxCodeSetCore();
			taxCodeSet.Add(EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland);
			return taxCodeSet;
		}

		protected override IReadOnlyList<ZString> ExcludedFeeCodesForAllOtherFees => new ZString[] {
			DutyCode,
			TaxCode,
			Core.Constants.Customs.CusEntryFeeTypes.DutyAmount,
			Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount,
			Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred,
			Core.Constants.Customs.CusEntryFeeTypes.TotalAmountPayable,
			GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, //A50
			EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland //B05
		};

		[ChildEditable(true)]
		public FECChallengeCollection FECChallenges
		{
			get
			{
				if (fecChallenges == null)
				{
					fecChallenges = new FECChallengeCollection(Factory, this);
					fecChallenges.Load();
					RegisterEditableChildObject(fecChallenges);
				}
				return fecChallenges;
			}
		}
		FECChallengeCollection fecChallenges;

		public ZBool FECExpectedTransportNationalityAtTheBorder => Declaration.JE_FecFLG;

		protected ZString GetSplitReference() => EntryInstruction?.CEI_SplitReference ?? ZString.Empty;

		public override void OnSaving()
		{
			PopulateLocalReferenceNumberIfNeed();
			base.OnSaving();
		}

		internal void PopulateLocalReferenceNumberIfNeed()
		{
			if (LRN.IsEmpty || (!IsInDatabase && !Globals.IsTest))
			{
				var newLRN = GetNewEntryLocalReferenceNumber();
				if (!newLRN.IsEmpty)
				{
					LRN = newLRN;
				}
			}
		}

		ZString GetNewEntryLocalReferenceNumber()
		{
			return Declaration?.ApplicationExtender.GetNewEntryLocalReferenceNumber(Factory) ?? ZString.Empty;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase && !LRN.IsEmpty)
			{
				LRN = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		public new CusEntryHeaderLookups Lookups => new CusEntryHeaderLookups(this);

		protected override bool IsLookupsCachedInBase => false;

		public new IEnumerable<MultiLineAddInfos.AdditionalInfo> AdditionalInfos => Enumerable.Cast<MultiLineAddInfos.AdditionalInfo>(base.AdditionalInfos);

		CusEntryInstruction GbInstruction => (CusEntryInstruction)base.EntryInstruction; // this could be improved I'm sure

		protected override IEnumerable<AdditionalInfo> GetAdditionalInfosToProcess()
		{
			if (Declaration.AreMultipleEntryInstructionsAllowed)
			{
				// CDS
				var baseAddInfos = base.GetAdditionalInfosToProcess(); // Declaration, plus invoice header's header-only rows
				var allAddInfos = baseAddInfos;
				if (GbInstruction != null)
				{
					allAddInfos = baseAddInfos.Union(GbInstruction.AdditionalInfos.OfType<AdditionalInfo>()).Where(x => x.IsHeaderOnly).GroupBy(ai => new { ai.CSI_Code, ai.CSI_Description }, (key, ai) => ai.FirstOrDefault());
				}

				foreach (var add in allAddInfos)
				{
					yield return (MultiLineAddInfos.AdditionalInfo)add;
				}
			}
			else
			{
				//CHIEF
				foreach (var add in base.GetAdditionalInfosToProcess())
				{
					yield return (MultiLineAddInfos.AdditionalInfo)add;
				}
			}
		}

		public bool IsPreLodgedOrLodgedWithCustoms => CH_EntryStatus == GBCommonConstants.EntryStatusCodes.DeclarationAccepted
													|| CH_EntryStatus == GBCommonConstants.EntryStatusCodes.DeclarationCleared
													|| CH_EntryStatus == EntryStatusList.Codes.Clear
													|| CH_EntryStatus == GBCommonConstants.EntryStatusCodes.TaxCalculated
													|| CH_EntryStatus == EDIMessageStatusList.Codes.Received;

		public ZBool IsCancelledWithCustoms => !CH_EntryStatus.IsEmpty && CH_EntryStatus == EntryStatusList.Codes.Cancelled;

		protected override bool IsMrnEntryNumberTheOneWeWantToShow => Declaration != null || base.IsMrnEntryNumberTheOneWeWantToShow;

		public bool IsAcceptedAndHasMisMatchedLineCount => this.CH_EntryStatus == GBCommonConstants.EntryStatusCodes.DeclarationAccepted && this.CH_HighestLineNumber != this.MergedLines.Count;

		public IEnumerable<EU.Business.Declaration.CusFiscalReference> FiscalReferences
		{
			get
			{
				var entInstruction = EntryInstruction;
				return entInstruction != null ? entInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>()
					.GroupBy(fr => new { fr.CFR_Code, fr.CFR_Reference }, (key, fr) => fr.FirstOrDefault()) :
					new List<EU.Business.Declaration.CusFiscalReference>();
			}
		}

		public Dictionary<ZString, Money> CDSChargeDeductions => Factory.GetValue(ref cdsChargeDeductionsCache, () =>
		{
			var allCdsCharges = AllEntryLines.Cast<CusEntryLine>().SelectMany(x => x.CDSChargeDeductionsAll);
			var cdsLineCharges = AllEntryLines.Cast<CusEntryLine>().SelectMany(x => x.CDSLineChargeDeductions);
			var cdsCharges = allCdsCharges.Except(cdsLineCharges);

			return JobComInvChargeHelper.GetCDSChargeDeductions(cdsCharges);
		});

		CachedProperty<Dictionary<ZString, Money>> cdsChargeDeductionsCache;

		public override ZString DefaultStatusDescription
		{
			get
			{
				var (overrided, newDescription) = Declaration != null ? GetDefaultStatusDescription(this) : (ZBool.False, ZString.Empty);
				return !overrided
					? base.DefaultStatusDescription
					: newDescription;
			}
		}

		(ZBool Overrided, ZString NewDescription) GetDefaultStatusDescription(CusEntryHeader entry)
		{
			return !entry.MovementReferenceNumber.IsEmpty
				? (ZBool.True, (ZString)ResString.GetMultilingualString("3D4307E9-C691-496F-A124-0B905C96D098", "Not Accepted"))
				: (ZBool.False, ZString.Empty);
		}

		public override ZString CH_EntryStatus
		{
			get => base.CH_EntryStatus;
			set
			{
				var oldValue = CH_EntryStatus;
				base.CH_EntryStatus = value;
				if (oldValue != CH_EntryStatus)
				{
					if (CH_EntryStatus == GBCommonConstants.EntryStatusCodes.DeclarationAccepted)
					{
						CH_HighestLineNumber = (short)(!MergedLines.Any() ? 0 : MergedLines.OfType<CusEntryLine>().Max(x => x.CL_LineNumber));
					}
				}
			}
		}

		protected override bool IsOriginAndDestinationRequiredInItinerary => Declaration != null;

		public override bool IsIndirectExport
		{
			get
			{
				if (Declaration.CountryCode != Core.Constants.CountryCodes.UnitedKingdom || (Declaration?.Origin?.IsInNorthernIreland ?? false))
				{
					return base.IsIndirectExport;
				}
				else
				{
					return false;
				}
			}
		}

		public bool IsFSD
		{
			get
			{
				return EntryInstruction != null && (IsSDI && Declaration.InvoiceLines.Count == 1 && Declaration.InvoiceLines[0].JI_Procedure == JobComInvoiceLine.CfspFsdCPCCode);
			}
		}
		public bool IsSDI
		{
			get { return IsISD && EntryInstruction.CEI_SubStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration; }
		}

		public bool IsISD => IsImport && EntryInstruction.CEI_Style == ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;

		protected override IList<PermitRecord> GetPermitRecordsCore() => Declaration?.ApplicationExtender.GetPermitRecords(this);

		protected override ZString ComplementaryJobPreviousDocumentCodeTypeCore => Declaration?.ApplicationExtender.GetComplementaryJobPreviousDocumentCodeType(base.ComplementaryJobPreviousDocumentCodeTypeCore) ?? ZString.Empty;

		public override ZString EntryHeaderStatusDescription => Declaration.ApplicationExtender.GetEntryHeaderStatusDescription(this.CH_EntryStatus, base.EntryHeaderStatusDescription);

		public virtual ZString CH_MasterUCR
		{
			get { return MasterUCR.CE_EntryNum; }
			set { MasterUCR.CE_EntryNum = value; }
		}

		public ZPropertyInfo CH_MasterUCRInfo { get { return GetWrappedZPropertyInfo(Schema.CH_MasterUCR, x => MasterUCR.CE_EntryNumInfo); } }

		CachedProperty<CusEntryNumber> masterUCR;
		CusEntryNumber MasterUCR => Factory.GetValue(ref masterUCR, () =>
		{
			var masterUCRInner = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.MasterUCR, CountryCode);
			if (masterUCRInner == null)
			{
				masterUCRInner = CusEntryNumber.New(this, CusEntryNumberTypes.EU.MasterUCR, CountryCode);
				masterUCRInner.CE_EntryIsSystemGenerated = false;
			}
			return masterUCRInner;
		});

		public override void InitialiseMasterUCR()
		{
			var splitReference = GetSplitReference();
			var declaration = Declaration;

			if (!splitReference.IsEmpty && !declaration.JE_MasterUCR.IsEmpty)
			{
				if (declaration.IsCNS)
				{
					CH_MasterUCR = declaration.JE_MasterUCR.EndsWith("00") ? declaration.JE_MasterUCR.Substring(0, declaration.JE_MasterUCR.Length - 2) + splitReference : declaration.JE_MasterUCR.PadRight(23, ' ') + splitReference;
				}
				else if (declaration.ZG_Gateway == GatewayList.Codes.MCP_CUSDECOnly)
				{
					CH_MasterUCR = declaration.JE_MasterUCR.Length == 9 ? declaration.JE_MasterUCR + "000" + splitReference
						: declaration.JE_MasterUCR.Length == 12 ? declaration.JE_MasterUCR + splitReference
						: declaration.JE_MasterUCR.PadRight(23, ' ') + splitReference;
				}
				else if (declaration.IsPentant)
				{
					if (declaration.ActiveEntryHeaders.Count > 1)
					{
						CH_MasterUCR = !CH_BGMReference.Contains("/") ? declaration.JE_MasterUCR : ZString.Empty;
					}
					else
					{
						CH_MasterUCR = declaration.JE_MasterUCR;
					}
				}
				else
				{
					CH_MasterUCR = declaration.JE_MasterUCR.PadRight(23, ' ') + splitReference;
				}
			}
			else
			{
				if (IsImport)
				{
					var isOtherCehActive = declaration.ActiveEntryHeaders.Except(new[] { this }).Cast<CusEntryHeader>().Any(ch => !ch.IsCancelledWithCustoms);
					if (!isOtherCehActive)
					{
						CH_MasterUCR = declaration.JE_MasterUCR;
					}
					else
					{
						CH_MasterUCR = CH_BGMReference.Contains("/") ? ZString.Empty : declaration.JE_MasterUCR;
					}
				}
				else
				{
					CH_MasterUCR = declaration.JE_MasterUCR;
				}
			}
		}

		bool Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter.SupportModificationState => true;

		protected override ZBool TaxFeePaymentCodeIsImmediate(ZString methodOfPayment) => MethodOfPaymentCodes.CDS.ImmediateMethodsOfPayment.Contains(methodOfPayment);

		public bool IsAnyEntryLineUsingControlledGoodsProcedure
		{
			get
			{
				var proceduresSuffixes = new[] { "1CD", "1CG", "2CD", "2CG" };

				foreach (var invoiceLine in InvoiceLines.OfType<JobComInvoiceLine>())
				{
					if (proceduresSuffixes.Any(ps => invoiceLine.JI_FormattedProcedure.EndsWith(ps) || invoiceLine.AdditionalProcedureCodes.Where(ap => ap.CY_Code.EndsWith(ps)).Any()))
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZString CH_ExitActualOffice
		{
			get => AddInfo.ZG_ExitActualOffice;
			set => AddInfo.ZG_ExitActualOffice = value;
		}

		public ZPropertyInfo CH_ExitActualOfficeInfo => GetWrappedZPropertyInfo(Schema.CH_ExitActualOffice, x => AddInfo.ZG_ExitActualOfficeInfo);

		public ZDateTime CH_ExitDate
		{
			get => AddInfo.ZG_ExitDate;
			set => AddInfo.ZG_ExitDate = value;
		}

		public ZPropertyInfo CH_ExitDateInfo => GetWrappedZPropertyInfo(Schema.CH_ExitDate, x => AddInfo.ZG_ExitDateInfo);

		#region IMessageAttachee

		string IMessageAttachee.JobNumber => CH_BGMReference;

		ZString IMessageAttachee.LocalReferenceNumber => LRN;

		string IMessageAttachee.DataGroupingCode => Declaration?.GetDefaultDataGroupingCode();

		ZString IMessageAttachee.JobReference => Declaration.JE_DeclarationReference;

		ZString IMessageAttachee.Gateway => Declaration.ZG_Gateway;

		void IMessageAttachee.UpdateStatusIfNotEmpty(ZString status) => this.UpdateStatusIfNotEmpty(status);

		#endregion
	}
}
