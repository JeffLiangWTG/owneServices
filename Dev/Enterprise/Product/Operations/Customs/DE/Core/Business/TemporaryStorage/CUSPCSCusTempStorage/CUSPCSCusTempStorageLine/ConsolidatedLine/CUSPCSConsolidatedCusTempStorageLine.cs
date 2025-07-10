using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CUSPCSCusTempStorageDec), "ConsolidatedCusTempStorageLine")]
	public class CUSPCSConsolidatedCusTempStorageLine : CusTempStorageLine
	{
		public CUSPCSConsolidatedCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Construction / Loading

		public static CUSPCSConsolidatedCusTempStorageLine LoadOrCreate(CUSPCSCusTempStorageDec parent)
		{
			return Load() ?? New();

			CUSPCSConsolidatedCusTempStorageLine Load()
			{
				var query = new ZDBOnlyQuery(typeof(CUSPCSConsolidatedCusTempStorageLine))
				{
					OrderBy = CusTempStorageLineSchema.TSL_SystemCreateTimeUtc.Name + OrderByClause.Descending,
					FetchOnlyFromLocalCache = !parent.IsInDatabase
				};
				query.AddToFilter(CusTempStorageLineSchema.TSL_STH, parent.PK);
				var subQuery = new ZDBOnlySubQuery(typeof(EU.Business.CusTempStorage.CusTempStorageLinePivot), CusTempStorageLinePivotSchema.SLR_TSL_ToLine, true);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return parent.Factory.LoadTop1<CUSPCSConsolidatedCusTempStorageLine>(query);
			}

			CUSPCSConsolidatedCusTempStorageLine New()
			{
				var result = parent.Factory.New<CUSPCSConsolidatedCusTempStorageLine>();
				using (result.SuspendSettingHasChanges())
				{
					result.TSL_STH = parent.PK;
				}
				return result;
			}
		}

		#endregion

		#region Dec

		public new CUSPCSCusTempStorageDec Dec => Factory.Load<CUSPCSCusTempStorageDec>(TSL_STH);

		#endregion

		[BusinessObjectTestExclude]
		[MaxLength(Schema.TSL_OwnerReferenceNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CUSPCSConsolidatedCusTempStorageLineLookups.CusTempStorageRegLineCollection))]
		[ResourceStringData("B32948CC-C42B-4E78-9E1F-1EA7B1FD4476", Caption = "Reference")]
		public ZString FormattedOwnerReferenceNumber
		{
			get
			{
				var result = TSL_OwnerReferenceNumber;
				if (!result.IsEmpty && IsREGDeclaration)
				{
					result = SumARegistrationNumberFormatter.Format(result);
				}
				return result;
			}
			set
			{
				CheckMaximumLength(FormattedOwnerReferenceNumberInfo, value);
				value = value.KeepAlphanumericCharacters();
				if (TSL_OwnerReferenceNumber != value)
				{
					TSL_OwnerReferenceNumber = value;
					PopulateRelatedRegLineProperties();
				}
			}
		}

		public ZPropertyInfo FormattedOwnerReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(FormattedOwnerReferenceNumber), x => TSL_OwnerReferenceNumberInfo);

		public override ZInt TSL_LineNo
		{
			get => base.TSL_LineNo;
			set
			{
				var oldValue = TSL_LineNo;
				base.TSL_LineNo = value;
				if (oldValue != TSL_LineNo)
				{
					PopulateRelatedRegLineProperties();
				}
			}
		}

		[ResourceStringData("4146540B-7E17-402F-BB8D-09E59A2300F1", Caption = "Customer Reference")]
		public ZString RelatedRegLineCustomerReference => RelatedRegLine?.RegHeader.SRH_InternalReference ?? ZString.Empty;

		public ZPropertyInfo RelatedRegLineCustomerReferenceInfo => GetZPropertyInfo(nameof(RelatedRegLineCustomerReference));

		[ResourceStringData("91DAEA3F-5366-424E-AFC5-59310A253DC6", Caption = "Owner Reference")]
		public ZString RelatedRegLineOwnerReference => RelatedRegLine != null ? (ZString)$"{RelatedRegLine.SRL_OwnerReferenceType} - {RelatedRegLine.SRL_OwnerReference}" : ZString.Empty;

		public ZPropertyInfo RelatedRegLineOwnerReferenceInfo => GetZPropertyInfo(nameof(RelatedRegLineOwnerReference));

		[ResourceStringData("27D1472D-DA1B-4BCC-B1A3-923D3402B2C9", Caption = "Goods Description")]
		public ZString RelatedRegLineGoodsDescription => RelatedRegLine?.SRL_GoodsDescription ?? ZString.Empty;

		public ZPropertyInfo RelatedRegLineGoodsDescriptionInfo => GetZPropertyInfo(nameof(RelatedRegLineGoodsDescription));

		[ResourceStringData("CF1BC8E3-CB20-4A48-83EE-51CC2FCB3E04", Caption = "Packages")]
		public ZString RelatedRegLinePackagesAndPackageType
		{
			get
			{
				var result = ZString.Empty;
				if (RelatedRegLine != null)
				{
					var remainingPackages = RelatedRegLine.SRL_PackagesRemaining;
					result = remainingPackages.ToString();
					if (remainingPackages > 0)
					{
						result += $" {RelatedRegLine.SRL_PackageType}";
					}
				}
				return result;
			}
		}

		public ZPropertyInfo RelatedRegLinePackagesAndPackageTypeInfo => GetZPropertyInfo(nameof(RelatedRegLinePackagesAndPackageType));

		CusTempStorageRegLine RelatedRegLine => Factory.GetCachedValue(
			$"DE.CUSPCSConsolidatedCusTempStorageLine.RelatedRegLine_{FormattedOwnerReferenceNumber}_{TSL_LineNo}",
			() =>
			{
				var availableRegLines = Lookups.CusTempStorageRegLineCollection;
				return availableRegLines.Cast<CusTempStorageRegLine>().SingleOrDefault(x =>
					x.RegHeader.SRH_Reference == FormattedOwnerReferenceNumber &&
					x.SRL_LineNumber == TSL_LineNo);
			});

		void PopulateRelatedRegLineProperties()
		{
			RelatedRegLineCustomerReferenceInfo.RefreshBinding();
			RelatedRegLineOwnerReferenceInfo.RefreshBinding();
			RelatedRegLinePackagesAndPackageTypeInfo.RefreshBinding();
			RelatedRegLineGoodsDescriptionInfo.RefreshBinding();
		}

		public void UpdatePropertiesFromRegLine(CusTempStorageRegLine regLine)
		{
			FormattedOwnerReferenceNumber = regLine?.RegHeader.SRH_Reference ?? ZString.Empty;
			TSL_LineNo = regLine?.SRL_LineNumber ?? 1;
		}

		#region Implement

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSL_LineNo = 1;
		}

		protected override bool ReadOnlyTSL_LineNo => false;

		protected override bool SequenceNumberEnabledCore => false;

		#endregion

		#region CusTempStorageLinesTo

		[ChildEditable]
		public CUSPCSSplitCusTempStorageLineCollection CusTempStorageLinesTo
		{
			get
			{
				if (cusTempStorageLinesTo == null)
				{
					cusTempStorageLinesTo = new CUSPCSSplitCusTempStorageLineCollection(this);
					cusTempStorageLinesTo.Load();
					RegisterEditableChildObject(cusTempStorageLinesTo);
				}
				return cusTempStorageLinesTo;
			}
		}
		CUSPCSSplitCusTempStorageLineCollection cusTempStorageLinesTo;

		#endregion

		#region Lookups

		public new CUSPCSConsolidatedCusTempStorageLineLookups Lookups => (CUSPCSConsolidatedCusTempStorageLineLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CUSPCSConsolidatedCusTempStorageLineLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CUSPCSConsolidatedCusTempStorageLineValidation(this);

		#endregion
	}
}
