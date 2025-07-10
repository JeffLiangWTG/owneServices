using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentCheckInAllChildPieces : AutoNonPersistentCheckInAllChildPieces
	{
		public NonPersistentCheckInAllChildPieces(ICcsukCusAwb awb)
			: base(awb.Factory)
		{
			Argument.NotNull(awb, "awb");
			this.mawb = (CusMAWB)awb;
		}

		protected override NonPersistentCheckInAllChildPiecesValidation GetNewValidation()
		{
			return new NonPersistentCheckInAllChildPiecesValidation(this);
		}

		#region PackagesUnits

		[List(nameof(PackagesUnitsList))]
		public override ZString PackagesUnits
		{
			get => base.PackagesUnits;
			set => base.PackagesUnits = value;
		}

		public CodeDescriptionPairList PackagesUnitsList
		{
			get
			{
				return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					Enterprise.Customs.EU.Business.UniversalReferenceConstants.UNPackTypeStartDate);
			}
		}

		#endregion

		#region ShedStorageLocation

		[List(nameof(WarehouseLocations))]
		[RelatedBusinessObject("ShedStorageLocation")]
		public override ZGuid ShedStorageLocationId
		{
			get
			{
				if (pivotForWarehouse == null || pivotForWarehouse.IsDeleted)
				{
					FindOrMakeNewWarehousePivot();
				}
				return pivotForWarehouse.XX_Relation2ID;
			}
			set
			{
				if (value.IsEmpty && pivotForWarehouse != null)
				{
					pivotForWarehouse.Delete();
					pivotForWarehouse = null;
				}
				else
				{
					if (pivotForWarehouse == null || pivotForWarehouse.IsDeleted)
					{
						FindOrMakeNewWarehousePivot();
					}
					pivotForWarehouse.XX_Relation2ID = value;
					Validation.ValidateShedStorageLocationId();
				}
				ShedStorageLocationIdInfo.RefreshBinding();
			}
		}

		internal void FindOrMakeNewWarehousePivot()
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
			pivotForWarehouse = Factory.LoadTop1<GenPivot>(pivotQuery);
			if (pivotForWarehouse == null)
			{
				pivotForWarehouse = Factory.New<GenPivot>();
				pivotForWarehouse.XX_Relation1ID = PK;
			}
		}

		public WhsLocation ShedStorageLocation => Factory.Load<WhsLocation>(ShedStorageLocationId);

		internal static WhsLocationCollection GetWarehouseLocations(CusMAWB mawb)
		{
			var ccsukShedCode = mawb.CargoTerminalOperator;
			return mawb.Factory.GetCachedValue("GB.CCSUK.CusOutTurn.WarehouseLocations." + ccsukShedCode, delegate
			{
				var warehouse = mawb.Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, ccsukShedCode));
				return warehouse != null ? new WhsLocationCollection(warehouse) : new WhsLocationCollection(mawb.Factory);
			});
		}

		public WhsLocationCollection WarehouseLocations => GetWarehouseLocations(mawb);

		#endregion

		public ZBool AnyChildBillHasSplits() => mawb.ChildBills.Any(hawb => ((CusHAWB)hawb).HasSplits);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (ReceivedDate.IsEmpty)
			{
				ReceivedDate = ZDateTime.Now;
			}

			if (PackagesUnits.IsEmpty)
			{
				PackagesUnits = "PK";
			}
		}

		GenPivot pivotForWarehouse;
		readonly CusMAWB mawb;
	}
}
