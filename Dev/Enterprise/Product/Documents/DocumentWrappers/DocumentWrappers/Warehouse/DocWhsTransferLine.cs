using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsTransferLine : DocWhsDocketLine
	{
		#region Static

		public static DocWhsTransferLine New(WhsTransferLine whsTransferLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsTransferLine == null) ? null : new DocWhsTransferLine(whsTransferLine, factoryToWrap);
		}

		#endregion

		#region Constructors

		DocWhsTransferLine(WhsTransferLine whsTransferLine, BusinessObjectFactory factoryToWrap)
			: base(whsTransferLine, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		WhsTransferLine WhsTransferLine
		{
			get { return (WhsTransferLine)WrappedObject; }
		}

		public override DocWhsDocket Docket
		{
			get { return DocWhsTransfer.New(WhsTransferLine.Docket, Factory); }
		}

		#endregion

		#region Properties

		#region ZDateTime Fields

		public ZDateTime ArrivalDate
		{
			get { return WhsTransferLine.WE_AdjustmentArrivalDate.ToZDateTime(); }
		}

		#endregion

		#region ZString Fields

		public ZString FromWarehouseCaption
		{
			get { return Res.GetString("267785b3-d857-4ff3-a46d-730e7a54bc85", "Source Warehouse"); }
		}

		public ZString FromLocationCaption
		{
			get { return Res.GetString("5f78a794-f4c5-4544-8df7-129a55cf8ee7", "Source Location"); }
		}

		public ZString DestWarehouseCaption
		{
			get { return Res.GetString("04d06c4d-8909-4de9-b4dd-a3befb64d3cb", "Dest. Warehouse"); }
		}

		public ZString DestLocationCaption
		{
			get { return Res.GetString("4d4ea8fe-9d7d-4417-93f7-0456eb8bd692", "Dest. Location"); }
		}

		public ZString TransferFromWarehouseName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Docket != null && ((WhsTransfer)Docket.WrappedObject).IsInterWarehouseTransfer)
				{
					WhsWarehouse transferFromWarehouse = Factory.Load<WhsWarehouse>(WhsTransferLine.Docket.WD_WW_Whs);
					if (transferFromWarehouse != null)
					{
						result = transferFromWarehouse.WW_WarehouseNameMultilingual;
					}
				}
				return result;
			}
		}

		public ZString TransferToWarehouseName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Docket != null && ((WhsTransfer)Docket.WrappedObject).IsInterWarehouseTransfer)
				{
					WhsWarehouse transferToWarehouse = WhsTransferLine.Warehouse;
					if (transferToWarehouse != null)
					{
						result = transferToWarehouse.WW_WarehouseNameMultilingual;
					}
				}
				return result;
			}
		}

		public ZString FromPalletIDCaption
		{
			get { return Res.GetString("cd692b7d-7016-47c1-b838-a3d9b061ea4f", "Source Pallet ID"); }
		}

		public ZString DestPalletIDCaption
		{
			get { return Res.GetString("691df83a-9803-4e8a-8de9-4d2c65c57436", "Dest. Pallet ID"); }
		}

		public ZString PalletID1
		{
			get { return WhsTransferLine.WE_TransferFromPalletId; }
		}

		public ZString PalletID2
		{
			get { return WhsTransferLine.WE_PalletID; }
		}

		#endregion

		#region ZDecimal Fields

		protected override ZDecimal UnitsCore
		{
			get { return WhsTransferLine.QtyToMoveIncludingMatchingLines; }
		}

		protected override ZDecimal PackQtyCore
		{
			get { return WhsTransferLine.PackQtyIncludingMatchingLines; }
		}

		#endregion

		#endregion
	}
}
