using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[CodeProperty(nameof(CusEntryHeader.CodeProperty))]
	public partial class CusEntryHeader : Customs.Business.CusEntryHeader,
		Integration.Customs.AsycudaCustoms.ICusEntryHeader
	{
		#region Schema

		public new class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string TransitPermitCount = "TransitPermitCount";
			public const string TransitPermitInTransitCount = "TransitPermitInTransitCount";
			public const string Expired = "Expired";
			public const string EarliestExpiryDate = "EarliestExpiryDate";
			public const string Completed = "Completed";
		}

		#endregion

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : Customs.Business.CusEntryHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusEntryHeader);

			public CusEntryHeader FindByEntryNumberAndMessageType(ZString entryNumber, ZString messageType)
			{
				CusEntryHeader result = null;
				if (!entryNumber.IsEmpty && !messageType.IsEmpty)
				{
					var entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, messageType);
					var companyPK = GlbCompany.CurrentCompany.PK;
					foreach (var bizO in Factory.Load<CusEntryNumber>(entryNumberFilter))
					{
						if (Factory.Load(typeof(Customs.Business.CusEntryHeader), bizO.CE_ParentID) is CusEntryHeader entryHeader
							&& entryHeader.CH_MessageType == messageType
							&& entryHeader.Declaration is JobDeclaration declaration
							&& declaration.JE_GC == companyPK)
						{
							result = entryHeader;
							break;
						}
					}
				}

				return result;
			}

			protected override Customs.Business.CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber)
			{
				ErrorReporter.ReportOnce("AsycudaCustomsFindByEntryNumberAndCurrentCompany Is Not Supported", "This method is not valid for Asycuda Customs; please use FindByEntryNumberAndMessageType.");
				return null;
			}

			protected override Customs.Business.CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber, Predicate<Customs.Business.CusEntryHeader> entryHeaderFilter)
			{
				ErrorReporter.ReportOnce("AsycudaCustomsFindByEntryNumberAndCurrentCompany Is Not Supported", "This method is not valid for Asycuda Customs; please use FindByEntryNumberAndMessageType.");
				return null;
			}
		}

		public override ZString ReferenceNumber
		{
			get
			{
				var entryNumber = EntryNumber;
				return entryNumber.IsEmpty ? CH_BGMReference : entryNumber;
			}
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public new ZString MovementReferenceNumber
		{
			get { return base.MovementReferenceNumber; }
			set
			{
				var oldValue = base.MovementReferenceNumber;
				if (!IsCopying && oldValue != value)
				{
					CheckMaximumLength(MovementReferenceNumberInfo, value);
					var issueDate = value.IsEmpty ? ZDateTime.Empty : ZDateTime.Today;
					MovementReferenceNumberSetter(value, issueDate);
					MovementReferenceNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public new ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_EntryStatusList))]
		public override ZString CH_EntryStatus
		{
			get { return base.CH_EntryStatus; }
			set
			{
				base.CH_EntryStatus = value;
				Declaration?.JE_EntryStatusDescriptionInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader|CH_BondValidToDate", Caption = "Acquit By Date")]
		public override ZDate CH_BondValidToDate
		{
			get { return base.CH_BondValidToDate; }
			set { base.CH_BondValidToDate = value; }
		}

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader|CH_BondAcquittedDate", Caption = "Acquitted Date")]
		public override ZDate CH_BondAcquittedDate
		{
			get => base.CH_BondAcquittedDate;
			set => base.CH_BondAcquittedDate = value;
		}

		[ResourceStringData("d7d2f9e2-7b8f-43f9-a60a-354b88b673ac", Caption = "Customs Value")]
		public override ZDecimal CustomsValue => base.CustomsValue;

		[ResourceStringData("033df409-fbfc-43b5-aad3-7761b2e356b1", Caption = "VAT/GST Value")]
		public override ZDecimal ValueForVAT => base.ValueForVAT;

		protected override ZString HumanReadableNameCore => Res.GetString("6E0EEA3F-1CC7-4D87-9423-D405CECDC49C", "Customs Entry {0}-{1}", EntryNumber, CH_BGMReference);

		public ZString CodeProperty => EntryNumber.IsEmpty ? CH_BGMReference : ZString.Format("{0}-{1}", CH_BGMReference, EntryNumber);

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				PopulateCH_BGMReferenceIfNeeded();
			}
		}

		public void PopulateCH_BGMReferenceIfNeeded()
		{
			var declaration = Declaration;
			if (declaration != null && NeedAllocateBGMReference)
			{
				declaration.AllocateAllBGMReferences();
			}
		}

		protected override bool EntryNumber_ReadOnly
		{
			get
			{
				return CH_HasManualWhsUpdate && !EntryNumber.IsEmpty;
			}
		}

		public override bool ShouldResetBGMReferenceOnUnsuccessfulSave => false;

		public bool NeedAllocateBGMReference => !IsDeleted && !IsDeleting && CH_BGMReference.IsEmpty;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public ZInt TransitPermitCount => EntryInstruction?.CusInBondPermitsHeaders.Count ?? ZInt.Zero;

		public ZPropertyInfo TransitPermitCountInfo => GetZPropertyInfo(nameof(TransitPermitCount));

		public ZInt TransitPermitInTransitCount => EntryInstruction?.CusInBondPermitsHeaders.Count(a => a.BM_ArrivalDate.IsEmpty) ?? ZInt.Zero;

		public ZPropertyInfo TransitPermitInTransitCountInfo => GetZPropertyInfo(nameof(TransitPermitInTransitCount));

		public ZBool Expired => EntryInstruction?.CusInBondPermitsHeaders.Any(a => IsExpired(a)) ?? ZBool.False;

		public ZPropertyInfo ExpiredInfo => GetZPropertyInfo(nameof(Expired));

		public ZDateTime EarliestExpiryDate => EntryInstruction?.CusInBondPermitsHeaders.Where(a => a.BM_ArrivalDate.IsEmpty && !a.BM_Calc_ValidityDate.IsEmpty).Select(b => b.BM_Calc_ValidityDate).DefaultIfEmpty().Min() ?? ZDateTime.Empty;

		public ZPropertyInfo EarliestExpiryDateInfo => GetZPropertyInfo(nameof(EarliestExpiryDate));

		public ZBool Completed
		{
			get
			{
				var headers = EntryInstruction?.CusInBondPermitsHeaders;
				return (headers?.Any() ?? ZBool.False) && headers.All(a => !a.BM_ArrivalDate.IsEmpty);
			}
		}

		public ZPropertyInfo CompletedInfo => GetZPropertyInfo(nameof(Completed));

		ZBool IsExpired(CusInBondMoveHeader moveHeader) => moveHeader.BM_ArrivalDate.IsEmpty && moveHeader.BM_Calc_ValidityDate < ZDateTime.Today;

		protected override void AddExtraRequiredFieldsMessageError(ZStringBuilder messageErrors, bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			base.AddExtraRequiredFieldsMessageError(messageErrors, checkProduct, checkQuantity, checkEntryDetails);

			if (EntryNumber.IsEmpty)
			{
				messageErrors.Append(Res.GetString("D5943802-D59D-414B-9368-8CF7244148DC", "An Entry Header marked for {0} must have a valid entry number; not all Entry Headers marked for {0} have a valid entry number specified.", Declaration.TermNameForBondedWarehouse));
			}

			if (IsOutwardBondedWarehousingEnabled && !HasPreviousEntryNumber)
			{
				messageErrors.Append(Res.GetString("DA37B62A-466D-44A8-A2D6-5E750CADF0CD", "An Invoice Line marked for {0} must have a valid previous entry number; not all Invoice Lines marked for {0} have a valid previous entry number specified.", Declaration.TermNameForBondedWarehouse));
			}

			if (IsOutwardBondedWarehousingEnabled && !HasPreviousEntryLineNumber)
			{
				messageErrors.Append(Res.GetString("45E85F01-18C0-443B-BA30-55FBD8F6774", "An Invoice Line marked for {0} must have a valid previous entry line number; not all Invoice Lines marked for {0} have a valid previous entry line number specified.", Declaration.TermNameForBondedWarehouse));
			}
		}

		bool HasPreviousEntryNumber => Factory.GetValue(ref hasPreviousEntryNumberCached, () => InvoiceLines.All(x => !x.JI_PreviousEntryNumber.IsEmpty));
		CachedProperty<bool> hasPreviousEntryNumberCached;

		bool HasPreviousEntryLineNumber => Factory.GetValue(ref hasPreviousEntryLineNumberCached, () => InvoiceLines.All(x => !x.JI_PreviousEntryLineNumber.IsEmpty));
		CachedProperty<bool> hasPreviousEntryLineNumberCached;

		public ZDecimal NetWeightKilograms => Factory.GetValue(ref netWeightKilogramsCached, () => (ZDecimal)AllEntryLines.Cast<CusEntryLine>().Sum(x => x.EffectiveNetWeight.InKilogramsSafe));
		CachedProperty<ZDecimal> netWeightKilogramsCached;

		public ZDecimal CustomsQuantity => Factory.GetValue(ref customsQuantityCached, () => (ZDecimal)AllEntryLines.Cast<CusEntryLine>().Sum(x => x.CustomsQuantity));
		CachedProperty<ZDecimal> customsQuantityCached;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusEntryHeaderFetchStrategy(this);

		public override bool ShouldLogEntryStatus => true;

		public override bool HasBeenWithdrawn => false;

		public bool IsOutOfRegime => Factory.GetValue(
			ref isOutOfRegime,
			() =>
			{
				var procedure = RandomEntryLine?.RandomLine?.CusProcedure;
				return procedure != null &&
					(procedure.IsOutOfWarehouse() ||
					procedure.IsOutOfInwardProcessing() ||
					procedure.IsOutOfOutwardProcessing() ||
					procedure.IsOutOfTemporaryImport() ||
					procedure.IsOutOfTemporaryExport());
			});
		CachedProperty<bool> isOutOfRegime;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			AddOrCancelLiabilityClearedEventLogIfNeeded();
		}

		void AddOrCancelLiabilityClearedEventLogIfNeeded()
		{
			if (EntryInstruction != null && EntryInstruction.IsRiskTabPageTabVisible)
			{
				if (EntryInstruction.HasRiskValue)
				{
					CancelEventLog();
				}
				else
				{
					AddEventLog();
				}
			}

			void AddEventLog()
			{
				if (CanAdd(GetEventLogs()))
				{
					Logs.AddNew(Events.LiabilityCleared, ZDateTimeOffset.Now);
				}
			}

			void CancelEventLog()
			{
				var logs = GetEventLogs();
				if (CanCancel(logs))
				{
					logs[0].Cancel(ZDateTime.Now);
				}
			}

			bool CanAdd(StmALog[] logs) => logs.Length == 0 || logs[0].IsCancelled;

			bool CanCancel(StmALog[] logs) => !CanAdd(logs);

			StmALog[] GetEventLogs()
				=> Logs.Find(log => log.SL_SE_NKEvent == Events.LiabilityCleared.Code && !log.IsDeleted)
				.OrderByDescending(x => x.SL_PostedTimeUtc)
				.ToArray();
		}
	}
}
