using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse
{
	public class WarehouseTransferLineWrapper : WarehouseDocketLineWrapper
	{
		#region Constructor

		public WarehouseTransferLineWrapper(WhsTransferLine transferLine, BusinessObjectFactory factory)
			: base(transferLine, factory)
		{
		}

		#endregion

		#region Properties

		#region DestLocationCaptionCore

		protected override ZString DestLocationCaptionCore
		{
			get { return Res.GetString("a0b06073-b98a-46fc-b20d-dca7385a8de4", "Dest. Location"); }
		}

		#endregion

		#region DestWarehouseCaptionCore

		protected override ZString DestWarehouseCaptionCore
		{
			get { return Res.GetString("418f37d1-6e6c-459b-acb5-45a249a7ed0a", "Dest. Warehouse"); }
		}

		#endregion

		#region DestPalletIDCaptionCore

		protected override ZString DestPalletIDCaptionCore
		{
			get { return Res.GetString("1fc9cec3-4a67-4a2b-aebc-550a984e9e59", "Dest. Pallet ID"); }
		}

		#endregion

		#region FromLocationCaptionCore

		protected override ZString FromLocationCaptionCore
		{
			get { return Res.GetString("2bd35e1a-f292-45de-81ae-836b5a71349a", "Source Location"); }
		}

		#endregion

		#region FromPalletIDCaptionCore

		protected override ZString FromPalletIDCaptionCore
		{
			get { return Res.GetString("bbb12bdf-64e6-431c-b9db-a14743a50943", "Source Pallet ID"); }
		}

		#endregion

		#region FromWarehouseCaptionCore

		protected override ZString FromWarehouseCaptionCore
		{
			get { return Res.GetString("a2a7ea4e-a7fe-49b7-a64d-1589974806ae", "Source Warehouse"); }
		}

		#endregion

		#region LocationStringCore

		protected override ZString LocationStringCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.TransferFromLocationString : ZString.Empty; }
		}

		#endregion

		#region LocationString2Core

		protected override ZString LocationString2Core
		{
			get { return (DocketLineBO != null) ? DocketLineBO.LocationString : ZString.Empty; }
		}

		#endregion

		#region TransferFromWarehouseNameCore

		protected override MultilingualString TransferFromWarehouseNameCore
		{
			get
			{
				MultilingualString result = (NoResString)"";
				if (DocketLineBO != null)
				{
					var transferFromWarehouse = DocketLineBO.TransferFromWarehouse;
					if (transferFromWarehouse != null)
					{
						result = transferFromWarehouse.WW_WarehouseNameMultilingual;
					}
				}
				return result;
			}
		}

		#endregion

		#region TransferToWarehouseNameCore

		protected override MultilingualString TransferToWarehouseNameCore
		{
			get
			{
				MultilingualString result = (NoResString)"";
				if (DocketLineBO != null)
				{
					var transferToWarehouse = DocketLineBO.Warehouse;
					if (transferToWarehouse != null)
					{
						result = transferToWarehouse.WW_WarehouseNameMultilingual;
					}
				}
				return result;
			}
		}

		#endregion

		#region PalletIDCore

		protected override ZString PalletIDCore
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_TransferFromPalletId : ZString.Empty; }
		}

		#endregion

		#region PalletID2Core

		protected override ZString PalletID2Core
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_PalletID : ZString.Empty; }
		}

		#endregion

		#region StatusCore

		protected override ZString StatusCore
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_DocketLineStatus : ZString.Empty; }
		}

		#endregion

		#region UnitsCore

		protected override ZDecimal UnitsCore
		{
			get { return DocketLineBO != null ? DocketLineBO.QtyToMoveIncludingMatchingLines : ZDecimal.Zero; }
			set { }
		}

		#endregion

		#region PacksCore

		protected override ZDecimal PacksCore
		{
			get { return DocketLineBO != null ? DocketLineBO.PackQtyIncludingMatchingLines : ZDecimal.Zero; }
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsTransferLine DocketLineBO
		{
			get
			{
				return (WhsTransferLine)WrappedBO;
			}
		}

		protected override WhsDocket DocketBO
		{
			get
			{
				return DocketLineBO?.Docket;
			}
		}

		#endregion
	}
}
