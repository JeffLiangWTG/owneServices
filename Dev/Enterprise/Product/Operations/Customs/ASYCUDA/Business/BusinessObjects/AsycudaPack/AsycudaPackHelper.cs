using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class AsycudaPackHelper
	{
		public static CusRefPacks LoadRefPackForManifestBill(BusinessObjectFactory factory, ZString packUQ, ZString country)
		{
			return CusRefPacksHelper.LoadRefPack(factory, packUQ, RPTypeList.Codes.GlobalManifestBill, country);
		}

		public static CusRefPacks LoadRefPackForManifestLine(BusinessObjectFactory factory, ZString packUQ, ZString country)
		{
			return CusRefPacksHelper.LoadRefPack(factory, packUQ, RPTypeList.Codes.GlobalManifestLine, country);
		}

#if DEBUG
		public static Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem GetSGPackedItemForTesting(this ManifestBase.AsycudaPack pack)
		{
			return (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem)((Integration.Customs.ASYCUDA.IAsycudaPackWithOnePackedItemRelationship)pack).PackedItem;
		}

		public static AsycudaPackedItem PackedItemForTesting(this ManifestBase.AsycudaPack pack)
		{
			var result = (AsycudaPackedItem)(pack as Integration.Customs.ASYCUDA.IAsycudaPackWithOnePackedItemRelationship)?.PackedItem;
			return result ?? pack.CreatePackedItemForTesting();
		}

		public static AsycudaPackedItem CreatePackedItemForTesting(this ManifestBase.AsycudaPack pack)
		{
			var factory = pack.Factory;
			var packedItem = (AsycudaPackedItem)factory.New(pack.GetPackedItemType());
			packedItem.API_ABL_Bill = pack.APA_ABL_Bill;
			var pivot = factory.New<AsycudaPackPackedItemPivot>();
			pivot.APP_APA_Pack = pack.PK;
			pivot.APP_API_Item = packedItem.PK;
			return packedItem;
		}
#endif
	}
}
