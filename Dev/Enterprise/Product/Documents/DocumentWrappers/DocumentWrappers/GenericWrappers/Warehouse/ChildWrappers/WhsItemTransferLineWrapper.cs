using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse
{
	public class WhsItemTransferLineWrapper : WarehouseGenericLineWrapper
	{
		#region Constructor

		public WhsItemTransferLineWrapper(BusinessObject whsItemTransferLineBO, BusinessObjectFactory factory)
			: base(whsItemTransferLineBO, factory)
		{
		}

		#endregion

		#region From

		#region FromLocationCaptionCore

		protected override ZString FromLocationCaptionCore => Res.GetString("2d99e26e-c9cd-4678-88ca-f2f0361814c4", "From Location");

		#endregion

		#region FromWarehouseCaptionCore

		protected override ZString FromWarehouseCaptionCore => Res.GetString("ca3f2d64-6935-4e3b-b814-85bf7e60b1e9", "From Warehouse");

		#endregion

		#region FromLocationStringCore

		protected override ZString LocationStringCore => TransferLineBO?.From?.WLV_LocationString ?? ZString.Empty;

		#endregion

		#region FromWarehouseNameCore

		protected override MultilingualString TransferFromWarehouseNameCore => TransferHeaderBO?.Warehouse?.WW_WarehouseNameMultilingual ?? (NoResString)string.Empty;

		#endregion

		#endregion

		#region To

		#region ToLocationCaptionCore

		protected override ZString DestLocationCaptionCore => Res.GetString("3563e88a-f345-4dc5-81cc-abe970e3b3da", "To Location");

		#endregion

		#region ToWarehouseCaptionCore

		protected override ZString DestWarehouseCaptionCore => Res.GetString("a31c2db9-8bda-450d-9f73-00a7535ee22d", "To Warehouse");

		#endregion

		#region ToLocationString2Core

		protected override ZString LocationString2Core => TransferLineBO?.To?.WLV_LocationString ?? ZString.Empty;

		#endregion

		#region ToWarehouseNameCore

		protected override MultilingualString TransferToWarehouseNameCore => TransferHeaderBO?.Warehouse?.WW_WarehouseNameMultilingual ?? (NoResString)string.Empty;

		#endregion

		#endregion

		#region StatusCore

		protected override ZString StatusCore => TransferLineBO?.PackageState?.WPS_Status ?? ZString.Empty;

		#endregion

		#region ArrivalDateCore

		protected override ZDateTime ArrivalDateCore
		{
			get
			{
				var datetime = TransferLineBO?.PackageState?.ReceiveTransportationUnit?.WRH_UnloadCompleteTime;

				return datetime == null || datetime.Value.IsEmpty ? ZDateTime.Empty : datetime.Value.ToLocalZDateTime();
			}
		}

		#endregion

		#region ProductCodeCore

		protected override ZString ProductCodeCore => TransferLineBO?.PackageState?.Package?.KP_PackageID ?? ZString.Empty;

		#endregion

		#region ProductDescriptionCore

		protected override ZString ProductDescriptionCore => TransferLineBO?.PackageState?.Package?.KP_GoodsDescription ?? ZString.Empty;

		#endregion

		#region UnitsCore

		protected override ZDecimal UnitsCore => (ZDecimal?)TransferLineBO?.PackageState?.Package?.KP_PackageQty ?? ZDecimal.Zero;

		#endregion

		#region UnitsUQCore

		protected override ZString UnitsUQCore => TransferLineBO?.PackageState?.Package?.KP_F3_NKPackType ?? ZString.Empty;

		#endregion

		#region PacksCore

		protected override ZDecimal PacksCore => (ZDecimal?)TransferLineBO?.PackageState?.Package?.KP_PackageQty ?? ZDecimal.Zero;

		#endregion

		#region PacksUQCore

		protected override ZString PacksUQCore => TransferLineBO?.PackageState?.Package?.KP_F3_NKPackType ?? ZString.Empty;

		#endregion

		WhsItemTransferLine TransferLineBO => transferLineBO ?? (transferLineBO = (WhsItemTransferLine)WrappedBO);
		WhsItemTransferLine transferLineBO;

		WhsItemTransferHeader TransferHeaderBO => transferHeaderBO ?? (transferHeaderBO = TransferLineBO?.TransferHeader);
		WhsItemTransferHeader transferHeaderBO;
	}
}
