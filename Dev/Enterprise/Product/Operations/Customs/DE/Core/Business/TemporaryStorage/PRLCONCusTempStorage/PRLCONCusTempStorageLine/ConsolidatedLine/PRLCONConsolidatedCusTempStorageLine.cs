using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONConsolidatedCusTempStorageLine : PRLCONCusTempStorageLine
	{
		public PRLCONConsolidatedCusTempStorageLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region LoadOrCreate

		public static PRLCONConsolidatedCusTempStorageLine LoadOrCreate(PRLCONCusTempStorageDec parent)
		{
			return Load() ?? Create();

			PRLCONConsolidatedCusTempStorageLine Load()
			{
				var query = new ZDBOnlyQuery(typeof(PRLCONConsolidatedCusTempStorageLine));
				query.AddToFilter(CusTempStorageLineSchema.TSL_STH, parent.PK);
				query.OrderBy = CusTempStorageLineSchema.TSL_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var subQuery = new ZDBOnlySubQuery(typeof(EU.Business.CusTempStorage.CusTempStorageLinePivot), CusTempStorageLinePivotSchema.SLR_TSL_FromLine, true);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return parent.Factory.LoadTop1<PRLCONConsolidatedCusTempStorageLine>(query);
			}

			PRLCONConsolidatedCusTempStorageLine Create()
			{
				var result = parent.Factory.New<PRLCONConsolidatedCusTempStorageLine>();
				result.TSL_STH = parent.PK;
				return result;
			}
		}

		#endregion

		#region Properties
		public override ZString TSL_OwnerReferenceType
		{
			get => base.TSL_OwnerReferenceType;
			set
			{
				base.TSL_OwnerReferenceType = value;
				SetPackageQty();
			}
		}

		public override ZString TSL_PackageType
		{
			get => base.TSL_PackageType;
			set
			{
				base.TSL_PackageType = value;
				SetPackageQty();
			}
		}

		void SetPackageQty()
		{
			if (PackageQtyShouldBeOne && TSL_PackageQty.IsEmpty)
			{
				TSL_PackageQty = 1;
			}
		}

		#endregion

		#region Implement

		[ResourceStringData("370b5655-7ccd-42b6-9d0b-1542ab83e52a", Caption = "Destination Place")]
		public override ZString TSL_DestinationPlace
		{
			get => base.TSL_DestinationPlace;
			set => base.TSL_DestinationPlace = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSL_LineNo = 1;
		}

		protected override bool SequenceNumberEnabledCore => false;

		public void RecalculatePackageQuantity()
		{
			if (!PackageQtyShouldBeOne)
			{
				TSL_PackageQty = TotalPackagesFromLinesToConsolidate;
			}
		}

		public ZInt TotalPackagesFromLinesToConsolidate =>
			Factory.GetValue(ref totalPackagesFromLinesToConsolidate,
				() => Dec?.CusTempStorageLines.Cast<PRLCONCusTempStorageLineToConsolidate>()
						  .Sum(x => x.TSL_PackageQty) ?? ZInt.Zero);
		CachedProperty<ZInt> totalPackagesFromLinesToConsolidate;

		protected override bool ShouldDeleteReleatedToLinesWhenDeletingCore() => false;

		#endregion

		#region Lookups

		public new PRLCONConsolidatedCusTempStorageLineLookups Lookups => (PRLCONConsolidatedCusTempStorageLineLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new PRLCONConsolidatedCusTempStorageLineLookups(this);

		#endregion

		#region Validation

		public new PRLCONConsolidatedCusTempStorageLineValidation Validation => (PRLCONConsolidatedCusTempStorageLineValidation)base.Validation;
		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new PRLCONConsolidatedCusTempStorageLineValidation(this);

		#endregion
	}
}
