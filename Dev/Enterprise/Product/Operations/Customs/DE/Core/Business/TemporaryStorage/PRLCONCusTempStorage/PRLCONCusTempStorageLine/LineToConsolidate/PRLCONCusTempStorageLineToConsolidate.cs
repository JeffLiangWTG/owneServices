using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageLineToConsolidate : PRLCONCusTempStorageLine
	{
		public PRLCONCusTempStorageLineToConsolidate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZInt TSL_PackageQty
		{
			get => base.TSL_PackageQty;
			set
			{
				if (TSL_PackageQty != value)
				{
					base.TSL_PackageQty = value;
					Dec?.ConsolidatedLine.RecalculatePackageQuantity();
				}
			}
		}

		[ResourceStringData("f5d79d35-2d9c-41e9-a0d6-723d18713b9a", Caption = "Owner Reference No.", ShortCaption = "Owner Ref. No.")] //Used for AWB Declarations.
		public override ZString TSL_OwnerReferenceNumber { get => base.TSL_OwnerReferenceNumber; set => base.TSL_OwnerReferenceNumber = value; }

		[BusinessObjectTestExclude]
		[MaxLength(27)]
		[ResourceStringData("26D0A148-D23D-473A-8E9D-92FE10296640", Caption = "Reference")]
		public ZString FormattedReferenceNumber
		{
			get
			{
				var result = TSL_ReferenceNumber;
				if (!result.IsEmpty)
				{
					result = SumARegistrationNumberFormatter.Format(result);
				}
				return result;
			}
			set
			{
				CheckMaximumLength(FormattedReferenceNumberInfo, value);
				value = value.KeepAlphanumericCharacters();
				if (TSL_ReferenceNumber != value)
				{
					TSL_ReferenceNumber = value;
				}
			}
		}

		public ZPropertyInfo FormattedReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(FormattedReferenceNumber), x => TSL_ReferenceNumberInfo);

		[ResourceStringData("B7936C9A-93A9-4AE3-8372-0E31373C521D", Caption = "Reference Line No.")]
		public override ZInt TSL_ReferenceNumberLine
		{
			get => base.TSL_ReferenceNumberLine;
			set => base.TSL_ReferenceNumberLine = value;
		}

		public override ZGuid TSL_STH
		{
			get => base.TSL_STH;
			set
			{
				base.TSL_STH = value;

				var dec = Dec;
				if (dec != null && !dec.IsDeleted && dec.IsREGDeclaration)
				{
					TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
				}
			}
		}

		protected override bool SequenceNumberEnabledCore => Dec?.IsREGDeclaration ?? false;

		#endregion

		#region Lookups

		public new PRLCONCusTempStorageLineToConsolidateLookups Lookups => (PRLCONCusTempStorageLineToConsolidateLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new PRLCONCusTempStorageLineToConsolidateLookups(this);

		#endregion

		#region Validation

		public new PRLCONCusTempStorageLineToConsolidateValidation Validation => (PRLCONCusTempStorageLineToConsolidateValidation)base.Validation;
		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new PRLCONCusTempStorageLineToConsolidateValidation(this);

		#endregion

		#region Pivot

		public CusTempStorageLinePivot Pivot
		{
			get
			{
				if (pivot == null || pivot.IsDeleted)
				{
					var consolidatedLine = Dec?.ConsolidatedLine;

					if (consolidatedLine != null && !consolidatedLine.IsDeleted)
					{
						var query = new ZQuery(CusTempStorageLinePivotSchema.SLR_TSL_FromLine, PK);
						query.AddToFilter(CusTempStorageLinePivotSchema.SLR_TSL_ToLine, consolidatedLine.PK);

						pivot = Factory.LoadTop1<CusTempStorageLinePivot>(query);
					}
				}

				return pivot;
			}
		}
		CusTempStorageLinePivot pivot;

		#endregion
	}
}
