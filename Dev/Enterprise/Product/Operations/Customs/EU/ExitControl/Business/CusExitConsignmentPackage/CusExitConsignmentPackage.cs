using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPackage : ExitControlBase.Business.CusExitConsignmentPackage
		, Integration.Customs.EUExitControl.ICusExitConsignmentPackage
		, IUcc6ValueProvider
	{
		public CusExitConsignmentPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusExitConsignmentPackage.Schema
		{
			public const string CXP_Calc_ReportQuantity = nameof(CusExitConsignmentPackage.CXP_Calc_ReportQuantity);
			public const string CXP_Calc_ShouldReportItem = nameof(CusExitConsignmentPackage.CXP_Calc_ShouldReportItem);
		}

		public static readonly CusExitConsignmentPackageTypeDecider TypeDecider = new CusExitConsignmentPackageTypeDecider();

		public new CusExitHeader Header => Factory.Load<CusExitHeader>(CXP_CXH_Header);

		public new CusExitConsignmentPivot ConsignmentPivot => Factory.LoadTop1<CusExitConsignmentPivot>(new ZQuery(CusExitConsignmentPivotSchema.CNP_CXP_Package, PK));

		[ResourceStringData("7B3B383D-2380-4522-869A-1DA28A43CC6D", Caption = "Seq No.")]
		[ReadOnlyMember(nameof(CXP_SequenceReadOnly))]
		public override ZShort CXP_Sequence
		{
			get => base.CXP_Sequence;
			set => base.CXP_Sequence = value;
		}

		protected virtual bool CXP_SequenceReadOnly => true;

		[ResourceStringData("A39E92EE-03A7-4580-9D2A-B2B1D5A12E24", Caption = "Pack Qty")]
		public override ZInt CXP_Quantity
		{
			get => base.CXP_Quantity;
			set => base.CXP_Quantity = value;
		}

		[ResourceStringData("8A0D9EA2-81E1-4FC2-9E94-2E5350FFCB4F", Caption = "Pack Type")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentPackageLookups.PackTypeList))]
		public override ZString CXP_PackageType
		{
			get => base.CXP_PackageType;
			set => base.CXP_PackageType = value;
		}

		[ResourceStringData("C1F9EDD1-B3AB-4C13-BAB5-9321CF5BA492", Caption = "Marks & Numbers")]
		public override ZString CXP_MarksAndNumbers
		{
			get => base.CXP_MarksAndNumbers;
			set => base.CXP_MarksAndNumbers = value;
		}

		[ResourceStringData("F9AA4758-305C-4C68-B9F8-77A0E2582FEC", Caption = "Status", ShortCaption = "Status", MediumCaption = "Status", FullDescription = "Consignment Package Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentPackageLookups.StatusList))]
		public override ZString CXP_MarksAndNumbersStatus { get => base.CXP_MarksAndNumbersStatus; set => base.CXP_MarksAndNumbersStatus = value; }

		#region CusExitReportItem related properties

		[ResourceStringData("{24898090-ABE6-46FB-BDA7-62A8DBA6FA3A}", Caption = "Pack Qty")]
		public ZInt CXP_Calc_ReportQuantity
		{
			get
			{
				if (!reportQuantityCached.HasValue)
				{
					if (FirstReportItem is CusExitReportItem reportItem)
					{
						reportQuantityCached = reportItem.ERI_Quantity;
					}
					else
					{
						reportQuantityCached = CXP_Quantity;
					}
				}
				return reportQuantityCached.Value;
			}
			set
			{
				reportQuantityCached = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCXP_Calc_ReportQuantity();
				}
				CXP_Calc_ReportQuantityInfo.RefreshBinding();
			}
		}
		ZInt? reportQuantityCached;

		public ZPropertyInfo CXP_Calc_ReportQuantityInfo => GetZPropertyInfo(Schema.CXP_Calc_ReportQuantity);

		[ResourceStringData("{D41E6DB2-F38E-41CD-9C6A-7B910C7AFA56}", Caption = "Select")]
		public ZBool CXP_Calc_ShouldReportItem
		{
			get
			{
				if (!shouldReportItemCached.HasValue)
				{
					shouldReportItemCached = FirstReportItem != null;
				}
				return shouldReportItemCached.Value;
			}
			set
			{
				shouldReportItemCached = value;
				CXP_Calc_ShouldReportItemInfo.RefreshBinding();
				ConsignmentPivot?.ConsignmentItem?.OnPackageShouldReportItemSet();
			}
		}
		ZBool? shouldReportItemCached;

		public ZPropertyInfo CXP_Calc_ShouldReportItemInfo => GetZPropertyInfo(Schema.CXP_Calc_ShouldReportItem);

		public CusExitReportItem FirstReportItem
		{
			get
			{
				if (firstReportItem == null || firstReportItem.IsDeleted)
				{
					firstReportItem = GetMatchingExitReportItems().OrderBy(x => x.ERI_SystemCreateTimeUtc).FirstOrDefault();
				}
				return firstReportItem;
			}
		}
		CusExitReportItem firstReportItem;

		public IEnumerable<CusExitReportItem> GetMatchingExitReportItems() => matchingExitReportPK.IsValid ? CusExitReportItems.Where(x => x.ERI_CER_Report == matchingExitReportPK) : Enumerable.Empty<CusExitReportItem>();

		internal bool IsCreatonOrUpdatingReportItemData { get; private set; }

		public IDisposable SetupReportItemData(ZGuid exitReportPK)
		{
			return new DisposableAction(() =>
			{
				IsCreatonOrUpdatingReportItemData = true;
				var oldReportQuantity = reportQuantityCached.GetValueOrDefault();
				var oldShouldReportItemCached = shouldReportItemCached.GetValueOrDefault();
				SetMatchingExitReportPK(exitReportPK);
				reportQuantityCached = null;
				shouldReportItemCached = null;
				CXP_Calc_ReportQuantityInfo.RefreshBinding(oldReportQuantity);
				CXP_Calc_ShouldReportItemInfo.RefreshBinding(oldShouldReportItemCached);
			}, () =>
			{
				IsCreatonOrUpdatingReportItemData = false;
				SetMatchingExitReportPK(ZGuid.Empty);
				Validation.ValidateCXP_Calc_ReportQuantity();
				reportQuantityCached = null;
				shouldReportItemCached = null;
			});
		}

		public void SetMatchingExitReportPK(ZGuid exitReportPK)
		{
			matchingExitReportPK = exitReportPK;
			firstReportItem = null;
		}
		ZGuid matchingExitReportPK;

		#endregion

		public bool IsBulk => Factory.GetValue(ref isBulkCached, () => Lookups.BulkPackageUnitTypeList.ContainsCode(CXP_PackageType));
		CachedProperty<bool> isBulkCached;
		public bool IsBreakBulk => Factory.GetValue(ref isBreakBulkCached, () => Lookups.BreakBulkPackageUnitTypeList.ContainsCode(CXP_PackageType));
		CachedProperty<bool> isBreakBulkCached;

		public ZBool StatusIsMissing => CXP_MarksAndNumbersStatus == DiscrepanciesStatusCodeList.Codes.Missing;
		public ZBool StatusIsDifferencesToDeclared => CXP_MarksAndNumbersStatus == DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Header?.IsUCC6 ?? false;

		public new CusExitConsignmentPackageLookups Lookups => (CusExitConsignmentPackageLookups)base.Lookups;

		public new CusExitConsignmentPackageValidation Validation => (CusExitConsignmentPackageValidation)base.Validation;

		public new ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

		protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

		protected override ExitControlBase.Business.CusExitConsignmentPackageValidation GetNewValidation() => new CusExitConsignmentPackageValidation(this);

		protected override ExitControlBase.Business.CusExitConsignmentPackageLookups GetNewLookups() => IsUCC6
			? new CusExitConsignmentPackageUcc6Lookups(this)
			: new CusExitConsignmentPackageLookups(this);

		public ICusExitConsignmentPackageValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusExitConsignmentPackageValidationDecider> validationDeciderCached;

		ICusExitConsignmentPackageValidationDecider GetValidationDecider() => Header?.Configuration.CusExitConsignmentPackageConfiguration.GetValidationDecider(this);
	}
}
