using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentItem : ExitControlBase.Business.CusExitConsignmentItem
		, Integration.Customs.EUExitControl.ICusExitConsignmentItem
		, IUcc6ValueProvider
		, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public CusExitConsignmentItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsUCC6)
			{
				SetReadOnlyIfStatusIsMissing(StatusIsMissing);
			}
		}

		public new class Schema : AutoCusExitConsignmentItem.Schema
		{
			public const string CCI_Calc_ReportGrossMass = nameof(CusExitConsignmentItem.CCI_Calc_ReportGrossMass);
			public const string CCI_Calc_ReportNetMass = nameof(CusExitConsignmentItem.CCI_Calc_ReportNetMass);
			public const string CCI_Calc_ShouldReportItem = nameof(CusExitConsignmentItem.CCI_Calc_ShouldReportItem);
		}

		public static readonly CusExitConsignmentItemTypeDecider TypeDecider = new CusExitConsignmentItemTypeDecider();

		public new CusExitConsignment Consignment => Factory.Load<CusExitConsignment>(CCI_CXC_Consignment);

		protected override ZString HumanReadableNameCore => Res.GetString("{80C48A5F-B4C4-434B-AB58-C10F8D620E22}", "Consignment Item ({0})", CCI_LineNumber);

		protected override int MaxPivotItemCountCore => 99;

		[ResourceStringData("B5B42532-449E-4627-9F60-0CE290B61ACA", Caption = "Item Number", ShortCaption = "Item No.")]
		public override ZShort CCI_LineNumber
		{
			get => base.CCI_LineNumber;
			set => base.CCI_LineNumber = value;
		}

		[ResourceStringData("F0557C29-430F-4848-98D7-1A02592A5132", Caption = "Gross Mass KG")]
		public override ZDecimal CCI_GrossMass
		{
			get => base.CCI_GrossMass;
			set => base.CCI_GrossMass = value;
		}

		[ResourceStringData("11B6C9F4-7DCA-4231-B7F0-ED54F4FC59FE", Caption = "Net Mass KG")]
		public override ZDecimal CCI_NetMass
		{
			get => base.CCI_NetMass;
			set => base.CCI_NetMass = value;
		}

		[ResourceStringData("F7A2835C-0607-426F-BFFC-E4C9D32CABB7", Caption = "Reference Number UCR")]
		public override ZString CCI_UniqueConsignmentReference
		{
			get => base.CCI_UniqueConsignmentReference;
			set => base.CCI_UniqueConsignmentReference = value;
		}

		#region CusExitReportItem related properties

		[ResourceStringData("{5195D04C-8BC0-4093-BEBF-84B2C253F2B2}", Caption = "Gross Mass KG")]
		public ZDecimal CCI_Calc_ReportGrossMass
		{
			get
			{
				if (!reportGrossMassCached.HasValue)
				{
					if (FirstMatchingReportItem is CusExitReportItem reportItem)
					{
						reportGrossMassCached = reportItem.ERI_GrossMass;
					}
					else
					{
						reportGrossMassCached = CCI_GrossMass;
					}
				}
				return reportGrossMassCached.Value;
			}
			set
			{
				var oldValue = CCI_Calc_ReportGrossMass;
				reportGrossMassCached = value;
				if (CCI_Calc_ShouldReportItem && oldValue != 0M)
				{
					CCI_Calc_ReportNetMass = Utilities.Round(CCI_Calc_ReportNetMass * value / oldValue, 5);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCCI_Calc_ReportGrossMass();
				}
				CCI_Calc_ReportGrossMassInfo.RefreshBinding();
			}
		}
		ZDecimal? reportGrossMassCached;

		public ZPropertyInfo CCI_Calc_ReportGrossMassInfo => GetZPropertyInfo(Schema.CCI_Calc_ReportGrossMass);

		[ResourceStringData("{A5D3C89E-21EA-472E-A4FF-66F01F5FA566}", Caption = "Net Mass KG")]
		public ZDecimal CCI_Calc_ReportNetMass
		{
			get
			{
				if (!reportNetMassCached.HasValue)
				{
					if (FirstMatchingReportItem is CusExitReportItem reportItem)
					{
						reportNetMassCached = reportItem.ERI_NetMass;
					}
					else
					{
						reportNetMassCached = CCI_NetMass;
					}
				}
				return reportNetMassCached.Value;
			}
			set
			{
				reportNetMassCached = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCCI_Calc_ReportNetMass();
				}
				CCI_Calc_ReportNetMassInfo.RefreshBinding();
			}
		}
		ZDecimal? reportNetMassCached;

		public ZPropertyInfo CCI_Calc_ReportNetMassInfo => GetZPropertyInfo(Schema.CCI_Calc_ReportNetMass);

		[ResourceStringData("{D41E6DB2-F38E-41CD-9C6A-7B910C7AFA56}", Caption = "Select")]
		public ZBool CCI_Calc_ShouldReportItem
		{
			get
			{
				if (!shouldReportItemCached.HasValue)
				{
					shouldReportItemCached = FirstMatchingReportItem != null;
				}
				return shouldReportItemCached.Value;
			}
			set
			{
				shouldReportItemCached = value;
				CCI_Calc_ShouldReportItemInfo.RefreshBinding();
				if (!IsPackageShouldReportItemSuspended)
				{
					using (GetPackageShouldReportItemSuspender())
					{
						CusExitConsignmentPackagePivots.Select(pivot => pivot.Package).WhereNotNull().ForEach(package => package.CXP_Calc_ShouldReportItem = value);
					}
				}
			}
		}
		ZBool? shouldReportItemCached;

		[ResourceStringData("E3E398FB-5FA8-4A03-A60A-1B65026AFB78", Caption = "Status", ShortCaption = "Status", MediumCaption = "Status", FullDescription = "Consignment Item Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentItemLookups.StatusList))]
		public override ZString CCI_DiscrepancyStatus
		{
			get => base.CCI_DiscrepancyStatus;
			set
			{
				var oldValue = CCI_DiscrepancyStatus;
				base.CCI_DiscrepancyStatus = value;
				if (!IsCopying && oldValue != CCI_DiscrepancyStatus && IsUCC6)
				{
					ClearStatusAndSetSealsReadOnlyIfStatusIsMissing();
				}
			}
		}

		[ResourceStringData("93B6F1FC-2B2A-486D-89C9-A62E4D457B3D", ShortCaption = "UCR Status", Caption = "UCR Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentItemLookups.UCRStatusList))]
		public override ZString CCI_UniqueConsignmentReferenceStatus
		{
			get => base.CCI_UniqueConsignmentReferenceStatus;
			set => base.CCI_UniqueConsignmentReferenceStatus = value;
		}

		DisposableAction GetPackageShouldReportItemSuspender() => new DisposableAction(() => reportItemSuspender++, () => reportItemSuspender--);

		public void OnPackageShouldReportItemSet()
		{
			if (!IsPackageShouldReportItemSuspended)
			{
				using (GetPackageShouldReportItemSuspender())
				{
					CCI_Calc_ShouldReportItem = CusExitConsignmentPackagePivots.Any(pivot => pivot.Package?.CXP_Calc_ShouldReportItem ?? false);
				}
			}
		}
		int reportItemSuspender;
		bool IsPackageShouldReportItemSuspended => reportItemSuspender > 0;

		public ZPropertyInfo CCI_Calc_ShouldReportItemInfo => GetZPropertyInfo(Schema.CCI_Calc_ShouldReportItem);

		public CusExitReportItem FirstMatchingReportItem
		{
			get
			{
				if (firstMatchingReportItem == null || firstMatchingReportItem.IsDeleted)
				{
					firstMatchingReportItem = GetMatchingExitReportItems().OrderBy(x => x.ERI_SystemCreateTimeUtc).FirstOrDefault();
				}
				return firstMatchingReportItem;
			}
		}
		CusExitReportItem firstMatchingReportItem;

		public IEnumerable<CusExitReportItem> GetMatchingExitReportItems() => matchingExitReport?.CusExitReportItems.Cast<CusExitReportItem>().Where(x => x.ERI_CCI_ConsignmentItem == PK) ?? Enumerable.Empty<CusExitReportItem>();

		internal bool IsCreatonOrUpdatingReportItemData { get; private set; }

		public IDisposable SetupReportItemData(CusExitReport exitReport)
		{
			return new DisposableAction(() =>
			{
				IsCreatonOrUpdatingReportItemData = true;
				var oldReportGrossMass = reportGrossMassCached.GetValueOrDefault();
				var oldReportNetMass = reportNetMassCached.GetValueOrDefault();
				var oldShouldReportItemCached = shouldReportItemCached.GetValueOrDefault();
				SetMatchingExitReport(exitReport);
				reportGrossMassCached = null;
				reportNetMassCached = null;
				shouldReportItemCached = null;
				CCI_Calc_ReportGrossMassInfo.RefreshBinding(oldReportGrossMass);
				CCI_Calc_ReportNetMassInfo.RefreshBinding(oldReportNetMass);
				CCI_Calc_ShouldReportItemInfo.RefreshBinding(oldShouldReportItemCached);
			}, () =>
			{
				IsCreatonOrUpdatingReportItemData = false;
				SetMatchingExitReport(null);
				var validation = Validation;
				validation.ValidateCCI_Calc_ReportGrossMass();
				validation.ValidateCCI_Calc_ReportNetMass();
				reportGrossMassCached = null;
				reportNetMassCached = null;
				shouldReportItemCached = null;
			});
		}

		public void SetMatchingExitReport(CusExitReport exitReport)
		{
			matchingExitReport = exitReport;
			firstMatchingReportItem = null;
		}
		CusExitReport matchingExitReport;

		#endregion

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Consignment?.IsUCC6 ?? false;

		protected override ExitControlBase.Business.CusExitConsignmentItemLookups GetNewLookups() => IsUCC6
			? new CusExitConsignmentItemUcc6Lookups(this)
			: new CusExitConsignmentItemLookups(this);

		protected override ExitControlBase.Business.CusExitConsignmentItemValidation GetNewValidation() => new CusExitConsignmentItemValidation(this);

		public new CusExitConsignmentItemLookups Lookups => (CusExitConsignmentItemLookups)base.Lookups;

		public new CusExitConsignmentItemValidation Validation => (CusExitConsignmentItemValidation)base.Validation;

		public ICusExitConsignmentItemValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusExitConsignmentItemValidationDecider> validationDeciderCached;

		ICusExitConsignmentItemValidationDecider GetValidationDecider() => Consignment?.Header?.Configuration.CusExitConsignmentItemConfiguration.GetValidationDecider(this);

		public new ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentPivots;

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPackagePivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentPackagePivots;

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentContainerPivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentContainerPivots;

		protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentContainerPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Container);

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPackagePivotCollection()
		{
			if (IsUCC6)
			{
				var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Package);
				collection.ApplySort(nameof(CusExitConsignmentPivot.SequenceNumber), System.ComponentModel.ListSortDirection.Ascending);
				return collection;
			}

			return new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Package);
		}

		public IDictionary<ZShort, ZShort> PackagesSequenceDictionary => Factory.GetValue(ref packagesSequenceDictionaryCached, () => CusExitConsignmentPackagePivots
			.Select(pivot => pivot.Package).WhereNotNull()
			.Select(x => x.CXP_Sequence).Where(x => x > ZShort.Zero)
			.GroupBy(x => x).ToDictionary(x => x.Key, y => (ZShort)y.Count()));
		CachedProperty<IDictionary<ZShort, ZShort>> packagesSequenceDictionaryCached;

		public ZBool StatusIsMissing => CCI_DiscrepancyStatus == DiscrepanciesStatusCodeList.Codes.Missing;

		public ZBool StatusIsDifferencesToDeclared => CCI_DiscrepancyStatus == DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

		public ZBool UCRStatusIsDifferencesToDeclared => CCI_UniqueConsignmentReferenceStatus == DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

		[ChildEditable(true)]
		public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => fAdditionalInfos ??= GetAdditionalInfos();
		IAdditionalInfoCollection<AdditionalInfo> fAdditionalInfos;

		IAdditionalInfoCollection<AdditionalInfo> GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}
		protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) }
			};
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		void ClearStatusAndSetSealsReadOnlyIfStatusIsMissing()
		{
			var readOnly = StatusIsMissing;
			if (readOnly)
			{
				CusExitConsignmentPackagePivots.Cast<CusExitConsignmentPivot>().ForEach(x => x.Package.CXP_MarksAndNumbersStatus = ZString.Empty);
				AdditionalInfos.Cast<AdditionalInfo>().ForEach(x => x.CSI_Status = ZString.Empty);
			}
			SetReadOnlyIfStatusIsMissing(readOnly);
		}

		void SetReadOnlyIfStatusIsMissing(bool readOnly)
		{
			CusExitConsignmentPackagePivots.ForEach(x => x.SetReadOnlyIncludingChildren(readOnly));
			AdditionalInfos.SetReadOnlyIncludingChildren(readOnly);
		}
	}
}
